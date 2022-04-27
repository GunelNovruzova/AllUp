using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P224Allup.DAL;
using P224Allup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P224Allup.ViewModels.Basket;
using Newtonsoft.Json;

namespace P224Allup.Controllers
{
    public class ProductController : Controller
    {
        private readonly AllupDbContext _context;

        public ProductController(AllupDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DetailModal(int? id)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.Include(p=>p.ProductImages).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            return PartialView("_ProductDetailPartial",product);
        }

        public async Task<IActionResult> AddBasket(int? id, int count = 1)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            string cookieBasket = HttpContext.Request.Cookies["basket"];

            List<BasketVM> basketVMs = null;

            if (cookieBasket != null)
            {
                basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookieBasket);

                if (basketVMs.Any(b=>b.ProductId == id))
                {
                    basketVMs.Find(b => b.ProductId == id).Count += count;
                }
                else
                {
                    #region old
                    //BasketVM basketVM = new BasketVM
                    //{
                    //    ProductId = (int)id,
                    //    Count = count
                    //};

                    //basketVMs.Add(basketVM);
                    #endregion

                    basketVMs.Add(new BasketVM
                    {
                        ProductId = (int)id,
                        Count = count
                    });
                }
            }
            else
            {
                basketVMs = new List<BasketVM>();
                basketVMs.Add(new BasketVM
                {
                    ProductId = (int)id,
                    Count = count
                });
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

            return PartialView("_BasketPartial",basketVMs);
        }
    }
}
