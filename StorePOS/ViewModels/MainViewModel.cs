using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StorePOS.Models;
using StorePOS.Services;

namespace StorePOS.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly CouchDbService _couchDbService;
    private readonly AppSettings _settings;
    
    private int _selectedTabIndex;
    private string _barcodeInput = string.Empty;
    private string _searchText = string.Empty;
    private ObservableCollection<CartItem> _cartItems = new();
    private ObservableCollection<Product> _searchResults = new();
    private ObservableCollection<Order> _orders = new();
    private Order? _selectedOrder;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set { _selectedTabIndex = value; OnPropertyChanged(); }
    }

    public string BarcodeInput
    {
        get => _barcodeInput;
        set { _barcodeInput = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); SearchCommand.Execute(null); }
    }

    public ObservableCollection<CartItem> CartItems
    {
        get => _cartItems;
        set { _cartItems = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Product> SearchResults
    {
        get => _searchResults;
        set { _searchResults = value; OnPropertyChanged(); }
    }

    public ObservableCollection<Order> Orders
    {
        get => _orders;
        set { _orders = value; OnPropertyChanged(); }
    }

    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set { _selectedOrder = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public decimal CartTotal => CartItems.Sum(i => i.Total);

    public ICommand AddToCartCommand { get; }
    public ICommand RemoveFromCartCommand { get; }
    public ICommand IncrementQuantityCommand { get; }
    public ICommand DecrementQuantityCommand { get; }
    public ICommand CheckoutCommand { get; }
    public ICommand CancelCartCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand LoadOrdersCommand { get; }
    public ICommand ProcessBarcodeCommand { get; }

    public MainViewModel(CouchDbService couchDbService, AppSettings settings)
    {
        _couchDbService = couchDbService;
        _settings = settings;

        AddToCartCommand = new RelayCommand<Product>(AddToCart);
        RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
        IncrementQuantityCommand = new RelayCommand<CartItem>(IncrementQuantity);
        DecrementQuantityCommand = new RelayCommand<CartItem>(DecrementQuantity);
        CheckoutCommand = new RelayCommand(async () => await CheckoutAsync());
        CancelCartCommand = new RelayCommand(CancelCart);
        SearchCommand = new RelayCommand(async () => await SearchProductsAsync());
        LoadOrdersCommand = new RelayCommand(async () => await LoadOrdersAsync());
        ProcessBarcodeCommand = new RelayCommand(async () => await ProcessBarcodeAsync());
    }

    private async Task ProcessBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput)) return;

        var product = await _couchDbService.GetProductByBarcodeAsync(BarcodeInput.Trim());
        if (product != null)
        {
            AddToCart(product);
            BarcodeInput = string.Empty;
        }
    }

    private void AddToCart(Product? product)
    {
        if (product == null) return;

        var existingItem = CartItems.FirstOrDefault(i => i.Product.Id == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
            OnPropertyChanged(nameof(CartTotal));
        }
        else
        {
            CartItems.Add(new CartItem { Product = product, Quantity = 1 });
            OnPropertyChanged(nameof(CartTotal));
        }
    }

    private void RemoveFromCart(CartItem? item)
    {
        if (item == null || !CartItems.Contains(item)) return;
        CartItems.Remove(item);
        OnPropertyChanged(nameof(CartTotal));
    }

    private void IncrementQuantity(CartItem? item)
    {
        if (item == null) return;
        item.Quantity++;
        OnPropertyChanged(nameof(CartTotal));
    }

    private void DecrementQuantity(CartItem? item)
    {
        if (item == null) return;
        if (item.Quantity > 1)
        {
            item.Quantity--;
        }
        else
        {
            RemoveFromCart(item);
        }
        OnPropertyChanged(nameof(CartTotal));
    }

    private async Task CheckoutAsync()
    {
        if (!CartItems.Any()) return;

        var order = new Order
        {
            Id = $"order_{DateTime.Now:yyyyMMddHHmmss}",
            OrderId = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}",
            OrderDate = DateTime.Today,
            OrderTime = DateTime.Now.TimeOfDay,
            Total = CartTotal,
            Products = CartItems.Select(i => new OrderProduct
            {
                ProductId = i.Product.Id,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        IsLoading = true;
        var success = await _couchDbService.SaveOrderAsync(order);
        IsLoading = false;

        if (success)
        {
            CancelCart();
            await LoadOrdersAsync();
        }
    }

    private void CancelCart()
    {
        CartItems.Clear();
        OnPropertyChanged(nameof(CartTotal));
    }

    private async Task SearchProductsAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            SearchResults.Clear();
            return;
        }

        var products = await _couchDbService.SearchProductsByNameAsync(SearchText);
        SearchResults.Clear();
        foreach (var product in products)
        {
            SearchResults.Add(product);
        }
    }

    private async Task LoadOrdersAsync()
    {
        IsLoading = true;
        var orders = await _couchDbService.GetAllOrdersAsync();
        Orders.Clear();
        foreach (var order in orders.OrderByDescending(o => o.OrderDate).ThenByDescending(o => o.OrderTime))
        {
            Orders.Add(order);
        }
        IsLoading = false;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
