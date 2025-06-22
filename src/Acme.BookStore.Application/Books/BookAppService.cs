using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Acme.BookStore.Authors;
using Acme.BookStore.Emails;
using Acme.BookStore.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Acme.BookStore.Books;

[Authorize(BookStorePermissions.Books.Default)]
public class BookAppService :
    CrudAppService<
        Book, //The Book entity
        BookDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        CreateUpdateBookDto>, //Used to create/update a book
    IBookAppService //implement the IBookAppService
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IBackgroundJobManager _backgroundJobManager;
    private readonly ICurrentUser _currentUser;

    public BookAppService(
        IRepository<Book, Guid> repository,
        IAuthorRepository authorRepository,
        IBackgroundJobManager backgroundJobManager,
        ICurrentUser currentUser 
        )
        : base(repository)
    {
        _authorRepository = authorRepository;
        _backgroundJobManager = backgroundJobManager;
        _currentUser = currentUser;
        GetPolicyName = BookStorePermissions.Books.Default;
        GetListPolicyName = BookStorePermissions.Books.Default;
        CreatePolicyName = BookStorePermissions.Books.Create;
        UpdatePolicyName = BookStorePermissions.Books.Edit;
        DeletePolicyName = BookStorePermissions.Books.Delete;
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

    public override async Task<PagedResultDto<BookDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        //Get the IQueryable<Book> from the repository
        var queryable = await Repository.GetQueryableAsync();

        //Prepare a query to join books and authors
        var query = from book in queryable
                    join author in await _authorRepository.GetQueryableAsync() on book.AuthorId equals author.Id
                    select new { book, author };

        //Paging
        query = query
            .OrderBy(NormalizeSorting(input.Sorting))
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        //Execute the query and get a list
        var queryResult = await AsyncExecuter.ToListAsync(query);

        //Convert the query result to a list of BookDto objects
        var bookDtos = queryResult.Select(x =>
        {
            var bookDto = ObjectMapper.Map<Book, BookDto>(x.book);
            bookDto.AuthorName = x.author.Name;
            return bookDto;
        }).ToList();

        //Get the total count with another query
        var totalCount = await Repository.GetCountAsync();

        return new PagedResultDto<BookDto>(
            totalCount,
            bookDtos
        );
    }

    public override async Task<BookDto> CreateAsync(CreateUpdateBookDto input)
    {
        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
                "phucanhbtt@gmail.com",
                $"Book {input.Name} has just been created",
                 $"{_currentUser.Name + _currentUser.SurName} has just created a book"
                ));

        return await base.CreateAsync(input);
    }

    public override async Task<BookDto> UpdateAsync(Guid id, CreateUpdateBookDto input)
    {
        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
            "phucanhbtt@gmail.com",
            $"Book {input.Name} has just been updated",
            $"{_currentUser.Name + _currentUser.SurName} has just updated a book"
            ));

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var bookName = await Repository.GetAsync(id).ContinueWith(x => x.Result.Name);
        await _backgroundJobManager.EnqueueAsync(
            new EmailSendingArgs(
                "phucanhbtt@gmail.com",
                $"Book {bookName} has just been deleted",
                $"{_currentUser.Name + _currentUser.SurName} has just deleted a book"
                ));

        await base.DeleteAsync(id);
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
            return $"book.{nameof(Book.Name)}";
        }

        if (sorting.Contains("authorName", StringComparison.OrdinalIgnoreCase))
        {
            return sorting.Replace(
                "authorName",
                "author.Name",
                StringComparison.OrdinalIgnoreCase
            );
        }

        return $"book.{sorting}";
    }
}
