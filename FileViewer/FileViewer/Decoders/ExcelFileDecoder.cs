using System.IO;
using System.Windows;
using XLY.XDD.Control;

namespace FileViewer.Decoders
{
    class ExcelFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get
            {
                return _officeViewer;
            }
        }

        readonly ExcelViewer _officeViewer = new ExcelViewer();

        public void Decode(string path)
        {
            path = Path.GetFullPath(path);
            _officeViewer.Open(path);
        }
    }
}
