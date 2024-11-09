using Azure.Core;
using Azure.Identity;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Kiota.Abstractions.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;

namespace Malarkey.Integration.Microsoft.ProfileImport;
internal class MicrosoftProfileImporter : IProfileImporter<MicrosoftImportProfile>
{
    private readonly GraphServiceClient _graphClient;

    public MicrosoftProfileImporter(GraphServiceClient graphClient)
    {
        _graphClient = graphClient;
    }

    public async Task<MicrosoftImportProfile?> LoadForImport()
    {
        var user = await _graphClient.Me.GetAsync(opts => { 
        
        });
        var userPhoto = await _graphClient.Me.Photo.GetAsync();
        var photos = await _graphClient.Me.Photos.GetAsync();
        var contacts = await _graphClient.Me.Contacts.GetAsync();
        var profileImage = await ReadImage(userPhoto);
        var imagesTasks = photos?.Value == null ? null : photos.Value.Select(ReadImage).ToList();
        if(imagesTasks != null)
        {
            await Task.WhenAll(imagesTasks);
        }
        var images = imagesTasks?
            .Select(_ => _.Result)?
            .Where(_ => _ != null)?
            .Select(_ => _!)
            .ToList();

        var returnee = new MicrosoftImportProfile(
            UserId: user!.Id!,
            Name: user?.GivenName ?? "Unknown",
            LastName: user?.Surname,
            ProfileImage: profileImage,
            Images: images,
            Contacts: contacts?.Value?
               .Where(_ => !string.IsNullOrWhiteSpace(_.GivenName) && !string.IsNullOrWhiteSpace(_.Id))?
               .Select(_ => new MicrosoftImportProfile(
                                   UserId: _.Id!,
                                   Name: _.GivenName!,
                                   LastName: _.Surname,
                                   ProfileImage: null,
                                   Images: null,
                                   Contacts: null
                                   )
               )?.ToList() ?? []

            );


        return returnee;
    }


    private async Task<ImportImage?> ReadImage(ProfilePhoto? photo)
    {
        try
        {
            if (photo == null)
                return null;
            await using var stream = await _graphClient.Me.Photos[photo.Id].Content.GetAsync();
            if (stream == null)
                return null;
            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            var bytes = memStream.ToArray();
            var returnee = new ImportImage(Data: bytes, FileType: photo.OdataType!);
            return returnee;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}


public static class MicrosoftProfileImporterFactory
{
    public static async Task<IProfileImporter<MicrosoftImportProfile>?> Create(
        this IServiceProvider serviceProvider,
        AuthenticationStateProvider authenticationStateProvider,
        HttpContext httpContext)
    {
        var currentUser = (await authenticationStateProvider.GetAuthenticationStateAsync())?.User;
        if(currentUser == null || !currentUser.IsAuthenticatedMicrosoftUser())
            return null;
        var microConf = serviceProvider.GetRequiredService<IOptions<MicrosoftIntegrationConfiguration>>().Value;
        var azConf = microConf.AzureAd;


        var tokenAqcuisition = serviceProvider.GetRequiredService<ITokenAcquisition>();
        var graphConf = microConf.DownstreamApis.MicrosoftGraph;
        var acqOptions = new TokenAcquisitionOptions { };
        var accessToken = await tokenAqcuisition.GetAccessTokenForUserAsync(
            scopes: [],
            authenticationScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
            tenantId: azConf.TenantId,
            user: currentUser);
        TryParseToken(accessToken, microConf);
        var jwtToken  = new JwtSecurityTokenHandler().ReadToken(accessToken);

        var credential = new OnBehalfOfCredential(
            tenantId: azConf.TenantId,
            clientId: azConf.ClientId,
            clientCertificate: azConf.ClientCertificates.First().AsCertificate,
            userAssertion: jwtToken.ToString());

        var client = new GraphServiceClient(credential, graphConf.Scopes);
        var returnee = new MicrosoftProfileImporter(client);
        return returnee;

    }


    private static void TryParseToken(string input, MicrosoftIntegrationConfiguration conf)
    {
        Console.WriteLine(input);
        var bytes = Convert.FromBase64String(input);
        var stringVal = Encoding.ASCII.GetString(bytes);

        var cert = conf.AzureAd.ClientCertificates.First().AsCertificate;
        var key = cert.GetRSAPrivateKey()!;

        var paddings = new List<RSAEncryptionPadding>();
        var typ = typeof(RSAEncryptionPadding);
        var props = typ.GetProperties().ToList();
        var statics = props.ToList();
        var padders = statics.Where(_ => _.PropertyType == typeof(RSAEncryptionPadding)).ToList();
        foreach (var fld in padders)
            paddings.Add(fld.GetValue(null) as RSAEncryptionPadding);
        foreach(var padd in paddings)
        {
            try
            {
                var decrypted = key.Decrypt(bytes, padd);
                var tess = "";

            }
            catch (Exception ex) {
            }

        }


    }



}
