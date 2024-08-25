using System.Diagnostics;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.Extensions.Options;
using Starchives;

namespace Starchives.Facades.YouTube
{
	public class YouTubeApiFacade(IOptions<Keys> options) : IYouTubeApiFacade
	{
		#region Properties
		/// <summary>
		/// Options initialized by the IOptions service in Program.cs.
		/// </summary>
		private IOptions<Keys> Options { get; } = options;
		#endregion



		#region Functions
		public YouTubeService GetYouTubeService()
		{
			return new YouTubeService(new BaseClientService.Initializer()
			{
				ApiKey          = Options.Value.YouTubeApiKey,
				ApplicationName = this.GetType().ToString()
			});
		}



		public async Task<Channel>? GetRsiChannel(YouTubeService youTubeService)
		{
			var getRsiChannel = youTubeService.Channels.List("contentDetails");
			getRsiChannel.Id = Options.Value.RsiChannelId;

			var channelListResponse = await getRsiChannel.ExecuteAsync();
			var rsiChannel          = channelListResponse.Items[0];
			Debug.Print($"Retrieved channel ID '{rsiChannel.Id}'");

			return rsiChannel;
		}



		public async Task<List<string>> GetRsiUploadIds(YouTubeService youTubeService, Channel rsiChannel)
		{
			var startTime = DateTime.Now;

			// this should be the playlist that contains all uploads to the channel
			var rsiUploadsPlaylistId       = rsiChannel.ContentDetails.RelatedPlaylists.Uploads;
			var nextPageTokenPlaylistItems = "";
			var videoPages                 = new List<string>();
			var videoCount                 = 0;

			// results are paginated, so we need to loop through all pages
			while (nextPageTokenPlaylistItems != null)
			{
				// this request only supports up to 50 results max
				var getRsiUploads = youTubeService.PlaylistItems.List("contentDetails");
				getRsiUploads.PlaylistId = rsiUploadsPlaylistId;
				getRsiUploads.MaxResults = 50;
				getRsiUploads.PageToken  = nextPageTokenPlaylistItems;

				var playlistItemsListResponse = await getRsiUploads.ExecuteAsync();
				var videoPage                 = new List<string>();

				foreach (var playlistItem in playlistItemsListResponse.Items)
				{
					videoCount++;
					videoPage.Add(playlistItem.ContentDetails.VideoId);
				}

				var videoPageString = string.Join(',', videoPage);
				videoPages.Add(videoPageString);

				nextPageTokenPlaylistItems = playlistItemsListResponse.NextPageToken;
			}

			var endTime = DateTime.Now;
			var elapsedTime = endTime - startTime;
			Debug.Print($"Retrieved {videoCount} video IDs from RSI channel (time elapsed: {elapsedTime:g})");

			return videoPages;
		}



		public async Task<List<Video>> GetUploadDataByIds(YouTubeService youTubeService, List<string> videoPages)
		{
			var startTime = DateTime.Now;
			var videoList = new List<Video>();

			foreach (var videoPage in videoPages)
			{
				// this request only supports up to 50 results max
				var getAllVideoData = youTubeService.Videos.List("snippet,contentDetails,statistics,player");
				getAllVideoData.Id         = videoPage;
				getAllVideoData.MaxResults = 50;

				var videoListResponse = await getAllVideoData.ExecuteAsync();
				videoList.AddRange(videoListResponse.Items);
			}

			var endTime     = DateTime.Now;
			var elapsedTime = endTime - startTime;
			Debug.Print($"Retrieved data for {videoList.Count} videos from RSI channel (time elapsed: {elapsedTime:g})");

			return videoList;
		}
		#endregion
	}
}
