using System.Windows;
using StorePOS.Models;
using StorePOS.Services;
using StorePOS.ViewModels;

namespace StorePOS;

public partial class App : Application
{
    public static AppSettings Settings { get; private set; } = new();
    public static CouchDbService? DbService { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        DbService = new CouchDbService(Settings);
    }
}
