using Microsoft.EntityFrameworkCore;
using Starchives.Models;

namespace Starchives.Data;

/// <summary>
/// Represents the Entity Framework database context for the Starchives application, providing access to video, caption,
/// and admin update trigger entities.
/// </summary>
/// <remarks>
/// Use this context to query and manage entities in the Starchives database. The context tracks changes
/// to entities and coordinates persistence to the underlying database. Configure the context with appropriate options,
/// such as the database provider and connection string, when instantiating.
/// </remarks>
public class StarchivesContext : DbContext
{
	/// <summary>
	/// Initializes a new instance of the StarchivesContext class using the specified database context options.
	/// </summary>
	/// <remarks>
	/// Use this constructor to configure the context with specific options, such as for dependency
	/// injection or testing scenarios.
	/// </remarks>
	/// <param name="options">
	/// The options to be used by the DbContext, including configuration such as the database provider, connection string,
	/// and other context behaviors. Cannot be null.
	/// </param>
	public StarchivesContext (DbContextOptions<StarchivesContext> options) : base(options)
	{
	}

	/// <summary>
	/// The collection of videos in the database.
	/// </summary>
	/// <remarks>
	/// Use this property to query, add, update, or remove video entities within the context. Changes made
	/// to this collection are tracked by the context and persisted to the database when SaveChanges is called.
	/// </remarks>
	public DbSet<Video> Videos { get; set; } = default!;

	/// <summary>
	/// The collection of caption entities in the database context.
	/// </summary>
	public DbSet<Caption> Captions { get; set; } = default!;

	/// <summary>
	/// The collection of admin update triggers in the database.
	/// </summary>
	public DbSet<AdminUpdateTrigger> AdminUpdateTriggers { get; set; } = null!;

	#region Required
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Caption>()
					.Property(b => b.VideoId)
					.IsRequired();

		// Configure full-text search
		modelBuilder.Entity<Caption>()
					.HasGeneratedTsVectorColumn(
						c => c.TextSearch,
						"english",
						c => new { c.Text })
					.HasIndex(c => c.TextSearch)
					.HasMethod("GIN");

		// Index on VideoId for join optimization
		modelBuilder.Entity<Caption>()
					.HasIndex(c => c.VideoId);
	}
	#endregion
}
