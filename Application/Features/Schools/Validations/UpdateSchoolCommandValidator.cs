using Application.Features.Schools.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Schools.Validations
{
    public class UpdateSchoolCommandValidator    :AbstractValidator<UpdateSchoolCommand>
    {
        public UpdateSchoolCommandValidator(ISchoolService schoolService) 
        { 
            RuleFor(command=>command.UpdateSchool)
                .SetValidator(new UpdateSchoolRequestValidator(schoolService));    

        }
    }
}
