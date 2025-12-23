using Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools.Commands
{
    public class DeleteSchoolCommand   :IRequest<IResponseWrapper>
    {
        public int SchoolId { get; set; }
    }
    public class DeleteSchoolCommandHandler : IRequestHandler<DeleteSchoolCommand, IResponseWrapper>
    {
        private readonly ISchoolService _schoolService;
        public DeleteSchoolCommandHandler(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }
        public async Task<IResponseWrapper> Handle(DeleteSchoolCommand request, CancellationToken cancellationToken)
        {
            var schoolInDb = await _schoolService.GetByIdAsync(request.SchoolId);
            if (schoolInDb == null)
            {
                return await ResponseWrapper.FailAsync("School not found.");
            }
            var deletedScoolId =  await _schoolService.DeleteAsync(schoolInDb);
            return await ResponseWrapper<int>.SuccessAsync(data: deletedScoolId ,  "School deleted successfully.");
        }
    }
}
