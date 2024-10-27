using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.NetworkInformation;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace ResolutionLocator
{
    public partial class MainWindow : Window
    {
        private static MainWindow mw = (MainWindow)Application.Current.MainWindow;

        private static string[][] photos = { new string[] { "" } };

        private static string filePath; 
        private static string directoryPath;
        private static string fileName;

        static Thread thread;



        public MainWindow()
        {
            InitializeComponent();
            mw = this;
        }



        private void Choose_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            bool? success = fileDialog.ShowDialog();

            if (success == true)
            {
                mw.SpinLoader.Visibility = Visibility.Visible;

                filePath = fileDialog.FileName;
                fileName = fileDialog.SafeFileName;
                directoryPath = System.IO.Path.GetDirectoryName(fileDialog.FileName);

                string searchFolder = directoryPath;
                var filters = new string[] { "jpg", "png" };

                PhotoGrid.ItemsSource = null;
                GetFilesFrom(searchFolder, filters, false);
            }
        }

        public async void GetFilesFrom(string searchFolder, string[] filters, bool isRecursive)
        {
            List<string> filePathes = new List<string>();

            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            object locker = new object();

            foreach (var filter in filters)
            {
                filePathes.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
                ArrayResize(ref photos, filePathes.Count, 4);
            }

            for (int i = 0; i < photos.GetLength(0) - 1; i++)
            {
                Thread.Sleep(10);

                thread = new Thread(() => SetResolution(filePathes[i], i, locker));
                thread.Start();

                PhotoGrid.ItemsSource = photos;

                Thread.Sleep(5);
            }
        }

        static void SetResolution(string currentPath, int i, object locker)
        {
            lock (locker)
            {
                Image image = Image.FromFile(currentPath);

                int width = image.Width;
                int height = image.Height;

                image.Dispose();

                photos[i][0] = (i + 1).ToString();
                photos[i][1] = currentPath.Replace(directoryPath + "\\", "");
                photos[i][2] = width + "x" + height;
                photos[i][3] = (Convert.ToDouble(height) / Convert.ToDouble(width)).ToString();
            }
        }

        public static void ArrayResize(ref string[][] arrayToResize, int maxRow, int maxColumn)
        {
            Array.Resize(ref arrayToResize, maxRow);

            for (int i = 0; i < arrayToResize.Length; i++)
            {
                Array.Resize(ref arrayToResize[i], maxColumn);
            }
        }
    }
}
