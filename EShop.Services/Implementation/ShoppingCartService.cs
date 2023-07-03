using EShop.Domain.DomainModels;
using EShop.Domain.DTO;
using EShop.Repository.Interface;
using EShop.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EShop.Services.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<EmailMessage> _mailRepository;
        private readonly IRepository<ProductInOrder> _productInOrderRepository;
        private readonly IUserRepository _userRepository;

        public ShoppingCartService(IRepository<EmailMessage> mailRepository, IRepository<ShoppingCart> shoppingCartRepository, IRepository<ProductInOrder> productInOrderRepository, IUserRepository userRepository, IRepository<Order> orderRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _userRepository = userRepository;
            _productInOrderRepository = productInOrderRepository;
            _orderRepository = orderRepository;
            _mailRepository = mailRepository;
        }

        public bool deleteProductFromShoppingCart(string userId, Guid id)
        {
            if (!string.IsNullOrEmpty(userId) && id != null)
            {
                var loggedInUser = this._userRepository.Get(userId);

                var userShoppingCart = loggedInUser.UserCart;

                var itemToDelete = userShoppingCart.ProductInShoppingCarts.Where(z => z.ProductId.Equals(id)).FirstOrDefault();

                userShoppingCart.ProductInShoppingCarts.Remove(itemToDelete);

                this._shoppingCartRepository.Update(userShoppingCart);

                return true;
            }
            return false;
        }

        public ShoppingCartDto getShoppingCartInfo(string userId)
        {
            var loggedInUser = this._userRepository.Get(userId);

            var userShoppingCart = loggedInUser.UserCart;

            var AllProducts = userShoppingCart.ProductInShoppingCarts.ToList();

            var allProductPrice = AllProducts.Select(z => new
            {
                ProductPrice = z.Product.ProductPrice,
                Quantity = z.Quantity
            }).ToList();

            double totalPrice = 0.0;

            foreach (var item in allProductPrice)
            {
                totalPrice += item.ProductPrice * item.Quantity;
            }

            ShoppingCartDto shoppingCartToItem = new ShoppingCartDto
            {
                ProductInShoppingCarts = AllProducts,
                TotalPrice = totalPrice
            };
            return shoppingCartToItem;
        }

        public bool orderNow(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {

                var loggedInUser = this._userRepository.Get(userId);

                var userShoppingCart = loggedInUser.UserCart;

                EmailMessage message = new EmailMessage();
                message.MailTo = loggedInUser.Email;
                message.Subject = "Succesfully created order";
                message.Status = false;


                Order orderItem = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    User = loggedInUser
                };

                this._orderRepository.Insert(orderItem);

                List<ProductInOrder> productInOrders = new List<ProductInOrder>();

                var result = userShoppingCart.ProductInShoppingCarts
                    .Select(z => new ProductInOrder
                    {
                        OrderId = orderItem.Id,
                        ProductId = z.Product.Id, // product.id
                        SelectedProduct = z.Product,
                        UserOrder = orderItem,
                        Quantity = z.Quantity,
                    }).ToList();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Your order is completed. The order contains: ");

                var totalPrice = 0.0;

                for (int i = 1; i <= result.Count; i++)
                {
                    var item = result[i - 1];
                    totalPrice += item.Quantity * item.SelectedProduct.ProductPrice;
                    sb.AppendLine(i.ToString() + ". " + item.SelectedProduct.ProductName + " with price of: " + item.SelectedProduct.ProductPrice + " and quantity of: " + item.Quantity);
                }

                sb.AppendLine("Total Price: " + totalPrice.ToString());

                message.Content = sb.ToString();

                productInOrders.AddRange(result);

                foreach (var item in productInOrders)
                {
                    this._productInOrderRepository.Insert(item); 
                }

                loggedInUser.UserCart.ProductInShoppingCarts.Clear();

                this._mailRepository.Insert(message);

                this._userRepository.Update(loggedInUser);

                return true;
            }
            return false;
        }
    }
}
