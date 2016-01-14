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

        public string ReplacementPicturePath { get; set; }
        public Stream Card { get; set; }

        public int CardWidth { get; set; }
        public int CardHeight { get; set; }

        /// <summary>
        /// Creates an HtmlGenerator.
        /// </summary>
        /// <param name="card">Card stream that will be OCR'd</param>
        /// <param name="replacementPicture">Picture to put as replacement in generated html.</param>
        public HtmlGenerator(Stream card, int cardWidth, int cardHeight, string replacementPicturePath)
        {
            this.htmlFile = ApplicationData.Current.TemporaryFolder.CreateFileAsync("TEMP_HTML.html", CreationCollisionOption.ReplaceExisting).AsTask().Result;
            this.ReplacementPicturePath = replacementPicturePath;
            this.Card = card;
            this.CardWidth = cardWidth;
            this.CardHeight = cardHeight;
        }

        public async void GenerateHtmlAsync()
        {
            if (htmlFile == null || Card == null || ReplacementPicturePath == null || CardHeight == 0 || CardWidth == 0)
            {
                return;
                //throw new MissingMemberException("File name, card or replacement picture not set.");
            }

            Task t = initFileAsync();
            await t;

            ImageReader imageReader = new ImageReader(this.Card);
            OcrResults results = await imageReader.GetImageAnalysisResultsAsync();
            // Possible backgrounds
            // http://www.userlogos.org/files/backgrounds/gtchamp7/Aqua.jpg
            string wrapperDiv = "    <div style=\"position: relative; " +
                                                "width: " + this.CardWidth + "px; " +
                                                "height: " + this.CardHeight + "px; " +
                                                "border: 1px solid black; border-radius: 10px; "+
                                                "background-image: url('http://www.joycefdn.org/ar/2009/common/images/page/background/Generic_blue_background.jpg')\">\n";
            await FileIO.AppendTextAsync(this.htmlFile, wrapperDiv);
            BoundingBox bBox;

            foreach (Region reg in results.Regions)
            {
                foreach (Line line in reg.Lines)
                {
                    foreach (Word word in line.Words)
                    {
                        bBox = new BoundingBox(word.BoundingBox);
                        string htmlWord = "      <span style=\"position: absolute; " +
                                                        "left: " + bBox.x + "px; " +
                                                        "top: " + bBox.y + "px; " +
                                                        "width: " + bBox.Width + "px; " +
                                                        "height: " + bBox.Height + "px; " +
                                                        "font-size: " + bBox.Height + "px;\">" + /* super-awesome hack */
                                                        word.Text + "</span>\n";
                        await FileIO.AppendTextAsync(this.htmlFile, htmlWord); /* aweful performance but whatever */
                    }
                }
            }
            await writeIdPicture(results.Regions);
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
                          "    <div style=\"color: #2db08f; font-size: 2.3em;\">Never forget: with great power comes great responsibility...</div>\n";
            await FileIO.WriteTextAsync(this.htmlFile, init);

        }

        private async Task endFileAsync()
        {
            string close = "  </body>\n</html>";
            await FileIO.AppendTextAsync(this.htmlFile, close);

            this.htmlFile = null;
            this.ReplacementPicturePath = null;
            this.Card = null;
            this.CardWidth = 0;
            this.CardHeight = 0;
        }

        private async Task writeIdPicture(Region[] regions)
        {
            int totalLeftLength = 0, totalRightLength = 0;
            BoundingBox bBox;
            for (int i = 0; i < regions.Length; i++)
            {
                if (isLeftmostRegion(regions, i))
                {
                    bBox = new BoundingBox(regions[i].BoundingBox);
                    totalLeftLength += (bBox.x * bBox.Height);
                }
                if (isRightmostRegion(regions, i))
                {
                    bBox = new BoundingBox(regions[i].BoundingBox);
                    totalRightLength += ((this.CardWidth - (bBox.x + bBox.Width)) * bBox.Height);
                }
            }

            bool pictureGoesOnTheLeft = totalLeftLength > totalRightLength ? true : false;
            string img;
            if (pictureGoesOnTheLeft)
            {
                img = "<div>PICTURE GOES ON LEFT</div>";
            }
            else 
            {
                img = "<div>PICTURE GOES ON RIGHT</div>";
            }
            // TODO: find minimal box

            // place it
            //string img = "      <img src=\"" + this.ReplacementPicturePath + "\" style=\"position: absolute; " +
            //                                                                            "TODO\"/>";
            await FileIO.AppendTextAsync(this.htmlFile, img);
        }

        private bool isLeftmostRegion(Region[] regions, int targetIndex)
        {
            // it's better for your mental health if you just stop reading here...
            BoundingBox targetBBox = new BoundingBox(regions[targetIndex].BoundingBox);
            BoundingBox otherBBox;
            for (int i = 0; i < regions.Length; i++)
            {
                if (i == targetIndex) continue;
                otherBBox = new BoundingBox(regions[i].BoundingBox);
                if (otherBBox.x < targetBBox.x && boxesOnSameLine(otherBBox, targetBBox))
                {
                    return false;
                }
            }
            return true;
        }

        private bool isRightmostRegion(Region[] regions, int targetIndex)
        {
            BoundingBox targetBBox = new BoundingBox(regions[targetIndex].BoundingBox);
            BoundingBox otherBBox;
            for (int i = 0; i < regions.Length; i++)
            {
                if (i == targetIndex) continue;
                otherBBox = new BoundingBox(regions[i].BoundingBox);
                if (otherBBox.x + otherBBox.Width > targetBBox.x + targetBBox.Width &&
                    boxesOnSameLine(otherBBox, targetBBox)                    
                   )
                {
                    return false;
                }
            }
            return true;
        }

        private bool boxesOnSameLine(BoundingBox otherBBox, BoundingBox targetBBox)
        {
            return (
                     (otherBBox.y > targetBBox.y && otherBBox.y < (targetBBox.y + targetBBox.Height)) ||
                     (targetBBox.y > otherBBox.y && targetBBox.y < (otherBBox.y + otherBBox.Height))
                    );
        }

        private class BoundingBox
        {
            public int x { get; set; }
            public int y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public BoundingBox(string box)
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
