
using ForgeScheduler.Application.Jobs;
using ForgeScheduler.Worker;
using ForgeScheduler.Infrastructure;


var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<JobDispatcher>();
builder.Services.AddHostedService<JobWorker>();

var host = builder.Build();
host.Run();
