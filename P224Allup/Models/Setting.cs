using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace P224Allup.Models
{
    public class Setting : BaseEntity
    {
        [StringLength(255)]
        public string Logo { get; set; }
        [Required]
        public string Offer { get; set; }
        [StringLength(255),Required,EmailAddress]
        public string Email { get; set; }
        [StringLength(255),Required]
        public string SupportPhone { get; set; }
        [StringLength(255),Required]
        public string Phone { get; set; }
        [StringLength(255),Required]
        public string Address { get; set; }
        [StringLength(255),Required]
        public string WorkDay { get; set; }

        [NotMapped]
        public IFormFile LogoImage { get; set; }
    }
}
