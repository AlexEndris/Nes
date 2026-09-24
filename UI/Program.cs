using System;

namespace UI
{
    using System.IO;

    public static class Program
    {
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                LogCrash(e.ExceptionObject as Exception);

            try
            {
                using var game = new Emulator();
                game.Run();
            }
            catch (Exception ex)
            {
                LogCrash(ex);
                throw;
            }
        }
        
        private static void LogCrash(Exception ex)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "crash.log");
            File.AppendAllText(path, $"{DateTime.Now:O}\n{ex}\n\n");
        }
    }
}