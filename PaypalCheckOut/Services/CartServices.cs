using PaypalCheckOut.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service
{
    public interface ICartServices
    {
        IEnumerable<ProductModel> GetCartItems();
        double GetTotalToPay();
    }

    public class CartServices : ICartServices
    {
        public List<ProductModel> product_list;

        public CartServices()
        {
            product_list = new List<ProductModel> {

                new ProductModel{
                 Id=1,
                 Name = "Portatil Acer i7 10548G 16 Gb RAM",
                 Price = 950,
                 Quantity = 1
                },
                 new ProductModel{
                 Id=2,
                 Name = "LG Monitor 22\"",
                 Price = 150,
                 Quantity = 2
                }
            };
        }

        public IEnumerable<ProductModel> GetCartItems()
        {
            return product_list;
        }

        public double GetTotalToPay()
        {
            return product_list.Sum(a => a.Price * a.Quantity);
        }
    }
}
