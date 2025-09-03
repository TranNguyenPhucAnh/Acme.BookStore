using Acme.BookStore.Authors;
using Acme.BookStore.Books;
using Acme.BookStore.Notifications;
using Acme.BookStore.Schedulers;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.Users.EntityFrameworkCore;

namespace Acme.BookStore.EntityFrameworkCore;
[ConnectionStringName("Default")]
public class BookStoreDbContext : AbpDbContext<BookStoreDbContext>
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */


    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityUserRole> UserRoles { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentityUserOrganizationUnit> UserOrganizationUnits { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Scheduler> Schedulers { get; set; }
    public DbSet<BookMedias> BookMedias { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    #endregion

    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options)
        : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureSettingManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<Book>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "Books", BookStoreConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.ExtraProperties).HasColumnType("json"); // Đảm bảo MySQL hỗ trợ cột JSON

            // ADD THE MAPPING FOR THE RELATION
            b.HasOne<Author>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<BookHistory>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "BookHistories", BookStoreConsts.DbSchema);
            b.ConfigureByConvention(); //auto configure for the base class props
            b.Property(x => x.PreviousAuditId).IsRequired(false);
            //IsConcurrencyToken(), or rowversion/timestamp will add a where clause in the SQL update statement
            //used for optimistic concurrency check against updating entity/aggregate root, no need for history
            //b.Property(x => x.Version).IsConcurrencyToken();
        });

        builder.Entity<Author>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "Authors",
                BookStoreConsts.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ExtraProperties).HasColumnType("json"); // Đảm bảo MySQL hỗ trợ cột JSON

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(AuthorConsts.MaxNameLength);

            b.HasIndex(x => x.Name);
        });

        builder.Entity<BookMedias>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "BookMedias", BookStoreConsts.DbSchema);
            b.ConfigureByConvention();

            b.HasOne<Book>()
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<Notification>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "Notifications", BookStoreConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.LocalizationKey).IsRequired(false);
            b.Property(x => x.LocalizationArguments).IsRequired(false);
            b.Property(x => x.RedirectUrl).IsRequired(false);
        });

        builder.Entity<Scheduler>(b =>
        {
            b.ToTable(BookStoreConsts.DbTablePrefix + "Schedulers", BookStoreConsts.DbSchema);
            b.ConfigureByConvention();
        });

        builder.Entity<IdentityUser>(b =>
        {
            b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "Users");
            b.ConfigureByConvention();
            b.ConfigureAbpUser();
            b.HasOne<OrganizationUnit>()
                .WithMany()
                .HasForeignKey("EntityId")
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<OrganizationUnit>(b =>
        {
            b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "OrganizationUnits");
            b.ConfigureByConvention();
        });

        builder.Entity<IdentityUserOrganizationUnit>(b =>
        {
            b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "UserOrganizationUnits");
            b.ConfigureByConvention();
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "Roles");
            b.ConfigureByConvention();
        });

        builder.Entity<IdentityUserRole>(b =>
        {
            b.ToTable(AbpIdentityDbProperties.DbTablePrefix + "UserRoles");
            b.ConfigureByConvention();
        });
    }
}
