using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P224Allup.DAL;
using P224Allup.Extensions;
using P224Allup.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P224Allup.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BrandController : Controller
    {
        private readonly AllupDbContext _context;

        public BrandController(AllupDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(bool? status, int page = 1)
        {
            ViewBag.Status = status;

            IEnumerable<Brand> brands = await _context.Brands
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)brands.Count() / 5);

            return View(brands.Skip((page - 1) * 5).Take(5));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                ModelState.AddModelError("Name", "Bosluq Olmamalidir");
                return View();
            }

            //tag.Name = tag.Name.Trim();

            if (brand.Name.CheckString())
            {
                ModelState.AddModelError("Name", "Yalniz Herf Ola Biler");
                return View();
            }

            if (await _context.Brands.AnyAsync(t => t.Name.ToLower() == brand.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Alreade Exists");
                return View();
            }

            brand.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Brand brand = await _context.Brands.FirstOrDefaultAsync(t => t.Id == id);

            if (brand == null) return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Brand brand, bool? status, int page = 1)
        {
            if (!ModelState.IsValid) return View(brand);

            if (id == null) return BadRequest();

            if (id != brand.Id) return BadRequest();

            Brand dbBrand = await _context.Brands.FirstOrDefaultAsync(t => t.Id == id);

            if (dbBrand == null) return NotFound();

            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                ModelState.AddModelError("Name", "Bosluq Olmamalidir");
                return View(brand);
            }

            if (brand.Name.CheckString())
            {
                ModelState.AddModelError("Name", "Yalniz Herf Ola Biler");
                return View(brand);
            }

            if (await _context.Brands.AnyAsync(t => t.Id != brand.Id && t.Name.ToLower() == brand.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Alreade Exists");
                return View(brand);
            }

            dbBrand.Name = brand.Name;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { status = status, page = page });
        }

        public async Task<IActionResult> Delete(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Brand dbBrand = await _context.Brands.FirstOrDefaultAsync(t => t.Id == id);

            if (dbBrand == null) return NotFound();

            dbBrand.IsDeleted = true;
            dbBrand.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            ViewBag.Status = status;

            IEnumerable<Brand> brands = await _context.Brands
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)brands.Count() / 5);



            return PartialView("_BrandIndexPartial", brands.Skip((page - 1) * 5).Take(5));
        }

        public async Task<IActionResult> Restore(int? id, bool? status, int page = 1)
        {
            if (id == null) return BadRequest();

            Brand dbBrand = await _context.Brands.FirstOrDefaultAsync(t => t.Id == id);

            if (dbBrand == null) return NotFound();

            dbBrand.IsDeleted = false;

            await _context.SaveChangesAsync();

            ViewBag.Status = status;

            IEnumerable<Brand> brands = await _context.Brands
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)brands.Count() / 5);



            return PartialView("_BrandIndexPartial", brands.Skip((page - 1) * 5).Take(5));
        }
    }
}
