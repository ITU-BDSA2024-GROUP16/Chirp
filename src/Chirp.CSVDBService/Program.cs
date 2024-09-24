
using SimpleDB;
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var db = CSVDatabase<Cheep>.Instance("chirp_cli_db.csv");

app.MapGet("/cheepss", () => new Cheep
{
    Author = "mig",
    Message = "hello",
    Timestamp = 1234
});


app.MapGet("/", () => "Helloooo");
app.MapGet("/cheeps", () => db.Read());
app.MapPost("/cheep", (Cheep cheep) => db.Store(cheep));

app.Run();

