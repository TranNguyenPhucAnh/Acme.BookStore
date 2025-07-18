using AutoFilterer.Attributes;
using AutoFilterer.Enums;
using AutoFilterer.Types;
using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Authors
{
    public class AuthorGetListInput : FilterBase, IPagedAndSortedResultRequest
    {
        [CompareTo(nameof(AuthorDto.Name))]
        [StringFilterOptions(StringFilterOption.Contains)]
        public string? Filter { get; set; }

        [CompareTo(nameof(AuthorDto.BirthDate))]
        public Range<DateTime> BirthDate { get; set; }
        public int SkipCount { get; set; }

        public int MaxResultCount { get; set; }

        public string? Sorting { get; set; }
    }
}