using System.IO;
using System.Windows;
using XLY.XDD.Control;

namespace FileViewer.Decoders
{
    class PptFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get
            {
                return _officeViewer;
            }
        }

        readonly PptViewer _officeViewer = new PptViewer();

        public void Decode(string path)
        {
            path = Path.GetFullPath(path);
            _officeViewer.Open(path);
        }
    }
}
