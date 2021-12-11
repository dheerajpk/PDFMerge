using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using AForge;
using AForge.Imaging;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;

namespace PDFMerge
{
    internal class ImageManipulator
    {
        private static Bitmap template = null;
        static ImageManipulator()
        {
            var strm = App.GetContentStream(ne§w Uri("/Signature.jpg", UriKind.RelativeOrAbsolute)).Stream;
            template = new Bitmap(strm);

        }
        public static bool IsEmpIdImageExist
        {
            get
            {
                return File.Exists(Path.Combine(new PDFProcess().WorkSpaceSettingsDirectory, "EmpId.png"));
            }
        }
        public static void CreateHeader(string text, Font font, int width, int height, string fname = "EmpId.png", bool isdrawunderline = false)
        {
            PDFProcess pdfProc = new PDFProcess();
            if (!Directory.Exists(pdfProc.WorkSpaceSettingsDirectory))
            {
                Directory.CreateDirectory(pdfProc.WorkSpaceSettingsDirectory);
            }

            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.FillRectangle(new SolidBrush(Color.Transparent), 0, 0, bmp.Width, bmp.Height);
                    graphics.DrawString(text, font, new SolidBrush(Color.FromArgb(255, 10, 68, 143)), 2, 2);
                    if (isdrawunderline)
                    {
                        graphics.DrawLine(new Pen(Color.FromArgb(10, 68, 143)), new PointF(5, 35), new PointF(100, 30));
                    }
                    graphics.Flush();
                    font.Dispose();
                    graphics.Dispose();


                }
                bmp.Save(Path.Combine(pdfProc.WorkSpaceSettingsDirectory, fname), System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        public static Rectangle RDImageSignature(Bitmap sourceImage)
        {

            // create template matching algorithm's instance
            // (set similarity threshold to 92.5%)
            ExhaustiveTemplateMatching tm = new ExhaustiveTemplateMatching(0.99f);
            // find all matchings with specified above similarity
            TemplateMatch[] matchings = tm.ProcessImage(sourceImage, template);
            // highlight found matchings
            BitmapData data = sourceImage.LockBits(
                new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                ImageLockMode.ReadWrite, sourceImage.PixelFormat);
            foreach (TemplateMatch m in matchings)
            {
                Drawing.Rectangle(data, m.Rectangle, Color.Red);
                // do something else with matching
            }
            sourceImage.UnlockBits(data);
            if (matchings.Any())
                return matchings[0].Rectangle;
            return Rectangle.Empty;
        }
    }
}
