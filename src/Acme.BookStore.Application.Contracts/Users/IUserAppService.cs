using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.Users
{
    public interface IUserAppService : IApplicationService
    {
        /// <summary>
        /// Gets a list of users.
        /// </summary>
        /// <returns>A list of user DTOs.</returns>
        Task<List<IdentityUserDto>> GetUsersAsync();
    }
}
