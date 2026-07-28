using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using PackageRegistryService.Models;

namespace PackageRegistryService.Data;

public static class DatabaseInitialization
{
    private const string DevelopmentBuildChannel = "dev";

    public static bool IsDeployedDevelopmentInstance(string? buildChannel) =>
        string.Equals(
            buildChannel,
            DevelopmentBuildChannel,
            StringComparison.Ordinal
        );

    public static bool ShouldInitializeDeployedDatabase(
        string? buildChannel,
        bool hasSchema
    ) => IsDeployedDevelopmentInstance(buildChannel) && !hasSchema;

    public static bool HasSchema(ValidationPackageDb context) =>
        context.Database.GetService<IRelationalDatabaseCreator>().HasTables();
}
