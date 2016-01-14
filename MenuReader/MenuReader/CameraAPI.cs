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
        CameraCaptureUI captureUI;

        public CameraAPI()
        {
            captureUI = new CameraCaptureUI();
        }

        public async Task<Stream> TakePhoto(CameraCaptureUIPhotoFormat format)
        {
            captureUI.PhotoSettings.Format = format;
            StorageFile photoFile = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (photoFile == null)
            {
                await ShowMessage("Photo not taken.");
                return null;
            }

            return await ConvertToStream(photoFile);
        }

        public async Task<Stream> ConvertToStream(StorageFile photoFile)
        {
            IRandomAccessStream stream = await photoFile.OpenAsync(FileAccessMode.Read);
            return stream.AsStream();
        }

        public async Task<SoftwareBitmap> ConvertToSoftwareBitmap(Stream stream)
        {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            SoftwareBitmap softwareBitmapBGR8 = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            return softwareBitmapBGR8;
        }

        public async Task<SoftwareBitmapSource> ConvertToSoftwareBitmapSource(SoftwareBitmap bitmap)
        {
            SoftwareBitmapSource bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(bitmap);

            return bitmapSource;
        }

        private async Task ShowMessage(string msg)
        {
            MessageDialog dialog = new MessageDialog(msg);
            await dialog.ShowAsync();
        }
    }
}
