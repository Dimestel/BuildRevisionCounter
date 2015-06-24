using System.Threading.Tasks;
using BuildRevisionCounter.DAL.Repositories;

namespace BuildRevisionCounter.Tests
{
    internal class MongoDBStorageUtils
    {
        public static async Task SetUpAsync(RevisionRepository revisionRepo, UserRepository userRepo)
        {
            await revisionRepo.DropDatabaseAsync();
            await userRepo.EnsureUsersIndex();
            await userRepo.EnsureAdminUser();
        }
    }
}
