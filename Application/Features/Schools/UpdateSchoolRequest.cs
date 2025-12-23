using Application.Features.Schools.Commands;
using Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools
{
    public class UpdateSchoolRequest
    {
        public UpdateSchoolRequest UpdateSchool{ get; set; }
    }          

    public class UpdateSchoolCommandHandler : IRequestHandler<UpdateSchoolCommand, IResponseWrapper>
    {
                   private readonly ISchoolService _schoolService;
        public UpdateSchoolCommandHandler(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }

        public async Task<IResponseWrapper> Handle(UpdateSchoolCommand request, CancellationToken cancellationToken)
        {
            var schoolInDb = await _schoolService.GetByIdAsync(request.Id); 
            if (schoolInDb == null)
            {
                return await ResponseWrapper.FailAsync("School not found.");
            }
            schoolInDb.Address = request.Address;
            schoolInDb.Name = request.Name;
            var updatedSchoolId = await _schoolService.UpdateAsync(schoolInDb);
            return await ResponseWrapper<int>.SuccessAsync(data: updatedSchoolId,"School updated successfuly ");
        }
    }

}

