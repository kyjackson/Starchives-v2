using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;



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
	/// Gets the RSI channel from YouTube.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <returns>A list of YouTube channels matching the specified filters. This should always return only the RSI channel.</returns>
	Task<Channel>? GetRsiChannel(YouTubeService youTubeService);



	/// <summary>
	/// Gets video IDs for all uploads from the RSI channel.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <param name="rsiChannel">The channel to get all uploads for. This should always be the RSI channel.</param>
	/// <returns>A list of strings representing all pages of all RSI uploads. Each string contains up to 50 video IDs delimited by semicolon.</returns>
	Task<List<string>> GetRsiUploadIds(YouTubeService youTubeService, Channel rsiChannel);



	/// <summary>
	/// Gets video data for all uploads from the RSI channel.
	/// </summary>
	/// <param name="youTubeService">The YouTube service configuration to use for API calls.</param>
	/// <param name="videoPages">The list of strings containing up to 50 semicolon-delimited video IDs each.</param>
	/// <returns>A list of videos in the YouTube API format, with detailed data for each video in the list.</returns>
	Task<List<Video>> GetUploadDataByIds(YouTubeService youTubeService, List<string> videoPages);
}
