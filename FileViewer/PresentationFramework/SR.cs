using System;
using System.Globalization;
using System.Resources;

namespace System.Windows
{
    internal static class SR
    {
        private static ResourceManager _resourceManager = new ResourceManager("ExceptionStringTable", typeof(SR).Assembly);

        internal static ResourceManager ResourceManager
        {
            get
            {
                return SR._resourceManager;
            }
        }

        internal static string Get(string id)
        {
            string @string = SR._resourceManager.GetString(id);
            if (@string == null)
            {
                @string = SR._resourceManager.GetString("Unavailable");
            }
            return @string;
        }

        internal static string Get(string id, params object[] args)
        {
            string text = SR._resourceManager.GetString(id);
            if (text == null)
            {
                text = SR._resourceManager.GetString("Unavailable");
            }
            else if (args != null && args.Length != 0)
            {
                text = string.Format(CultureInfo.CurrentCulture, text, args);
            }
            return text;
        }
    }
}
