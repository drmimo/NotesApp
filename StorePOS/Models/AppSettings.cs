namespace StorePOS.Models;

public class AppSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5984;
    public string Username { get; set; } = "admin";
    public string Password { get; set; } = "sama.store";
    public string DatabaseName { get; set; } = "storepos";
    public bool IsDarkMode { get; set; } = true;
}
