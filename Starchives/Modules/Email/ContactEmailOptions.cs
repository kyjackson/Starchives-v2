namespace Starchives.Modules.Email;

/// <summary>
/// Configuration options for sending contact emails.
/// </summary>
public class ContactEmailOptions
{
	/// <summary>
	/// Destination (recipient) email address for contact messages.
	/// </summary>
	public string ToAddress { get; set; } = default!;

	/// <summary>
	/// Origin (sender) email address used in the outbound message.
	/// </summary>
	public string FromAddress { get; set; } = default!;

	/// <summary>
	/// SMTP server host name. Defaults to Google's relay host.
	/// </summary>
	public string SmtpHost { get; set; } = "smtp-relay.gmail.com";

	/// <summary>
	/// SMTP server port. Typically 587 for STARTTLS or 465 for implicit TLS.
	/// </summary>
	public int SmtpPort { get; set; } = 587;

	/// <summary>
	/// Indicates whether SMTP authentication should be attempted.
	/// </summary>
	public bool UseAuthentication { get; set; } = false;

	/// <summary>
	/// Username for SMTP authentication (ignored if <see cref="UseAuthentication"/> is false).
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Password for SMTP authentication (ignored if <see cref="UseAuthentication"/> is false).
	/// </summary>
	public string? Password { get; set; }
}
