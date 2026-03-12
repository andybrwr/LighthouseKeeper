using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace LighthouseKeeper;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.Run(new MainController());
    }
}