var app = builder.Build();

UsernameGenerator.Initialize(app.Services.GetRequiredService<IWebHostEnvironment>());