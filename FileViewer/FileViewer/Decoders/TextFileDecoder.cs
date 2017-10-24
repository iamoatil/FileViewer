using System.Windows;
using FileViewer.UserControls.LargeFileTextBox;

namespace FileViewer.Decoders
{
    class TextFileDecoder : IFileDecoder
    {
        public FrameworkElement Element { get { return _textBox; } }
        private readonly TextBoxUserControl _textBox = new TextBoxUserControl();

        public void Decode(string path)
        {
            _textBox.OpenFile(path);
        }
    }
}
