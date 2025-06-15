using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace ClonePlayWeb.Services;

public class YouTubeAuth
{
    private readonly GoogleAuthorizationCodeFlow _flow;
    private readonly string _redirectUri;
    private static TokenResponse? _tokenCache;   // demo cache

    public YouTubeAuth(IConfiguration cfg)
    {
        var secrets = new ClientSecrets
        {
            ClientId     = cfg["GOOGLE_CLIENT_ID"],
            ClientSecret = cfg["GOOGLE_CLIENT_SECRET"]
        };

        _flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = secrets,
            Scopes = new[]
            {
                YouTubeService.Scope.Youtube,
                YouTubeService.Scope.YoutubeForceSsl
            }
        });

        _redirectUri = $"{cfg["BASE_URL"]}/oauth2callback";
    }

    // Generates Google consent URL
    public string GetAuthUrl() =>
     _flow.CreateAuthorizationCodeRequest(_redirectUri).Build().ToString();

    // Exchanges ?code=… for access + refresh tokens
    public async Task<YouTubeService> ExchangeAsync(string code, CancellationToken ct)
    {
        _tokenCache = await _flow.ExchangeCodeForTokenAsync("user", code, _redirectUri, ct);
        return CreateService(_tokenCache);
    }

    // Returns a ready service if user already authorised
    public YouTubeService? TryGetService() =>
        _tokenCache is null ? null : CreateService(_tokenCache);

    private YouTubeService CreateService(TokenResponse t) =>
        new(new BaseClientService.Initializer
        {
            HttpClientInitializer = new UserCredential(_flow, "user", t),
            ApplicationName = "PlaylistCloner"
        });
}
