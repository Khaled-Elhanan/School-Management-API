using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenApi
{
    public class SwaggerSettings
    {
        public string Title { get; set; } = "API Documentation";

        public string Description { get; set; } 
        public string ContactName { get; set; } = "API Support";
        public string ContactEmail { get; set; } 
        public string ContactUrl { get; set; }

        public string LicenseName { get; set; } = "MIT License";

        public string LicenseUrl { get; set; } = "https://opensource.org/licenses/MIT";


    }
}
