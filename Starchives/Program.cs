using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Serilog;
using Serilog.Events;
using Starchives.Components;
using Starchives.Data;
using Starchives.Facades.YouTube;

namespace Starchives;

/// <summary>
/// The entry class of the web app.
/// </summary>
public static class Program
{
	#region Fields
	private static string? _connectionString;
	#endregion



	#region Methods
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
					 .MinimumLevel.Information()
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
		// In Development, load environment variables from the local .env file (not committed)
		if (builder.Environment.IsDevelopment())
		{
			var envPath = Path.Combine(builder.Environment.ContentRootPath, ".env");
			LoadEnvFile(envPath);
		}

		// Also load process/user/machine environment variables
		builder.Configuration.AddEnvironmentVariables();

		// Read the connection string from env (populated by .env locally)
		_connectionString = Environment.GetEnvironmentVariable("DbConnectionStringPostgres");

		if (string.IsNullOrWhiteSpace(_connectionString))
			throw new InvalidOperationException("Connection string for Starchives database not found.");

		Log.Information("Configurations loaded");
	}



	/// <summary>
	/// Loads environment variables from a file containing key-value pairs in the format KEY=VALUE.
	/// </summary>
	/// <param name="filePath">The path to the environment file to load. The file must exist and contain lines in the format KEY=VALUE. Lines starting with '#' or blank lines are ignored.</param>
	private static void LoadEnvFile(string filePath)
	{
		if (!File.Exists(filePath)) return;

		foreach (var raw in File.ReadAllLines(filePath))
		{
			var line = raw.Trim();
			if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

			var idx = line.IndexOf('=');
			if (idx <= 0) continue;

			var key = line[..idx].Trim();
			var value = line[(idx + 1)..].Trim().Trim('"', '\''); // strip surrounding quotes

			if (!string.IsNullOrEmpty(key))
			{
				Environment.SetEnvironmentVariable(key, value);
			}
		}
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
																		.UseNpgsql(_connectionString ?? throw new InvalidOperationException("Connection string for Starchives database not found.")));
																		//.EnableSensitiveDataLogging());
		
		// services for Entity Framework Core
		builder.Services.AddQuickGridEntityFrameworkAdapter();
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		// services for custom logic
		builder.Services.AddScoped<IVideoApiFacade, YouTubeApiFacade>();
		builder.Services.AddSingleton<SharedService>();

		// services for the API controller
		builder.Services.AddScoped(sp =>
		{
			var cfg = sp.GetRequiredService<IConfiguration>();
			var nav = sp.GetRequiredService<NavigationManager>();

			var baseUrl = cfg["ApiBaseUrl"]; // set only in dev if you like
			var baseUri = !string.IsNullOrWhiteSpace(baseUrl)
							  ? new Uri(baseUrl,     UriKind.Absolute)
							  : new Uri(nav.BaseUri, UriKind.Absolute);

			return new HttpClient { BaseAddress = baseUri };
		});


		// services for server-side component rendering
		builder.Services.AddRazorComponents()
			   .AddInteractiveServerComponents();

		// allows navigation with relative URLs
		builder.Services.AddScoped(sp =>
		{
			var nav = sp.GetRequiredService<NavigationManager>();
			return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
		});

		// services for MudBlazor
		builder.Services.AddMudServices();

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
		// test endpoint
		app.MapGet("/api", async (StarchivesContext db) =>
		{
			var videos = await db.Videos.ToListAsync();
			Log.Information($"Retrieved {videos.Count} videos");
			return Results.Ok(videos);
		});



		app.MapGet("/health", () =>
		{
			var ver   = Environment.GetEnvironmentVariable("APP_VERSION")    ?? "unknown";
			var sha   = Environment.GetEnvironmentVariable("APP_SHA")        ?? "unknown";
			var built = Environment.GetEnvironmentVariable("APP_BUILD_TIME") ?? "unknown";
			return Results.Ok(new { status = "ok", version = ver, sha, built });
		});



		// gets a list of videos matching all valid query parameters included in the request
		// TODO: original working endpoint
		//app.MapGet("/api/videos", async (StarchivesContext db, HttpRequest request) =>
		//{
		//	var keywords      = request.Query["keywords"];
		//	var publishYear   = request.Query["publishYear"];
		//	var duration      = request.Query["duration"];
		//	var sortBy        = request.Query["sortBy"];
		//	var sortDirection = request.Query["sortDirection"];

		//	var videos = await db.Videos
		//						 .Where(video => db.Captions
		//										   .Any(caption => caption.VideoId == video.VideoId && EF.Functions.Like(caption.Text.ToLower(), $"%{keywords}%")))
		//						 .ToListAsync();

		//	// TODO: paginate the results
		//	var videoPages = new List<object>();



		//	return Results.Ok(videoPages);
		//});



		// TODO: second pagination attempt
		app.MapGet("/api/videos", async (StarchivesContext db, HttpRequest request) =>
		{
			var keywords      = request.Query["keywords"];
			var publishYear   = request.Query["publishYear"];
			var duration      = request.Query["duration"];
			var sortBy        = request.Query["sortBy"];
			var sortDirection = request.Query["sortDirection"];
			var page          = int.TryParse(request.Query["page"],     out var parsedPage) ? parsedPage : 1;
			var pageSize      = int.TryParse(request.Query["pageSize"], out var parsedPageSize) ? parsedPageSize : 10;

			// build the base query
			var videos = db.Videos
						  .Where(video => db.Captions
											.Any(caption => caption.VideoId == video.VideoId && EF.Functions.Like(caption.Text.ToLower(), $"%{keywords}%")));

			// apply sorting
			if (!string.IsNullOrEmpty(sortBy))
			{
				videos = sortDirection == "desc"
					? videos.OrderByDescending(video => EF.Property<object>(video, sortBy))
					: videos.OrderBy(video => EF.Property<object>(video,   sortBy));
			}

			// get the total count first (before applying Skip and Take)
			var videoCount = await videos.CountAsync();

			// apply pagination
			var paginatedData = await videos
									  .Skip((page - 1) * pageSize)
									  .Take(pageSize)
									  .Select(video => new
									  {
										  video.VideoId,
										  video.Title,
										  video.PublishedAt,
										  video.Duration,
										  video.ViewCount,
										  video.LikeCount,
										  video.CommentCount,
										  video.EmbedHtml,
										  video.Captions

										  // Add other fields you need here
									  })
									  .ToListAsync();

			// prepare the response object with pagination info
			var videoPage = new
			{
				CurrentPage = page,
				PageSize    = pageSize,
				VideoCount  = videoCount,
				PageCount   = (int)Math.Ceiling((double)videoCount / pageSize),
				Data        = paginatedData,
				Keywords    = keywords.ToString()
			};

			return Results.Ok(videoPage);
		});
	}
	#endregion
}
