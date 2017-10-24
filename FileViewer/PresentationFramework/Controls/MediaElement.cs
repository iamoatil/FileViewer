using System;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
    /// <summary>Represents a control that contains audio and/or video. </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    public class MediaElement : FrameworkElement, IUriContext
    {
        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Source" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Source" /> dependency property.</returns>
        public static readonly DependencyProperty SourceProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Volume" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Volume" /> dependency property.</returns>
        public static readonly DependencyProperty VolumeProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Balance" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Balance" /> dependency property.</returns>
        public static readonly DependencyProperty BalanceProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.IsMuted" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.IsMuted" /> dependency property.</returns>
        public static readonly DependencyProperty IsMutedProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.ScrubbingEnabled" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.ScrubbingEnabled" /> dependency property.</returns>
        public static readonly DependencyProperty ScrubbingEnabledProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.UnloadedBehavior" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.UnloadedBehavior" /> dependency property.</returns>
        public static readonly DependencyProperty UnloadedBehaviorProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.LoadedBehavior" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.LoadedBehavior" /> dependency property.</returns>
        public static readonly DependencyProperty LoadedBehaviorProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Stretch" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Stretch" /> dependency property.</returns>
        public static readonly DependencyProperty StretchProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.StretchDirection" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.StretchDirection" /> dependency property.</returns>
        public static readonly DependencyProperty StretchDirectionProperty;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaFailed" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaFailed" /> dependency property.</returns>
        public static readonly RoutedEvent MediaFailedEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaOpened" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaOpened" /> dependency property.</returns>
        public static readonly RoutedEvent MediaOpenedEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.BufferingStarted" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.BufferingStarted" /> routed event.</returns>
        public static readonly RoutedEvent BufferingStartedEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.BufferingEnded" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.BufferingEnded" /> routed event.</returns>
        public static readonly RoutedEvent BufferingEndedEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> routed event.</returns>
        public static readonly RoutedEvent ScriptCommandEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaEnded" /> routed event.</summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaEnded" /> routed event.</returns>
        public static readonly RoutedEvent MediaEndedEvent;

        private AVElementHelper _helper;

        /// <summary>Occurs when an error is encountered.</summary>
        public event EventHandler<ExceptionRoutedEventArgs> MediaFailed
        {
            add
            {
                base.AddHandler(MediaElement.MediaFailedEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.MediaFailedEvent, value);
            }
        }

        /// <summary>Occurs when media loading has finished.</summary>
        public event RoutedEventHandler MediaOpened
        {
            add
            {
                base.AddHandler(MediaElement.MediaOpenedEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.MediaOpenedEvent, value);
            }
        }

        /// <summary>Occurs when media buffering has begun.</summary>
        public event RoutedEventHandler BufferingStarted
        {
            add
            {
                base.AddHandler(MediaElement.BufferingStartedEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.BufferingStartedEvent, value);
            }
        }

        /// <summary>Occurs when media buffering has ended.</summary>
        public event RoutedEventHandler BufferingEnded
        {
            add
            {
                base.AddHandler(MediaElement.BufferingEndedEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.BufferingEndedEvent, value);
            }
        }

        /// <summary>Occurs when a script command is encountered in the media.</summary>
        public event EventHandler<MediaScriptCommandRoutedEventArgs> ScriptCommand
        {
            add
            {
                base.AddHandler(MediaElement.ScriptCommandEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.ScriptCommandEvent, value);
            }
        }

        /// <summary>Occurs when the media has ended.</summary>
        public event RoutedEventHandler MediaEnded
        {
            add
            {
                base.AddHandler(MediaElement.MediaEndedEvent, value);
            }
            remove
            {
                base.RemoveHandler(MediaElement.MediaEndedEvent, value);
            }
        }

        /// <summary>Gets or sets a media source on the <see cref="T:System.Windows.Controls.MediaElement" />.  </summary>
        /// <returns>The URI that specifies the source of the element. The default is null.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public Uri Source
        {
            get
            {
                return (Uri)base.GetValue(MediaElement.SourceProperty);
            }
            set
            {
                base.SetValue(MediaElement.SourceProperty, value);
            }
        }

        /// <summary>Gets or sets the clock associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> that controls media playback.</summary>
        /// <returns>A clock associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> that controls media playback. The default value is null.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MediaClock Clock
        {
            get
            {
                return this._helper.Clock;
            }
            set
            {
                this._helper.SetClock(value);
            }
        }

        /// <summary>Gets or sets a <see cref="T:System.Windows.Media.Stretch" /> value that describes how a <see cref="T:System.Windows.Controls.MediaElement" /> fills the destination rectangle.  </summary>
        /// <returns>The stretch value for the rendered media. The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
        public Stretch Stretch
        {
            get
            {
                return (Stretch)base.GetValue(MediaElement.StretchProperty);
            }
            set
            {
                base.SetValue(MediaElement.StretchProperty, value);
            }
        }

        /// <summary>Gets or sets a value that determines the restrictions on scaling that are applied to the image.  </summary>
        /// <returns>The value that specifies the direction the element is stretched. The default is <see cref="F:System.Windows.Controls.StretchDirection.Both" />.</returns>
        public StretchDirection StretchDirection
        {
            get
            {
                return (StretchDirection)base.GetValue(MediaElement.StretchDirectionProperty);
            }
            set
            {
                base.SetValue(MediaElement.StretchDirectionProperty, value);
            }
        }

        /// <summary>Gets or sets the media's volume. </summary>
        /// <returns>The media's volume represented on a linear scale between 0 and 1. The default is 0.5.</returns>
        public double Volume
        {
            get
            {
                return (double)base.GetValue(MediaElement.VolumeProperty);
            }
            set
            {
                base.SetValue(MediaElement.VolumeProperty, value);
            }
        }

        /// <summary>Gets or sets a ratio of volume across speakers.  </summary>
        /// <returns>The ratio of volume across speakers in the range between -1 and 1. The default is 0.</returns>
        public double Balance
        {
            get
            {
                return (double)base.GetValue(MediaElement.BalanceProperty);
            }
            set
            {
                base.SetValue(MediaElement.BalanceProperty, value);
            }
        }

        /// <summary>Gets or sets a value indicating whether the audio is muted.  </summary>
        /// <returns>true if audio is muted; otherwise, false. The default is false.</returns>
        public bool IsMuted
        {
            get
            {
                return (bool)base.GetValue(MediaElement.IsMutedProperty);
            }
            set
            {
                base.SetValue(MediaElement.IsMutedProperty, value);
            }
        }

        /// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.MediaElement" /> will update frames for seek operations while paused. </summary>
        /// <returns>true if frames are updated while paused; otherwise, false. The default is false.</returns>
        public bool ScrubbingEnabled
        {
            get
            {
                return (bool)base.GetValue(MediaElement.ScrubbingEnabledProperty);
            }
            set
            {
                base.SetValue(MediaElement.ScrubbingEnabledProperty, value);
            }
        }

        /// <summary>Gets or sets the unload behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media. </summary>
        /// <returns>The unload behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media.</returns>
        public MediaState UnloadedBehavior
        {
            get
            {
                return (MediaState)base.GetValue(MediaElement.UnloadedBehaviorProperty);
            }
            set
            {
                base.SetValue(MediaElement.UnloadedBehaviorProperty, value);
            }
        }

        /// <summary>Gets or sets the load behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media. </summary>
        /// <returns>The load behavior <see cref="T:System.Windows.Controls.MediaState" /> set for the media. The default value is <see cref="F:System.Windows.Controls.MediaState.Play" />.</returns>
        public MediaState LoadedBehavior
        {
            get
            {
                return (MediaState)base.GetValue(MediaElement.LoadedBehaviorProperty);
            }
            set
            {
                base.SetValue(MediaElement.LoadedBehaviorProperty, value);
            }
        }

        /// <summary>Gets a value indicating whether the media can be paused.</summary>
        /// <returns>true if the media can be paused; otherwise, false.</returns>
        public bool CanPause
        {
            get
            {
                return this._helper.Player.CanPause;
            }
        }

        /// <summary>Get a value indicating whether the media is buffering.</summary>
        /// <returns>true if the media is buffering; otherwise, false.</returns>
        public bool IsBuffering
        {
            get
            {
                return this._helper.Player.IsBuffering;
            }
        }

        /// <summary>Gets a percentage value indicating the amount of download completed for content located on a remote server.</summary>
        /// <returns>A percentage value indicating the amount of download completed for content located on a remote server. The value ranges from 0 to 1. The default value is 0.</returns>
        public double DownloadProgress
        {
            get
            {
                return this._helper.Player.DownloadProgress;
            }
        }

        /// <summary>Gets a value that indicates the percentage of buffering progress made.</summary>
        /// <returns>The percentage of buffering completed for streaming content. The value ranges from 0 to 1.</returns>
        public double BufferingProgress
        {
            get
            {
                return this._helper.Player.BufferingProgress;
            }
        }

        /// <summary>Gets the height of the video associated with the media.</summary>
        /// <returns>The height of the video associated with the media. Audio files will return zero.</returns>
        public int NaturalVideoHeight
        {
            get
            {
                return this._helper.Player.NaturalVideoHeight;
            }
        }

        /// <summary>Gets the width of the video associated with the media.</summary>
        /// <returns>The width of the video associated with the media.</returns>
        public int NaturalVideoWidth
        {
            get
            {
                return this._helper.Player.NaturalVideoWidth;
            }
        }

        /// <summary>Gets a value indicating whether the media has audio.</summary>
        /// <returns>true if the media has audio; otherwise, false.</returns>
        public bool HasAudio
        {
            get
            {
                return this._helper.Player.HasAudio;
            }
        }

        /// <summary>Gets a value indicating whether the media has video.</summary>
        /// <returns>true if the media has video; otherwise, false.</returns>
        public bool HasVideo
        {
            get
            {
                return this._helper.Player.HasVideo;
            }
        }

        /// <summary>Gets the natural duration of the media.</summary>
        /// <returns>The natural duration of the media.</returns>
        public Duration NaturalDuration
        {
            get
            {
                return this._helper.Player.NaturalDuration;
            }
        }

        /// <summary>Gets or sets the current position of progress through the media's playback time.</summary>
        /// <returns>The amount of time since the beginning of the media. The default is 00:00:00.</returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public TimeSpan Position
        {
            get
            {
                return this._helper.Position;
            }
            set
            {
                this._helper.SetPosition(value);
            }
        }

        /// <summary>Gets or sets the speed ratio of the media.</summary>
        /// <returns>The speed ratio of the media. The valid range is between 0 (zero) and infinity. Values less than 1 yield slower than normal playback, and values greater than 1 yield faster than normal playback. Negative values are treated as 0. The default value is 1. </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public double SpeedRatio
        {
            get
            {
                return this._helper.SpeedRatio;
            }
            set
            {
                this._helper.SetSpeedRatio(value);
            }
        }

        /// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
        /// <returns>The base URI of the current context.</returns>
        Uri IUriContext.BaseUri
        {
            get
            {
                return this._helper.BaseUri;
            }
            set
            {
                this._helper.BaseUri = value;
            }
        }

        internal AVElementHelper Helper
        {
            get
            {
                return this._helper;
            }
        }

        /// <summary>Instantiates a new instance of the <see cref="T:System.Windows.Controls.MediaElement" /> class.</summary>
        public MediaElement()
        {
            this.Initialize();
        }

        static MediaElement()
        {
            MediaElement.SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MediaElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(AVElementHelper.OnSourceChanged)));
            MediaElement.VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.VolumePropertyChanged)));
            MediaElement.BalanceProperty = DependencyProperty.Register("Balance", typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.BalancePropertyChanged)));
            MediaElement.IsMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.IsMutedPropertyChanged)));
            MediaElement.ScrubbingEnabledProperty = DependencyProperty.Register("ScrubbingEnabled", typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.ScrubbingEnabledPropertyChanged)));
            MediaElement.UnloadedBehaviorProperty = DependencyProperty.Register("UnloadedBehavior", typeof(MediaState), typeof(MediaElement), new FrameworkPropertyMetadata(MediaState.Close, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.UnloadedBehaviorPropertyChanged)));
            MediaElement.LoadedBehaviorProperty = DependencyProperty.Register("LoadedBehavior", typeof(MediaState), typeof(MediaElement), new FrameworkPropertyMetadata(MediaState.Play, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(MediaElement.LoadedBehaviorPropertyChanged)));
            MediaElement.StretchProperty = Viewbox.StretchProperty.AddOwner(typeof(MediaElement));
            MediaElement.StretchDirectionProperty = Viewbox.StretchDirectionProperty.AddOwner(typeof(MediaElement));
            MediaElement.MediaFailedEvent = EventManager.RegisterRoutedEvent("MediaFailed", RoutingStrategy.Bubble, typeof(EventHandler<ExceptionRoutedEventArgs>), typeof(MediaElement));
            MediaElement.MediaOpenedEvent = EventManager.RegisterRoutedEvent("MediaOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
            MediaElement.BufferingStartedEvent = EventManager.RegisterRoutedEvent("BufferingStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
            MediaElement.BufferingEndedEvent = EventManager.RegisterRoutedEvent("BufferingEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
            MediaElement.ScriptCommandEvent = EventManager.RegisterRoutedEvent("ScriptCommand", RoutingStrategy.Bubble, typeof(EventHandler<MediaScriptCommandRoutedEventArgs>), typeof(MediaElement));
            MediaElement.MediaEndedEvent = EventManager.RegisterRoutedEvent("MediaEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
            Style defaultValue = MediaElement.CreateDefaultStyles();
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(defaultValue));
            MediaElement.StretchProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));
            MediaElement.StretchDirectionProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        private static Style CreateDefaultStyles()
        {
            Style expr_10 = new Style(typeof(MediaElement), null);
            expr_10.Setters.Add(new Setter(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight));
            expr_10.Seal();
            return expr_10;
        }

        /// <summary>Plays media from the current position.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public void Play()
        {
            this._helper.SetState(MediaState.Play);
        }

        /// <summary>Pauses media at the current position.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public void Pause()
        {
            this._helper.SetState(MediaState.Pause);
        }

        /// <summary>Stops and resets media to be played from the beginning.</summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
        public void Stop()
        {
            this._helper.SetState(MediaState.Stop);
        }

        /// <summary>Closes the media.</summary>
        public void Close()
        {
            this._helper.SetState(MediaState.Close);
        }

        /// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.MediaElement" />.</summary>
        /// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.MediaElement" />.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MediaElementAutomationPeer(this);
        }

        /// <summary>Updates the <see cref="P:System.Windows.UIElement.DesiredSize" /> of the <see cref="T:System.Windows.Controls.MediaElement" />. This method is called by a parent <see cref="T:System.Windows.UIElement" />. This is the first pass of layout.</summary>
        /// <param name="availableSize">The upper limit the element should not exceed.</param>
        /// <returns>The desired size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            return this.MeasureArrangeHelper(availableSize);
        }

        /// <summary>Arranges and sizes a <see cref="T:System.Windows.Controls.MediaElement" /> control.</summary>
        /// <param name="finalSize">Size used to arrange the control.</param>
        /// <returns>Size of the control.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            return this.MeasureArrangeHelper(finalSize);
        }

        /// <summary>Draws the content of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a <see cref="T:System.Windows.Controls.MediaElement" /> control. </summary>
        /// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> to draw.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this._helper.Player == null)
            {
                return;
            }
            drawingContext.DrawVideo(this._helper.Player, new Rect(default(Point), base.RenderSize));
        }

        private void Initialize()
        {
            this._helper = new AVElementHelper(this);
        }

        private Size MeasureArrangeHelper(Size inputSize)
        {
            MediaPlayer player = this._helper.Player;
            if (player == null)
            {
                return default(Size);
            }
            Size contentSize = new Size((double)player.NaturalVideoWidth, (double)player.NaturalVideoHeight);
            Size size = Viewbox.ComputeScaleFactor(inputSize, contentSize, this.Stretch, this.StretchDirection);
            return new Size(contentSize.Width * size.Width, contentSize.Height * size.Height);
        }

        private static void VolumePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetVolume((double)e.NewValue);
            }
        }

        private static void BalancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetBalance((double)e.NewValue);
            }
        }

        private static void IsMutedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetIsMuted((bool)e.NewValue);
            }
        }

        private static void ScrubbingEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetScrubbingEnabled((bool)e.NewValue);
            }
        }

        private static void UnloadedBehaviorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetUnloadedBehavior((MediaState)e.NewValue);
            }
        }

        private static void LoadedBehaviorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            MediaElement mediaElement = (MediaElement)d;
            if (mediaElement != null)
            {
                mediaElement._helper.SetLoadedBehavior((MediaState)e.NewValue);
            }
        }

        internal void OnMediaFailed(object sender, ExceptionEventArgs args)
        {
            base.RaiseEvent(new ExceptionRoutedEventArgs(MediaElement.MediaFailedEvent, this, args.ErrorException));
        }

        internal void OnMediaOpened(object sender, EventArgs args)
        {
            base.RaiseEvent(new RoutedEventArgs(MediaElement.MediaOpenedEvent, this));
        }

        internal void OnBufferingStarted(object sender, EventArgs args)
        {
            base.RaiseEvent(new RoutedEventArgs(MediaElement.BufferingStartedEvent, this));
        }

        internal void OnBufferingEnded(object sender, EventArgs args)
        {
            base.RaiseEvent(new RoutedEventArgs(MediaElement.BufferingEndedEvent, this));
        }

        internal void OnMediaEnded(object sender, EventArgs args)
        {
            base.RaiseEvent(new RoutedEventArgs(MediaElement.MediaEndedEvent, this));
        }

        internal void OnScriptCommand(object sender, MediaScriptCommandEventArgs args)
        {
            base.RaiseEvent(new MediaScriptCommandRoutedEventArgs(MediaElement.ScriptCommandEvent, this, args.ParameterType, args.ParameterValue));
        }
    }
}