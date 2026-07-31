using PackageRegistryService.Data;

namespace APITests;

public class DatabaseInitializationTests
{
    [Theory]
    [InlineData("dev", true)]
    [InlineData("release", false)]
    [InlineData("local", false)]
    [InlineData("DEV", false)]
    [InlineData(null, false)]
    public void OnlyDevBuildsAreDeployedDevelopmentInstances(
        string? buildChannel,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            DatabaseInitialization.IsDeployedDevelopmentInstance(buildChannel)
        );
    }

    [Theory]
    [InlineData("dev", false, true)]
    [InlineData("dev", true, false)]
    [InlineData("release", false, false)]
    [InlineData("local", false, false)]
    [InlineData(null, false, false)]
    public void DeployedDatabaseInitializationRequiresDevAndNoSchema(
        string? buildChannel,
        bool hasSchema,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            DatabaseInitialization.ShouldInitializeDeployedDatabase(
                buildChannel,
                hasSchema
            )
        );
    }
}
