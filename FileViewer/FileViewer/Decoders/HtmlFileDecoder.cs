using System;
using System.Windows;
using System.Windows.Controls;

namespace FileViewer.Decoders
{
    class HtmlFileDecoder : IFileDecoder
    {
        public FrameworkElement Element
        {
            get { return webBrowser; }
        }

        WebBrowser webBrowser = new WebBrowser();
        public void Decode(string path)
        {
            webBrowser.Source = new Uri(path);
        }
    }
}
