using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Acme.BookStore.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class BookStoreDbContextFactory : IDesignTimeDbContextFactory<BookStoreDbContext>
{
    public BookStoreDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        BookStoreEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<BookStoreDbContext>()
            .UseMySql(configuration.GetConnectionString("Default"), MySqlServerVersion.LatestSupportedServerVersion);
        
        return new BookStoreDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Acme.BookStore.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false, true)
            .AddJsonFile("appsettings.Development.json", optional: true, true);

        return builder.Build();
    }
}
