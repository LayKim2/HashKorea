var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HashKorea>("hashkorea");

builder.Build().Run();
