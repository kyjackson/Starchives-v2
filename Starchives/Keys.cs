namespace Starchives;

/// <summary>
/// Stores the values of the keys defined either in secrets.json or as environment variables.
/// </summary>
public class Keys
{
	/// <summary>
	/// Used for connecting to the Starchives database.
	/// </summary>
	public string DbConnectionString { get; set; }

	/// <summary>
	/// Used for logging into the admin control panel.
	/// </summary>
	public string AdminPassword { get; set; }

	/// <summary>
	/// Used for authenticating requests to the YouTube Data API.
	/// </summary>
	public string YouTubeApiKey { get; set; }

	/// <summary>
	/// Used for accessing a YouTube channel's video library.
	/// </summary>
	public string ChannelId { get; set; }
}
