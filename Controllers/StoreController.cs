using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CooliosoteaFinal.Models;
using System.Threading.Tasks;

namespace CooliosoteaFinal.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            //list of products
            var products = new List<Product>();
            for(var i=1;i<=10; i++)
            {
                products.Add(new Product { Name = "Product" + i.ToString() });
            }
            return View(products);
        }

        public IActionResult Details(string product)
        {
            //Store selected product name in ViewBag to the view that has corresponding drink
            ViewBag.product = product;
            return View();
        }
    }
}
