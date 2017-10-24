using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;

namespace System.Windows
{
    /// <summary>Provides a WPF framework-level set of properties, events, and methods for Windows Presentation Foundation (WPF) elements. This class represents the provided WPF framework-level implementation that is built on the WPF core-level APIs that are defined by <see cref="T:System.Windows.UIElement" />.</summary>
    [RuntimeNameProperty("Name"), UsableDuringInitialization(true), XmlLangProperty("Language"), StyleTypedProperty(Property = "FocusVisualStyle", StyleTargetType = typeof(Control))]
    public class FrameworkElement : UIElement, IFrameworkInputElement, IInputElement, ISupportInitialize, IHaveResources, IQueryAmbient
    {
        private struct MinMax
        {
            internal double minWidth;

            internal double maxWidth;

            internal double minHeight;

            internal double maxHeight;

            internal MinMax(FrameworkElement e)
            {
                this.maxHeight = e.MaxHeight;
                this.minHeight = e.MinHeight;
                double num = e.Height;
                double num2 = DoubleUtil.IsNaN(num) ? double.PositiveInfinity : num;
                this.maxHeight = Math.Max(Math.Min(num2, this.maxHeight), this.minHeight);
                num2 = (DoubleUtil.IsNaN(num) ? 0.0 : num);
                this.minHeight = Math.Max(Math.Min(this.maxHeight, num2), this.minHeight);
                this.maxWidth = e.MaxWidth;
                this.minWidth = e.MinWidth;
                num = e.Width;
                double num3 = DoubleUtil.IsNaN(num) ? double.PositiveInfinity : num;
                this.maxWidth = Math.Max(Math.Min(num3, this.maxWidth), this.minWidth);
                num3 = (DoubleUtil.IsNaN(num) ? 0.0 : num);
                this.minWidth = Math.Max(Math.Min(this.maxWidth, num3), this.minWidth);
            }
        }

        private class LayoutTransformData
        {
            internal Size UntransformedDS;

            internal Size TransformedUnroundedDS;

            private Transform _transform;

            internal Transform Transform
            {
                get
                {
                    return this._transform;
                }
            }

            internal void CreateTransformSnapshot(Transform sourceTransform)
            {
                this._transform = new MatrixTransform(sourceTransform.Value);
                this._transform.Freeze();
            }
        }

        private class FrameworkServices
        {
            internal KeyboardNavigation _keyboardNavigation;

            internal PopupControlService _popupControlService;

            internal FrameworkServices()
            {
                this._keyboardNavigation = new KeyboardNavigation();
                this._popupControlService = new PopupControlService();
            }
        }

        private static readonly Type _typeofThis;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Style" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Style" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty StyleProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.OverridesDefaultStyle" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.OverridesDefaultStyle" /> dependency property.</returns>
        public static readonly DependencyProperty OverridesDefaultStyleProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.UseLayoutRounding" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.UseLayoutRounding" /> dependency property.</returns>
        public static readonly DependencyProperty UseLayoutRoundingProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.DefaultStyleKey" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.DefaultStyleKey" /> dependency property identifier.</returns>
        protected internal static readonly DependencyProperty DefaultStyleKeyProperty;

        internal static readonly NumberSubstitution DefaultNumberSubstitution;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.DataContext" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.DataContext" /> dependency property identifier.</returns>
        public static readonly DependencyProperty DataContextProperty;

        internal static readonly EventPrivateKey DataContextChangedKey;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.BindingGroup" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.BindingGroup" /> dependency property.</returns>
        public static readonly DependencyProperty BindingGroupProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Language" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.Language" /> dependency property identifier.</returns>
        public static readonly DependencyProperty LanguageProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Name" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Name" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty NameProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Tag" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Tag" /> dependency property.</returns>
        public static readonly DependencyProperty TagProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.InputScope" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.InputScope" /> dependency property.</returns>
        public static readonly DependencyProperty InputScopeProperty;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event.Routed event identifiers are created when routed events are registered. These identifiers contain an identifying name, owner type, handler type, routing strategy, and utility method for adding owners for the event. You can use these identifiers to add class handlers. For more information about registering routed events, see <see cref="M:System.Windows.EventManager.RegisterRoutedEvent(System.String,System.Windows.RoutingStrategy,System.Type,System.Type)" />. For more information about using routed event identifiers to add class handlers, see <see cref="M:System.Windows.EventManager.RegisterClassHandler(System.Type,System.Windows.RoutedEvent,System.Delegate)" />.</returns>
        public static readonly RoutedEvent RequestBringIntoViewEvent;

        /// <summary> Identifies the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.RequestBringIntoView" /> routed event.</returns>
        public static readonly RoutedEvent SizeChangedEvent;

        private static PropertyMetadata _actualWidthMetadata;

        private static readonly DependencyPropertyKey ActualWidthPropertyKey;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> dependency property.</returns>
        public static readonly DependencyProperty ActualWidthProperty;

        private static PropertyMetadata _actualHeightMetadata;

        private static readonly DependencyPropertyKey ActualHeightPropertyKey;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> dependency property.</returns>
        public static readonly DependencyProperty ActualHeightProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.LayoutTransform" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.LayoutTransform" /> dependency property identifier.</returns>
        public static readonly DependencyProperty LayoutTransformProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.Width" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Width" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty WidthProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.MinWidth" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MinWidth" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MinWidthProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.MaxWidth" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MaxWidth" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MaxWidthProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Height" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Height" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty HeightProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.MinHeight" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MinHeight" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MinHeightProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.MaxHeight" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.MaxHeight" /> dependency property.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MaxHeightProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> dependency property identifier.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty FlowDirectionProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.Margin" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.Margin" /> dependency property identifier.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MarginProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.HorizontalAlignment" /> dependency property identifier.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty HorizontalAlignmentProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.VerticalAlignment" /> dependency property identifier.</returns>
        [CommonDependencyProperty]
        public static readonly DependencyProperty VerticalAlignmentProperty;

        private static Style _defaultFocusVisualStyle;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.FocusVisualStyle" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.FocusVisualStyle" /> dependency property.</returns>
        public static readonly DependencyProperty FocusVisualStyleProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.Cursor" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.Cursor" /> dependency property.</returns>
        public static readonly DependencyProperty CursorProperty;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ForceCursor" /> dependency property. </summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.FrameworkElement.ForceCursor" /> dependency property.</returns>
        public static readonly DependencyProperty ForceCursorProperty;

        internal static readonly EventPrivateKey InitializedKey;

        internal static readonly DependencyPropertyKey LoadedPendingPropertyKey;

        internal static readonly DependencyProperty LoadedPendingProperty;

        internal static readonly DependencyPropertyKey UnloadedPendingPropertyKey;

        internal static readonly DependencyProperty UnloadedPendingProperty;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.Loaded" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.Loaded" /> routed event.</returns>
        public static readonly RoutedEvent LoadedEvent;

        /// <summary> Identifies the <see cref="E:System.Windows.FrameworkElement.Unloaded" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.Unloaded" /> routed event.</returns>
        public static readonly RoutedEvent UnloadedEvent;

        /// <summary>Identifies the <see cref="P:System.Windows.FrameworkElement.ToolTip" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.ToolTip" /> dependency property identifier.</returns>
        public static readonly DependencyProperty ToolTipProperty;

        /// <summary> Identifies the <see cref="P:System.Windows.FrameworkElement.ContextMenu" /> dependency property. </summary>
        /// <returns>The <see cref="P:System.Windows.FrameworkElement.ContextMenu" /> dependency property identifier.</returns>
        public static readonly DependencyProperty ContextMenuProperty;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event.</returns>
        public static readonly RoutedEvent ToolTipOpeningEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event.</returns>
        public static readonly RoutedEvent ToolTipClosingEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event.</returns>
        public static readonly RoutedEvent ContextMenuOpeningEvent;

        /// <summary>Identifies the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event. </summary>
        /// <returns>The identifier for the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event.</returns>
        public static readonly RoutedEvent ContextMenuClosingEvent;

        private Style _themeStyleCache;

        private static readonly UncommonField<SizeBox> UnclippedDesiredSizeField;

        private static readonly UncommonField<FrameworkElement.LayoutTransformData> LayoutTransformDataField;

        private Style _styleCache;

        internal static readonly UncommonField<ResourceDictionary> ResourcesField;

        internal DependencyObject _templatedParent;

        private UIElement _templateChild;

        private InternalFlags _flags;

        private InternalFlags2 _flags2 = InternalFlags2.Default;

        internal static DependencyObjectType UIElementDType;

        private static DependencyObjectType _controlDType;

        private static DependencyObjectType _contentPresenterDType;

        private static DependencyObjectType _pageFunctionBaseDType;

        private static DependencyObjectType _pageDType;

        [ThreadStatic]
        private static FrameworkElement.FrameworkServices _frameworkServices;

        internal static readonly EventPrivateKey ResourcesChangedKey;

        internal static readonly EventPrivateKey InheritedPropertyChangedKey;

        internal new static DependencyObjectType DType;

        private new DependencyObject _parent;

        private FrugalObjectList<DependencyProperty> _inheritableProperties;

        private static readonly UncommonField<DependencyObject> InheritanceContextField;

        private static readonly UncommonField<DependencyObject> MentorField;

        /// <summary>Occurs when the target value changes for any property binding on this element. </summary>
        public event EventHandler<DataTransferEventArgs> TargetUpdated
        {
            add
            {
                base.AddHandler(Binding.TargetUpdatedEvent, value);
            }
            remove
            {
                base.RemoveHandler(Binding.TargetUpdatedEvent, value);
            }
        }

        /// <summary>Occurs when the source value changes for any existing property binding on this element.</summary>
        public event EventHandler<DataTransferEventArgs> SourceUpdated
        {
            add
            {
                base.AddHandler(Binding.SourceUpdatedEvent, value);
            }
            remove
            {
                base.RemoveHandler(Binding.SourceUpdatedEvent, value);
            }
        }

        /// <summary>Occurs when the data context for this element changes. </summary>
        public event DependencyPropertyChangedEventHandler DataContextChanged
        {
            add
            {
                this.EventHandlersStoreAdd(FrameworkElement.DataContextChangedKey, value);
            }
            remove
            {
                this.EventHandlersStoreRemove(FrameworkElement.DataContextChangedKey, value);
            }
        }

        /// <summary>Occurs when <see cref="M:System.Windows.FrameworkElement.BringIntoView(System.Windows.Rect)" /> is called on this element. </summary>
        public event RequestBringIntoViewEventHandler RequestBringIntoView
        {
            add
            {
                base.AddHandler(FrameworkElement.RequestBringIntoViewEvent, value, false);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.RequestBringIntoViewEvent, value);
            }
        }

        /// <summary>Occurs when either the <see cref="P:System.Windows.FrameworkElement.ActualHeight" /> or the <see cref="P:System.Windows.FrameworkElement.ActualWidth" /> properties change value on this element. </summary>
        public event SizeChangedEventHandler SizeChanged
        {
            add
            {
                base.AddHandler(FrameworkElement.SizeChangedEvent, value, false);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.SizeChangedEvent, value);
            }
        }

        /// <summary>Occurs when this <see cref="T:System.Windows.FrameworkElement" /> is initialized. This event coincides with cases where the value of the <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> property changes from false (or undefined) to true. </summary>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public event EventHandler Initialized
        {
            add
            {
                this.EventHandlersStoreAdd(FrameworkElement.InitializedKey, value);
            }
            remove
            {
                this.EventHandlersStoreRemove(FrameworkElement.InitializedKey, value);
            }
        }

        /// <summary>Occurs when the element is laid out, rendered, and ready for interaction.</summary>
        public event RoutedEventHandler Loaded
        {
            add
            {
                base.AddHandler(FrameworkElement.LoadedEvent, value, false);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.LoadedEvent, value);
            }
        }

        /// <summary>Occurs when the element is removed from within an element tree of loaded elements.</summary>
        public event RoutedEventHandler Unloaded
        {
            add
            {
                base.AddHandler(FrameworkElement.UnloadedEvent, value, false);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.UnloadedEvent, value);
            }
        }

        /// <summary>Occurs when any tooltip on the element is opened. </summary>
        public event ToolTipEventHandler ToolTipOpening
        {
            add
            {
                base.AddHandler(FrameworkElement.ToolTipOpeningEvent, value);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.ToolTipOpeningEvent, value);
            }
        }

        /// <summary>Occurs just before any tooltip on the element is closed. </summary>
        public event ToolTipEventHandler ToolTipClosing
        {
            add
            {
                base.AddHandler(FrameworkElement.ToolTipClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.ToolTipClosingEvent, value);
            }
        }

        /// <summary>Occurs when any context menu on the element is opened. </summary>
        public event ContextMenuEventHandler ContextMenuOpening
        {
            add
            {
                base.AddHandler(FrameworkElement.ContextMenuOpeningEvent, value);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.ContextMenuOpeningEvent, value);
            }
        }

        /// <summary>Occurs just before any context menu on the element is closed. </summary>
        public event ContextMenuEventHandler ContextMenuClosing
        {
            add
            {
                base.AddHandler(FrameworkElement.ContextMenuClosingEvent, value);
            }
            remove
            {
                base.RemoveHandler(FrameworkElement.ContextMenuClosingEvent, value);
            }
        }

        internal event EventHandler ResourcesChanged
        {
            add
            {
                this.PotentiallyHasMentees = true;
                this.EventHandlersStoreAdd(FrameworkElement.ResourcesChangedKey, value);
            }
            remove
            {
                this.EventHandlersStoreRemove(FrameworkElement.ResourcesChangedKey, value);
            }
        }

        internal event InheritedPropertyChangedEventHandler InheritedPropertyChanged
        {
            add
            {
                this.PotentiallyHasMentees = true;
                this.EventHandlersStoreAdd(FrameworkElement.InheritedPropertyChangedKey, value);
            }
            remove
            {
                this.EventHandlersStoreRemove(FrameworkElement.InheritedPropertyChangedKey, value);
            }
        }

        /// <summary>Gets or sets the style used by this element when it is rendered.  </summary>
        /// <returns>The applied, nondefault style for the element, if present. Otherwise, null. The default for a default-constructed <see cref="T:System.Windows.FrameworkElement" /> is null.</returns>
        public Style Style
        {
            get
            {
                return this._styleCache;
            }
            set
            {
                base.SetValue(FrameworkElement.StyleProperty, value);
            }
        }

        /// <summary>Gets or sets a value that indicates whether this element incorporates style properties from theme styles. </summary>
        /// <returns>true if this element does not use theme style properties; all style-originating properties come from local application styles, and theme style properties do not apply. false if application styles apply first, and then theme styles apply for properties that were not specifically set in application styles. The default is false.</returns>
        public bool OverridesDefaultStyle
        {
            get
            {
                return (bool)base.GetValue(FrameworkElement.OverridesDefaultStyleProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.OverridesDefaultStyleProperty, BooleanBoxes.Box(value));
            }
        }

        /// <summary>Gets or sets a value that indicates whether layout rounding should be applied to this element's size and position during layout. </summary>
        /// <returns>true if layout rounding is applied; otherwise, false. The default is false.</returns>
        public bool UseLayoutRounding
        {
            get
            {
                return (bool)base.GetValue(FrameworkElement.UseLayoutRoundingProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.UseLayoutRoundingProperty, BooleanBoxes.Box(value));
            }
        }

        /// <summary>Gets or sets the key to use to reference the style for this control, when theme styles are used or defined.</summary>
        /// <returns>The style key. To work correctly as part of theme style lookup, this value is expected to be the <see cref="T:System.Type" /> of the control being styled.</returns>
        protected internal object DefaultStyleKey
        {
            get
            {
                return base.GetValue(FrameworkElement.DefaultStyleKeyProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.DefaultStyleKeyProperty, value);
            }
        }

        internal Style ThemeStyle
        {
            get
            {
                return this._themeStyleCache;
            }
        }

        internal virtual DependencyObjectType DTypeThemeStyleKey
        {
            get
            {
                return null;
            }
        }

        internal virtual FrameworkTemplate TemplateInternal
        {
            get
            {
                return null;
            }
        }

        internal virtual FrameworkTemplate TemplateCache
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        /// <summary>Gets the collection of triggers established directly on this element, or in child elements. </summary>
        /// <returns>A strongly typed collection of <see cref="T:System.Windows.Trigger" /> objects.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TriggerCollection Triggers
        {
            get
            {
                TriggerCollection triggerCollection = EventTrigger.TriggerCollectionField.GetValue(this);
                if (triggerCollection == null)
                {
                    triggerCollection = new TriggerCollection(this);
                    EventTrigger.TriggerCollectionField.SetValue(this, triggerCollection);
                }
                return triggerCollection;
            }
        }

        /// <summary>Gets a reference to the template parent of this element. This property is not relevant if the element was not created through a template.</summary>
        /// <returns>The element whose <see cref="T:System.Windows.FrameworkTemplate" /> <see cref="P:System.Windows.FrameworkTemplate.VisualTree" /> caused this element to be created. This value is frequently null; see Remarks.</returns>
        public DependencyObject TemplatedParent
        {
            get
            {
                return this._templatedParent;
            }
        }

        internal bool IsTemplateRoot
        {
            get
            {
                return this.TemplateChildIndex == 1;
            }
        }

        internal virtual UIElement TemplateChild
        {
            get
            {
                return this._templateChild;
            }
            set
            {
                if (value != this._templateChild)
                {
                    base.RemoveVisualChild(this._templateChild);
                    this._templateChild = value;
                    base.AddVisualChild(value);
                }
            }
        }

        internal virtual FrameworkElement StateGroupsRoot
        {
            get
            {
                return this._templateChild as FrameworkElement;
            }
        }

        /// <summary>Gets the number of visual child elements within this element.</summary>
        /// <returns>The number of visual child elements for this element.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                if (this._templateChild != null)
                {
                    return 1;
                }
                return 0;
            }
        }

        internal bool HasResources
        {
            get
            {
                ResourceDictionary value = FrameworkElement.ResourcesField.GetValue(this);
                return value != null && (value.Count > 0 || value.MergedDictionaries.Count > 0);
            }
        }

        /// <summary> Gets or sets the locally-defined resource dictionary. </summary>
        /// <returns>The current locally-defined dictionary of resources, where each resource can be accessed by key.</returns>
        [Ambient]
        public ResourceDictionary Resources
        {
            get
            {
                ResourceDictionary resourceDictionary = FrameworkElement.ResourcesField.GetValue(this);
                if (resourceDictionary == null)
                {
                    resourceDictionary = new ResourceDictionary();
                    resourceDictionary.AddOwner(this);
                    FrameworkElement.ResourcesField.SetValue(this, resourceDictionary);
                    if (TraceResourceDictionary.IsEnabled)
                    {
                        TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.NewResourceDictionary, this, 0, resourceDictionary);
                    }
                }
                return resourceDictionary;
            }
            set
            {
                ResourceDictionary value2 = FrameworkElement.ResourcesField.GetValue(this);
                FrameworkElement.ResourcesField.SetValue(this, value);
                if (TraceResourceDictionary.IsEnabled)
                {
                    TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.NewResourceDictionary, this, value2, value);
                }
                if (value2 != null)
                {
                    value2.RemoveOwner(this);
                }
                if (value != null && !value.ContainsOwner(this))
                {
                    value.AddOwner(this);
                }
                if (value2 != value)
                {
                    TreeWalkHelper.InvalidateOnResourcesChange(this, null, new ResourcesChangeInfo(value2, value));
                }
                if (TraceResourceDictionary.IsEnabled)
                {
                    TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.NewResourceDictionary, this, value2, value);
                }
            }
        }

        ResourceDictionary IHaveResources.Resources
        {
            get
            {
                return this.Resources;
            }
            set
            {
                this.Resources = value;
            }
        }

        /// <summary>Gets or sets the scope limits for property value inheritance, resource key lookup, and RelativeSource FindAncestor lookup.</summary>
        /// <returns>A value of the enumeration. The default is <see cref="F:System.Windows.InheritanceBehavior.Default" />.</returns>
        protected internal InheritanceBehavior InheritanceBehavior
        {
            get
            {
                return (InheritanceBehavior)((this._flags & (InternalFlags)56u) >> 3);
            }
            set
            {
                if (this.IsInitialized)
                {
                    throw new InvalidOperationException(SR.Get("Illegal_InheritanceBehaviorSettor"));
                }
                if (value < InheritanceBehavior.Default || value > InheritanceBehavior.SkipAllNext)
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(InheritanceBehavior));
                }
                uint num = (uint)((uint)value << 3);
                this._flags = (InternalFlags)((num & 56u) | (uint)(this._flags & (InternalFlags)4294967239u));
                if (this._parent != null)
                {
                    TreeWalkHelper.InvalidateOnTreeChange(this, null, this._parent, true);
                    return;
                }
            }
        }

        /// <summary> Gets or sets the data context for an element when it participates in data binding.</summary>
        /// <returns>The object to use as data context.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizability(LocalizationCategory.NeverLocalize)]
        public object DataContext
        {
            get
            {
                return base.GetValue(FrameworkElement.DataContextProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.DataContextProperty, value);
            }
        }

        /// <summary>Gets or sets the <see cref="T:System.Windows.Data.BindingGroup" /> that is used for the element.</summary>
        /// <returns>The <see cref="T:System.Windows.Data.BindingGroup" /> that is used for the element.</returns>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Localizability(LocalizationCategory.NeverLocalize)]
        public BindingGroup BindingGroup
        {
            get
            {
                return (BindingGroup)base.GetValue(FrameworkElement.BindingGroupProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.BindingGroupProperty, value);
            }
        }

        /// <summary>Gets or sets localization/globalization language information that applies to an element.</summary>
        /// <returns>The language information for this element. The default value is an <see cref="T:System.Windows.Markup.XmlLanguage" /> with its <see cref="P:System.Windows.Markup.XmlLanguage.IetfLanguageTag" /> value set to the string "en-US".</returns>
        public XmlLanguage Language
        {
            get
            {
                return (XmlLanguage)base.GetValue(FrameworkElement.LanguageProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.LanguageProperty, value);
            }
        }

        /// <summary>Gets or sets the identifying name of the element. The name provides a reference so that code-behind, such as event handler code, can refer to a markup element after it is constructed during processing by a XAML processor.</summary>
        /// <returns>The name of the element. The default is an empty string.</returns>
        [MergableProperty(false), Localizability(LocalizationCategory.NeverLocalize), DesignerSerializationOptions(DesignerSerializationOptions.SerializeAsAttribute)]
        public string Name
        {
            get
            {
                return (string)base.GetValue(FrameworkElement.NameProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.NameProperty, value);
            }
        }

        /// <summary>Gets or sets an arbitrary object value that can be used to store custom information about this element.</summary>
        /// <returns>The intended value. This property has no default value.</returns>
        [Localizability(LocalizationCategory.NeverLocalize)]
        public object Tag
        {
            get
            {
                return base.GetValue(FrameworkElement.TagProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.TagProperty, value);
            }
        }

        /// <summary>Gets or sets the context for input used by this <see cref="T:System.Windows.FrameworkElement" />. </summary>
        /// <returns>The input scope, which modifies how input from alternative input methods is interpreted. The default value is null (which results in a default handling of commands).</returns>
        public InputScope InputScope
        {
            get
            {
                return (InputScope)base.GetValue(FrameworkElement.InputScopeProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.InputScopeProperty, value);
            }
        }

        /// <summary>Gets the rendered width of this element.</summary>
        /// <returns>The element's width, as a value in device-independent units (1/96th inch per unit). The default value is 0 (zero).</returns>
        public double ActualWidth
        {
            get
            {
                return base.RenderSize.Width;
            }
        }

        /// <summary>Gets the rendered height of this element.</summary>
        /// <returns>The element's height, as a value in device-independent units (1/96th inch per unit). The default value is 0 (zero).</returns>
        public double ActualHeight
        {
            get
            {
                return base.RenderSize.Height;
            }
        }

        /// <summary> Gets or sets a graphics transformation that should apply to this element when  layout is performed.</summary>
        /// <returns>The transform this element should use. The default is <see cref="P:System.Windows.Media.Transform.Identity" />.</returns>
        public Transform LayoutTransform
        {
            get
            {
                return (Transform)base.GetValue(FrameworkElement.LayoutTransformProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.LayoutTransformProperty, value);
            }
        }

        /// <summary> Gets or sets the width of the element.</summary>
        /// <returns>The width of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.NaN" />. This value must be equal to or greater than 0.0. See Remarks for upper bound information.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double Width
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.WidthProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.WidthProperty, value);
            }
        }

        /// <summary> Gets or sets the minimum width constraint of the element.</summary>
        /// <returns>The minimum width of the element, in device-independent units (1/96th inch per unit). The default value is 0.0. This value can be any value equal to or greater than 0.0. However, <see cref="F:System.Double.PositiveInfinity" /> is not valid, nor is <see cref="F:System.Double.NaN" />.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double MinWidth
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.MinWidthProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.MinWidthProperty, value);
            }
        }

        /// <summary>Gets or sets the maximum width constraint of the element.</summary>
        /// <returns>The maximum width of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.PositiveInfinity" />. This value can be any value equal to or greater than 0.0. <see cref="F:System.Double.PositiveInfinity" /> is also valid.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double MaxWidth
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.MaxWidthProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.MaxWidthProperty, value);
            }
        }

        /// <summary> Gets or sets the suggested height of the element.</summary>
        /// <returns>The height of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.NaN" />. This value must be equal to or greater than 0.0. See Remarks for upper bound information.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double Height
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.HeightProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.HeightProperty, value);
            }
        }

        /// <summary>Gets or sets the minimum height constraint of the element.</summary>
        /// <returns>The minimum height of the element, in device-independent units (1/96th inch per unit). The default value is 0.0. This value can be any value equal to or greater than 0.0. However, <see cref="F:System.Double.PositiveInfinity" /> is NOT valid, nor is <see cref="F:System.Double.NaN" />.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double MinHeight
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.MinHeightProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.MinHeightProperty, value);
            }
        }

        /// <summary>Gets or sets the maximum height constraint of the element.</summary>
        /// <returns>The maximum height of the element, in device-independent units (1/96th inch per unit). The default value is <see cref="F:System.Double.PositiveInfinity" />. This value can be any value equal to or greater than 0.0. <see cref="F:System.Double.PositiveInfinity" /> is also valid.</returns>
        [TypeConverter(typeof(LengthConverter)), Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
        public double MaxHeight
        {
            get
            {
                return (double)base.GetValue(FrameworkElement.MaxHeightProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.MaxHeightProperty, value);
            }
        }

        /// <summary>Gets or sets the direction that text and other user interface (UI) elements flow within any parent element that controls their layout.</summary>
        /// <returns>The direction that text and other UI elements flow within their parent element, as a value of the enumeration. The default value is <see cref="F:System.Windows.FlowDirection.LeftToRight" />.</returns>
        [Localizability(LocalizationCategory.None)]
        public FlowDirection FlowDirection
        {
            get
            {
                if (!this.IsRightToLeft)
                {
                    return FlowDirection.LeftToRight;
                }
                return FlowDirection.RightToLeft;
            }
            set
            {
                base.SetValue(FrameworkElement.FlowDirectionProperty, value);
            }
        }

        /// <summary>Gets or sets the outer margin of an element.</summary>
        /// <returns>Provides margin values for the element. The default value is a <see cref="T:System.Windows.Thickness" /> with all properties equal to 0 (zero).</returns>
        public Thickness Margin
        {
            get
            {
                return (Thickness)base.GetValue(FrameworkElement.MarginProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.MarginProperty, value);
            }
        }

        /// <summary>Gets or sets the horizontal alignment characteristics applied to this element when it is composed within a parent element, such as a panel or items control.</summary>
        /// <returns>A horizontal alignment setting, as a value of the enumeration. The default is <see cref="F:System.Windows.HorizontalAlignment.Stretch" />.</returns>
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return (HorizontalAlignment)base.GetValue(FrameworkElement.HorizontalAlignmentProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.HorizontalAlignmentProperty, value);
            }
        }

        /// <summary>Gets or sets the vertical alignment characteristics applied to this element when it is composed within a parent element such as a panel or items control.</summary>
        /// <returns>A vertical alignment setting. The default is <see cref="F:System.Windows.VerticalAlignment.Stretch" />.</returns>
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return (VerticalAlignment)base.GetValue(FrameworkElement.VerticalAlignmentProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.VerticalAlignmentProperty, value);
            }
        }

        internal static Style DefaultFocusVisualStyle
        {
            get
            {
                if (FrameworkElement._defaultFocusVisualStyle == null)
                {
                    Style expr_0C = new Style();
                    expr_0C.Seal();
                    FrameworkElement._defaultFocusVisualStyle = expr_0C;
                }
                return FrameworkElement._defaultFocusVisualStyle;
            }
        }

        /// <summary>Gets or sets a property that enables customization of appearance, effects, or other style characteristics that will apply to this element when it captures keyboard focus.</summary>
        /// <returns>The desired style to apply on focus. The default value as declared in the dependency property is an empty static <see cref="T:System.Windows.Style" />. However, the effective value at run time is often (but not always) a style as supplied by theme support for controls. </returns>
        public Style FocusVisualStyle
        {
            get
            {
                return (Style)base.GetValue(FrameworkElement.FocusVisualStyleProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.FocusVisualStyleProperty, value);
            }
        }

        /// <summary>Gets or sets the cursor that displays when the mouse pointer is over this element.</summary>
        /// <returns>The cursor to display. The default value is defined as null per this dependency property. However, the practical default at run time will come from a variety of factors.</returns>
        public Cursor Cursor
        {
            get
            {
                return (Cursor)base.GetValue(FrameworkElement.CursorProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.CursorProperty, value);
            }
        }

        /// <summary>Gets or sets a value that indicates whether this <see cref="T:System.Windows.FrameworkElement" /> should force the user interface (UI) to render the cursor as declared by the <see cref="P:System.Windows.FrameworkElement.Cursor" /> property.</summary>
        /// <returns>true if cursor presentation while over this element is forced to use current <see cref="P:System.Windows.FrameworkElement.Cursor" /> settings for the cursor (including on all child elements); otherwise false. The default value is false.</returns>
        public bool ForceCursor
        {
            get
            {
                return (bool)base.GetValue(FrameworkElement.ForceCursorProperty);
            }
            set
            {
                base.SetValue(FrameworkElement.ForceCursorProperty, BooleanBoxes.Box(value));
            }
        }

        /// <summary>Gets a value that indicates whether this element has been initialized, either during processing by a XAML processor, or by explicitly having its <see cref="M:System.Windows.FrameworkElement.EndInit" /> method called. </summary>
        /// <returns>true if the element is initialized per the aforementioned XAML processing or method calls; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool IsInitialized
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.IsInitialized);
            }
        }

        /// <summary>Gets a value that indicates whether this element has been loaded for presentation. </summary>
        /// <returns>true if the current element is attached to an element tree; false if the element has never been attached to a loaded element tree. </returns>
        public bool IsLoaded
        {
            get
            {
                object[] loadedPending = this.LoadedPending;
                object[] unloadedPending = this.UnloadedPending;
                if (loadedPending != null || unloadedPending != null)
                {
                    return unloadedPending != null;
                }
                if (this.SubtreeHasLoadedChangeHandler)
                {
                    return this.IsLoadedCache;
                }
                return BroadcastEventHelper.IsParentLoaded(this);
            }
        }

        internal static PopupControlService PopupControlService
        {
            get
            {
                return FrameworkElement.EnsureFrameworkServices()._popupControlService;
            }
        }

        internal static KeyboardNavigation KeyboardNavigation
        {
            get
            {
                return FrameworkElement.EnsureFrameworkServices()._keyboardNavigation;
            }
        }

        /// <summary> Gets or sets the tool-tip object that is displayed for this element in the user interface (UI).</summary>
        /// <returns>The tooltip object. See Remarks below for details on why this parameter is not strongly typed.</returns>
        [Bindable(true), Category("Appearance"), Localizability(LocalizationCategory.ToolTip)]
        public object ToolTip
        {
            get
            {
                return ToolTipService.GetToolTip(this);
            }
            set
            {
                ToolTipService.SetToolTip(this, value);
            }
        }

        /// <summary> Gets or sets the context menu element that should appear whenever the context menu is requested through user interface (UI) from within this element.</summary>
        /// <returns>The context menu assigned to this element. </returns>
        public ContextMenu ContextMenu
        {
            get
            {
                return base.GetValue(FrameworkElement.ContextMenuProperty) as ContextMenu;
            }
            set
            {
                base.SetValue(FrameworkElement.ContextMenuProperty, value);
            }
        }

        internal bool HasResourceReference
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasResourceReferences);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasResourceReferences, value);
            }
        }

        internal bool IsLogicalChildrenIterationInProgress
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.IsLogicalChildrenIterationInProgress);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.IsLogicalChildrenIterationInProgress, value);
            }
        }

        internal bool InVisibilityCollapsedTree
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.InVisibilityCollapsedTree);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.InVisibilityCollapsedTree, value);
            }
        }

        internal bool SubtreeHasLoadedChangeHandler
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.TreeHasLoadedChangeHandler);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.TreeHasLoadedChangeHandler, value);
            }
        }

        internal bool IsLoadedCache
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.IsLoadedCache);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.IsLoadedCache, value);
            }
        }

        internal bool IsParentAnFE
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.IsParentAnFE);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.IsParentAnFE, value);
            }
        }

        internal bool IsTemplatedParentAnFE
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.IsTemplatedParentAnFE);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.IsTemplatedParentAnFE, value);
            }
        }

        internal bool HasLogicalChildren
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasLogicalChildren);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasLogicalChildren, value);
            }
        }

        private bool NeedsClipBounds
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.NeedsClipBounds);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.NeedsClipBounds, value);
            }
        }

        private bool HasWidthEverChanged
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasWidthEverChanged);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasWidthEverChanged, value);
            }
        }

        private bool HasHeightEverChanged
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasHeightEverChanged);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasHeightEverChanged, value);
            }
        }

        internal bool IsRightToLeft
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.IsRightToLeft);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.IsRightToLeft, value);
            }
        }

        internal int TemplateChildIndex
        {
            get
            {
                uint num = (uint)(this._flags2 & InternalFlags2.Default);
                if (num == 65535u)
                {
                    return -1;
                }
                return (int)num;
            }
            set
            {
                if (value < -1 || value >= 65535)
                {
                    throw new ArgumentOutOfRangeException("value", SR.Get("TemplateChildIndexOutOfRange"));
                }
                uint num = (uint)((value == -1) ? 65535 : value);
                this._flags2 = (InternalFlags2)(num | (uint)(this._flags2 & ~(InternalFlags2.R0 | InternalFlags2.R1 | InternalFlags2.R2 | InternalFlags2.R3 | InternalFlags2.R4 | InternalFlags2.R5 | InternalFlags2.R6 | InternalFlags2.R7 | InternalFlags2.R8 | InternalFlags2.R9 | InternalFlags2.RA | InternalFlags2.RB | InternalFlags2.RC | InternalFlags2.RD | InternalFlags2.RE | InternalFlags2.RF)));
            }
        }

        internal bool IsRequestingExpression
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.IsRequestingExpression);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.IsRequestingExpression, value);
            }
        }

        internal bool BypassLayoutPolicies
        {
            get
            {
                return this.ReadInternalFlag2((InternalFlags2)2147483648u);
            }
            set
            {
                this.WriteInternalFlag2((InternalFlags2)2147483648u, value);
            }
        }

        private static DependencyObjectType ControlDType
        {
            get
            {
                if (FrameworkElement._controlDType == null)
                {
                    FrameworkElement._controlDType = DependencyObjectType.FromSystemTypeInternal(typeof(Control));
                }
                return FrameworkElement._controlDType;
            }
        }

        private static DependencyObjectType ContentPresenterDType
        {
            get
            {
                if (FrameworkElement._contentPresenterDType == null)
                {
                    FrameworkElement._contentPresenterDType = DependencyObjectType.FromSystemTypeInternal(typeof(ContentPresenter));
                }
                return FrameworkElement._contentPresenterDType;
            }
        }

        private static DependencyObjectType PageDType
        {
            get
            {
                if (FrameworkElement._pageDType == null)
                {
                    FrameworkElement._pageDType = DependencyObjectType.FromSystemTypeInternal(typeof(Page));
                }
                return FrameworkElement._pageDType;
            }
        }

        private static DependencyObjectType PageFunctionBaseDType
        {
            get
            {
                if (FrameworkElement._pageFunctionBaseDType == null)
                {
                    FrameworkElement._pageFunctionBaseDType = DependencyObjectType.FromSystemTypeInternal(typeof(PageFunctionBase));
                }
                return FrameworkElement._pageFunctionBaseDType;
            }
        }

        internal override int EffectiveValuesInitialSize
        {
            get
            {
                return 7;
            }
        }

        internal static double DpiScaleX
        {
            get
            {
                if (SystemParameters.DpiX != 96)
                {
                    return (double)SystemParameters.DpiX / 96.0;
                }
                return 1.0;
            }
        }

        internal static double DpiScaleY
        {
            get
            {
                if (SystemParameters.Dpi != 96)
                {
                    return (double)SystemParameters.Dpi / 96.0;
                }
                return 1.0;
            }
        }

        /// <summary>Gets the logical parent  element of this element. </summary>
        /// <returns>This element's logical parent.</returns>
        public DependencyObject Parent
        {
            get
            {
                return this.ContextVerifiedGetParent();
            }
        }

        /// <summary> Gets an enumerator for logical child elements of this element. </summary>
        /// <returns>An enumerator for logical child elements of this element.</returns>
        protected internal virtual IEnumerator LogicalChildren
        {
            get
            {
                return null;
            }
        }

        internal bool ThisHasLoadedChangeEventHandler
        {
            get
            {
                return (base.EventHandlersStore != null && (base.EventHandlersStore.Contains(FrameworkElement.LoadedEvent) || base.EventHandlersStore.Contains(FrameworkElement.UnloadedEvent))) || (this.Style != null && this.Style.HasLoadedChangeHandler) || (this.ThemeStyle != null && this.ThemeStyle.HasLoadedChangeHandler) || (this.TemplateInternal != null && this.TemplateInternal.HasLoadedChangeHandler) || this.HasFefLoadedChangeHandler;
            }
        }

        internal bool HasFefLoadedChangeHandler
        {
            get
            {
                if (this.TemplatedParent == null)
                {
                    return false;
                }
                FrameworkElementFactory fEFTreeRoot = BroadcastEventHelper.GetFEFTreeRoot(this.TemplatedParent);
                if (fEFTreeRoot == null)
                {
                    return false;
                }
                FrameworkElementFactory frameworkElementFactory = StyleHelper.FindFEF(fEFTreeRoot, this.TemplateChildIndex);
                return frameworkElementFactory != null && frameworkElementFactory.HasLoadedChangeHandler;
            }
        }

        internal override DependencyObject InheritanceContext
        {
            get
            {
                return FrameworkElement.InheritanceContextField.GetValue(this);
            }
        }

        internal bool IsStyleUpdateInProgress
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.IsStyleUpdateInProgress);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.IsStyleUpdateInProgress, value);
            }
        }

        internal bool IsThemeStyleUpdateInProgress
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.IsThemeStyleUpdateInProgress);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.IsThemeStyleUpdateInProgress, value);
            }
        }

        internal bool StoresParentTemplateValues
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.StoresParentTemplateValues);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.StoresParentTemplateValues, value);
            }
        }

        internal bool HasNumberSubstitutionChanged
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasNumberSubstitutionChanged);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasNumberSubstitutionChanged, value);
            }
        }

        internal bool HasTemplateGeneratedSubTree
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasTemplateGeneratedSubTree);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasTemplateGeneratedSubTree, value);
            }
        }

        internal bool HasImplicitStyleFromResources
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasImplicitStyleFromResources);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasImplicitStyleFromResources, value);
            }
        }

        internal bool ShouldLookupImplicitStyles
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.ShouldLookupImplicitStyles);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.ShouldLookupImplicitStyles, value);
            }
        }

        internal bool IsStyleSetFromGenerator
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.IsStyleSetFromGenerator);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.IsStyleSetFromGenerator, value);
            }
        }

        internal bool HasStyleChanged
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.HasStyleChanged);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.HasStyleChanged, value);
            }
        }

        internal bool HasTemplateChanged
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.HasTemplateChanged);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.HasTemplateChanged, value);
            }
        }

        internal bool HasStyleInvalidated
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.HasStyleInvalidated);
            }
            set
            {
                this.WriteInternalFlag2(InternalFlags2.HasStyleInvalidated, value);
            }
        }

        internal bool HasStyleEverBeenFetched
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasStyleEverBeenFetched);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasStyleEverBeenFetched, value);
            }
        }

        internal bool HasLocalStyle
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasLocalStyle);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasLocalStyle, value);
            }
        }

        internal bool HasThemeStyleEverBeenFetched
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.HasThemeStyleEverBeenFetched);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.HasThemeStyleEverBeenFetched, value);
            }
        }

        internal bool AncestorChangeInProgress
        {
            get
            {
                return this.ReadInternalFlag(InternalFlags.AncestorChangeInProgress);
            }
            set
            {
                this.WriteInternalFlag(InternalFlags.AncestorChangeInProgress, value);
            }
        }

        internal FrugalObjectList<DependencyProperty> InheritableProperties
        {
            get
            {
                return this._inheritableProperties;
            }
            set
            {
                this._inheritableProperties = value;
            }
        }

        internal object[] LoadedPending
        {
            get
            {
                return (object[])base.GetValue(FrameworkElement.LoadedPendingProperty);
            }
        }

        internal object[] UnloadedPending
        {
            get
            {
                return (object[])base.GetValue(FrameworkElement.UnloadedPendingProperty);
            }
        }

        internal override bool HasMultipleInheritanceContexts
        {
            get
            {
                return this.ReadInternalFlag2(InternalFlags2.HasMultipleInheritanceContexts);
            }
        }

        internal bool PotentiallyHasMentees
        {
            get
            {
                return this.ReadInternalFlag((InternalFlags)2147483648u);
            }
            set
            {
                this.WriteInternalFlag((InternalFlags)2147483648u, value);
            }
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkElement" /> class. </summary>
        public FrameworkElement()
        {
            PropertyMetadata metadata = FrameworkElement.StyleProperty.GetMetadata(base.DependencyObjectType);
            Style style = (Style)metadata.DefaultValue;
            if (style != null)
            {
                FrameworkElement.OnStyleChanged(this, new DependencyPropertyChangedEventArgs(FrameworkElement.StyleProperty, metadata, null, style));
            }
            if ((FlowDirection)FrameworkElement.FlowDirectionProperty.GetDefaultValue(base.DependencyObjectType) == FlowDirection.RightToLeft)
            {
                this.IsRightToLeft = true;
            }
            Application current = Application.Current;
            if (current != null && current.HasImplicitStylesInResources)
            {
                this.ShouldLookupImplicitStyles = true;
            }
            FrameworkElement.EnsureFrameworkServices();
        }

        /// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Style" /> property.</summary>
        /// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Style" /> property value should be serialized; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeStyle()
        {
            return !this.IsStyleSetFromGenerator && base.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue;
        }

        private static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement)d;
            frameworkElement.HasLocalStyle = (e.NewEntry.BaseValueSourceInternal == BaseValueSourceInternal.Local);
            StyleHelper.UpdateStyleCache(frameworkElement, null, (Style)e.OldValue, (Style)e.NewValue, ref frameworkElement._styleCache);
        }

        private static void OnUseLayoutRoundingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visual arg_19_0 = (FrameworkElement)d;
            bool value = (bool)e.NewValue;
            arg_19_0.SetFlags(value, VisualFlags.UseLayoutRounding);
        }

        private static void OnThemeStyleKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement)d).UpdateThemeStyleProperty();
        }

        internal static void OnThemeStyleChanged(DependencyObject d, object oldValue, object newValue)
        {
            FrameworkElement frameworkElement = (FrameworkElement)d;
            StyleHelper.UpdateThemeStyleCache(frameworkElement, null, (Style)oldValue, (Style)newValue, ref frameworkElement._themeStyleCache);
        }

        internal virtual void OnTemplateChangedInternal(FrameworkTemplate oldTemplate, FrameworkTemplate newTemplate)
        {
            this.HasTemplateChanged = true;
        }

        /// <summary>Invoked when the style in use on this element changes, which will invalidate the layout. </summary>
        /// <param name="oldStyle">The old style.</param>
        /// <param name="newStyle">The new style.</param>
        protected internal virtual void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            this.HasStyleChanged = true;
        }

        /// <summary> Supports incremental layout implementations in specialized subclasses of <see cref="T:System.Windows.FrameworkElement" />. <see cref="M:System.Windows.FrameworkElement.ParentLayoutInvalidated(System.Windows.UIElement)" />  is invoked when a child element has invalidated a property that is marked in metadata as affecting the parent's measure or arrange passes during layout. </summary>
        /// <param name="child">The child element reporting the change.</param>
        protected internal virtual void ParentLayoutInvalidated(UIElement child)
        {
        }

        /// <summary>Builds the current template's visual tree if necessary, and returns a value that indicates whether the visual tree was rebuilt by this call. </summary>
        /// <returns>true if visuals were added to the tree; returns false otherwise.</returns>
        public bool ApplyTemplate()
        {
            this.OnPreApplyTemplate();
            bool flag = false;
            UncommonField<HybridDictionary[]> templateDataField = StyleHelper.TemplateDataField;
            FrameworkTemplate templateInternal = this.TemplateInternal;
            int num = 2;
            int num2 = 0;
            while (templateInternal != null && num2 < num && !this.HasTemplateGeneratedSubTree)
            {
                flag = templateInternal.ApplyTemplateContent(templateDataField, this);
                if (flag)
                {
                    this.HasTemplateGeneratedSubTree = true;
                    StyleHelper.InvokeDeferredActions(this, templateInternal);
                    this.OnApplyTemplate();
                }
                if (templateInternal == this.TemplateInternal)
                {
                    break;
                }
                templateInternal = this.TemplateInternal;
                num2++;
            }
            this.OnPostApplyTemplate();
            return flag;
        }

        internal virtual void OnPreApplyTemplate()
        {
        }

        /// <summary>When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.</summary>
        public virtual void OnApplyTemplate()
        {
        }

        internal virtual void OnPostApplyTemplate()
        {
        }

        /// <summary>Begins the sequence of actions that are contained in the provided storyboard. </summary>
        /// <param name="storyboard">The storyboard to begin.</param>
        public void BeginStoryboard(Storyboard storyboard)
        {
            this.BeginStoryboard(storyboard, HandoffBehavior.SnapshotAndReplace, false);
        }

        /// <summary>Begins the sequence of actions contained in the provided storyboard, with options specified for what should happen if the property is already animated. </summary>
        /// <param name="storyboard">The storyboard to begin.</param>
        /// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a property described in the storyboard is already animated.</param>
        public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior)
        {
            this.BeginStoryboard(storyboard, handoffBehavior, false);
        }

        /// <summary> Begins the sequence of actions contained in the provided storyboard, with specified state for control of the animation after it is started. </summary>
        /// <param name="storyboard">The storyboard to begin. </param>
        /// <param name="handoffBehavior">A value of the enumeration that describes behavior to use if a property described in the storyboard is already animated.</param>
        /// <param name="isControllable">Declares whether the animation is controllable (can be paused) after it is started.</param>
        public void BeginStoryboard(Storyboard storyboard, HandoffBehavior handoffBehavior, bool isControllable)
        {
            if (storyboard == null)
            {
                throw new ArgumentNullException("storyboard");
            }
            storyboard.Begin(this, handoffBehavior, isControllable);
        }

        internal static FrameworkElement FindNamedFrameworkElement(FrameworkElement startElement, string targetName)
        {
            FrameworkElement result;
            if (targetName == null || targetName.Length == 0)
            {
                result = startElement;
            }
            else
            {
                DependencyObject dependencyObject = LogicalTreeHelper.FindLogicalNode(startElement, targetName);
                if (dependencyObject == null)
                {
                    throw new ArgumentException(SR.Get("TargetNameNotFound", new object[]
					{
						targetName
					}));
                }
                FrameworkObject frameworkObject = new FrameworkObject(dependencyObject);
                if (!frameworkObject.IsFE)
                {
                    throw new InvalidOperationException(SR.Get("NamedObjectMustBeFrameworkElement", new object[]
					{
						targetName
					}));
                }
                result = frameworkObject.FE;
            }
            return result;
        }

        /// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Triggers" /> property.</summary>
        /// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Triggers" /> property value should be serialized; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTriggers()
        {
            TriggerCollection value = EventTrigger.TriggerCollectionField.GetValue(this);
            return value != null && value.Count != 0;
        }

        private void PrivateInitialized()
        {
            EventTrigger.ProcessTriggerCollection(this);
        }

        /// <summary>Overrides <see cref="M:System.Windows.Media.Visual.GetVisualChild(System.Int32)" />, and returns a child at the specified index from a collection of child elements. </summary>
        /// <param name="index">The zero-based index of the requested child element in the collection.</param>
        /// <returns>The requested child element. This should not return null; if the provided index is out of range, an exception is thrown.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (this._templateChild == null)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.Get("Visual_ArgumentOutOfRange"));
            }
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.Get("Visual_ArgumentOutOfRange"));
            }
            return this._templateChild;
        }

        /// <summary>For a description of this member, see the <see cref="M:System.Windows.Markup.IQueryAmbient.IsAmbientPropertyAvailable(System.String)" /> method.</summary>
        /// <param name="propertyName">The name of the requested ambient property.</param>
        /// <returns>true if <paramref name="propertyName" /> is available; otherwise, false. </returns>
        bool IQueryAmbient.IsAmbientPropertyAvailable(string propertyName)
        {
            return propertyName != "Resources" || this.HasResources;
        }

        /// <summary>Returns whether serialization processes should serialize the contents of the <see cref="P:System.Windows.FrameworkElement.Resources" /> property. </summary>
        /// <returns>true if the <see cref="P:System.Windows.FrameworkElement.Resources" /> property value should be serialized; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeResources()
        {
            return this.Resources != null && this.Resources.Count != 0;
        }

        /// <summary>Returns the named element in the visual tree of an instantiated <see cref="T:System.Windows.Controls.ControlTemplate" />.</summary>
        /// <param name="childName">Name of the child to find.</param>
        /// <returns>The requested element. May be null if no element of the requested name exists.</returns>
        protected internal DependencyObject GetTemplateChild(string childName)
        {
            FrameworkTemplate templateInternal = this.TemplateInternal;
            if (templateInternal == null)
            {
                return null;
            }
            return StyleHelper.FindNameInTemplateContent(this, childName, templateInternal) as DependencyObject;
        }

        /// <summary>Searches for a resource with the specified key, and throws an exception if the requested resource is not found. </summary>
        /// <param name="resourceKey">The key identifier for the requested resource.</param>
        /// <returns>The requested resource. If no resource with the provided key was found, an exception is thrown. An <see cref="F:System.Windows.DependencyProperty.UnsetValue" /> value might also be returned in the exception case.</returns>
        /// <exception cref="T:System.Windows.ResourceReferenceKeyNotFoundException">
        ///   <paramref name="resourceKey" /> was not found and an event handler does not exist for the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> event.-or-<paramref name="resourceKey" /> was not found and the <see cref="P:System.Windows.Threading.DispatcherUnhandledExceptionEventArgs.Handled" /> property is false in the <see cref="E:System.Windows.Threading.Dispatcher.UnhandledException" /> event.</exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="resourceKey" /> is null.</exception>
        public object FindResource(object resourceKey)
        {
            if (resourceKey == null)
            {
                throw new ArgumentNullException("resourceKey");
            }
            object expr_16 = FrameworkElement.FindResourceInternal(this, null, resourceKey);
            if (expr_16 == DependencyProperty.UnsetValue)
            {
                Helper.ResourceFailureThrow(resourceKey);
            }
            return expr_16;
        }

        /// <summary>Searches for a resource with the specified key, and returns that resource if found. </summary>
        /// <param name="resourceKey">The key identifier of the resource to be found.</param>
        /// <returns>The found resource, or null if no resource with the provided <paramref name="key" /> is found.</returns>
        public object TryFindResource(object resourceKey)
        {
            if (resourceKey == null)
            {
                throw new ArgumentNullException("resourceKey");
            }
            object obj = FrameworkElement.FindResourceInternal(this, null, resourceKey);
            if (obj == DependencyProperty.UnsetValue)
            {
                obj = null;
            }
            return obj;
        }

        internal static object FindImplicitStyleResource(FrameworkElement fe, object resourceKey, out object source)
        {
            if (fe.ShouldLookupImplicitStyles)
            {
                object unlinkedParent = null;
                bool allowDeferredResourceReference = false;
                bool mustReturnDeferredResourceReference = false;
                bool isImplicitStyleLookup = true;
                DependencyObject boundaryElement = null;
                if (!(fe is Control))
                {
                    boundaryElement = fe.TemplatedParent;
                }
                return FrameworkElement.FindResourceInternal(fe, null, FrameworkElement.StyleProperty, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, boundaryElement, isImplicitStyleLookup, out source);
            }
            source = null;
            return DependencyProperty.UnsetValue;
        }

        internal static object FindImplicitStyleResource(FrameworkContentElement fce, object resourceKey, out object source)
        {
            if (fce.ShouldLookupImplicitStyles)
            {
                object unlinkedParent = null;
                bool allowDeferredResourceReference = false;
                bool mustReturnDeferredResourceReference = false;
                bool isImplicitStyleLookup = true;
                DependencyObject templatedParent = fce.TemplatedParent;
                return FrameworkElement.FindResourceInternal(null, fce, FrameworkContentElement.StyleProperty, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, templatedParent, isImplicitStyleLookup, out source);
            }
            source = null;
            return DependencyProperty.UnsetValue;
        }

        internal static object FindResourceInternal(FrameworkElement fe, FrameworkContentElement fce, object resourceKey)
        {
            object obj;
            return FrameworkElement.FindResourceInternal(fe, fce, null, resourceKey, null, false, false, null, false, out obj);
        }

        internal static object FindResourceFromAppOrSystem(object resourceKey, out object source, bool disableThrowOnResourceNotFound, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
        {
            return FrameworkElement.FindResourceInternal(null, null, null, resourceKey, null, allowDeferredResourceReference, mustReturnDeferredResourceReference, null, disableThrowOnResourceNotFound, out source);
        }

        internal static object FindResourceInternal(FrameworkElement fe, FrameworkContentElement fce, DependencyProperty dp, object resourceKey, object unlinkedParent, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, DependencyObject boundaryElement, bool isImplicitStyleLookup, out object source)
        {
            InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
            if (TraceResourceDictionary.IsEnabled)
            {
                FrameworkObject frameworkObject = new FrameworkObject(fe, fce);
                TraceResourceDictionary.Trace(TraceEventType.Start, TraceResourceDictionary.FindResource, frameworkObject.DO, resourceKey);
            }
            try
            {
                if (fe != null || fce != null || unlinkedParent != null)
                {
                    object obj = FrameworkElement.FindResourceInTree(fe, fce, dp, resourceKey, unlinkedParent, allowDeferredResourceReference, mustReturnDeferredResourceReference, boundaryElement, out inheritanceBehavior, out source);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        object result = obj;
                        return result;
                    }
                }
                Application current = Application.Current;
                if (current != null && (inheritanceBehavior == InheritanceBehavior.Default || inheritanceBehavior == InheritanceBehavior.SkipToAppNow || inheritanceBehavior == InheritanceBehavior.SkipToAppNext))
                {
                    object obj = current.FindResourceInternal(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != null)
                    {
                        source = current;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInApplication, resourceKey, obj);
                        }
                        object result = obj;
                        return result;
                    }
                }
                if (!isImplicitStyleLookup && inheritanceBehavior != InheritanceBehavior.SkipAllNow && inheritanceBehavior != InheritanceBehavior.SkipAllNext)
                {
                    object obj = SystemResources.FindResourceInternal(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != null)
                    {
                        source = SystemResourceHost.Instance;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInTheme, source, resourceKey, obj);
                        }
                        object result = obj;
                        return result;
                    }
                }
            }
            finally
            {
                if (TraceResourceDictionary.IsEnabled)
                {
                    FrameworkObject frameworkObject2 = new FrameworkObject(fe, fce);
                    TraceResourceDictionary.Trace(TraceEventType.Stop, TraceResourceDictionary.FindResource, frameworkObject2.DO, resourceKey);
                }
            }
            if (TraceResourceDictionary.IsEnabledOverride && !isImplicitStyleLookup)
            {
                if ((fe != null && fe.IsLoaded) || (fce != null && fce.IsLoaded))
                {
                    TraceResourceDictionary.Trace(TraceEventType.Warning, TraceResourceDictionary.ResourceNotFound, resourceKey);
                }
                else if (TraceResourceDictionary.IsEnabled)
                {
                    TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.ResourceNotFound, resourceKey);
                }
            }
            source = null;
            return DependencyProperty.UnsetValue;
        }

        internal static object FindResourceInTree(FrameworkElement feStart, FrameworkContentElement fceStart, DependencyProperty dp, object resourceKey, object unlinkedParent, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference, DependencyObject boundaryElement, out InheritanceBehavior inheritanceBehavior, out object source)
        {
            FrameworkObject frameworkObject = new FrameworkObject(feStart, fceStart);
            FrameworkObject frameworkObject2 = frameworkObject;
            int num = 0;
            bool flag = true;
            inheritanceBehavior = InheritanceBehavior.Default;
            while (flag)
            {
                if (num > ContextLayoutManager.s_LayoutRecursionLimit)
                {
                    throw new InvalidOperationException(SR.Get("LogicalTreeLoop"));
                }
                num++;
                Style style = null;
                FrameworkTemplate frameworkTemplate = null;
                Style style2 = null;
                if (frameworkObject2.IsFE)
                {
                    FrameworkElement fE = frameworkObject2.FE;
                    object obj = fE.FindResourceOnSelf(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        source = fE;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceOnElement, source, resourceKey, obj);
                        }
                        return obj;
                    }
                    if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromStyle(dp))
                    {
                        style = fE.Style;
                    }
                    if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromTemplate(dp))
                    {
                        frameworkTemplate = fE.TemplateInternal;
                    }
                    if (fE != frameworkObject.FE || StyleHelper.ShouldGetValueFromThemeStyle(dp))
                    {
                        style2 = fE.ThemeStyle;
                    }
                }
                else if (frameworkObject2.IsFCE)
                {
                    FrameworkContentElement fCE = frameworkObject2.FCE;
                    object obj = fCE.FindResourceOnSelf(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        source = fCE;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceOnElement, source, resourceKey, obj);
                        }
                        return obj;
                    }
                    if (fCE != frameworkObject.FCE || StyleHelper.ShouldGetValueFromStyle(dp))
                    {
                        style = fCE.Style;
                    }
                    if (fCE != frameworkObject.FCE || StyleHelper.ShouldGetValueFromThemeStyle(dp))
                    {
                        style2 = fCE.ThemeStyle;
                    }
                }
                if (style != null)
                {
                    object obj = style.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        source = style;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInStyle, new object[]
							{
								style.Resources,
								resourceKey,
								style,
								frameworkObject2.DO,
								obj
							});
                        }
                        return obj;
                    }
                }
                if (frameworkTemplate != null)
                {
                    object obj = frameworkTemplate.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        source = frameworkTemplate;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInTemplate, new object[]
							{
								frameworkTemplate.Resources,
								resourceKey,
								frameworkTemplate,
								frameworkObject2.DO,
								obj
							});
                        }
                        return obj;
                    }
                }
                if (style2 != null)
                {
                    object obj = style2.FindResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference);
                    if (obj != DependencyProperty.UnsetValue)
                    {
                        source = style2;
                        if (TraceResourceDictionary.IsEnabled)
                        {
                            TraceResourceDictionary.TraceActivityItem(TraceResourceDictionary.FoundResourceInThemeStyle, new object[]
							{
								style2.Resources,
								resourceKey,
								style2,
								frameworkObject2.DO,
								obj
							});
                        }
                        return obj;
                    }
                }
                if (boundaryElement != null && frameworkObject2.DO == boundaryElement)
                {
                    break;
                }
                if (frameworkObject2.IsValid && TreeWalkHelper.SkipNext(frameworkObject2.InheritanceBehavior))
                {
                    inheritanceBehavior = frameworkObject2.InheritanceBehavior;
                    break;
                }
                if (unlinkedParent != null)
                {
                    DependencyObject dependencyObject = unlinkedParent as DependencyObject;
                    if (dependencyObject != null)
                    {
                        frameworkObject2.Reset(dependencyObject);
                        if (frameworkObject2.IsValid)
                        {
                            flag = true;
                        }
                        else
                        {
                            DependencyObject frameworkParent = FrameworkElement.GetFrameworkParent(unlinkedParent);
                            if (frameworkParent != null)
                            {
                                frameworkObject2.Reset(frameworkParent);
                                flag = true;
                            }
                            else
                            {
                                flag = false;
                            }
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                    unlinkedParent = null;
                }
                else
                {
                    frameworkObject2 = frameworkObject2.FrameworkParent;
                    flag = frameworkObject2.IsValid;
                }
                if (frameworkObject2.IsValid && TreeWalkHelper.SkipNow(frameworkObject2.InheritanceBehavior))
                {
                    inheritanceBehavior = frameworkObject2.InheritanceBehavior;
                    break;
                }
            }
            source = null;
            return DependencyProperty.UnsetValue;
        }

        internal static object FindTemplateResourceInternal(DependencyObject target, object item, Type templateType)
        {
            if (item == null || item is UIElement)
            {
                return null;
            }
            Type type;
            object dataType = ContentPresenter.DataTypeForItem(item, target, out type);
            ArrayList arrayList = new ArrayList();
            int num = -1;
            while (dataType != null)
            {
                object obj = null;
                if (templateType == typeof(ItemContainerTemplate))
                {
                    obj = new ItemContainerTemplateKey(dataType);
                }
                else if (templateType == typeof(DataTemplate))
                {
                    obj = new DataTemplateKey(dataType);
                }
                if (obj != null)
                {
                    arrayList.Add(obj);
                }
                if (num == -1)
                {
                    num = arrayList.Count;
                }
                if (type != null)
                {
                    type = type.BaseType;
                    if (type == typeof(object))
                    {
                        type = null;
                    }
                }
                dataType = type;
            }
            int count = arrayList.Count;
            object result = FrameworkElement.FindTemplateResourceInTree(target, arrayList, num, ref count);
            if (count >= num)
            {
                object obj2 = Helper.FindTemplateResourceFromAppOrSystem(target, arrayList, num, ref count);
                if (obj2 != null)
                {
                    result = obj2;
                }
            }
            return result;
        }

        private static object FindTemplateResourceInTree(DependencyObject target, ArrayList keys, int exactMatch, ref int bestMatch)
        {
            object result = null;
            FrameworkObject frameworkParent = new FrameworkObject(target);
            while (frameworkParent.IsValid)
            {
                ResourceDictionary resourceDictionary = FrameworkElement.GetInstanceResourceDictionary(frameworkParent.FE, frameworkParent.FCE);
                if (resourceDictionary != null)
                {
                    object obj = FrameworkElement.FindBestMatchInResourceDictionary(resourceDictionary, keys, exactMatch, ref bestMatch);
                    if (obj != null)
                    {
                        result = obj;
                        if (bestMatch < exactMatch)
                        {
                            return result;
                        }
                    }
                }
                resourceDictionary = FrameworkElement.GetStyleResourceDictionary(frameworkParent.FE, frameworkParent.FCE);
                if (resourceDictionary != null)
                {
                    object obj = FrameworkElement.FindBestMatchInResourceDictionary(resourceDictionary, keys, exactMatch, ref bestMatch);
                    if (obj != null)
                    {
                        result = obj;
                        if (bestMatch < exactMatch)
                        {
                            return result;
                        }
                    }
                }
                resourceDictionary = FrameworkElement.GetThemeStyleResourceDictionary(frameworkParent.FE, frameworkParent.FCE);
                if (resourceDictionary != null)
                {
                    object obj = FrameworkElement.FindBestMatchInResourceDictionary(resourceDictionary, keys, exactMatch, ref bestMatch);
                    if (obj != null)
                    {
                        result = obj;
                        if (bestMatch < exactMatch)
                        {
                            return result;
                        }
                    }
                }
                resourceDictionary = FrameworkElement.GetTemplateResourceDictionary(frameworkParent.FE, frameworkParent.FCE);
                if (resourceDictionary != null)
                {
                    object obj = FrameworkElement.FindBestMatchInResourceDictionary(resourceDictionary, keys, exactMatch, ref bestMatch);
                    if (obj != null)
                    {
                        result = obj;
                        if (bestMatch < exactMatch)
                        {
                            return result;
                        }
                    }
                }
                if (frameworkParent.IsValid && TreeWalkHelper.SkipNext(frameworkParent.InheritanceBehavior))
                {
                    break;
                }
                frameworkParent = frameworkParent.FrameworkParent;
                if (frameworkParent.IsValid && TreeWalkHelper.SkipNext(frameworkParent.InheritanceBehavior))
                {
                    break;
                }
            }
            return result;
        }

        private static object FindBestMatchInResourceDictionary(ResourceDictionary table, ArrayList keys, int exactMatch, ref int bestMatch)
        {
            object result = null;
            if (table != null)
            {
                for (int i = 0; i < bestMatch; i++)
                {
                    object obj = table[keys[i]];
                    if (obj != null)
                    {
                        result = obj;
                        bestMatch = i;
                        if (bestMatch < exactMatch)
                        {
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        private static ResourceDictionary GetInstanceResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
        {
            ResourceDictionary result = null;
            if (fe != null)
            {
                if (fe.HasResources)
                {
                    result = fe.Resources;
                }
            }
            else if (fce.HasResources)
            {
                result = fce.Resources;
            }
            return result;
        }

        private static ResourceDictionary GetStyleResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
        {
            ResourceDictionary result = null;
            if (fe != null)
            {
                if (fe.Style != null && fe.Style._resources != null)
                {
                    result = fe.Style._resources;
                }
            }
            else if (fce.Style != null && fce.Style._resources != null)
            {
                result = fce.Style._resources;
            }
            return result;
        }

        private static ResourceDictionary GetThemeStyleResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
        {
            ResourceDictionary result = null;
            if (fe != null)
            {
                if (fe.ThemeStyle != null && fe.ThemeStyle._resources != null)
                {
                    result = fe.ThemeStyle._resources;
                }
            }
            else if (fce.ThemeStyle != null && fce.ThemeStyle._resources != null)
            {
                result = fce.ThemeStyle._resources;
            }
            return result;
        }

        private static ResourceDictionary GetTemplateResourceDictionary(FrameworkElement fe, FrameworkContentElement fce)
        {
            ResourceDictionary result = null;
            if (fe != null && fe.TemplateInternal != null && fe.TemplateInternal._resources != null)
            {
                result = fe.TemplateInternal._resources;
            }
            return result;
        }

        internal bool HasNonDefaultValue(DependencyProperty dp)
        {
            return !Helper.HasDefaultValue(this, dp);
        }

        internal static INameScope FindScope(DependencyObject d)
        {
            DependencyObject dependencyObject;
            return FrameworkElement.FindScope(d, out dependencyObject);
        }

        internal static INameScope FindScope(DependencyObject d, out DependencyObject scopeOwner)
        {
            while (d != null)
            {
                INameScope nameScope = NameScope.NameScopeFromObject(d);
                if (nameScope != null)
                {
                    scopeOwner = d;
                    return nameScope;
                }
                DependencyObject parent = LogicalTreeHelper.GetParent(d);
                d = ((parent != null) ? parent : Helper.FindMentor(d.InheritanceContext));
            }
            scopeOwner = null;
            return null;
        }

        /// <summary>Searches for a resource with the specified name and sets up a resource reference to it for the specified property. </summary>
        /// <param name="dp">The property to which the resource is bound.</param>
        /// <param name="name">The name of the resource.</param>
        public void SetResourceReference(DependencyProperty dp, object name)
        {
            base.SetValue(dp, new ResourceReferenceExpression(name));
            this.HasResourceReference = true;
        }

        internal sealed override void EvaluateBaseValueCore(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry newEntry)
        {
            if (dp == FrameworkElement.StyleProperty)
            {
                this.HasStyleEverBeenFetched = true;
                this.HasImplicitStyleFromResources = false;
                this.IsStyleSetFromGenerator = false;
            }
            this.GetRawValue(dp, metadata, ref newEntry);
            Storyboard.GetComplexPathValue(this, dp, ref newEntry, metadata);
        }

        internal void GetRawValue(DependencyProperty dp, PropertyMetadata metadata, ref EffectiveValueEntry entry)
        {
            if (entry.BaseValueSourceInternal == BaseValueSourceInternal.Local && entry.GetFlattenedEntry(RequestFlags.FullyResolved).Value != DependencyProperty.UnsetValue)
            {
                return;
            }
            if (this.TemplateChildIndex != -1 && this.GetValueFromTemplatedParent(dp, ref entry))
            {
                return;
            }
            if (dp != FrameworkElement.StyleProperty)
            {
                if (StyleHelper.GetValueFromStyleOrTemplate(new FrameworkObject(this, null), dp, ref entry))
                {
                    return;
                }
            }
            else
            {
                object obj2;
                object obj = FrameworkElement.FindImplicitStyleResource(this, base.GetType(), out obj2);
                if (obj != DependencyProperty.UnsetValue)
                {
                    this.HasImplicitStyleFromResources = true;
                    entry.BaseValueSourceInternal = BaseValueSourceInternal.ImplicitReference;
                    entry.Value = obj;
                    return;
                }
            }
            FrameworkPropertyMetadata frameworkPropertyMetadata = metadata as FrameworkPropertyMetadata;
            if (frameworkPropertyMetadata != null && frameworkPropertyMetadata.Inherits)
            {
                object inheritableValue = this.GetInheritableValue(dp, frameworkPropertyMetadata);
                if (inheritableValue != DependencyProperty.UnsetValue)
                {
                    entry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
                    entry.Value = inheritableValue;
                    return;
                }
            }
        }

        private bool GetValueFromTemplatedParent(DependencyProperty dp, ref EffectiveValueEntry entry)
        {
            FrameworkTemplate templateInternal = ((FrameworkElement)this._templatedParent).TemplateInternal;
            return templateInternal != null && StyleHelper.GetValueFromTemplatedParent(this._templatedParent, this.TemplateChildIndex, new FrameworkObject(this, null), dp, ref templateInternal.ChildRecordFromChildIndex, templateInternal.VisualTree, ref entry);
        }

        private object GetInheritableValue(DependencyProperty dp, FrameworkPropertyMetadata fmetadata)
        {
            if (!TreeWalkHelper.SkipNext(this.InheritanceBehavior) || fmetadata.OverridesInheritanceBehavior)
            {
                InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
                FrameworkElement frameworkElement;
                FrameworkContentElement frameworkContentElement;
                bool frameworkParent = FrameworkElement.GetFrameworkParent(this, out frameworkElement, out frameworkContentElement);
                while (frameworkParent)
                {
                    bool flag;
                    if (frameworkElement != null)
                    {
                        flag = TreeWalkHelper.IsInheritanceNode(frameworkElement, dp, out inheritanceBehavior);
                    }
                    else
                    {
                        flag = TreeWalkHelper.IsInheritanceNode(frameworkContentElement, dp, out inheritanceBehavior);
                    }
                    if (TreeWalkHelper.SkipNow(inheritanceBehavior))
                    {
                        break;
                    }
                    if (flag)
                    {
                        if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
                        {
                            string text = string.Format(CultureInfo.InvariantCulture, "[{0}]{1}({2})", new object[]
							{
								base.GetType().Name,
								dp.Name,
								base.GetHashCode()
							});
                            EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientPropParentCheck, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, new object[]
							{
								base.GetHashCode(),
								text
							});
                        }
                        DependencyObject dependencyObject = frameworkElement;
                        if (dependencyObject == null)
                        {
                            dependencyObject = frameworkContentElement;
                        }
                        EntryIndex entryIndex = dependencyObject.LookupEntry(dp.GlobalIndex);
                        return dependencyObject.GetValueEntry(entryIndex, dp, fmetadata, (RequestFlags)12).Value;
                    }
                    if (TreeWalkHelper.SkipNext(inheritanceBehavior))
                    {
                        break;
                    }
                    if (frameworkElement != null)
                    {
                        frameworkParent = FrameworkElement.GetFrameworkParent(frameworkElement, out frameworkElement, out frameworkContentElement);
                    }
                    else
                    {
                        frameworkParent = FrameworkElement.GetFrameworkParent(frameworkContentElement, out frameworkElement, out frameworkContentElement);
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        internal Expression GetExpressionCore(DependencyProperty dp, PropertyMetadata metadata)
        {
            this.IsRequestingExpression = true;
            EffectiveValueEntry effectiveValueEntry = new EffectiveValueEntry(dp);
            effectiveValueEntry.Value = DependencyProperty.UnsetValue;
            this.EvaluateBaseValueCore(dp, metadata, ref effectiveValueEntry);
            this.IsRequestingExpression = false;
            return effectiveValueEntry.Value as Expression;
        }

        /// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.FrameworkElement" /> has been updated. The specific dependency property that changed is reported in the arguments parameter. Overrides <see cref="M:System.Windows.DependencyObject.OnPropertyChanged(System.Windows.DependencyPropertyChangedEventArgs)" />.</summary>
        /// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            DependencyProperty property = e.Property;
            base.OnPropertyChanged(e);
            if (e.IsAValueChange || e.IsASubPropertyChange)
            {
                if (property != null && property.OwnerType == typeof(PresentationSource) && property.Name == "RootSource")
                {
                    this.TryFireInitialized();
                }
                if (property == FrameworkElement.NameProperty && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose))
                {
                    EventTrace.EventProvider.TraceEvent(EventTrace.Event.PerfElementIDName, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Verbose, new object[]
					{
						PerfService.GetPerfElementID(this),
						base.GetType().Name,
						base.GetValue(property)
					});
                }
                if (property != FrameworkElement.StyleProperty && property != Control.TemplateProperty && property != FrameworkElement.DefaultStyleKeyProperty)
                {
                    if (this.TemplatedParent != null)
                    {
                        FrameworkTemplate templateInternal = (this.TemplatedParent as FrameworkElement).TemplateInternal;
                        if (templateInternal != null)
                        {
                            StyleHelper.OnTriggerSourcePropertyInvalidated(null, templateInternal, this.TemplatedParent, property, e, false, ref templateInternal.TriggerSourceRecordFromChildIndex, ref templateInternal.PropertyTriggersWithActions, this.TemplateChildIndex);
                        }
                    }
                    if (this.Style != null)
                    {
                        StyleHelper.OnTriggerSourcePropertyInvalidated(this.Style, null, this, property, e, true, ref this.Style.TriggerSourceRecordFromChildIndex, ref this.Style.PropertyTriggersWithActions, 0);
                    }
                    if (this.TemplateInternal != null)
                    {
                        StyleHelper.OnTriggerSourcePropertyInvalidated(null, this.TemplateInternal, this, property, e, !this.HasTemplateGeneratedSubTree, ref this.TemplateInternal.TriggerSourceRecordFromChildIndex, ref this.TemplateInternal.PropertyTriggersWithActions, 0);
                    }
                    if (this.ThemeStyle != null && this.Style != this.ThemeStyle)
                    {
                        StyleHelper.OnTriggerSourcePropertyInvalidated(this.ThemeStyle, null, this, property, e, true, ref this.ThemeStyle.TriggerSourceRecordFromChildIndex, ref this.ThemeStyle.PropertyTriggersWithActions, 0);
                    }
                }
            }
            FrameworkPropertyMetadata frameworkPropertyMetadata = e.Metadata as FrameworkPropertyMetadata;
            if (frameworkPropertyMetadata != null)
            {
                if (frameworkPropertyMetadata.Inherits && (this.InheritanceBehavior == InheritanceBehavior.Default || frameworkPropertyMetadata.OverridesInheritanceBehavior) && (!DependencyObject.IsTreeWalkOperation(e.OperationType) || this.PotentiallyHasMentees))
                {
                    EffectiveValueEntry newEntry = e.NewEntry;
                    EffectiveValueEntry oldEntry = e.OldEntry;
                    if (oldEntry.BaseValueSourceInternal > newEntry.BaseValueSourceInternal)
                    {
                        newEntry = new EffectiveValueEntry(property, BaseValueSourceInternal.Inherited);
                    }
                    else
                    {
                        newEntry = newEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
                        newEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
                    }
                    if (oldEntry.BaseValueSourceInternal != BaseValueSourceInternal.Default || oldEntry.HasModifiers)
                    {
                        oldEntry = oldEntry.GetFlattenedEntry(RequestFlags.FullyResolved);
                        oldEntry.BaseValueSourceInternal = BaseValueSourceInternal.Inherited;
                    }
                    else
                    {
                        oldEntry = default(EffectiveValueEntry);
                    }
                    InheritablePropertyChangeInfo info = new InheritablePropertyChangeInfo(this, property, oldEntry, newEntry);
                    if (!DependencyObject.IsTreeWalkOperation(e.OperationType))
                    {
                        TreeWalkHelper.InvalidateOnInheritablePropertyChange(this, null, info, true);
                    }
                    if (this.PotentiallyHasMentees)
                    {
                        TreeWalkHelper.OnInheritedPropertyChanged(this, ref info, this.InheritanceBehavior);
                    }
                }
                if ((e.IsAValueChange || e.IsASubPropertyChange) && (!this.AncestorChangeInProgress || !this.InVisibilityCollapsedTree))
                {
                    bool affectsParentMeasure = frameworkPropertyMetadata.AffectsParentMeasure;
                    bool affectsParentArrange = frameworkPropertyMetadata.AffectsParentArrange;
                    bool arg_2DF_0 = frameworkPropertyMetadata.AffectsMeasure;
                    bool affectsArrange = frameworkPropertyMetadata.AffectsArrange;
                    if (arg_2DF_0 | affectsArrange | affectsParentArrange | affectsParentMeasure)
                    {
                        Visual visual = VisualTreeHelper.GetParent(this) as Visual;
                        while (visual != null)
                        {
                            UIElement uIElement = visual as UIElement;
                            if (uIElement != null)
                            {
                                if (FrameworkElement.DType.IsInstanceOfType(uIElement))
                                {
                                    ((FrameworkElement)uIElement).ParentLayoutInvalidated(this);
                                }
                                if (affectsParentMeasure)
                                {
                                    uIElement.InvalidateMeasure();
                                }
                                if (affectsParentArrange)
                                {
                                    uIElement.InvalidateArrange();
                                    break;
                                }
                                break;
                            }
                            else
                            {
                                visual = (VisualTreeHelper.GetParent(visual) as Visual);
                            }
                        }
                    }
                    if (frameworkPropertyMetadata.AffectsMeasure && (!this.BypassLayoutPolicies || (property != FrameworkElement.WidthProperty && property != FrameworkElement.HeightProperty)))
                    {
                        base.InvalidateMeasure();
                    }
                    if (frameworkPropertyMetadata.AffectsArrange)
                    {
                        base.InvalidateArrange();
                    }
                    if (frameworkPropertyMetadata.AffectsRender && (e.IsAValueChange || !frameworkPropertyMetadata.SubPropertiesDoNotAffectRender))
                    {
                        base.InvalidateVisual();
                    }
                }
            }
        }

        internal static DependencyObject GetFrameworkParent(object current)
        {
            FrameworkObject frameworkParent = new FrameworkObject(current as DependencyObject);
            frameworkParent = frameworkParent.FrameworkParent;
            return frameworkParent.DO;
        }

        internal static bool GetFrameworkParent(FrameworkElement current, out FrameworkElement feParent, out FrameworkContentElement fceParent)
        {
            FrameworkObject frameworkParent = new FrameworkObject(current, null);
            frameworkParent = frameworkParent.FrameworkParent;
            feParent = frameworkParent.FE;
            fceParent = frameworkParent.FCE;
            return frameworkParent.IsValid;
        }

        internal static bool GetFrameworkParent(FrameworkContentElement current, out FrameworkElement feParent, out FrameworkContentElement fceParent)
        {
            FrameworkObject frameworkParent = new FrameworkObject(null, current);
            frameworkParent = frameworkParent.FrameworkParent;
            feParent = frameworkParent.FE;
            fceParent = frameworkParent.FCE;
            return frameworkParent.IsValid;
        }

        internal static bool GetContainingFrameworkElement(DependencyObject current, out FrameworkElement fe, out FrameworkContentElement fce)
        {
            FrameworkObject containingFrameworkElement = FrameworkObject.GetContainingFrameworkElement(current);
            if (containingFrameworkElement.IsValid)
            {
                fe = containingFrameworkElement.FE;
                fce = containingFrameworkElement.FCE;
                return true;
            }
            fe = null;
            fce = null;
            return false;
        }

        internal static void GetTemplatedParentChildRecord(DependencyObject templatedParent, int childIndex, out ChildRecord childRecord, out bool isChildRecordValid)
        {
            isChildRecordValid = false;
            childRecord = default(ChildRecord);
            if (templatedParent != null)
            {
                FrameworkObject frameworkObject = new FrameworkObject(templatedParent, true);
                FrameworkTemplate templateInternal = frameworkObject.FE.TemplateInternal;
                if (templateInternal != null && 0 <= childIndex && childIndex < templateInternal.ChildRecordFromChildIndex.Count)
                {
                    childRecord = templateInternal.ChildRecordFromChildIndex[childIndex];
                    isChildRecordValid = true;
                }
            }
        }

        internal virtual string GetPlainText()
        {
            return null;
        }

        static FrameworkElement()
        {
            FrameworkElement._typeofThis = typeof(FrameworkElement);
            FrameworkElement.StyleProperty = DependencyProperty.Register("Style", typeof(Style), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnStyleChanged)));
            FrameworkElement.OverridesDefaultStyleProperty = DependencyProperty.Register("OverridesDefaultStyle", typeof(bool), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnThemeStyleKeyChanged)));
            FrameworkElement.UseLayoutRoundingProperty = DependencyProperty.Register("UseLayoutRounding", typeof(bool), typeof(FrameworkElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(FrameworkElement.OnUseLayoutRoundingChanged)));
            FrameworkElement.DefaultStyleKeyProperty = DependencyProperty.Register("DefaultStyleKey", typeof(object), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnThemeStyleKeyChanged)));
            FrameworkElement.DefaultNumberSubstitution = new NumberSubstitution(NumberCultureSource.User, null, NumberSubstitutionMethod.AsCulture);
            FrameworkElement.DataContextProperty = DependencyProperty.Register("DataContext", typeof(object), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(FrameworkElement.OnDataContextChanged)));
            FrameworkElement.DataContextChangedKey = new EventPrivateKey();
            FrameworkElement.BindingGroupProperty = DependencyProperty.Register("BindingGroup", typeof(BindingGroup), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
            FrameworkElement.LanguageProperty = DependencyProperty.RegisterAttached("Language", typeof(XmlLanguage), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("en-US"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));
            FrameworkElement.NameProperty = DependencyProperty.Register("Name", typeof(string), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None, null, null, true), new ValidateValueCallback(NameValidationHelper.NameValidationCallback));
            FrameworkElement.TagProperty = DependencyProperty.Register("Tag", typeof(object), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null));
            FrameworkElement.InputScopeProperty = InputMethod.InputScopeProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
            FrameworkElement.RequestBringIntoViewEvent = EventManager.RegisterRoutedEvent("RequestBringIntoView", RoutingStrategy.Bubble, typeof(RequestBringIntoViewEventHandler), FrameworkElement._typeofThis);
            FrameworkElement.SizeChangedEvent = EventManager.RegisterRoutedEvent("SizeChanged", RoutingStrategy.Direct, typeof(SizeChangedEventHandler), FrameworkElement._typeofThis);
            FrameworkElement._actualWidthMetadata = new ReadOnlyFrameworkPropertyMetadata(0.0, new GetReadOnlyValueCallback(FrameworkElement.GetActualWidth));
            FrameworkElement.ActualWidthPropertyKey = DependencyProperty.RegisterReadOnly("ActualWidth", typeof(double), FrameworkElement._typeofThis, FrameworkElement._actualWidthMetadata);
            FrameworkElement.ActualWidthProperty = FrameworkElement.ActualWidthPropertyKey.DependencyProperty;
            FrameworkElement._actualHeightMetadata = new ReadOnlyFrameworkPropertyMetadata(0.0, new GetReadOnlyValueCallback(FrameworkElement.GetActualHeight));
            FrameworkElement.ActualHeightPropertyKey = DependencyProperty.RegisterReadOnly("ActualHeight", typeof(double), FrameworkElement._typeofThis, FrameworkElement._actualHeightMetadata);
            FrameworkElement.ActualHeightProperty = FrameworkElement.ActualHeightPropertyKey.DependencyProperty;
            FrameworkElement.LayoutTransformProperty = DependencyProperty.Register("LayoutTransform", typeof(Transform), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(Transform.Identity, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnLayoutTransformChanged)));
            FrameworkElement.WidthProperty = DependencyProperty.Register("Width", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsWidthHeightValid));
            FrameworkElement.MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsMinWidthHeightValid));
            FrameworkElement.MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsMaxWidthHeightValid));
            FrameworkElement.HeightProperty = DependencyProperty.Register("Height", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsWidthHeightValid));
            FrameworkElement.MinHeightProperty = DependencyProperty.Register("MinHeight", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsMinWidthHeightValid));
            FrameworkElement.MaxHeightProperty = DependencyProperty.Register("MaxHeight", typeof(double), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(FrameworkElement.OnTransformDirty)), new ValidateValueCallback(FrameworkElement.IsMaxWidthHeightValid));
            FrameworkElement.FlowDirectionProperty = DependencyProperty.RegisterAttached("FlowDirection", typeof(FlowDirection), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(FlowDirection.LeftToRight, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(FrameworkElement.OnFlowDirectionChanged), new CoerceValueCallback(FrameworkElement.CoerceFlowDirectionProperty)), new ValidateValueCallback(FrameworkElement.IsValidFlowDirection));
            FrameworkElement.MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(FrameworkElement.IsMarginValid));
            FrameworkElement.HorizontalAlignmentProperty = DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange), new ValidateValueCallback(FrameworkElement.ValidateHorizontalAlignmentValue));
            FrameworkElement.VerticalAlignmentProperty = DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsArrange), new ValidateValueCallback(FrameworkElement.ValidateVerticalAlignmentValue));
            FrameworkElement._defaultFocusVisualStyle = null;
            FrameworkElement.FocusVisualStyleProperty = DependencyProperty.Register("FocusVisualStyle", typeof(Style), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(FrameworkElement.DefaultFocusVisualStyle));
            FrameworkElement.CursorProperty = DependencyProperty.Register("Cursor", typeof(Cursor), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(FrameworkElement.OnCursorChanged)));
            FrameworkElement.ForceCursorProperty = DependencyProperty.Register("ForceCursor", typeof(bool), FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(FrameworkElement.OnForceCursorChanged)));
            FrameworkElement.InitializedKey = new EventPrivateKey();
            FrameworkElement.LoadedPendingPropertyKey = DependencyProperty.RegisterReadOnly("LoadedPending", typeof(object[]), FrameworkElement._typeofThis, new PropertyMetadata(null));
            FrameworkElement.LoadedPendingProperty = FrameworkElement.LoadedPendingPropertyKey.DependencyProperty;
            FrameworkElement.UnloadedPendingPropertyKey = DependencyProperty.RegisterReadOnly("UnloadedPending", typeof(object[]), FrameworkElement._typeofThis, new PropertyMetadata(null));
            FrameworkElement.UnloadedPendingProperty = FrameworkElement.UnloadedPendingPropertyKey.DependencyProperty;
            FrameworkElement.LoadedEvent = EventManager.RegisterRoutedEvent("Loaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), FrameworkElement._typeofThis);
            FrameworkElement.UnloadedEvent = EventManager.RegisterRoutedEvent("Unloaded", RoutingStrategy.Direct, typeof(RoutedEventHandler), FrameworkElement._typeofThis);
            FrameworkElement.ToolTipProperty = ToolTipService.ToolTipProperty.AddOwner(FrameworkElement._typeofThis);
            FrameworkElement.ContextMenuProperty = ContextMenuService.ContextMenuProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null));
            FrameworkElement.ToolTipOpeningEvent = ToolTipService.ToolTipOpeningEvent.AddOwner(FrameworkElement._typeofThis);
            FrameworkElement.ToolTipClosingEvent = ToolTipService.ToolTipClosingEvent.AddOwner(FrameworkElement._typeofThis);
            FrameworkElement.ContextMenuOpeningEvent = ContextMenuService.ContextMenuOpeningEvent.AddOwner(FrameworkElement._typeofThis);
            FrameworkElement.ContextMenuClosingEvent = ContextMenuService.ContextMenuClosingEvent.AddOwner(FrameworkElement._typeofThis);
            FrameworkElement.UnclippedDesiredSizeField = new UncommonField<SizeBox>();
            FrameworkElement.LayoutTransformDataField = new UncommonField<FrameworkElement.LayoutTransformData>();
            FrameworkElement.ResourcesField = new UncommonField<ResourceDictionary>();
            FrameworkElement.UIElementDType = DependencyObjectType.FromSystemTypeInternal(typeof(UIElement));
            FrameworkElement._controlDType = null;
            FrameworkElement._contentPresenterDType = null;
            FrameworkElement._pageFunctionBaseDType = null;
            FrameworkElement._pageDType = null;
            FrameworkElement.ResourcesChangedKey = new EventPrivateKey();
            FrameworkElement.InheritedPropertyChangedKey = new EventPrivateKey();
            FrameworkElement.DType = DependencyObjectType.FromSystemTypeInternal(typeof(FrameworkElement));
            FrameworkElement.InheritanceContextField = new UncommonField<DependencyObject>();
            FrameworkElement.MentorField = new UncommonField<DependencyObject>();
            UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, Mouse.QueryCursorEvent, new QueryCursorEventHandler(FrameworkElement.OnQueryCursorOverride), true);
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, Keyboard.PreviewGotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FrameworkElement.OnPreviewGotKeyboardFocus));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FrameworkElement.OnGotKeyboardFocus));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(FrameworkElement.OnLostKeyboardFocus));
            UIElement.AllowDropProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));
            Stylus.IsPressAndHoldEnabledProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
            Stylus.IsFlicksEnabledProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
            Stylus.IsTapFeedbackEnabledProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
            Stylus.IsTouchFeedbackEnabledProperty.AddOwner(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(BooleanBoxes.TrueBox, FrameworkPropertyMetadataOptions.Inherits));
            PropertyChangedCallback propertyChangedCallback = new PropertyChangedCallback(FrameworkElement.NumberSubstitutionChanged);
            NumberSubstitution.CultureSourceProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(NumberCultureSource.User, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
            NumberSubstitution.CultureOverrideProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
            NumberSubstitution.SubstitutionProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(NumberSubstitutionMethod.AsCulture, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, propertyChangedCallback));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(FrameworkElement.OnToolTipOpeningThunk));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, FrameworkElement.ToolTipClosingEvent, new ToolTipEventHandler(FrameworkElement.OnToolTipClosingThunk));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, FrameworkElement.ContextMenuOpeningEvent, new ContextMenuEventHandler(FrameworkElement.OnContextMenuOpeningThunk));
            EventManager.RegisterClassHandler(FrameworkElement._typeofThis, FrameworkElement.ContextMenuClosingEvent, new ContextMenuEventHandler(FrameworkElement.OnContextMenuClosingThunk));
            TextElement.FontFamilyProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits, null, new CoerceValueCallback(FrameworkElement.CoerceFontFamily)));
            TextElement.FontSizeProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits, null, new CoerceValueCallback(FrameworkElement.CoerceFontSize)));
            TextElement.FontStyleProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits, null, new CoerceValueCallback(FrameworkElement.CoerceFontStyle)));
            TextElement.FontWeightProperty.OverrideMetadata(FrameworkElement._typeofThis, new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits, null, new CoerceValueCallback(FrameworkElement.CoerceFontWeight)));
            TextOptions.TextRenderingModeProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(new PropertyChangedCallback(FrameworkElement.TextRenderingMode_Changed)));
        }

        private static void TextRenderingMode_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement)d).pushTextRenderingMode();
        }

        internal virtual void pushTextRenderingMode()
        {
            if (DependencyPropertyHelper.GetValueSource(this, TextOptions.TextRenderingModeProperty).BaseValueSource > BaseValueSource.Inherited)
            {
                base.VisualTextRenderingMode = TextOptions.GetTextRenderingMode(this);
            }
        }

        internal virtual void OnAncestorChanged()
        {
        }

        /// <summary>Invoked when the parent of this element in the visual tree is changed. Overrides <see cref="M:System.Windows.UIElement.OnVisualParentChanged(System.Windows.DependencyObject)" />.</summary>
        /// <param name="oldParent">The old parent element. May be null to indicate that the element did not have a visual parent previously.</param>
        protected internal override void OnVisualParentChanged(DependencyObject oldParent)
        {
            DependencyObject parentInternal = VisualTreeHelper.GetParentInternal(this);
            if (parentInternal != null)
            {
                this.ClearInheritanceContext();
            }
            BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, oldParent, parentInternal);
            BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, oldParent, parentInternal);
            if (parentInternal != null && !(parentInternal is FrameworkElement))
            {
                Visual visual = parentInternal as Visual;
                if (visual != null)
                {
                    visual.VisualAncestorChanged += new Visual.AncestorChangedEventHandler(this.OnVisualAncestorChanged);
                }
                else if (parentInternal is Visual3D)
                {
                    ((Visual3D)parentInternal).VisualAncestorChanged += new Visual.AncestorChangedEventHandler(this.OnVisualAncestorChanged);
                }
            }
            else if (oldParent != null && !(oldParent is FrameworkElement))
            {
                Visual visual2 = oldParent as Visual;
                if (visual2 != null)
                {
                    visual2.VisualAncestorChanged -= new Visual.AncestorChangedEventHandler(this.OnVisualAncestorChanged);
                }
                else if (oldParent is Visual3D)
                {
                    ((Visual3D)oldParent).VisualAncestorChanged -= new Visual.AncestorChangedEventHandler(this.OnVisualAncestorChanged);
                }
            }
            if (this.Parent == null)
            {
                DependencyObject parent = (parentInternal != null) ? parentInternal : oldParent;
                TreeWalkHelper.InvalidateOnTreeChange(this, null, parent, parentInternal != null);
            }
            this.TryFireInitialized();
            base.OnVisualParentChanged(oldParent);
        }

        internal new void OnVisualAncestorChanged(object sender, AncestorChangedEventArgs e)
        {
            FrameworkElement frameworkElement = null;
            FrameworkContentElement frameworkContentElement = null;
            FrameworkElement.GetContainingFrameworkElement(VisualTreeHelper.GetParent(this), out frameworkElement, out frameworkContentElement);
            if (e.OldParent == null)
            {
                if (frameworkElement == null || !VisualTreeHelper.IsAncestorOf(e.Ancestor, frameworkElement))
                {
                    BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, null, VisualTreeHelper.GetParent(e.Ancestor));
                    BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, null, VisualTreeHelper.GetParent(e.Ancestor));
                    return;
                }
            }
            else if (frameworkElement == null)
            {
                FrameworkElement.GetContainingFrameworkElement(e.OldParent, out frameworkElement, out frameworkContentElement);
                if (frameworkElement != null)
                {
                    BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, frameworkElement, null);
                    BroadcastEventHelper.BroadcastLoadedOrUnloadedEvent(this, frameworkElement, null);
                }
            }
        }

        private static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == BindingExpressionBase.DisconnectedItem)
            {
                return;
            }
            ((FrameworkElement)d).RaiseDependencyPropertyChanged(FrameworkElement.DataContextChangedKey, e);
        }

        /// <summary>Returns the <see cref="T:System.Windows.Data.BindingExpression" /> that represents the binding on the specified property. </summary>
        /// <param name="dp">The target <see cref="T:System.Windows.DependencyProperty" /> to get the binding from.</param>
        /// <returns>A <see cref="T:System.Windows.Data.BindingExpression" /> if the target property has an active binding; otherwise, returns null.</returns>
        public BindingExpression GetBindingExpression(DependencyProperty dp)
        {
            return BindingOperations.GetBindingExpression(this, dp);
        }

        /// <summary>Attaches a binding to this element, based on the provided binding object. </summary>
        /// <param name="dp">Identifies the property where the binding should be established.</param>
        /// <param name="binding">Represents the specifics of the data binding.</param>
        /// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
        public BindingExpressionBase SetBinding(DependencyProperty dp, BindingBase binding)
        {
            return BindingOperations.SetBinding(this, dp, binding);
        }

        /// <summary>Attaches a binding to this element, based on the provided source property name as a path qualification to the data source. </summary>
        /// <param name="dp">Identifies the destination property where the binding should be established.</param>
        /// <param name="path">The source property name or the path to the property used for the binding.</param>
        /// <returns>Records the conditions of the binding. This return value can be useful for error checking.</returns>
        public BindingExpression SetBinding(DependencyProperty dp, string path)
        {
            return (BindingExpression)this.SetBinding(dp, new Binding(path));
        }

        /// <summary>Returns an alternative logical parent for this element if there is no visual parent.</summary>
        /// <returns>Returns something other than null whenever a WPF framework-level implementation of this method has a non-visual parent connection.</returns>
        protected internal override DependencyObject GetUIParentCore()
        {
            return this._parent;
        }

        internal override object AdjustEventSource(RoutedEventArgs args)
        {
            object result = null;
            if (this._parent != null || this.HasLogicalChildren)
            {
                DependencyObject dependencyObject = args.Source as DependencyObject;
                if (dependencyObject == null || !this.IsLogicalDescendent(dependencyObject))
                {
                    args.Source = this;
                    result = this;
                }
            }
            return result;
        }

        internal virtual void AdjustBranchSource(RoutedEventArgs args)
        {
        }

        internal override bool BuildRouteCore(EventRoute route, RoutedEventArgs args)
        {
            return this.BuildRouteCoreHelper(route, args, true);
        }

        internal bool BuildRouteCoreHelper(EventRoute route, RoutedEventArgs args, bool shouldAddIntermediateElementsToRoute)
        {
            bool result = false;
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            DependencyObject uIParentCore = this.GetUIParentCore();
            DependencyObject dependencyObject = route.PeekBranchNode() as DependencyObject;
            if (dependencyObject != null && this.IsLogicalDescendent(dependencyObject))
            {
                args.Source = route.PeekBranchSource();
                this.AdjustBranchSource(args);
                route.AddSource(args.Source);
                route.PopBranchNode();
                if (shouldAddIntermediateElementsToRoute)
                {
                    FrameworkElement.AddIntermediateElementsToRoute(this, route, args, LogicalTreeHelper.GetParent(dependencyObject));
                }
            }
            if (!this.IgnoreModelParentBuildRoute(args))
            {
                if (parent == null)
                {
                    result = (uIParentCore != null);
                }
                else if (uIParentCore != null)
                {
                    Visual visual = parent as Visual;
                    if (visual != null)
                    {
                        if (visual.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
                        {
                            result = true;
                        }
                    }
                    else if (((Visual3D)parent).CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
                    {
                        result = true;
                    }
                    route.PushBranchNode(this, args.Source);
                    args.Source = parent;
                }
            }
            return result;
        }

        internal override void AddToEventRouteCore(EventRoute route, RoutedEventArgs args)
        {
            FrameworkElement.AddStyleHandlersToEventRoute(this, null, route, args);
        }

        internal static void AddStyleHandlersToEventRoute(FrameworkElement fe, FrameworkContentElement fce, EventRoute route, RoutedEventArgs args)
        {
            DependencyObject source = (fe != null) ? fe : fce;
            FrameworkTemplate frameworkTemplate = null;
            Style style;
            DependencyObject templatedParent;
            int templateChildIndex;
            if (fe != null)
            {
                style = fe.Style;
                frameworkTemplate = fe.TemplateInternal;
                templatedParent = fe.TemplatedParent;
                templateChildIndex = fe.TemplateChildIndex;
            }
            else
            {
                style = fce.Style;
                templatedParent = fce.TemplatedParent;
                templateChildIndex = fce.TemplateChildIndex;
            }
            if (style != null && style.EventHandlersStore != null)
            {
                RoutedEventHandlerInfo[] handlers = style.EventHandlersStore.GetRoutedEventHandlers(args.RoutedEvent);
                FrameworkElement.AddStyleHandlersToEventRoute(route, source, handlers);
            }
            if (frameworkTemplate != null && frameworkTemplate.EventHandlersStore != null)
            {
                RoutedEventHandlerInfo[] handlers = frameworkTemplate.EventHandlersStore.GetRoutedEventHandlers(args.RoutedEvent);
                FrameworkElement.AddStyleHandlersToEventRoute(route, source, handlers);
            }
            if (templatedParent != null)
            {
                FrameworkTemplate templateInternal = (templatedParent as FrameworkElement).TemplateInternal;
                RoutedEventHandlerInfo[] handlers = null;
                if (templateInternal != null && templateInternal.HasEventDependents)
                {
                    handlers = StyleHelper.GetChildRoutedEventHandlers(templateChildIndex, args.RoutedEvent, ref templateInternal.EventDependents);
                }
                FrameworkElement.AddStyleHandlersToEventRoute(route, source, handlers);
            }
        }

        private static void AddStyleHandlersToEventRoute(EventRoute route, DependencyObject source, RoutedEventHandlerInfo[] handlers)
        {
            if (handlers != null)
            {
                for (int i = 0; i < handlers.Length; i++)
                {
                    route.Add(source, handlers[i].Handler, handlers[i].InvokeHandledEventsToo);
                }
            }
        }

        internal virtual bool IgnoreModelParentBuildRoute(RoutedEventArgs args)
        {
            return false;
        }

        internal override bool InvalidateAutomationAncestorsCore(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree)
        {
            bool shouldInvalidateIntermediateElements = true;
            return this.InvalidateAutomationAncestorsCoreHelper(branchNodeStack, out continuePastCoreTree, shouldInvalidateIntermediateElements);
        }

        internal override void InvalidateForceInheritPropertyOnChildren(DependencyProperty property)
        {
            if (property == UIElement.IsEnabledProperty)
            {
                IEnumerator logicalChildren = this.LogicalChildren;
                if (logicalChildren != null)
                {
                    while (logicalChildren.MoveNext())
                    {
                        DependencyObject dependencyObject = logicalChildren.Current as DependencyObject;
                        if (dependencyObject != null)
                        {
                            dependencyObject.CoerceValue(property);
                        }
                    }
                }
            }
            base.InvalidateForceInheritPropertyOnChildren(property);
        }

        internal bool InvalidateAutomationAncestorsCoreHelper(Stack<DependencyObject> branchNodeStack, out bool continuePastCoreTree, bool shouldInvalidateIntermediateElements)
        {
            bool result = true;
            continuePastCoreTree = false;
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            DependencyObject uIParentCore = this.GetUIParentCore();
            DependencyObject dependencyObject = (branchNodeStack.Count > 0) ? branchNodeStack.Peek() : null;
            if (dependencyObject != null && this.IsLogicalDescendent(dependencyObject))
            {
                branchNodeStack.Pop();
                if (shouldInvalidateIntermediateElements)
                {
                    result = FrameworkElement.InvalidateAutomationIntermediateElements(this, LogicalTreeHelper.GetParent(dependencyObject));
                }
            }
            if (parent == null)
            {
                continuePastCoreTree = (uIParentCore != null);
            }
            else if (uIParentCore != null)
            {
                Visual visual = parent as Visual;
                if (visual != null)
                {
                    if (visual.CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
                    {
                        continuePastCoreTree = true;
                    }
                }
                else if (((Visual3D)parent).CheckFlagsAnd(VisualFlags.IsLayoutIslandRoot))
                {
                    continuePastCoreTree = true;
                }
                branchNodeStack.Push(this);
            }
            return result;
        }

        internal static bool InvalidateAutomationIntermediateElements(DependencyObject mergePoint, DependencyObject modelTreeNode)
        {
            UIElement uIElement = null;
            ContentElement contentElement = null;
            UIElement3D uIElement3D = null;
            while (modelTreeNode != null && modelTreeNode != mergePoint)
            {
                if (!UIElementHelper.InvalidateAutomationPeer(modelTreeNode, out uIElement, out contentElement, out uIElement3D))
                {
                    return false;
                }
                modelTreeNode = LogicalTreeHelper.GetParent(modelTreeNode);
            }
            return true;
        }

        /// <summary>Attempts to bring this element into view, within any scrollable regions it is contained within. </summary>
        public void BringIntoView()
        {
            this.BringIntoView(Rect.Empty);
        }

        /// <summary>Attempts to bring the provided region size of this element into view, within any scrollable regions it is contained within. </summary>
        /// <param name="targetRectangle">Specified size of the element that should also be brought into view. </param>
        public void BringIntoView(Rect targetRectangle)
        {
            base.RaiseEvent(new RequestBringIntoViewEventArgs(this, targetRectangle)
            {
                RoutedEvent = FrameworkElement.RequestBringIntoViewEvent
            });
        }

        private static object GetActualWidth(DependencyObject d, out BaseValueSourceInternal source)
        {
            FrameworkElement frameworkElement = (FrameworkElement)d;
            if (frameworkElement.HasWidthEverChanged)
            {
                source = BaseValueSourceInternal.Local;
                return frameworkElement.RenderSize.Width;
            }
            source = BaseValueSourceInternal.Default;
            return 0.0;
        }

        private static object GetActualHeight(DependencyObject d, out BaseValueSourceInternal source)
        {
            FrameworkElement frameworkElement = (FrameworkElement)d;
            if (frameworkElement.HasHeightEverChanged)
            {
                source = BaseValueSourceInternal.Local;
                return frameworkElement.RenderSize.Height;
            }
            source = BaseValueSourceInternal.Default;
            return 0.0;
        }

        private static void OnLayoutTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement)d).AreTransformsClean = false;
        }

        private static bool IsWidthHeightValid(object value)
        {
            double num = (double)value;
            return DoubleUtil.IsNaN(num) || (num >= 0.0 && !double.IsPositiveInfinity(num));
        }

        private static bool IsMinWidthHeightValid(object value)
        {
            double num = (double)value;
            return !DoubleUtil.IsNaN(num) && num >= 0.0 && !double.IsPositiveInfinity(num);
        }

        private static bool IsMaxWidthHeightValid(object value)
        {
            double num = (double)value;
            return !DoubleUtil.IsNaN(num) && num >= 0.0;
        }

        private static void OnTransformDirty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement)d).AreTransformsClean = false;
        }

        private static object CoerceFlowDirectionProperty(DependencyObject d, object value)
        {
            FrameworkElement frameworkElement = d as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.InvalidateArrange();
                frameworkElement.InvalidateVisual();
                frameworkElement.AreTransformsClean = false;
            }
            return value;
        }

        private static void OnFlowDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement frameworkElement = d as FrameworkElement;
            if (frameworkElement != null)
            {
                frameworkElement.IsRightToLeft = ((FlowDirection)e.NewValue == FlowDirection.RightToLeft);
                frameworkElement.AreTransformsClean = false;
            }
        }

        /// <summary>Gets the value of the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> attached property for the specified <see cref="T:System.Windows.DependencyObject" />. </summary>
        /// <param name="element">The element to return a <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> for.</param>
        /// <returns>The requested flow direction, as a value of the enumeration.</returns>
        public static FlowDirection GetFlowDirection(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (FlowDirection)element.GetValue(FrameworkElement.FlowDirectionProperty);
        }

        /// <summary>Sets the value of the <see cref="P:System.Windows.FrameworkElement.FlowDirection" /> attached property for the provided element. </summary>
        /// <param name="element">The element that specifies a flow direction.</param>
        /// <param name="value">A value of the enumeration, specifying the direction.</param>
        public static void SetFlowDirection(DependencyObject element, FlowDirection value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(FrameworkElement.FlowDirectionProperty, value);
        }

        private static bool IsValidFlowDirection(object o)
        {
            FlowDirection flowDirection = (FlowDirection)o;
            return flowDirection == FlowDirection.LeftToRight || flowDirection == FlowDirection.RightToLeft;
        }

        private static bool IsMarginValid(object value)
        {
            return ((Thickness)value).IsValid(true, false, true, false);
        }

        internal static bool ValidateHorizontalAlignmentValue(object value)
        {
            HorizontalAlignment horizontalAlignment = (HorizontalAlignment)value;
            return horizontalAlignment == HorizontalAlignment.Left || horizontalAlignment == HorizontalAlignment.Center || horizontalAlignment == HorizontalAlignment.Right || horizontalAlignment == HorizontalAlignment.Stretch;
        }

        internal static bool ValidateVerticalAlignmentValue(object value)
        {
            VerticalAlignment verticalAlignment = (VerticalAlignment)value;
            return verticalAlignment == VerticalAlignment.Top || verticalAlignment == VerticalAlignment.Center || verticalAlignment == VerticalAlignment.Bottom || verticalAlignment == VerticalAlignment.Stretch;
        }

        private static void OnCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)d).IsMouseOver)
            {
                Mouse.UpdateCursor();
            }
        }

        private static void OnForceCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((FrameworkElement)d).IsMouseOver)
            {
                Mouse.UpdateCursor();
            }
        }

        private static void OnQueryCursorOverride(object sender, QueryCursorEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement)sender;
            Cursor cursor = frameworkElement.Cursor;
            if (cursor != null && (!e.Handled || frameworkElement.ForceCursor))
            {
                e.Cursor = cursor;
                e.Handled = true;
            }
        }

        private Transform GetFlowDirectionTransform()
        {
            if (!this.BypassLayoutPolicies && FrameworkElement.ShouldApplyMirrorTransform(this))
            {
                return new MatrixTransform(-1.0, 0.0, 0.0, 1.0, base.RenderSize.Width, 0.0);
            }
            return null;
        }

        internal static bool ShouldApplyMirrorTransform(FrameworkElement fe)
        {
            FlowDirection flowDirection = fe.FlowDirection;
            FlowDirection parentFD = FlowDirection.LeftToRight;
            DependencyObject parent = VisualTreeHelper.GetParent(fe);
            FrameworkElement frameworkElement;
            FrameworkContentElement frameworkContentElement;
            if (parent != null)
            {
                parentFD = FrameworkElement.GetFlowDirectionFromVisual(parent);
            }
            else if (FrameworkElement.GetFrameworkParent(fe, out frameworkElement, out frameworkContentElement))
            {
                if (frameworkElement != null && frameworkElement is IContentHost)
                {
                    parentFD = frameworkElement.FlowDirection;
                }
                else if (frameworkContentElement != null)
                {
                    parentFD = (FlowDirection)frameworkContentElement.GetValue(FrameworkElement.FlowDirectionProperty);
                }
            }
            return FrameworkElement.ApplyMirrorTransform(parentFD, flowDirection);
        }

        private static FlowDirection GetFlowDirectionFromVisual(DependencyObject visual)
        {
            FlowDirection result = FlowDirection.LeftToRight;
            for (DependencyObject dependencyObject = visual; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
            {
                FrameworkElement frameworkElement = dependencyObject as FrameworkElement;
                if (frameworkElement != null)
                {
                    result = frameworkElement.FlowDirection;
                    break;
                }
                object obj = dependencyObject.ReadLocalValue(FrameworkElement.FlowDirectionProperty);
                if (obj != DependencyProperty.UnsetValue)
                {
                    result = (FlowDirection)obj;
                    break;
                }
            }
            return result;
        }

        internal static bool ApplyMirrorTransform(FlowDirection parentFD, FlowDirection thisFD)
        {
            return (parentFD == FlowDirection.LeftToRight && thisFD == FlowDirection.RightToLeft) || (parentFD == FlowDirection.RightToLeft && thisFD == FlowDirection.LeftToRight);
        }

        private Size FindMaximalAreaLocalSpaceRect(Transform layoutTransform, Size transformSpaceBounds)
        {
            double num = transformSpaceBounds.Width;
            double num2 = transformSpaceBounds.Height;
            if (DoubleUtil.IsZero(num) || DoubleUtil.IsZero(num2))
            {
                return new Size(0.0, 0.0);
            }
            bool flag = double.IsInfinity(num);
            bool flag2 = double.IsInfinity(num2);
            if (flag & flag2)
            {
                return new Size(double.PositiveInfinity, double.PositiveInfinity);
            }
            if (flag)
            {
                num = num2;
            }
            else if (flag2)
            {
                num2 = num;
            }
            Matrix value = layoutTransform.Value;
            if (!value.HasInverse)
            {
                return new Size(0.0, 0.0);
            }
            double m = value.M11;
            double m2 = value.M12;
            double m3 = value.M21;
            double m4 = value.M22;
            double num5;
            double num6;
            if (DoubleUtil.IsZero(m2) || DoubleUtil.IsZero(m3))
            {
                double num3 = flag2 ? double.PositiveInfinity : Math.Abs(num2 / m4);
                double num4 = flag ? double.PositiveInfinity : Math.Abs(num / m);
                if (DoubleUtil.IsZero(m2))
                {
                    if (DoubleUtil.IsZero(m3))
                    {
                        num5 = num3;
                        num6 = num4;
                    }
                    else
                    {
                        num5 = Math.Min(0.5 * Math.Abs(num / m3), num3);
                        num6 = num4 - m3 * num5 / m;
                    }
                }
                else
                {
                    num6 = Math.Min(0.5 * Math.Abs(num2 / m2), num4);
                    num5 = num3 - m2 * num6 / m4;
                }
            }
            else if (DoubleUtil.IsZero(m) || DoubleUtil.IsZero(m4))
            {
                double num7 = Math.Abs(num2 / m2);
                double num8 = Math.Abs(num / m3);
                if (DoubleUtil.IsZero(m))
                {
                    if (DoubleUtil.IsZero(m4))
                    {
                        num5 = num8;
                        num6 = num7;
                    }
                    else
                    {
                        num5 = Math.Min(0.5 * Math.Abs(num2 / m4), num8);
                        num6 = num7 - m4 * num5 / m2;
                    }
                }
                else
                {
                    num6 = Math.Min(0.5 * Math.Abs(num / m), num7);
                    num5 = num8 - m * num6 / m3;
                }
            }
            else
            {
                double num9 = Math.Abs(num / m);
                double num10 = Math.Abs(num / m3);
                double num11 = Math.Abs(num2 / m2);
                double num12 = Math.Abs(num2 / m4);
                num6 = Math.Min(num11, num9) * 0.5;
                num5 = Math.Min(num10, num12) * 0.5;
                if ((DoubleUtil.GreaterThanOrClose(num9, num11) && DoubleUtil.LessThanOrClose(num10, num12)) || (DoubleUtil.LessThanOrClose(num9, num11) && DoubleUtil.GreaterThanOrClose(num10, num12)))
                {
                    Rect rect = Rect.Transform(new Rect(0.0, 0.0, num6, num5), layoutTransform.Value);
                    double num13 = Math.Min(num / rect.Width, num2 / rect.Height);
                    if (!double.IsNaN(num13) && !double.IsInfinity(num13))
                    {
                        num6 *= num13;
                        num5 *= num13;
                    }
                }
            }
            return new Size(num6, num5);
        }

        /// <summary>Implements basic measure-pass layout system behavior for <see cref="T:System.Windows.FrameworkElement" />. </summary>
        /// <param name="availableSize">The available size that the parent element can give to the child elements.</param>
        /// <returns>The desired size of this element in layout.</returns>
        protected sealed override Size MeasureCore(Size availableSize)
        {
            bool useLayoutRounding = this.UseLayoutRounding;
            if (useLayoutRounding && !base.CheckFlagsAnd(VisualFlags.UseLayoutRounding))
            {
                base.SetFlags(true, VisualFlags.UseLayoutRounding);
            }
            this.ApplyTemplate();
            if (this.BypassLayoutPolicies)
            {
                return this.MeasureOverride(availableSize);
            }
            Thickness margin = this.Margin;
            double num = margin.Left + margin.Right;
            double num2 = margin.Top + margin.Bottom;
            if (useLayoutRounding && (this is ScrollContentPresenter || !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness))
            {
                num = UIElement.RoundLayoutValue(num, FrameworkElement.DpiScaleX);
                num2 = UIElement.RoundLayoutValue(num2, FrameworkElement.DpiScaleY);
            }
            Size size = new Size(Math.Max(availableSize.Width - num, 0.0), Math.Max(availableSize.Height - num2, 0.0));
            FrameworkElement.MinMax minMax = new FrameworkElement.MinMax(this);
            if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
            {
                minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, FrameworkElement.DpiScaleY);
                minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, FrameworkElement.DpiScaleX);
                minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, FrameworkElement.DpiScaleY);
                minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, FrameworkElement.DpiScaleX);
            }
            FrameworkElement.LayoutTransformData layoutTransformData = FrameworkElement.LayoutTransformDataField.GetValue(this);
            Transform layoutTransform = this.LayoutTransform;
            if (layoutTransform != null && !layoutTransform.IsIdentity)
            {
                if (layoutTransformData == null)
                {
                    layoutTransformData = new FrameworkElement.LayoutTransformData();
                    FrameworkElement.LayoutTransformDataField.SetValue(this, layoutTransformData);
                }
                layoutTransformData.CreateTransformSnapshot(layoutTransform);
                layoutTransformData.UntransformedDS = default(Size);
                if (useLayoutRounding)
                {
                    layoutTransformData.TransformedUnroundedDS = default(Size);
                }
            }
            else if (layoutTransformData != null)
            {
                layoutTransformData = null;
                FrameworkElement.LayoutTransformDataField.ClearValue(this);
            }
            if (layoutTransformData != null)
            {
                size = this.FindMaximalAreaLocalSpaceRect(layoutTransformData.Transform, size);
            }
            size.Width = Math.Max(minMax.minWidth, Math.Min(size.Width, minMax.maxWidth));
            size.Height = Math.Max(minMax.minHeight, Math.Min(size.Height, minMax.maxHeight));
            if (useLayoutRounding)
            {
                size = UIElement.RoundLayoutSize(size, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            Size size2 = this.MeasureOverride(size);
            size2 = new Size(Math.Max(size2.Width, minMax.minWidth), Math.Max(size2.Height, minMax.minHeight));
            Size size3 = size2;
            if (layoutTransformData != null)
            {
                layoutTransformData.UntransformedDS = size3;
                Rect rect = Rect.Transform(new Rect(0.0, 0.0, size3.Width, size3.Height), layoutTransformData.Transform.Value);
                size3.Width = rect.Width;
                size3.Height = rect.Height;
            }
            bool flag = false;
            if (size2.Width > minMax.maxWidth)
            {
                size2.Width = minMax.maxWidth;
                flag = true;
            }
            if (size2.Height > minMax.maxHeight)
            {
                size2.Height = minMax.maxHeight;
                flag = true;
            }
            if (layoutTransformData != null)
            {
                Rect rect2 = Rect.Transform(new Rect(0.0, 0.0, size2.Width, size2.Height), layoutTransformData.Transform.Value);
                size2.Width = rect2.Width;
                size2.Height = rect2.Height;
            }
            double num3 = size2.Width + num;
            double num4 = size2.Height + num2;
            if (num3 > availableSize.Width)
            {
                num3 = availableSize.Width;
                flag = true;
            }
            if (num4 > availableSize.Height)
            {
                num4 = availableSize.Height;
                flag = true;
            }
            if (layoutTransformData != null)
            {
                layoutTransformData.TransformedUnroundedDS = new Size(Math.Max(0.0, num3), Math.Max(0.0, num4));
            }
            if (useLayoutRounding)
            {
                num3 = UIElement.RoundLayoutValue(num3, FrameworkElement.DpiScaleX);
                num4 = UIElement.RoundLayoutValue(num4, FrameworkElement.DpiScaleY);
            }
            SizeBox sizeBox = FrameworkElement.UnclippedDesiredSizeField.GetValue(this);
            if (flag || num3 < 0.0 || num4 < 0.0)
            {
                if (sizeBox == null)
                {
                    sizeBox = new SizeBox(size3);
                    FrameworkElement.UnclippedDesiredSizeField.SetValue(this, sizeBox);
                }
                else
                {
                    sizeBox.Width = size3.Width;
                    sizeBox.Height = size3.Height;
                }
            }
            else if (sizeBox != null)
            {
                FrameworkElement.UnclippedDesiredSizeField.ClearValue(this);
            }
            return new Size(Math.Max(0.0, num3), Math.Max(0.0, num4));
        }

        /// <summary>Implements <see cref="M:System.Windows.UIElement.ArrangeCore(System.Windows.Rect)" /> (defined as virtual in <see cref="T:System.Windows.UIElement" />) and seals the implementation.</summary>
        /// <param name="finalRect">The final area within the parent that this element should use to arrange itself and its children.</param>
        protected sealed override void ArrangeCore(Rect finalRect)
        {
            bool useLayoutRounding = this.UseLayoutRounding;
            FrameworkElement.LayoutTransformData value = FrameworkElement.LayoutTransformDataField.GetValue(this);
            Size size = Size.Empty;
            if (useLayoutRounding && !base.CheckFlagsAnd(VisualFlags.UseLayoutRounding))
            {
                base.SetFlags(true, VisualFlags.UseLayoutRounding);
            }
            if (this.BypassLayoutPolicies)
            {
                Size renderSize = base.RenderSize;
                Size renderSize2 = this.ArrangeOverride(finalRect.Size);
                base.RenderSize = renderSize2;
                this.SetLayoutOffset(new Vector(finalRect.X, finalRect.Y), renderSize);
                return;
            }
            this.NeedsClipBounds = false;
            Size size2 = finalRect.Size;
            Thickness margin = this.Margin;
            double num = margin.Left + margin.Right;
            double num2 = margin.Top + margin.Bottom;
            if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
            {
                num = UIElement.RoundLayoutValue(num, FrameworkElement.DpiScaleX);
                num2 = UIElement.RoundLayoutValue(num2, FrameworkElement.DpiScaleY);
            }
            size2.Width = Math.Max(0.0, size2.Width - num);
            size2.Height = Math.Max(0.0, size2.Height - num2);
            if (useLayoutRounding && value != null)
            {
                Size arg_120_0 = value.TransformedUnroundedDS;
                size = value.TransformedUnroundedDS;
                size.Width = Math.Max(0.0, size.Width - num);
                size.Height = Math.Max(0.0, size.Height - num2);
            }
            SizeBox value2 = FrameworkElement.UnclippedDesiredSizeField.GetValue(this);
            Size untransformedDS;
            if (value2 == null)
            {
                untransformedDS = new Size(Math.Max(0.0, base.DesiredSize.Width - num), Math.Max(0.0, base.DesiredSize.Height - num2));
                if (size != Size.Empty)
                {
                    untransformedDS.Width = Math.Max(size.Width, untransformedDS.Width);
                    untransformedDS.Height = Math.Max(size.Height, untransformedDS.Height);
                }
            }
            else
            {
                untransformedDS = new Size(value2.Width, value2.Height);
            }
            if (DoubleUtil.LessThan(size2.Width, untransformedDS.Width))
            {
                this.NeedsClipBounds = true;
                size2.Width = untransformedDS.Width;
            }
            if (DoubleUtil.LessThan(size2.Height, untransformedDS.Height))
            {
                this.NeedsClipBounds = true;
                size2.Height = untransformedDS.Height;
            }
            if (this.HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                size2.Width = untransformedDS.Width;
            }
            if (this.VerticalAlignment != VerticalAlignment.Stretch)
            {
                size2.Height = untransformedDS.Height;
            }
            if (value != null)
            {
                Size size3 = this.FindMaximalAreaLocalSpaceRect(value.Transform, size2);
                size2 = size3;
                untransformedDS = value.UntransformedDS;
                if (!DoubleUtil.IsZero(size3.Width) && !DoubleUtil.IsZero(size3.Height) && (LayoutDoubleUtil.LessThan(size3.Width, untransformedDS.Width) || LayoutDoubleUtil.LessThan(size3.Height, untransformedDS.Height)))
                {
                    size2 = untransformedDS;
                }
                if (DoubleUtil.LessThan(size2.Width, untransformedDS.Width))
                {
                    this.NeedsClipBounds = true;
                    size2.Width = untransformedDS.Width;
                }
                if (DoubleUtil.LessThan(size2.Height, untransformedDS.Height))
                {
                    this.NeedsClipBounds = true;
                    size2.Height = untransformedDS.Height;
                }
            }
            FrameworkElement.MinMax minMax = new FrameworkElement.MinMax(this);
            if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
            {
                minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, FrameworkElement.DpiScaleY);
                minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, FrameworkElement.DpiScaleX);
                minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, FrameworkElement.DpiScaleY);
                minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, FrameworkElement.DpiScaleX);
            }
            double num3 = Math.Max(untransformedDS.Width, minMax.maxWidth);
            if (DoubleUtil.LessThan(num3, size2.Width))
            {
                this.NeedsClipBounds = true;
                size2.Width = num3;
            }
            double num4 = Math.Max(untransformedDS.Height, minMax.maxHeight);
            if (DoubleUtil.LessThan(num4, size2.Height))
            {
                this.NeedsClipBounds = true;
                size2.Height = num4;
            }
            if (useLayoutRounding)
            {
                size2 = UIElement.RoundLayoutSize(size2, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            Size renderSize3 = base.RenderSize;
            Size renderSize4 = this.ArrangeOverride(size2);
            base.RenderSize = renderSize4;
            if (useLayoutRounding)
            {
                base.RenderSize = UIElement.RoundLayoutSize(base.RenderSize, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            Size size4 = new Size(Math.Min(renderSize4.Width, minMax.maxWidth), Math.Min(renderSize4.Height, minMax.maxHeight));
            if (useLayoutRounding)
            {
                size4 = UIElement.RoundLayoutSize(size4, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            this.NeedsClipBounds |= (DoubleUtil.LessThan(size4.Width, renderSize4.Width) || DoubleUtil.LessThan(size4.Height, renderSize4.Height));
            if (value != null)
            {
                Rect rect = Rect.Transform(new Rect(0.0, 0.0, size4.Width, size4.Height), value.Transform.Value);
                size4.Width = rect.Width;
                size4.Height = rect.Height;
                if (useLayoutRounding)
                {
                    size4 = UIElement.RoundLayoutSize(size4, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
                }
            }
            Size size5 = new Size(Math.Max(0.0, finalRect.Width - num), Math.Max(0.0, finalRect.Height - num2));
            if (useLayoutRounding)
            {
                size5 = UIElement.RoundLayoutSize(size5, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            this.NeedsClipBounds |= (DoubleUtil.LessThan(size5.Width, size4.Width) || DoubleUtil.LessThan(size5.Height, size4.Height));
            Vector offset = this.ComputeAlignmentOffset(size5, size4);
            offset.X += finalRect.X + margin.Left;
            offset.Y += finalRect.Y + margin.Top;
            if (useLayoutRounding)
            {
                offset.X = UIElement.RoundLayoutValue(offset.X, FrameworkElement.DpiScaleX);
                offset.Y = UIElement.RoundLayoutValue(offset.Y, FrameworkElement.DpiScaleY);
            }
            this.SetLayoutOffset(offset, renderSize3);
        }

        /// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.SizeChanged" /> event, using the specified information as part of the eventual event data. </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            SizeChangedEventArgs sizeChangedEventArgs = new SizeChangedEventArgs(this, sizeInfo);
            sizeChangedEventArgs.RoutedEvent = FrameworkElement.SizeChangedEvent;
            if (sizeInfo.WidthChanged)
            {
                this.HasWidthEverChanged = true;
                base.NotifyPropertyChange(new DependencyPropertyChangedEventArgs(FrameworkElement.ActualWidthProperty, FrameworkElement._actualWidthMetadata, sizeInfo.PreviousSize.Width, sizeInfo.NewSize.Width));
            }
            if (sizeInfo.HeightChanged)
            {
                this.HasHeightEverChanged = true;
                base.NotifyPropertyChange(new DependencyPropertyChangedEventArgs(FrameworkElement.ActualHeightProperty, FrameworkElement._actualHeightMetadata, sizeInfo.PreviousSize.Height, sizeInfo.NewSize.Height));
            }
            base.RaiseEvent(sizeChangedEventArgs);
        }

        private Vector ComputeAlignmentOffset(Size clientSize, Size inkSize)
        {
            Vector result = default(Vector);
            HorizontalAlignment horizontalAlignment = this.HorizontalAlignment;
            VerticalAlignment verticalAlignment = this.VerticalAlignment;
            if (horizontalAlignment == HorizontalAlignment.Stretch && inkSize.Width > clientSize.Width)
            {
                horizontalAlignment = HorizontalAlignment.Left;
            }
            if (verticalAlignment == VerticalAlignment.Stretch && inkSize.Height > clientSize.Height)
            {
                verticalAlignment = VerticalAlignment.Top;
            }
            if (horizontalAlignment == HorizontalAlignment.Center || horizontalAlignment == HorizontalAlignment.Stretch)
            {
                result.X = (clientSize.Width - inkSize.Width) * 0.5;
            }
            else if (horizontalAlignment == HorizontalAlignment.Right)
            {
                result.X = clientSize.Width - inkSize.Width;
            }
            else
            {
                result.X = 0.0;
            }
            if (verticalAlignment == VerticalAlignment.Center || verticalAlignment == VerticalAlignment.Stretch)
            {
                result.Y = (clientSize.Height - inkSize.Height) * 0.5;
            }
            else if (verticalAlignment == VerticalAlignment.Bottom)
            {
                result.Y = clientSize.Height - inkSize.Height;
            }
            else
            {
                result.Y = 0.0;
            }
            return result;
        }

        /// <summary>Returns a geometry for a clipping mask. The mask applies if the layout system attempts to arrange an element that is larger than the available display space.</summary>
        /// <param name="layoutSlotSize">The size of the part of the element that does visual presentation. </param>
        /// <returns>The clipping geometry.</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            bool useLayoutRounding = this.UseLayoutRounding;
            if (useLayoutRounding && !base.CheckFlagsAnd(VisualFlags.UseLayoutRounding))
            {
                base.SetFlags(true, VisualFlags.UseLayoutRounding);
            }
            if (!this.NeedsClipBounds && !base.ClipToBounds)
            {
                return base.GetLayoutClip(layoutSlotSize);
            }
            FrameworkElement.MinMax minMax = new FrameworkElement.MinMax(this);
            if (useLayoutRounding && !FrameworkAppContextSwitches.DoNotApplyLayoutRoundingToMarginsAndBorderThickness)
            {
                minMax.maxHeight = UIElement.RoundLayoutValue(minMax.maxHeight, FrameworkElement.DpiScaleY);
                minMax.maxWidth = UIElement.RoundLayoutValue(minMax.maxWidth, FrameworkElement.DpiScaleX);
                minMax.minHeight = UIElement.RoundLayoutValue(minMax.minHeight, FrameworkElement.DpiScaleY);
                minMax.minWidth = UIElement.RoundLayoutValue(minMax.minWidth, FrameworkElement.DpiScaleX);
            }
            Size renderSize = base.RenderSize;
            double num = double.IsPositiveInfinity(minMax.maxWidth) ? renderSize.Width : minMax.maxWidth;
            double num2 = double.IsPositiveInfinity(minMax.maxHeight) ? renderSize.Height : minMax.maxHeight;
            bool flag = base.ClipToBounds || DoubleUtil.LessThan(num, renderSize.Width) || DoubleUtil.LessThan(num2, renderSize.Height);
            renderSize.Width = Math.Min(renderSize.Width, minMax.maxWidth);
            renderSize.Height = Math.Min(renderSize.Height, minMax.maxHeight);
            FrameworkElement.LayoutTransformData value = FrameworkElement.LayoutTransformDataField.GetValue(this);
            Rect rect = default(Rect);
            if (value != null)
            {
                rect = Rect.Transform(new Rect(0.0, 0.0, renderSize.Width, renderSize.Height), value.Transform.Value);
                renderSize.Width = rect.Width;
                renderSize.Height = rect.Height;
            }
            Thickness margin = this.Margin;
            double num3 = margin.Left + margin.Right;
            double num4 = margin.Top + margin.Bottom;
            Size clientSize = new Size(Math.Max(0.0, layoutSlotSize.Width - num3), Math.Max(0.0, layoutSlotSize.Height - num4));
            bool flag2 = base.ClipToBounds || DoubleUtil.LessThan(clientSize.Width, renderSize.Width) || DoubleUtil.LessThan(clientSize.Height, renderSize.Height);
            Transform flowDirectionTransform = this.GetFlowDirectionTransform();
            if (flag && !flag2)
            {
                Rect rect2 = new Rect(0.0, 0.0, num, num2);
                if (useLayoutRounding)
                {
                    rect2 = UIElement.RoundLayoutRect(rect2, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
                }
                RectangleGeometry rectangleGeometry = new RectangleGeometry(rect2);
                if (flowDirectionTransform != null)
                {
                    rectangleGeometry.Transform = flowDirectionTransform;
                }
                return rectangleGeometry;
            }
            if (!flag2)
            {
                return null;
            }
            Vector vector = this.ComputeAlignmentOffset(clientSize, renderSize);
            if (value == null)
            {
                Rect rect3 = new Rect(-vector.X + rect.X, -vector.Y + rect.Y, clientSize.Width, clientSize.Height);
                if (useLayoutRounding)
                {
                    rect3 = UIElement.RoundLayoutRect(rect3, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
                }
                if (flag)
                {
                    Rect rect4 = new Rect(0.0, 0.0, num, num2);
                    if (useLayoutRounding)
                    {
                        rect4 = UIElement.RoundLayoutRect(rect4, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
                    }
                    rect3.Intersect(rect4);
                }
                RectangleGeometry rectangleGeometry2 = new RectangleGeometry(rect3);
                if (flowDirectionTransform != null)
                {
                    rectangleGeometry2.Transform = flowDirectionTransform;
                }
                return rectangleGeometry2;
            }
            Rect rect5 = new Rect(-vector.X + rect.X, -vector.Y + rect.Y, clientSize.Width, clientSize.Height);
            if (useLayoutRounding)
            {
                rect5 = UIElement.RoundLayoutRect(rect5, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
            }
            RectangleGeometry rectangleGeometry3 = new RectangleGeometry(rect5);
            Matrix value2 = value.Transform.Value;
            if (value2.HasInverse)
            {
                value2.Invert();
                rectangleGeometry3.Transform = new MatrixTransform(value2);
            }
            if (flag)
            {
                Rect rect6 = new Rect(0.0, 0.0, num, num2);
                if (useLayoutRounding)
                {
                    rect6 = UIElement.RoundLayoutRect(rect6, FrameworkElement.DpiScaleX, FrameworkElement.DpiScaleY);
                }
                PathGeometry pathGeometry = Geometry.Combine(new RectangleGeometry(rect6), rectangleGeometry3, GeometryCombineMode.Intersect, null);
                if (flowDirectionTransform != null)
                {
                    pathGeometry.Transform = flowDirectionTransform;
                }
                return pathGeometry;
            }
            if (flowDirectionTransform != null)
            {
                if (rectangleGeometry3.Transform != null)
                {
                    rectangleGeometry3.Transform = new MatrixTransform(rectangleGeometry3.Transform.Value * flowDirectionTransform.Value);
                }
                else
                {
                    rectangleGeometry3.Transform = flowDirectionTransform;
                }
            }
            return rectangleGeometry3;
        }

        internal Geometry GetLayoutClipInternal()
        {
            if (base.IsMeasureValid && base.IsArrangeValid)
            {
                return this.GetLayoutClip(base.PreviousArrangeRect.Size);
            }
            return null;
        }

        /// <summary>When overridden in a derived class, measures the size in layout required for child elements and determines a size for the <see cref="T:System.Windows.FrameworkElement" />-derived class. </summary>
        /// <param name="availableSize">The available size that this element can give to child elements. Infinity can be specified as a value to indicate that the element will size to whatever content is available.</param>
        /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
        protected virtual Size MeasureOverride(Size availableSize)
        {
            return new Size(0.0, 0.0);
        }

        /// <summary>When overridden in a derived class, positions child elements and determines a size for a <see cref="T:System.Windows.FrameworkElement" /> derived class. </summary>
        /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
        /// <returns>The actual size used.</returns>
        protected virtual Size ArrangeOverride(Size finalSize)
        {
            return finalSize;
        }

        internal static void InternalSetLayoutTransform(UIElement element, Transform layoutTransform)
        {
            FrameworkElement frameworkElement = element as FrameworkElement;
            element.InternalSetOffsetWorkaround(default(Vector));
            Transform transform = (frameworkElement == null) ? null : frameworkElement.GetFlowDirectionTransform();
            Transform transform2 = element.RenderTransform;
            if (transform2 == Transform.Identity)
            {
                transform2 = null;
            }
            TransformCollection transformCollection = new TransformCollection();
            transformCollection.CanBeInheritanceContext = false;
            if (transform != null)
            {
                transformCollection.Add(transform);
            }
            if (transform2 != null)
            {
                transformCollection.Add(transform2);
            }
            transformCollection.Add(layoutTransform);
            element.InternalSetTransformWorkaround(new TransformGroup
            {
                Children = transformCollection
            });
        }

        private void SetLayoutOffset(Vector offset, Size oldRenderSize)
        {
            if (!base.AreTransformsClean || !DoubleUtil.AreClose(base.RenderSize, oldRenderSize))
            {
                Transform flowDirectionTransform = this.GetFlowDirectionTransform();
                Transform transform = base.RenderTransform;
                if (transform == Transform.Identity)
                {
                    transform = null;
                }
                FrameworkElement.LayoutTransformData value = FrameworkElement.LayoutTransformDataField.GetValue(this);
                TransformGroup transformGroup = null;
                if (flowDirectionTransform != null || transform != null || value != null)
                {
                    transformGroup = new TransformGroup();
                    transformGroup.CanBeInheritanceContext = false;
                    transformGroup.Children.CanBeInheritanceContext = false;
                    if (flowDirectionTransform != null)
                    {
                        transformGroup.Children.Add(flowDirectionTransform);
                    }
                    if (value != null)
                    {
                        transformGroup.Children.Add(value.Transform);
                        FrameworkElement.MinMax minMax = new FrameworkElement.MinMax(this);
                        Size renderSize = base.RenderSize;
                        if (double.IsPositiveInfinity(minMax.maxWidth))
                        {
                            double arg_B5_0 = renderSize.Width;
                        }
                        if (double.IsPositiveInfinity(minMax.maxHeight))
                        {
                            double arg_CB_0 = renderSize.Height;
                        }
                        renderSize.Width = Math.Min(renderSize.Width, minMax.maxWidth);
                        renderSize.Height = Math.Min(renderSize.Height, minMax.maxHeight);
                        Rect rect = Rect.Transform(new Rect(renderSize), value.Transform.Value);
                        transformGroup.Children.Add(new TranslateTransform(-rect.X, -rect.Y));
                    }
                    if (transform != null)
                    {
                        Point renderTransformOrigin = this.GetRenderTransformOrigin();
                        bool expr_172 = renderTransformOrigin.X != 0.0 || renderTransformOrigin.Y != 0.0;
                        if (expr_172)
                        {
                            TranslateTransform translateTransform = new TranslateTransform(-renderTransformOrigin.X, -renderTransformOrigin.Y);
                            translateTransform.Freeze();
                            transformGroup.Children.Add(translateTransform);
                        }
                        transformGroup.Children.Add(transform);
                        if (expr_172)
                        {
                            TranslateTransform translateTransform2 = new TranslateTransform(renderTransformOrigin.X, renderTransformOrigin.Y);
                            translateTransform2.Freeze();
                            transformGroup.Children.Add(translateTransform2);
                        }
                    }
                }
                base.VisualTransform = transformGroup;
                base.AreTransformsClean = true;
            }
            Vector visualOffset = base.VisualOffset;
            if (!DoubleUtil.AreClose(visualOffset.X, offset.X) || !DoubleUtil.AreClose(visualOffset.Y, offset.Y))
            {
                base.VisualOffset = offset;
            }
        }

        private Point GetRenderTransformOrigin()
        {
            Point renderTransformOrigin = base.RenderTransformOrigin;
            Size renderSize = base.RenderSize;
            return new Point(renderSize.Width * renderTransformOrigin.X, renderSize.Height * renderTransformOrigin.Y);
        }

        /// <summary>Moves the keyboard focus away from this element and to another element in a provided traversal direction. </summary>
        /// <param name="request">The direction that focus is to be moved, as a value of the enumeration.</param>
        /// <returns>Returns true if focus is moved successfully; false if the target element in direction as specified does not exist or could not be keyboard focused.</returns>
        public sealed override bool MoveFocus(TraversalRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return KeyboardNavigation.Current.Navigate(this, request);
        }

        /// <summary>Determines the next element that would receive focus relative to this element for a provided focus movement direction, but does not actually move the focus.</summary>
        /// <param name="direction">The direction for which a prospective focus change should be determined.</param>
        /// <returns>The next element that focus would move to if focus were actually traversed. May return null if focus cannot be moved relative to this element for the provided direction.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Specified one of the following directions in the <see cref="T:System.Windows.Input.TraversalRequest" />: <see cref="F:System.Windows.Input.FocusNavigationDirection.Next" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Previous" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.First" />, <see cref="F:System.Windows.Input.FocusNavigationDirection.Last" />. These directions are not legal for <see cref="M:System.Windows.FrameworkElement.PredictFocus(System.Windows.Input.FocusNavigationDirection)" /> (but they are legal for <see cref="M:System.Windows.FrameworkElement.MoveFocus(System.Windows.Input.TraversalRequest)" />). </exception>
        public sealed override DependencyObject PredictFocus(FocusNavigationDirection direction)
        {
            return KeyboardNavigation.Current.PredictFocusedElement(this, direction);
        }

        private static void OnPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (e.OriginalSource == sender)
            {
                IInputElement focusedElement = FocusManager.GetFocusedElement((FrameworkElement)sender, true);
                if (focusedElement != null && focusedElement != sender && Keyboard.IsFocusable(focusedElement as DependencyObject))
                {
                    IInputElement focusedElement2 = Keyboard.FocusedElement;
                    focusedElement.Focus();
                    if (Keyboard.FocusedElement == focusedElement || Keyboard.FocusedElement != focusedElement2)
                    {
                        e.Handled = true;
                        return;
                    }
                }
            }
        }

        private static void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender == e.OriginalSource)
            {
                FrameworkElement frameworkElement = (FrameworkElement)sender;
                KeyboardNavigation.UpdateFocusedElement(frameworkElement);
                KeyboardNavigation arg_20_0 = KeyboardNavigation.Current;
                KeyboardNavigation.ShowFocusVisual();
                arg_20_0.NotifyFocusChanged(frameworkElement, e);
                arg_20_0.UpdateActiveElement(frameworkElement);
            }
        }

        private static void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender == e.OriginalSource)
            {
                KeyboardNavigation.Current.HideFocusVisual();
                if (e.NewFocus == null)
                {
                    KeyboardNavigation.Current.NotifyFocusChanged(sender, e);
                }
            }
        }

        /// <summary>Invoked whenever an unhandled <see cref="E:System.Windows.UIElement.GotFocus" /> event reaches this element in its route.</summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (base.IsKeyboardFocused)
            {
                this.BringIntoView();
            }
            base.OnGotFocus(e);
        }

        /// <summary>Starts the initialization process for this element. </summary>
        public virtual void BeginInit()
        {
            if (this.ReadInternalFlag(InternalFlags.InitPending))
            {
                throw new InvalidOperationException(SR.Get("NestedBeginInitNotSupported"));
            }
            this.WriteInternalFlag(InternalFlags.InitPending, true);
        }

        /// <summary>Indicates that the initialization process for the element is complete. </summary>
        /// <exception cref="T:System.InvalidOperationException">
        ///   <see cref="M:System.Windows.FrameworkElement.EndInit" /> was called without <see cref="M:System.Windows.FrameworkElement.BeginInit" /> having previously been called on the element.</exception>
        public virtual void EndInit()
        {
            if (!this.ReadInternalFlag(InternalFlags.InitPending))
            {
                throw new InvalidOperationException(SR.Get("EndInitWithoutBeginInitNotSupported"));
            }
            this.WriteInternalFlag(InternalFlags.InitPending, false);
            this.TryFireInitialized();
        }

        /// <summary>Raises the <see cref="E:System.Windows.FrameworkElement.Initialized" /> event. This method is invoked whenever <see cref="P:System.Windows.FrameworkElement.IsInitialized" /> is set to true internally. </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected virtual void OnInitialized(EventArgs e)
        {
            if (!this.HasStyleEverBeenFetched)
            {
                this.UpdateStyleProperty();
            }
            if (!this.HasThemeStyleEverBeenFetched)
            {
                this.UpdateThemeStyleProperty();
            }
            this.RaiseInitialized(FrameworkElement.InitializedKey, e);
        }

        private void TryFireInitialized()
        {
            if (!this.ReadInternalFlag(InternalFlags.InitPending) && !this.ReadInternalFlag(InternalFlags.IsInitialized))
            {
                this.WriteInternalFlag(InternalFlags.IsInitialized, true);
                this.PrivateInitialized();
                this.OnInitialized(EventArgs.Empty);
            }
        }

        private void RaiseInitialized(EventPrivateKey key, EventArgs e)
        {
            EventHandlersStore eventHandlersStore = base.EventHandlersStore;
            if (eventHandlersStore != null)
            {
                Delegate @delegate = eventHandlersStore.Get(key);
                if (@delegate != null)
                {
                    ((EventHandler)@delegate)(this, e);
                }
            }
        }

        private static void NumberSubstitutionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((FrameworkElement)o).HasNumberSubstitutionChanged = true;
        }

        private static bool ShouldUseSystemFont(FrameworkElement fe, DependencyProperty dp)
        {
            bool flag;
            return (SystemResources.SystemResourcesAreChanging || (fe.ReadInternalFlag(InternalFlags.CreatingRoot) && SystemResources.SystemResourcesHaveChanged)) && fe._parent == null && VisualTreeHelper.GetParent(fe) == null && fe.GetValueSource(dp, null, out flag) == BaseValueSourceInternal.Default;
        }

        private static object CoerceFontFamily(DependencyObject o, object value)
        {
            if (FrameworkElement.ShouldUseSystemFont((FrameworkElement)o, TextElement.FontFamilyProperty))
            {
                return SystemFonts.MessageFontFamily;
            }
            return value;
        }

        private static object CoerceFontSize(DependencyObject o, object value)
        {
            if (FrameworkElement.ShouldUseSystemFont((FrameworkElement)o, TextElement.FontSizeProperty))
            {
                return SystemFonts.MessageFontSize;
            }
            return value;
        }

        private static object CoerceFontStyle(DependencyObject o, object value)
        {
            if (FrameworkElement.ShouldUseSystemFont((FrameworkElement)o, TextElement.FontStyleProperty))
            {
                return SystemFonts.MessageFontStyle;
            }
            return value;
        }

        private static object CoerceFontWeight(DependencyObject o, object value)
        {
            if (FrameworkElement.ShouldUseSystemFont((FrameworkElement)o, TextElement.FontWeightProperty))
            {
                return SystemFonts.MessageFontWeight;
            }
            return value;
        }

        internal sealed override void OnPresentationSourceChanged(bool attached)
        {
            base.OnPresentationSourceChanged(attached);
            if (attached)
            {
                this.FireLoadedOnDescendentsInternal();
                if (SystemResources.SystemResourcesHaveChanged)
                {
                    this.WriteInternalFlag(InternalFlags.CreatingRoot, true);
                    base.CoerceValue(TextElement.FontFamilyProperty);
                    base.CoerceValue(TextElement.FontSizeProperty);
                    base.CoerceValue(TextElement.FontStyleProperty);
                    base.CoerceValue(TextElement.FontWeightProperty);
                    this.WriteInternalFlag(InternalFlags.CreatingRoot, false);
                    return;
                }
            }
            else
            {
                this.FireUnloadedOnDescendentsInternal();
            }
        }

        internal override void OnAddHandler(RoutedEvent routedEvent, Delegate handler)
        {
            base.OnAddHandler(routedEvent, handler);
            if (routedEvent == FrameworkElement.LoadedEvent || routedEvent == FrameworkElement.UnloadedEvent)
            {
                BroadcastEventHelper.AddHasLoadedChangeHandlerFlagInAncestry(this);
            }
        }

        internal override void OnRemoveHandler(RoutedEvent routedEvent, Delegate handler)
        {
            base.OnRemoveHandler(routedEvent, handler);
            if (routedEvent != FrameworkElement.LoadedEvent && routedEvent != FrameworkElement.UnloadedEvent)
            {
                return;
            }
            if (!this.ThisHasLoadedChangeEventHandler)
            {
                BroadcastEventHelper.RemoveHasLoadedChangeHandlerFlagInAncestry(this);
            }
        }

        internal void OnLoaded(RoutedEventArgs args)
        {
            base.RaiseEvent(args);
        }

        internal void OnUnloaded(RoutedEventArgs args)
        {
            base.RaiseEvent(args);
        }

        internal override void AddSynchronizedInputPreOpportunityHandlerCore(EventRoute route, RoutedEventArgs args)
        {
            UIElement uIElement = this._templatedParent as UIElement;
            if (uIElement != null)
            {
                uIElement.AddSynchronizedInputPreOpportunityHandler(route, args);
            }
        }

        internal void RaiseClrEvent(EventPrivateKey key, EventArgs args)
        {
            EventHandlersStore eventHandlersStore = base.EventHandlersStore;
            if (eventHandlersStore != null)
            {
                Delegate @delegate = eventHandlersStore.Get(key);
                if (@delegate != null)
                {
                    ((EventHandler)@delegate)(this, args);
                }
            }
        }

        private static FrameworkElement.FrameworkServices EnsureFrameworkServices()
        {
            if (FrameworkElement._frameworkServices == null)
            {
                FrameworkElement._frameworkServices = new FrameworkElement.FrameworkServices();
            }
            return FrameworkElement._frameworkServices;
        }

        private static void OnToolTipOpeningThunk(object sender, ToolTipEventArgs e)
        {
            ((FrameworkElement)sender).OnToolTipOpening(e);
        }

        /// <summary> Invoked whenever the <see cref="E:System.Windows.FrameworkElement.ToolTipOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
        /// <param name="e">Provides data about the event.</param>
        protected virtual void OnToolTipOpening(ToolTipEventArgs e)
        {
        }

        private static void OnToolTipClosingThunk(object sender, ToolTipEventArgs e)
        {
            ((FrameworkElement)sender).OnToolTipClosing(e);
        }

        /// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ToolTipClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
        /// <param name="e">Provides data about the event.</param>
        protected virtual void OnToolTipClosing(ToolTipEventArgs e)
        {
        }

        private static void OnContextMenuOpeningThunk(object sender, ContextMenuEventArgs e)
        {
            ((FrameworkElement)sender).OnContextMenuOpening(e);
        }

        /// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs" /> that contains the event data.</param>
        protected virtual void OnContextMenuOpening(ContextMenuEventArgs e)
        {
        }

        private static void OnContextMenuClosingThunk(object sender, ContextMenuEventArgs e)
        {
            ((FrameworkElement)sender).OnContextMenuClosing(e);
        }

        /// <summary> Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> routed event reaches this class in its route. Implement this method to add class handling for this event. </summary>
        /// <param name="e">Provides data about the event.</param>
        protected virtual void OnContextMenuClosing(ContextMenuEventArgs e)
        {
        }

        private void RaiseDependencyPropertyChanged(EventPrivateKey key, DependencyPropertyChangedEventArgs args)
        {
            EventHandlersStore eventHandlersStore = base.EventHandlersStore;
            if (eventHandlersStore != null)
            {
                Delegate @delegate = eventHandlersStore.Get(key);
                if (@delegate != null)
                {
                    ((DependencyPropertyChangedEventHandler)@delegate)(this, args);
                }
            }
        }

        internal static void AddIntermediateElementsToRoute(DependencyObject mergePoint, EventRoute route, RoutedEventArgs args, DependencyObject modelTreeNode)
        {
            while (modelTreeNode != null && modelTreeNode != mergePoint)
            {
                UIElement uIElement = modelTreeNode as UIElement;
                ContentElement contentElement = modelTreeNode as ContentElement;
                UIElement3D uIElement3D = modelTreeNode as UIElement3D;
                if (uIElement != null)
                {
                    uIElement.AddToEventRoute(route, args);
                    FrameworkElement frameworkElement = uIElement as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        FrameworkElement.AddStyleHandlersToEventRoute(frameworkElement, null, route, args);
                    }
                }
                else if (contentElement != null)
                {
                    contentElement.AddToEventRoute(route, args);
                    FrameworkContentElement frameworkContentElement = contentElement as FrameworkContentElement;
                    if (frameworkContentElement != null)
                    {
                        FrameworkElement.AddStyleHandlersToEventRoute(null, frameworkContentElement, route, args);
                    }
                }
                else if (uIElement3D != null)
                {
                    uIElement3D.AddToEventRoute(route, args);
                }
                modelTreeNode = LogicalTreeHelper.GetParent(modelTreeNode);
            }
        }

        private bool IsLogicalDescendent(DependencyObject child)
        {
            while (child != null)
            {
                if (child == this)
                {
                    return true;
                }
                child = LogicalTreeHelper.GetParent(child);
            }
            return false;
        }

        internal void EventHandlersStoreAdd(EventPrivateKey key, Delegate handler)
        {
            base.EnsureEventHandlersStore();
            base.EventHandlersStore.Add(key, handler);
        }

        internal void EventHandlersStoreRemove(EventPrivateKey key, Delegate handler)
        {
            EventHandlersStore eventHandlersStore = base.EventHandlersStore;
            if (eventHandlersStore != null)
            {
                eventHandlersStore.Remove(key, handler);
            }
        }

        internal bool ReadInternalFlag(InternalFlags reqFlag)
        {
            return (this._flags & reqFlag) > (InternalFlags)0u;
        }

        internal bool ReadInternalFlag2(InternalFlags2 reqFlag)
        {
            return (this._flags2 & reqFlag) > (InternalFlags2)0u;
        }

        internal void WriteInternalFlag(InternalFlags reqFlag, bool set)
        {
            if (set)
            {
                this._flags |= reqFlag;
                return;
            }
            this._flags &= ~reqFlag;
        }

        internal void WriteInternalFlag2(InternalFlags2 reqFlag, bool set)
        {
            if (set)
            {
                this._flags2 |= reqFlag;
                return;
            }
            this._flags2 &= ~reqFlag;
        }

        /// <summary>Provides an accessor that simplifies access to the <see cref="T:System.Windows.NameScope" /> registration method.</summary>
        /// <param name="name">Name to use for the specified name-object mapping.</param>
        /// <param name="scopedElement">Object for the mapping.</param>
        public void RegisterName(string name, object scopedElement)
        {
            INameScope nameScope = FrameworkElement.FindScope(this);
            if (nameScope != null)
            {
                nameScope.RegisterName(name, scopedElement);
                return;
            }
            throw new InvalidOperationException(SR.Get("NameScopeNotFound", new object[]
			{
				name,
				"register"
			}));
        }

        /// <summary>Simplifies access to the <see cref="T:System.Windows.NameScope" /> de-registration method.</summary>
        /// <param name="name">Name of the name-object pair to remove from the current scope.</param>
        public void UnregisterName(string name)
        {
            INameScope nameScope = FrameworkElement.FindScope(this);
            if (nameScope != null)
            {
                nameScope.UnregisterName(name);
                return;
            }
            throw new InvalidOperationException(SR.Get("NameScopeNotFound", new object[]
			{
				name,
				"unregister"
			}));
        }

        /// <summary>Finds an element that has the provided identifier name. </summary>
        /// <param name="name">The name of the requested element.</param>
        /// <returns>The requested element. This can be null if no matching element was found.</returns>
        public object FindName(string name)
        {
            DependencyObject dependencyObject;
            return this.FindName(name, out dependencyObject);
        }

        internal object FindName(string name, out DependencyObject scopeOwner)
        {
            INameScope nameScope = FrameworkElement.FindScope(this, out scopeOwner);
            if (nameScope != null)
            {
                return nameScope.FindName(name);
            }
            return null;
        }

        /// <summary>Reapplies the default style to the current <see cref="T:System.Windows.FrameworkElement" />.</summary>
        public void UpdateDefaultStyle()
        {
            TreeWalkHelper.InvalidateOnResourcesChange(this, null, ResourcesChangeInfo.ThemeChangeInfo);
        }

        internal object FindResourceOnSelf(object resourceKey, bool allowDeferredResourceReference, bool mustReturnDeferredResourceReference)
        {
            ResourceDictionary value = FrameworkElement.ResourcesField.GetValue(this);
            if (value != null && value.Contains(resourceKey))
            {
                bool flag;
                return value.FetchResource(resourceKey, allowDeferredResourceReference, mustReturnDeferredResourceReference, out flag);
            }
            return DependencyProperty.UnsetValue;
        }

        internal DependencyObject ContextVerifiedGetParent()
        {
            return this._parent;
        }

        /// <summary>Adds the provided object to the logical tree of this element. </summary>
        /// <param name="child">Child element to be added.</param>
        protected internal void AddLogicalChild(object child)
        {
            if (child != null)
            {
                if (this.IsLogicalChildrenIterationInProgress)
                {
                    throw new InvalidOperationException(SR.Get("CannotModifyLogicalChildrenDuringTreeWalk"));
                }
                this.TryFireInitialized();
                try
                {
                    this.HasLogicalChildren = true;
                    FrameworkObject frameworkObject = new FrameworkObject(child as DependencyObject);
                    frameworkObject.ChangeLogicalParent(this);
                }
                finally
                {
                }
            }
        }

        /// <summary>Removes the provided object from this element's logical tree. <see cref="T:System.Windows.FrameworkElement" /> updates the affected logical tree parent pointers to keep in sync with this deletion.</summary>
        /// <param name="child">The element to remove.</param>
        protected internal void RemoveLogicalChild(object child)
        {
            if (child != null)
            {
                if (this.IsLogicalChildrenIterationInProgress)
                {
                    throw new InvalidOperationException(SR.Get("CannotModifyLogicalChildrenDuringTreeWalk"));
                }
                FrameworkObject frameworkObject = new FrameworkObject(child as DependencyObject);
                if (frameworkObject.Parent == this)
                {
                    frameworkObject.ChangeLogicalParent(null);
                }
                IEnumerator logicalChildren = this.LogicalChildren;
                if (logicalChildren == null)
                {
                    this.HasLogicalChildren = false;
                    return;
                }
                this.HasLogicalChildren = logicalChildren.MoveNext();
            }
        }

        internal void ChangeLogicalParent(DependencyObject newParent)
        {
            base.VerifyAccess();
            if (newParent != null)
            {
                newParent.VerifyAccess();
            }
            if (this._parent != null && newParent != null && this._parent != newParent)
            {
                throw new InvalidOperationException(SR.Get("HasLogicalParent"));
            }
            if (newParent == this)
            {
                throw new InvalidOperationException(SR.Get("CannotBeSelfParent"));
            }
            if (newParent != null)
            {
                this.ClearInheritanceContext();
            }
            this.IsParentAnFE = (newParent is FrameworkElement);
            DependencyObject parent = this._parent;
            this.OnNewParent(newParent);
            BroadcastEventHelper.AddOrRemoveHasLoadedChangeHandlerFlag(this, parent, newParent);
            DependencyObject parent2 = (newParent != null) ? newParent : parent;
            TreeWalkHelper.InvalidateOnTreeChange(this, null, parent2, newParent != null);
            this.TryFireInitialized();
        }

        internal virtual void OnNewParent(DependencyObject newParent)
        {
            DependencyObject parent = this._parent;
            this._parent = newParent;
            if (this._parent != null && this._parent is ContentElement)
            {
                UIElement.SynchronizeForceInheritProperties(this, null, null, this._parent);
            }
            else if (parent is ContentElement)
            {
                UIElement.SynchronizeForceInheritProperties(this, null, null, parent);
            }
            base.SynchronizeReverseInheritPropertyFlags(parent, false);
        }

        [SecurityCritical, SecurityTreatAsSafe]
        internal void OnAncestorChangedInternal(TreeChangeInfo parentTreeState)
        {
            bool isSelfInheritanceParent = base.IsSelfInheritanceParent;
            if (parentTreeState.Root != this)
            {
                this.HasStyleChanged = false;
                this.HasStyleInvalidated = false;
                this.HasTemplateChanged = false;
            }
            if (parentTreeState.IsAddOperation)
            {
                FrameworkObject frameworkObject = new FrameworkObject(this, null);
                frameworkObject.SetShouldLookupImplicitStyles();
            }
            if (this.HasResourceReference)
            {
                TreeWalkHelper.OnResourcesChanged(this, ResourcesChangeInfo.TreeChangeInfo, false);
            }
            FrugalObjectList<DependencyProperty> item = this.InvalidateTreeDependentProperties(parentTreeState, isSelfInheritanceParent);
            parentTreeState.InheritablePropertiesStack.Push(item);
            this.OnAncestorChanged();
            if (this.PotentiallyHasMentees)
            {
                this.RaiseClrEvent(FrameworkElement.ResourcesChangedKey, EventArgs.Empty);
            }
        }

        internal FrugalObjectList<DependencyProperty> InvalidateTreeDependentProperties(TreeChangeInfo parentTreeState, bool isSelfInheritanceParent)
        {
            this.AncestorChangeInProgress = true;
            this.InVisibilityCollapsedTree = false;
            if (parentTreeState.TopmostCollapsedParentNode == null)
            {
                if (base.Visibility == Visibility.Collapsed)
                {
                    parentTreeState.TopmostCollapsedParentNode = this;
                    this.InVisibilityCollapsedTree = true;
                }
            }
            else
            {
                this.InVisibilityCollapsedTree = true;
            }
            FrugalObjectList<DependencyProperty> result;
            try
            {
                if (this.IsInitialized && !this.HasLocalStyle && this != parentTreeState.Root)
                {
                    this.UpdateStyleProperty();
                }
                ChildRecord childRecord = default(ChildRecord);
                bool isChildRecordValid = false;
                Style style = this.Style;
                Style themeStyle = this.ThemeStyle;
                DependencyObject arg_92_0 = this.TemplatedParent;
                int templateChildIndex = this.TemplateChildIndex;
                bool hasStyleChanged = this.HasStyleChanged;
                FrameworkElement.GetTemplatedParentChildRecord(arg_92_0, templateChildIndex, out childRecord, out isChildRecordValid);
                FrameworkElement frameworkElement;
                FrameworkContentElement frameworkContentElement;
                bool arg_A7_0 = FrameworkElement.GetFrameworkParent(this, out frameworkElement, out frameworkContentElement);
                DependencyObject parent = null;
                InheritanceBehavior inheritanceBehavior = InheritanceBehavior.Default;
                if (arg_A7_0)
                {
                    if (frameworkElement != null)
                    {
                        parent = frameworkElement;
                        inheritanceBehavior = frameworkElement.InheritanceBehavior;
                    }
                    else
                    {
                        parent = frameworkContentElement;
                        inheritanceBehavior = frameworkContentElement.InheritanceBehavior;
                    }
                }
                if (!TreeWalkHelper.SkipNext(this.InheritanceBehavior) && !TreeWalkHelper.SkipNow(inheritanceBehavior))
                {
                    base.SynchronizeInheritanceParent(parent);
                }
                else if (!base.IsSelfInheritanceParent)
                {
                    base.SetIsSelfInheritanceParent();
                }
                result = TreeWalkHelper.InvalidateTreeDependentProperties(parentTreeState, this, null, style, themeStyle, ref childRecord, isChildRecordValid, hasStyleChanged, isSelfInheritanceParent);
            }
            finally
            {
                this.AncestorChangeInProgress = false;
                this.InVisibilityCollapsedTree = false;
            }
            return result;
        }

        internal void UpdateStyleProperty()
        {
            if (!this.HasStyleInvalidated)
            {
                if (!this.IsStyleUpdateInProgress)
                {
                    this.IsStyleUpdateInProgress = true;
                    try
                    {
                        base.InvalidateProperty(FrameworkElement.StyleProperty);
                        this.HasStyleInvalidated = true;
                        return;
                    }
                    finally
                    {
                        this.IsStyleUpdateInProgress = false;
                    }
                }
                throw new InvalidOperationException(SR.Get("CyclicStyleReferenceDetected", new object[]
				{
					this
				}));
            }
        }

        internal void UpdateThemeStyleProperty()
        {
            if (!this.IsThemeStyleUpdateInProgress)
            {
                this.IsThemeStyleUpdateInProgress = true;
                try
                {
                    StyleHelper.GetThemeStyle(this, null);
                    ContextMenu contextMenu = base.GetValueEntry(base.LookupEntry(FrameworkElement.ContextMenuProperty.GlobalIndex), FrameworkElement.ContextMenuProperty, null, RequestFlags.DeferredReferences).Value as ContextMenu;
                    if (contextMenu != null)
                    {
                        TreeWalkHelper.InvalidateOnResourcesChange(contextMenu, null, ResourcesChangeInfo.ThemeChangeInfo);
                    }
                    DependencyObject dependencyObject = base.GetValueEntry(base.LookupEntry(FrameworkElement.ToolTipProperty.GlobalIndex), FrameworkElement.ToolTipProperty, null, RequestFlags.DeferredReferences).Value as DependencyObject;
                    if (dependencyObject != null)
                    {
                        FrameworkObject frameworkObject = new FrameworkObject(dependencyObject);
                        if (frameworkObject.IsValid)
                        {
                            TreeWalkHelper.InvalidateOnResourcesChange(frameworkObject.FE, frameworkObject.FCE, ResourcesChangeInfo.ThemeChangeInfo);
                        }
                    }
                    this.OnThemeChanged();
                    return;
                }
                finally
                {
                    this.IsThemeStyleUpdateInProgress = false;
                }
            }
            throw new InvalidOperationException(SR.Get("CyclicThemeStyleReferenceDetected", new object[]
			{
				this
			}));
        }

        internal virtual void OnThemeChanged()
        {
        }

        internal void FireLoadedOnDescendentsInternal()
        {
            if (this.LoadedPending == null)
            {
                DependencyObject parent = this.Parent;
                if (parent == null)
                {
                    parent = VisualTreeHelper.GetParent(this);
                }
                object[] unloadedPending = this.UnloadedPending;
                if (unloadedPending == null || unloadedPending[2] != parent)
                {
                    BroadcastEventHelper.AddLoadedCallback(this, parent);
                    return;
                }
                BroadcastEventHelper.RemoveUnloadedCallback(this, unloadedPending);
            }
        }

        internal void FireUnloadedOnDescendentsInternal()
        {
            if (this.UnloadedPending == null)
            {
                DependencyObject parent = this.Parent;
                if (parent == null)
                {
                    parent = VisualTreeHelper.GetParent(this);
                }
                object[] loadedPending = this.LoadedPending;
                if (loadedPending == null)
                {
                    BroadcastEventHelper.AddUnloadedCallback(this, parent);
                    return;
                }
                BroadcastEventHelper.RemoveLoadedCallback(this, loadedPending);
            }
        }

        internal override bool ShouldProvideInheritanceContext(DependencyObject target, DependencyProperty property)
        {
            FrameworkObject frameworkObject = new FrameworkObject(target);
            return !frameworkObject.IsValid;
        }

        internal override void AddInheritanceContext(DependencyObject context, DependencyProperty property)
        {
            base.AddInheritanceContext(context, property);
            this.TryFireInitialized();
            if ((property == VisualBrush.VisualProperty || property == BitmapCacheBrush.TargetProperty) && FrameworkElement.GetFrameworkParent(this) == null && !FrameworkObject.IsEffectiveAncestor(this, context))
            {
                if (!this.HasMultipleInheritanceContexts && this.InheritanceContext == null)
                {
                    FrameworkElement.InheritanceContextField.SetValue(this, context);
                    base.OnInheritanceContextChanged(EventArgs.Empty);
                    return;
                }
                if (this.InheritanceContext != null)
                {
                    FrameworkElement.InheritanceContextField.ClearValue(this);
                    this.WriteInternalFlag2(InternalFlags2.HasMultipleInheritanceContexts, true);
                    base.OnInheritanceContextChanged(EventArgs.Empty);
                }
            }
        }

        internal override void RemoveInheritanceContext(DependencyObject context, DependencyProperty property)
        {
            if (this.InheritanceContext == context)
            {
                FrameworkElement.InheritanceContextField.ClearValue(this);
                base.OnInheritanceContextChanged(EventArgs.Empty);
            }
            base.RemoveInheritanceContext(context, property);
        }

        private void ClearInheritanceContext()
        {
            if (this.InheritanceContext != null)
            {
                FrameworkElement.InheritanceContextField.ClearValue(this);
                base.OnInheritanceContextChanged(EventArgs.Empty);
            }
        }

        internal override void OnInheritanceContextChangedCore(EventArgs args)
        {
            DependencyObject value = FrameworkElement.MentorField.GetValue(this);
            DependencyObject dependencyObject = Helper.FindMentor(this.InheritanceContext);
            if (value != dependencyObject)
            {
                FrameworkElement.MentorField.SetValue(this, dependencyObject);
                if (value != null)
                {
                    this.DisconnectMentor(value);
                }
                if (dependencyObject != null)
                {
                    this.ConnectMentor(dependencyObject);
                }
            }
        }

        private void ConnectMentor(DependencyObject mentor)
        {
            FrameworkObject frameworkObject = new FrameworkObject(mentor);
            frameworkObject.InheritedPropertyChanged += new InheritedPropertyChangedEventHandler(this.OnMentorInheritedPropertyChanged);
            frameworkObject.ResourcesChanged += new EventHandler(this.OnMentorResourcesChanged);
            TreeWalkHelper.InvalidateOnTreeChange(this, null, frameworkObject.DO, true);
            if (this.SubtreeHasLoadedChangeHandler)
            {
                bool isLoaded = frameworkObject.IsLoaded;
                this.ConnectLoadedEvents(ref frameworkObject, isLoaded);
                if (isLoaded)
                {
                    this.FireLoadedOnDescendentsInternal();
                }
            }
        }

        private void DisconnectMentor(DependencyObject mentor)
        {
            FrameworkObject frameworkObject = new FrameworkObject(mentor);
            frameworkObject.InheritedPropertyChanged -= new InheritedPropertyChangedEventHandler(this.OnMentorInheritedPropertyChanged);
            frameworkObject.ResourcesChanged -= new EventHandler(this.OnMentorResourcesChanged);
            TreeWalkHelper.InvalidateOnTreeChange(this, null, frameworkObject.DO, false);
            if (this.SubtreeHasLoadedChangeHandler)
            {
                bool isLoaded = frameworkObject.IsLoaded;
                this.DisconnectLoadedEvents(ref frameworkObject, isLoaded);
                if (frameworkObject.IsLoaded)
                {
                    this.FireUnloadedOnDescendentsInternal();
                }
            }
        }

        internal void ChangeSubtreeHasLoadedChangedHandler(DependencyObject mentor)
        {
            FrameworkObject frameworkObject = new FrameworkObject(mentor);
            bool isLoaded = frameworkObject.IsLoaded;
            if (this.SubtreeHasLoadedChangeHandler)
            {
                this.ConnectLoadedEvents(ref frameworkObject, isLoaded);
                return;
            }
            this.DisconnectLoadedEvents(ref frameworkObject, isLoaded);
        }

        private void OnMentorLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkObject frameworkObject = new FrameworkObject((DependencyObject)sender);
            frameworkObject.Loaded -= new RoutedEventHandler(this.OnMentorLoaded);
            frameworkObject.Unloaded += new RoutedEventHandler(this.OnMentorUnloaded);
            BroadcastEventHelper.BroadcastLoadedSynchronously(this, this.IsLoaded);
        }

        private void OnMentorUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkObject frameworkObject = new FrameworkObject((DependencyObject)sender);
            frameworkObject.Unloaded -= new RoutedEventHandler(this.OnMentorUnloaded);
            frameworkObject.Loaded += new RoutedEventHandler(this.OnMentorLoaded);
            BroadcastEventHelper.BroadcastUnloadedSynchronously(this, this.IsLoaded);
        }

        private void ConnectLoadedEvents(ref FrameworkObject foMentor, bool isLoaded)
        {
            if (foMentor.IsValid)
            {
                if (isLoaded)
                {
                    foMentor.Unloaded += new RoutedEventHandler(this.OnMentorUnloaded);
                    return;
                }
                foMentor.Loaded += new RoutedEventHandler(this.OnMentorLoaded);
            }
        }

        private void DisconnectLoadedEvents(ref FrameworkObject foMentor, bool isLoaded)
        {
            if (foMentor.IsValid)
            {
                if (isLoaded)
                {
                    foMentor.Unloaded -= new RoutedEventHandler(this.OnMentorUnloaded);
                    return;
                }
                foMentor.Loaded -= new RoutedEventHandler(this.OnMentorLoaded);
            }
        }

        private void OnMentorInheritedPropertyChanged(object sender, InheritedPropertyChangedEventArgs e)
        {
            TreeWalkHelper.InvalidateOnInheritablePropertyChange(this, null, e.Info, false);
        }

        private void OnMentorResourcesChanged(object sender, EventArgs e)
        {
            TreeWalkHelper.InvalidateOnResourcesChange(this, null, ResourcesChangeInfo.CatastrophicDictionaryChangeInfo);
        }

        internal void RaiseInheritedPropertyChangedEvent(ref InheritablePropertyChangeInfo info)
        {
            EventHandlersStore eventHandlersStore = base.EventHandlersStore;
            if (eventHandlersStore != null)
            {
                Delegate @delegate = eventHandlersStore.Get(FrameworkElement.InheritedPropertyChangedKey);
                if (@delegate != null)
                {
                    InheritedPropertyChangedEventArgs e = new InheritedPropertyChangedEventArgs(ref info);
                    ((InheritedPropertyChangedEventHandler)@delegate)(this, e);
                }
            }
        }
    }
}
