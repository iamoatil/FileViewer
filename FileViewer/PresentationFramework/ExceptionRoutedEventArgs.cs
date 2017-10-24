using System;

namespace System.Windows
{
    /// <summary>Provides data for the  <see cref="T:System.Windows.Controls.Image" /> and <see cref="T:System.Windows.Controls.MediaElement" /> failed events.</summary>
    public sealed class ExceptionRoutedEventArgs : RoutedEventArgs
    {
        private Exception _errorException;

        /// <summary>Gets the exception that caused the error condition.</summary>
        /// <returns>The exception that details the specific error condition.</returns>
        /// <exception cref="T:System.Security.SecurityException">The attempt to access the media file is denied.</exception>
        /// <exception cref="T:System.IO.FileNotFoundException">The media file is not found.</exception>
        /// <exception cref="T:System.IO.FileFormatException">The media file format is not supported by any installed codec.-or-The file format is not recognized.</exception>
        /// <exception cref="T:System.NotSupportedException">Windows Media Player version 10 or later is not detected.-or-Video resources are insufficient for media playback.</exception>
        /// <exception cref="T:System.Runtime.InteropServices.COMException">A COM error code appears.</exception>
        public Exception ErrorException
        {
            get
            {
                return this._errorException;
            }
        }

        internal ExceptionRoutedEventArgs(RoutedEvent routedEvent, object sender, Exception errorException)
            : base(routedEvent, sender)
        {
            if (errorException == null)
            {
                throw new ArgumentNullException("errorException");
            }
            this._errorException = errorException;
        }
    }
}
