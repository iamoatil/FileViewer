using System;

namespace System.Windows.Markup
{
    internal interface IHaveResources
    {
        ResourceDictionary Resources
        {
            get;
            set;
        }
    }
}
