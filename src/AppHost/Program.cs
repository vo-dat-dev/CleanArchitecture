using CleanArchitecture.Shared;

var builder = DistributedApplication.CreateBuilder(args);

#if (UsePostgreSQL)
var databaseServer = builder
    .AddPostgres(Services.DatabaseServer)
    .AddDatabase(Services.Database);
#elif (UseSqlServer)
var databaseServer = builder.AddSqlServer(Services.DatabaseServer)
    .AddDatabase(Services.Database);
#else
var databaseServer = builder
    .AddSqlite(Services.Database)
    .WithSqliteWeb();
#endif

var rabbitMqUser = builder.AddParameter("rabbitmq-user", secret: false);
var rabbitMqPassword = builder.AddParameter("rabbitmq-password", secret: true);

var rabbitMq = builder
    .AddRabbitMQ(Services.RabbitMq, userName: rabbitMqUser, password: rabbitMqPassword)
    .WithManagementPlugin();

var minioUser = builder.AddParameter("minio-user", "minioadmin", secret: false);
var minioPassword = builder.AddParameter("minio-password", "minioadmin", secret: true);

var minio = builder
    .AddMinioContainer(Services.MinIO, rootUser: minioUser, rootPassword: minioPassword);

var pgUser = builder.AddParameter("customer-pg-user", secret: false);
var pgPassword = builder.AddParameter("customer-pg-password", secret: true);

var customerDb = builder
    .AddPostgres("customer-postgres", userName: pgUser, password: pgPassword)
    .WithHostPort(59157)
    .WithDataVolume("customer-postgres-data")
    .AddDatabase(Services.CustomerServiceDatabase);

var customerService = builder.AddProject<Projects.CustomerService_Web>(Services.CustomerServiceApi)
    .WithReference(customerDb)
    .WaitFor(customerDb)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Customer Service API";
        url.Url = "/scalar";
    });

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(minio)
    .WaitFor(minio)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

#if (!UseApiOnly)
builder.AddJavaScriptApp(Services.WebFrontend, "./../Web/ClientApp-React")
    .WithRunScript("start")
    .WithReference(web)
    .WaitFor(web)
    .WithReference(customerService)
    .WaitFor(customerService)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();
#endif

builder.AddProject<Projects.PdfExportService_Worker>(Services.PdfExportWorker)
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(minio)
    .WaitFor(minio);

builder.Build().Run();
