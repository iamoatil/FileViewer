using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace System.Windows.Input
{
    /// <summary>Provides logical and directional navigation between focusable objects. </summary>
    public sealed class KeyboardNavigation
    {
        private sealed class FocusVisualAdorner : Adorner
        {
            private GeneralTransform _hostToAdornedElement = Transform.Identity;

            private IContentHost _contentHostParent;

            private ContentElement _adornedContentElement;

            private Style _focusVisualStyle;

            private UIElement _adorderChild;

            private UIElementCollection _canvasChildren;

            private ReadOnlyCollection<Rect> _contentRects;

            protected override int VisualChildrenCount
            {
                get
                {
                    return 1;
                }
            }

            private IContentHost ContentHost
            {
                get
                {
                    if (this._adornedContentElement != null && (this._contentHostParent == null || VisualTreeHelper.GetParent(this._contentHostParent as Visual) == null))
                    {
                        this._contentHostParent = ContentHostHelper.FindContentHost(this._adornedContentElement);
                    }
                    return this._contentHostParent;
                }
            }

            public FocusVisualAdorner(UIElement adornedElement, Style focusVisualStyle)
                : base(adornedElement)
            {
                this._adorderChild = new Control
                {
                    Style = focusVisualStyle
                };
                base.IsClipEnabled = true;
                base.IsHitTestVisible = false;
                base.IsEnabled = false;
                base.AddVisualChild(this._adorderChild);
            }

            public FocusVisualAdorner(ContentElement adornedElement, UIElement adornedElementParent, IContentHost contentHostParent, Style focusVisualStyle)
                : base(adornedElementParent)
            {
                this._contentHostParent = contentHostParent;
                this._adornedContentElement = adornedElement;
                this._focusVisualStyle = focusVisualStyle;
                Canvas canvas = new Canvas();
                this._canvasChildren = canvas.Children;
                this._adorderChild = canvas;
                base.AddVisualChild(this._adorderChild);
                base.IsClipEnabled = true;
                base.IsHitTestVisible = false;
                base.IsEnabled = false;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                Size size = default(Size);
                if (this._adornedContentElement == null)
                {
                    size = base.AdornedElement.RenderSize;
                    constraint = size;
                }
                ((UIElement)this.GetVisualChild(0)).Measure(constraint);
                return size;
            }

            protected override Size ArrangeOverride(Size size)
            {
                Size size2 = base.ArrangeOverride(size);
                if (this._adornedContentElement != null)
                {
                    if (this._contentRects == null)
                    {
                        this._canvasChildren.Clear();
                    }
                    else
                    {
                        IContentHost contentHost = this.ContentHost;
                        if (!(contentHost is Visual) || !base.AdornedElement.IsAncestorOf((Visual)contentHost))
                        {
                            this._canvasChildren.Clear();
                            return default(Size);
                        }
                        Rect arg_67_0 = Rect.Empty;
                        IEnumerator<Rect> enumerator = this._contentRects.GetEnumerator();
                        if (this._canvasChildren.Count == this._contentRects.Count)
                        {
                            for (int i = 0; i < this._canvasChildren.Count; i++)
                            {
                                enumerator.MoveNext();
                                Rect rect = enumerator.Current;
                                rect = this._hostToAdornedElement.TransformBounds(rect);
                                Control expr_C4 = (Control)this._canvasChildren[i];
                                expr_C4.Width = rect.Width;
                                expr_C4.Height = rect.Height;
                                Canvas.SetLeft(expr_C4, rect.X);
                                Canvas.SetTop(expr_C4, rect.Y);
                            }
                            this._adorderChild.InvalidateArrange();
                        }
                        else
                        {
                            this._canvasChildren.Clear();
                            while (enumerator.MoveNext())
                            {
                                Rect rect2 = enumerator.Current;
                                rect2 = this._hostToAdornedElement.TransformBounds(rect2);
                                Control control = new Control();
                                control.Style = this._focusVisualStyle;
                                control.Width = rect2.Width;
                                control.Height = rect2.Height;
                                Canvas.SetLeft(control, rect2.X);
                                Canvas.SetTop(control, rect2.Y);
                                this._canvasChildren.Add(control);
                            }
                        }
                    }
                }
                ((UIElement)this.GetVisualChild(0)).Arrange(new Rect(default(Point), size2));
                return size2;
            }

            protected override Visual GetVisualChild(int index)
            {
                if (index == 0)
                {
                    return this._adorderChild;
                }
                throw new ArgumentOutOfRangeException("index", index, SR.Get("Visual_ArgumentOutOfRange"));
            }

            internal override bool NeedsUpdate(Size oldSize)
            {
                if (this._adornedContentElement == null)
                {
                    return !DoubleUtil.AreClose(base.AdornedElement.RenderSize, oldSize);
                }
                ReadOnlyCollection<Rect> contentRects = this._contentRects;
                this._contentRects = null;
                IContentHost contentHost = this.ContentHost;
                if (contentHost != null)
                {
                    this._contentRects = contentHost.GetRectangles(this._adornedContentElement);
                }
                GeneralTransform hostToAdornedElement = this._hostToAdornedElement;
                if (contentHost is Visual && base.AdornedElement.IsAncestorOf((Visual)contentHost))
                {
                    this._hostToAdornedElement = ((Visual)contentHost).TransformToAncestor(base.AdornedElement);
                }
                else
                {
                    this._hostToAdornedElement = Transform.Identity;
                }
                if (hostToAdornedElement != this._hostToAdornedElement && (!(hostToAdornedElement is MatrixTransform) || !(this._hostToAdornedElement is MatrixTransform) || !Matrix.Equals(((MatrixTransform)hostToAdornedElement).Matrix, ((MatrixTransform)this._hostToAdornedElement).Matrix)))
                {
                    return true;
                }
                if (this._contentRects != null && contentRects != null && this._contentRects.Count == contentRects.Count)
                {
                    for (int i = 0; i < contentRects.Count; i++)
                    {
                        if (!DoubleUtil.AreClose(contentRects[i].Size, this._contentRects[i].Size))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return this._contentRects != contentRects;
            }
        }

        internal delegate bool EnterMenuModeEventHandler(object sender, EventArgs e);

        private class WeakReferenceList : DispatcherObject
        {
            private List<WeakReference> _list = new List<WeakReference>(1);

            private bool _isCleanupRequested;

            public int Count
            {
                get
                {
                    return this._list.Count;
                }
            }

            public void Add(object item)
            {
                if (this._list.Count == this._list.Capacity)
                {
                    this.Purge();
                }
                this._list.Add(new WeakReference(item));
            }

            public void Remove(object target)
            {
                bool flag = false;
                for (int i = 0; i < this._list.Count; i++)
                {
                    object target2 = this._list[i].Target;
                    if (target2 != null)
                    {
                        if (target2 == target)
                        {
                            this._list.RemoveAt(i);
                            i--;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.Purge();
                }
            }

            public void Process(Func<object, bool> action)
            {
                bool flag = false;
                for (int i = 0; i < this._list.Count; i++)
                {
                    object target = this._list[i].Target;
                    if (target != null)
                    {
                        if (action(target))
                        {
                            break;
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    this.ScheduleCleanup();
                }
            }

            private void Purge()
            {
                int num = 0;
                int count = this._list.Count;
                for (int i = 0; i < count; i++)
                {
                    if (this._list[i].IsAlive)
                    {
                        this._list[num++] = this._list[i];
                    }
                }
                if (num < count)
                {
                    this._list.RemoveRange(num, count - num);
                    int num2 = num << 1;
                    if (num2 < this._list.Capacity)
                    {
                        this._list.Capacity = num2;
                    }
                }
            }

            private void ScheduleCleanup()
            {
                if (!this._isCleanupRequested)
                {
                    this._isCleanupRequested = true;
                    base.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new DispatcherOperationCallback(delegate(object unused)
                    {
                        lock (this)
                        {
                            this.Purge();
                            this._isCleanupRequested = false;
                        }
                        return null;
                    }), null);
                }
            }
        }

        private static readonly DependencyProperty TabOnceActiveElementProperty = DependencyProperty.RegisterAttached("TabOnceActiveElement", typeof(WeakReference), typeof(KeyboardNavigation));

        internal static readonly DependencyProperty ControlTabOnceActiveElementProperty = DependencyProperty.RegisterAttached("ControlTabOnceActiveElement", typeof(WeakReference), typeof(KeyboardNavigation));

        internal static readonly DependencyProperty DirectionalNavigationMarginProperty = DependencyProperty.RegisterAttached("DirectionalNavigationMargin", typeof(Thickness), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(default(Thickness)));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property.</returns>
        public static readonly DependencyProperty TabIndexProperty = DependencyProperty.RegisterAttached("TabIndex", typeof(int), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(2147483647));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property.</returns>
        public static readonly DependencyProperty IsTabStopProperty = DependencyProperty.RegisterAttached("IsTabStop", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property.</returns>
        [CommonDependencyProperty, CustomCategory("Accessibility"), Localizability(LocalizationCategory.NeverLocalize)]
        public static readonly DependencyProperty TabNavigationProperty = DependencyProperty.RegisterAttached("TabNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), new ValidateValueCallback(KeyboardNavigation.IsValidKeyNavigationMode));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property. </summary>
        /// <returns>The identifier for the  attached property.</returns>
        [CommonDependencyProperty, CustomCategory("Accessibility"), Localizability(LocalizationCategory.NeverLocalize)]
        public static readonly DependencyProperty ControlTabNavigationProperty = DependencyProperty.RegisterAttached("ControlTabNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), new ValidateValueCallback(KeyboardNavigation.IsValidKeyNavigationMode));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property.</returns>
        [CommonDependencyProperty, CustomCategory("Accessibility"), Localizability(LocalizationCategory.NeverLocalize)]
        public static readonly DependencyProperty DirectionalNavigationProperty = DependencyProperty.RegisterAttached("DirectionalNavigation", typeof(KeyboardNavigationMode), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue), new ValidateValueCallback(KeyboardNavigation.IsValidKeyNavigationMode));

        internal static readonly DependencyProperty ShowKeyboardCuesProperty = DependencyProperty.RegisterAttached("ShowKeyboardCues", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, null, new CoerceValueCallback(KeyboardNavigation.CoerceShowKeyboardCues)));

        /// <summary>Identifies the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property.</returns>
        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.RegisterAttached("AcceptsReturn", typeof(bool), typeof(KeyboardNavigation), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        private KeyboardNavigation.WeakReferenceList _weakFocusChangedHandlers = new KeyboardNavigation.WeakReferenceList();

        private static bool _alwaysShowFocusVisual = SystemParameters.KeyboardCues;

        private KeyboardNavigation.FocusVisualAdorner _focusVisualAdornerCache;

        private Key _lastKeyPressed;

        private KeyboardNavigation.WeakReferenceList _weakEnterMenuModeHandlers;

        private bool _win32MenuModeWorkAround;

        private KeyboardNavigation.WeakReferenceList _weakFocusEnterMainFocusScopeHandlers = new KeyboardNavigation.WeakReferenceList();

        private const double BASELINE_DEFAULT = -1.7976931348623157E+308;

        private double _verticalBaseline = -1.7976931348623157E+308;

        private double _horizontalBaseline = -1.7976931348623157E+308;

        private DependencyProperty _navigationProperty;

        private Hashtable _containerHashtable = new Hashtable(10);

        private static object _fakeNull = new object();

        internal event KeyboardFocusChangedEventHandler FocusChanged
        {
            add
            {
                KeyboardNavigation.WeakReferenceList weakFocusChangedHandlers = this._weakFocusChangedHandlers;
                lock (weakFocusChangedHandlers)
                {
                    this._weakFocusChangedHandlers.Add(value);
                }
            }
            remove
            {
                KeyboardNavigation.WeakReferenceList weakFocusChangedHandlers = this._weakFocusChangedHandlers;
                lock (weakFocusChangedHandlers)
                {
                    this._weakFocusChangedHandlers.Remove(value);
                }
            }
        }

        internal event KeyboardNavigation.EnterMenuModeEventHandler EnterMenuMode
        {
            [SecurityCritical, SecurityTreatAsSafe]
            add
            {
                SecurityHelper.DemandUIWindowPermission();
                if (this._weakEnterMenuModeHandlers == null)
                {
                    this._weakEnterMenuModeHandlers = new KeyboardNavigation.WeakReferenceList();
                }
                KeyboardNavigation.WeakReferenceList weakEnterMenuModeHandlers = this._weakEnterMenuModeHandlers;
                lock (weakEnterMenuModeHandlers)
                {
                    this._weakEnterMenuModeHandlers.Add(value);
                }
            }
            remove
            {
                if (this._weakEnterMenuModeHandlers != null)
                {
                    KeyboardNavigation.WeakReferenceList weakEnterMenuModeHandlers = this._weakEnterMenuModeHandlers;
                    lock (weakEnterMenuModeHandlers)
                    {
                        this._weakEnterMenuModeHandlers.Remove(value);
                    }
                }
            }
        }

        internal event EventHandler FocusEnterMainFocusScope
        {
            add
            {
                KeyboardNavigation.WeakReferenceList weakFocusEnterMainFocusScopeHandlers = this._weakFocusEnterMainFocusScopeHandlers;
                lock (weakFocusEnterMainFocusScopeHandlers)
                {
                    this._weakFocusEnterMainFocusScopeHandlers.Add(value);
                }
            }
            remove
            {
                KeyboardNavigation.WeakReferenceList weakFocusEnterMainFocusScopeHandlers = this._weakFocusEnterMainFocusScopeHandlers;
                lock (weakFocusEnterMainFocusScopeHandlers)
                {
                    this._weakFocusEnterMainFocusScopeHandlers.Remove(value);
                }
            }
        }

        internal static bool AlwaysShowFocusVisual
        {
            get
            {
                return KeyboardNavigation._alwaysShowFocusVisual;
            }
            set
            {
                KeyboardNavigation._alwaysShowFocusVisual = value;
            }
        }

        internal static KeyboardNavigation Current
        {
            get
            {
                return FrameworkElement.KeyboardNavigation;
            }
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal KeyboardNavigation()
        {
            InputManager expr_4C = InputManager.Current;
            expr_4C.PostProcessInput += new ProcessInputEventHandler(this.PostProcessInput);
            expr_4C.TranslateAccelerator += new KeyEventHandler(this.TranslateAccelerator);
        }

        internal static DependencyObject GetTabOnceActiveElement(DependencyObject d)
        {
            WeakReference weakReference = (WeakReference)d.GetValue(KeyboardNavigation.TabOnceActiveElementProperty);
            if (weakReference != null && weakReference.IsAlive)
            {
                DependencyObject dependencyObject = weakReference.Target as DependencyObject;
                if (KeyboardNavigation.GetVisualRoot(dependencyObject) == KeyboardNavigation.GetVisualRoot(d))
                {
                    return dependencyObject;
                }
                d.SetValue(KeyboardNavigation.TabOnceActiveElementProperty, null);
            }
            return null;
        }

        internal static void SetTabOnceActiveElement(DependencyObject d, DependencyObject value)
        {
            d.SetValue(KeyboardNavigation.TabOnceActiveElementProperty, new WeakReference(value));
        }

        private static DependencyObject GetControlTabOnceActiveElement(DependencyObject d)
        {
            WeakReference weakReference = (WeakReference)d.GetValue(KeyboardNavigation.ControlTabOnceActiveElementProperty);
            if (weakReference != null && weakReference.IsAlive)
            {
                DependencyObject dependencyObject = weakReference.Target as DependencyObject;
                if (KeyboardNavigation.GetVisualRoot(dependencyObject) == KeyboardNavigation.GetVisualRoot(d))
                {
                    return dependencyObject;
                }
                d.SetValue(KeyboardNavigation.ControlTabOnceActiveElementProperty, null);
            }
            return null;
        }

        private static void SetControlTabOnceActiveElement(DependencyObject d, DependencyObject value)
        {
            d.SetValue(KeyboardNavigation.ControlTabOnceActiveElementProperty, new WeakReference(value));
        }

        private DependencyObject GetActiveElement(DependencyObject d)
        {
            if (this._navigationProperty != KeyboardNavigation.ControlTabNavigationProperty)
            {
                return KeyboardNavigation.GetTabOnceActiveElement(d);
            }
            return KeyboardNavigation.GetControlTabOnceActiveElement(d);
        }

        private void SetActiveElement(DependencyObject d, DependencyObject value)
        {
            if (this._navigationProperty == KeyboardNavigation.TabNavigationProperty)
            {
                KeyboardNavigation.SetTabOnceActiveElement(d, value);
                return;
            }
            KeyboardNavigation.SetControlTabOnceActiveElement(d, value);
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal static Visual GetVisualRoot(DependencyObject d)
        {
            if (d is Visual || d is Visual3D)
            {
                PresentationSource presentationSource = PresentationSource.CriticalFromVisual(d);
                if (presentationSource != null)
                {
                    return presentationSource.RootVisual;
                }
            }
            else
            {
                FrameworkContentElement frameworkContentElement = d as FrameworkContentElement;
                if (frameworkContentElement != null)
                {
                    return KeyboardNavigation.GetVisualRoot(frameworkContentElement.Parent);
                }
            }
            return null;
        }

        private static object CoerceShowKeyboardCues(DependencyObject d, object value)
        {
            if (!SystemParameters.KeyboardCues)
            {
                return value;
            }
            return BooleanBoxes.TrueBox;
        }

        internal void NotifyFocusChanged(object sender, KeyboardFocusChangedEventArgs e)
        {
            this._weakFocusChangedHandlers.Process(delegate(object item)
            {
                KeyboardFocusChangedEventHandler keyboardFocusChangedEventHandler = item as KeyboardFocusChangedEventHandler;
                if (keyboardFocusChangedEventHandler != null)
                {
                    keyboardFocusChangedEventHandler(sender, e);
                }
                return false;
            });
        }

        /// <summary>Set the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> attached property for the specified element. </summary>
        /// <param name="element">The element on which to set the attached property to.</param>
        /// <param name="index">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetTabIndex(DependencyObject element, int index)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.TabIndexProperty, index);
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" />  attached property for the specified element. </summary>
        /// <param name="element">The element from which to read the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabIndex" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static int GetTabIndex(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return KeyboardNavigation.GetTabIndexHelper(element);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property for the specified element. </summary>
        /// <param name="element">The element to which to write the attached property.</param>
        /// <param name="isTabStop">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetIsTabStop(DependencyObject element, bool isTabStop)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.IsTabStopProperty, BooleanBoxes.Box(isTabStop));
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> attached property for the specified element. </summary>
        /// <param name="element">The element from which to read the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.IsTabStop" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsTabStop(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(KeyboardNavigation.IsTabStopProperty);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element on which to set the attached property.</param>
        /// <param name="mode">Property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetTabNavigation(DependencyObject element, KeyboardNavigationMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.TabNavigationProperty, mode);
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element from which to get the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.TabNavigation" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject)), CustomCategory("Accessibility")]
        public static KeyboardNavigationMode GetTabNavigation(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (KeyboardNavigationMode)element.GetValue(KeyboardNavigation.TabNavigationProperty);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element on which to set the attached property.</param>
        /// <param name="mode">The property value to set</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetControlTabNavigation(DependencyObject element, KeyboardNavigationMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.ControlTabNavigationProperty, mode);
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element from which to get the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.ControlTabNavigation" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject)), CustomCategory("Accessibility")]
        public static KeyboardNavigationMode GetControlTabNavigation(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (KeyboardNavigationMode)element.GetValue(KeyboardNavigation.ControlTabNavigationProperty);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element on which to set the attached property.</param>
        /// <param name="mode">Property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetDirectionalNavigation(DependencyObject element, KeyboardNavigationMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.DirectionalNavigationProperty, mode);
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> attached property for the specified element. </summary>
        /// <param name="element">Element from which to get the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.DirectionalNavigation" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject)), CustomCategory("Accessibility")]
        public static KeyboardNavigationMode GetDirectionalNavigation(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (KeyboardNavigationMode)element.GetValue(KeyboardNavigation.DirectionalNavigationProperty);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" />  attached property for the specified element. </summary>
        /// <param name="element">The element to write the attached property to.</param>
        /// <param name="enabled">The property value to set.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        public static void SetAcceptsReturn(DependencyObject element, bool enabled)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(KeyboardNavigation.AcceptsReturnProperty, BooleanBoxes.Box(enabled));
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> attached property for the specified element. </summary>
        /// <param name="element">The element from which to read the attached property.</param>
        /// <returns>The value of the <see cref="P:System.Windows.Input.KeyboardNavigation.AcceptsReturn" /> property.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="element" /> is null.</exception>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetAcceptsReturn(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(KeyboardNavigation.AcceptsReturnProperty);
        }

        private static bool IsValidKeyNavigationMode(object o)
        {
            KeyboardNavigationMode keyboardNavigationMode = (KeyboardNavigationMode)o;
            return keyboardNavigationMode == KeyboardNavigationMode.Contained || keyboardNavigationMode == KeyboardNavigationMode.Continue || keyboardNavigationMode == KeyboardNavigationMode.Cycle || keyboardNavigationMode == KeyboardNavigationMode.None || keyboardNavigationMode == KeyboardNavigationMode.Once || keyboardNavigationMode == KeyboardNavigationMode.Local;
        }

        internal static UIElement GetParentUIElementFromContentElement(ContentElement ce)
        {
            IContentHost contentHost = null;
            return KeyboardNavigation.GetParentUIElementFromContentElement(ce, ref contentHost);
        }

        private static UIElement GetParentUIElementFromContentElement(ContentElement ce, ref IContentHost ichParent)
        {
            if (ce == null)
            {
                return null;
            }
            IContentHost contentHost = ContentHostHelper.FindContentHost(ce);
            if (ichParent == null)
            {
                ichParent = contentHost;
            }
            DependencyObject dependencyObject = contentHost as DependencyObject;
            if (dependencyObject != null)
            {
                UIElement uIElement = dependencyObject as UIElement;
                if (uIElement != null)
                {
                    return uIElement;
                }
                Visual visual = dependencyObject as Visual;
                while (visual != null)
                {
                    visual = (VisualTreeHelper.GetParent(visual) as Visual);
                    UIElement uIElement2 = visual as UIElement;
                    if (uIElement2 != null)
                    {
                        return uIElement2;
                    }
                }
                ContentElement contentElement = dependencyObject as ContentElement;
                if (contentElement != null)
                {
                    return KeyboardNavigation.GetParentUIElementFromContentElement(contentElement, ref ichParent);
                }
            }
            return null;
        }

        internal void HideFocusVisual()
        {
            if (this._focusVisualAdornerCache != null)
            {
                AdornerLayer adornerLayer = VisualTreeHelper.GetParent(this._focusVisualAdornerCache) as AdornerLayer;
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(this._focusVisualAdornerCache);
                }
                this._focusVisualAdornerCache = null;
            }
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal static bool IsKeyboardMostRecentInputDevice()
        {
            return InputManager.Current.MostRecentInputDevice is KeyboardDevice;
        }

        internal static void ShowFocusVisual()
        {
            KeyboardNavigation.Current.ShowFocusVisual(Keyboard.FocusedElement as DependencyObject);
        }

        private void ShowFocusVisual(DependencyObject element)
        {
            this.HideFocusVisual();
            if (!KeyboardNavigation.IsKeyboardMostRecentInputDevice())
            {
                KeyboardNavigation.EnableKeyboardCues(element, false);
            }
            if (KeyboardNavigation.AlwaysShowFocusVisual || KeyboardNavigation.IsKeyboardMostRecentInputDevice())
            {
                FrameworkElement frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
                {
                    AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(frameworkElement);
                    if (adornerLayer == null)
                    {
                        return;
                    }
                    Style style = frameworkElement.FocusVisualStyle;
                    if (style == FrameworkElement.DefaultFocusVisualStyle)
                    {
                        style = (SystemResources.FindResourceInternal(SystemParameters.FocusVisualStyleKey) as Style);
                    }
                    if (style != null)
                    {
                        this._focusVisualAdornerCache = new KeyboardNavigation.FocusVisualAdorner(frameworkElement, style);
                        adornerLayer.Add(this._focusVisualAdornerCache);
                        return;
                    }
                }
                else
                {
                    FrameworkContentElement frameworkContentElement = element as FrameworkContentElement;
                    if (frameworkContentElement != null)
                    {
                        IContentHost contentHost = null;
                        UIElement parentUIElementFromContentElement = KeyboardNavigation.GetParentUIElementFromContentElement(frameworkContentElement, ref contentHost);
                        if (contentHost != null && parentUIElementFromContentElement != null)
                        {
                            AdornerLayer adornerLayer2 = AdornerLayer.GetAdornerLayer(parentUIElementFromContentElement);
                            if (adornerLayer2 != null)
                            {
                                Style style2 = frameworkContentElement.FocusVisualStyle;
                                if (style2 == FrameworkElement.DefaultFocusVisualStyle)
                                {
                                    style2 = (SystemResources.FindResourceInternal(SystemParameters.FocusVisualStyleKey) as Style);
                                }
                                if (style2 != null)
                                {
                                    this._focusVisualAdornerCache = new KeyboardNavigation.FocusVisualAdorner(frameworkContentElement, parentUIElementFromContentElement, contentHost, style2);
                                    adornerLayer2.Add(this._focusVisualAdornerCache);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void UpdateFocusedElement(DependencyObject focusTarget)
        {
            DependencyObject focusScope = FocusManager.GetFocusScope(focusTarget);
            if (focusScope != null && focusScope != focusTarget)
            {
                FocusManager.SetFocusedElement(focusScope, focusTarget as IInputElement);
                Visual visualRoot = KeyboardNavigation.GetVisualRoot(focusTarget);
                if (visualRoot != null && focusScope == visualRoot)
                {
                    KeyboardNavigation.Current.NotifyFocusEnterMainFocusScope(visualRoot, EventArgs.Empty);
                }
            }
        }

        internal void UpdateActiveElement(DependencyObject activeElement)
        {
            this.UpdateActiveElement(activeElement, KeyboardNavigation.TabNavigationProperty);
            this.UpdateActiveElement(activeElement, KeyboardNavigation.ControlTabNavigationProperty);
        }

        private void UpdateActiveElement(DependencyObject activeElement, DependencyProperty dp)
        {
            this._navigationProperty = dp;
            DependencyObject groupParent = this.GetGroupParent(activeElement);
            this.UpdateActiveElement(groupParent, activeElement, dp);
        }

        internal void UpdateActiveElement(DependencyObject container, DependencyObject activeElement)
        {
            this.UpdateActiveElement(container, activeElement, KeyboardNavigation.TabNavigationProperty);
            this.UpdateActiveElement(container, activeElement, KeyboardNavigation.ControlTabNavigationProperty);
        }

        private void UpdateActiveElement(DependencyObject container, DependencyObject activeElement, DependencyProperty dp)
        {
            this._navigationProperty = dp;
            if (activeElement == container)
            {
                return;
            }
            if (this.GetKeyNavigationMode(container) == KeyboardNavigationMode.Once)
            {
                this.SetActiveElement(container, activeElement);
            }
        }

        internal bool Navigate(DependencyObject currentElement, TraversalRequest request)
        {
            return this.Navigate(currentElement, request, Keyboard.Modifiers);
        }

        private bool Navigate(DependencyObject currentElement, TraversalRequest request, ModifierKeys modifierKeys)
        {
            return this.Navigate(currentElement, request, modifierKeys, null);
        }

        private bool Navigate(DependencyObject currentElement, TraversalRequest request, ModifierKeys modifierKeys, DependencyObject firstElement)
        {
            DependencyObject dependencyObject = null;
            switch (request.FocusNavigationDirection)
            {
                case FocusNavigationDirection.Next:
                    this._navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? KeyboardNavigation.ControlTabNavigationProperty : KeyboardNavigation.TabNavigationProperty);
                    dependencyObject = this.GetNextTab(currentElement, this.GetGroupParent(currentElement, true), false);
                    break;
                case FocusNavigationDirection.Previous:
                    this._navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? KeyboardNavigation.ControlTabNavigationProperty : KeyboardNavigation.TabNavigationProperty);
                    dependencyObject = this.GetPrevTab(currentElement, null, false);
                    break;
                case FocusNavigationDirection.First:
                    this._navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? KeyboardNavigation.ControlTabNavigationProperty : KeyboardNavigation.TabNavigationProperty);
                    dependencyObject = this.GetNextTab(null, currentElement, true);
                    break;
                case FocusNavigationDirection.Last:
                    this._navigationProperty = (((modifierKeys & ModifierKeys.Control) == ModifierKeys.Control) ? KeyboardNavigation.ControlTabNavigationProperty : KeyboardNavigation.TabNavigationProperty);
                    dependencyObject = this.GetPrevTab(null, currentElement, true);
                    break;
                case FocusNavigationDirection.Left:
                case FocusNavigationDirection.Right:
                case FocusNavigationDirection.Up:
                case FocusNavigationDirection.Down:
                    this._navigationProperty = KeyboardNavigation.DirectionalNavigationProperty;
                    dependencyObject = this.GetNextInDirection(currentElement, request.FocusNavigationDirection);
                    break;
            }
            if (dependencyObject == null)
            {
                if (request.Wrapped || request.FocusNavigationDirection == FocusNavigationDirection.First || request.FocusNavigationDirection == FocusNavigationDirection.Last)
                {
                    return false;
                }
                if (this.NavigateOutsidePresentationSource(currentElement, request))
                {
                    return true;
                }
                if (request.FocusNavigationDirection == FocusNavigationDirection.Next || request.FocusNavigationDirection == FocusNavigationDirection.Previous)
                {
                    Visual visualRoot = KeyboardNavigation.GetVisualRoot(currentElement);
                    if (visualRoot != null)
                    {
                        return this.Navigate(visualRoot, new TraversalRequest((request.FocusNavigationDirection == FocusNavigationDirection.Next) ? FocusNavigationDirection.First : FocusNavigationDirection.Last));
                    }
                }
                return false;
            }
            else
            {
                IKeyboardInputSink keyboardInputSink = dependencyObject as IKeyboardInputSink;
                if (keyboardInputSink == null)
                {
                    IInputElement expr_15A = dependencyObject as IInputElement;
                    expr_15A.Focus();
                    return expr_15A.IsKeyboardFocusWithin;
                }
                bool flag;
                if (request.FocusNavigationDirection == FocusNavigationDirection.First || request.FocusNavigationDirection == FocusNavigationDirection.Next)
                {
                    flag = keyboardInputSink.TabInto(new TraversalRequest(FocusNavigationDirection.First));
                }
                else if (request.FocusNavigationDirection == FocusNavigationDirection.Last || request.FocusNavigationDirection == FocusNavigationDirection.Previous)
                {
                    flag = keyboardInputSink.TabInto(new TraversalRequest(FocusNavigationDirection.Last));
                }
                else
                {
                    flag = keyboardInputSink.TabInto(new TraversalRequest(request.FocusNavigationDirection)
                    {
                        Wrapped = true
                    });
                }
                if (!flag && firstElement != dependencyObject)
                {
                    flag = this.Navigate(dependencyObject, request, modifierKeys, (firstElement == null) ? dependencyObject : firstElement);
                }
                return flag;
            }
        }

        [SecurityCritical, SecurityTreatAsSafe]
        private bool NavigateOutsidePresentationSource(DependencyObject currentElement, TraversalRequest request)
        {
            Visual visual = currentElement as Visual;
            if (visual == null)
            {
                visual = KeyboardNavigation.GetParentUIElementFromContentElement(currentElement as ContentElement);
                if (visual == null)
                {
                    return false;
                }
            }
            IKeyboardInputSink keyboardInputSink = PresentationSource.CriticalFromVisual(visual) as IKeyboardInputSink;
            if (keyboardInputSink != null)
            {
                IKeyboardInputSite keyboardInputSite = null;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    keyboardInputSite = keyboardInputSink.KeyboardInputSite;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (keyboardInputSite != null && this.ShouldNavigateOutsidePresentationSource(currentElement, request))
                {
                    return keyboardInputSite.OnNoMoreTabStops(request);
                }
            }
            return false;
        }

        private bool ShouldNavigateOutsidePresentationSource(DependencyObject currentElement, TraversalRequest request)
        {
            if (request.FocusNavigationDirection == FocusNavigationDirection.Left || request.FocusNavigationDirection == FocusNavigationDirection.Right || request.FocusNavigationDirection == FocusNavigationDirection.Up || request.FocusNavigationDirection == FocusNavigationDirection.Down)
            {
                DependencyObject groupParent;
                while ((groupParent = this.GetGroupParent(currentElement)) != null && groupParent != currentElement)
                {
                    KeyboardNavigationMode keyNavigationMode = this.GetKeyNavigationMode(groupParent);
                    if (keyNavigationMode == KeyboardNavigationMode.Contained || keyNavigationMode == KeyboardNavigationMode.Cycle)
                    {
                        return false;
                    }
                    currentElement = groupParent;
                }
            }
            return true;
        }

        [SecurityCritical]
        private void PostProcessInput(object sender, ProcessInputEventArgs e)
        {
            this.ProcessInput(e.StagingItem.Input);
        }

        [SecurityCritical]
        private void TranslateAccelerator(object sender, KeyEventArgs e)
        {
            this.ProcessInput(e);
        }

        [SecurityCritical]
        private void ProcessInput(InputEventArgs inputEventArgs)
        {
            this.ProcessForMenuMode(inputEventArgs);
            this.ProcessForUIState(inputEventArgs);
            if (inputEventArgs.RoutedEvent != Keyboard.KeyDownEvent)
            {
                return;
            }
            KeyEventArgs keyEventArgs = (KeyEventArgs)inputEventArgs;
            if (keyEventArgs.Handled)
            {
                return;
            }
            DependencyObject dependencyObject = keyEventArgs.OriginalSource as DependencyObject;
            DependencyObject dependencyObject2 = keyEventArgs.KeyboardDevice.Target as DependencyObject;
            if (dependencyObject2 != null && dependencyObject != dependencyObject2 && dependencyObject is HwndHost)
            {
                dependencyObject = dependencyObject2;
            }
            if (dependencyObject == null)
            {
                HwndSource hwndSource = keyEventArgs.UnsafeInputSource as HwndSource;
                if (hwndSource == null)
                {
                    return;
                }
                dependencyObject = hwndSource.RootVisual;
                if (dependencyObject == null)
                {
                    return;
                }
            }
            Key realKey = this.GetRealKey(keyEventArgs);
            if (realKey <= Key.Down)
            {
                if (realKey != Key.Tab)
                {
                    switch (realKey)
                    {
                        case Key.Left:
                        case Key.Up:
                        case Key.Right:
                        case Key.Down:
                            break;
                        default:
                            goto IL_C7;
                    }
                }
                KeyboardNavigation.ShowFocusVisual();
            }
            else if (realKey == Key.LeftAlt || realKey == Key.RightAlt)
            {
                KeyboardNavigation.ShowFocusVisual();
                KeyboardNavigation.EnableKeyboardCues(dependencyObject, true);
            }
        IL_C7:
            keyEventArgs.Handled = this.Navigate(dependencyObject, keyEventArgs.Key, keyEventArgs.KeyboardDevice.Modifiers);
        }

        internal static void EnableKeyboardCues(DependencyObject element, bool enable)
        {
            Visual visual = element as Visual;
            if (visual == null)
            {
                visual = KeyboardNavigation.GetParentUIElementFromContentElement(element as ContentElement);
                if (visual == null)
                {
                    return;
                }
            }
            Visual visualRoot = KeyboardNavigation.GetVisualRoot(visual);
            if (visualRoot != null)
            {
                visualRoot.SetValue(KeyboardNavigation.ShowKeyboardCuesProperty, enable ? BooleanBoxes.TrueBox : BooleanBoxes.FalseBox);
            }
        }

        internal static FocusNavigationDirection KeyToTraversalDirection(Key key)
        {
            switch (key)
            {
                case Key.Left:
                    return FocusNavigationDirection.Left;
                case Key.Up:
                    return FocusNavigationDirection.Up;
                case Key.Right:
                    return FocusNavigationDirection.Right;
                case Key.Down:
                    return FocusNavigationDirection.Down;
                default:
                    throw new NotSupportedException();
            }
        }

        internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction)
        {
            return this.PredictFocusedElement(sourceElement, direction, false);
        }

        internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation)
        {
            return this.PredictFocusedElement(sourceElement, direction, treeViewNavigation, true);
        }

        internal DependencyObject PredictFocusedElement(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, bool considerDescendants)
        {
            if (sourceElement == null)
            {
                return null;
            }
            this._navigationProperty = KeyboardNavigation.DirectionalNavigationProperty;
            this._verticalBaseline = -1.7976931348623157E+308;
            this._horizontalBaseline = -1.7976931348623157E+308;
            return this.GetNextInDirection(sourceElement, direction, treeViewNavigation, considerDescendants);
        }

        internal DependencyObject PredictFocusedElementAtViewportEdge(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, FrameworkElement viewportBoundsElement, DependencyObject container)
        {
            DependencyObject result;
            try
            {
                this._containerHashtable.Clear();
                result = this.PredictFocusedElementAtViewportEdgeRecursive(sourceElement, direction, treeViewNavigation, viewportBoundsElement, container);
            }
            finally
            {
                this._containerHashtable.Clear();
            }
            return result;
        }

        private DependencyObject PredictFocusedElementAtViewportEdgeRecursive(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, FrameworkElement viewportBoundsElement, DependencyObject container)
        {
            this._navigationProperty = KeyboardNavigation.DirectionalNavigationProperty;
            this._verticalBaseline = -1.7976931348623157E+308;
            this._horizontalBaseline = -1.7976931348623157E+308;
            if (container == null)
            {
                container = this.GetGroupParent(sourceElement);
            }
            if (container == sourceElement)
            {
                return null;
            }
            if (this.IsEndlessLoop(sourceElement, container))
            {
                return null;
            }
            DependencyObject dependencyObject = this.FindElementAtViewportEdge(sourceElement, viewportBoundsElement, container, direction, treeViewNavigation);
            if (dependencyObject != null)
            {
                if (this.IsElementEligible(dependencyObject, treeViewNavigation))
                {
                    return dependencyObject;
                }
                DependencyObject sourceElement2 = dependencyObject;
                dependencyObject = this.PredictFocusedElementAtViewportEdgeRecursive(sourceElement, direction, treeViewNavigation, viewportBoundsElement, dependencyObject);
                if (dependencyObject != null)
                {
                    return dependencyObject;
                }
                dependencyObject = this.PredictFocusedElementAtViewportEdgeRecursive(sourceElement2, direction, treeViewNavigation, viewportBoundsElement, null);
            }
            return dependencyObject;
        }

        internal bool Navigate(DependencyObject sourceElement, Key key, ModifierKeys modifiers)
        {
            bool result = false;
            if (key != Key.Tab)
            {
                switch (key)
                {
                    case Key.Left:
                        result = this.Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Left), modifiers);
                        break;
                    case Key.Up:
                        result = this.Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Up), modifiers);
                        break;
                    case Key.Right:
                        result = this.Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Right), modifiers);
                        break;
                    case Key.Down:
                        result = this.Navigate(sourceElement, new TraversalRequest(FocusNavigationDirection.Down), modifiers);
                        break;
                }
            }
            else
            {
                result = this.Navigate(sourceElement, new TraversalRequest(((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next), modifiers);
            }
            return result;
        }

        private bool IsInNavigationTree(DependencyObject visual)
        {
            UIElement uIElement = visual as UIElement;
            if (uIElement != null && uIElement.IsVisible)
            {
                return true;
            }
            if (visual is IContentHost && !(visual is UIElementIsland))
            {
                return true;
            }
            UIElement3D uIElement3D = visual as UIElement3D;
            return uIElement3D != null && uIElement3D.IsVisible;
        }

        private DependencyObject GetPreviousSibling(DependencyObject e)
        {
            DependencyObject parent = this.GetParent(e);
            IContentHost contentHost = parent as IContentHost;
            if (contentHost != null)
            {
                IInputElement inputElement = null;
                IEnumerator<IInputElement> hostedElements = contentHost.HostedElements;
                while (hostedElements.MoveNext())
                {
                    IInputElement current = hostedElements.Current;
                    if (current == e)
                    {
                        return inputElement as DependencyObject;
                    }
                    if (current is UIElement || current is UIElement3D)
                    {
                        inputElement = current;
                    }
                    else
                    {
                        ContentElement contentElement = current as ContentElement;
                        if (contentElement != null && this.IsTabStop(contentElement))
                        {
                            inputElement = current;
                        }
                    }
                }
                return null;
            }
            DependencyObject dependencyObject = parent as UIElement;
            if (dependencyObject == null)
            {
                dependencyObject = (parent as UIElement3D);
            }
            DependencyObject dependencyObject2 = e as Visual;
            if (dependencyObject2 == null)
            {
                dependencyObject2 = (e as Visual3D);
            }
            if (dependencyObject != null && dependencyObject2 != null)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                DependencyObject result = null;
                for (int i = 0; i < childrenCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child == dependencyObject2)
                    {
                        break;
                    }
                    if (this.IsInNavigationTree(child))
                    {
                        result = child;
                    }
                }
                return result;
            }
            return null;
        }

        private DependencyObject GetNextSibling(DependencyObject e)
        {
            DependencyObject parent = this.GetParent(e);
            IContentHost contentHost = parent as IContentHost;
            if (contentHost != null)
            {
                IEnumerator<IInputElement> hostedElements = contentHost.HostedElements;
                bool flag = false;
                while (hostedElements.MoveNext())
                {
                    IInputElement current = hostedElements.Current;
                    if (flag)
                    {
                        if (current is UIElement || current is UIElement3D)
                        {
                            return current as DependencyObject;
                        }
                        ContentElement contentElement = current as ContentElement;
                        if (contentElement != null && this.IsTabStop(contentElement))
                        {
                            return contentElement;
                        }
                    }
                    else if (current == e)
                    {
                        flag = true;
                    }
                }
            }
            else
            {
                DependencyObject dependencyObject = parent as UIElement;
                if (dependencyObject == null)
                {
                    dependencyObject = (parent as UIElement3D);
                }
                DependencyObject dependencyObject2 = e as Visual;
                if (dependencyObject2 == null)
                {
                    dependencyObject2 = (e as Visual3D);
                }
                if (dependencyObject != null && dependencyObject2 != null)
                {
                    int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
                    int i = 0;
                    while (i < childrenCount && VisualTreeHelper.GetChild(dependencyObject, i) != dependencyObject2)
                    {
                        i++;
                    }
                    for (i++; i < childrenCount; i++)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                        if (this.IsInNavigationTree(child))
                        {
                            return child;
                        }
                    }
                }
            }
            return null;
        }

        private DependencyObject FocusedElement(DependencyObject e)
        {
            IInputElement inputElement = e as IInputElement;
            if (inputElement != null && !inputElement.IsKeyboardFocusWithin)
            {
                DependencyObject dependencyObject = FocusManager.GetFocusedElement(e) as DependencyObject;
                if (dependencyObject != null && (this._navigationProperty == KeyboardNavigation.ControlTabNavigationProperty || !this.IsFocusScope(e)))
                {
                    Visual visual = dependencyObject as Visual;
                    if (visual == null)
                    {
                        Visual3D visual3D = dependencyObject as Visual3D;
                        if (visual3D == null)
                        {
                            visual = KeyboardNavigation.GetParentUIElementFromContentElement(dependencyObject as ContentElement);
                        }
                        else if (visual3D != e && visual3D.IsDescendantOf(e))
                        {
                            return dependencyObject;
                        }
                    }
                    if (visual != null && visual != e && visual.IsDescendantOf(e))
                    {
                        return dependencyObject;
                    }
                }
            }
            return null;
        }

        private DependencyObject GetFirstChild(DependencyObject e)
        {
            DependencyObject dependencyObject = this.FocusedElement(e);
            if (dependencyObject != null)
            {
                return dependencyObject;
            }
            IContentHost contentHost = e as IContentHost;
            if (contentHost != null)
            {
                IEnumerator<IInputElement> hostedElements = contentHost.HostedElements;
                while (hostedElements.MoveNext())
                {
                    IInputElement current = hostedElements.Current;
                    if (current is UIElement || current is UIElement3D)
                    {
                        return current as DependencyObject;
                    }
                    ContentElement contentElement = current as ContentElement;
                    if (contentElement != null && this.IsTabStop(contentElement))
                    {
                        return contentElement;
                    }
                }
                return null;
            }
            DependencyObject dependencyObject2 = e as UIElement;
            if (dependencyObject2 == null)
            {
                dependencyObject2 = (e as UIElement3D);
            }
            if (dependencyObject2 == null || UIElementHelper.IsVisible(dependencyObject2))
            {
                DependencyObject dependencyObject3 = e as Visual;
                if (dependencyObject3 == null)
                {
                    dependencyObject3 = (e as Visual3D);
                }
                if (dependencyObject3 != null)
                {
                    int childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject3);
                    for (int i = 0; i < childrenCount; i++)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(dependencyObject3, i);
                        if (this.IsInNavigationTree(child))
                        {
                            return child;
                        }
                        DependencyObject firstChild = this.GetFirstChild(child);
                        if (firstChild != null)
                        {
                            return firstChild;
                        }
                    }
                }
            }
            return null;
        }

        private DependencyObject GetLastChild(DependencyObject e)
        {
            DependencyObject dependencyObject = this.FocusedElement(e);
            if (dependencyObject != null)
            {
                return dependencyObject;
            }
            IContentHost contentHost = e as IContentHost;
            if (contentHost != null)
            {
                IEnumerator<IInputElement> hostedElements = contentHost.HostedElements;
                IInputElement inputElement = null;
                while (hostedElements.MoveNext())
                {
                    IInputElement current = hostedElements.Current;
                    if (current is UIElement || current is UIElement3D)
                    {
                        inputElement = current;
                    }
                    else
                    {
                        ContentElement contentElement = current as ContentElement;
                        if (contentElement != null && this.IsTabStop(contentElement))
                        {
                            inputElement = current;
                        }
                    }
                }
                return inputElement as DependencyObject;
            }
            DependencyObject dependencyObject2 = e as UIElement;
            if (dependencyObject2 == null)
            {
                dependencyObject2 = (e as UIElement3D);
            }
            if (dependencyObject2 == null || UIElementHelper.IsVisible(dependencyObject2))
            {
                DependencyObject dependencyObject3 = e as Visual;
                if (dependencyObject3 == null)
                {
                    dependencyObject3 = (e as Visual3D);
                }
                if (dependencyObject3 != null)
                {
                    for (int i = VisualTreeHelper.GetChildrenCount(dependencyObject3) - 1; i >= 0; i--)
                    {
                        DependencyObject child = VisualTreeHelper.GetChild(dependencyObject3, i);
                        if (this.IsInNavigationTree(child))
                        {
                            return child;
                        }
                        DependencyObject lastChild = this.GetLastChild(child);
                        if (lastChild != null)
                        {
                            return lastChild;
                        }
                    }
                }
            }
            return null;
        }

        private DependencyObject GetParent(DependencyObject e)
        {
            if (e is Visual || e is Visual3D)
            {
                DependencyObject dependencyObject = e;
                while ((dependencyObject = VisualTreeHelper.GetParent(dependencyObject)) != null)
                {
                    if (this.IsInNavigationTree(dependencyObject))
                    {
                        return dependencyObject;
                    }
                }
            }
            else
            {
                ContentElement contentElement = e as ContentElement;
                if (contentElement != null)
                {
                    return ContentHostHelper.FindContentHost(contentElement) as DependencyObject;
                }
            }
            return null;
        }

        private DependencyObject GetNextInTree(DependencyObject e, DependencyObject container)
        {
            DependencyObject dependencyObject = null;
            if (e == container || !this.IsGroup(e))
            {
                dependencyObject = this.GetFirstChild(e);
            }
            if (dependencyObject != null || e == container)
            {
                return dependencyObject;
            }
            DependencyObject dependencyObject2 = e;
            DependencyObject nextSibling;
            while (true)
            {
                nextSibling = this.GetNextSibling(dependencyObject2);
                if (nextSibling != null)
                {
                    break;
                }
                dependencyObject2 = this.GetParent(dependencyObject2);
                if (dependencyObject2 == null || dependencyObject2 == container)
                {
                    goto IL_3E;
                }
            }
            return nextSibling;
        IL_3E:
            return null;
        }

        private DependencyObject GetPreviousInTree(DependencyObject e, DependencyObject container)
        {
            if (e == container)
            {
                return null;
            }
            DependencyObject previousSibling = this.GetPreviousSibling(e);
            if (previousSibling == null)
            {
                return this.GetParent(e);
            }
            if (this.IsGroup(previousSibling))
            {
                return previousSibling;
            }
            return this.GetLastInTree(previousSibling);
        }

        private DependencyObject GetLastInTree(DependencyObject container)
        {
            DependencyObject result;
            do
            {
                result = container;
                container = this.GetLastChild(container);
            }
            while (container != null && !this.IsGroup(container));
            if (container != null)
            {
                return container;
            }
            return result;
        }

        private DependencyObject GetGroupParent(DependencyObject e)
        {
            return this.GetGroupParent(e, false);
        }

        private DependencyObject GetGroupParent(DependencyObject e, bool includeCurrent)
        {
            DependencyObject result = e;
            if (!includeCurrent)
            {
                result = e;
                e = this.GetParent(e);
                if (e == null)
                {
                    return result;
                }
            }
            while (e != null)
            {
                if (this.IsGroup(e))
                {
                    return e;
                }
                result = e;
                e = this.GetParent(e);
            }
            return result;
        }

        private bool IsTabStop(DependencyObject e)
        {
            FrameworkElement frameworkElement = e as FrameworkElement;
            if (frameworkElement != null)
            {
                return frameworkElement.Focusable && (bool)frameworkElement.GetValue(KeyboardNavigation.IsTabStopProperty) && frameworkElement.IsEnabled && frameworkElement.IsVisible;
            }
            FrameworkContentElement frameworkContentElement = e as FrameworkContentElement;
            return frameworkContentElement != null && frameworkContentElement.Focusable && (bool)frameworkContentElement.GetValue(KeyboardNavigation.IsTabStopProperty) && frameworkContentElement.IsEnabled;
        }

        private bool IsGroup(DependencyObject e)
        {
            return this.GetKeyNavigationMode(e) > KeyboardNavigationMode.Continue;
        }

        internal bool IsFocusableInternal(DependencyObject element)
        {
            UIElement uIElement = element as UIElement;
            if (uIElement != null)
            {
                return uIElement.Focusable && uIElement.IsEnabled && uIElement.IsVisible;
            }
            ContentElement contentElement = element as ContentElement;
            return contentElement != null && (contentElement != null && contentElement.Focusable) && contentElement.IsEnabled;
        }

        private bool IsElementEligible(DependencyObject element, bool treeViewNavigation)
        {
            if (treeViewNavigation)
            {
                return element is TreeViewItem && this.IsFocusableInternal(element);
            }
            return this.IsTabStop(element);
        }

        private bool IsGroupElementEligible(DependencyObject element, bool treeViewNavigation)
        {
            if (treeViewNavigation)
            {
                return element is TreeViewItem && this.IsFocusableInternal(element);
            }
            return this.IsTabStopOrGroup(element);
        }

        private KeyboardNavigationMode GetKeyNavigationMode(DependencyObject e)
        {
            return (KeyboardNavigationMode)e.GetValue(this._navigationProperty);
        }

        private bool IsTabStopOrGroup(DependencyObject e)
        {
            return this.IsTabStop(e) || this.IsGroup(e);
        }

        private static int GetTabIndexHelper(DependencyObject d)
        {
            return (int)d.GetValue(KeyboardNavigation.TabIndexProperty);
        }

        internal DependencyObject GetFirstTabInGroup(DependencyObject container)
        {
            DependencyObject dependencyObject = null;
            int num = -2147483648;
            DependencyObject dependencyObject2 = container;
            while ((dependencyObject2 = this.GetNextInTree(dependencyObject2, container)) != null)
            {
                if (this.IsTabStopOrGroup(dependencyObject2))
                {
                    int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(dependencyObject2);
                    if (tabIndexHelper < num || dependencyObject == null)
                    {
                        num = tabIndexHelper;
                        dependencyObject = dependencyObject2;
                    }
                }
            }
            return dependencyObject;
        }

        private DependencyObject GetNextTabWithSameIndex(DependencyObject e, DependencyObject container)
        {
            int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(e);
            DependencyObject dependencyObject = e;
            while ((dependencyObject = this.GetNextInTree(dependencyObject, container)) != null)
            {
                if (this.IsTabStopOrGroup(dependencyObject) && KeyboardNavigation.GetTabIndexHelper(dependencyObject) == tabIndexHelper)
                {
                    return dependencyObject;
                }
            }
            return null;
        }

        private DependencyObject GetNextTabWithNextIndex(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
        {
            DependencyObject dependencyObject = null;
            DependencyObject dependencyObject2 = null;
            int num = -2147483648;
            int num2 = -2147483648;
            int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(e);
            DependencyObject dependencyObject3 = container;
            while ((dependencyObject3 = this.GetNextInTree(dependencyObject3, container)) != null)
            {
                if (this.IsTabStopOrGroup(dependencyObject3))
                {
                    int tabIndexHelper2 = KeyboardNavigation.GetTabIndexHelper(dependencyObject3);
                    if (tabIndexHelper2 > tabIndexHelper && (tabIndexHelper2 < num2 || dependencyObject == null))
                    {
                        num2 = tabIndexHelper2;
                        dependencyObject = dependencyObject3;
                    }
                    if (tabIndexHelper2 < num || dependencyObject2 == null)
                    {
                        num = tabIndexHelper2;
                        dependencyObject2 = dependencyObject3;
                    }
                }
            }
            if (tabbingType == KeyboardNavigationMode.Cycle && dependencyObject == null)
            {
                dependencyObject = dependencyObject2;
            }
            return dependencyObject;
        }

        private DependencyObject GetNextTabInGroup(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
        {
            if (tabbingType == KeyboardNavigationMode.None)
            {
                return null;
            }
            if (e == null || e == container)
            {
                return this.GetFirstTabInGroup(container);
            }
            if (tabbingType == KeyboardNavigationMode.Once)
            {
                return null;
            }
            DependencyObject nextTabWithSameIndex = this.GetNextTabWithSameIndex(e, container);
            if (nextTabWithSameIndex != null)
            {
                return nextTabWithSameIndex;
            }
            return this.GetNextTabWithNextIndex(e, container, tabbingType);
        }

        private DependencyObject GetNextTab(DependencyObject e, DependencyObject container, bool goDownOnly)
        {
            KeyboardNavigationMode keyNavigationMode = this.GetKeyNavigationMode(container);
            if (e == null)
            {
                if (this.IsTabStop(container))
                {
                    return container;
                }
                DependencyObject activeElement = this.GetActiveElement(container);
                if (activeElement != null)
                {
                    return this.GetNextTab(null, activeElement, true);
                }
            }
            else if ((keyNavigationMode == KeyboardNavigationMode.Once || keyNavigationMode == KeyboardNavigationMode.None) && container != e)
            {
                if (goDownOnly)
                {
                    return null;
                }
                DependencyObject groupParent = this.GetGroupParent(container);
                return this.GetNextTab(container, groupParent, goDownOnly);
            }
            DependencyObject dependencyObject = null;
            DependencyObject dependencyObject2 = e;
            KeyboardNavigationMode keyboardNavigationMode = keyNavigationMode;
            while ((dependencyObject2 = this.GetNextTabInGroup(dependencyObject2, container, keyboardNavigationMode)) != null && dependencyObject != dependencyObject2)
            {
                if (dependencyObject == null)
                {
                    dependencyObject = dependencyObject2;
                }
                DependencyObject nextTab = this.GetNextTab(null, dependencyObject2, true);
                if (nextTab != null)
                {
                    return nextTab;
                }
                if (keyboardNavigationMode == KeyboardNavigationMode.Once)
                {
                    keyboardNavigationMode = KeyboardNavigationMode.Contained;
                }
            }
            if (!goDownOnly && keyboardNavigationMode != KeyboardNavigationMode.Contained && this.GetParent(container) != null)
            {
                return this.GetNextTab(container, this.GetGroupParent(container), false);
            }
            return null;
        }

        internal DependencyObject GetLastTabInGroup(DependencyObject container)
        {
            DependencyObject dependencyObject = null;
            int num = 2147483647;
            DependencyObject dependencyObject2 = this.GetLastInTree(container);
            while (dependencyObject2 != null && dependencyObject2 != container)
            {
                if (this.IsTabStopOrGroup(dependencyObject2))
                {
                    int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(dependencyObject2);
                    if (tabIndexHelper > num || dependencyObject == null)
                    {
                        num = tabIndexHelper;
                        dependencyObject = dependencyObject2;
                    }
                }
                dependencyObject2 = this.GetPreviousInTree(dependencyObject2, container);
            }
            return dependencyObject;
        }

        private DependencyObject GetPrevTabWithSameIndex(DependencyObject e, DependencyObject container)
        {
            int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(e);
            for (DependencyObject previousInTree = this.GetPreviousInTree(e, container); previousInTree != null; previousInTree = this.GetPreviousInTree(previousInTree, container))
            {
                if (this.IsTabStopOrGroup(previousInTree) && KeyboardNavigation.GetTabIndexHelper(previousInTree) == tabIndexHelper && previousInTree != container)
                {
                    return previousInTree;
                }
            }
            return null;
        }

        private DependencyObject GetPrevTabWithPrevIndex(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
        {
            DependencyObject dependencyObject = null;
            DependencyObject dependencyObject2 = null;
            int tabIndexHelper = KeyboardNavigation.GetTabIndexHelper(e);
            int num = 2147483647;
            int num2 = 2147483647;
            for (DependencyObject dependencyObject3 = this.GetLastInTree(container); dependencyObject3 != null; dependencyObject3 = this.GetPreviousInTree(dependencyObject3, container))
            {
                if (this.IsTabStopOrGroup(dependencyObject3) && dependencyObject3 != container)
                {
                    int tabIndexHelper2 = KeyboardNavigation.GetTabIndexHelper(dependencyObject3);
                    if (tabIndexHelper2 < tabIndexHelper && (tabIndexHelper2 > num2 || dependencyObject2 == null))
                    {
                        num2 = tabIndexHelper2;
                        dependencyObject2 = dependencyObject3;
                    }
                    if (tabIndexHelper2 > num || dependencyObject == null)
                    {
                        num = tabIndexHelper2;
                        dependencyObject = dependencyObject3;
                    }
                }
            }
            if (tabbingType == KeyboardNavigationMode.Cycle && dependencyObject2 == null)
            {
                dependencyObject2 = dependencyObject;
            }
            return dependencyObject2;
        }

        private DependencyObject GetPrevTabInGroup(DependencyObject e, DependencyObject container, KeyboardNavigationMode tabbingType)
        {
            if (tabbingType == KeyboardNavigationMode.None)
            {
                return null;
            }
            if (e == null)
            {
                return this.GetLastTabInGroup(container);
            }
            if (tabbingType == KeyboardNavigationMode.Once)
            {
                return null;
            }
            if (e == container)
            {
                return null;
            }
            DependencyObject prevTabWithSameIndex = this.GetPrevTabWithSameIndex(e, container);
            if (prevTabWithSameIndex != null)
            {
                return prevTabWithSameIndex;
            }
            return this.GetPrevTabWithPrevIndex(e, container, tabbingType);
        }

        private DependencyObject GetPrevTab(DependencyObject e, DependencyObject container, bool goDownOnly)
        {
            if (container == null)
            {
                container = this.GetGroupParent(e);
            }
            KeyboardNavigationMode keyNavigationMode = this.GetKeyNavigationMode(container);
            if (e == null)
            {
                DependencyObject activeElement = this.GetActiveElement(container);
                if (activeElement != null)
                {
                    return this.GetPrevTab(null, activeElement, true);
                }
                if (keyNavigationMode == KeyboardNavigationMode.Once)
                {
                    DependencyObject nextTabInGroup = this.GetNextTabInGroup(null, container, keyNavigationMode);
                    if (nextTabInGroup != null)
                    {
                        return this.GetPrevTab(null, nextTabInGroup, true);
                    }
                    if (this.IsTabStop(container))
                    {
                        return container;
                    }
                    if (goDownOnly)
                    {
                        return null;
                    }
                    return this.GetPrevTab(container, null, false);
                }
            }
            else if (keyNavigationMode == KeyboardNavigationMode.Once || keyNavigationMode == KeyboardNavigationMode.None)
            {
                if (goDownOnly || container == e)
                {
                    return null;
                }
                if (this.IsTabStop(container))
                {
                    return container;
                }
                return this.GetPrevTab(container, null, false);
            }
            DependencyObject dependencyObject = null;
            DependencyObject dependencyObject2 = e;
            while ((dependencyObject2 = this.GetPrevTabInGroup(dependencyObject2, container, keyNavigationMode)) != null && (dependencyObject2 != container || keyNavigationMode != KeyboardNavigationMode.Local))
            {
                if (this.IsTabStop(dependencyObject2) && !this.IsGroup(dependencyObject2))
                {
                    return dependencyObject2;
                }
                if (dependencyObject == dependencyObject2)
                {
                    break;
                }
                if (dependencyObject == null)
                {
                    dependencyObject = dependencyObject2;
                }
                DependencyObject prevTab = this.GetPrevTab(null, dependencyObject2, true);
                if (prevTab != null)
                {
                    return prevTab;
                }
            }
            if (keyNavigationMode == KeyboardNavigationMode.Contained)
            {
                return null;
            }
            if (e != container && this.IsTabStop(container))
            {
                return container;
            }
            if (!goDownOnly && this.GetParent(container) != null)
            {
                return this.GetPrevTab(container, null, false);
            }
            return null;
        }

        internal static Rect GetRectangle(DependencyObject element)
        {
            UIElement uIElement = element as UIElement;
            if (uIElement != null)
            {
                if (!uIElement.IsArrangeValid)
                {
                    uIElement.UpdateLayout();
                }
                Visual visualRoot = KeyboardNavigation.GetVisualRoot(uIElement);
                if (visualRoot != null)
                {
                    GeneralTransform arg_106_0 = uIElement.TransformToAncestor(visualRoot);
                    Thickness thickness = (Thickness)uIElement.GetValue(KeyboardNavigation.DirectionalNavigationMarginProperty);
                    double x = -thickness.Left;
                    double y = -thickness.Top;
                    double num = uIElement.RenderSize.Width + thickness.Left + thickness.Right;
                    double num2 = uIElement.RenderSize.Height + thickness.Top + thickness.Bottom;
                    if (num < 0.0)
                    {
                        x = uIElement.RenderSize.Width * 0.5;
                        num = 0.0;
                    }
                    if (num2 < 0.0)
                    {
                        y = uIElement.RenderSize.Height * 0.5;
                        num2 = 0.0;
                    }
                    return arg_106_0.TransformBounds(new Rect(x, y, num, num2));
                }
            }
            else
            {
                ContentElement contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    IContentHost contentHost = null;
                    UIElement parentUIElementFromContentElement = KeyboardNavigation.GetParentUIElementFromContentElement(contentElement, ref contentHost);
                    Visual visual = contentHost as Visual;
                    if (contentHost != null && visual != null && parentUIElementFromContentElement != null)
                    {
                        Visual visualRoot2 = KeyboardNavigation.GetVisualRoot(visual);
                        if (visualRoot2 != null)
                        {
                            if (!parentUIElementFromContentElement.IsMeasureValid)
                            {
                                parentUIElementFromContentElement.UpdateLayout();
                            }
                            IEnumerator<Rect> enumerator = contentHost.GetRectangles(contentElement).GetEnumerator();
                            if (enumerator.MoveNext())
                            {
                                GeneralTransform arg_191_0 = visual.TransformToAncestor(visualRoot2);
                                Rect current = enumerator.Current;
                                return arg_191_0.TransformBounds(current);
                            }
                        }
                    }
                }
                else
                {
                    UIElement3D uIElement3D = element as UIElement3D;
                    if (uIElement3D != null)
                    {
                        Visual visualRoot3 = KeyboardNavigation.GetVisualRoot(uIElement3D);
                        Visual containingVisual2D = VisualTreeHelper.GetContainingVisual2D(uIElement3D);
                        if (visualRoot3 != null && containingVisual2D != null)
                        {
                            Rect visual2DContentBounds = uIElement3D.Visual2DContentBounds;
                            return containingVisual2D.TransformToAncestor(visualRoot3).TransformBounds(visual2DContentBounds);
                        }
                    }
                }
            }
            return Rect.Empty;
        }

        private Rect GetRepresentativeRectangle(DependencyObject element)
        {
            Rect rectangle = KeyboardNavigation.GetRectangle(element);
            TreeViewItem treeViewItem = element as TreeViewItem;
            if (treeViewItem != null)
            {
                Panel itemsHost = treeViewItem.ItemsHost;
                if (itemsHost != null && itemsHost.IsVisible)
                {
                    Rect rectangle2 = KeyboardNavigation.GetRectangle(itemsHost);
                    if (rectangle2 != Rect.Empty)
                    {
                        bool? flag = null;
                        FrameworkElement frameworkElement = treeViewItem.TryGetHeaderElement();
                        if (frameworkElement != null && frameworkElement != treeViewItem && frameworkElement.IsVisible)
                        {
                            Rect rectangle3 = KeyboardNavigation.GetRectangle(frameworkElement);
                            if (!rectangle3.IsEmpty)
                            {
                                if (DoubleUtil.LessThan(rectangle3.Top, rectangle2.Top))
                                {
                                    flag = new bool?(true);
                                }
                                else if (DoubleUtil.GreaterThan(rectangle3.Bottom, rectangle2.Bottom))
                                {
                                    flag = new bool?(false);
                                }
                            }
                        }
                        double num = rectangle2.Top - rectangle.Top;
                        double num2 = rectangle.Bottom - rectangle2.Bottom;
                        if (!flag.HasValue)
                        {
                            flag = new bool?(DoubleUtil.GreaterThanOrClose(num, num2));
                        }
                        if (flag == true)
                        {
                            rectangle.Height = Math.Min(Math.Max(num, 0.0), rectangle.Height);
                        }
                        else
                        {
                            double num3 = Math.Min(Math.Max(num2, 0.0), rectangle.Height);
                            rectangle.Y = rectangle.Bottom - num3;
                            rectangle.Height = num3;
                        }
                    }
                }
            }
            return rectangle;
        }

        private double GetDistance(Point p1, Point p2)
        {
            double arg_1F_0 = p1.X - p2.X;
            double num = p1.Y - p2.Y;
            double arg_24_0 = arg_1F_0 * arg_1F_0;
            double expr_22 = num;
            return Math.Sqrt(arg_24_0 + expr_22 * expr_22);
        }

        private double GetPerpDistance(Rect sourceRect, Rect targetRect, FocusNavigationDirection direction)
        {
            switch (direction)
            {
                case FocusNavigationDirection.Left:
                    return sourceRect.Right - targetRect.Right;
                case FocusNavigationDirection.Right:
                    return targetRect.Left - sourceRect.Left;
                case FocusNavigationDirection.Up:
                    return sourceRect.Bottom - targetRect.Bottom;
                case FocusNavigationDirection.Down:
                    return targetRect.Top - sourceRect.Top;
                default:
                    throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
            }
        }

        private double GetDistance(Rect sourceRect, Rect targetRect, FocusNavigationDirection direction)
        {
            Point p;
            Point p2;
            switch (direction)
            {
                case FocusNavigationDirection.Left:
                    p = sourceRect.TopRight;
                    if (this._horizontalBaseline != -1.7976931348623157E+308)
                    {
                        p.Y = this._horizontalBaseline;
                    }
                    p2 = targetRect.TopRight;
                    break;
                case FocusNavigationDirection.Right:
                    p = sourceRect.TopLeft;
                    if (this._horizontalBaseline != -1.7976931348623157E+308)
                    {
                        p.Y = this._horizontalBaseline;
                    }
                    p2 = targetRect.TopLeft;
                    break;
                case FocusNavigationDirection.Up:
                    p = sourceRect.BottomLeft;
                    if (this._verticalBaseline != -1.7976931348623157E+308)
                    {
                        p.X = this._verticalBaseline;
                    }
                    p2 = targetRect.BottomLeft;
                    break;
                case FocusNavigationDirection.Down:
                    p = sourceRect.TopLeft;
                    if (this._verticalBaseline != -1.7976931348623157E+308)
                    {
                        p.X = this._verticalBaseline;
                    }
                    p2 = targetRect.TopLeft;
                    break;
                default:
                    throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
            }
            return this.GetDistance(p, p2);
        }

        private bool IsInDirection(Rect fromRect, Rect toRect, FocusNavigationDirection direction)
        {
            switch (direction)
            {
                case FocusNavigationDirection.Left:
                    return DoubleUtil.GreaterThanOrClose(fromRect.Left, toRect.Right);
                case FocusNavigationDirection.Right:
                    return DoubleUtil.LessThanOrClose(fromRect.Right, toRect.Left);
                case FocusNavigationDirection.Up:
                    return DoubleUtil.GreaterThanOrClose(fromRect.Top, toRect.Bottom);
                case FocusNavigationDirection.Down:
                    return DoubleUtil.LessThanOrClose(fromRect.Bottom, toRect.Top);
                default:
                    throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
            }
        }

        private bool IsFocusScope(DependencyObject e)
        {
            return FocusManager.GetIsFocusScope(e) || this.GetParent(e) == null;
        }

        private bool IsAncestorOf(DependencyObject sourceElement, DependencyObject targetElement)
        {
            Visual visual = sourceElement as Visual;
            Visual visual2 = targetElement as Visual;
            return visual != null && visual2 != null && visual.IsAncestorOf(visual2);
        }

        internal bool IsAncestorOfEx(DependencyObject sourceElement, DependencyObject targetElement)
        {
            while (targetElement != null && targetElement != sourceElement)
            {
                targetElement = this.GetParent(targetElement);
            }
            return targetElement == sourceElement;
        }

        private bool IsInRange(DependencyObject sourceElement, DependencyObject targetElement, Rect sourceRect, Rect targetRect, FocusNavigationDirection direction, double startRange, double endRange)
        {
            switch (direction)
            {
                case FocusNavigationDirection.Left:
                case FocusNavigationDirection.Right:
                    if (this._horizontalBaseline != -1.7976931348623157E+308)
                    {
                        startRange = Math.Min(startRange, this._horizontalBaseline);
                        endRange = Math.Max(endRange, this._horizontalBaseline);
                    }
                    if (DoubleUtil.GreaterThan(targetRect.Bottom, startRange) && DoubleUtil.LessThan(targetRect.Top, endRange))
                    {
                        if (sourceElement == null)
                        {
                            return true;
                        }
                        if (direction == FocusNavigationDirection.Right)
                        {
                            return DoubleUtil.GreaterThan(targetRect.Left, sourceRect.Left) || (DoubleUtil.AreClose(targetRect.Left, sourceRect.Left) && this.IsAncestorOf(sourceElement, targetElement));
                        }
                        return DoubleUtil.LessThan(targetRect.Right, sourceRect.Right) || (DoubleUtil.AreClose(targetRect.Right, sourceRect.Right) && this.IsAncestorOf(sourceElement, targetElement));
                    }
                    break;
                case FocusNavigationDirection.Up:
                case FocusNavigationDirection.Down:
                    if (this._verticalBaseline != -1.7976931348623157E+308)
                    {
                        startRange = Math.Min(startRange, this._verticalBaseline);
                        endRange = Math.Max(endRange, this._verticalBaseline);
                    }
                    if (DoubleUtil.GreaterThan(targetRect.Right, startRange) && DoubleUtil.LessThan(targetRect.Left, endRange))
                    {
                        if (sourceElement == null)
                        {
                            return true;
                        }
                        if (direction == FocusNavigationDirection.Down)
                        {
                            return DoubleUtil.GreaterThan(targetRect.Top, sourceRect.Top) || (DoubleUtil.AreClose(targetRect.Top, sourceRect.Top) && this.IsAncestorOf(sourceElement, targetElement));
                        }
                        return DoubleUtil.LessThan(targetRect.Bottom, sourceRect.Bottom) || (DoubleUtil.AreClose(targetRect.Bottom, sourceRect.Bottom) && this.IsAncestorOf(sourceElement, targetElement));
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException("direction", (int)direction, typeof(FocusNavigationDirection));
            }
            return false;
        }

        private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction)
        {
            return this.GetNextInDirection(sourceElement, direction, false);
        }

        private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation)
        {
            return this.GetNextInDirection(sourceElement, direction, treeViewNavigation, true);
        }

        private DependencyObject GetNextInDirection(DependencyObject sourceElement, FocusNavigationDirection direction, bool treeViewNavigation, bool considerDescendants)
        {
            this._containerHashtable.Clear();
            DependencyObject dependencyObject = this.MoveNext(sourceElement, null, direction, -1.7976931348623157E+308, -1.7976931348623157E+308, treeViewNavigation, considerDescendants);
            if (dependencyObject != null)
            {
                UIElement uIElement = sourceElement as UIElement;
                if (uIElement != null)
                {
                    uIElement.RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus));
                }
                else
                {
                    ContentElement contentElement = sourceElement as ContentElement;
                    if (contentElement != null)
                    {
                        contentElement.RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus));
                    }
                }
                UIElement uIElement2 = dependencyObject as UIElement;
                if (uIElement2 == null)
                {
                    uIElement2 = KeyboardNavigation.GetParentUIElementFromContentElement(dependencyObject as ContentElement);
                }
                else
                {
                    ContentElement contentElement2 = dependencyObject as ContentElement;
                    if (contentElement2 != null)
                    {
                        contentElement2.AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus), true);
                    }
                }
                if (uIElement2 != null)
                {
                    uIElement2.LayoutUpdated += new EventHandler(this.OnLayoutUpdated);
                    if (dependencyObject == uIElement2)
                    {
                        uIElement2.AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus), true);
                    }
                }
            }
            this._containerHashtable.Clear();
            return dependencyObject;
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            UIElement uIElement = sender as UIElement;
            if (uIElement != null)
            {
                uIElement.LayoutUpdated -= new EventHandler(this.OnLayoutUpdated);
            }
            this._verticalBaseline = -1.7976931348623157E+308;
            this._horizontalBaseline = -1.7976931348623157E+308;
        }

        private void _LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this._verticalBaseline = -1.7976931348623157E+308;
            this._horizontalBaseline = -1.7976931348623157E+308;
            if (sender is UIElement)
            {
                ((UIElement)sender).RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus));
                return;
            }
            if (sender is ContentElement)
            {
                ((ContentElement)sender).RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(this._LostFocus));
            }
        }

        private bool IsEndlessLoop(DependencyObject element, DependencyObject container)
        {
            object key = (element != null) ? element : KeyboardNavigation._fakeNull;
            Hashtable hashtable = this._containerHashtable[container] as Hashtable;
            if (hashtable != null)
            {
                if (hashtable[key] != null)
                {
                    return true;
                }
            }
            else
            {
                hashtable = new Hashtable(10);
                this._containerHashtable[container] = hashtable;
            }
            hashtable[key] = BooleanBoxes.TrueBox;
            return false;
        }

        private void ResetBaseLines(double value, bool horizontalDirection)
        {
            if (horizontalDirection)
            {
                this._verticalBaseline = -1.7976931348623157E+308;
                if (this._horizontalBaseline == -1.7976931348623157E+308)
                {
                    this._horizontalBaseline = value;
                    return;
                }
            }
            else
            {
                this._horizontalBaseline = -1.7976931348623157E+308;
                if (this._verticalBaseline == -1.7976931348623157E+308)
                {
                    this._verticalBaseline = value;
                }
            }
        }

        private DependencyObject FindNextInDirection(DependencyObject sourceElement, Rect sourceRect, DependencyObject container, FocusNavigationDirection direction, double startRange, double endRange, bool treeViewNavigation, bool considerDescendants)
        {
            DependencyObject dependencyObject = null;
            Rect targetRect = Rect.Empty;
            double value = 0.0;
            bool flag = sourceElement == null;
            DependencyObject dependencyObject2 = container;
            while ((dependencyObject2 = this.GetNextInTree(dependencyObject2, container)) != null)
            {
                if (dependencyObject2 != sourceElement && this.IsGroupElementEligible(dependencyObject2, treeViewNavigation))
                {
                    Rect representativeRectangle = this.GetRepresentativeRectangle(dependencyObject2);
                    if (representativeRectangle != Rect.Empty)
                    {
                        bool flag2 = this.IsInDirection(sourceRect, representativeRectangle, direction);
                        bool flag3 = this.IsInRange(sourceElement, dependencyObject2, sourceRect, representativeRectangle, direction, startRange, endRange);
                        if (flag | flag2 | flag3)
                        {
                            double num = flag3 ? this.GetPerpDistance(sourceRect, representativeRectangle, direction) : this.GetDistance(sourceRect, representativeRectangle, direction);
                            if (!double.IsNaN(num))
                            {
                                if (dependencyObject == null && (considerDescendants || !this.IsAncestorOfEx(sourceElement, dependencyObject2)))
                                {
                                    dependencyObject = dependencyObject2;
                                    targetRect = representativeRectangle;
                                    value = num;
                                }
                                else if ((DoubleUtil.LessThan(num, value) || (DoubleUtil.AreClose(num, value) && this.GetDistance(sourceRect, targetRect, direction) > this.GetDistance(sourceRect, representativeRectangle, direction))) && (considerDescendants || !this.IsAncestorOfEx(sourceElement, dependencyObject2)))
                                {
                                    dependencyObject = dependencyObject2;
                                    targetRect = representativeRectangle;
                                    value = num;
                                }
                            }
                        }
                    }
                }
            }
            return dependencyObject;
        }

        private DependencyObject MoveNext(DependencyObject sourceElement, DependencyObject container, FocusNavigationDirection direction, double startRange, double endRange, bool treeViewNavigation, bool considerDescendants)
        {
            if (container == null)
            {
                container = this.GetGroupParent(sourceElement);
            }
            if (container == sourceElement)
            {
                return null;
            }
            if (this.IsEndlessLoop(sourceElement, container))
            {
                return null;
            }
            KeyboardNavigationMode keyNavigationMode = this.GetKeyNavigationMode(container);
            bool flag = sourceElement == null;
            if (keyNavigationMode == KeyboardNavigationMode.None & flag)
            {
                return null;
            }
            Rect sourceRect = flag ? KeyboardNavigation.GetRectangle(container) : this.GetRepresentativeRectangle(sourceElement);
            bool flag2 = direction == FocusNavigationDirection.Right || direction == FocusNavigationDirection.Left;
            this.ResetBaseLines(flag2 ? sourceRect.Top : sourceRect.Left, flag2);
            if (startRange == -1.7976931348623157E+308 || endRange == -1.7976931348623157E+308)
            {
                startRange = (flag2 ? sourceRect.Top : sourceRect.Left);
                endRange = (flag2 ? sourceRect.Bottom : sourceRect.Right);
            }
            if (keyNavigationMode == KeyboardNavigationMode.Once && !flag)
            {
                return this.MoveNext(container, null, direction, startRange, endRange, treeViewNavigation, true);
            }
            DependencyObject dependencyObject = this.FindNextInDirection(sourceElement, sourceRect, container, direction, startRange, endRange, treeViewNavigation, considerDescendants);
            if (dependencyObject == null)
            {
                if (keyNavigationMode == KeyboardNavigationMode.Cycle)
                {
                    return this.MoveNext(null, container, direction, startRange, endRange, treeViewNavigation, true);
                }
                if (keyNavigationMode != KeyboardNavigationMode.Contained)
                {
                    return this.MoveNext(container, null, direction, startRange, endRange, treeViewNavigation, true);
                }
                return null;
            }
            else
            {
                if (this.IsElementEligible(dependencyObject, treeViewNavigation))
                {
                    return dependencyObject;
                }
                DependencyObject activeElementChain = this.GetActiveElementChain(dependencyObject, treeViewNavigation);
                if (activeElementChain != null)
                {
                    return activeElementChain;
                }
                DependencyObject dependencyObject2 = this.MoveNext(null, dependencyObject, direction, startRange, endRange, treeViewNavigation, true);
                if (dependencyObject2 != null)
                {
                    return dependencyObject2;
                }
                return this.MoveNext(dependencyObject, null, direction, startRange, endRange, treeViewNavigation, true);
            }
        }

        private DependencyObject GetActiveElementChain(DependencyObject element, bool treeViewNavigation)
        {
            DependencyObject result = null;
            DependencyObject dependencyObject = element;
            while ((dependencyObject = this.GetActiveElement(dependencyObject)) != null)
            {
                if (this.IsElementEligible(dependencyObject, treeViewNavigation))
                {
                    result = dependencyObject;
                }
            }
            return result;
        }

        private DependencyObject FindElementAtViewportEdge(DependencyObject sourceElement, FrameworkElement viewportBoundsElement, DependencyObject container, FocusNavigationDirection direction, bool treeViewNavigation)
        {
            Rect rect = new Rect(0.0, 0.0, 0.0, 0.0);
            if (sourceElement != null && ItemsControl.GetElementViewportPosition(viewportBoundsElement, ItemsControl.TryGetTreeViewItemHeader(sourceElement) as UIElement, direction, false, out rect) == ElementViewportPosition.None)
            {
                rect = new Rect(0.0, 0.0, 0.0, 0.0);
            }
            DependencyObject dependencyObject = null;
            double value = double.NegativeInfinity;
            double value2 = double.NegativeInfinity;
            DependencyObject dependencyObject2 = null;
            double value3 = double.NegativeInfinity;
            double value4 = double.NegativeInfinity;
            DependencyObject dependencyObject3 = container;
            while ((dependencyObject3 = this.GetNextInTree(dependencyObject3, container)) != null)
            {
                if (this.IsGroupElementEligible(dependencyObject3, treeViewNavigation))
                {
                    DependencyObject dependencyObject4 = dependencyObject3;
                    if (treeViewNavigation)
                    {
                        dependencyObject4 = ItemsControl.TryGetTreeViewItemHeader(dependencyObject3);
                    }
                    Rect rect2;
                    ElementViewportPosition elementViewportPosition = ItemsControl.GetElementViewportPosition(viewportBoundsElement, dependencyObject4 as UIElement, direction, false, out rect2);
                    if (elementViewportPosition == ElementViewportPosition.CompletelyInViewport || elementViewportPosition == ElementViewportPosition.PartiallyInViewport)
                    {
                        double num = double.NegativeInfinity;
                        switch (direction)
                        {
                            case FocusNavigationDirection.Left:
                                num = -rect2.Left;
                                break;
                            case FocusNavigationDirection.Right:
                                num = rect2.Right;
                                break;
                            case FocusNavigationDirection.Up:
                                num = -rect2.Top;
                                break;
                            case FocusNavigationDirection.Down:
                                num = rect2.Bottom;
                                break;
                        }
                        double num2 = double.NegativeInfinity;
                        switch (direction)
                        {
                            case FocusNavigationDirection.Left:
                            case FocusNavigationDirection.Right:
                                num2 = this.ComputeRangeScore(rect.Top, rect.Bottom, rect2.Top, rect2.Bottom);
                                break;
                            case FocusNavigationDirection.Up:
                            case FocusNavigationDirection.Down:
                                num2 = this.ComputeRangeScore(rect.Left, rect.Right, rect2.Left, rect2.Right);
                                break;
                        }
                        if (elementViewportPosition == ElementViewportPosition.CompletelyInViewport)
                        {
                            if (dependencyObject == null || DoubleUtil.GreaterThan(num, value) || (DoubleUtil.AreClose(num, value) && DoubleUtil.GreaterThan(num2, value2)))
                            {
                                dependencyObject = dependencyObject3;
                                value = num;
                                value2 = num2;
                            }
                        }
                        else if (dependencyObject2 == null || DoubleUtil.GreaterThan(num, value3) || (DoubleUtil.AreClose(num, value3) && DoubleUtil.GreaterThan(num2, value4)))
                        {
                            dependencyObject2 = dependencyObject3;
                            value3 = num;
                            value4 = num2;
                        }
                    }
                }
            }
            if (dependencyObject == null)
            {
                return dependencyObject2;
            }
            return dependencyObject;
        }

        private double ComputeRangeScore(double rangeStart1, double rangeEnd1, double rangeStart2, double rangeEnd2)
        {
            if (DoubleUtil.GreaterThan(rangeStart1, rangeStart2))
            {
                double arg_0D_0 = rangeStart1;
                rangeStart1 = rangeStart2;
                rangeStart2 = arg_0D_0;
                double arg_14_0 = rangeEnd1;
                rangeEnd1 = rangeEnd2;
                rangeEnd2 = arg_14_0;
            }
            if (DoubleUtil.LessThan(rangeEnd1, rangeEnd2))
            {
                return rangeEnd1 - rangeStart2;
            }
            return rangeEnd2 - rangeStart2;
        }

        [SecurityCritical]
        private void ProcessForMenuMode(InputEventArgs inputEventArgs)
        {
            if (inputEventArgs.RoutedEvent == Keyboard.LostKeyboardFocusEvent)
            {
                KeyboardFocusChangedEventArgs keyboardFocusChangedEventArgs = inputEventArgs as KeyboardFocusChangedEventArgs;
                if ((keyboardFocusChangedEventArgs != null && keyboardFocusChangedEventArgs.NewFocus == null) || inputEventArgs.Handled)
                {
                    this._lastKeyPressed = Key.None;
                    return;
                }
            }
            else if (inputEventArgs.RoutedEvent == Keyboard.KeyDownEvent)
            {
                if (inputEventArgs.Handled)
                {
                    this._lastKeyPressed = Key.None;
                    return;
                }
                KeyEventArgs keyEventArgs = inputEventArgs as KeyEventArgs;
                if (!keyEventArgs.IsRepeat)
                {
                    if (this._lastKeyPressed == Key.None)
                    {
                        if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows)) == ModifierKeys.None)
                        {
                            this._lastKeyPressed = this.GetRealKey(keyEventArgs);
                        }
                    }
                    else
                    {
                        this._lastKeyPressed = Key.None;
                    }
                    this._win32MenuModeWorkAround = false;
                    return;
                }
            }
            else
            {
                if (inputEventArgs.RoutedEvent == Keyboard.KeyUpEvent)
                {
                    if (!inputEventArgs.Handled)
                    {
                        KeyEventArgs keyEventArgs2 = inputEventArgs as KeyEventArgs;
                        Key realKey = this.GetRealKey(keyEventArgs2);
                        if (realKey == this._lastKeyPressed && this.IsMenuKey(realKey))
                        {
                            KeyboardNavigation.EnableKeyboardCues(keyEventArgs2.Source as DependencyObject, true);
                            keyEventArgs2.Handled = this.OnEnterMenuMode(keyEventArgs2.Source);
                        }
                        if (this._win32MenuModeWorkAround)
                        {
                            if (this.IsMenuKey(realKey))
                            {
                                this._win32MenuModeWorkAround = false;
                                keyEventArgs2.Handled = true;
                            }
                        }
                        else if (keyEventArgs2.Handled)
                        {
                            this._win32MenuModeWorkAround = true;
                        }
                    }
                    this._lastKeyPressed = Key.None;
                    return;
                }
                if (inputEventArgs.RoutedEvent == Mouse.MouseDownEvent || inputEventArgs.RoutedEvent == Mouse.MouseUpEvent)
                {
                    this._lastKeyPressed = Key.None;
                    this._win32MenuModeWorkAround = false;
                }
            }
        }

        private bool IsMenuKey(Key key)
        {
            return key == Key.LeftAlt || key == Key.RightAlt || key == Key.F10;
        }

        private Key GetRealKey(KeyEventArgs e)
        {
            if (e.Key != Key.System)
            {
                return e.Key;
            }
            return e.SystemKey;
        }

        [SecurityCritical, SecurityTreatAsSafe]
        private bool OnEnterMenuMode(object eventSource)
        {
            if (this._weakEnterMenuModeHandlers == null)
            {
                return false;
            }
            KeyboardNavigation.WeakReferenceList weakEnterMenuModeHandlers = this._weakEnterMenuModeHandlers;
            bool flag = false;
            bool result;
            try
            {
                Monitor.Enter(weakEnterMenuModeHandlers, ref flag);
                if (this._weakEnterMenuModeHandlers.Count == 0)
                {
                    result = false;
                }
                else
                {
                    PresentationSource source = null;
                    if (eventSource != null)
                    {
                        Visual visual = eventSource as Visual;
                        source = ((visual != null) ? PresentationSource.CriticalFromVisual(visual) : null);
                    }
                    else
                    {
                        IntPtr activeWindow = UnsafeNativeMethods.GetActiveWindow();
                        if (activeWindow != IntPtr.Zero)
                        {
                            source = HwndSource.CriticalFromHwnd(activeWindow);
                        }
                    }
                    if (source == null)
                    {
                        result = false;
                    }
                    else
                    {
                        EventArgs e = EventArgs.Empty;
                        bool handled = false;
                        this._weakEnterMenuModeHandlers.Process(delegate(object obj)
                        {
                            KeyboardNavigation.EnterMenuModeEventHandler enterMenuModeEventHandler = obj as KeyboardNavigation.EnterMenuModeEventHandler;
                            if (enterMenuModeEventHandler != null && enterMenuModeEventHandler(source, e))
                            {
                                handled = true;
                            }
                            return handled;
                        });
                        result = handled;
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    Monitor.Exit(weakEnterMenuModeHandlers);
                }
            }
            return result;
        }

        [SecurityCritical]
        private void ProcessForUIState(InputEventArgs inputEventArgs)
        {
            RawUIStateInputReport rawUIStateInputReport = this.ExtractRawUIStateInputReport(inputEventArgs, InputManager.InputReportEvent);
            PresentationSource inputSource;
            if (rawUIStateInputReport != null && (inputSource = rawUIStateInputReport.InputSource) != null && (rawUIStateInputReport.Targets & RawUIStateTargets.HideAccelerators) != RawUIStateTargets.None)
            {
                DependencyObject arg_35_0 = inputSource.RootVisual;
                bool enable = rawUIStateInputReport.Action == RawUIStateActions.Clear;
                KeyboardNavigation.EnableKeyboardCues(arg_35_0, enable);
            }
        }

        [SecurityCritical]
        private RawUIStateInputReport ExtractRawUIStateInputReport(InputEventArgs e, RoutedEvent Event)
        {
            RawUIStateInputReport result = null;
            InputReportEventArgs inputReportEventArgs = e as InputReportEventArgs;
            if (inputReportEventArgs != null && inputReportEventArgs.Report.Type == InputType.Keyboard && inputReportEventArgs.RoutedEvent == Event)
            {
                result = (inputReportEventArgs.Report as RawUIStateInputReport);
            }
            return result;
        }

        private void NotifyFocusEnterMainFocusScope(object sender, EventArgs e)
        {
            this._weakFocusEnterMainFocusScopeHandlers.Process(delegate(object item)
            {
                EventHandler eventHandler = item as EventHandler;
                if (eventHandler != null)
                {
                    eventHandler(sender, e);
                }
                return false;
            });
        }
    }
}
