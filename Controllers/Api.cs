using Microsoft.AspNetCore.Mvc;
using replace_and_execute.Types;
using System.Diagnostics;
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
        /// ExecudeCommand
        /// </summary>
        /// <param name="commands">Commands</param>
        private static List<String> ExecuteCommand(String[] commands, String cwd)
        {
            List<String> outputs = [];

            outputs.Add("Commands Execution Start");
            var process = Process.Start(new ProcessStartInfo(commands[0], commands[1] ?? "") { RedirectStandardOutput = true, WorkingDirectory = cwd });
            if (process == null)
            {
                Console.WriteLine("Commands Execution Failed");
            }
            else
            {
                using (var reader = process.StandardOutput)
                {
                    while (!reader.EndOfStream)
                    {
                        Console.WriteLine(reader.ReadLine());
                    }
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                Console.WriteLine($"Commands Execution Failed with Code: {process.ExitCode}");
            }
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
            var module = configuration.GetSection("modules").Get<List<Module>>()?.Find((a) => a.Name == name);
            if (module != null)
            {
                if (module.Pre.Length > 0)
                {
                    ExecuteCommand(module.Pre, module.Path);
                }

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
                        var entryDestinationPath = Path.Combine(module.Path, entry.FullName);
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

                if (module.Post.Length > 0)
                {
                    ExecuteCommand(module.Post, module.Path);
                }

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
