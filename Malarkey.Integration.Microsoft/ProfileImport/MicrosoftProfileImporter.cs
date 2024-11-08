using Azure.Identity;
using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Malarkey.Integration.Microsoft.Configuration;
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
        var accessToken = await tokenAqcuisition.GetAccessTokenForUserAsync(
            scopes: [],
            authenticationScheme: IntegrationConstants.IdProviders.MicrosoftAuthenticationSchemeName,
            tenantId: azConf.TenantId,
            user: currentUser);
        Console.WriteLine(accessToken);
        var jwtToken  = new JwtSecurityTokenHandler().ReadToken(accessToken);
        Console.WriteLine("\r\n\r\n" + jwtToken.UnsafeToString());

        var credential = new OnBehalfOfCredential(
            tenantId: azConf.TenantId,
            clientId: azConf.ClientId,
            clientCertificate: azConf.ClientCertificates.First().AsCertificate,
            userAssertion: jwtToken.ToString());

        var client = new GraphServiceClient(credential, graphConf.Scopes);
        var returnee = new MicrosoftProfileImporter(client);
        return returnee;

    }


    private static void TryParseToken(string input)
    {
        var providers = new List<SecurityTokenHandler>
        {
            new JwtSecurityTokenHandler()
        };
    }



}
