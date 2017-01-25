using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using MyFirstApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace MyFirstApp.ViewComponents
{
    public class EditedImageViewComponent : ViewComponent
    {
        // private readonly ImageModel model;

        public EditedImageViewComponent()
        {
            // model = mod;
        }

        public IViewComponentResult Invoke()
        {
            var model =  GetModel();
            return View(model);
        }
        private ImageModel GetModel()
        {
            // Retreive the model
            ImageModel viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");

            return viewModel;
        }
        
        // public IViewComponentResult brigthness(int value)
        // {
        //     // Retreive the model
        //     ImageModel viewModel = HttpContext.Session.GetObjectFromJson<ImageModel>("ImageModel");
            
        //     viewModel.ImageEffectValue += value;
        //     Image image = null;

        //     string uplaodPath = Path.Combine(viewModel.AbsUploadPath, "uploads");
        //     using (var input = System.IO.File.OpenRead(Path.Combine(uplaodPath, "ret.jpg")))
        //     {
        //         image = new Image(input);
        //         image.Brightness(viewModel.ImageEffectValue);
                    
        //         image.ExifProfile = null;
        //         image.Quality = 75;
        //     }
        //     using (var output = System.IO.File.OpenWrite(Path.Combine(uplaodPath, "ret.jpg")))
        //     {
        //         image.Save(output);
        //     }

        //     HttpContext.Session.SetObjectAsJson("ImageModel", viewModel);

        //     return View(viewModel);
        // }
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

