using AutoMapper;
using ElsaRegister.Models;
using ElsaRegister.Services;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using FluentValidation;
using Elsa;
using ElsaGuides.ContentApproval.Web;
using Elsa.Services;
using Elsa.Models;

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
    .AddWorkflow<RegisterWorkflow>()
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
        
        var worflowInput = new  WorkflowInput(user);
        var result = await workflowRunner.BuildAndStartWorkflowAsync<RegisterWorkflow>("main", worflowInput);
        if(!result.Executed)
            throw new Exception("Not executed");
    }
    else
        return Results.BadRequest("User has been already registered");
    return Results.Created();
});

app.Run();