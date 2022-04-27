using Microsoft.AspNetCore.Hosting;
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
    public class CategoryController : Controller
    {
        private readonly AllupDbContext _context;
        private readonly IWebHostEnvironment _env;


        public CategoryController(AllupDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(bool? status, bool? isMainRoute, int page = 1)
        {
            ViewBag.Status = status;
            ViewBag.IsMain = isMainRoute;

            IEnumerable<Category> categories = await _context.Categories
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true && isMainRoute != null ? t.IsMain == isMainRoute : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)categories.Count() / 5);

            return View(categories.Skip((page - 1) * 5).Take(5));
        }

        public async Task<IActionResult> Create(bool? status, bool? isMainRoute, int page = 1)
        {
            ViewBag.MainCategory = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category, bool? status, bool? isMainRoute, int page = 1)
        {
            ViewBag.MainCategory = await _context.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Bosluq Olmamalidir");
                return View();
            }

            //tag.Name = tag.Name.Trim();

            if (category.Name.CheckString())
            {
                ModelState.AddModelError("Name", "Yalniz Herf Ola Biler");
                return View();
            }

            if (await _context.Categories.AnyAsync(t => t.Name.ToLower() == category.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Alreade Exists");
                return View();
            }

            if (category.IsMain)
            {
                category.ParentId = null;

                if (category.CategoryImage == null)
                {
                    ModelState.AddModelError("CategoryImage", "Sekil Mutleq Olmalidi");
                    return View();
                }

                if (!category.CategoryImage.CheckFileContentType("image/jpeg"))
                {
                    ModelState.AddModelError("LogoImage", "Secilen Seklin Novu Uygun");
                    return View();
                }

                if (!category.CategoryImage.CheckFileSize(30))
                {
                    ModelState.AddModelError("LogoImage", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                    return View();
                }

                category.Image = category.CategoryImage.CreateFile(_env, "assets", "images");
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Parent Mutleq Secilmelidir");
                    return View();
                }

                if (!await _context.Categories.AnyAsync(c=>c.Id == category.ParentId && !c.IsDeleted && c.IsMain))
                {
                    ModelState.AddModelError("ParentId", "Parent Id yanlisdir");
                    return View();
                }
            }

            category.CreatedAt = DateTime.UtcNow.AddHours(4);

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { status = status, isMainRoute = isMainRoute, page = page });
        }

        public async Task<IActionResult> Update(int? id, bool? status, bool? isMainRoute, int page = 1)
        {
            if (id == null) return BadRequest();

            Category category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            ViewBag.MainCategory = await _context.Categories.Where(c => c.Id != id && c.IsMain && !c.IsDeleted).ToListAsync();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, Category category, bool? status, bool? isMainRoute, int page = 1)
        {
            ViewBag.MainCategory = await _context.Categories.Where(c => c.Id != id && c.IsMain && !c.IsDeleted).ToListAsync();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (dbCategory == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(dbCategory);
            }

            if (id != category.Id) return BadRequest();

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                ModelState.AddModelError("Name", "Bosluq Olmamalidir");
                return View(dbCategory);
            }

            //tag.Name = tag.Name.Trim();

            if (category.Name.CheckString())
            {
                ModelState.AddModelError("Name", "Yalniz Herf Ola Biler");
                return View(dbCategory);
            }

            if (await _context.Categories.AnyAsync(t => t.Id != id && t.Name.ToLower() == category.Name.ToLower()))
            {
                ModelState.AddModelError("Name", "Alreade Exists");
                return View(dbCategory);
            }

            if (category.ParentId != null && id == category.ParentId)
            {
                ModelState.AddModelError("ParentId", "Duzun Parent Sec");
                return View(category);
            }

            if (category.IsMain)
            {
                if (!dbCategory.IsMain && category.CategoryImage == null)
                {
                    ModelState.AddModelError("CategoryImage", "Sekil Mutleq Olmalidi");
                    return View(dbCategory);
                }

                if (category.CategoryImage != null )
                {
                    if (!category.CategoryImage.CheckFileContentType("image/jpeg"))
                    {
                        ModelState.AddModelError("LogoImage", "Secilen Seklin Novu Uygun");
                        return View();
                    }

                    if (!category.CategoryImage.CheckFileSize(30))
                    {
                        ModelState.AddModelError("LogoImage", "Secilen Seklin Olcusu Maksimum 30 Kb Ola Biler");
                        return View();
                    }

                    if (dbCategory.Image != null)
                    {
                        Helper.DeleteFile(_env, dbCategory.Image, "assets", "images");
                    }

                    dbCategory.Image = category.CategoryImage.CreateFile(_env, "assets", "images");
                }

                dbCategory.ParentId = null;
            }
            else
            {
                if (category.ParentId == null)
                {
                    ModelState.AddModelError("ParentId", "Parent Mutleq Secilmelidir");
                    return View(dbCategory);
                }

                if (!await _context.Categories.AnyAsync(c => c.Id == category.ParentId && !c.IsDeleted && c.IsMain))
                {
                    ModelState.AddModelError("ParentId", "Parent Id yanlisdir");
                    return View(dbCategory);
                }

                dbCategory.ParentId = category.ParentId;
                Helper.DeleteFile(_env, dbCategory.Image, "assets", "images");
                dbCategory.Image = null;
            }

            dbCategory.IsMain = category.IsMain;
            dbCategory.Name = category.Name;
            dbCategory.UpdatedAt = DateTime.UtcNow.AddHours(4);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { status = status, isMainRoute = isMainRoute, page = page });
        }

        public async Task<IActionResult> Delete(int? id, bool? status, bool? isMainRoute, int page = 1)
        {
            if (id == null) return BadRequest();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(t => t.Id == id);

            if (dbCategory == null) return NotFound();

            dbCategory.IsDeleted = true;
            dbCategory.DeletedAt = DateTime.UtcNow.AddHours(4);

            await _context.SaveChangesAsync();

            ViewBag.Status = status;
            ViewBag.IsMain = isMainRoute;

            IEnumerable<Category> categories = await _context.Categories
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true && isMainRoute != null ? t.IsMain == isMainRoute : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)categories.Count() / 5);

            return PartialView("_CategoryIndexPartial", categories.Skip((page - 1) * 5).Take(5));
        }

        public async Task<IActionResult> Restore(int? id, bool? status, bool? isMainRoute, int page = 1)
        {
            if (id == null) return BadRequest();

            Category dbCategory = await _context.Categories.FirstOrDefaultAsync(t => t.Id == id);

            if (dbCategory == null) return NotFound();

            dbCategory.IsDeleted = false;

            await _context.SaveChangesAsync();

            ViewBag.Status = status;
            ViewBag.IsMain = isMainRoute;

            IEnumerable<Category> categories = await _context.Categories
                .Include(t => t.Products)
                .Where(t => status != null ? t.IsDeleted == status : true && isMainRoute != null ? t.IsMain == isMainRoute : true)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewBag.PageIndex = page;
            ViewBag.PageCount = Math.Ceiling((double)categories.Count() / 5);

            return PartialView("_CategoryIndexPartial", categories.Skip((page - 1) * 5).Take(5));
        }
    }
}
