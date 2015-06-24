using System.Threading.Tasks;
using BuildRevisionCounter.Model.BuildRevisionStorage;

namespace BuildRevisionCounter.DAL.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<UserModel>
    {
        Task EnsureAdminUser();

        Task EnsureUsersIndex();
    }
}
