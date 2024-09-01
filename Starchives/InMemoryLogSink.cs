using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using Serilog.Formatting.Display;



namespace Starchives;

/// <summary>
/// Custom log sink that writes log events to a memory buffer.
/// </summary>
/// <param name="formatProvider">The format provider to use for the sink's logs.</param>
/// <param name="outputTemplate">The output template to use for the sink's logs.</param>
public class InMemoryLogSink(IFormatProvider? formatProvider = null, string? outputTemplate = null) : ILogEventSink
{
	private readonly        MessageTemplateTextFormatter _formatter = new(outputTemplate ?? string.Empty, formatProvider);
	private static readonly ConcurrentQueue<string>      LogEvents  = new();



	/// <summary>
	/// Adds a log event to the queue and dequeues logs as needed once the queue limit has been reached.
	/// </summary>
	/// <param name="logEvent">The log event to add to the queue.</param>
	public void Emit(LogEvent logEvent)
	{
		using var writer = new StringWriter();
		_formatter.Format(logEvent, writer);
		var message = writer.ToString();
		
		LogEvents.Enqueue(message);

		// Keep the buffer size manageable
		while (LogEvents.Count > 1000)
		{
			LogEvents.TryDequeue(out _);
		}
	}



	/// <summary>
	/// Gets all log events currently in the queue.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<string> GetLogEvents()
	{
		return LogEvents.ToArray();
	}
}



/// <summary>
/// Provides extensions for class <see cref="Starchives.InMemoryLogSink"/>.
/// </summary>
public static class InMemoryLogSinkExtensions
{
	/// <summary>
	/// Writes log events to <see cref="Starchives.InMemoryLogSink"/>.
	/// </summary>
	/// <param name="loggerConfiguration">The receiver of this method.</param>
	/// <param name="formatProvider">The format provider to use for the sink's logs.</param>
	/// <param name="outputTemplate">The output template to use for the sink's logs.</param>
	/// <returns>Configuration object allowing method chaining.</returns>
	public static LoggerConfiguration InMemoryLogSink
	(
		this LoggerSinkConfiguration loggerConfiguration,
		IFormatProvider?             formatProvider = null,
		string?                      outputTemplate = null
	)
	{
		return loggerConfiguration.Sink(new InMemoryLogSink(formatProvider, outputTemplate));
	}
}
