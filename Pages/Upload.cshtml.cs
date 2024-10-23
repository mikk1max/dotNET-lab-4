using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class UploadModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }
        private string imagesDir;
        private string watermarkPath;

        public UploadModel(IWebHostEnvironment environment)
        {
            imagesDir = Path.Combine(environment.WebRootPath, "images");
            watermarkPath = Path.Combine(environment.WebRootPath, "watermark.png");
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (Upload != null)
            {
                string extension = ".jpg";
                switch (Upload.ContentType)
                {
                    case "image/jpeg":
                        extension = ".jpg";
                        break;
                    case "image/png":
                        extension = ".png";
                        break;
                    case "image/gif":
                        extension = ".gif";
                        break;
                }
                var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + extension;
                var filePath = Path.Combine(imagesDir, fileName);

                using (var stream = new MemoryStream())
                {
                    Upload.CopyTo(stream);
                    stream.Position = 0;

                    using (var image = new MagickImage(stream))
                    {
                        using (var watermark = new MagickImage(watermarkPath))
                        {
                            int imageWidth = (int)image.Width;
                            int imageHeight = (int)image.Height;

                            int watermarkWidth = (int)(imageWidth * 0.2);
                            int watermarkHeight = (int)(imageHeight * 0.2);

                            watermark.Resize((uint)watermarkWidth, (uint)watermarkHeight);
                            watermark.Evaluate(Channels.Alpha, EvaluateOperator.Divide, 4);
                            image.Composite(watermark, Gravity.Southeast, CompositeOperator.Over);
                        }
                        image.Write(filePath);
                    }
                }
            }
            return RedirectToPage("Index");
        }
    }
}
