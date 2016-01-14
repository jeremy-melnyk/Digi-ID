using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.ProjectOxford.Vision.Contract;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Web;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MenuReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CameraAPI camera;
        private FaceAPI faceAPI;

        private Stream photoStream;
        private SoftwareBitmap photoBitmap;
        private SoftwareBitmapSource photoBitmapSource;

        private Stream idStream;
        private Stream portraitStream;
        private Stream htmlStream;

        private SoftwareBitmap idBitmap;
        private SoftwareBitmap portraitBitmap;
        private SoftwareBitmap htmlBitmap;

        private SoftwareBitmapSource idBitmapSource;
        private SoftwareBitmapSource portraitBitmapSource;
        private SoftwareBitmapSource htmlBitmapSource;

        private SoftwareBitmap htmlB;

        string portraitPath;

        public MainPage()
        {
            this.InitializeComponent();
            camera = new CameraAPI();
            faceAPI = new FaceAPI();
        }

        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            photoStream = await camera.TakePhoto(CameraCaptureUIPhotoFormat.Jpeg);
            photoBitmap = await camera.ConvertToSoftwareBitmap(photoStream);
            photoBitmapSource = await camera.ConvertToSoftwareBitmapSource(photoBitmap);
        }

        private async void BrowsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                photoStream = await camera.ConvertToStream(file);
                photoBitmap = await camera.ConvertToSoftwareBitmap(photoStream);
                photoBitmapSource = await camera.ConvertToSoftwareBitmapSource(photoBitmap);
            }
        }

        private void LoadIDButton_Click(object sender, RoutedEventArgs e)
        {
            idStream = photoStream;
            idBitmap = photoBitmap;
            idBitmapSource = photoBitmapSource;

            IDPhoto.Source = idBitmapSource;
        }

        private async void LoadPortraitButton_Click(object sender, RoutedEventArgs e)
        {
            portraitStream = photoStream;
            portraitBitmap = photoBitmap;
            portraitBitmapSource = photoBitmapSource;

            PortraitPhoto.Source = portraitBitmapSource;

            portraitPath = await StorageAPI.SavePhoto(portraitStream, "Portrait");
        }

        private void GenerateHTMLButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            htmlStream = photoStream;
            htmlBitmap = photoBitmap;
            htmlBitmapSource = photoBitmapSource;

            HtmlPhoto.Source = htmlBitmapSource;
            */

            if (idStream != null && idBitmap != null && portraitPath != null)
            {
                HtmlGenerator gen = new HtmlGenerator(idStream, idBitmap.PixelWidth, idBitmap.PixelHeight, portraitPath);
                gen.GenerateHtmlAsync();

                Uri source = new Uri("ms-appdata:///Local/Packages/59ef7bd3-08e9-4e25-993d-ccf404cd019e_cj1w1dahfk9sw/TempState/TEMP_HTML.html");
                WebView.Navigate(source);
            }
        }

        private async void ShowMessage(string msg)
        {
            MessageDialog dialog = new MessageDialog(msg);
            await dialog.ShowAsync();
        }
    }
}
