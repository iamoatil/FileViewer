using System;
using System.IO;
using System.Windows;
using FileViewer.UserControls.PlayerControl;

namespace FileViewer.Decoders
{
    class AudioFileDecoder :IFileDecoder
    {
        public AudioFileDecoder()
        {
            _audioUserControl.SetMediaElement(_mediaElement);
        }

        public  FrameworkElement Element
        {
            get
            {
                return _audioUserControl;
            }
        }

        readonly PlayerUserControlVLC _audioUserControl = new PlayerUserControlVLC();
        readonly XLY.XDD.Control.MediaElement _mediaElement = new XLY.XDD.Control.MediaElement();        

        public void Decode(string path)
        {
            path=Path.GetFullPath(path);
            _mediaElement.Open(path);
        }        
    }
}
