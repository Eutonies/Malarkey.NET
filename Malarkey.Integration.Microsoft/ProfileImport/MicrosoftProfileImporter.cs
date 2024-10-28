using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Serialization;

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
        var user = await _graphClient.Me.GetAsync();
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
        if (photo == null)
            return null;
        await using var stream = await _graphClient.Me.Photos[photo.Id].Content.GetAsync();
        if(stream == null)
            return null; 
        using var memStream = new MemoryStream();
        await stream.CopyToAsync(memStream);
        var bytes = memStream.ToArray();
        var returnee = new ImportImage(Data: bytes, FileType: photo.OdataType!);
        return returnee;
    }



}
