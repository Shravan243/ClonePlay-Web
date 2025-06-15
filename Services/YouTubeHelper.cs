using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ClonePlayWeb.Services
{
    public class YouTubeHelper
    {
        private readonly YouTubeService _youtubeService;

        public YouTubeHelper(YouTubeService service)
        {
            _youtubeService = service;
        }

        public async Task<string> CreatePlaylist(string title, string description)
        {
            var newPlaylist = new Playlist
            {
                Snippet = new PlaylistSnippet
                {
                    Title = title,
                    Description = description
                },
                Status = new PlaylistStatus
                {
                    PrivacyStatus = "private" // or "public"
                }
            };

            var request = _youtubeService.Playlists.Insert(newPlaylist, "snippet,status");
            var response = await request.ExecuteAsync();

            Console.WriteLine($"✅ Created Playlist: {response.Snippet.Title} (ID: {response.Id})");
            return response.Id;
        }

        public async Task<List<string>> GetPlaylistVideoIds(string playlistId, string accessToken)
        {
            var videoIds = new List<string>();

            var request = _youtubeService.PlaylistItems.List("snippet");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;

            var response = await request.ExecuteAsync();

            foreach (var item in response.Items)
            {
                var videoId = item.Snippet?.ResourceId?.VideoId;
                if (!string.IsNullOrEmpty(videoId))
                {
                    videoIds.Add(videoId);
                }
            }

            return videoIds;
        }

        public async Task AddVideoToPlaylist(string playlistId, string videoId)
        {
            var playlistItem = new PlaylistItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    ResourceId = new ResourceId
                    {
                        Kind = "youtube#video",
                        VideoId = videoId
                    }
                }
            };

            var request = _youtubeService.PlaylistItems.Insert(playlistItem, "snippet");
            await request.ExecuteAsync();

            Console.WriteLine($"✅ Added Video: {videoId} to Playlist: {playlistId}");
        }
    }
}
