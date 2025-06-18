// Program.cs  ──────────────────────────────────────────────────────────
using ClonePlayWeb.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

// ── DI & host setup ───────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<YouTubeAuth>();              // our auth helper

var app = builder.Build();

// ── Static files / default index.html (optional) ──────────────────────
app.UseDefaultFiles();
app.UseStaticFiles();

// ── 1.  /login  → send user to Google consent page ────────────────────
app.MapGet("/login", (YouTubeAuth auth, HttpContext ctx) =>
{
    ctx.Response.Redirect(auth.GetAuthUrl());               // 302 → Google
});

// ── 2.  /oauth2callback  → Google redirects back here ────────────────
app.MapGet("/oauth2callback", async (
        string          code,                               // ?code=…
        YouTubeAuth     auth,
        HttpContext     ctx,
        CancellationToken ct) =>
{
    await auth.ExchangeAsync(code, ct);                     // fetch tokens
    ctx.Response.Redirect("/");                             // back home
});

// ── 3.  /clone  → clone a playlist after user is authorised ──────────
app.MapPost("/clone", async (
        CloneRequest    req,
        YouTubeAuth     auth,
        CancellationToken ct) =>
{
    // validate body
    if (string.IsNullOrWhiteSpace(req.SourcePlaylistId) ||
        string.IsNullOrWhiteSpace(req.NewTitle))
        return Results.BadRequest("sourcePlaylistId and newTitle required.");

    // ensure user has logged in
    var ytService = auth.TryGetService();
    if (ytService is null) return Results.Unauthorized();

    // do the cloning
    var helper          = new YouTubeHelper(ytService);
    var newPlaylistId   = await helper.CreatePlaylist(req.NewTitle, "Cloned via web");
    var videoIds        = await helper.GetPlaylistVideoIds(req.SourcePlaylistId);

    foreach (var vid in videoIds)
        await helper.AddVideoToPlaylist(newPlaylistId, vid);

    return Results.Ok(new { clonedTo = newPlaylistId });
});

app.Run();

// ── record type for JSON body binding ─────────────────────────────────
public record CloneRequest(string SourcePlaylistId, string NewTitle);
