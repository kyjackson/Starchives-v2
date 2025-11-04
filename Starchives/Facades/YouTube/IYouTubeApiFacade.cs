using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode.Videos.ClosedCaptions;



namespace Starchives.Facades.YouTube;

/// <summary>
/// Simplifies the access and usage of the YouTube Data API.
/// </summary>
public interface IYouTubeApiFacade
{
	/// <summary>
	/// Gets authenticated access to the YouTube Data API.
	/// </summary>
	YouTubeService GetYouTubeService();



	/// <summary>
	/// Gets a channel from YouTube.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <returns>A list of YouTube channels matching the specified filters.</returns>
	Task<Channel>? GetChannel(YouTubeService youTubeService);



	/// <summary>
	/// Gets video IDs for all uploads from the specified channel.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <param name="channel">The channel to get all uploads for.</param>
	/// <returns>A list of strings representing all pages of all channel uploads. Each string contains up to 50 video IDs delimited by semicolon.</returns>
	Task<List<string>> GetUploadIds(YouTubeService youTubeService, Channel channel);



	/// <summary>
	/// Gets video data for all uploads from the specified channel.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <param name="videoPages">The list of strings containing up to 50 semicolon-delimited video IDs each.</param>
	/// <returns>A list of videos in the YouTube API format, with detailed data for each video in the list.</returns>
	Task<List<Video>> GetUploadDataByIds(YouTubeService youTubeService, List<string> videoPages);



	/// <summary>
	/// Gets the caption track for a specific video.
	/// </summary>
	/// <param name="videoId">The ID of the video to get the caption track for.</param>
	/// <returns>The full caption track for the specified video.</returns>
	Task<ClosedCaptionTrack?> GetCaptionTrackByVideoId(string videoId);
}
