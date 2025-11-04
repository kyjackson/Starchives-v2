using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Starchives.Data;

/// <summary>
/// Provides a design-time factory for creating instances of the StarchivesContext for Entity Framework Core tooling.
/// </summary>
/// <remarks>
/// This class is used by Entity Framework Core tools to create a StarchivesContext instance at design
/// time, such as when running migrations or scaffolding. It reads the connection string from the
/// 'ConnectionStrings__Default' environment variable, and falls back to a default dummy connection string if the
/// environment variable is not set. The database provider is selected based on the format of the connection string,
/// supporting both PostgreSQL and SQL Server during transitions.
/// </remarks>
public sealed class StarchivesContextFactory : IDesignTimeDbContextFactory<StarchivesContext>
{
	/// <summary>
	/// Creates a new instance of the StarchivesContext configured for design-time operations.
	/// </summary>
	/// <param name="args">The command-line arguments. This parameter is not used in the context creation process.</param>
	/// <returns>A new StarchivesContext instance configured with a connection string determined by environment variables or a
	/// default value.</returns>
	public StarchivesContext CreateDbContext(string[] args)
	{
		var options = new DbContextOptionsBuilder<StarchivesContext>();

		// Prefer env var if provided (CI/App Platform)
		var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
		if (string.IsNullOrWhiteSpace(cs))
		{
			// Fallback dummy for design-time operations; EF won't connect for list/bundle
			cs = "Host=localhost;Database=dummy;Username=dummy;Password=dummy";
		}

		// Choose provider by prefix so you can support SQL Server or PG during transition
		if (cs.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
			cs.StartsWith("Server=", StringComparison.OrdinalIgnoreCase) && cs.Contains("Port="))
		{
			// PostgreSQL
			options.UseNpgsql(cs);
		}
		else
		{
			// SQL Server (if someone passes an old MSSQL string)
			options.UseSqlServer(cs);
		}

		return new StarchivesContext(options.Options);
	}
}