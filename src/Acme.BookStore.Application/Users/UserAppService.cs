using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.Users
{
    public class UserAppService(
        IIdentityUserRepository identityUserRepository
        ) : ApplicationService, IUserAppService
    {
        private readonly IIdentityUserRepository _identityUserRepository = identityUserRepository;

        public async Task<List<IdentityUserDto>> GetUsersAsync()
        {
            var users = await _identityUserRepository.GetListAsync();
            return [.. users.Select(user => new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname
            })];
        }
    }
}
