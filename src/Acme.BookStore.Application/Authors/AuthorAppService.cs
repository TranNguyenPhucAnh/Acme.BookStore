using Acme.BookStore.Emails;
using Acme.BookStore.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Acme.BookStore.Authors;

[Authorize(BookStorePermissions.Authors.Default)]
public class AuthorAppService : BookStoreAppService, IAuthorAppService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly AuthorManager _authorManager;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ICurrentUser _currentUser;
    public AuthorAppService(
        IAuthorRepository authorRepository,
        AuthorManager authorManager,
        IBackgroundJobManager backgroundJobManager,
        ICurrentUser currentUser
        )
    {
        _authorRepository = authorRepository;
        _authorManager = authorManager;
        _backgroundJobManager = backgroundJobManager;
        _currentUser = currentUser;
    }

    public async Task<AuthorDto> GetAsync(Guid id)
    {
        var author = await _authorRepository.GetAsync(id);
        return ObjectMapper.Map<Author, AuthorDto>(author);
    }

    public async Task<PagedResultDto<AuthorDto>> GetListAsync(GetAuthorListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Author.Name);
        }

        var authors = await _authorRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter
        );

        var totalCount = input.Filter == null
            ? await _authorRepository.CountAsync()
            : await _authorRepository.CountAsync(
                author => author.Name.Contains(input.Filter));

        return new PagedResultDto<AuthorDto>(
            totalCount,
            ObjectMapper.Map<List<Author>, List<AuthorDto>>(authors)
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
        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
                "phucanhbtt@gmail.com",
                $"An author has been created by user {_currentUser.Name + _currentUser.SurName}.",
                $"Author {author.Name} has just been created.")
        );
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

        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
                "phucanhbtt@gmail.com",
                $"An author has been updated by user {_currentUser.Name + _currentUser.SurName}.",
                $"Author {author.Name} has just been updated."
                )
            );
    }

    [Authorize(BookStorePermissions.Authors.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var author = await _authorRepository.GetAsync(id);
        await _authorRepository.DeleteAsync(id);

        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
                "phucanhbtt@gmail.com",
                $"An author has been deleted by user {_currentUser.Name + _currentUser.SurName}.",
                $"Author {author.Name} has just been updated."
                )
            );
    }
}
