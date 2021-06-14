using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CooliosoteaFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

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
            //browse items by specific categories
            ViewBag.Category = category;
            var products = _context.Product.Where(p => p.Category.Name == category).OrderBy(p => p.Name).ToList();
            return View(products);
        }
        //getting product details 
        public IActionResult ProductDetails(string product)
        {
            //use singleordefault to find matches
            var selectedProduct = _context.Product.SingleOrDefault(p => p.Name == product);
            return View(selectedProduct);

        }
        //adding to cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int Quantity, int ProductId)
        {
            //get price
            var product = _context.Product.SingleOrDefault(p => p.ProductId == ProductId);
            var price = product.Price;
            //try to save item into the cart
            //get username
            var cartUsername = GetCartUserName();
            var cart = new Cart
            {
                ProductId = ProductId,
                Quantity = Quantity,
                Price = price,
                Username = cartUsername
            };
            _context.Cart.Add(cart);
            _context.SaveChanges();

            //show what they have in the cart

            return RedirectToAction("cart");
        }

        private string GetCartUserName()
        {
            //see if username is stored in their session
            if(HttpContext.Session.GetString("CartUsername") ==null)
            {
                var cartUsername = "";
                //check if user is logged in. if they are use email for their variable
                if(User.Identity.IsAuthenticated)
                {
                    cartUsername = User.Identity.Name;
                }
                else
                {
                    //create a new id for user if not logged in w email
                    cartUsername = Guid.NewGuid().ToString();
                }
                HttpContext.Session.SetString("CartUserName", cartUsername);
            }
            return HttpContext.Session.GetString("CartUserName");


        }
        public IActionResult Cart()
        {
            //find out who is the user
            var cartUsername = GetCartUserName();
            //Query DB To get their added items from cart
            var cartItems = _context.Cart.Include(c => c.Product).Where(c=> c.Username == cartUsername).ToList();
            //pass view to pass items to be shown to cust
            return View(cartItems);
        }

        public IActionResult RemoveFromCart(int id)
        {
            //select what they want to delete
            var cartItem = _context.Cart.SingleOrDefault(c => c.CartId == id);

            //delete the item
            _context.Cart.Remove(cartItem);
            _context.SaveChanges();
            //shows cart again refreshed
            return RedirectToAction("Cart");
        }
    }
}
