using CodeExecutionWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<DockerService>();
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<KafkaProducer>());
builder.Services.AddHostedService<CodeExecutionWorker.CodeExecutionWorker>();

var host = builder.Build();
host.Run();