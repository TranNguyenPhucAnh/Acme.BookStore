using Acme.BookStore.Authors;
using Acme.BookStore.Books;
using Acme.BookStore.Notifications;
using Acme.BookStore.Schedulers;
using AutoMapper;
using CronExpressionDescriptor;
using Volo.Abp.Identity;

namespace Acme.BookStore;

public class BookStoreApplicationAutoMapperProfile : Profile
{
    public BookStoreApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
        CreateMap<IdentityUser, IdentityUserDto>().ReverseMap();

        CreateMap<Book, BookDto>().ReverseMap();
        CreateMap<CreateUpdateBookDto, Book>().ReverseMap();
        CreateMap<CreateUpdateBookDto, BookDto>().ReverseMap();

        CreateMap<Author, AuthorDto>().ReverseMap();
        CreateMap<Author, AuthorLookupDto>().ReverseMap();
        CreateMap<CreateAuthorDto, AuthorDto>().ReverseMap();

        CreateMap<Scheduler, SchedulerDto>()
            .ForMember(
                dest => dest.CronExpression,
                opt => opt.MapFrom(src => CronHelper.ConvertUtcCronToLocalCron(src.CronExpression, src.TimeZone))
            )
            .ForMember(
                dest => dest.Description,
                opt => opt.MapFrom(src => ExpressionDescriptor.GetDescription(CronHelper.ConvertUtcCronToLocalCron(src.CronExpression, src.TimeZone)))
            );

        CreateMap<CreateUpdateSchedulerDto, Scheduler>().ReverseMap();

        CreateMap<Notification, NotificationDto>().ReverseMap();
    }
}
