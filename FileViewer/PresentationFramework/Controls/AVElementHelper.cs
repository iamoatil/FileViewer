using MS.Internal;
using System;
using System.IO.Packaging;
using System.Security;
using System.Windows.Media;

namespace System.Windows.Controls
{
    internal class AVElementHelper
    {
        private MediaPlayer _mediaPlayer;

        private MediaElement _element;

        private Uri _baseUri;

        private MediaState _unloadedBehavior = MediaState.Close;

        private MediaState _loadedBehavior = MediaState.Play;

        private MediaState _currentState = MediaState.Close;

        private bool _isLoaded;

        private SettableState<TimeSpan> _position;

        private SettableState<MediaState> _mediaState;

        private SettableState<Uri> _source;

        private SettableState<MediaClock> _clock;

        private SettableState<double> _speedRatio;

        private SettableState<double> _volume;

        private SettableState<bool> _isMuted;

        private SettableState<double> _balance;

        private SettableState<bool> _isScrubbingEnabled;

        internal MediaPlayer Player
        {
            get
            {
                return this._mediaPlayer;
            }
        }

        internal Uri BaseUri
        {
            get
            {
                return this._baseUri;
            }
            set
            {
                if (value.Scheme != PackUriHelper.UriSchemePack)
                {
                    this._baseUri = value;
                    return;
                }
                this._baseUri = null;
            }
        }

        internal TimeSpan Position
        {
            get
            {
                if (this._currentState == MediaState.Close)
                {
                    return this._position._value;
                }
                return this._mediaPlayer.Position;
            }
        }

        internal MediaClock Clock
        {
            get
            {
                return this._clock._value;
            }
        }

        internal double SpeedRatio
        {
            get
            {
                return this._speedRatio._value;
            }
        }

        internal AVElementHelper(MediaElement element)
        {
            this._element = element;
            this._position = new SettableState<TimeSpan>(new TimeSpan(0L));
            this._mediaState = new SettableState<MediaState>(MediaState.Close);
            this._source = new SettableState<Uri>(null);
            this._clock = new SettableState<MediaClock>(null);
            this._speedRatio = new SettableState<double>(1.0);
            this._volume = new SettableState<double>(0.5);
            this._isMuted = new SettableState<bool>(false);
            this._balance = new SettableState<double>(0.0);
            this._isScrubbingEnabled = new SettableState<bool>(false);
            this._mediaPlayer = new MediaPlayer();
            this.HookEvents();
        }

        internal static AVElementHelper GetHelper(DependencyObject d)
        {
            MediaElement mediaElement = d as MediaElement;
            if (mediaElement != null)
            {
                return mediaElement.Helper;
            }
            throw new ArgumentException(SR.Get("AudioVideo_InvalidDependencyObject"));
        }

        internal void SetUnloadedBehavior(MediaState unloadedBehavior)
        {
            this._unloadedBehavior = unloadedBehavior;
            this.HandleStateChange();
        }

        internal void SetLoadedBehavior(MediaState loadedBehavior)
        {
            this._loadedBehavior = loadedBehavior;
            this.HandleStateChange();
        }

        internal void SetPosition(TimeSpan position)
        {
            this._position._isSet = true;
            this._position._value = position;
            this.HandleStateChange();
        }

        internal void SetClock(MediaClock clock)
        {
            this._clock._value = clock;
            this._clock._isSet = true;
            this.HandleStateChange();
        }

        internal void SetSpeedRatio(double speedRatio)
        {
            this._speedRatio._wasSet = (this._speedRatio._isSet = true);
            this._speedRatio._value = speedRatio;
            this.HandleStateChange();
        }

        internal void SetState(MediaState mediaState)
        {
            if (this._loadedBehavior != MediaState.Manual && this._unloadedBehavior != MediaState.Manual)
            {
                throw new NotSupportedException(SR.Get("AudioVideo_CannotControlMedia"));
            }
            this._mediaState._value = mediaState;
            this._mediaState._isSet = true;
            this.HandleStateChange();
        }

        internal void SetVolume(double volume)
        {
            this._volume._wasSet = (this._volume._isSet = true);
            this._volume._value = volume;
            this.HandleStateChange();
        }

        internal void SetBalance(double balance)
        {
            this._balance._wasSet = (this._balance._isSet = true);
            this._balance._value = balance;
            this.HandleStateChange();
        }

        internal void SetIsMuted(bool isMuted)
        {
            this._isMuted._wasSet = (this._isMuted._isSet = true);
            this._isMuted._value = isMuted;
            this.HandleStateChange();
        }

        internal void SetScrubbingEnabled(bool isScrubbingEnabled)
        {
            this._isScrubbingEnabled._wasSet = (this._isScrubbingEnabled._isSet = true);
            this._isScrubbingEnabled._value = isScrubbingEnabled;
            this.HandleStateChange();
        }

        private void HookEvents()
        {
            this._mediaPlayer.MediaOpened += new EventHandler(this.OnMediaOpened);
            this._mediaPlayer.MediaFailed += new EventHandler<ExceptionEventArgs>(this.OnMediaFailed);
            this._mediaPlayer.BufferingStarted += new EventHandler(this.OnBufferingStarted);
            this._mediaPlayer.BufferingEnded += new EventHandler(this.OnBufferingEnded);
            this._mediaPlayer.MediaEnded += new EventHandler(this.OnMediaEnded);
            this._mediaPlayer.ScriptCommand += new EventHandler<MediaScriptCommandEventArgs>(this.OnScriptCommand);
            this._element.Loaded += new RoutedEventHandler(this.OnLoaded);
            this._element.Unloaded += new RoutedEventHandler(this.OnUnloaded);
        }

        private void HandleStateChange()
        {
            MediaState mediaState = this._mediaState._value;
            bool flag = false;
            bool flag2 = false;
            if (this._isLoaded)
            {
                if (this._clock._value != null)
                {
                    mediaState = MediaState.Manual;
                    flag = true;
                }
                else if (this._loadedBehavior != MediaState.Manual)
                {
                    mediaState = this._loadedBehavior;
                }
                else if (this._source._wasSet)
                {
                    if (this._loadedBehavior != MediaState.Manual)
                    {
                        mediaState = MediaState.Play;
                    }
                    else
                    {
                        flag2 = true;
                    }
                }
            }
            else if (this._unloadedBehavior != MediaState.Manual)
            {
                mediaState = this._unloadedBehavior;
            }
            else
            {
                Invariant.Assert(this._unloadedBehavior == MediaState.Manual);
                if (this._clock._value != null)
                {
                    mediaState = MediaState.Manual;
                    flag = true;
                }
                else
                {
                    flag2 = true;
                }
            }
            bool flag3 = false;
            if (mediaState != MediaState.Close && mediaState != MediaState.Manual)
            {
                Invariant.Assert(!flag);
                if (this._mediaPlayer.Clock != null)
                {
                    this._mediaPlayer.Clock = null;
                }
                if (this._currentState == MediaState.Close || this._source._isSet)
                {
                    if (this._isScrubbingEnabled._wasSet)
                    {
                        this._mediaPlayer.ScrubbingEnabled = this._isScrubbingEnabled._value;
                        this._isScrubbingEnabled._isSet = false;
                    }
                    if (this._clock._value == null)
                    {
                        this._mediaPlayer.Open(this.UriFromSourceUri(this._source._value));
                    }
                    flag3 = true;
                }
            }
            else if (flag)
            {
                if (this._currentState == MediaState.Close || this._clock._isSet)
                {
                    if (this._isScrubbingEnabled._wasSet)
                    {
                        this._mediaPlayer.ScrubbingEnabled = this._isScrubbingEnabled._value;
                        this._isScrubbingEnabled._isSet = false;
                    }
                    this._mediaPlayer.Clock = this._clock._value;
                    this._clock._isSet = false;
                    flag3 = true;
                }
            }
            else if (mediaState == MediaState.Close && this._currentState != MediaState.Close)
            {
                this._mediaPlayer.Clock = null;
                this._mediaPlayer.Close();
                this._currentState = MediaState.Close;
            }
            if (this._currentState != MediaState.Close | flag3)
            {
                if (this._position._isSet)
                {
                    this._mediaPlayer.Position = this._position._value;
                    this._position._isSet = false;
                }
                if (this._volume._isSet || (flag3 && this._volume._wasSet))
                {
                    this._mediaPlayer.Volume = this._volume._value;
                    this._volume._isSet = false;
                }
                if (this._balance._isSet || (flag3 && this._balance._wasSet))
                {
                    this._mediaPlayer.Balance = this._balance._value;
                    this._balance._isSet = false;
                }
                if (this._isMuted._isSet || (flag3 && this._isMuted._wasSet))
                {
                    this._mediaPlayer.IsMuted = this._isMuted._value;
                    this._isMuted._isSet = false;
                }
                if (this._isScrubbingEnabled._isSet)
                {
                    this._mediaPlayer.ScrubbingEnabled = this._isScrubbingEnabled._value;
                    this._isScrubbingEnabled._isSet = false;
                }
                if (mediaState == MediaState.Play && this._source._isSet)
                {
                    this._mediaPlayer.Play();
                    if (!this._speedRatio._wasSet)
                    {
                        this._mediaPlayer.SpeedRatio = 1.0;
                    }
                    this._source._isSet = false;
                    this._mediaState._isSet = false;
                }
                else if (this._currentState != mediaState || (flag2 && this._mediaState._isSet))
                {
                    switch (mediaState)
                    {
                        case MediaState.Manual:
                            goto IL_3BE;
                        case MediaState.Play:
                            this._mediaPlayer.Play();
                            goto IL_3BE;
                        case MediaState.Pause:
                            this._mediaPlayer.Pause();
                            goto IL_3BE;
                        case MediaState.Stop:
                            this._mediaPlayer.Stop();
                            goto IL_3BE;
                    }
                    Invariant.Assert(false, "Unexpected state request.");
                IL_3BE:
                    if (flag2)
                    {
                        this._mediaState._isSet = false;
                    }
                }
                this._currentState = mediaState;
                if (this._speedRatio._isSet || (flag3 && this._speedRatio._wasSet))
                {
                    this._mediaPlayer.SpeedRatio = this._speedRatio._value;
                    this._speedRatio._isSet = false;
                }
            }
        }

        private Uri UriFromSourceUri(Uri sourceUri)
        {
            if (sourceUri != null)
            {
                if (sourceUri.IsAbsoluteUri)
                {
                    return sourceUri;
                }
                if (this.BaseUri != null)
                {
                    return new Uri(this.BaseUri, sourceUri);
                }
            }
            return sourceUri;
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.IsASubPropertyChange)
            {
                return;
            }
            AVElementHelper.GetHelper(d).MemberOnInvalidateSource(e);
        }

        private void MemberOnInvalidateSource(DependencyPropertyChangedEventArgs e)
        {
            if (this._clock._value != null)
            {
                throw new InvalidOperationException(SR.Get("MediaElement_CannotSetSourceOnMediaElementDrivenByClock"));
            }
            this._source._value = (Uri)e.NewValue;
            this._source._wasSet = (this._source._isSet = true);
            this.HandleStateChange();
        }

        private void OnMediaFailed(object sender, ExceptionEventArgs args)
        {
            this._element.OnMediaFailed(sender, args);
        }

        private void OnMediaOpened(object sender, EventArgs args)
        {
            this._element.InvalidateMeasure();
            this._element.OnMediaOpened(sender, args);
        }

        private void OnBufferingStarted(object sender, EventArgs args)
        {
            this._element.OnBufferingStarted(sender, args);
        }

        private void OnBufferingEnded(object sender, EventArgs args)
        {
            this._element.OnBufferingEnded(sender, args);
        }

        private void OnMediaEnded(object sender, EventArgs args)
        {
            this._element.OnMediaEnded(sender, args);
        }

        private void OnScriptCommand(object sender, MediaScriptCommandEventArgs args)
        {
            this._element.OnScriptCommand(sender, args);
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            this._isLoaded = true;
            this.HandleStateChange();
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            this._isLoaded = false;
            this.HandleStateChange();
        }
    }

    internal struct SettableState<T>
    {
        internal T _value;

        internal bool _isSet;

        internal bool _wasSet;

        internal SettableState(T value)
        {
            this._value = value;
            this._isSet = (this._wasSet = false);
        }
    }
}
