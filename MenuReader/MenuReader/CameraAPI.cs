using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace MenuReader
{
    class CameraAPI
    {
        public readonly int MAX_RATIO_WIDTH = 16;
        public readonly int MAX_RATIO_HEIGHT = 9;

        private Stream photo;
        private SoftwareBitmapSource bitmapSource;

        //private LinkedList<Stream> photos;
        //private LinkedList<SoftwareBitmapSource> bitmapSources;

        public CameraAPI()
        {
        }

        public async Task TakePhoto(int ratio_width, int ratio_height)
        {
            if (ratio_width > MAX_RATIO_WIDTH)
            {
                ShowMessage("Max photo width exceeded.");
                return;
            }

            if (ratio_height > MAX_RATIO_HEIGHT)
            {
                ShowMessage("Max photo height exceeded.");
                return;
            }

            CameraCaptureUI captureUI = new CameraCaptureUI();
            Size aspectRatio = new Size(ratio_width, ratio_height);
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedAspectRatio = aspectRatio;

            StorageFile photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photo == null)
            {
                ShowMessage("Photo not taken.");
                return;
            }
            else
            {
                StorePhoto(photo);
            }
        }

        public async Task TakePhoto()
        {
            await TakePhoto(MAX_RATIO_WIDTH, MAX_RATIO_HEIGHT);
        }

        public Stream getPhotoAsStream()
        {
            return photo;
        }

        public SoftwareBitmapSource getPhotoAsSoftwareBitmapSource()
        {
            return bitmapSource;
        }

        public async void StorePhoto(StorageFile photo)
        {
            IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
            Stream streamPhoto = stream.AsStream();
            this.photo = streamPhoto;
            await ConvertToBGR8(streamPhoto);
            ShowMessage("Photo stored.");
        }

        private async Task ConvertToBGR8(Stream capturedPhoto)
        {
            IRandomAccessStream stream = capturedPhoto.AsRandomAccessStream();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();

            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(softwareBitmapBGR8);
            this.bitmapSource = bitmapSource;
        }

        private async void ShowMessage(string msg)
        {
            MessageDialog dialog = new MessageDialog(msg);
            await dialog.ShowAsync();
        }
    }
}
