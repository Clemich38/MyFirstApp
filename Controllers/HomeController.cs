using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace MyFirstApp.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment _environment;
        private List<String> list = new List<String>();

        public HomeController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {

            Configuration.Default.AddImageFormat(new JpegFormat());

            var uploads = Path.Combine(_environment.WebRootPath, "uploads");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using (var input = System.IO.File.Open(Path.Combine(uploads, file.FileName), FileMode.Create))
                    {
                        // await file.CopyToAsync(input);
                        file.CopyTo(input);
                        ViewData["fileName"] = Path.Combine("uploads", file.FileName);
                    }

                    using (var input = System.IO.File.OpenRead(Path.Combine(uploads, file.FileName)))
                    {
                        var image = new Image(input)
                            .Resize(new ResizeOptions
                            {
                                Size = new Size(100, 100),
                                Mode = ResizeMode.Max
                            });

                        image.ExifProfile = null;
                        image.Quality = 75;

                        using (var output = System.IO.File.OpenWrite(Path.Combine(uploads, "ret.jpg")))
                        {
                            image.Save(output);
                            ViewData["fileNameRet"] = Path.Combine("uploads", "ret.jpg");
                        }
                    }


                }
            }

            // // List the uploaded files
            // foreach (string file in Directory.EnumerateFiles(uploads, "*" , SearchOption.AllDirectories))
            // {   
            //     list.Add(file);
            // }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
