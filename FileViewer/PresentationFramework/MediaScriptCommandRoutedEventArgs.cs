using System;

namespace System.Windows
{
    /// <summary>Provides data for the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> and <see cref="E:System.Windows.Media.MediaPlayer.ScriptCommand" /> events.</summary>
    public sealed class MediaScriptCommandRoutedEventArgs : RoutedEventArgs
    {
        private string _parameterType;

        private string _parameterValue;

        /// <summary>Gets the type of script command that was raised.</summary>
        /// <returns>The type of script command that was raised.</returns>
        public string ParameterType
        {
            get
            {
                return this._parameterType;
            }
        }

        /// <summary>Gets the arguments associated with the script command type.</summary>
        /// <returns>The arguments associated with the script command type.</returns>
        public string ParameterValue
        {
            get
            {
                return this._parameterValue;
            }
        }

        internal MediaScriptCommandRoutedEventArgs(RoutedEvent routedEvent, object sender, string parameterType, string parameterValue)
            : base(routedEvent, sender)
        {
            if (parameterType == null)
            {
                throw new ArgumentNullException("parameterType");
            }
            if (parameterValue == null)
            {
                throw new ArgumentNullException("parameterValue");
            }
            this._parameterType = parameterType;
            this._parameterValue = parameterValue;
        }
    }
}
