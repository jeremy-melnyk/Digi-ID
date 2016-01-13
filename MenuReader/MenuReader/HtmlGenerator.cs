using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="fileName">File name to be added to Documents library.</param>
        /// <param name="card">Card stream that will be OCR'd</param>
        /// <param name="replacementPicture">Picture to put as replacement in generated html.</param>
        public HtmlGenerator(string fileName, Stream card, Stream replacementPicture)
        {
            setFilePath(fileName);

            this.ReplacementPicture = replacementPicture;
            this.Card = card;
        }

        public void setFilePath(string fileName)
        {
            StorageFolder storageFolder = KnownFolders.DocumentsLibrary;
            this.htmlFile = storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting).AsTask().Result;
        }

        public void GenerateHtml()
        {
            if (htmlFile == null || Card == null|| ReplacementPicture == null)
            {
                throw new MissingMemberException("File name, card or replacement picture not set.");
            }
            // TODO: Call private methods in appropriate order, using ImageReader to get OCR.
        }

        private void initFile()
        {
            // TODO: Code to generate <html> and all
        }

        private void endFile()
        {
            // TODO: Close the file i.e. </body> and </html>
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
