using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Identity.Models
{
    public class ApplicationRoleClaim :IdentityRoleClaim<string>
    {
        public string? Description { get; set; }
        public string? RoleName { get; set; }
        [NotMapped]
        public string ? Group { get; set; }
    }
}
