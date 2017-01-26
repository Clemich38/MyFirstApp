using System;

namespace MyFirstApp.ViewModels
{
  public class ImageModel
  {
    public string ImageName { get; set; }
    public string RetImageName { get; set; }
    public string ImagePath { get; set; }
    public string RetImagePath { get; set; }
    public string UploadPath { get; set; }
    public string AbsUploadPath { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public int ImageAngle { get; set; }
    public int ImageBrightnessValue  { get; set; }
    public int ImageContrastValue  { get; set; }
    public int ImageSaturationValue  { get; set; }
    public int ImageFilterType  { get; set; }
  }
}