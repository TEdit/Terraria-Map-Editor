using System;
using System.Diagnostics;
using Velopack;

namespace TEdit;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var totalSw = Stopwatch.StartNew();
        var sw = Stopwatch.StartNew();

        VelopackApp.Build().Run();
        Trace.WriteLine($"[Startup] VelopackApp.Run: {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        var app = new App();
        Trace.WriteLine($"[Startup] new App() (static ctor): {sw.ElapsedMilliseconds}ms");

        sw.Restart();
        app.InitializeComponent();
        Trace.WriteLine($"[Startup] InitializeComponent: {sw.ElapsedMilliseconds}ms");

        Trace.WriteLine($"[Startup] Total before app.Run(): {totalSw.ElapsedMilliseconds}ms");

        app.Run();
    }
}
