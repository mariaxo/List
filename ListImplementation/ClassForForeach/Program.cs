using System;

namespace ClassForForeach
{
    class Program
    {
        static void Main(string[] args)
        {
            Product[] products = new Product[]
            {
                new Product() { serialNumber = 111, Description = "pr1" },
                new Product() { serialNumber = 112, Description = "pr2" },
                new Product() { serialNumber = 113, Description = "pr3" },
                new Product() { serialNumber = 114, Description = "pr4" }
            };

            Factory f = new Factory(products);
            foreach (Product p in f)
            {
                Console.WriteLine(p.serialNumber);
            }
        }
    }
}
