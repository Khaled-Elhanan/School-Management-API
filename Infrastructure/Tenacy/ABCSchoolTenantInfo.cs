using Finbuckle.MultiTenant.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Tenacy
{
    public class ABCSchoolTenantInfo : ITenantInfo
    {
        public string? Id { get; set; }
        public string? Identifier { get; set; }
        public string? Name { get; set; }

        public string? ConnectionString { get; set; }

        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName  { get; set; }

        public DateTime ValidUpTo { get; set; }
        public bool IsActive { get; set; }

    }
}
