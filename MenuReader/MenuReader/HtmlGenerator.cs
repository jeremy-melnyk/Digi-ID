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
            // Calculate area between regions and left & right borders.
            // The assumption is that the greater the area, the greater the chance that
            // it's the area for the picture.
            int leftArea = 0, rightArea = 0;
            BoundingBox bBox;
            for (int i = 0; i < regions.Length; i++)
            {
                if (isLeftmostRegion(regions, i))
                {
                    bBox = new BoundingBox(regions[i].BoundingBox);
                    leftArea += (bBox.x * bBox.Height);
                }
                if (isRightmostRegion(regions, i))
                {
                    bBox = new BoundingBox(regions[i].BoundingBox);
                    rightArea += ((this.CardWidth - (bBox.x + bBox.Width)) * bBox.Height);
                }
            }

            bool pictureGoesOnTheLeft = leftArea > rightArea;

            // TODO: find minimal box
            BoundingBox pictureBox = findPictureBoundaries(regions, pictureGoesOnTheLeft);
            string img = "    <div style=\"position: absolute; " +
                                                "width: " + pictureBox.Width + "px; " +
                                                "height: " + pictureBox.Height + "px; " +
                                                "border: 1px solid red; border-radius: 10px; " +
                                                "left: " + pictureBox.x + "px; " +
                                                "top : " + pictureBox.y + "px;\">\n";

            // place it
            //string img = "      <img src=\"" + this.ReplacementPicturePath + "\" style=\"position: absolute; " +
            //                                                                            "\"/>";
            await FileIO.AppendTextAsync(this.htmlFile, img);
        }

        private BoundingBox findPictureBoundaries(Region[] regions, bool pictureGoesOnTheLeft)
        {
            int minBoundary = (int) (0.2 * this.CardWidth); /* approx to remove lines that are close to border */
            BoundingBox picBBox = new BoundingBox();
            
            // yuk, I know
            if (pictureGoesOnTheLeft)
            {
                picBBox.x = 0;
                picBBox.y = 0;
                picBBox.Width = 0;
            }
            else
            {
                picBBox.x = this.CardWidth;
                picBBox.y = 0;
            }


            BoundingBox bBox;

            // Initially lowest point on card; will be modified in loop
            // if ever there is some text under the picture
            int bottomBoundary = CardHeight;

            foreach (Region reg in regions)
            {
                foreach (Line line in reg.Lines)
                {
                    bBox = new BoundingBox(line.BoundingBox);
                    // this makes me sick
                    // 1. update picBBox.y (top) if bBox.x doesn't meet minBoundary requirement
                    // 2. 
                    if (pictureGoesOnTheLeft)
                    {
                        if (bBox.x < minBoundary) /* Assumption: Picture for sure doesn't span this line. Now, is this line over or under picture? */
                        {
                            if (bBox.y < (0.5 * this.CardHeight)) /* current line is within top half of card. Thus, lower y. */
                            {
                                picBBox.y = bBox.y + bBox.Height;
                            }
                            else
                            {
                                bottomBoundary = bBox.y; /* assume you're at bottom of where picture goes. */
                                break; 
                            }
                        }
                        else /* This is a good line - i.e. picture spans this line */ 
                        {
                            if (bBox.x < picBBox.Width) /* found a line that reduces possible boundary */
                            {
                                picBBox.Width = bBox.x;
                            }
                        }
                    }
                    else // picture is on the right
                    {
                        if ((this.CardWidth - bBox.x) < minBoundary) /* Assumption: Picture for sure doesn't span this line. Now, is this line over or under picture? */
                        {
                            if (bBox.y < (0.5 * this.CardHeight)) /* current line is within top half of card. Thus, lower y. */
                            {
                                picBBox.y = bBox.y + bBox.Height;
                            }
                            else
                            {
                                bottomBoundary = bBox.y; /* assume you're at bottom of where picture goes. */
                                break;
                            }
                        }
                        else /* This is a good line - i.e. picture spans this line */
                        {
                            if ((this.CardWidth - bBox.x) < picBBox.Width) /* found a line that reduces possible boundary */
                            {
                                picBBox.x = bBox.x;
                            }
                        }
                    }
                }
            }

            // set width & height from what we discovered of x & y
            picBBox.Height = bottomBoundary - picBBox.y;
            if (!pictureGoesOnTheLeft)
            {
                picBBox.Width = this.CardWidth - picBBox.x;
            }
            return picBBox;
        }

        private bool isLeftmostRegion(Region[] regions, int targetIndex)
        {
            // it's probably best for your mental health if you just stop reading here...
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

            public BoundingBox() {}

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
