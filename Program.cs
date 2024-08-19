using System.Diagnostics;
using Starchives.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Starchives.Data;

namespace Starchives;

/// <summary>
/// The entry class of the web app.
/// </summary>
public class Program
{
	#region Fields
	private static string? _connectionString;
	#endregion



	#region Functions
	/// <summary>
	/// The main method of the web app. The web app begins and ends with this method.
	/// </summary>
	/// <param name="args">Arguments to be interpreted at the entry point of the web app.</param>
	public static void Main(string[] args)
	{
		// 1. create the web application builder
		var builder = WebApplication.CreateBuilder(args);

		// 2. get web app configurations from their sources
		GetConfigurations(builder);

		// 3. add necessary services to the web app builder
		GetServices(builder);

		// 4. complete the build of the web app
		var app = builder.Build();

		// 5. add middlewares to the web app, when applicable
		GetMiddlewares(app);

		// 6. finally, run the web app
		app.Run();
	}



	/// <summary>
	/// Sets up configuration sources for the web app.
	/// </summary>
	/// <param name="builder">The web app builder to configure.</param>
	static void GetConfigurations(WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables("ConnectionStrings_");

		// retrieve the connection string from the environment variables
		//_connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
		_connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

		Debug.Print(_connectionString);

		Debug.Print("Configurations loaded");
	}



	/// <summary>
	/// Sets up services for the web app.
	/// </summary>
	/// <param name="builder">The web app builder for which services will be set up.</param>
	static void GetServices(WebApplicationBuilder builder)
	{
		builder.Services.AddDbContextFactory<StarchivesContext>(options =>
			options.UseSqlServer(_connectionString ?? throw new InvalidOperationException("Connection string for Starchives database not found.")));

		builder.Services.AddQuickGridEntityFrameworkAdapter();

		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		Debug.Print("Services loaded");
	}



	/// <summary>
	/// Sets up middlewares for the web app.
	/// </summary>
	/// <param name="app">The web app for which middlewares will be set up.</param>
	static void GetMiddlewares(WebApplication app)
	{
		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error", createScopeForErrors: true);

			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
			app.UseMigrationsEndPoint();
		}

		app.UseHttpsRedirection();
		app.UseStaticFiles();
		app.UseAntiforgery();
		app.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		Debug.Print("Middlewares loaded");
	}
	#endregion
}
