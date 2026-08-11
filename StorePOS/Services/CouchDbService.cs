using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StorePOS.Models;

namespace StorePOS.Services;

public class CouchDbService
{
    private readonly HttpClient _httpClient;
    private readonly AppSettings _settings;
    private string ProductsDb => $"{_settings.DatabaseName}_products";
    private string OrdersDb => $"{_settings.DatabaseName}_orders";

    public CouchDbService(AppSettings settings)
    {
        _settings = settings;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{_settings.Host}:{_settings.Port}/")
        };
        
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> EnsureDatabaseExistsAsync(string dbName)
    {
        try
        {
            var response = await _httpClient.PutAsync($"/{dbName}", null);
            return response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        try
        {
            await EnsureDatabaseExistsAsync(ProductsDb);
            var response = await _httpClient.GetAsync($"/{ProductsDb}/_all_docs?include_docs=true");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AllDocsResponse<Product>>(json);
                return result?.Rows?.Select(r => r.Doc).ToList() ?? new List<Product>();
            }
        }
        catch { }
        return new List<Product>();
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        try
        {
            await EnsureDatabaseExistsAsync(ProductsDb);
            var response = await _httpClient.GetAsync($"/{ProductsDb}/_all_docs?include_docs=true");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AllDocsResponse<Product>>(json);
                return result?.Rows?.FirstOrDefault(r => r.Doc.Barcodes?.Contains(barcode) == true)?.Doc;
            }
        }
        catch { }
        return null;
    }

    public async Task<List<Product>> SearchProductsByNameAsync(string searchTerm)
    {
        try
        {
            await EnsureDatabaseExistsAsync(ProductsDb);
            var allProducts = await GetAllProductsAsync();
            return allProducts.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        catch
        {
            return new List<Product>();
        }
    }

    public async Task<bool> SaveProductAsync(Product product)
    {
        try
        {
            await EnsureDatabaseExistsAsync(ProductsDb);
            var json = JsonConvert.SerializeObject(product);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = string.IsNullOrEmpty(product.Id) 
                ? $"/{ProductsDb}" 
                : $"/{ProductsDb}/{product.Id}";
            
            var method = string.IsNullOrEmpty(product.Id) ? HttpMethod.Post : HttpMethod.Put;
            var response = await _httpClient.SendAsync(new HttpRequestMessage(method, url) { Content = content });
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(string productId)
    {
        try
        {
            await EnsureDatabaseExistsAsync(ProductsDb);
            var response = await _httpClient.DeleteAsync($"/{ProductsDb}/{productId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> SaveOrderAsync(Order order)
    {
        try
        {
            await EnsureDatabaseExistsAsync(OrdersDb);
            var json = JsonConvert.SerializeObject(order);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = $"/{OrdersDb}/{order.Id}";
            var response = await _httpClient.PutAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        try
        {
            await EnsureDatabaseExistsAsync(OrdersDb);
            var response = await _httpClient.GetAsync($"/{OrdersDb}/_all_docs?include_docs=true");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AllDocsResponse<Order>>(json);
                return result?.Rows?.Select(r => r.Doc).ToList() ?? new List<Order>();
            }
        }
        catch { }
        return new List<Order>();
    }

    public void UpdateSettings(AppSettings settings)
    {
        _settings.Host = settings.Host;
        _settings.Port = settings.Port;
        _settings.Username = settings.Username;
        _settings.Password = settings.Password;
        _settings.DatabaseName = settings.DatabaseName;
        
        _httpClient.BaseAddress = new Uri($"http://{_settings.Host}:{_settings.Port}/");
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }
}

public class AllDocsResponse<T>
{
    [JsonProperty("rows")]
    public List<Row<T>> Rows { get; set; } = new();
}

public class Row<T>
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;
    
    [JsonProperty("doc")]
    public T Doc { get; set; } = default!;
}
