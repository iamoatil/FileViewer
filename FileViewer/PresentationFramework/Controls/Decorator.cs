using MS.Internal.Controls;
using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows.Controls
{
	/// <summary>Provides a base class for elements that apply effects onto or around a single child element, such as <see cref="T:System.Windows.Controls.Border" /> or <see cref="T:System.Windows.Controls.Viewbox" />.</summary>
	[Localizability(LocalizationCategory.Ignore, Readability = Readability.Unreadable), ContentProperty("Child")]
	public class Decorator : FrameworkElement, IAddChild
	{
		private UIElement _child;

		/// <summary>Gets or sets the single child element of a <see cref="T:System.Windows.Controls.Decorator" />.</summary>
		/// <returns>The single child element of a <see cref="T:System.Windows.Controls.Decorator" />.</returns>
		[DefaultValue(null)]
		public virtual UIElement Child
		{
			get
			{
				return this._child;
			}
			set
			{
				if (this._child != value)
				{
					base.RemoveVisualChild(this._child);
					base.RemoveLogicalChild(this._child);
					this._child = value;
					base.AddLogicalChild(value);
					base.AddVisualChild(value);
					base.InvalidateMeasure();
				}
			}
		}

		/// <summary>Gets an enumerator that can be used to iterate the logical child elements of a <see cref="T:System.Windows.Controls.Decorator" />.</summary>
		/// <returns>An enumerator that can be used to iterate the logical child elements of a <see cref="T:System.Windows.Controls.Decorator" />.</returns>
		protected internal override IEnumerator LogicalChildren
		{
			get
			{
				if (this._child == null)
				{
					return EmptyEnumerator.Instance;
				}
				return new SingleChildEnumerator(this._child);
			}
		}

		/// <summary>Gets a value that is equal to the number of visual child elements of this instance of <see cref="T:System.Windows.Controls.Decorator" />.</summary>
		/// <returns>The number of visual child elements.</returns>
		protected override int VisualChildrenCount
		{
			get
			{
				if (this._child != null)
				{
					return 1;
				}
				return 0;
			}
		}

		internal UIElement IntChild
		{
			get
			{
				return this._child;
			}
			set
			{
				this._child = value;
			}
		}

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		/// <param name="value"> An object to add as a child.</param>
		void IAddChild.AddChild(object value)
		{
			if (!(value is UIElement))
			{
				throw new ArgumentException(SR.Get("UnexpectedParameterType", new object[]
				{
					value.GetType(),
					typeof(UIElement)
				}), "value");
			}
			if (this.Child != null)
			{
				throw new ArgumentException(SR.Get("CanOnlyHaveOneChild", new object[]
				{
					base.GetType(),
					value.GetType()
				}));
			}
			this.Child = (UIElement)value;
		}

		/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
		/// <param name="text"> A string to add to the object.</param>
		void IAddChild.AddText(string text)
		{
			XamlSerializerUtil.ThrowIfNonWhiteSpaceInAddText(text, this);
		}

		/// <summary>Gets the child <see cref="T:System.Windows.Media.Visual" /> element at the specified <paramref name="index" /> position.</summary>
		/// <param name="index">Index position of the child element.</param>
		/// <returns>The child element at the specified <paramref name="index" /> position.</returns>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is greater than the number of visual child elements.</exception>
		protected override Visual GetVisualChild(int index)
		{
			if (this._child == null || index != 0)
			{
				throw new ArgumentOutOfRangeException("index", index, SR.Get("Visual_ArgumentOutOfRange"));
			}
			return this._child;
		}

		/// <summary>Measures the child element of a <see cref="T:System.Windows.Controls.Decorator" /> to prepare for arranging it during the <see cref="M:System.Windows.Controls.Decorator.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
		/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
		/// <returns>The target <see cref="T:System.Windows.Size" /> of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			UIElement child = this.Child;
			if (child != null)
			{
				child.Measure(constraint);
				return child.DesiredSize;
			}
			return default(Size);
		}

		/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.Decorator" /> element.</summary>
		/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> this element uses to arrange its child content.</param>
		/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.Decorator" /> element and its child.</returns>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			UIElement child = this.Child;
			if (child != null)
			{
				child.Arrange(new Rect(arrangeSize));
			}
			return arrangeSize;
		}
	}
}
