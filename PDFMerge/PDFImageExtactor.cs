using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PDFMerge
{

    /// <summary>
    /// Helper lass to dump all images from a PDF into separate files
    /// </summary>
    internal class ImageExtractor
    {
        readonly string _outputFilePrefix;
        readonly string _outputFolder;
        readonly bool _overwriteExistingFiles;

        private ImageExtractor(string outputFilePrefix, string outputFolder, bool overwriteExistingFiles)
        {
            _outputFilePrefix = outputFilePrefix;
            _outputFolder = outputFolder;
            _overwriteExistingFiles = overwriteExistingFiles;
        }

        /// <summary>
        /// Extract all images from a PDF file
        /// </summary>
        /// <param name="pdfPath">Full path and file name of PDF file</param>
        /// <param name="outputFilePrefix">Basic name of exported files. If null then uses same name as PDF file.</param>
        /// <param name="outputFolder">Where to save images. If null or empty then uses same folder as PDF file.</param>
        /// <param name="overwriteExistingFiles">True to overwrite existing image files, false to skip past them</param>
        /// <returns>Count of number of images extracted.</returns>
        public static int ExtractImagesFromFile(string pdfPath, string outputFilePrefix, string outputFolder, bool overwriteExistingFiles)
        {
            // Handle setting of any default values
            outputFilePrefix = outputFilePrefix ?? System.IO.Path.GetFileNameWithoutExtension(pdfPath);
            outputFolder = String.IsNullOrEmpty(outputFolder) ? System.IO.Path.GetDirectoryName(pdfPath) : outputFolder;
            int pagecount = -1;

            string path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Ghostscript.NET.Rasterizer.GhostscriptRasterizer rasterizer = null;
            Ghostscript.NET.GhostscriptVersionInfo vesion = new Ghostscript.NET.GhostscriptVersionInfo(new Version(0, 0, 0), path + @"\gsdll32.dll", string.Empty, Ghostscript.NET.GhostscriptLicense.GPL);

            using (rasterizer = new Ghostscript.NET.Rasterizer.GhostscriptRasterizer())
            {
                rasterizer.Open(pdfPath, vesion, false);
                pagecount = rasterizer.PageCount;

                for (int pgnum = 1; pgnum <= rasterizer.PageCount; pgnum++)
                {
                    string pageFilePath = System.IO.Path.Combine(outputFolder, String.Format("{0}_{1}{2}", outputFilePrefix, pgnum, ".jpg"));
                    Image img = rasterizer.GetPage(80, 80, pgnum);
                    var bwImg = MakeGrayscale3(new Bitmap(img));
                    bwImg.Save(pageFilePath, ImageFormat.Jpeg);
                }

                rasterizer.Close();
            }
            return pagecount;
        }

        private static Bitmap GrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)((c.R + c.G + c.B) / 3);
                    Bmp.SetPixel(x, y, Color.FromArgb(rgb, rgb, rgb));
                }
            return Bmp;
        }

        private static Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
         new float[] {.3f, .3f, .3f, 0, 0},
         new float[] {.59f, .59f, .59f, 0, 0},
         new float[] {.11f, .11f, .11f, 0, 0},
         new float[] {0, 0, 0, 1, 0},
         new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }
    }
}
