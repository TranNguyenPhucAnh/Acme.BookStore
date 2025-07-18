using Acme.BookStore.Authors;
using Acme.BookStore.Books;
using Acme.BookStore.Settings;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Acme.BookStore;

public class BookStoreDataSeederContributor(
    IRepository<Book, Guid> bookRepository,
    IAuthorRepository authorRepository,
    IOrganizationUnitRepository orgRepository,
    OrganizationUnitManager organizationUnitManager,
    IdentityUserManager identityUserManager,
    IIdentityUserRepository userRepository,
    IIdentityRoleRepository identityRoleRepository,
    IPermissionGrantRepository permissionGrantRepository,
    PermissionDefinitionManager permissionDefinitionManager)
        : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Book, Guid> _bookRepository = bookRepository;
    private readonly IAuthorRepository _authorRepository = authorRepository;
    private readonly IOrganizationUnitRepository _orgRepository = orgRepository;
    private readonly OrganizationUnitManager _organizationUnitManager = organizationUnitManager;
    private readonly IdentityUserManager _identityUserManager = identityUserManager;
    private readonly IIdentityUserRepository _userRepository = userRepository;
    private readonly IIdentityRoleRepository _identityRoleRepository = identityRoleRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository = permissionGrantRepository;
    private readonly PermissionDefinitionManager _permissionDefinitionManager = permissionDefinitionManager;

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _userRepository.GetCountAsync() <= 1)
        {
            await SeedUsers();
        }

        if (await _bookRepository.GetCountAsync() == 0)
        {
            await SeedBooks();
        }
    }

    private async Task SeedUsers()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedFiles", "BX-Users.xlsx");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found", filePath);
        }

        using var userWorkBook = new XLWorkbook(filePath);
        var userWorksheet = userWorkBook.Worksheet(1); // sheet đầu tiên
        var rows = userWorksheet.RangeUsed()!.RowsUsed().Skip(1); // bỏ dòng header

        var orgs = new List<OrganizationUnit>();

        if (await _orgRepository.GetCountAsync() == 0)
        {
            orgs.AddRange(
                new(Guid.NewGuid(), "USA"),
                new(Guid.NewGuid(), "Japan"),
                new(Guid.NewGuid(), "Korea"),
                new(Guid.NewGuid(), "China"),
                new(Guid.NewGuid(), "Taiwan")
            );

            foreach (var org in orgs)
            {
                await _organizationUnitManager.CreateAsync(org);
            }
        }
        
        var roles = new List<IdentityRole>();

        if (await _identityRoleRepository.GetCountAsync() <= 1)
        {
            roles.AddRange(
                new(Guid.NewGuid(), BookStoreSettings.SupportAdminRoleName),
                new(Guid.NewGuid(), BookStoreSettings.ManagerRoleName),
                new(Guid.NewGuid(), BookStoreSettings.EmployeeRoleName)
                );

            await _identityRoleRepository.InsertManyAsync(roles);
        }
        var permissionGrants = await _permissionDefinitionManager.GetPermissionsAsync();

        var nonSupportAdminPermissions = permissionGrants.Where(p =>
        p.Name.StartsWith(BookStoreSettings.Prefix) &&
        (!p.Name.EndsWith(BookStoreSettings.FeatureManagementPermission) ||
        !p.Name.EndsWith(BookStoreSettings.SchedulerPermission)));

        var supportAdminPermissions = permissionGrants.Where(p =>
        p.Name.StartsWith(BookStoreSettings.AbpIdentity) ||
        p.Name.StartsWith(BookStoreSettings.SchedulerPermission));

        var permissions = nonSupportAdminPermissions.Concat(supportAdminPermissions).ToList();

        foreach (var permission in permissions)
        {
            if (permission.Name.StartsWith(BookStoreSettings.Prefix))
            {
                await _permissionGrantRepository.InsertAsync(
                    new PermissionGrant(Guid.NewGuid(), permission.Name, BookStoreSettings.RoleProviderName, BookStoreSettings.ManagerRoleName));

                if (permission.Name.Count(c => c == '.') == 1)
                {
                    await _permissionGrantRepository.InsertAsync(
                        new PermissionGrant(Guid.NewGuid(), permission.Name, BookStoreSettings.RoleProviderName, BookStoreSettings.EmployeeRoleName));
                }
            }
            else if (permission.Name.StartsWith(BookStoreSettings.AbpIdentity))
            {
                await _permissionGrantRepository.InsertAsync(
                    new PermissionGrant(Guid.NewGuid(), permission.Name, BookStoreSettings.RoleProviderName, BookStoreSettings.SupportAdminRoleName));
            }
        }

        foreach (var row in rows)
        {
            try
            {
                var username = row.Cell(1).GetString();
                var age = row.Cell(3).GetString();

                var user = new IdentityUser(
                    Guid.NewGuid(),
                    username,
                    $"{username}@yopmail.com");

                user.SetProperty("Age", age);

                var orgId = orgs[new Random().Next(orgs.Count)].Id;
                user.SetProperty("EntityId", orgId);
                user.SetProperty("Entity", orgs.FirstOrDefault(o => o.Id == orgId)?.DisplayName ?? "Unknown");

                var roleName = roles[new Random().Next(roles.Count)].Name;
                user.SetProperty("Roles", roleName);

                var userObj = await _identityUserManager.CreateAsync(user, "123456");

                await _identityUserManager.AddToOrganizationUnitAsync(user.Id, orgId);

                await _identityUserManager.AddToRoleAsync(user, roleName);
  
                if (userObj.Errors.Any())
                {
                    throw new AbpException(userObj.Errors.First().Description);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                break;
            }
        }
    }

    private async Task SeedBooks()
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SeedFiles", "BX-Books.xlsx");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found", filePath);
        }

        using var workBook = new XLWorkbook(filePath);
        var worksheet = workBook.Worksheet(1); // sheet đầu tiên
        var rows = worksheet.RangeUsed()!.RowsUsed().Skip(1); // bỏ dòng header

        foreach (var row in rows)
        {
            try
            {
                var isbn = row.Cell(1).GetString();
                var title = row.Cell(2).GetString();
                var type = row.Cell(3).GetString();
                var publishYear = row.Cell(6).GetString();
                var publisher = row.Cell(7).GetString();
                
                var authorName = row.Cell(4).GetString();
                var authorBirth = row.Cell(5).GetString();

                var author = new Author(Guid.NewGuid(), authorName, new DateTime(Convert.ToInt32(authorBirth), 1, 1));

                await _authorRepository.InsertAsync(author);

                var book = new Book()
                {
                    ISBN = isbn,
                    Name = title,
                    Type = Enum.Parse<BookType>(type),
                    PublishDate = new DateTime(Convert.ToInt32(publishYear), 1, 1),
                    Publisher = publisher,
                    AuthorId = author.Id
                };

                await _bookRepository.InsertAsync(book);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                break;
            }
        }
    }
}
