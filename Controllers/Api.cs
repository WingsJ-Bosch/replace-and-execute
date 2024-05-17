using Microsoft.AspNetCore.Mvc;

namespace replace_and_execute.Controllers
{
    /// <summary>
    /// API
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class Api(IConfiguration configuration) : ControllerBase
    {
        public IConfiguration Configuration = configuration;

        [HttpPost("Update")]
        public ActionResult Update([FromForm] string name, [FromForm] IFormFile file)
        {
            var modules = configuration.GetValue<dynamic>("modules");

            return Ok();
        }
    }
}
