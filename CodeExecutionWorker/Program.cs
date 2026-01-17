using CodeExecutionWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DockerService>();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMqPublisher>());
builder.Services.AddHostedService<CodeExecutionWorker.CodeExecutionWorker>();

var host = builder.Build();
host.Run();