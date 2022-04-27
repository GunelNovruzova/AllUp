using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P224Allup.Models;

namespace P224Allup.DAL
{
    public class AllupDbContext : DbContext
    {
        public AllupDbContext(DbContextOptions<AllupDbContext> options) : base(options){ }

        public DbSet<Setting> Settings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
    }
}
