using System.ComponentModel;
using System.Runtime.CompilerServices;
using StorePOS.Models;

namespace StorePOS.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly CouchDbService _couchDbService;
    private readonly AppSettings _settings;
    
    private string _host;
    private int _port;
    private string _username;
    private string _password;
    private string _databaseName;
    private bool _isDarkMode;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Host
    {
        get => _host;
        set { _host = value; OnPropertyChanged(); }
    }

    public int Port
    {
        get => _port;
        set { _port = value; OnPropertyChanged(); }
    }

    public string Username
    {
        get => _username;
        set { _username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public string DatabaseName
    {
        get => _databaseName;
        set { _databaseName = value; OnPropertyChanged(); }
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set { _isDarkMode = value; OnPropertyChanged(); }
    }

    public SettingsViewModel(CouchDbService couchDbService, AppSettings settings)
    {
        _couchDbService = couchDbService;
        _settings = settings;
        
        Host = settings.Host;
        Port = settings.Port;
        Username = settings.Username;
        Password = settings.Password;
        DatabaseName = settings.DatabaseName;
        IsDarkMode = settings.IsDarkMode;
    }

    public void SaveSettings()
    {
        _settings.Host = Host;
        _settings.Port = Port;
        _settings.Username = Username;
        _settings.Password = Password;
        _settings.DatabaseName = DatabaseName;
        _settings.IsDarkMode = IsDarkMode;
        
        _couchDbService.UpdateSettings(_settings);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
