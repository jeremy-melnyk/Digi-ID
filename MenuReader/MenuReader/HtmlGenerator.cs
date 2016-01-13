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
            // TODO: Get image dimensions from outside
            string wrapperDiv = "    <div style=\"position: relative; " +
                                                "width: " + 1024 + "px; " +
                                                "height: " + 665 + "px; " +
                                                "border: 1px solid black; border-radius: 10px\">\n";
            await FileIO.AppendTextAsync(this.htmlFile, wrapperDiv);
            BoundaryBox bBox;
            foreach (Region reg in results.Regions)
            {
                foreach (Line line in reg.Lines)
                {
                    foreach (Word word in line.Words)
                    {
                        bBox = new BoundaryBox(word.BoundingBox);
                        string htmlWord = "      <span style=\"position: absolute; " +
                                                        "left: " + bBox.x + "px; " +
                                                        "top: " + bBox.y + "px; " +
                                                        "width: " + bBox.Width + "px; " +
                                                        "height: " + bBox.Height + "px; " +
                                                        "font-size: " + bBox.Height + "px;\">" + /* super-awesome hack */
                                                        word.Text + "</span>\n";
                        await FileIO.AppendTextAsync(this.htmlFile, htmlWord);
                    }
                }
            }
            // TODO: Add picture
            await FileIO.AppendTextAsync(this.htmlFile, "    </div>\n");
            t = endFileAsync();
            await t;
        }

        private async Task initFileAsync()
        {
            string init = "<html>\n" +
                          "  <head>\n" +
                          "    <title>Your new card!</title>\n" +
                          "  </head>\n" +
                          "  <body style=\"font-family: Calibri;\">\n" +
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

        private class BoundaryBox
        {
            public int x { get; set; }
            public int y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public BoundaryBox(string box)
            {
                string[] splitStr = box.Split(new Char[] { ',' });
                this.x = Int32.Parse(splitStr[0]);
                this.y = Int32.Parse(splitStr[1]);
                this.Width = Int32.Parse(splitStr[2]);
                this.Height = Int32.Parse(splitStr[3]);
            }
        }
    }
}
