using Lighthouse.Api.Data;
using Lighthouse.Api.Endpoints;
using Lighthouse.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:5080");
builder.Services.AddProblemDetails();

// LAB_CONFIGURATION: a local SQLite path, not a credential.
string connectionString = "Data Source=Data/lighthouse.db;Mode=ReadOnly";
connectionString = DatabasePaths.Resolve(connectionString, builder.Environment.ContentRootPath);
builder.Services.AddSingleton(new MemberService(connectionString));
builder.Services.AddSingleton(new LoanRepository(connectionString));
builder.Services.AddSingleton(new AccountService(connectionString));
builder.Services.AddSingleton<LoanPayoffService>();

var app = builder.Build();
app.UseExceptionHandler();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = builder.Configuration["Lighthouse:ServiceName"], synthetic = true }));
app.MapGet("/api/members/search", (string name, MemberService members) => Results.Ok(members.SearchByName(name)));
app.MapGet("/api/members/{memberId}", (string memberId, MemberService members) =>
    members.Find(memberId) is { } member ? Results.Ok(member) : Results.NotFound());
app.MapGet("/api/accounts/{memberId}", (string memberId, AccountService accounts) => Results.Ok(accounts.ForMember(memberId)));
app.MapLoanPayoffEndpoints();
app.Run();

public partial class Program { }
