using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CooliosoteaFinal.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace CooliosoteaFinal.Controllers
{
    public class ShopController : Controller
    {
        //adding db connection
        private readonly CooliosoteaContext _context;
        //add configuration so controller can read values in appsetting.json
        private IConfiguration _configuration;
        public ShopController(CooliosoteaContext context, IConfiguration configuration)
        {

            _context = context;
            //accept instance of config obj so we can read appsetting json
            _configuration = configuration;
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

            //see if product already exists in the cart so can update quantity
            var cartItem = _context.Cart.SingleOrDefault(c => c.ProductId == ProductId && c.Username == cartUsername);
            if (cartItem == null)
            {


                var cart = new Cart
                {
                    ProductId = ProductId,
                    Quantity = Quantity,
                    Price = price,
                    Username = cartUsername
                };
                _context.Cart.Add(cart);
            }
            else
            {
                cartItem.Quantity += Quantity; //add the new quantity to existing quantity
                _context.Update(cartItem);
            }
                _context.SaveChanges();
            

            //show what they have in the cart

            return RedirectToAction("Cart");
        }

        private string GetCartUserName()
        {
            //see if username is stored in their session
            if (HttpContext.Session.GetString("CartUsername") == null)
            {
                var cartUsername = "";
                //check if user is logged in. if they are use email for their variable
                if (User.Identity.IsAuthenticated)
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
            var cartItems = _context.Cart.Include(c => c.Product).Where(c => c.Username == cartUsername).ToList();
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
        [Authorize]
        public IActionResult Checkout()
        {
            //check if they have bene shopping as anon now that they are logged in
            MigrateCart();
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //get all the regis info into our order model
        public IActionResult Checkout([Bind("FirstName, LastName, Address, City, Province, PostalCode, Phone")] Models.Order order)
        {
            //date, user and total will be auto generated instead of user inputting
            order.OrderDate = DateTime.Now;
            order.UserId = User.Identity.Name;
            var cartItems = _context.Cart.Where(c => c.Username == User.Identity.Name);
            decimal cartTotal = (from c in cartItems
                                 select c.Quantity * c.Price).Sum();

            order.Total = cartTotal;
            //need sessionextension to store complex object
            HttpContext.Session.SetObject("Order", order);

            HttpContext.Session.SetString("cartTotal", cartTotal.ToString());

            return RedirectToAction("Payment");

        }
        private void MigrateCart()
        {
            //if they originally did not have acc add their items to their newly created username
            if (HttpContext.Session.GetString("CartUsername") != User.Identity.Name)
            {
                var cartUsername = HttpContext.Session.GetString("CartUsername");
                

                //get the items
                var cartItems = _context.Cart.Where(c => c.Username == cartUsername);
                //get all cart items thru loop and uodate username for each associated item
                foreach (var item in cartItems)
                {
                    item.Username = User.Identity.Name;
                    _context.Update(item);
                }
                _context.SaveChanges();
                //update session from guid to user email
                HttpContext.Session.SetString("CartUsername", User.Identity.Name);
            }
        }

        public IActionResult Payment()
        {
            //get order from session variable and cast as order obj
            var order = HttpContext.Session.GetObject<Models.Order>("Order");
            //display total and pass amount to Stripe Api
            ViewBag.Total = order.Total;
            //convert total into cents only for stripe since takes it in that value
            ViewBag.CentsTotal = order.Total * 100;
            ViewBag.PublishableKey = _configuration.GetSection("Stripe")["PublishableKey"];
            return View();

            //this is for authorizing the payment but we haven't really gotten their info yet
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken] //good practice
        public IActionResult Payment(string stripeEmail, string stripeToken)
        {
            //send payment to stripe and then generate and save order
            StripeConfiguration.ApiKey = _configuration.GetSection("Stripe")["SecretKey"];
            var cartUsername = HttpContext.Session.GetString("CartUsername");
            var cartItems = _context.Cart.Where(c => c.Username == cartUsername);
            var order=HttpContext.Session.GetObject<Order
            //save the order details, get rid of the card so it refreshes
            //confirm with total amount shown as a receipt

            return RedirectToAction("Details", "Orders");

        }
    }
}
