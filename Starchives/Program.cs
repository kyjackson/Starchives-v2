using System.Diagnostics;
using Starchives.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Starchives.Data;
using Starchives.Facades.YouTube;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Build.Framework;
using Serilog.Events;



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
		/*
		 * If trying to find the source of a noisy log event to silence it, add {SourceContext} to the logTemplate below.
		 */
		const string logTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}] - {Level:u3} - {Message:lj}{NewLine}{Exception}";

		Log.Logger = new LoggerConfiguration()
					 .MinimumLevel.Debug()
					 .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.Components.RenderTree.Renderer", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.Components.Server.Circuits.RemoteRenderer", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.SignalR.Internal.DefaultHubDispatcher", LogEventLevel.Warning)
					 .MinimumLevel.Override("Microsoft.AspNetCore.Components.Server.ComponentHub", LogEventLevel.Warning)
					 .WriteTo.Console(outputTemplate: logTemplate)
					 .WriteTo.InMemoryLogSink(outputTemplate: logTemplate)
					 .CreateLogger();



		try
		{
			Log.Information("Starting up");

			// 1. create the web application builder
			var builder = WebApplication.CreateBuilder(args);
			//builder.Host.UseSerilog();

			// 2. get web app configurations from their sources
			ConfigureVariables(builder);

			// 3. add necessary services to the web app builder
			ConfigureServices(builder);

			// 4. complete the build of the web app
			var app = builder.Build();

			// 5. add middlewares to the web app, when applicable
			ConfigureMiddlewares(app);

			// 6. finally, run the web app
			app.Run();
		}
		catch (Exception ex)
		{
			Log.Fatal(ex, $"Application fatal error: {ex.Message}");
		}
		finally
		{
			Log.CloseAndFlush();
		}
	}



	/// <summary>
	/// Sets up configuration sources for the web app.
	/// </summary>
	/// <param name="builder">The web app builder to configure.</param>
	private static void ConfigureVariables(WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables();

		// retrieve the connection string from the environment variables
		// with Docker Compose, all special env vars are set from .env file
		_connectionString = Environment.GetEnvironmentVariable("Keys__ConnectionString");

		Log.Information("Configurations loaded");
	}



	/// <summary>
	/// Sets up services for the web app.
	/// </summary>
	/// <param name="builder">The web app builder for which services will be set up.</param>
	private static void ConfigureServices(WebApplicationBuilder builder)
	{
		builder.Services.AddSerilog();

		builder.Services.Configure<Keys>(builder.Configuration.GetSection("Keys"));
		builder.Services.AddScoped<IYouTubeApiFacade, YouTubeApiFacade>();
		builder.Services.AddDbContextFactory<StarchivesContext>(options =>
																	options
																		.UseSqlServer(_connectionString ?? throw new InvalidOperationException("Connection string for Starchives database not found.")));
																		//.EnableSensitiveDataLogging());
		builder.Services.AddQuickGridEntityFrameworkAdapter();

		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		// Add services to the container.
		builder.Services.AddSingleton<SharedService>();

		builder.Services.AddRazorComponents()
			   .AddInteractiveServerComponents();

		Log.Information("Services loaded");
	}



	/// <summary>
	/// Sets up middlewares for the web app.
	/// </summary>
	/// <param name="app">The web app for which middlewares will be set up.</param>
	private static void ConfigureMiddlewares(WebApplication app)
	{
		app.UseSerilogRequestLogging(options =>
		{
			// Customize the message template
			options.MessageTemplate = "Handled {RequestPath}";

			// Emit debug-level events instead of the defaults
			options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Debug;

			// Attach additional properties to the request completion event
			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				diagnosticContext.Set("RequestHost",   httpContext.Request.Host.Value);
				diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
			};
		});

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

		Log.Information("Middlewares loaded");
	}
	#endregion
}
