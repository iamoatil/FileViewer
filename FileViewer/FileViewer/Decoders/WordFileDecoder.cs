using System.IO;
using System.Windows;
using XLY.XDD.Control;

namespace FileViewer.Decoders
{
    class WordFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get
            {
                return _officeViewer;
            }
        }

        readonly WordViewer _officeViewer = new WordViewer();

        public void Decode(string path)
        {
            path = Path.GetFullPath(path);
            _officeViewer.Open(path);
        }
    }
}
