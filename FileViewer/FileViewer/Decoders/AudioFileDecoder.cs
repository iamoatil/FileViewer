using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FileViewer.UserControls.PlayerControl;

namespace FileViewer.Decoders
{
    class AudioFileDecoder :IFileDecoder
    {
        public  FrameworkElement Element
        {
            get
            {
                return _audioUserControl;
            }
        }

        readonly AudioUserControl _audioUserControl = new AudioUserControl();
        readonly MediaElement _mediaElement = new MediaElement();        

        public void Decode(string path)
        {
            path=Path.GetFullPath(path);
            _mediaElement.Source = new Uri(path);
            _audioUserControl.SetMediaElement(_mediaElement);           
        }        
    }
}
