using System.Diagnostics;
using Starchives.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Starchives.Data;
using Starchives.Facades.YouTube;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Build.Framework;
using Serilog.Events;
using Microsoft.AspNetCore.Components;



namespace Starchives;

/// <summary>
/// The entry class of the web app.
/// </summary>
public static class Program
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

			/*
			 *	1. create the web application builder
			 *	2. get web app configurations from their sources
			 *	3. add necessary services to the web app builder
			 */
			var builder = WebApplication.CreateBuilder(args);
			builder.ConfigureVariables();
			builder.ConfigureServices();

			/*
			 *	4. complete the build of the web app
			 *	5. add middlewares to the web app
			 *	6. configure API endpoints for the web app
			 *	7. finally, run the web app
			 */
			var app = builder.Build();
			app.ConfigureMiddlewares();
			app.ConfigureApi();
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
	private static void ConfigureVariables(this WebApplicationBuilder builder)
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
	private static void ConfigureServices(this WebApplicationBuilder builder)
	{
		// services for enhanced logging
		builder.Services.AddSerilog();

		// services for credential configuration
		builder.Services.Configure<Keys>(builder.Configuration.GetSection("Keys"));

		// services for database access
		builder.Services.AddDbContextFactory<StarchivesContext>(options =>
																	options
																		.UseSqlServer(_connectionString ?? throw new InvalidOperationException("Connection string for Starchives database not found.")));
																		//.EnableSensitiveDataLogging());
		
		// services for Entity Framework Core
		builder.Services.AddQuickGridEntityFrameworkAdapter();
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		// services for custom logic
		builder.Services.AddScoped<IYouTubeApiFacade, YouTubeApiFacade>();
		builder.Services.AddSingleton<SharedService>();

		// services for the API controller
		builder.Services.AddScoped<HttpClient>();
		builder.Services.AddHttpClient("api", client =>
		{
			client.BaseAddress = new Uri("http://localhost:8080");
		});
		
		

		// services for server-side component rendering
		builder.Services.AddRazorComponents()
			   .AddInteractiveServerComponents();

		Log.Information("Services loaded");
	}



	/// <summary>
	/// Sets up middlewares for the web app.
	/// </summary>
	/// <param name="app">The web app for which middlewares will be set up.</param>
	private static void ConfigureMiddlewares(this WebApplication app)
	{
		// middlewares for enhanced logging
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

		// middlewares for the HTTP request pipeline
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error", createScopeForErrors: true);

			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
			app.UseMigrationsEndPoint();
			app.UseHttpsRedirection();
		}

		app.UseStaticFiles();
		app.UseAntiforgery();

		// middlewares for server-side component rendering
		app.MapRazorComponents<App>()
		   .AddInteractiveServerRenderMode();

		Log.Information("Middlewares loaded");
	}

	

	/// <summary>
	/// Sets up API endpoints for the web app.
	/// </summary>
	/// <param name="app">The web app for which API endpoints will be configured.</param>
	private static void ConfigureApi(this WebApplication app)
	{
		// map API routes
		app.MapGet("/api", async (StarchivesContext db) =>
		{
			var videos = await db.Videos.ToListAsync();
			Log.Information($"Retrieved {videos.Count} videos");
			return Results.Ok(videos);
		});

		app.MapGet("/api/results", async (StarchivesContext db, string query, string date = "1/1/2024", string duration = "10", string orderBy = "Date", string order = "desc", int page = 1) =>
		{
			var videos = await db.Videos
								 .Where(video => db.Captions
												   .Any(caption => caption.VideoId == video.VideoId && EF.Functions.Like(caption.Text.ToLower(), $"%{query}%")))
								 .ToListAsync();

			return Results.Ok(videos);
		});

		app.MapGet("/api/videos", async (StarchivesContext db, HttpRequest request) =>
		{
			var query = request.Query["query"];

			var videos = await db.Videos
								 .Where(video => db.Captions
												   .Any(caption => caption.VideoId == video.VideoId && EF.Functions.Like(caption.Text.ToLower(), $"%{query}%")))
								 .ToListAsync();

			return Results.Ok(videos);
		});
	}
	#endregion
}
