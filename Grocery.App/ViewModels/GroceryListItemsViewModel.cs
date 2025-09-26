using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grocery.App.Views;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Diagnostics;

namespace Grocery.App.ViewModels
{
    [QueryProperty(nameof(GroceryList), nameof(GroceryList))]
    public partial class GroceryListItemsViewModel : BaseViewModel
    {
        private readonly IGroceryListItemsService _groceryListItemsService;
        private readonly IProductService _productService;
        private readonly IFileSaverService _fileSaverService;

        // ✅ Interne lijst van alle producten
        private List<Product> _allProducts = new();

        public ObservableCollection<GroceryListItem> MyGroceryListItems { get; set; } = [];
        public ObservableCollection<Product> AvailableProducts { get; set; } = [];

        [ObservableProperty]
        GroceryList groceryList = new(0, "None", DateOnly.MinValue, "", 0);
        [ObservableProperty]
        string myMessage;

        public GroceryListItemsViewModel(IGroceryListItemsService groceryListItemsService, IProductService productService, IFileSaverService fileSaverService)
        {
            _groceryListItemsService = groceryListItemsService;
            _productService = productService;
            _fileSaverService = fileSaverService;
           
        }

        private void Load(int id)
        {
            MyGroceryListItems.Clear();
            foreach (var item in _groceryListItemsService.GetAllOnGroceryListId(id)) MyGroceryListItems.Add(item);
            GetAvailableProducts();
        }

        private void GetAvailableProducts()
        {
            AvailableProducts.Clear();

            _allProducts = _productService.GetAll().ToList();

            foreach (Product p in _allProducts)
            {
                bool zitAlOpLijst = MyGroceryListItems.Any(g => g.ProductId == p.Id);
                if (!zitAlOpLijst && p.Stock > 0)
                {
                    AvailableProducts.Add(p);
                }
            }
        }

        partial void OnGroceryListChanged(GroceryList value)
        {
            Load(value.Id);
        }

        [RelayCommand]
        public async Task ChangeColor()
        {
            Dictionary<string, object> paramater = new() { { nameof(GroceryList), GroceryList } };
            await Shell.Current.GoToAsync($"{nameof(ChangeColorView)}?Name={GroceryList.Name}", true, paramater);
        }
        [RelayCommand]
        public void AddProduct(Product product)
        {
            if (product == null) return;

            // Voeg alleen toe als het product nog niet op de lijst staat
            if (MyGroceryListItems.Any(g => g.ProductId == product.Id)) return;

            GroceryListItem nieuwItem = new(0, GroceryList.Id, product.Id, 1);
            _groceryListItemsService.Add(nieuwItem);

            product.Stock--; // optioneel
            OnGroceryListChanged(GroceryList); 
        }

        [RelayCommand]
        public async Task ShareGroceryList(CancellationToken cancellationToken)
        {
            if (GroceryList == null || MyGroceryListItems == null) return;
            string jsonString = JsonSerializer.Serialize(MyGroceryListItems);
            try
            {
                await _fileSaverService.SaveFileAsync("Boodschappen.json", jsonString, cancellationToken);
                await Toast.Make("Boodschappenlijst is opgeslagen.").Show(cancellationToken);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Opslaan mislukt: {ex.Message}").Show(cancellationToken);
            }
        }
        [RelayCommand]
        public void Search(string zoekterm)
        {

            IEnumerable<Product> filtered = _allProducts
                .Where(p => MyGroceryListItems.Count(g => g.ProductId == p.Id) < p.Stock);

            if (!string.IsNullOrWhiteSpace(zoekterm))
            {
                filtered = filtered.Where(p =>
                    p.Name.Contains(zoekterm, StringComparison.OrdinalIgnoreCase));
            }

            AvailableProducts.Clear();
            foreach (var p in filtered)
            {
                AvailableProducts.Add(p);
            }
        }
    }
}