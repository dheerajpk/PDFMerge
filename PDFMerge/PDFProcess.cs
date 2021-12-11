using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO.Compression;
using Ionic.Zip;

namespace PDFMerge
{
    internal class PDFProcess
    {
        private string WorkSpaceDirectory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PDFMerge");
            }
        }

        private string WorkSpaceTempDirectory
        {
            get
            {
                return Path.Combine(WorkSpaceDirectory, "Temp");
            }
        }

        public string WorkSpaceOutPutDirectory
        {
            get
            {
                return Path.Combine(WorkSpaceDirectory, "Output");
            }
        }

        public string WorkSpaceSettingsDirectory
        {
            get
            {
                return Path.Combine(WorkSpaceDirectory, "Settings");
            }
        }

        public event EventHandler<PDFProcessEventArgs> ImageExtractCompleted;

        public PDFProcess()
        {
            CheckWorkSpaceDirectory();
        }

        public void StartProcess(IList<string> pdffilepaths)
        {
            if (null != pdffilepaths && pdffilepaths.Any())
            {
                //ClearTemp();
                foreach (var item in pdffilepaths)
                {

                    ImageExtractor.ExtractImagesFromFile(item, Path.GetFileNameWithoutExtension(item) + "_P", WorkSpaceTempDirectory, true);
                }

                //Raise Event
                var imageFiles = Directory.GetFiles(WorkSpaceTempDirectory);
                if (null != imageFiles && imageFiles.Any())
                {
                    if (null != ImageExtractCompleted)
                    {
                        ImageExtractCompleted(this, new PDFProcessEventArgs(imageFiles));
                    }
                }
            }
        }

        private void CheckWorkSpaceDirectory()
        {
            if (!Directory.Exists(WorkSpaceDirectory))
            {
                Directory.CreateDirectory(WorkSpaceDirectory);
            }
        }

        public void ClearTemp()
        {
            if (!Directory.Exists(WorkSpaceTempDirectory))
            {
                Directory.CreateDirectory(WorkSpaceTempDirectory);
            }
            else
            {
                Directory.Delete(WorkSpaceTempDirectory, true);
                System.Threading.Thread.Sleep(150);//Give wait time delete all files
                Directory.CreateDirectory(WorkSpaceTempDirectory);
            }
        }

        private void CreateWorkSpaceOutPutDirectory()
        {
            if (!Directory.Exists(WorkSpaceOutPutDirectory))
            {
                Directory.CreateDirectory(WorkSpaceOutPutDirectory);
            }
        }

        public void CreatePDF(IEnumerable<PDFImageView> imagepaths, string outputpdf = null)
        {
            if (string.IsNullOrEmpty(outputpdf))
            {
                CreateWorkSpaceOutPutDirectory();
                outputpdf = Path.Combine(WorkSpaceOutPutDirectory, string.Format("Merge_{0}.pdf", DateTime.Now.Ticks));
            }

            ConvertImageToPdf(imagepaths, outputpdf);

            string compressfile = Path.Combine(WorkSpaceOutPutDirectory, string.Format("Merge_{0}.zip", DateTime.Now.Ticks));
            CompressToZip(new string[] { outputpdf }, compressfile);

        }

        private void ConvertImageToPdf(IEnumerable<PDFImageView> imagepaths, string dstFilename)
        {
            string signpath = Path.Combine(WorkSpaceSettingsDirectory, "EmpId.png");
            iTextSharp.text.Rectangle pageSize = null;
            using (Document document = new Document(GetImagePageSize(imagepaths.FirstOrDefault().FileName)))
            {
                PdfWriter.GetInstance(document, new FileStream(dstFilename, FileMode.Create));
                document.Open();
                foreach (var srcFilename in imagepaths)
                {
                    pageSize = GetImagePageSize(srcFilename.FileName);
                    document.SetPageSize(pageSize);
                    document.SetMargins(0, 0, 0, 0);

                    var image = iTextSharp.text.Image.GetInstance(srcFilename.FileName);
                    image.GrayFill = 10f;
                    document.Add(image);
                    if (srcFilename.IsSigned)
                    {
                        var signImage = iTextSharp.text.Image.GetInstance(signpath);
                        signImage.ImageMask.MakeMask();
                        signImage.ScalePercent(70f);
                        signImage.SetAbsolutePosition(pageSize.Width - 220, pageSize.Height - 50);
                        document.Add(signImage);
                    }
                    if (srcFilename.CanAddSignature)
                    {
                        string signaturepath = Path.Combine(WorkSpaceSettingsDirectory, "Signature.png");
                        var signImage = iTextSharp.text.Image.GetInstance(signaturepath);
                        signImage.ImageMask.MakeMask();
                        signImage.ScalePercent(80f);
                        var postion = ImageManipulator.RDImageSignature(new Bitmap(srcFilename.FileName));
                        if (!postion.IsEmpty)
                        {
                            signImage.SetAbsolutePosition(postion.X, postion.Y - 20);
                            document.Add(signImage);
                        }
                    }
                    document.NewPage();
                }
                document.Close();
            }

        }

        private void CompressToZip(IEnumerable<string> pdffiles, string outcompressfile)
        {

            using (ZipFile zip = new ZipFile())
            {
                zip.FlattenFoldersOnExtract = true;
                foreach (var item in pdffiles)
                {
                    zip.AddFile(item, string.Empty);
                }

                zip.Save(outcompressfile);
            }
        }

        private static iTextSharp.text.Rectangle GetImagePageSize(string imagefile)
        {
            iTextSharp.text.Rectangle pageSize = null;
            using (var srcImage = new Bitmap(imagefile))
            {
                pageSize = new iTextSharp.text.Rectangle(0, 0, srcImage.Width, srcImage.Height);
            }

            return pageSize;
        }
    }

    internal class PDFProcessEventArgs : EventArgs
    {
        public IEnumerable<string> PageImages { get; private set; }

        //public string PDFFileName { get; set; }

        public PDFProcessEventArgs(IEnumerable<string> files)
        {
            PageImages = files;
            //PDFFileName = pdfname;
        }
    }
}
