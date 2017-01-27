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

using ImageEditionApp.ViewModels;

using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace ImageEditionApp.Controllers
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
            Configuration.Default.AddImageFormat(new JpegFormat());
            Configuration.Default.AddImageFormat(new BmpFormat());
            Configuration.Default.AddImageFormat(new GifFormat());
            Configuration.Default.AddImageFormat(new PngFormat());

            // Create the best view model ever
            var viewModel = new ImageModel
            {
                ImageName = "lena.jpg",
                ImagePath = "/uploads/lena.jpg",
                UploadPath = "/uploads/",
                RawImageName = "raw.jpg",
                RetImageName = "ret.jpg",
                RetImagePath = "/uploads/ret.jpg",
                ImageExtension = ".jpg",
                ImageWidth = 0,
                ImageHeight = 0,
                NewImageWidth = 0,
                NewImageHeight = 0,
                ImageResolution = 100,
                ImageBrightnessValue = 0,
                ImageContrastValue = 0,
                ImageAngle = 0,
                ImageFilterType = 0,
                ImageSaturationValue = 0,
            };
            
            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            System.IO.File.Copy(Path.Combine(uplaodPath, viewModel.ImageName), Path.Combine(uplaodPath, viewModel.RetImageName), true);
 
            using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, viewModel.ImageName)))
                {
                    var image = new Image(input);
                    viewModel.ImageHeight = image.Height;
                    viewModel.ImageWidth = image.Width;
                    viewModel.NewImageHeight = image.Height;
                    viewModel.NewImageWidth = image.Width;
                }

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return Reset();
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
                    viewModel.ImageName = Path.ChangeExtension(viewModel.RawImageName,Path.GetExtension(file.FileName));
                    viewModel.ImagePath = Path.Combine(viewModel.UploadPath, viewModel.ImageName);
                    using (var input = System.IO.File.Open(Path.Combine(uplaodPath, viewModel.ImageName), FileMode.Create))
                    {
                        file.CopyTo(input);
                        viewModel.RetImageName = Path.ChangeExtension(viewModel.RetImageName,Path.GetExtension(viewModel.ImageName));
                        viewModel.RetImagePath = Path.ChangeExtension(viewModel.RetImagePath,Path.GetExtension(viewModel.ImageName));

                        using (var inputret = System.IO.File.Open(Path.Combine(uplaodPath, viewModel.RetImageName), FileMode.Create))
                        {
                            file.CopyTo(inputret);
                        }
                    }

                    using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, viewModel.ImageName)))
                    {
                        var image = new Image(input);
                        viewModel.ImageHeight = image.Height;
                        viewModel.ImageWidth = image.Width;
                        viewModel.NewImageHeight = image.Height;
                        viewModel.NewImageWidth = image.Width;
                        viewModel.ImageResolution = 100;
                        viewModel.ImageBrightnessValue = 0;
                        viewModel.ImageContrastValue = 0;
                        viewModel.ImageAngle = 0;
                        viewModel.ImageFilterType = 0;
                        viewModel.ImageSaturationValue = 0;
                    }

                }
            }

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return View("Index", viewModel);
        }


        [HttpPost]
        public IActionResult Reset()
        {
            ImageModel viewModel = new ImageModel();

            return View("Index", processImage(0, 0, viewModel));
        }

        [HttpGet]  
        public FileResult Download()  
        { 
            // Retreive the model
            var viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(uplaodPath, viewModel.RetImageName));
            string fileName = viewModel.RetImageName;
            return File(fileBytes, MimeKit.MimeTypes.GetMimeType(fileName), fileName); 
        }  

        

        public IActionResult filter(int type, int value)
        {
            ImageModel viewModel = new ImageModel();

            return ViewComponent("EditedImage", processImage(type, value, viewModel));
        }

        public ImageModel processImage(int type, int value, ImageModel viewModel)
        {
            // Retreive the model
            viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");
            
            Image image = null;

            string uplaodPath = Path.Combine(_environment.WebRootPath, "uploads");
            using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, viewModel.ImageName)))
            {
                image = new Image(input);

                switch(type)
                {
                    case 0: //Reset
                        viewModel.ImageBrightnessValue = 0;
                        viewModel.ImageContrastValue = 0;
                        viewModel.ImageAngle = 0;
                        viewModel.ImageFilterType = 0;
                        viewModel.ImageSaturationValue = 0;
                        viewModel.ImageResolution = 100;
                        viewModel.NewImageHeight = viewModel.ImageHeight;
                        viewModel.NewImageWidth = viewModel.ImageWidth;
                        break;
                    case 1: //Brightness
                        viewModel.ImageBrightnessValue = value;
                        break;
                    case 2: //Contrast
                        viewModel.ImageContrastValue = value;
                        break;
                    case 3: //Rotate
                        viewModel.ImageAngle = value;
                        break;
                    case 4: //Filter
                        viewModel.ImageFilterType = value;
                        break;
                    case 5: //Saturation
                        viewModel.ImageSaturationValue = value;
                        break;
                    case 6: //Resize
                        viewModel.ImageResolution = value;
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

                if (viewModel.ImageResolution != 100)
                {
                    IResampler resampler = new BicubicResampler();
                    viewModel.NewImageHeight = (int)Math.Floor((decimal)((viewModel.ImageHeight * viewModel.ImageResolution) / 100));
                    viewModel.NewImageWidth = (int)Math.Floor((decimal)((viewModel.ImageWidth * viewModel.ImageResolution) / 100));
                    image.Resize(viewModel.NewImageWidth, viewModel.NewImageHeight, resampler);
                }

                image.ExifProfile = null;
                image.Quality = 100;
            }

            using (var output = System.IO.File.OpenWrite(Path.Combine(uplaodPath, viewModel.RetImageName)))
            {
                image.Save(output);
            }

            HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

            return viewModel;
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
