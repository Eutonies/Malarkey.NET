using Malarkey.Application.ProfileImport;
using Malarkey.Domain.ProfileImport;
using Microsoft.Graph;

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
        var contacts = await _graphClient.Me.Contacts.GetAsync();

        var returnee = new MicrosoftImportProfile(
            UserId: user!.Id!,
            Name: user?.GivenName ?? "Unknown",
            LastName: user?.Surname,
            Contacts: contacts?.Value?
               .Where(_ => !string.IsNullOrWhiteSpace(_.GivenName) && !string.IsNullOrWhiteSpace(_.Id))?
               .Select(_ => new MicrosoftImportProfile(
                                   UserId: _.Id!,
                                   Name: _.GivenName!,
                                   LastName: _.Surname,
                                   Contacts: null
                                   )
               )?.ToList() ?? []

            );


        return returnee;
    }



}
