/*
 * Lua Automation IDE
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2022 All rights reserved.
 * @license           : Closed Source
 */

using System.Drawing;
using System.Drawing.Imaging;

namespace MoonSharp.Interpreter.Wpf.Types
{
    /// <summary>
    /// A thin wrapper around the <see cref="Bitmap"/>
    /// </summary>
    public class LuaBitmap
    {
        private Bitmap? _bitmap;

        public LuaBitmap(int height, int width)
        {
            _bitmap = new(height, width);
        }

        public LuaBitmap(string filename)
        {
            _bitmap = new Bitmap(filename);
        }

        public LuaBitmap(Bitmap bmp)
        {
            _bitmap = bmp;
        }

        [MoonSharpModuleMethod(Description = "Sets the color of the specified pixel.",
                               ParameterCount = 3)]
        public void SetPixel(int x, int y, string htmlColor)
        {
            var color = ColorTranslator.FromHtml(htmlColor);
            _bitmap?.SetPixel(x, y, color);
        }

        [MoonSharpModuleMethod(Description = "Sets the color of the specified pixel.",
            ParameterCount = 3)]
        public string GetPixel(int x, int y)
        {
            if (_bitmap == null)
            {
                return "";
            }

            var color = _bitmap.GetPixel(x, y);
            return ColorTranslator.ToHtml(color);
        }

        [MoonSharpModuleMethod(Description = "Loads an image into the current bitmap freeing the memory of any existing LuaBitmap.",
            ParameterCount = 2)]
        public void Load(string filename)
        {
            _bitmap?.Dispose();
            _bitmap = new Bitmap(filename);
        }

        [MoonSharpModuleMethod(Description = "Saves an image in the specified format.",
            ParameterCount = 2)]
        public void Save(string filename, ImageFormat format)
        {
            _bitmap?.Save(filename, format);
        }

        [MoonSharpModuleMethod(Description = "Saves an image as a PNG file.",
            ParameterCount = 1)]
        public void SavePng(string filename)
        {
            this.Save(filename, ImageFormat.Png);    
        }

        [MoonSharpModuleMethod(Description = "Saves an image as a JPEG file.",
            ParameterCount = 1)]
        public void SaveJpeg(string filename)
        {
            this.Save(filename, ImageFormat.Jpeg);;
        }

        [MoonSharpModuleMethod(Description = "Saves an image as a TIFF file.",
            ParameterCount = 1)]
        public void SaveTiff(string filename)
        {
            this.Save(filename, ImageFormat.Tiff);
        }

        /// <summary>
        /// Frees the memory associated with an instance of <see cref="LuaBitmap"/>.
        /// </summary>
        [MoonSharpModuleMethod(Description = "Frees the memory associated with an instance of the LuaBitmap.",
            ParameterCount = 0)]
        public void Dispose()
        {
            _bitmap?.Dispose();
        }
    }
}