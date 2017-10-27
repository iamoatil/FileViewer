using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FileViewer.UserControls.PlayerControl;

namespace FileViewer.Decoders
{
    class VideoFileDecoder : IFileDecoder
    {
        public VideoFileDecoder()
        {
            _videoUserControl.SetMediaElement(_mediaElement);
        }

        public FrameworkElement Element
        {
            get
            {
                return _videoUserControl;
            }
        }

        readonly PlayerUserControlVLC _videoUserControl = new PlayerUserControlVLC();
        readonly XLY.XDD.Control.MediaElement _mediaElement = new XLY.XDD.Control.MediaElement();

        public void Decode(string path)
        {
            path = Path.GetFullPath(path);
            _mediaElement.Open(path);
        }
    }
}
