using EShop.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EShop.Repository.Interface
{
    public interface IUserRepository
    {
        IEnumerable<EShopApplicationUser3> GetAll();
        EShopApplicationUser3 Get(string id);
        void Insert(EShopApplicationUser3 entity);
        void Update(EShopApplicationUser3 entity);
        void Delete(EShopApplicationUser3 entity);
    }
}
