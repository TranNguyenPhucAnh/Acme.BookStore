using Acme.BookStore.Books;
using Acme.BookStore.Notifications;
using Acme.BookStore.Permissions;
using AutoFilterer.Extensions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
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
    IRepository<Book, Guid> bookRepository,
    ILogger<AuthorAppService> logger
    ) : BookStoreAppService, IAuthorAppService
{
    private readonly IAuthorRepository _authorRepository = authorRepository;
    private readonly AuthorManager _authorManager = authorManager;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly INotificationAppService _notificationAppService = notificationAppService;
    private readonly IRepository<Book, Guid> _bookRepository = bookRepository;
    private readonly ILogger<AuthorAppService> _logger = logger;

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

    public async Task<FileContentResult> ExportAsync(GetAuthorListDto? input = null)
    {
        var query = await GetListWithoutPaginationAsync(input);
        var list = await AsyncExecuter.ToListAsync(query);

        if (list.Count == 0)
        {
            throw new UserFriendlyException("No items to be exported, please try again.");
        }

        var data = list.Select(author => new
        {
            author.Name,
            author.BirthDate
        });

        using var memoryStream = new MemoryStream();
        try
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            worksheet.Cell(1, 1).InsertTable(data);

            var lastRow = worksheet.LastRowUsed().RowNumber();

            var lastColumn = worksheet.LastColumnUsed().ColumnNumber();

            worksheet.Range(1, 1, lastRow, lastColumn).Style.Border
                    .SetOutsideBorder(XLBorderStyleValues.Thin).Border
                    .SetInsideBorder(XLBorderStyleValues.Thin);

            worksheet.Columns().AdjustToContents();

            worksheet.Rows().AdjustToContents();

            workbook.SaveAs(memoryStream);

            workbook.Dispose();

            return new FileContentResult(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Authors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating Excel file");
            throw new UserFriendlyException("An error occurred while exporting the authors. Please try again later.");
        }
    }

    private async Task<IQueryable<AuthorDto>> GetListWithoutPaginationAsync(GetAuthorListDto? input)
    {
        var query = (await _authorRepository.GetQueryableAsync())
        .AsNoTracking()
        .OrderBy(NormalizeSorting(input?.Sorting))
        .Select(author => new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            BirthDate = author.BirthDate
        });

        if (input == null)
        {
            return query;
        }

        query = query.ApplyFilter(input);

        _logger.LogInformation($"Book Query From ApplyFilter(): {query.ToQueryString()}");

        return query;
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
