
using Malarkey.UI;
using Malarkey.UI.Pages;
using System.Security.Cryptography;


var builder = WebApplication.CreateBuilder(args)
    .AddConfiguration();

builder.AddUiServices();

var app = builder.Build();
app.UseUiServices();

app.Run();
