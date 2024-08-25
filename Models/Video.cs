namespace Starchives.Models;

/// <summary>
/// Defines the properties of a YouTube video that are required by Starchives.
/// </summary>
public class Video
{
	/// <summary>
	/// The ID that YouTube uses to uniquely identify the video.
	/// </summary>
	public string VideoId { get; set; }

	/// <summary>
	/// The date and time that the video was published. Note that this time might be different than the time that the video was uploaded.
	/// For example, if a video is uploaded as a private video and then made public at a later time, this property will specify the time that the video was made public.
	/// </summary>
	public DateTime PublishedAt { get; set; }

	/// <summary>
	/// The ID that YouTube uses to uniquely identify the channel that the video was uploaded to.
	/// </summary>
	public string ChannelId { get; set; }

	/// <summary>
	/// The video's title. The property value has a maximum length of 100 characters and may contain all valid UTF-8 characters except left and right angle brackets.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// The video's description. The property value has a maximum length of 5000 bytes and may contain all valid UTF-8 characters except left and right angle brackets.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The length of the video. The property value is an ISO 8601 duration. For example, for a video that is at least one minute long and less than one hour long,
	/// the duration is in the format PT#M#S, in which the letters PT indicate that the value specifies a period of time, and the letters M and S refer to length in
	/// minutes and seconds, respectively. The # characters preceding the M and S letters are both integers that specify the number of minutes (or seconds) of the video.
	/// For example, a value of PT15M33S indicates that the video is 15 minutes and 33 seconds long.
	/// <br/><br/>
	/// If the video is at least one hour long, the duration is in the format PT#H#M#S, in which the # preceding the letter H specifies the length of the video in hours
	/// and all of the other details are the same as described above. If the video is at least one day long, the letters P and T are separated, and the value's format is P#DT#H#M#S.
	/// Please refer to the ISO 8601 specification for complete details.
	/// </summary>
	public string Duration { get; set; }

	/// <summary>
	/// The number of times the video has been viewed.
	/// </summary>
	public long ViewCount { get; set; }

	/// <summary>
	/// The number of users who have indicated that they liked the video.
	/// </summary>
	public long LikeCount { get; set; }

	/// <summary>
	/// The number of comments for the video.
	/// </summary>
	public long CommentCount { get; set; }

	/// <summary>
	/// An iframe tag that embeds a player that plays the video.
	/// </summary>
	public string EmbedHtml { get; set; }

	/// <summary>
	/// Indicates whether captions are available for the video.
	/// </summary>
	public bool CaptionsAvailable { get; set; }

	/// <summary>
	/// The captions for the video.
	/// </summary>
	public ICollection<Caption> Captions { get; } = new List<Caption>();
}
