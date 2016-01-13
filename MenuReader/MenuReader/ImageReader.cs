using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;

namespace MenuReader
{
    public class ImageReader
    {
        private const string SUBSCRIPTION_KEY = "24c5facd2c594a86afd06050f6a147c5";

        private IVisionServiceClient visionClient;
        private Task<OcrResults> photoResultsTask;
        private Stream _photo;

        public Stream photo
        {
            get
            {
                return _photo;
            }
            set
            {
                if (value != null)
                {
                    this.photoResultsTask = this.visionClient.RecognizeTextAsync(value);
                }
                this._photo = value;
            }
        }

        public ImageReader(Stream photo = null)
        {
            this.visionClient = new VisionServiceClient(SUBSCRIPTION_KEY);
            this.photo = photo;
        }

        public async Task<OcrResults> GetImageAnalysisResultsAsync()
        {
            return await photoResultsTask;
        }
    }
}
