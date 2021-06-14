using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CooliosoteaFinal.Models;

namespace CooliosoteaFinal.Controllers
{
    public class ShopController : Controller
    {
        //adding db connection
        private readonly CooliosoteaContext _context;

        public ShopController(CooliosoteaContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            //return categories to customer
            var categories = _context.Category.OrderBy(c => c.Name).ToList();
            return View(categories);
        }

        public IActionResult Browse(string category)
        {
            ViewBag.Category = category;
            var products = _context.Product.Where(p => p.Category.Name == category).OrderBy(p => p.Name).ToList();
            return View(products);
        }

    }
}
