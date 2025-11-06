using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Starchives.Models;

/// <summary>
/// Represents an administrative trigger entity used to track and manage update operations for various resource types.
/// </summary>
/// <remarks>
/// This class encapsulates metadata and operational details for triggers that initiate administrative
/// updates, such as for videos, captions, or documents. It includes information about the trigger type, status, user
/// activity, and structured details stored as JSON.
/// </remarks>
public class AdminUpdateTrigger
{
	/// <summary>
	/// The unique identifier for the entity.
	/// </summary>
	[Key]
	public long Id { get; set; }

	/// <summary>
	/// The unique key that identifies the type of trigger to be used.
	/// </summary>
	/// <remarks>
	/// This property is required and must be set to a non-empty string. Common values include "videos",
	/// "captions", or "documents", depending on the trigger type needed. The value determines which trigger logic will be
	/// applied.
	/// </remarks>
	[Required]
	public string TriggerKey { get; set; } = null!;   // e.g. "videos", "captions", "documents"

	/// <summary>
	/// The descriptive text associated with the trigger.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// The date and time when the trigger was last activated.
	/// </summary>
	public DateTimeOffset? LastTriggered { get; set; }

	/// <summary>
	/// The status message representing the outcome of the most recent operation.
	/// </summary>
	/// <remarks>
	/// Typical values include "success", "failed", or other descriptive status indicators. The value may
	/// be null if no status has been set.
	/// </remarks>
	public string? LastStatus { get; set; }

	/// <summary>
	/// The name of the last user who accessed or modified the trigger.
	/// </summary>
	public string? LastUser   { get; set; }

	/// <summary>
	/// The structured details associated with the trigger as a JSON document.
	/// </summary>
	/// <remarks>
	/// The value is stored in the database using the PostgreSQL 'jsonb' type, allowing for efficient
	/// querying and storage of complex data. The property may be null if no details are available.
	/// </remarks>
	[Column(TypeName = "jsonb")]
	public JsonDocument? Details { get; set; }

	/// <summary>
	/// The date and time when the object was created.
	/// </summary>
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// The date and time when the entity was last updated.
	/// </summary>
	public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
