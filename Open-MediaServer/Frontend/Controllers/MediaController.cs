using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Open_MediaServer.Backend.Schema;
using Open_MediaServer.Utils;

namespace Open_MediaServer.Frontend.Controllers;

[ApiController]
[Route("/api/frontend/[controller]")]
public class MediaController : Controller
{
    [HttpPost("/api/frontend/upload/")]
    public async Task<ActionResult> PostUploadContentForm()
    {
        if (ModelState.IsValid)
        {
            if (Request.Cookies["user_session"] == null || !UserUtils.IsAuthed(Request.Cookies["user_session"]))
            {
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var formFiles = HttpContext.Request.Form.Files.DistinctBy(file => file.FileName).ToList();
            if (formFiles.Count <= 0)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            for (int i = 0; i < formFiles.Count; i++)
            {
                var id = i;
                var file = formFiles[i];
                bool visible = true;
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);

                if (HttpContext.Request.Form.Count(form => form.Key.Contains("Name")) > formFiles.Count)
                {
                    id++;
                }

                if (HttpContext.Request.Form.ContainsKey($"Name {id}"))
                {
                    var name = HttpContext.Request.Form[$"Name {id}"];
                    fileName = Path.GetFileNameWithoutExtension(name);
                }

                if (HttpContext.Request.Form.ContainsKey($"Private {id}"))
                {
                    if (HttpContext.Request.Form[$"Private {id}"] == "on")
                        visible = false;
                }

                byte[] data = new byte[file.Length];
                if (file.Length > 0)
                {
                    using var memStream = new MemoryStream(data);
                    await file.CopyToAsync(memStream);
                }
                else
                {
                    continue;
                }

                if (fileName.Length > Program.ConfigManager.Config.UploadNameLimit)
                {
                    if (file.FileName.Length > Program.ConfigManager.Config.UploadNameLimit)
                    {
                        continue;
                    }

                    fileName = Path.GetFileNameWithoutExtension(file.FileName);
                }

                var safeFileName =
                    Uri.EscapeDataString(Uri.UnescapeDataString(fileName.Trim()));

                var upload = new MediaSchema.MediaUpload()
                {
                    Name = safeFileName,
                    Extension = fileExtension,
                    Content = data,
                    Public = visible
                };
                // await PostUploadContent(upload);
            }

            // if (HttpContext.Request.Form.ContainsKey("returnURL"))
            // {
            //     return RedirectToPage(HttpContext.Request.Form["returnURL"]);
            // }

            return StatusCode(StatusCodes.Status200OK);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }
}