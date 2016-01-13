using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace MenuReader
{
    class FaceAPI :INotifyPropertyChanged
    {
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("24c5facd2c594a86afd06050f6a147c5");

        public float width;
        public float height;
        public float left;
        public float top;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void setWidth(float x)
        {
            width = x;
            NotifyPropertyChanged("width");
        }

        public void setHeight(float x)
        {
            height = x;
            NotifyPropertyChanged("height");
        }

        public void setLeft(float x)
        {
            left = x;
            NotifyPropertyChanged("left");
        }

        public void setTop(float x)
        {
            top = x;
            NotifyPropertyChanged("top");
        }

        private async Task<FaceRectangle[]> UploadAndDetectFaces(Stream image)
        {
            try
            {
                Face[] faces = await faceServiceClient.DetectAsync(image);
                IEnumerable<FaceRectangle> faceRects = faces.Select(face => face.FaceRectangle);
                return faceRects.ToArray();
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }

        /*
        public Task<Stream> extractImage(Face face)
        {
            setWidth(face.FaceRectangle.Width);
            setHeight(face.FaceRectangle.Height);
            face.FaceRectangle.
        }
        */
    }
}
