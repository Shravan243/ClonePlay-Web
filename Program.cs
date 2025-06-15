using System.Net.Http.Json;

using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

using ClonePlayWeb.Services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles(); 
// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapPost("/clone", async (HttpContext http) =>
{
    var form = await http.Request.ReadFromJsonAsync<CloneRequest>();

    if (form == null)
        return Results.BadRequest("invalid input");

    var ytAuth = new YouTubeAuth();
    var ytService = await ytAuth.Login();

    var helper = new YouTubeHelper(ytService);

    var targetPlaylistId = await helper.CreatePlaylist(form.NewTitle, "Cloned via web");

    var videoIds = await helper.GetPlaylistVideoIds(form.SourcePlaylistId);

    foreach (var videoId in videoIds)
        await helper.AddVideoToPlaylist(targetPlaylistId, videoId);

    return Results.Ok($"Cloned to Playlist Id: {targetPlaylistId}");
});

app.Run();

record CloneRequest(string SourcePlaylistId, string NewTitle);