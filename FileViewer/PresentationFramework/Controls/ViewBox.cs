using MS.Internal;
using MS.Internal.Controls;
using System;
using System.Collections;
using System.Windows.Media;

namespace System.Windows.Controls
{
    /// <summary>Defines a content decorator that can stretch and scale a single child to fill the available space.</summary>
    public class Viewbox : Decorator
    {
        /// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewbox.Stretch" /> dependency property.</returns>
        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register("Stretch", typeof(Stretch), typeof(Viewbox), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(Viewbox.ValidateStretchValue));

        /// <summary>Identifies the <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" /> dependency property.</summary>
        /// <returns>The identifier for the <see cref="P:System.Windows.Controls.Viewbox.StretchDirection" /> dependency property.</returns>
        public static readonly DependencyProperty StretchDirectionProperty = DependencyProperty.Register("StretchDirection", typeof(StretchDirection), typeof(Viewbox), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(Viewbox.ValidateStretchDirectionValue));

        private ContainerVisual _internalVisual;

        private ContainerVisual InternalVisual
        {
            get
            {
                if (this._internalVisual == null)
                {
                    this._internalVisual = new ContainerVisual();
                    base.AddVisualChild(this._internalVisual);
                }
                return this._internalVisual;
            }
        }

        private UIElement InternalChild
        {
            get
            {
                VisualCollection children = this.InternalVisual.Children;
                if (children.Count != 0)
                {
                    return children[0] as UIElement;
                }
                return null;
            }
            set
            {
                VisualCollection children = this.InternalVisual.Children;
                if (children.Count != 0)
                {
                    children.Clear();
                }
                children.Add(value);
            }
        }

        private Transform InternalTransform
        {
            get
            {
                return this.InternalVisual.Transform;
            }
            set
            {
                this.InternalVisual.Transform = value;
            }
        }

        /// <summary>Gets or sets the single child of a <see cref="T:System.Windows.Controls.Viewbox" /> element. </summary>
        /// <returns>The single child of a <see cref="T:System.Windows.Controls.Viewbox" /> element. This property has no default value.</returns>
        public override UIElement Child
        {
            get
            {
                return this.InternalChild;
            }
            set
            {
                UIElement internalChild = this.InternalChild;
                if (internalChild != value)
                {
                    base.RemoveLogicalChild(internalChild);
                    if (value != null)
                    {
                        base.AddLogicalChild(value);
                    }
                    this.InternalChild = value;
                    base.InvalidateMeasure();
                }
            }
        }

        /// <summary>Gets the number of child <see cref="T:System.Windows.Media.Visual" /> objects in this instance of <see cref="T:System.Windows.Controls.Viewbox" />.</summary>
        /// <returns>The number of <see cref="T:System.Windows.Media.Visual" /> children.</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>Gets an enumerator that can iterate the logical children of this <see cref="T:System.Windows.Controls.Viewbox" /> element.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" />. This property has no default value.</returns>
        protected internal override IEnumerator LogicalChildren
        {
            get
            {
                if (this.InternalChild == null)
                {
                    return EmptyEnumerator.Instance;
                }
                return new SingleChildEnumerator(this.InternalChild);
            }
        }

        /// <summary>Gets or sets the <see cref="T:System.Windows.Controls.Viewbox" /> <see cref="T:System.Windows.Media.Stretch" /> mode, which determines how content fits into the available space.  </summary>
        /// <returns>A <see cref="T:System.Windows.Media.Stretch" /> that determines how content fits in the available space. The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
        public Stretch Stretch
        {
            get
            {
                return (Stretch)base.GetValue(Viewbox.StretchProperty);
            }
            set
            {
                base.SetValue(Viewbox.StretchProperty, value);
            }
        }

        /// <summary>Gets or sets the <see cref="T:System.Windows.Controls.StretchDirection" />, which determines how scaling is applied to the contents of a <see cref="T:System.Windows.Controls.Viewbox" />.  </summary>
        /// <returns>A <see cref="T:System.Windows.Controls.StretchDirection" /> that determines how scaling is applied to the contents of a <see cref="T:System.Windows.Controls.Viewbox" />. The default is <see cref="F:System.Windows.Controls.StretchDirection.Both" />.</returns>
        public StretchDirection StretchDirection
        {
            get
            {
                return (StretchDirection)base.GetValue(Viewbox.StretchDirectionProperty);
            }
            set
            {
                base.SetValue(Viewbox.StretchDirectionProperty, value);
            }
        }

        private static bool ValidateStretchValue(object value)
        {
            Stretch stretch = (Stretch)value;
            return stretch == Stretch.Uniform || stretch == Stretch.None || stretch == Stretch.Fill || stretch == Stretch.UniformToFill;
        }

        private static bool ValidateStretchDirectionValue(object value)
        {
            StretchDirection stretchDirection = (StretchDirection)value;
            return stretchDirection == StretchDirection.Both || stretchDirection == StretchDirection.DownOnly || stretchDirection == StretchDirection.UpOnly;
        }

        /// <summary>Gets a <see cref="T:System.Windows.Media.Visual" /> child at the specified <paramref name="index" /> position.</summary>
        /// <param name="index">The index position of the wanted <see cref="T:System.Windows.Media.Visual" /> child.</param>
        /// <returns>A <see cref="T:System.Windows.Media.Visual" /> child of the parent <see cref="T:System.Windows.Controls.Viewbox" /> element.</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException("index", index, SR.Get("Visual_ArgumentOutOfRange"));
            }
            return this.InternalVisual;
        }

        /// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.Viewbox" /> prior to arranging them during the <see cref="M:System.Windows.Controls.WrapPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
        /// <param name="constraint">A <see cref="T:System.Windows.Size" /> limit that <see cref="T:System.Windows.Controls.Viewbox" /> cannot exceed.</param>
        /// <returns>The <see cref="T:System.Windows.Size" /> that represents the element size you want.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            UIElement internalChild = this.InternalChild;
            Size result = default(Size);
            if (internalChild != null)
            {
                Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
                internalChild.Measure(availableSize);
                Size desiredSize = internalChild.DesiredSize;
                Size size = Viewbox.ComputeScaleFactor(constraint, desiredSize, this.Stretch, this.StretchDirection);
                result.Width = size.Width * desiredSize.Width;
                result.Height = size.Height * desiredSize.Height;
            }
            return result;
        }

        /// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Viewbox" /> element.</summary>
        /// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> this element uses to arrange its child elements.</param>
        /// <returns>
        ///   <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.Viewbox" /> element and its child elements.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement internalChild = this.InternalChild;
            if (internalChild != null)
            {
                Size desiredSize = internalChild.DesiredSize;
                Size size = Viewbox.ComputeScaleFactor(arrangeSize, desiredSize, this.Stretch, this.StretchDirection);
                this.InternalTransform = new ScaleTransform(size.Width, size.Height);
                internalChild.Arrange(new Rect(default(Point), internalChild.DesiredSize));
                arrangeSize.Width = size.Width * desiredSize.Width;
                arrangeSize.Height = size.Height * desiredSize.Height;
            }
            return arrangeSize;
        }

        internal static Size ComputeScaleFactor(Size availableSize, Size contentSize, Stretch stretch, StretchDirection stretchDirection)
        {
            double num = 1.0;
            double num2 = 1.0;
            bool flag = !double.IsPositiveInfinity(availableSize.Width);
            bool flag2 = !double.IsPositiveInfinity(availableSize.Height);
            if ((stretch == Stretch.Uniform || stretch == Stretch.UniformToFill || stretch == Stretch.Fill) && (flag | flag2))
            {
                num = (DoubleUtil.IsZero(contentSize.Width) ? 0.0 : (availableSize.Width / contentSize.Width));
                num2 = (DoubleUtil.IsZero(contentSize.Height) ? 0.0 : (availableSize.Height / contentSize.Height));
                if (!flag)
                {
                    num = num2;
                }
                else if (!flag2)
                {
                    num2 = num;
                }
                else
                {
                    switch (stretch)
                    {
                        case Stretch.Uniform:
                            num2 = (num = ((num < num2) ? num : num2));
                            break;
                        case Stretch.UniformToFill:
                            num2 = (num = ((num > num2) ? num : num2));
                            break;
                    }
                }
                switch (stretchDirection)
                {
                    case StretchDirection.UpOnly:
                        if (num < 1.0)
                        {
                            num = 1.0;
                        }
                        if (num2 < 1.0)
                        {
                            num2 = 1.0;
                        }
                        break;
                    case StretchDirection.DownOnly:
                        if (num > 1.0)
                        {
                            num = 1.0;
                        }
                        if (num2 > 1.0)
                        {
                            num2 = 1.0;
                        }
                        break;
                }
            }
            return new Size(num, num2);
        }
    }
}
