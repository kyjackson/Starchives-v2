namespace Starchives.Modules.Email;

/// <summary>
/// Defines a contract for sending email messages asynchronously.
/// </summary>
/// <remarks>
/// Implementations of this interface are responsible for delivering email messages using the specified
/// subject and body. The delivery mechanism and recipient details are determined by the implementation. This interface
/// is typically used to abstract email sending functionality for dependency injection and testing purposes.
/// </remarks>
public interface IEmailSender
{
	/// <summary>
	/// Sends an email message asynchronously.
	/// </summary>
	/// <param name="subject">The subject line of the email.</param>
	/// <param name="body">The content/body of the email.</param>
	/// <param name="replyTo">Optional reply-to email address. If null, the implementation's default sender or reply-to address will be used.</param>
	/// <returns>A task that represents the asynchronous send operation.</returns>
	Task SendAsync(string subject, string body, string? replyTo = null);
}
