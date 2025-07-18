using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;
using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Books
{
    public class BookGetListInput : FilterBase, IPagedAndSortedResultRequest
    {
        [CompareTo(
        nameof(BookDto.Name),
        nameof(BookDto.AuthorName),
        nameof(BookDto.ISBN),
        nameof(BookDto.Publisher)
        )]
        [StringFilterOptions(StringFilterOption.Contains)]
        public string? Filter { get; set; }
        [CompareTo(nameof(BookDto.PublishDate))]
        public Range<DateTime> PublishDate { get; set; }

        [CompareTo(nameof(BookDto.Type))]
        public BookType? Type { get; set; }

        public int SkipCount { get; set; }

        public int MaxResultCount { get; set; }

        public string? Sorting { get; set; }
    }
}