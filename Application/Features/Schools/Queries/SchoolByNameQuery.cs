using Application.Wrappers;
using Mapster;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools.Queries
{
    public class SchoolByNameQuery : IRequest<IResponseWrapper>
    {
        public string Name { get; set; }
    }

    public class SchoolByNameQueryHandler : IRequestHandler<SchoolByNameQuery, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService;

        public SchoolByNameQueryHandler(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        public async Task<IResponseWrapper> Handle(SchoolByNameQuery request, CancellationToken cancellationToken)
        {
            var schoolInDb = await _schoolService.GetByNameAsync(request.Name);
            if (schoolInDb == null)
            {
                return await ResponseWrapper.FailAsync("School not found.");
            }
            return await ResponseWrapper<SchoolResponse>.SuccessAsync(data: schoolInDb.Adapt<SchoolResponse>(), "School retrieved successfully.");
        }
    }
}
