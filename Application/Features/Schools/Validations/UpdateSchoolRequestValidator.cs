using Application.Features.Schools.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools.Validations
{
    internal class UpdateSchoolRequestValidator : AbstractValidator<UpdateSchoolRequest>
    {
        public UpdateSchoolRequestValidator(ISchoolService schoolService)
        {
            RuleFor(request=>request.Id)
                .NotEmpty().WithMessage("School Id is required.")
                .MustAsync(async (id, cancellationToken) =>
                {
                    var schoolInDb = await schoolService.GetByIdAsync(id);
                    return schoolInDb != null;
                }).WithMessage("School with the specified Id does not exist.");          

            RuleFor(request => request.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Name is required.");

            RuleFor(request => request.EstablishedDate)
                .NotEmpty().WithMessage("Established Date is required.");
        }
    }
}
