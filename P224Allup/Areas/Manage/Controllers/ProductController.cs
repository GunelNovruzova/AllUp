using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P224Allup.DAL;
using P224Allup.Extensions;
using P224Allup.Helpers;
using P224Allup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P224Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class ProductController : Controller
    {
        private readonly AllupDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AllupDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(bool? status, int page = 1)
        {
            ViewBag.Status = status;

            IEnumerable<Product> products = await _context.Products
                .Include(t => t.Brand)
                .Include(t => t.Category)
                .Include(t => t.ProductTags).ThenInclude(pt=>pt.Tag)
                .Where(t => status != null ? t.IsDeleted == status : !t.IsDeleted )
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)products.Count() / 5);

            return View(products.Skip((page - 1) * 5).Take(5));
        }

        public async Task<IActionResult> Detail(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductImages)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public async Task<IActionResult> Create(bool? status, int page = 1)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, bool? status, int page = 1)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (product.ProductImagesFile.Count() > 6)
            {
                ModelState.AddModelError("ProductImagesFile", $"maksimum yukleyebileceyin say 6");
                return View();
            }

            if (!await _context.Brands.AnyAsync(b=>b.Id == product.BrandId && !b.IsDeleted))
            {
                ModelState.AddModelError("BrandId", "Duzgun Brand Secin");
                return View();
            }

            if (!await _context.Categories.AnyAsync(b => b.Id == product.CategoryId && !b.IsDeleted))
            {
                ModelState.AddModelError("CategoryId", "Duzgun Category Secin ");
                return View();
            }

            if (product.TagIds.Count > 0)
            {
                List<ProductTag> productTags = new List<ProductTag>();

                foreach (int item in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(t=>t.Id != item && !t.IsDeleted))
                    {
                        ModelState.AddModelError("TagIds", $"Secilen Id {item} - li Tag Yanlisdir");
                        return View();
                    }

                    ProductTag productTag = new ProductTag
                    {
                        TagId = item
                    };

                    productTags.Add(productTag);
                }

                product.ProductTags = productTags;
            }

            if (product.MainImageFile != null)
            {
                if (!product.MainImageFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainImageFile", "Secilen Seklin Novu Uygun");
                    return View();
                }

                if (!product.MainImageFile.CheckFileSize(300))
                {
                    ModelState.AddModelError("MainImageFile", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                    return View();
                }

                product.MainImage = product.MainImageFile.CreateFile(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("MainImageFile", "Main Sekil Mutleq Secilmelidir");
                return View();
            }

            if (product.HoverImageFile != null)
            {
                if (!product.HoverImageFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverImageFile", "Secilen Seklin Novu Uygun");
                    return View();
                }

                if (!product.HoverImageFile.CheckFileSize(300))
                {
                    ModelState.AddModelError("HoverImageFile", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                    return View();
                }

                product.HoverImage = product.HoverImageFile.CreateFile(_env, "assets", "images", "product");
            }
            else
            {
                ModelState.AddModelError("HoverImageFile", "Hover Sekil Mutleq Secilmelidir");
                return View();
            }

            if (product.ProductImagesFile.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.ProductImagesFile)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("ProductImagesFile", "Secilen Seklin Novu Uygun");
                        return View();
                    }

                    if (!file.CheckFileSize(300))
                    {
                        ModelState.AddModelError("ProductImagesFile", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                        return View();
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = file.CreateFile(_env, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    };

                    productImages.Add(productImage);
                }

                product.ProductImages = productImages;
            }
            else
            {
                ModelState.AddModelError("ProductImagesFile", "Product Images File Sekil Mutleq Secilmelidir");
                return View();
            }

            product.Availability = product.Count > 0 ? true : false;
            product.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("index",new { status,page});
        }

        public async Task<IActionResult> Update(int? id, bool? status, int page = 1)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (id == null) return BadRequest();

            Product product = await _context.Products.Include(p=>p.ProductTags).ThenInclude(pt=>pt.Tag).Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            product.TagIds = product.ProductTags.Select(pt => pt.Tag.Id).ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Product product, bool? status, int page = 1)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (id == null) return BadRequest();

            if (id != product.Id) return BadRequest();

            Product dbProduct = await _context.Products
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (dbProduct == null) return NotFound();

            if (!ModelState.IsValid) return View(dbProduct);

            int canuploadimage = 6 - (int)(dbProduct.ProductImages?.Where(p=>!p.IsDeleted).Count());

            if (product.ProductImagesFile != null && product.ProductImagesFile.Length > canuploadimage)
            {
                ModelState.AddModelError("ProductImagesFile", $"maksimum yukleyebileceyin say {canuploadimage}");
                return View(dbProduct);
            }

            if (!await _context.Brands.AnyAsync(b => b.Id == product.BrandId && !b.IsDeleted))
            {
                ModelState.AddModelError("BrandId", "Duzgun Brand Secin");
                return View(product);
            }

            if (!await _context.Categories.AnyAsync(b => b.Id == product.CategoryId && !b.IsDeleted))
            {
                ModelState.AddModelError("CategoryId", "Duzgun Category Secin ");
                return View(product);
            }

            //if (product.TagIds.Count > 0)
            //{
            //    foreach (ProductTag productTag in dbProduct.ProductTags)
            //    {
            //        if (!product.TagIds.Any(t=>t == productTag.TagId))
            //        {
            //            _context.ProductTags.Remove(productTag);
            //        }
            //    }

            //    foreach (var item in product.TagIds)
            //    {
            //        if (!await _context.Tags.AnyAsync(t => t.Id != item && !t.IsDeleted))
            //        {
            //            ModelState.AddModelError("TagIds", $"Secilen Id {item} - li Tag Yanlisdir");
            //            return View();
            //        }

            //        if (!dbProduct.ProductTags.Any(t=>t.TagId == item))
            //        {
            //            ProductTag productTag = new ProductTag
            //            {
            //                ProductId = dbProduct.Id,
            //                TagId = item
            //            };

            //            dbProduct.ProductTags.Add(productTag);
            //        }
            //    }
            //}

            if (product.TagIds.Count > 0)
            {
                _context.ProductTags.RemoveRange(dbProduct.ProductTags);

                List<ProductTag> productTags = new List<ProductTag>();

                foreach (int item in product.TagIds)
                {
                    if (!await _context.Tags.AnyAsync(t => t.Id != item && !t.IsDeleted))
                    {
                        ModelState.AddModelError("TagIds", $"Secilen Id {item} - li Tag Yanlisdir");
                        return View(product);
                    }

                    ProductTag productTag = new ProductTag
                    {
                        TagId = item
                    };

                    productTags.Add(productTag);
                }

                dbProduct.ProductTags = productTags;
            }
            else
            {
                _context.ProductTags.RemoveRange(dbProduct.ProductTags);
            }

            if (product.MainImageFile != null)
            {
                if (!product.MainImageFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("MainImageFile", "Secilen Seklin Novu Uygun");
                    return View();
                }

                if (!product.MainImageFile.CheckFileSize(300))
                {
                    ModelState.AddModelError("MainImageFile", "Secilen Seklin Olcusu Maksimum 300 Kb Ola Biler");
                    return View();
                }
                Helper.DeleteFile(_env, dbProduct.MainImage, "assets", "images", "product");

                dbProduct.MainImage = product.MainImageFile.CreateFile(_env, "assets", "images", "product");
            }

            if (product.HoverImageFile != null)
            {
                if (!product.HoverImageFile.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("HoverImageFile", "Secilen Seklin Novu Uygun");
                    return View();
                }

                if (!product.HoverImageFile.CheckFileSize(300))
                {
                    ModelState.AddModelError("HoverImageFile", "Secilen Seklin Olcusu Maksimum 300 Kb Ola Biler");
                    return View();
                }
                Helper.DeleteFile(_env, dbProduct.HoverImage, "assets", "images", "product");

                dbProduct.HoverImage = product.HoverImageFile.CreateFile(_env, "assets", "images", "product");
            }

            if (product.ProductImagesFile != null && product.ProductImagesFile.Count() > 0)
            {
                List<ProductImage> productImages = new List<ProductImage>();

                foreach (IFormFile file in product.ProductImagesFile)
                {
                    if (!file.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("ProductImagesFile", "Secilen Seklin Novu Uygun");
                        return View();
                    }

                    if (!file.CheckFileSize(300))
                    {
                        ModelState.AddModelError("ProductImagesFile", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                        return View();
                    }

                    ProductImage productImage = new ProductImage
                    {
                        Image = file.CreateFile(_env, "assets", "images", "product"),
                        CreatedAt = DateTime.UtcNow.AddHours(4)
                    };

                    dbProduct.ProductImages.Add(productImage);
                }
            }

            dbProduct.BrandId = product.BrandId;
            dbProduct.CategoryId = product.CategoryId;
            dbProduct.Title = product.Title;
            dbProduct.Price = product.Price;
            dbProduct.DiscountPrice = product.DiscountPrice;
            dbProduct.ExTax = product.ExTax;
            dbProduct.IsFeatured = product.IsFeatured;
            dbProduct.IsNewArrival = product.IsNewArrival;
            dbProduct.IsBestseller = product.IsBestseller;
            dbProduct.Description = product.Description;
            dbProduct.Count = product.Count;
            dbProduct.Availability = product.Count > 0 ? true : false;
            dbProduct.UpdatedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction("index", new { status, page });
        }

        public async Task<IActionResult> DeleteImage(int? id)
        {
            ViewBag.Brands = await _context.Brands.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Categories = await _context.Categories.Where(b => !b.IsDeleted).ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.IsDeleted).ToListAsync();

            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p=>p.ProductTags).ThenInclude(pt=>pt.Tag)
                .FirstOrDefaultAsync(p => p.ProductImages.Any(pi => pi.Id == id && !pi.IsDeleted));

            if (product == null) return NotFound();

            product.ProductImages.FirstOrDefault(p => p.Id == id).IsDeleted = true;
            product.ProductImages.FirstOrDefault(p => p.Id == id).DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            product.TagIds = product.ProductTags.Select(pt => pt.Tag.Id).ToList();

            return PartialView("_ProductUpdateImagePartial",product);
        }

        public async Task<IActionResult> Delete(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductImages)
                .Include(p=>p.Brand)
                .Include(p=>p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            return View(product);
        }
        public async Task<IActionResult> DeleteProduct(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Product product = await _context.Products
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductImages)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            return RedirectToAction("index",new { status,page});
        }
    }
}
