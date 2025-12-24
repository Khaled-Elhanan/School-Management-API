using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools.Validations
{
    public class CreateSchoolRequestValidator : AbstractValidator<CreateSchoolRequest>
    {
        public CreateSchoolRequestValidator()
        {
            RuleFor(request=>request.Name)
                .NotEmpty().WithMessage("School name is required.")
                .MaximumLength(60).WithMessage("School name must not exceed 100 characters.");
            RuleFor(request => request.Address)
                .NotEmpty().WithMessage("School address is required.")
                .MaximumLength(200).WithMessage("School address must not exceed 200 characters.");
            RuleFor(CreateSchoolRequest=> CreateSchoolRequest.EstablishedDate)
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Established date cannot be in the future.");
        }
    }
}
