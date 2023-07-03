using EShop.Domain.DomainModels;
using System.Collections.Generic;

namespace EShop.Domain.DTO
{
    public class ShoppingCartDto
    {
        public List<ProductInShoppingCart> ProductInShoppingCarts { get; set; }

        public double TotalPrice { get; set; }


    }
}
