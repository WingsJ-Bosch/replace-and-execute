using Microsoft.AspNetCore.Mvc;
using replace_and_execute.Types;
using System.IO.Compression;

namespace replace_and_execute.Controllers
{
    /// <summary>
    /// API
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class Api(IConfiguration configuration) : ControllerBase
    {
        /// <summary>
        /// Get Modules
        /// </summary>
        /// <returns>Modules</returns>
        [HttpGet("Modules")]
        public ActionResult<List<Module>> GetModules()
        {
            var modules = GetModuleList();

            return Ok(modules);
        }
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="file">File</param>
        /// <returns>Action Result</returns>
        [HttpPost("Update")]
        public async Task<ActionResult> PostUpdate([FromForm] string name, [FromForm] IFormFile file)
        {
            var modules = GetModuleList();
            var target = modules.Find((a) => a.Name == name);
            if (target != null)
            {
                var tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);
                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                };

                try
                {
                    using var archive = ZipFile.OpenRead(tempFilePath);
                    foreach (var entry in archive.Entries)
                    {
                        var entryDestinationPath = Path.Combine(target.Path, entry.FullName);
                        if (entry.FullName.EndsWith('/'))
                        {
                            Directory.CreateDirectory(entryDestinationPath);
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(entryDestinationPath)!);
                            using var entryStream = entry.Open();
                            using var fileStream = new FileStream(entryDestinationPath, FileMode.Create);
                            await entryStream.CopyToAsync(fileStream);
                        }
                    }
                }
                finally
                {
                    if (System.IO.File.Exists(tempFilePath))
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }

                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        /// <summary>
        /// Get Module List
        /// </summary>
        /// <returns>Modules</returns>
        private List<Module> GetModuleList()
        {
            return configuration.GetSection("modules").Get<List<Module>>() ?? [];
        }
    }
}
