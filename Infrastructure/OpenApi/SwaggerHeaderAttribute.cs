using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenApi
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerHeaderAttribute : Attribute
    {
        public SwaggerHeaderAttribute(
            string headerName,
            string description,
            string defaultValue,
            bool isRequired = true)
        {
            HeaderName = headerName;
            Description = description;
            DefaultValue = defaultValue;
            IsRequired = isRequired;
        }

        public string HeaderName { get; }
        public string Description { get; }
        public string DefaultValue { get; }
        public bool IsRequired { get; }
    }


}
