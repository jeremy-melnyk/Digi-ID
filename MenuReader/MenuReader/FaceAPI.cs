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
        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("cfa91c3094b445feb7913728d5e996b3");

        public float width;
        public float height;
        public float left;
        public float top;

        public event PropertyChangedEventHandler PropertyChanged;

        public FaceAPI()
        {
            width = 0;
            height = 0;
            left = 0;
            top = 0;
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public float Width
        {
            get { return width; }
            set
            {
                width = value;
                NotifyPropertyChanged("Width");
            }
        }


        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                NotifyPropertyChanged("Height");
            }
        }

        public float Left
        {
            get { return left; }
            set
            {
                left = value;
                NotifyPropertyChanged("Left");
            }
        }

        public float Top
        {
            get { return top; }
            set
            {
                top = value;
                NotifyPropertyChanged("Top");
            }
        }

        public async Task DetectFace(Stream image)
        {
            FaceRectangle[] faceRects = await UploadAndDetectFaces(image);

            if (faceRects.Length > 0)
            {
                PositionRectangle(faceRects[0]);
            }
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

        
        private void PositionRectangle(FaceRectangle faceRect)
        {
            Width = faceRect.Width;
            Height = faceRect.Height;
            Left = faceRect.Left;
            Top = faceRect.Top;
        }

        public void PositionRectangle(int width, int height, int left, int top)
        {
            Width = width;
            Height = height;
            Left = left;
            Top = top;
        }  
    }
}
