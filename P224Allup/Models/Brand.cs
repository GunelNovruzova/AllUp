using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace P224Allup.Models
{
    public class Brand : BaseEntity
    {
        [StringLength(255), Required]
        public string Name { get; set; }

        public IEnumerable<Product> Products { get; set; }
    }
}
