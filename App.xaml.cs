using System.Text;
using System.Windows;
using T4C_Translator.Properties;
using System.IO;

namespace T4C_Translator
{
	public partial class App : System.Windows.Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			base.OnStartup(e);
			Main mainWindow = new Main();
			mainWindow.Show();
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DispatcherUnhandledException += App_DispatcherUnhandledException;
		}

		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			LogException(e.Exception);
			e.Handled = true;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				LogException(ex);
			}
		}

		private void LogException(Exception ex)
		{
			string logFilePath = "t4c_translator_error_log.txt";
			string logMessage = $"[{DateTime.Now}] {ex.Message}\n{ex.StackTrace}\n";

			try
			{
				// Check if the file exists, and create it if it doesn't
				if (!File.Exists(logFilePath))
				{
					using (var stream = File.Create(logFilePath))
					{
						// Optionally, you can write a header or initial content to the file here
						byte[] info = new UTF8Encoding(true).GetBytes("--T4C Translator Error Log--\n\n");
						stream.Write(info, 0, info.Length);
					}
				}

				// Append the log message to the file
				File.AppendAllText(logFilePath, logMessage);
			}
			catch (Exception logEx)
			{
				// If logging fails, there's not much we can do, but you might want to handle this case.
				System.Windows.MessageBox.Show($"Failed to log error: {logEx.Message}");
			}
		}

		private void OnExit(object sender, ExitEventArgs e)
		{
			Settings.Default.Save();
		}
	}
}


