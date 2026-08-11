using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StorePOS.Models;
using StorePOS.Services;

namespace StorePOS.ViewModels;

public class InventoryViewModel : INotifyPropertyChanged
{
    private readonly CouchDbService _couchDbService;
    
    private ObservableCollection<Product> _products = new();
    private Product? _selectedProduct;
    private string _searchText = string.Empty;
    private bool _isEditing;
    private string _productName = string.Empty;
    private decimal _productPrice;
    private string _productBarcodes = string.Empty;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Product> Products
    {
        get => _products;
        set { _products = value; OnPropertyChanged(); }
    }

    public Product? SelectedProduct
    {
        get => _selectedProduct;
        set { _selectedProduct = value; OnPropertyChanged(); LoadSelectedProduct(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); FilterProducts(); }
    }

    public bool IsEditing
    {
        get => _isEditing;
        set { _isEditing = value; OnPropertyChanged(); }
    }

    public string ProductName
    {
        get => _productName;
        set { _productName = value; OnPropertyChanged(); }
    }

    public decimal ProductPrice
    {
        get => _productPrice;
        set { _productPrice = value; OnPropertyChanged(); }
    }

    public string ProductBarcodes
    {
        get => _productBarcodes;
        set { _productBarcodes = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ICommand LoadProductsCommand { get; }
    public ICommand AddNewProductCommand { get; }
    public ICommand SaveProductCommand { get; }
    public ICommand DeleteProductCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand EditProductCommand { get; }

    public InventoryViewModel(CouchDbService couchDbService)
    {
        _couchDbService = couchDbService;

        LoadProductsCommand = new RelayCommand(async () => await LoadProductsAsync());
        AddNewProductCommand = new RelayCommand(AddNewProduct);
        SaveProductCommand = new RelayCommand(async () => await SaveProductAsync());
        DeleteProductCommand = new RelayCommand(async () => await DeleteProductAsync());
        CancelEditCommand = new RelayCommand(CancelEdit);
        EditProductCommand = new RelayCommand(EditSelectedProduct);
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        var products = await _couchDbService.GetAllProductsAsync();
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
        IsLoading = false;
    }

    private void FilterProducts()
    {
        // Filtering is done in the view through CollectionViewSource or directly
    }

    private void AddNewProduct()
    {
        IsEditing = true;
        SelectedProduct = null;
        ProductName = string.Empty;
        ProductPrice = 0;
        ProductBarcodes = string.Empty;
    }

    private void EditSelectedProduct()
    {
        if (SelectedProduct == null) return;
        IsEditing = true;
        ProductName = SelectedProduct.Name;
        ProductPrice = SelectedProduct.Price;
        ProductBarcodes = string.Join(", ", SelectedProduct.Barcodes ?? new List<string>());
    }

    private void LoadSelectedProduct()
    {
        if (SelectedProduct != null && !IsEditing)
        {
            ProductName = SelectedProduct.Name;
            ProductPrice = SelectedProduct.Price;
            ProductBarcodes = string.Join(", ", SelectedProduct.Barcodes ?? new List<string>());
        }
    }

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(ProductName)) return;

        var product = SelectedProduct ?? new Product();
        product.Name = ProductName;
        product.Price = ProductPrice;
        product.Barcodes = ProductBarcodes.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim()).Where(b => !string.IsNullOrEmpty(b)).ToList();

        if (SelectedProduct == null)
        {
            product.Id = $"prod_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
            product.AdditionDate = DateTime.Now;
        }
        else
        {
            product.EditDate = DateTime.Now;
        }

        IsLoading = true;
        var success = await _couchDbService.SaveProductAsync(product);
        IsLoading = false;

        if (success)
        {
            await LoadProductsAsync();
            CancelEdit();
        }
    }

    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        IsLoading = true;
        var success = await _couchDbService.DeleteProductAsync(SelectedProduct.Id);
        IsLoading = false;

        if (success)
        {
            await LoadProductsAsync();
            CancelEdit();
        }
    }

    private void CancelEdit()
    {
        IsEditing = false;
        SelectedProduct = null;
        ProductName = string.Empty;
        ProductPrice = 0;
        ProductBarcodes = string.Empty;
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
