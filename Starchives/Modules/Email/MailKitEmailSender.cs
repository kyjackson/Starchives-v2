using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Starchives.Modules.Email;

/// <summary>
/// MailKit-based implementation of <see cref="IEmailSender"/> that sends a single
/// message to a fixed recipient configured via <see cref="ContactEmailOptions"/>.
/// </summary>
/// <remarks>
/// This sender enforces a strong identity by always using the configured <c>FromAddress</c>
/// and <c>ToAddress</c>, never arbitrary user-supplied values. An optional <c>replyTo</c>
/// address is validated and, if invalid, ignored with a logged warning.
/// SMTP connectivity uses STARTTLS (explicit TLS upgrade). Optional authentication
/// is performed when <see cref="ContactEmailOptions.UseAuthentication"/> is true and
/// a username is supplied.
/// </remarks>
public class MailKitEmailSender : IEmailSender
{
	/// <summary>
	/// Contact email configuration values (recipient, sender, SMTP host/port, auth).
	/// </summary>
	private readonly ContactEmailOptions _opts;

	/// <summary>
	/// Logger used for diagnostics (invalid reply-to, send failures, etc.).
	/// </summary>
	private readonly ILogger<MailKitEmailSender> _logger;

	/// <summary>
	/// Creates a new <see cref="MailKitEmailSender"/>.
	/// </summary>
	/// <param name="opts">Wrapped <see cref="ContactEmailOptions"/> retrieved via options pattern.</param>
	/// <param name="logger">Logger for operational and error events.</param>
	public MailKitEmailSender(
		IOptions<ContactEmailOptions> opts,
		ILogger<MailKitEmailSender> logger)
	{
		_opts   = opts.Value;
		_logger = logger;
	}

	/// <summary>
	/// Sends an email using the configured sender and recipient.
	/// </summary>
	/// <param name="subject">Subject line of the message.</param>
	/// <param name="body">Plain-text body content.</param>
	/// <param name="replyTo">
	/// Optional reply-to address. If malformed, it is ignored and a warning is logged.
	/// </param>
	/// <exception cref="MailKit.CommandException">
	/// May be thrown if the SMTP server rejects a command (wrapped by <see cref="Exception"/>).
	/// </exception>
	/// <exception cref="MailKit.ProtocolException">
	/// May be thrown for protocol-level errors (wrapped by <see cref="Exception"/>).
	/// </exception>
	/// <exception cref="System.Exception">
	/// Any unexpected error during connect, authenticate, or send. The original exception is logged then rethrown.
	/// </exception>
	/// <remarks>
	/// The method:
	/// 1. Builds a <see cref="MimeMessage"/> with fixed From/To addresses.
	/// 2. Validates and conditionally adds a Reply-To.
	/// 3. Connects via STARTTLS.
	/// 4. Authenticates if configured.
	/// 5. Sends and then disconnects.
	/// </remarks>
	public async Task SendAsync(string subject, string body, string? replyTo = null)
	{
		var message = new MimeMessage();

		// Strong identity: always from your domain, never from arbitrary user input
		message.From.Add(MailboxAddress.Parse(_opts.FromAddress));
		message.To.Add(MailboxAddress.Parse(_opts.ToAddress));
		message.Subject = subject;

		if (!string.IsNullOrWhiteSpace(replyTo))
		{
			try
			{
				message.ReplyTo.Add(MailboxAddress.Parse(replyTo));
			}
			catch (FormatException ex)
			{
				_logger.LogWarning(ex, "Invalid reply-to email ignored: {ReplyTo}", replyTo);
			}
		}

		var builder = new BodyBuilder
		{
			TextBody = body
		};

		message.Body = builder.ToMessageBody();

		using var client = new SmtpClient();

		try
		{
			// connect with STARTTLS
			await client.ConnectAsync(_opts.SmtpHost, _opts.SmtpPort, SecureSocketOptions.StartTls);

			if (_opts.UseAuthentication && !string.IsNullOrWhiteSpace(_opts.Username))
			{
				await client.AuthenticateAsync(_opts.Username, _opts.Password);
			}

			await client.SendAsync(message);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send contact email via MailKit.");
			throw; // let the endpoint decide how to present failures
		}
		finally
		{
			if (client.IsConnected)
				await client.DisconnectAsync(true);
		}
	}
}
