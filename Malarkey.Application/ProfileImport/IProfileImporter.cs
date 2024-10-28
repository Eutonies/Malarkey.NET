using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Malarkey.Application.ProfileImport;
public interface IProfileImporter<TProfile, TKey> where TKey : notnull
{
    Task<TProfile?> LoadForImport(TKey key);
}

public interface IProfileImporter<TProfile> : IProfileImporter<TProfile, long>
{
    Task<TProfile?> LoadForImport();
    async Task<TProfile?> IProfileImporter<TProfile, long>.LoadForImport(long key) => await LoadForImport();

}
