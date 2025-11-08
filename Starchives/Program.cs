using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MudExtensions.Services;
using Serilog;
using Serilog.Events;
using Starchives.Components;
using Starchives.Data;
using Starchives.Facades.YouTube;
using Starchives.Modules.Email;

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

		// Configure email options from configuration (appsettings.json or environment variables)
		builder.Services.Configure<ContactEmailOptions>(builder.Configuration.GetSection("ContactEmail"));

		// services for email sending
		builder.Services.AddTransient<IEmailSender, MailKitEmailSender>();

		// services for server-side component rendering
		builder.Services.AddRazorComponents()
			   .AddInteractiveServerComponents();

		// allows navigation with relative URLs
		builder.Services.AddScoped(sp =>
		{
			var nav = sp.GetRequiredService<NavigationManager>();
			return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
		});

		// services for MudBlazor and MudExtensions
		builder.Services.AddMudServices();
		builder.Services.AddMudExtensions();

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
		app.MapGet("/api/videos", async (StarchivesContext db, HttpRequest request) =>
		{
			var keywords        = request.Query["keywords"].ToString();
			var publishFromRaw  = request.Query["publishFrom"].ToString();
			var publishToRaw    = request.Query["publishTo"].ToString();
			var durationMinRaw  = request.Query["durationMin"].ToString();
			var durationMaxRaw  = request.Query["durationMax"].ToString();
			var sortBy          = request.Query["sortBy"].ToString();
			var sortDirection   = request.Query["sortDirection"].ToString();
			var page            = int.TryParse(request.Query["page"],     out var parsedPage) ? parsedPage : 1;
			var pageSize        = int.TryParse(request.Query["pageSize"], out var parsedPageSize) ? parsedPageSize : 10;

			// Process keywords: split by spaces and join with & for AND logic
			// Use websearch_to_tsquery for flexible, LIKE-style search with full-text speed
			var searchTerms = string.IsNullOrWhiteSpace(keywords) 
				? "" 
				: keywords.Trim();

			var videos = string.IsNullOrWhiteSpace(searchTerms)
				? db.Videos.AsQueryable()
				: db.Videos.Where(video => db.Captions.Any(caption => 
					caption.VideoId == video.VideoId && (
						// Try full-text first (smart word matching)
						caption.TextSearch!.Matches(EF.Functions.WebSearchToTsQuery("english", searchTerms))
						||
						// Fallback to substring (now indexed via trigram)
						EF.Functions.ILike(caption.Text, $"%{searchTerms}%")
					)));

			// apply published date range filter if provided (expected as years, e.g. 2024)
			if (int.TryParse(publishFromRaw, out var publishFromYear))
			{
				var fromDate = new DateTime(publishFromYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				videos = videos.Where(v => v.PublishedAt >= fromDate);
			}
			if (int.TryParse(publishToRaw, out var publishToYear))
			{
				// include the whole year until the end of Dec 31
				var toDate = new DateTime(publishToYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
				videos = videos.Where(v => v.PublishedAt <= toDate);
			}

			// Parse duration filter parameters (minutes)
			var hasDurationFilter = int.TryParse(durationMinRaw, out var durationMin) || int.TryParse(durationMaxRaw, out var durationMax);
			if (!int.TryParse(durationMinRaw, out durationMin)) durationMin = 0;
			if (!int.TryParse(durationMaxRaw, out durationMax)) durationMax = 0;

			// If there's no duration filter, keep database-side pagination and sorting for efficiency
			if (!hasDurationFilter)
			{
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
											  // project captions without TextSearch
											  Captions = video.Captions.Select(c => new
											  {
												  c.CaptionId,
												  c.Duration,
												  c.Offset,
												  c.Text,
												  c.VideoId
											  }).ToList()
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
					Keywords    = searchTerms  // Use trimmed value
				};

				return Results.Ok(videoPage);
			}

			// --- Duration filter present: need to evaluate ISO8601 durations in .NET (in-memory)
			// Project required fields, bring them into memory, parse durations to minutes/seconds, then filter + sort + paginate
			var projected = await videos
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
									  // project captions without TextSearch
									  Captions = video.Captions.Select(c => new
									  {
										  c.CaptionId,
										  c.Duration,
										  c.Offset,
										  c.Text,
										  c.VideoId
									  }).ToList()
								  })
								  .ToListAsync();

			// compute total seconds and minutes for each video
			var withDurationNumbers = projected.Select(p =>
			{
				var totalSeconds = ParseIso8601DurationToTotalSeconds(p.Duration);
				var minutes = totalSeconds / 60; // integer division => floor minutes
				return new
				{
					p.VideoId,
					p.Title,
					p.PublishedAt,
					p.Duration,
					p.ViewCount,
					p.LikeCount,
					p.CommentCount,
					p.EmbedHtml,
					p.Captions,
					Minutes = minutes,
					TotalSeconds = totalSeconds
				};
			}).AsQueryable();

			// apply duration range. UI uses 60 to mean "60+"
			if (durationMax >= 60)
			{
				withDurationNumbers = withDurationNumbers.Where(p => p.Minutes >= durationMin);
			}
			else
			{
				withDurationNumbers = withDurationNumbers.Where(p => p.Minutes >= durationMin && p.Minutes <= durationMax);
			}

			// apply sorting in-memory (based on known sort keys)
			if (!string.IsNullOrEmpty(sortBy))
			{
				withDurationNumbers = (sortBy, sortDirection?.ToLower()) switch
				{
					("PublishedAt", "desc") => withDurationNumbers.OrderByDescending(p => p.PublishedAt),
					("PublishedAt", _)      => withDurationNumbers.OrderBy(p => p.PublishedAt),
					("ViewCount", "desc")   => withDurationNumbers.OrderByDescending(p => p.ViewCount),
					("ViewCount", _)        => withDurationNumbers.OrderBy(p => p.ViewCount),
					("LikeCount", "desc")   => withDurationNumbers.OrderByDescending(p => p.LikeCount),
					("LikeCount", _)        => withDurationNumbers.OrderBy(p => p.LikeCount),
					// For Duration sorting, use total seconds so ordering includes seconds resolution
					("Duration", "desc")    => withDurationNumbers.OrderByDescending(p => p.TotalSeconds),
					("Duration", _)         => withDurationNumbers.OrderBy(p => p.TotalSeconds),
					_                       => withDurationNumbers
				};
			}

			var filteredList = withDurationNumbers.ToList();
			var filteredCount = filteredList.Count;

			var pageData = filteredList
						   .Skip((page - 1) * pageSize)
						   .Take(pageSize)
						   .Select(p => new
						   {
							   p.VideoId,
							   p.Title,
							   p.PublishedAt,
							   p.Duration,
							   p.ViewCount,
							   p.LikeCount,
							   p.CommentCount,
							   p.EmbedHtml,
							   p.Captions
						   })
						   .ToList();

			var resultPage = new
			{
				CurrentPage = page,
				PageSize    = pageSize,
				VideoCount  = filteredCount,
				PageCount   = (int)Math.Ceiling((double)filteredCount / pageSize),
				Data        = pageData,
				Keywords    = searchTerms  // Use trimmed value
			};

			return Results.Ok(resultPage);
		});

		app.MapPost("/api/contact", async (ContactRequest req, IEmailSender emailSender) =>
		{
			// Basic validation
			if (string.IsNullOrWhiteSpace(req.Message) || req.Message.Length < 10)
				return Results.BadRequest("Message too short.");

			if (req.Message.Length > 8000)
				return Results.BadRequest("Message too long.");

			var subject = string.IsNullOrWhiteSpace(req.Subject)
							  ? "New Starchives contact form submission"
							  : $"Starchives Mail: {req.Subject}";

			var body = $"""
					   New message received from Starchives contact form

					   Name: {req.Name ?? "(not provided)"}
					   Email: {req.Email ?? "(not provided)"}

					   Message:
					   {req.Message}
					   """;

			try
			{
				await emailSender.SendAsync(subject, body, req.Email);
				return Results.Ok();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to send contact email");
				return Results.Problem("Failed to send email. Please try again later.");
			}
		});
	}

	/// <summary>
	/// Parses an ISO 8601 duration (YouTube format like "PT15M33S" or "PT1H2M3S") to total whole seconds (floor).
	/// Returns 0 on parse failure.
	/// </summary>
	private static int ParseIso8601DurationToTotalSeconds(string? isoDuration)
	{
		if (string.IsNullOrWhiteSpace(isoDuration)) return 0;
		try
		{
			var ts = System.Xml.XmlConvert.ToTimeSpan(isoDuration);
			return (int)Math.Floor(ts.TotalSeconds);
		}
		catch
		{
			// If parsing fails, fall back to 0 seconds to avoid crashes.
			return 0;
		}
	}
	#endregion
}
