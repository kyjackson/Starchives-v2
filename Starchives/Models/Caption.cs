using System.ComponentModel.DataAnnotations.Schema;



namespace Starchives.Models;

/// <summary>
/// Defines the properties of a caption belonging to the caption track of a YouTube video.
/// </summary>
public class Caption
{
	/// <summary>
	/// The ID of the caption.
	/// </summary>
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long CaptionId { get; set; }

	/// <summary>
	/// The ID of the YouTube video to which the caption belongs.
	/// </summary>
	public string VideoId { get; set; }

	/// <summary>
	/// The duration of the caption.
	/// </summary>
	public TimeSpan Duration { get; set; }

	/// <summary>
	/// The time of the video at which the caption begins.
	/// </summary>
	public TimeSpan Offset { get; set; }

	/// <summary>
	/// The text of the caption.
	/// </summary>
	public string Text { get; set; }

	/// <summary>
	/// The YouTube video to which the caption belongs.
	/// </summary>
	public Video Video { get; set; }
}
