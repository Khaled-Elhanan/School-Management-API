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
    public class GetSchoolsQuery:IRequest<IResponseWrapper>
    {

    }
    public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService;
        public GetSchoolsQueryHandler(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }
        public async Task<IResponseWrapper> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
        {
            var schoolsInDb = await _schoolService.GetAllAsync();
            if ((schoolsInDb?.Count > 0))
            {
                return await ResponseWrapper<List<SchoolResponse>>.SuccessAsync(data: schoolsInDb.Adapt<List<SchoolResponse>>(), "Schools retrieved successfully.");

            }
            return await ResponseWrapper.FailAsync("No schools found.");
        }
    }
}
