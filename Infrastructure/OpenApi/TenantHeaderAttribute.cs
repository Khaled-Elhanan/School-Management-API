using Infrastructure.Tenacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenApi
{
    public class TenantHeaderAttribute() : SwaggerHeaderAttribute(TenancyConstants.TenantIdName,
        description:"Enter your tenant name to access this API.",
        defaultValue: string.Empty,
        isRequired:true)
    {
        
    }
}
