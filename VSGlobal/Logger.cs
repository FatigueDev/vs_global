using Microsoft.Extensions.Logging;
using Vintagestory.API.Client;

namespace VSGlobal
{
    /// <exclude />
    public class Logger : ILogger
	{
		private readonly string logFilePath;
		
		public Logger(ICoreClientAPI api)
		{
			logFilePath = Path.Combine(api.DataBasePath, "Logs", "client-vsglobal.txt");
			
			if(File.Exists(logFilePath))
			{
				File.CreateText(logFilePath);
			}
		}
		
		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return false;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			(state?.ToString() switch
			{
				"A pong was received." => ReturnPattern(),
				"It has been signaled." => ReturnPattern(),
				"WS: Do keep-alive ping..." => ReturnPattern(),
				string s when s.Contains("sessionkey") => RedactPattern(),
				_ => DefaultPattern(logLevel, state, exception),
			}).Invoke();
		}
		
		private Action ReturnPattern() => () => {};
		private Action RedactPattern()
		{
			return () => {
				using StreamWriter stream = File.AppendText(logFilePath);
				stream.WriteLineAsync($"REDACTION: We have sent data to the server that contains your session key. This is normal for Vintage Story authorization services. We do NOT store your session key.");
				stream.Close();
			};
		}
		private Action DefaultPattern<TState>(LogLevel logLevel, TState state, Exception? exception)
		{
			return () => {
				using StreamWriter stream = File.AppendText(logFilePath);
				stream.WriteLineAsync($"{logLevel}: {state}" + $"{(exception != null ? $" {exception.Message}" : "")}");
				stream.Close();
			};
		}
	}
}