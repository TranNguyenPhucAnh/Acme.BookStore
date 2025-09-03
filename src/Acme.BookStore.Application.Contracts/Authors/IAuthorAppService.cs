using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.Authors;

public interface IAuthorAppService : IApplicationService
{
    Task<AuthorDto> GetAsync(Guid id);

    Task<PagedResultDto<AuthorDto>> GetListAsync(GetAuthorListDto input);

    Task<AuthorDto> CreateAsync(CreateAuthorDto input);

    Task UpdateAsync(Guid id, UpdateAuthorDto input);

    Task DeleteAsync(Guid id);

    Task<DateTime> GetMinDateTimeAsync();

    Task<FileContentResult> ExportAsync(GetAuthorListDto? input = null);
}
