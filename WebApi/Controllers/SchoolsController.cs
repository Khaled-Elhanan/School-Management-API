using Application.Features.Schools;
using Application.Features.Schools.Commands;
using Application.Features.Schools.Queries;
using Infrastructure.Constants;
using Infrastructure.Identity.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class SchoolsController : BaseApiController
    {
        [HttpPost("add")]
        [ShouldHavePermission(SchoolAction.Create, SchoolFeature.Schools)]
        public async Task<IActionResult> CreateSchoolAsync([FromBody] CreateSchoolRequest createSchoolCommand)
        {
            var response = await Sender.Send(new CreateSchoolCommand { CreateSchool = createSchoolCommand });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return BadRequest(response);

        }
        [HttpPut("update")]
        [ShouldHavePermission(SchoolAction.Update, SchoolFeature.Schools)]
        public async Task<IActionResult> UpdateSchoolAsync([FromBody] UpdateSchoolRequest updateSchoolCommand)
        {
            var response = await Sender.Send(new UpdateSchoolCommand { UpdateSchool = updateSchoolCommand });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpDelete("{schoolId}")]
        [ShouldHavePermission(SchoolAction.Delete, SchoolFeature.Schools)]
        public async Task<IActionResult> DeleteSchoolAsync(int schoolId)
        {
            var response = await Sender.Send(new DeleteSchoolCommand { SchoolId = schoolId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("by-id{schoolId}")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Schools)]
        public async Task<IActionResult> GetSchoolByIdAsync(int schoolId)

        {
            var response = await Sender.Send(new GetSchoolByIdQuery { SchoolId = schoolId });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }

        [HttpGet("by-name")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Schools)]
        public async Task<IActionResult> GetSchoolByNameAsync([FromQuery] string name)
        {
            var response = await Sender.Send(new GetSchoolByNameQuery { Name = name });
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
            return NotFound(response);
        }


        [HttpGet("all")]
        [ShouldHavePermission(SchoolAction.Read, SchoolFeature.Schools)]
        public async Task<IActionResult> GetAllSchoolsAsync()
        {
            var response = await Sender.Send(new GetSchoolsQuery());
            if (response.IsSuccessful)
            {
                return Ok(response);
            }
             return NotFound(response);

        }
       

    }
}
