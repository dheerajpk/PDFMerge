
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
namespace PDFMerge
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PDFProcess pdfProc = new PDFProcess();
        private OpenFileDialog openFileDialog = new OpenFileDialog();
        private List<string> pdfFiles = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            pdfProc.ImageExtractCompleted += PdfProc_ImageExtractCompleted;
            //TestImageSign();
        }

       
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //if (PDFMerge.Properties.Settings.Default["EmpId"] != null)
            //{
            //    txtEmpdLbl.Content = string.Format("Employee ID {0}", PDFMerge.Properties.Settings.Default["EmpId"].ToString());
            //}

            if (!ImageManipulator.IsEmpIdImageExist)
            {
                grdEmpId.Visibility = Visibility.Visible;
                txtEmpId.Focus();
            }
            else
            {
                grdEmpId.Visibility = Visibility.Collapsed;
            }
            pdfProc.ClearTemp();

        }

        IEnumerable<string> pageImages = null;
        private void PdfProc_ImageExtractCompleted(object sender, PDFProcessEventArgs e)
        {
            pageImages = e.PageImages;
            //Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,);
            listView.Dispatcher.BeginInvoke((Action)(() => { listView.ItemsSource = e.PageImages.Select(x => new PDFImageView() { FileName = x }); }));

            //listView.ItemsSource = e.PageImages;


            var thumbs = new List<PDFThumb>();

            foreach (var item in e.PageImages)
            {

                foreach (var itempdf in pdfFiles)
                {
                    if (item.Contains(System.IO.Path.GetFileNameWithoutExtension(itempdf)))
                    {
                        thumbs.Add(new PDFThumb()
                        {
                            PDFName = System.IO.Path.GetFileNameWithoutExtension(itempdf),
                            FileName = item
                        });
                        break;
                    }
                }
            }

            lstvwpdf.Dispatcher.BeginInvoke((Action)(() =>
            {
                lstvwpdf.ItemsSource = thumbs.DistinctBy(x => x.PDFName);
                listView.SelectAll();
                pgrsbar.Visibility = Visibility.Collapsed;
            }));
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Adobe Pdf files (*.pdf)|*.pdf";
            openFileDialog.FileOk += OpenFileDialog_FileOk;
            openFileDialog.ShowDialog(this);
        }

        private void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            pgrsbar.Visibility = Visibility.Visible;
            var opf = sender as OpenFileDialog;

            pdfFiles.AddRange(opf.FileNames);

            Task.Factory.StartNew(() =>
            {
                pdfProc.StartProcess(opf.FileNames);
            });

        }

        private void btnZip_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                var items = listView.SelectedItems.AsQueryable().Cast<PDFImageView>();
                this.IsEnabled = false;
                pgrsbar.Visibility = Visibility.Visible;
                Task.Factory.StartNew(() =>
                 {
                     pdfProc.CreatePDF(items);
                     Process.Start(pdfProc.WorkSpaceOutPutDirectory);

                     this.Dispatcher.BeginInvoke((Action)(() =>
                     {
                         this.IsEnabled = true;
                         pgrsbar.Visibility = Visibility.Collapsed;
                     }));
                 });
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            pdfProc.ClearTemp();
            listView.ItemsSource = null;
            lstvwpdf.ItemsSource = null;
            pageImages = null;
            pdfFiles = new List<string>();
        }

        private void btnEmpId_Click(object sender, RoutedEventArgs e)
        {
            ImageManipulator.CreateHeader("EmpId:" + txtEmpId.Text.Trim(), new Font(GetFont().Families[0], 31f, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel), 140, 40);
            ImageManipulator.CreateHeader(txtEmpName.Text.Trim(), new Font(GetFont().Families[0], 30f, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel), 140, 40, "Signature.png",true);
            grdEmpId.Visibility = Visibility.Collapsed;
            PDFMerge.Properties.Settings.Default.Save();
            //txtEmpdLbl.Content = string.Format("Employee ID {0}", txtEmpId.Text);
        }

        private PrivateFontCollection GetFont()
        {

            // specify embedded resource name
            PrivateFontCollection private_fonts = new PrivateFontCollection();

            // receive resource stream
            Stream fontStream = Application.GetContentStream(new Uri("/VINCHAND.ttf", UriKind.RelativeOrAbsolute)).Stream;

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);

            // create a buffer to read in to
            byte[] fontdata = new byte[fontStream.Length];

            // read the font data from the resource
            fontStream.Read(fontdata, 0, (int)fontStream.Length);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);

            // pass the font to the font collection
            private_fonts.AddMemoryFont(data, (int)fontStream.Length);

            // close the resource stream
            fontStream.Close();

            // free up the unsafe memory
            Marshal.FreeCoTaskMem(data);
            return private_fonts;
        }

        private void TestImageSign()
        {
            string sourceImage = @"C:\Users\dheepk\Documents\PDFMerge\Reim_P_2.jpg";
            string templateImage = @"C:\Users\dheepk\Documents\PDFMerge\template.jpg";
            //Bitmap.FromFile(sourceImage), Bitmap.FromFile(templateImage)

            //ImageManipulator.RDImageSignature(new Bitmap(sourceImage), new Bitmap(templateImage));

        }

        #region ZoomInOut
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var sz = (double)this.Resources["szHeight"];
            sz = sz + 10;
            this.Resources["szHeight"] = sz;
        }

        private void Buttonzmout_Click(object sender, RoutedEventArgs e)
        {
            var sz = (double)this.Resources["szHeight"];
            if (sz < 100) return;
            sz = sz - 10;
            this.Resources["szHeight"] = sz;
        } 
        #endregion
    }

    public class PDFThumb
    {
        public string PDFName { get; set; }

        public string FileName { get; set; }
    }

    public class PDFImageView : PropertyBase
    {
        public string FileName { get; set; }

        public bool IsSigned
        {
            get
            {
                return isSigned;
            }
            set
            {
                isSigned = value;
                RaisePropertyChanged("IsSigned");
            }
        }

        private bool isSigned;

        private bool addSignature;

        public bool CanAddSignature
        {
            get { return addSignature; }
            set { addSignature = value; }
        }


        public PDFImageView()
        {
            IsSigned = true;
            addSignature = false;
        }
    }

    public class PropertyBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyname">The propertyname.</param>
        public void RaisePropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

    }
}

/*

using (FileStream file = File.OpenRead(@"C:\Users\dheepk\Desktop\Bill.pdf")) // in file
        {
            var bytes = new byte[file.Length];
            file.Read(bytes, 0, bytes.Length);
            using (var pdf = new libpdf.LibPdf(bytes))
            {
                byte[] pngBytes = pdf.GetImage(0, ImageType.PNG); // image type
                using (var outFile = File.Create(@"C:\Users\dheepk\Documents\PDFMerge\Temp\file.png")) // out file
                {
                    outFile.Write(pngBytes, 0, pngBytes.Length);
                }
            }
        }
*/

