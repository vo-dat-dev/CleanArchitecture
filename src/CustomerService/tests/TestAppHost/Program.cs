using CustomerService.Shared;

namespace CustomerService.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder
            .AddSqlite(Services.Database);

        builder.Build().Run();
    }
}