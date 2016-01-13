using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Vision.Contract;
using Windows.Storage;

namespace MenuReader
{

    class HtmlGenerator
    {
        private StorageFile htmlFile;

        public Stream ReplacementPicture { get; set; }
        public Stream Card { get; set; }

        /// <summary>
        /// Creates an HtmlGenerator.
        /// </summary>
        /// <param name="card">Card stream that will be OCR'd</param>
        /// <param name="replacementPicture">Picture to put as replacement in generated html.</param>
        public HtmlGenerator(Stream card, Stream replacementPicture)
        {
            this.htmlFile = ApplicationData.Current.TemporaryFolder.CreateFileAsync("TEMP_HTML.html", CreationCollisionOption.ReplaceExisting).AsTask().Result;
            this.ReplacementPicture = replacementPicture;
            this.Card = card;
        }

        public async void GenerateHtmlAsync()
        {
            if (htmlFile == null || Card == null || ReplacementPicture == null)
            {
                throw new MissingMemberException("File name, card or replacement picture not set.");
            }
            // TODO: Call private methods in appropriate order, using ImageReader to get OCR.
            Task t = initFileAsync();
            await t;

            ImageReader imageReader = new ImageReader(this.Card);
            OcrResults results = await imageReader.GetImageAnalysisResultsAsync();

            t = endFileAsync();
            await t;
        }

        private async Task initFileAsync()
        {
            string init = "<html>\n" +
                          "  <head>\n" +
                          "    <title>Your new card!</title>\n" +
                          "  </head>\n" +
                          "  <body>\n" +
                          "    <h1>Never forget: with great power comes great responsibility</h1>\n";
            await FileIO.WriteTextAsync(this.htmlFile, init);
        }

        private async Task endFileAsync()
        {
            string close = "  </body>\n</html>";
            await FileIO.AppendTextAsync(this.htmlFile, close);

            this.htmlFile = null;
            this.ReplacementPicture = null;
            this.Card = null;
        }

        private void writeEntity(CardEntity cardEntity)
        {
            // TODO: convert entity to html span
        }

        private void writeIdPicture()
        {
            // TODO: add picture to html with img tag
        }
    }
}
