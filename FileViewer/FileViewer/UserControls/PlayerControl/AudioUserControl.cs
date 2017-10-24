using System.Windows;

/* ==============================================================================
* Description：AudioUserControl  
* Author     ：litao
* Create Date：2017/10/23 15:05:34
* ==============================================================================*/

namespace FileViewer.UserControls.PlayerControl
{
    class AudioUserControl: PlayerUserControl
    {
        public AudioUserControl() : base()
        {
            this.VerticalAlignment = VerticalAlignment.Center;
            MediaElementContainer.Height = 0;
        }
    }
}
