 
 
 using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClonePlayWeb.Services
{
    public class YouTubeAuth
    {
        public async Task<YouTubeService> Login()
        {
            
var secrets = new ClientSecrets
{
    ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"),
    ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
};

            var scopes = new[]
            {
                YouTubeService.Scope.Youtube,
                YouTubeService.Scope.YoutubeForceSsl
            };

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                scopes,
                "user",
                CancellationToken.None
            );

            return new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "PlaylistCloner"
            });
        }
    }
}

 
 
 
 
