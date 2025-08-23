using Acme.BookStore.Authors;
using Acme.BookStore.Commons;
using Acme.BookStore.Notifications;
using Acme.BookStore.Permissions;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFilterer.Extensions;
using ClosedXML.Excel;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Http;
using Volo.Abp.Users;

//hangfire implemented failed due to mysql isn't compatible

namespace Acme.BookStore.Books;

[Authorize(BookStorePermissions.Books.Default)]
public class BookAppService :
    CrudAppService<
        Book, //The Book entity
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        BookGetListInput, //Used for paging/sorting
        CreateUpdateBookDto>, //Used to create/update a book
    IBookAppService //implement the IBookAppService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<BookAppService> _logger;
    private readonly INotificationAppService _notificationAppService;
    private readonly IValidator<BookDto> _validator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<BookMedias, Guid> _bookMediaRepository;

    public BookAppService(
        IRepository<Book, Guid> repository,
        IAuthorRepository authorRepository,
        ICurrentUser currentUser,
        ILogger<BookAppService> logger,
        INotificationAppService notificationAppService,
        IValidator<BookDto> validator,
        IHttpContextAccessor httpContextAccessor,
        IRepository<BookMedias, Guid> bookMediaRepository
        )
        : base(repository)
    {
        _authorRepository = authorRepository;
        _currentUser = currentUser;
        _logger = logger;
        _notificationAppService = notificationAppService;
        _validator = validator;
        _httpContextAccessor = httpContextAccessor;
        _bookMediaRepository = bookMediaRepository;

        GetPolicyName = BookStorePermissions.Books.Default;
        GetListPolicyName = BookStorePermissions.Books.Default;
        CreatePolicyName = BookStorePermissions.Books.Create;
        UpdatePolicyName = BookStorePermissions.Books.Edit;
        DeletePolicyName = BookStorePermissions.Books.Delete;
    }

    public class ImportBookValidator : AbstractValidator<BookDto>
    {
        public ImportBookValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Book name is required.");
            RuleFor(x => x.AuthorName).NotEmpty().WithMessage("Author name is required.");
            RuleFor(x => x.ISBN).NotEmpty().WithMessage("ISBN is required.");
            RuleFor(x => x.Type).IsInEnum().WithMessage("Invalid book type.");
            RuleFor(x => x.PublishDate).NotEmpty().WithMessage("Publish date is required.")
                .Must(date => date.Year <= DateTime.Now.Year)
                .WithMessage("Publish date must be lesser than the current year.");
            RuleFor(x => x.Publisher).NotEmpty().WithMessage("Publisher is required.");
        }
    }

    public override async Task<BookDto> GetAsync(Guid id)
    {
        //Get the IQueryable<Book> from the repository
        var queryable = await Repository.GetQueryableAsync();

        //Prepare a query to join books and authors
        var query = from book in queryable
                    join author in await _authorRepository.GetQueryableAsync() on book.AuthorId equals author.Id
                    where book.Id == id
                    select new { book, author };

        //Execute the query and get the book with author
        var queryResult = await AsyncExecuter.FirstOrDefaultAsync(query);
        if (queryResult == null)
        {
            throw new EntityNotFoundException(typeof(Book), id);
        }

        var bookDto = ObjectMapper.Map<Book, BookDto>(queryResult.book);
        bookDto.AuthorName = queryResult.author.Name;
        return bookDto;
    }

    public override async Task<PagedResultDto<BookDto>> GetListAsync(BookGetListInput input)
    {
        var query = await GetListWithoutPaginationAsync(input);

        var totalCount = await AsyncExecuter.CountAsync(query);

        //Paging
        query = query.OrderBy(NormalizeSorting(input.Sorting)).PageBy(input.SkipCount, input.MaxResultCount);

        //Execute the query and get a list
        var queryResult = await AsyncExecuter.ToListAsync(query);

        return new PagedResultDto<BookDto>(
            totalCount,
            queryResult
        );
    }

    private async Task<IQueryable<BookDto>> GetListWithoutPaginationAsync(BookGetListInput? input)
    {
        //Get the IQueryable<Book> from the repository
        var queryable = await Repository.GetQueryableAsync();

        //Prepare a query to join books and authors
        var query = (from book in queryable
                    join author in await _authorRepository.GetQueryableAsync() on book.AuthorId equals author.Id
                    select new BookDto
                    {
                        Id = book.Id,
                        AuthorId = author.Id,
                        ISBN = book.ISBN,
                        AuthorName = author.Name,
                        Name = book.Name,
                        Type = book.Type,
                        PublishDate = book.PublishDate,
                        Publisher = book.Publisher
                    }).AsNoTracking();

        if (input == null)
        {
            return query;
        }

        query = query.ApplyFilter(input);

        _logger.LogInformation($"Book Query From ApplyFilter(): {query.ToQueryString()}");

        return query;
    }

    public async Task<DateTime> GetMinDateTimeAsync()
    {
        return await Repository.MinAsync(book => book.PublishDate);
    }

    public async Task<FileContentResult> ExportAsync(BookGetListInput? input = null)
    {
        var query = await GetListWithoutPaginationAsync(input);
        var list = await AsyncExecuter.ToListAsync(query);

        if (list.Count == 0)
        {
            throw new UserFriendlyException("No items to be exported, please try again.");
        }

        var data = list.Select(book => new
        {
            book.Name,
            book.AuthorName,
            book.ISBN,
            book.Type,
            book.PublishDate,
            book.Publisher
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
                FileDownloadName = $"Books_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating Excel file");
            throw new UserFriendlyException("An error occurred while exporting the books. Please try again later.");
        }
    }
    /// <returns>
    /// A value tuple containing:
    /// <para> (FileContentResult): Error file content if import fails, otherwise empty.</para>
    /// </returns>
    public async Task<FileContentResult> ImportAsync(IFormFile file)
    {
        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var workbook = new XLWorkbook(memoryStream);
            var worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new UserFriendlyException("The uploaded file does not contain any worksheets.");

            var data = worksheet.RowsUsed()
                .Skip(1) // Skip header row
                .Select(row => new BookDto
                {
                    Name = row.Cell(1).GetString(),
                    AuthorName = row.Cell(2).GetString(),
                    ISBN = row.Cell(3).GetString(),
                    Type = Enum.TryParse<BookType>(row.Cell(4).GetString(), out var type) ? type : BookType.UNDEFINED,
                    PublishDate = row.Cell(5).GetDateTime(),
                    Publisher = row.Cell(6).GetString()
                });

            var result = await MapDataAsync(data);
            if (result != null)
            {
                return result;
            }
            ;

            workbook.Save();
            workbook.Dispose();
        }
        return new FileContentResult([], "text/plain")
        {
            FileDownloadName = string.Empty
        };
    }

    public async Task<bool> UploadAsync(Guid bookId)
    {
        try
        {
            if (bookId == Guid.Empty)
            {
                _logger.LogError("Book ID is empty.");
                throw new UserFriendlyException("Invalid book ID. Please try again.");
            }

            var files = _httpContextAccessor.HttpContext.Request.Form.Files;

            _logger.LogInformation($"UploadAsync called with {files.Count} files.");

            if (files.Count == 0)
            {
                _logger.LogWarning("No files found in the request.");
                return false;
            }

            var validType = new[] { ".pdf", ".epub", ".mobi" };
            var isValidType = files.Any(s => validType.Contains(Path.GetExtension(s.FileName).ToLowerInvariant()));

            if (!isValidType)
            {
                _logger.LogError("Invalid file type.");
                return false;
            }

            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].Length == 0)
                {
                    _logger.LogWarning($"File {files[i].FileName} is empty. Skipping.");
                    continue; // Skip empty files
                }
                var client = new AmazonS3Client("AKIATCKASQLCAWNTY3LR", "ywno5QQsiwjlS2mWotGdlMji23aU0TbdvA7mTdfJ", RegionEndpoint.APSoutheast1);

                var putRequest = new PutObjectRequest
                {
                    BucketName = "abp-book-store-uploaded-documents",
                    Key = $"uploads/book-{Guid.NewGuid().ToString()}-{files[i].FileName}",
                    ContentType = files[i].ContentType,
                    InputStream = files[i].OpenReadStream()
                };

                await _bookMediaRepository.InsertAsync(new BookMedias
                {
                    BookId = bookId,
                    ObjectKey = putRequest.Key,
                    FileName = files[i].FileName,
                    Size = files[i].Length
                });

                await client.PutObjectAsync(putRequest);

                _logger.LogInformation($"File {files[i].FileName} has been uploaded to S3 abp-book-store-uploaded-documents bucket.");
            }
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex.Message, "An error occurred while uploading files.");
            throw new UserFriendlyException("An error occurred while uploading files. Please try again later.");
        }

        return true;
    }

    public async Task<FileContentResult> DownloadSample()
    {
        var client = new AmazonS3Client("AKIATCKASQLCAWNTY3LR", "ywno5QQsiwjlS2mWotGdlMji23aU0TbdvA7mTdfJ", RegionEndpoint.APSoutheast1);

        var request = new GetObjectRequest
        {
            BucketName = "abp-book-store-uploaded-documents",
            Key = "uploads/Mastering_ABP_Framework.pdf" //include prefix
        };

        using var response = await client.GetObjectAsync(request);
        var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return new FileContentResult(memoryStream.ToArray(), MimeTypes.Application.Pdf)
        {
            FileDownloadName = $"Mastering_ABP_Framework_{DateTime.Now:yyyyMMdd_HHmmss}.pdf" // .NET sẽ wrap chuỗi trong dấu ngoặc kép nếu có khoảng trắng
        };
    }

    public async Task<FileContentResult> DownloadAsync(Guid bookId)
    {
        if (bookId == Guid.Empty)
        {
            _logger.LogError("Book ID is empty.");
            throw new UserFriendlyException("Invalid book ID. Please try again.");
        }

        var bookMediaQuery = await _bookMediaRepository.GetQueryableAsync();
        var bookMedias = bookMediaQuery.Where(b => b.BookId == bookId).ToList();

        if (bookMedias.Count == 0)
        {
            _logger.LogError("None object keys is found.");

            return new FileContentResult([], MimeTypes.Text.Plain)
            {
                FileDownloadName = string.Empty
            };
        }

        var client = new AmazonS3Client("AKIATCKASQLCAWNTY3LR", "ywno5QQsiwjlS2mWotGdlMji23aU0TbdvA7mTdfJ", RegionEndpoint.APSoutheast1);

        var memoryStreams = new List<(string fileName, MemoryStream stream)>();

        foreach (var media in bookMedias)
        {
            if (string.IsNullOrEmpty(media.ObjectKey))
            {
                _logger.LogError("Object key is empty.");
                continue; // Skip if the object key is empty
            }

            var request = new GetObjectRequest
            {
                BucketName = "abp-book-store-uploaded-documents",
                Key = media.ObjectKey
            };

            using var response = await client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            memoryStreams.Add((media.FileName, memoryStream));
        }

        var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (fileName, fileStream) in memoryStreams)
            {
                var entry = archive.CreateEntry(fileName);
                using var entryStream = entry.Open();
                fileStream.Position = 0;
                await fileStream.CopyToAsync(entryStream);
            }
        }

        zipStream.Position = 0;

        var bookName = (await Repository.GetAsync(bookId)).Name;

        //optional:
        //Caching file .zip trong Redis hoặc local nếu đã tạo
        //Tải file lớn(> 100MB) từng phần
        //Tạo link tải.zip bằng pre-signed URL: up file .zip lên s3 bucket, rồi get presigned URL trả về FE

        if (bookName.Contains(' ')) //sanity check for spaces
        {
            _logger.LogInformation($"Book name contains space: {bookName}. Replacing spaces with underscores for file download name.");
            bookName = bookName.Replace(' ', '_'); // Replace spaces with underscores for file download name (sanitize string)
        }

        return new FileContentResult(zipStream.ToArray(), "application/zip")
        {
            FileDownloadName = $"All_{bookName}_Ebook_Format_{DateTime.Now:yyyyMMdd_HHmmss}.zip" // .NET sẽ wrap chuỗi trong dấu ngoặc kép nếu có khoảng trắng
        };
    }

    public async Task<string> PreviewAsync(Guid bookId)
    {
        if (bookId == Guid.Empty)
        {
            _logger.LogError("Book ID is empty.");
            throw new UserFriendlyException("Invalid book ID. Please try again.");
        }

        var bookMedia = await _bookMediaRepository.FirstOrDefaultAsync(x => x.BookId == bookId && x.FileName.EndsWith(".pdf"));            

        if (bookMedia == null || bookMedia.ObjectKey.IsNullOrEmpty())
        {
            _logger.LogError("Object key is null or empty.");
            return string.Empty;
        }

        var client = new AmazonS3Client("AKIATCKASQLCAWNTY3LR", "ywno5QQsiwjlS2mWotGdlMji23aU0TbdvA7mTdfJ", RegionEndpoint.APSoutheast1);

        var preSignedUrl = client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = "abp-book-store-uploaded-documents",
            Key = bookMedia.ObjectKey,
            Expires = DateTime.UtcNow.AddMinutes(10)
        });

        if (string.IsNullOrEmpty(preSignedUrl))
        {
            _logger.LogError("Pre-signed URL is null or empty.");
            throw new UserFriendlyException("Failed to generate download link. Please try again later.");
        }

        _logger.LogInformation($"Pre-signed URL generated: {preSignedUrl}");

        return preSignedUrl;
    }

    private async Task<FileContentResult?> MapDataAsync(IEnumerable<BookDto> data)
    {
        //map book
        var books = await Repository.GetListAsync();
        var authors = await _authorRepository.GetListAsync();
        var importedBooks = data.Select(s => s.ISBN);

        foreach (var (item, index) in data.Select((item, index) => (item, index)))
        {
            // Validate row by row
            var result = await ValidateDataAsync(item, index);
            if (result != null)
            {
                _logger.LogWarning($"Validation failed for row {index + 1}");
                return result; // Return the error file if validation fails
            }

            var author = await _authorRepository.FindByNameAsync(item.AuthorName);
            if (author == null)
            {
                _logger.LogWarning($"Author with Name: {item.AuthorName} does not exists.");
                continue; // Skip if the author does not exist
            }
            var itemExists = await Repository.FirstOrDefaultAsync(book => book.ISBN == item.ISBN);

            if (itemExists != null && itemExists.Id != Guid.Empty)
            {
                // Update the existing book with the new data
                itemExists.Name = item.Name;
                itemExists.AuthorId = author.Id; // Set the AuthorId from the found author
                itemExists.Publisher = item.Publisher;
                itemExists.PublishDate = item.PublishDate;
                itemExists.Type = item.Type;

                await Repository.UpdateAsync(itemExists);
                _logger.LogInformation($"Book with Name: {item.Name} is updated.");
            }
            else
            {
                // Create a new book if it does not exist
                _logger.LogInformation($"Creating a new book with Name: {item.Name}.");
                itemExists = new Book
                {
                    Name = item.Name,
                    ISBN = item.ISBN,
                    Type = item.Type,
                    PublishDate = item.PublishDate,
                    Publisher = item.Publisher,
                    AuthorId = author.Id // Set the AuthorId from the found author
                };
                await Repository.InsertAsync(itemExists);
            }
        }
        //delete books that are not in the imported data
        var booksToDelete = books.Where(book => !importedBooks.Contains(book.ISBN)).ToList();

        foreach (var book in booksToDelete)
        {
            await DeepDeleteAsync(book);
            _logger.LogInformation($"Book with ISBN: {book.ISBN} will be deleted (not found in imported data).");
        }

        return null;
    }

    private async Task<FileContentResult?> ValidateDataAsync(BookDto item, int index)
    {
        var validationResult = await _validator.ValidateAsync(item);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(s => new ImportErrorDto { Message = s.ErrorMessage, Column = s.PropertyName, Row = index });
            return GenerateErrorFileAsync(errors);
        }
        return null;
    }

    private static FileContentResult GenerateErrorFileAsync(IEnumerable<ImportErrorDto> errors)
    {
        using var memoryStream = new MemoryStream();
        using var workBook = new XLWorkbook();

        var worksheet = workBook.AddWorksheet("Error Report");
        worksheet.Cell(1, 1).InsertTable(errors);

        var lastRow = worksheet.LastRowUsed().RowNumber();

        var lastColumn = worksheet.LastColumnUsed().ColumnNumber();

        worksheet.Range(1, 1, lastRow, lastColumn).Style.Border
                .SetOutsideBorder(XLBorderStyleValues.Thin).Border
                .SetInsideBorder(XLBorderStyleValues.Thin);

        worksheet.Columns().AdjustToContents();
        worksheet.Rows().AdjustToContents();

        workBook.SaveAs(memoryStream);
        workBook.Dispose();

        var fileName = $"ImportErrors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

        var fileProvider = new FileExtensionContentTypeProvider();

        if (!fileProvider.TryGetContentType(fileName, out string contentType))
        {
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        return new FileContentResult(memoryStream.ToArray(), contentType)
        {

            FileDownloadName = $"ImportErrors_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };
    }

    public override async Task<BookDto> CreateAsync(CreateUpdateBookDto input)
    {
        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.BookCRUD,
            ObjectMapper.Map<CreateUpdateBookDto, BookDto>(input),
            "created");

        return await base.CreateAsync(input);
    }

    public override async Task<BookDto> UpdateAsync(Guid id, CreateUpdateBookDto input)
    {
        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.BookCRUD,
            ObjectMapper.Map<CreateUpdateBookDto, BookDto>(input),
            "updated");

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var book = await Repository.GetAsync(id);

        book.SetDefaultsForExtraProperties();// Ensure extra properties are set before deletion

        await _notificationAppService.InsertNotificationAndSendEmailAsync(
            _currentUser.Id.GetValueOrDefault(),
            _currentUser.Email,
            NotificationType.BookCRUD,
            ObjectMapper.Map<Book, BookDto>(book),
            "deleted");

        await DeepDeleteAsync(book);
    }

    private async Task DeepDeleteAsync(Book book)
    {
        // Delete related book media
        var bookMediaQuery = await _bookMediaRepository.GetQueryableAsync();
        var bookMedias = bookMediaQuery.Where(b => b.BookId == book.Id).ToList();

        foreach (var media in bookMedias)
        {
            if (!media.ObjectKey.IsNullOrEmpty())
            {
                var client = new AmazonS3Client("AKIATCKASQLCAWNTY3LR", "ywno5QQsiwjlS2mWotGdlMji23aU0TbdvA7mTdfJ", RegionEndpoint.APSoutheast1);
                await client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = "abp-book-store-uploaded-documents",
                    Key = media.ObjectKey
                });
            }
            await _bookMediaRepository.DeleteAsync(media);
        }

        // Delete the book itself
        await Repository.DeleteAsync(book.Id);

        //if the author has no other books, delete the author
        var authorBooks = await Repository.CountAsync(b => b.AuthorId == book.AuthorId);
        if (authorBooks == 0)
        {
            _logger.LogInformation($"Author with ID {book.AuthorId} has no other books, deleting author.");
            await _authorRepository.DeleteAsync(book.AuthorId);
        }
    }

    public async Task<ListResultDto<AuthorLookupDto>> GetAuthorLookupAsync()
    {
        var authors = await _authorRepository.GetListAsync();

        return new ListResultDto<AuthorLookupDto>(
            ObjectMapper.Map<List<Author>, List<AuthorLookupDto>>(authors)
        );
    }

    private static string NormalizeSorting(string sorting)
    {
        if (sorting.IsNullOrEmpty())
        {
            return nameof(BookDto.Name); // Mặc định sắp xếp theo Name của BookDto
        }

        // Danh sách các thuộc tính hợp lệ của BookDto
        var validSortProperties = new[]
        {
            nameof(BookDto.Name),
            nameof(BookDto.AuthorName),
            nameof(BookDto.ISBN),
            nameof(BookDto.Type),
            nameof(BookDto.PublishDate),
            nameof(BookDto.Publisher),
        };

        // Tách sorting thành tên thuộc tính và hướng sắp xếp (asc/desc)
        var sortParts = sorting.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var propertyName = sortParts[0];

        // Kiểm tra xem propertyName có hợp lệ không
        if (validSortProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase))
        {
            return sorting; // Trả về nguyên sorting nếu thuộc tính hợp lệ
        }

        // Nếu thuộc tính không hợp lệ, trả về mặc định
        return nameof(BookDto.Name);
    }
}