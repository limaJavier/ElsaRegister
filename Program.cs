using AutoMapper;
using ElsaRegister.Models;
using ElsaRegister.Services;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using FluentValidation;
using Elsa;
using Elsa.Services;
using Elsa.Models;
using ElsaRegister.Workflows;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddMySqlDataSource(config.GetConnectionString("Default")!);
builder.Services.AddTransient<IUserRepository, UserRepository>();

// Elsa settup
var elsaSection = config.GetSection("Elsa");
var elsaService = builder.Services.AddElsa(elsa => elsa
    .AddConsoleActivities()
    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
    .AddEmailActivities(elsaSection.GetSection("Smtp").Bind)
    .AddQuartzTemporalActivities()
    .AddWorkflow<RegisterAdministrationWorkflow>()
    .AddWorkflow<RegisterResponseWorkflow>()
).BuildServiceProvider();
var workflowRunner = elsaService.GetRequiredService<IBuildsAndStartsWorkflow>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseHttpsRedirection();


app.MapPost("/register", async (IUserRepository repository, IMapper mapper, IValidator<UserDTO> validator, [FromBody] UserDTO request) =>
{
    // Validating request
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.BadRequest(validation.Errors.FirstOrDefault()!.ErrorMessage);

    var user = mapper.Map<User>(request);
    if (await repository.GetUser(user.Email) is null)
    {
        await repository.InsertUser(user);

        var worflowInput = new WorkflowInput(user);
        await workflowRunner.BuildAndStartWorkflowAsync<RegisterAdministrationWorkflow>("main", worflowInput);
    }
    else
        return Results.BadRequest("User has been already registered");
    return Results.Created();
});


// TODO must recieve a token instead of an implicit email
bool adminAuthorized = false;
app.MapGet("/register/{encoding}", async (IUserRepository repo, string encoding) =>
{
    // var email = encoding.Substring(0, encoding.Length - 1);
    var email = encoding[..^1];
    // var status = (encoding[encoding.Length - 1] - 48) == 1;
    var status = (encoding[^1] - 48) == 1;

    if (status && adminAuthorized)
        encoding = "accepted";
    else if (status)
    {
        encoding = "wait";
        adminAuthorized = true;
    }
    else
        encoding = "rejected";

    var worflowInput = new WorkflowInput(new { Email = email, Encoding = encoding });
    await workflowRunner.BuildAndStartWorkflowAsync<RegisterResponseWorkflow>("main", worflowInput);

    return Results.Ok(encoding);
});

app.Run();