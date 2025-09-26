
using CommunityToolkit.Mvvm.ComponentModel;

namespace Grocery.Core.Models
{
    public partial class GroceryListItem : ObservableObject
    {
        public int GroceryListId { get; set; }
        public int ProductId { get; set; }

        [ObservableProperty]
        private int amount;

        public Product Product { get; set; } = new(0, "None", 0);

        public GroceryListItem(int id, int groceryListId, int productId, int amount)
        {
            Id = id;
            GroceryListId = groceryListId;
            ProductId = productId;
            this.amount = amount;
        }

        public int Id { get; set; } // ← als 'Model' deze bevatte, kun je dit ook laten staan
    }
}