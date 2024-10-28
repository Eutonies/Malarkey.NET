
using Malarkey.UI;
using Malarkey.UI.Pages;

var builder = WebApplication.CreateBuilder(args)
    .AddConfiguration();

builder.AddUiServices();


var app = builder.Build();
app.UseUiServices();

app.Run();
