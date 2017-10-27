using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FileViewer.UserControls.PlayerControl
{
    /// <summary>
    /// AudioUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class PlayerUserControlVLC : UserControl
    {
        public PlayerUserControlVLC()
        {
            InitializeComponent();
            TimeSlider.LargeChange = 0.1;
            this.VerticalAlignment =VerticalAlignment.Center;            
        }
        private XLY.XDD.Control.MediaElement _mediaElement;

        /// <summary>
        /// 更新界面进度条的
        /// </summary>
        readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        public void SetMediaElement(XLY.XDD.Control.MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.Volume = 1;

            MediaElementContainer.Children.Add(_mediaElement);
            _mediaElement.Opened += MediaElement_MediaOpened;
        }        

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (this._mediaElement.Length.HasValue)
            {
                this.TimeSlider.Maximum = this._mediaElement.Length.Value.TotalMilliseconds;
                TotalTime.Text = this._mediaElement.Length.Value.ToString("hh':'mm':'ss"); 
            }

            Title.Text = Path.GetFileName(_mediaElement.Source.ToString());

            _dispatcherTimer.Tick += UpdateSliderValueByTimer;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _dispatcherTimer.Start();
        }

        private void UpdateSliderValueByTimer(object sender, EventArgs e)
        {
            if (this._mediaElement.Position.HasValue)
            {
                TimeSlider.Value = this._mediaElement.Position.Value.TotalSeconds;
                StartTime.Text = _mediaElement.Position.Value.ToString("hh':'mm':'ss");
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _mediaElement.Position = TimeSpan.FromSeconds(TimeSlider.Value);
        }
        
        private void TimeSlider_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(TimeSlider);
            TimeSlider.Value = p.X / TimeSlider.ActualWidth * TimeSlider.Maximum;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            _mediaElement.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            _mediaElement.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            _mediaElement.Stop();
        }        
    }
}
