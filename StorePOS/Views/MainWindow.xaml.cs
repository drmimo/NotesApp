using System.Windows;
using StorePOS.Models;
using StorePOS.Services;
using StorePOS.ViewModels;

namespace StorePOS.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;
    private readonly InventoryViewModel _inventoryViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    public MainWindow()
    {
        InitializeComponent();
        
        var settings = App.Settings;
        var dbService = App.DbService!;
        
        _mainViewModel = new MainViewModel(dbService, settings);
        _inventoryViewModel = new InventoryViewModel(dbService);
        _settingsViewModel = new SettingsViewModel(dbService, settings);
        
        DataContext = _mainViewModel;
        
        // Load initial data
        _inventoryViewModel.LoadProductsCommand.Execute(null);
        _mainViewModel.LoadOrdersCommand.Execute(null);
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsViewModel.SaveSettings();
        MessageBox.Show(this, "تم حفظ الإعدادات بنجاح", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
        
        // Apply theme change if needed
        ApplyTheme(_settingsViewModel.IsDarkMode);
    }
    
    private void ApplyTheme(bool isDarkMode)
    {
        var resources = Application.Current.Resources;
        
        if (isDarkMode)
        {
            resources["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(30, 30, 30));
            resources["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(45, 45, 45));
            resources["TextBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 255, 255));
            resources["SecondaryTextBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(176, 176, 176));
            resources["AccentBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(76, 175, 80));
            resources["ErrorBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(244, 67, 54));
        }
        else
        {
            resources["BackgroundBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(245, 245, 245));
            resources["SurfaceBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(255, 255, 255));
            resources["TextBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(33, 33, 33));
            resources["SecondaryTextBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(117, 117, 117));
            resources["AccentBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(56, 142, 60));
            resources["ErrorBrush"] = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(211, 47, 47));
        }
    }
}
