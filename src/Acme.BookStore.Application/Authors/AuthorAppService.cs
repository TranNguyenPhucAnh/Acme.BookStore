using Acme.BookStore.Books;
using Acme.BookStore.Notifications;
using Acme.BookStore.Permissions;
using AutoFilterer.Extensions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Acme.BookStore.Authors;

[Authorize(BookStorePermissions.Authors.Default)]
public class AuthorAppService(
    IAuthorRepository authorRepository,
    AuthorManager authorManager,
    ICurrentUser currentUser,
    INotificationAppService notificationAppService,
    IRepository<Book, Guid> bookRepository
    ) : BookStoreAppService, IAuthorAppService
{
    private readonly IAuthorRepository _authorRepository = authorRepository;
    private readonly AuthorManager _authorManager = authorManager;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly INotificationAppService _notificationAppService = notificationAppService;
    private readonly IRepository<Book, Guid> _bookRepository = bookRepository;

    public async Task<AuthorDto> GetAsync(Guid id)
    {
        var author = await _authorRepository.GetAsync(id);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    public async Task<PagedResultDto<AuthorDto>> GetListAsync(GetAuthorListDto input)
    {
        var authors = await _authorRepository.GetQueryableAsync();

        authors = authors.ApplyFilter(input);

        var totalCount = await AsyncExecuter.CountAsync(authors);

        authors = authors.OrderBy(NormalizeSorting(input.Sorting)).PageBy(input.SkipCount, input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(authors);

        return new PagedResultDto<AuthorDto>(
            totalCount,
            ObjectMapper.Map<List<Author>, List<AuthorDto>>(items)
        );
    }

    [Authorize(BookStorePermissions.Authors.Create)]
    public async Task<AuthorDto> CreateAsync(CreateAuthorDto input)
    {
        var author = await _authorManager.CreateAsync(
            input.Name,
            input.BirthDate
        );

        await _authorRepository.InsertAsync(author);

        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.AuthorCRUD,
            ObjectMapper.Map<CreateAuthorDto, AuthorDto>(input),
            "created");

        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    [Authorize(BookStorePermissions.Authors.Edit)]
    public async Task UpdateAsync(Guid id, UpdateAuthorDto input)
    {
        var author = await _authorRepository.GetAsync(id);

        if (author.Name != input.Name)
        {
            await _authorManager.ChangeNameAsync(author, input.Name);
        }

        author.BirthDate = input.BirthDate;

        await _authorRepository.UpdateAsync(author);

        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.AuthorCRUD,
            ObjectMapper.Map<Author, AuthorDto>(author),
            "updated");
    }

    [Authorize(BookStorePermissions.Authors.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var author = await _authorRepository.GetAsync(id);
        Console.WriteLine($"Author before set extra properties: {author}");
        // Check if there is any referential entity associated with this lookup entity to ensure referential integrity constraint
        if (await _bookRepository.AnyAsync(x => x.AuthorId == author.Id))
        {
            throw new UserFriendlyException(
                $"Author '{author.Name}' is associated with existing books.",
                "405",
                $"Please delete or reassign the books before deleting author '{author.Name}'."
            );
        }

        author.SetDefaultsForExtraProperties();// Ensure extra properties are set before deletion

        Console.WriteLine($"Author after set extra properties: {author}");
        
        await _authorRepository.DeleteAsync(id);

        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.AuthorCRUD,
            ObjectMapper.Map<Author, AuthorDto>(author),
            "deleted");
    }

    public async Task<DateTime> GetMinDateTimeAsync()
    {
        return await _authorRepository.MinAsync(author => author.BirthDate);
    }

    private static string NormalizeSorting(string sorting)
    {
        if (sorting.IsNullOrEmpty())
        {
            return nameof(AuthorDto.Name); // Mặc định sắp xếp theo Name của BookDto
        }

        // Danh sách các thuộc tính hợp lệ của BookDto
        var validSortProperties = new[]
        {
            nameof(AuthorDto.Name),
            nameof(AuthorDto.BirthDate)
        };

        // Tách sorting thành tên thuộc tính và hướng sắp xếp (asc/desc)
        var sortParts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var propertyName = sortParts[0];
        var sortDirection = sortParts.Length > 1 ? sortParts[1].ToLower() : "asc";

        // Kiểm tra xem propertyName có hợp lệ không
        if (validSortProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
        {
            return sorting; // Trả về nguyên sorting nếu thuộc tính hợp lệ
        }

        // Nếu thuộc tính không hợp lệ, trả về mặc định
        return nameof(AuthorDto.Name);
    }
}
