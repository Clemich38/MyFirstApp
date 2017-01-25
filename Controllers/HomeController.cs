using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections;

using MyFirstApp.ViewModels;

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
            // Create the best view model ever
            var viewModel = new ImageModel
            {
                ImageName = "No Image Yet",
                UploadPath = "/uploads",
                ImagePath = "No Image Yet",
                ImageWidth = 100,
                ImageHeight = 100,
            };

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult Upload(ICollection<IFormFile> files)
        {
            // Retreive the model
            var viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");

            Configuration.Default.AddImageFormat(new JpegFormat());

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string imageName = file.FileName;
                    using (var input = System.IO.File.Open(Path.Combine(uplaodPath, imageName), FileMode.Create))
                    {
                        // await file.CopyToAsync(input);
                        file.CopyTo(input);
                        viewModel.ImageName = imageName;
                        viewModel.ImagePath = Path.Combine(viewModel.UploadPath, imageName);

                        var image = new Image(input);
                        viewModel.ImageHeight = image.Height;
                        viewModel.ImageWidth = image.Width;


                        using (var inputret = System.IO.File.Open(Path.Combine(uplaodPath, "ret.jpg"), FileMode.Create))
                        {
                            // await file.CopyToAsync(input);
                            file.CopyTo(inputret);
                            viewModel.RetImagePath = Path.Combine(viewModel.UploadPath, "ret.jpg");
                        }
                    }
                }
            }




            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return View("Index", viewModel);
        }

        [HttpGet]  
        public FileResult Download()  
        { 
            // Retreive the model
            var viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(uplaodPath, "ret.jpg"));
            string fileName = "ret.jpg";
            return File(fileBytes, MimeKit.MimeTypes.GetMimeType(fileName), fileName); 
        }  

        public IActionResult resize(int width, int height)
        {
            // Retreive the model
            var viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");
            
            viewModel.ImageWidth = width;
            viewModel.ImageHeight = height;

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, viewModel.ImageName)))
            {
                var image = new Image(input)
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(viewModel.ImageWidth, viewModel.ImageHeight),
                        Mode = ResizeMode.Max
                    });
                    

                image.ExifProfile = null;
                image.Quality = 75;

                using (var output = System.IO.File.OpenWrite(Path.Combine(uplaodPath, "ret.jpg")))
                {
                    image.Save(output);
                }
            }

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return View("Index", viewModel);
        }

        

        public IActionResult filter(int type, int value)
        {
            // Retreive the model
            ImageModel viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");
            
            Image image = null;

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, viewModel.ImageName)))
            {
                image = new Image(input);

                switch(type)
                {
                    case 1: //Brightness
                        viewModel.ImageBrightnessValue = value;
                        break;
                    case 2: //Contrast
                        viewModel.ImageContrastValue = value;
                        break;
                    case 3: //Rotate
                        viewModel.ImageAngle = value;
                        break;
                    case 4: //filter
                        viewModel.ImageFilterType = value;
                        break;
                    case 5: //filter
                        viewModel.ImageSaturationValue = value;
                        break;
                }
                    
                image.Brightness(viewModel.ImageBrightnessValue);
                image.Contrast(viewModel.ImageContrastValue);
                image.Saturation(viewModel.ImageSaturationValue);
                image.Rotate(viewModel.ImageAngle);

                switch(viewModel.ImageFilterType)
                {
                    case 1: image.Grayscale(); break;
                    case 2: image.Lomograph(); break;
                    case 3: image.Polaroid(); break;
                    case 4: image.Sepia(); break;
                    case 5: image.Kodachrome(); break;


                }

                image.ExifProfile = null;
                image.Quality = 100;
            }

            using (var output = System.IO.File.OpenWrite(Path.Combine(uplaodPath, "ret.jpg")))
            {
                image.Save(output);
            }

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return ViewComponent("EditedImage", viewModel);
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


    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
