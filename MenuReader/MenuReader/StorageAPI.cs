using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace MenuReader
{
    class StorageAPI
    {
        public StorageAPI()
        {
        }

        public static async Task<string> SavePhoto(Stream photo, string fileName)
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedFileName = fileName;
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg", ".jpeg", ".png" });
            savePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFile file = await savePicker.PickSaveFileAsync();

            using (var outputFileStream = await file.OpenStreamForWriteAsync())
            {
                await photo.CopyToAsync(outputFileStream);
            }
            return file.Path;
        }

        public static async Task<string> PickPhotoPath()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            StorageFile file = await openPicker.PickSingleFileAsync();
            return file.Path;
        }

        public static async Task<string> SaveStreamAsync(IRandomAccessStream streamToSave, uint width, uint height, string fileName)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.ViewMode = PickerViewMode.List;
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".jpeg");
            folderPicker.FileTypeFilter.Add(".png");
            StorageFolder newFolder = await folderPicker.PickSingleFolderAsync();

            StorageFile destination = await newFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            BitmapTransform transform = new BitmapTransform();

            BitmapDecoder bmpDecoder = await BitmapDecoder.CreateAsync(streamToSave);
            PixelDataProvider pixelData = await bmpDecoder.GetPixelDataAsync(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.DoNotColorManage);
            using (var destFileStream = await destination.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder bmpEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destFileStream);
                bmpEncoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, width, height, 300, 300, pixelData.DetachPixelData());
                await bmpEncoder.FlushAsync();
            }
            return destination.Path;
        }
    }
}
