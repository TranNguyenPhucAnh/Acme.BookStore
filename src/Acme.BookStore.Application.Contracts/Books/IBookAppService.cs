using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.Books;

public interface IBookAppService :
    ICrudAppService< //Defines CRUD methods
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        BookGetListInput, //Used for paging/sorting
        CreateUpdateBookDto> //Used to create/update a book
{
    // ADD the NEW METHOD
    Task<ListResultDto<AuthorLookupDto>> GetAuthorLookupAsync();
    Task<DateTime> GetMinDateTimeAsync();
    Task<FileContentResult> ExportAsync(BookGetListInput? input = null);
    Task<bool> UploadAsync(Guid bookId);
    Task<FileContentResult> DownloadSample();
    Task<FileContentResult> DownloadAsync(Guid bookId);
    Task<string> PreviewAsync(Guid bookId);
}
