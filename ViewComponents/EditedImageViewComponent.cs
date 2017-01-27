using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ImageEditionApp.ViewModels;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.Processing;

namespace ImageEditionApp.ViewComponents
{
    public class EditedImageViewComponent : ViewComponent
    {

        public EditedImageViewComponent()
        {
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

