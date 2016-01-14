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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MenuReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CameraAPI camera;
        private Stream idPhoto;
        private Stream portraitPhoto;
        private Stream htmlPhoto;
        private SoftwareBitmapSource idPhotoBitmap;
        private SoftwareBitmapSource portraitPhotoBitmap;
        private SoftwareBitmapSource htmlPhotoBitmap;
        private SoftwareBitmap htmlB;

        string idPath;
        string portraitPath;

        private FaceAPI faceAPI;

        public MainPage()
        {
            this.InitializeComponent();
            camera = new CameraAPI();
            faceAPI = new FaceAPI();
        }

        private async void CameraButton_Click(object sender, RoutedEventArgs e)
        {
            await camera.TakePhoto();
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
                camera.StorePhoto(file);
            }
            else
            {
                ShowMessage("Operation cancelled.");
            }
        }

        private async void LoadIDButton_Click(object sender, RoutedEventArgs e)
        {
            idPhotoBitmap = camera.getPhotoAsSoftwareBitmapSource();
            idPhoto = camera.getPhotoAsStream();
            IDPhoto.Source = idPhotoBitmap;

            idPath = await StorageAPI.SavePhoto(idPhoto, "ID");
        }

        private async void LoadPortraitButton_Click(object sender, RoutedEventArgs e)
        {
            portraitPhotoBitmap = camera.getPhotoAsSoftwareBitmapSource();
            portraitPhoto = camera.getPhotoAsStream();
            PortraitPhoto.Source = portraitPhotoBitmap;

            portraitPath = await StorageAPI.SavePhoto(idPhoto, "Portrait");
        }

        private void GenerateHTMLButton_Click(object sender, RoutedEventArgs e)
        {
            htmlPhotoBitmap = camera.getPhotoAsSoftwareBitmapSource();
            htmlPhoto = camera.getPhotoAsStream();
            HtmlPhoto.Source = htmlPhotoBitmap;
            htmlB = camera.getPhotoAsSoftwareBitmap();

            HtmlGenerator gen = new HtmlGenerator(idPhoto, htmlB.PixelWidth, htmlB.PixelHeight, portraitPath);
            gen.GenerateHtmlAsync();      
        }

        private async void ShowMessage(string msg)
        {
            MessageDialog dialog = new MessageDialog(msg);
            await dialog.ShowAsync();
        }
    }
}
