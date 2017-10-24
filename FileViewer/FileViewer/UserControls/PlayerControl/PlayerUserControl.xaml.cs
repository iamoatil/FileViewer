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
    public partial class PlayerUserControl : UserControl
    {
        public PlayerUserControl()
        {
            InitializeComponent();
            TimeSlider.LargeChange = 0.1;
            this.VerticalAlignment =VerticalAlignment.Center;            
        }
        private MediaElement _mediaElement;

        /// <summary>
        /// 更新界面进度条的
        /// </summary>
        readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer(); 

        public void SetMediaElement(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _mediaElement.Volume = 1;
            MediaElementContainer.Children.Clear();
            MediaElementContainer.Children.Add(_mediaElement);

            _mediaElement.LoadedBehavior = MediaState.Manual;           
            _mediaElement.MediaOpened += MediaElement_MediaOpened;

            Title.Text = Path.GetFileName(_mediaElement.Source.ToString());
        }        

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            TimeSlider.Maximum = _mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
            TotalTime.Text = _mediaElement.NaturalDuration.TimeSpan.ToString("hh':'mm':'ss");           
           
            _dispatcherTimer.Tick += UpdateSliderValueByTimer;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _dispatcherTimer.Start();
        }

        private void UpdateSliderValueByTimer(object sender, EventArgs e)
        {
            TimeSlider.Value = _mediaElement.Position.TotalSeconds;
            StartTime.Text = _mediaElement.Position.ToString("hh':'mm':'ss");
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
