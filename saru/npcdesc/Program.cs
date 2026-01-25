using Cocona;
using NpcDesc.Commands;

var builder = CoconaApp.CreateBuilder();
var app = builder.Build();

app.AddCommands<NpcDescCommands>();

app.Run();
