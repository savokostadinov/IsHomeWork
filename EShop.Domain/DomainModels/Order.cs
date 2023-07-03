using EShop.Domain.Identity;
using System;
using System.Collections;
using System.Collections.Generic;

namespace EShop.Domain.DomainModels
{
    public class Order : BaseEntity
    {

        public String UserId { get; set; }

        public EShopApplicationUser3 User { get; set; }

        public virtual ICollection<ProductInOrder> ProductsInOrder { get; set; }
    }
}
