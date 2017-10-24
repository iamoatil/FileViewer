using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Win32;
using System;
using System.Security;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace System.Windows.Controls
{
    internal sealed class PopupControlService
    {
        internal static readonly RoutedEvent ContextMenuOpenedEvent = EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupControlService));

        internal static readonly RoutedEvent ContextMenuClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PopupControlService));

        internal static readonly DependencyProperty ServiceOwnedProperty = DependencyProperty.RegisterAttached("ServiceOwned", typeof(bool), typeof(PopupControlService), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        internal static readonly DependencyProperty OwnerProperty = DependencyProperty.RegisterAttached("Owner", typeof(DependencyObject), typeof(PopupControlService), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(PopupControlService.OnOwnerChanged)));

        private DispatcherTimer _toolTipTimer;

        private bool _quickShow;

        private WeakReference _lastMouseDirectlyOver;

        private WeakReference _lastMouseOverWithToolTip;

        private WeakReference _lastChecked;

        private ToolTip _currentToolTip;

        private DispatcherTimer _forceCloseTimer;

        private bool _ownToolTip;

        private IInputElement LastMouseDirectlyOver
        {
            get
            {
                if (this._lastMouseDirectlyOver != null)
                {
                    IInputElement inputElement = (IInputElement)this._lastMouseDirectlyOver.Target;
                    if (inputElement != null)
                    {
                        return inputElement;
                    }
                    this._lastMouseDirectlyOver = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this._lastMouseDirectlyOver = null;
                    return;
                }
                if (this._lastMouseDirectlyOver == null)
                {
                    this._lastMouseDirectlyOver = new WeakReference(value);
                    return;
                }
                this._lastMouseDirectlyOver.Target = value;
            }
        }

        private DependencyObject LastMouseOverWithToolTip
        {
            get
            {
                if (this._lastMouseOverWithToolTip != null)
                {
                    DependencyObject dependencyObject = (DependencyObject)this._lastMouseOverWithToolTip.Target;
                    if (dependencyObject != null)
                    {
                        return dependencyObject;
                    }
                    this._lastMouseOverWithToolTip = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this._lastMouseOverWithToolTip = null;
                    return;
                }
                if (this._lastMouseOverWithToolTip == null)
                {
                    this._lastMouseOverWithToolTip = new WeakReference(value);
                    return;
                }
                this._lastMouseOverWithToolTip.Target = value;
            }
        }

        private DependencyObject LastChecked
        {
            get
            {
                if (this._lastChecked != null)
                {
                    DependencyObject dependencyObject = (DependencyObject)this._lastChecked.Target;
                    if (dependencyObject != null)
                    {
                        return dependencyObject;
                    }
                    this._lastChecked = null;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this._lastChecked = null;
                    return;
                }
                if (this._lastChecked == null)
                {
                    this._lastChecked = new WeakReference(value);
                    return;
                }
                this._lastChecked.Target = value;
            }
        }

        internal static PopupControlService Current
        {
            get
            {
                return FrameworkElement.PopupControlService;
            }
        }

        internal ToolTip CurrentToolTip
        {
            get
            {
                return this._currentToolTip;
            }
        }

        private DispatcherTimer ToolTipTimer
        {
            get
            {
                return this._toolTipTimer;
            }
            set
            {
                this.ResetToolTipTimer();
                this._toolTipTimer = value;
            }
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal PopupControlService()
        {
            InputManager.Current.PostProcessInput += new ProcessInputEventHandler(this.OnPostProcessInput);
        }

        [SecurityCritical]
        private void OnPostProcessInput(object sender, ProcessInputEventArgs e)
        {
            if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
            {
                InputReportEventArgs inputReportEventArgs = (InputReportEventArgs)e.StagingItem.Input;
                if (!inputReportEventArgs.Handled && inputReportEventArgs.Report.Type == InputType.Mouse)
                {
                    RawMouseInputReport rawMouseInputReport = (RawMouseInputReport)inputReportEventArgs.Report;
                    if ((rawMouseInputReport.Actions & RawMouseActions.AbsoluteMove) == RawMouseActions.AbsoluteMove)
                    {
                        if (Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed)
                        {
                            this.RaiseToolTipClosingEvent(true);
                            return;
                        }
                        IInputElement inputElement = Mouse.PrimaryDevice.RawDirectlyOver;
                        if (inputElement != null)
                        {
                            Point position = Mouse.PrimaryDevice.GetPosition(inputElement);
                            if (Mouse.CapturedMode != CaptureMode.None)
                            {
                                PresentationSource presentationSource = PresentationSource.CriticalFromVisual((DependencyObject)inputElement);
                                UIElement uIElement = (presentationSource != null) ? (presentationSource.RootVisual as UIElement) : null;
                                if (uIElement != null)
                                {
                                    position = Mouse.PrimaryDevice.GetPosition(uIElement);
                                    IInputElement inputElement2;
                                    uIElement.InputHitTest(position, out inputElement2, out inputElement);
                                    position = Mouse.PrimaryDevice.GetPosition(inputElement);
                                }
                                else
                                {
                                    inputElement = null;
                                }
                            }
                            if (inputElement != null)
                            {
                                this.OnMouseMove(inputElement, position);
                                return;
                            }
                        }
                    }
                    else if ((rawMouseInputReport.Actions & RawMouseActions.Deactivate) == RawMouseActions.Deactivate && this.LastMouseDirectlyOver != null)
                    {
                        this.LastMouseDirectlyOver = null;
                        if (this.LastMouseOverWithToolTip != null)
                        {
                            this.RaiseToolTipClosingEvent(true);
                            if (SafeNativeMethods.GetCapture() == IntPtr.Zero)
                            {
                                this.LastMouseOverWithToolTip = null;
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyDownEvent)
                {
                    this.ProcessKeyDown(sender, (KeyEventArgs)e.StagingItem.Input);
                    return;
                }
                if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyUpEvent)
                {
                    this.ProcessKeyUp(sender, (KeyEventArgs)e.StagingItem.Input);
                    return;
                }
                if (e.StagingItem.Input.RoutedEvent == Mouse.MouseUpEvent)
                {
                    this.ProcessMouseUp(sender, (MouseButtonEventArgs)e.StagingItem.Input);
                    return;
                }
                if (e.StagingItem.Input.RoutedEvent == Mouse.MouseDownEvent)
                {
                    this.RaiseToolTipClosingEvent(true);
                }
            }
        }

        private void OnMouseMove(IInputElement directlyOver, Point pt)
        {
            if (directlyOver != this.LastMouseDirectlyOver)
            {
                this.LastMouseDirectlyOver = directlyOver;
                if (directlyOver != this.LastMouseOverWithToolTip)
                {
                    this.InspectElementForToolTip(directlyOver as DependencyObject);
                }
            }
        }

        [SecurityCritical]
        private void ProcessMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.RaiseToolTipClosingEvent(false);
            if (!e.Handled && e.ChangedButton == MouseButton.Right && e.RightButton == MouseButtonState.Released)
            {
                IInputElement rawDirectlyOver = Mouse.PrimaryDevice.RawDirectlyOver;
                if (rawDirectlyOver != null)
                {
                    Point position = Mouse.PrimaryDevice.GetPosition(rawDirectlyOver);
                    if (this.RaiseContextMenuOpeningEvent(rawDirectlyOver, position.X, position.Y, e.UserInitiated))
                    {
                        e.Handled = true;
                    }
                }
            }
        }

        [SecurityCritical]
        private void ProcessKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled && e.SystemKey == Key.F10 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                this.RaiseContextMenuOpeningEvent(e);
            }
        }

        [SecurityCritical]
        private void ProcessKeyUp(object sender, KeyEventArgs e)
        {
            if (!e.Handled && e.Key == Key.Apps)
            {
                this.RaiseContextMenuOpeningEvent(e);
            }
        }

        private void InspectElementForToolTip(DependencyObject o)
        {
            DependencyObject lastChecked = o;
            if (this.LocateNearestToolTip(ref o))
            {
                if (o != null)
                {
                    if (this.LastMouseOverWithToolTip != null)
                    {
                        this.RaiseToolTipClosingEvent(true);
                    }
                    this.LastChecked = lastChecked;
                    this.LastMouseOverWithToolTip = o;
                    bool arg_3E_0 = this._quickShow;
                    this.ResetToolTipTimer();
                    if (arg_3E_0)
                    {
                        this._quickShow = false;
                        this.RaiseToolTipOpeningEvent();
                        return;
                    }
                    this.ToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
                    this.ToolTipTimer.Interval = TimeSpan.FromMilliseconds((double)ToolTipService.GetInitialShowDelay(o));
                    this.ToolTipTimer.Tag = BooleanBoxes.TrueBox;
                    this.ToolTipTimer.Tick += new EventHandler(this.OnRaiseToolTipOpeningEvent);
                    this.ToolTipTimer.Start();
                    return;
                }
            }
            else
            {
                this.RaiseToolTipClosingEvent(true);
                this.LastMouseOverWithToolTip = null;
            }
        }

        private bool LocateNearestToolTip(ref DependencyObject o)
        {
            IInputElement inputElement = o as IInputElement;
            if (inputElement != null)
            {
                FindToolTipEventArgs findToolTipEventArgs = new FindToolTipEventArgs();
                inputElement.RaiseEvent(findToolTipEventArgs);
                if (findToolTipEventArgs.TargetElement != null)
                {
                    o = findToolTipEventArgs.TargetElement;
                    return true;
                }
                if (findToolTipEventArgs.KeepCurrentActive)
                {
                    o = null;
                    return true;
                }
            }
            return false;
        }

        internal bool StopLookingForToolTip(DependencyObject o)
        {
            return o == this.LastChecked || o == this.LastMouseOverWithToolTip || o == this._currentToolTip || this.WithinCurrentToolTip(o);
        }

        private bool WithinCurrentToolTip(DependencyObject o)
        {
            if (this._currentToolTip == null)
            {
                return false;
            }
            DependencyObject dependencyObject = o as Visual;
            if (dependencyObject == null)
            {
                ContentElement contentElement = o as ContentElement;
                if (contentElement != null)
                {
                    dependencyObject = PopupControlService.FindContentElementParent(contentElement);
                }
                else
                {
                    dependencyObject = (o as Visual3D);
                }
            }
            return dependencyObject != null && ((dependencyObject is Visual && ((Visual)dependencyObject).IsDescendantOf(this._currentToolTip)) || (dependencyObject is Visual3D && ((Visual3D)dependencyObject).IsDescendantOf(this._currentToolTip)));
        }

        private void ResetToolTipTimer()
        {
            if (this._toolTipTimer != null)
            {
                this._toolTipTimer.Stop();
                this._toolTipTimer = null;
                this._quickShow = false;
            }
        }

        internal void OnRaiseToolTipOpeningEvent(object sender, EventArgs e)
        {
            this.RaiseToolTipOpeningEvent();
        }

        private void RaiseToolTipOpeningEvent()
        {
            this.ResetToolTipTimer();
            if (this._forceCloseTimer != null)
            {
                this.OnForceClose(null, EventArgs.Empty);
            }
            DependencyObject lastMouseOverWithToolTip = this.LastMouseOverWithToolTip;
            if (lastMouseOverWithToolTip != null)
            {
                bool flag = true;
                IInputElement inputElement = lastMouseOverWithToolTip as IInputElement;
                if (inputElement != null)
                {
                    ToolTipEventArgs toolTipEventArgs = new ToolTipEventArgs(true);
                    inputElement.RaiseEvent(toolTipEventArgs);
                    flag = !toolTipEventArgs.Handled;
                }
                if (flag)
                {
                    ToolTip toolTip = ToolTipService.GetToolTip(lastMouseOverWithToolTip) as ToolTip;
                    if (toolTip != null)
                    {
                        this._currentToolTip = toolTip;
                        this._ownToolTip = false;
                    }
                    else if (this._currentToolTip == null || !this._ownToolTip)
                    {
                        this._currentToolTip = new ToolTip();
                        this._ownToolTip = true;
                        this._currentToolTip.SetValue(PopupControlService.ServiceOwnedProperty, BooleanBoxes.TrueBox);
                        Binding binding = new Binding();
                        binding.Path = new PropertyPath(ToolTipService.ToolTipProperty);
                        binding.Mode = BindingMode.OneWay;
                        binding.Source = lastMouseOverWithToolTip;
                        this._currentToolTip.SetBinding(ContentControl.ContentProperty, binding);
                    }
                    if (!this._currentToolTip.StaysOpen)
                    {
                        throw new NotSupportedException(SR.Get("ToolTipStaysOpenFalseNotAllowed"));
                    }
                    this._currentToolTip.SetValue(PopupControlService.OwnerProperty, lastMouseOverWithToolTip);
                    this._currentToolTip.Closed += new RoutedEventHandler(this.OnToolTipClosed);
                    this._currentToolTip.IsOpen = true;
                    this.ToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
                    this.ToolTipTimer.Interval = TimeSpan.FromMilliseconds((double)ToolTipService.GetShowDuration(lastMouseOverWithToolTip));
                    this.ToolTipTimer.Tick += new EventHandler(this.OnRaiseToolTipClosingEvent);
                    this.ToolTipTimer.Start();
                }
            }
        }

        internal void OnRaiseToolTipClosingEvent(object sender, EventArgs e)
        {
            this.RaiseToolTipClosingEvent(false);
        }

        private void RaiseToolTipClosingEvent(bool reset)
        {
            this.ResetToolTipTimer();
            if (reset)
            {
                this.LastChecked = null;
            }
            DependencyObject lastMouseOverWithToolTip = this.LastMouseOverWithToolTip;
            if (lastMouseOverWithToolTip != null && this._currentToolTip != null)
            {
                bool isOpen = this._currentToolTip.IsOpen;
                try
                {
                    if (isOpen)
                    {
                        IInputElement inputElement = lastMouseOverWithToolTip as IInputElement;
                        if (inputElement != null)
                        {
                            inputElement.RaiseEvent(new ToolTipEventArgs(false));
                        }
                    }
                }
                finally
                {
                    if (isOpen)
                    {
                        this._currentToolTip.IsOpen = false;
                        if (this._currentToolTip != null)
                        {
                            this._forceCloseTimer = new DispatcherTimer(DispatcherPriority.Normal);
                            this._forceCloseTimer.Interval = Popup.AnimationDelayTime;
                            this._forceCloseTimer.Tick += new EventHandler(this.OnForceClose);
                            this._forceCloseTimer.Tag = this._currentToolTip;
                            this._forceCloseTimer.Start();
                        }
                        this._quickShow = true;
                        this.ToolTipTimer = new DispatcherTimer(DispatcherPriority.Normal);
                        this.ToolTipTimer.Interval = TimeSpan.FromMilliseconds((double)ToolTipService.GetBetweenShowDelay(lastMouseOverWithToolTip));
                        this.ToolTipTimer.Tick += new EventHandler(this.OnBetweenShowDelay);
                        this.ToolTipTimer.Start();
                    }
                    else
                    {
                        this._currentToolTip.ClearValue(PopupControlService.OwnerProperty);
                        if (this._ownToolTip)
                        {
                            BindingOperations.ClearBinding(this._currentToolTip, ContentControl.ContentProperty);
                        }
                    }
                    this._currentToolTip = null;
                }
            }
        }

        private void OnToolTipClosed(object sender, EventArgs e)
        {
            ToolTip toolTip = (ToolTip)sender;
            toolTip.Closed -= new RoutedEventHandler(this.OnToolTipClosed);
            toolTip.ClearValue(PopupControlService.OwnerProperty);
            if ((bool)toolTip.GetValue(PopupControlService.ServiceOwnedProperty))
            {
                BindingOperations.ClearBinding(toolTip, ContentControl.ContentProperty);
            }
        }

        private void OnForceClose(object sender, EventArgs e)
        {
            this._forceCloseTimer.Stop();
            ((ToolTip)this._forceCloseTimer.Tag).ForceClose();
            this._forceCloseTimer = null;
        }

        private void OnBetweenShowDelay(object source, EventArgs e)
        {
            this.ResetToolTipTimer();
        }

        [SecurityCritical]
        private void RaiseContextMenuOpeningEvent(KeyEventArgs e)
        {
            IInputElement inputElement = e.OriginalSource as IInputElement;
            if (inputElement != null && this.RaiseContextMenuOpeningEvent(inputElement, -1.0, -1.0, e.UserInitiated))
            {
                e.Handled = true;
            }
        }

        [SecurityCritical]
        private bool RaiseContextMenuOpeningEvent(IInputElement source, double x, double y, bool userInitiated)
        {
            ContextMenuEventArgs contextMenuEventArgs = new ContextMenuEventArgs(source, true, x, y);
            DependencyObject dependencyObject = source as DependencyObject;
            if (userInitiated && dependencyObject != null)
            {
                if (InputElement.IsUIElement(dependencyObject))
                {
                    ((UIElement)dependencyObject).RaiseEvent(contextMenuEventArgs, userInitiated);
                }
                else if (InputElement.IsContentElement(dependencyObject))
                {
                    ((ContentElement)dependencyObject).RaiseEvent(contextMenuEventArgs, userInitiated);
                }
                else if (InputElement.IsUIElement3D(dependencyObject))
                {
                    ((UIElement3D)dependencyObject).RaiseEvent(contextMenuEventArgs, userInitiated);
                }
                else
                {
                    source.RaiseEvent(contextMenuEventArgs);
                }
            }
            else
            {
                source.RaiseEvent(contextMenuEventArgs);
            }
            if (contextMenuEventArgs.Handled)
            {
                this.RaiseToolTipClosingEvent(true);
                return true;
            }
            DependencyObject targetElement = contextMenuEventArgs.TargetElement;
            if (targetElement != null && ContextMenuService.ContextMenuIsEnabled(targetElement))
            {
                ContextMenu contextMenu = ContextMenuService.GetContextMenu(targetElement) as ContextMenu;
                contextMenu.SetValue(PopupControlService.OwnerProperty, targetElement);
                contextMenu.Closed += new RoutedEventHandler(this.OnContextMenuClosed);
                if (x == -1.0 && y == -1.0)
                {
                    contextMenu.Placement = PlacementMode.Center;
                }
                else
                {
                    contextMenu.Placement = PlacementMode.MousePoint;
                }
                this.RaiseToolTipClosingEvent(true);
                contextMenu.SetCurrentValueInternal(ContextMenu.IsOpenProperty, BooleanBoxes.TrueBox);
                return true;
            }
            return false;
        }

        private void OnContextMenuClosed(object source, RoutedEventArgs e)
        {
            ContextMenu contextMenu = source as ContextMenu;
            if (contextMenu != null)
            {
                contextMenu.Closed -= new RoutedEventHandler(this.OnContextMenuClosed);
                DependencyObject dependencyObject = (DependencyObject)contextMenu.GetValue(PopupControlService.OwnerProperty);
                if (dependencyObject != null)
                {
                    contextMenu.ClearValue(PopupControlService.OwnerProperty);
                    UIElement target = PopupControlService.GetTarget(dependencyObject);
                    if (target != null && !PopupControlService.IsPresentationSourceNull(target))
                    {
                        object arg_6A_0;
                        if (!(dependencyObject is ContentElement) && !(dependencyObject is UIElement3D))
                        {
                            IInputElement inputElement = target;
                            arg_6A_0 = inputElement;
                        }
                        else
                        {
                            arg_6A_0 = (IInputElement)dependencyObject;
                        }
                        object expr_6A = arg_6A_0;
                        ContextMenuEventArgs e2 = new ContextMenuEventArgs(expr_6A, false);
                        ((IInputElement)expr_6A).RaiseEvent(e2);
                    }
                }
            }
        }

        [SecurityCritical, SecurityTreatAsSafe]
        private static bool IsPresentationSourceNull(DependencyObject uie)
        {
            return PresentationSource.CriticalFromVisual(uie) == null;
        }

        internal static DependencyObject FindParent(DependencyObject o)
        {
            DependencyObject dependencyObject = o as Visual;
            if (dependencyObject == null)
            {
                dependencyObject = (o as Visual3D);
            }
            ContentElement contentElement = (dependencyObject == null) ? (o as ContentElement) : null;
            if (contentElement != null)
            {
                o = ContentOperations.GetParent(contentElement);
                if (o != null)
                {
                    return o;
                }
                FrameworkContentElement frameworkContentElement = contentElement as FrameworkContentElement;
                if (frameworkContentElement != null)
                {
                    return frameworkContentElement.Parent;
                }
            }
            else if (dependencyObject != null)
            {
                return VisualTreeHelper.GetParent(dependencyObject);
            }
            return null;
        }

        internal static DependencyObject FindContentElementParent(ContentElement ce)
        {
            DependencyObject dependencyObject = null;
            DependencyObject dependencyObject2 = ce;
            while (dependencyObject2 != null)
            {
                dependencyObject = (dependencyObject2 as Visual);
                if (dependencyObject != null)
                {
                    break;
                }
                dependencyObject = (dependencyObject2 as Visual3D);
                if (dependencyObject != null)
                {
                    break;
                }
                ce = (dependencyObject2 as ContentElement);
                if (ce == null)
                {
                    break;
                }
                dependencyObject2 = ContentOperations.GetParent(ce);
                if (dependencyObject2 == null)
                {
                    FrameworkContentElement frameworkContentElement = ce as FrameworkContentElement;
                    if (frameworkContentElement != null)
                    {
                        dependencyObject2 = frameworkContentElement.Parent;
                    }
                }
            }
            return dependencyObject;
        }

        internal static bool IsElementEnabled(DependencyObject o)
        {
            bool result = true;
            UIElement uIElement = o as UIElement;
            ContentElement contentElement = (uIElement == null) ? (o as ContentElement) : null;
            UIElement3D uIElement3D = (uIElement == null && contentElement == null) ? (o as UIElement3D) : null;
            if (uIElement != null)
            {
                result = uIElement.IsEnabled;
            }
            else if (contentElement != null)
            {
                result = contentElement.IsEnabled;
            }
            else if (uIElement3D != null)
            {
                result = uIElement3D.IsEnabled;
            }
            return result;
        }

        private static UIElement GetTarget(DependencyObject o)
        {
            UIElement uIElement = o as UIElement;
            if (uIElement == null)
            {
                ContentElement contentElement = o as ContentElement;
                if (contentElement != null)
                {
                    DependencyObject dependencyObject = PopupControlService.FindContentElementParent(contentElement);
                    uIElement = (dependencyObject as UIElement);
                    if (uIElement == null)
                    {
                        UIElement3D uIElement3D = dependencyObject as UIElement3D;
                        if (uIElement3D != null)
                        {
                            uIElement = UIElementHelper.GetContainingUIElement2D(uIElement3D);
                        }
                    }
                }
                else
                {
                    UIElement3D uIElement3D2 = o as UIElement3D;
                    if (uIElement3D2 != null)
                    {
                        uIElement = UIElementHelper.GetContainingUIElement2D(uIElement3D2);
                    }
                }
            }
            return uIElement;
        }

        private static void OnOwnerChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            if (o is ContextMenu)
            {
                o.CoerceValue(ContextMenu.HorizontalOffsetProperty);
                o.CoerceValue(ContextMenu.VerticalOffsetProperty);
                o.CoerceValue(ContextMenu.PlacementTargetProperty);
                o.CoerceValue(ContextMenu.PlacementRectangleProperty);
                o.CoerceValue(ContextMenu.PlacementProperty);
                o.CoerceValue(ContextMenu.HasDropShadowProperty);
                return;
            }
            if (o is ToolTip)
            {
                o.CoerceValue(ToolTip.HorizontalOffsetProperty);
                o.CoerceValue(ToolTip.VerticalOffsetProperty);
                o.CoerceValue(ToolTip.PlacementTargetProperty);
                o.CoerceValue(ToolTip.PlacementRectangleProperty);
                o.CoerceValue(ToolTip.PlacementProperty);
                o.CoerceValue(ToolTip.HasDropShadowProperty);
            }
        }

        internal static object CoerceProperty(DependencyObject o, object value, DependencyProperty dp)
        {
            DependencyObject dependencyObject = (DependencyObject)o.GetValue(PopupControlService.OwnerProperty);
            if (dependencyObject != null)
            {
                bool flag;
                if (dependencyObject.GetValueSource(dp, null, out flag) != BaseValueSourceInternal.Default | flag)
                {
                    return dependencyObject.GetValue(dp);
                }
                if (dp == ToolTip.PlacementTargetProperty || dp == ContextMenu.PlacementTargetProperty)
                {
                    UIElement target = PopupControlService.GetTarget(dependencyObject);
                    if (target != null)
                    {
                        return target;
                    }
                }
            }
            return value;
        }
    }
}
