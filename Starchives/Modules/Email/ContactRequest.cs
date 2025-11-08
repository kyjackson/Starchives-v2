namespace Starchives.Modules.Email;

/// <summary>
/// Represents a contact form submission request containing user information and message details.
/// </summary>
public record ContactRequest
{
	/// <summary>
	/// Gets or sets the name of the person submitting the contact request.
	/// </summary>
	public string? Name { get; set; }
	
	/// <summary>
	/// Gets or sets the email address of the person submitting the contact request.
	/// </summary>
	public string? Email { get; set; }
	
	/// <summary>
	/// Gets or sets the message content of the contact request.
	/// </summary>
	public string Message { get; set; }
	
	/// <summary>
	/// Gets or sets the subject line for the contact request.
	/// </summary>
	public string? Subject { get; set; }
}
