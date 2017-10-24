using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace FileViewer.Decoders
{
    class PictureFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get
            {
                return _image;
            }
        }

        private Image _image = new Image();

        public void Decode(string path)
        {
            try
            {
                _image.Source = new BitmapImage(new Uri(path));
            }
            catch (Exception)
            {
                MessageBox.Show("Can not Open " + path);
            }
        }
    }
}
