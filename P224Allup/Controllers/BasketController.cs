using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using P224Allup.DAL;
using P224Allup.Models;
using P224Allup.ViewModels.Basket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P224Allup.Controllers
{
    public class BasketController : Controller
    {
        private readonly AllupDbContext _context;

        public BasketController(AllupDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            string cookieBasket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (cookieBasket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookieBasket);
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }

            foreach (BasketVM basketVM in basketVMs)
            {
                Product dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.ProductId);
                basketVM.Image = dbProduct.MainImage;
                basketVM.Price = dbProduct.DiscountPrice > 0 ? dbProduct.DiscountPrice : dbProduct.Price;
                basketVM.ExTax = dbProduct.ExTax;
                basketVM.Title = dbProduct.Title;
            }

            return View(basketVMs);
        }
       

        public async Task<IActionResult> Update(int? id, int? count)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            string cookieBasket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (cookieBasket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookieBasket);

                if (!basketVMs.Any(b => b.ProductId == id))
                {
                    return NotFound();
                }

                basketVMs.Find(b => b.ProductId == id).Count = (int)count;
            }
            else
            {
                return BadRequest();
            }

            cookieBasket = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", cookieBasket);

            foreach (BasketVM basketVM in basketVMs)
            {
                Product dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.ProductId);
                basketVM.Image = dbProduct.MainImage;
                basketVM.Price = dbProduct.DiscountPrice > 0 ? dbProduct.DiscountPrice : dbProduct.Price;
                basketVM.ExTax = dbProduct.ExTax;
                basketVM.Title = dbProduct.Title;
            }

            return PartialView("_BasketIndexPartial", basketVMs);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            if (!await _context.Products.AnyAsync(p => p.Id == id)) return NotFound();

            string cookieBasket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;
            if (cookieBasket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookieBasket);

                if (!basketVMs.Any(b => b.ProductId == id))
                {
                    return NotFound();
                }
                BasketVM basketVM = basketVMs.FirstOrDefault(b => b.ProductId == id);
                basketVMs.Remove(basketVM);
               
            }
            else
            {
                basketVMs = new List<BasketVM>();
            }
            cookieBasket = JsonConvert.SerializeObject(basketVMs);
            HttpContext.Response.Cookies.Append("basket", cookieBasket);
            foreach (BasketVM basketVM in basketVMs)
            {
                Product dbProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == basketVM.ProductId);
                basketVM.Image = dbProduct.MainImage;
                basketVM.Price = dbProduct.DiscountPrice > 0 ? dbProduct.DiscountPrice : dbProduct.Price;
                basketVM.ExTax = dbProduct.ExTax;
                basketVM.Title = dbProduct.Title;
            }
            return PartialView("_BasketIndexPartial", basketVMs);
        }
    }
}
