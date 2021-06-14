using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CooliosoteaFinal.Controllers
{
    public class StoreController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(string product)
        {
            //Store selected product name in ViewBag to the view that has corresponding drink
            ViewBag.product = product;
            return View();
        }
    }
}
