using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Open_MediaServer.Utils;

namespace Open_MediaServer.Media
{
    public class MediaHandler
    {
        public async Task HandleRequestImg(HttpContext context)
        {
            var file = (string) context.Request.RouteValues["file"];
            var fileArray = MediaUtils.GetMedia(MediaType.Images, file);

            if (fileArray != null)
            {
                context.Response.Clear();
                context.Response.Headers.Add("Content-Disposition", $"attachment; filename={file}");
                context.Response.Headers.Add("Content-Length", fileArray.Length.ToString());
                context.Response.ContentType = new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType) ? contentType : "application/octet-stream";
                context.Response.StatusCode = StatusCodes.Status200OK;
                
                await context.Response.Body.WriteAsync(fileArray);
                return;
            }
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.Redirect("/404/");
        }

        public async Task HandleRequestVideo(HttpContext context)
        {
            var file = (string) context.Request.RouteValues["file"];
            var fileArray = MediaUtils.GetMedia(MediaType.Videos, file);

            if (fileArray != null)
            {
                context.Response.Clear();
                context.Response.Headers.Add("Content-Disposition", $"attachment; filename={file}");
                context.Response.Headers.Add("Content-Length", fileArray.Length.ToString());
                context.Response.ContentType = new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType) ? contentType : "application/octet-stream";
                context.Response.StatusCode = StatusCodes.Status200OK;
                
                await context.Response.Body.WriteAsync(fileArray);
                return;
            }
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.Redirect("/404/");
        }

        public async Task HandleRequestOther(HttpContext context)
        {
            var file = (string) context.Request.RouteValues["file"];
            var fileArray = MediaUtils.GetMedia(MediaType.Other, file);

            if (fileArray != null)
            {
                context.Response.Clear();
                context.Response.Headers.Add("Content-Disposition", $"attachment; filename={file}");
                context.Response.Headers.Add("Content-Length", fileArray.Length.ToString());
                context.Response.ContentType = new FileExtensionContentTypeProvider().TryGetContentType(file, out string contentType) ? contentType : "application/octet-stream";
                context.Response.StatusCode = StatusCodes.Status200OK;
                
                await context.Response.Body.WriteAsync(fileArray);
                return;
            }
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.Redirect("/404/");
        }
    }
}