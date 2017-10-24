using System.IO;
using System.Windows;
using XLY.XDD.Control;

namespace FileViewer.Decoders
{
    class PdfFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get
            {
                return _viewer;
            }
        }

        readonly PdfViewer _viewer = new PdfViewer();

        public void Decode(string path)
        {
            path = Path.GetFullPath(path);
            _viewer.Open(path);
        }
    }
}
