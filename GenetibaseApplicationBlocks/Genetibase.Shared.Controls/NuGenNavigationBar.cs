/* -----------------------------------------------
 * NuGenNavigationBar.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using Genetibase.Shared.ComponentModel;
using Genetibase.Shared.Controls.ComponentModel;
using Genetibase.Shared.Controls.NavigationBarInternals;
using Genetibase.Shared.Windows;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Genetibase.Shared.Controls
{
	/// <summary>
	/// Microsoft Outlook like navigation bar.
	/// </summary>
	[ToolboxItem(false)]
	[System.ComponentModel.DesignerCategory("Code")]
	public class NuGenNavigationBar : NuGenControlBase
	{
		#region Declarations.Fields

		private NuGenNavigationButton _hoveringButton;
		private NuGenNavigationButton _rightClickedButton;

		private bool _canGrow;
		private bool _canShrink;
		private bool _dropDownHovering;
		private bool _isResizing;

		private int _maxLargeButtonCount;
		private int _maxSmallButtonCount;

		#endregion

		#region Events

		private static readonly object _buttonClicked = new object();

		/// <summary>
		/// </summary>
		public event EventHandler ButtonClicked
		{
			add
			{
				this.Events.AddHandler(_buttonClicked, value);
			}
			remove
			{
				this.Events.RemoveHandler(_buttonClicked, value);
			}
		}

		/// <summary>
		/// Will bubble the <see cref="Genetibase.Shared.Controls.NuGenNavigationBar.ButtonClicked"/> event.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnButtonClicked(EventArgs e)
		{
			this.Initiator.InvokeAction(_buttonClicked, e);
		}

		#endregion

		#region Properties.Behavior

		/*
		 * Buttons
		 */

		private NuGenNavigationButtonCollection _buttons;

		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[NuGenSRCategory("Category_Behavior")]
		public NuGenNavigationButtonCollection Buttons
		{
			get
			{
				if (_buttons == null)
				{
					_buttons = new NuGenNavigationButtonCollection(this);
				}

				return _buttons;
			}
		}

		#endregion

		#region Properties.Public

		/*
		 * SelectedButton
		 */

		private NuGenNavigationButton _selectedButton;

		/// <summary>
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public NuGenNavigationButton SelectedButton
		{
			get
			{
				return _selectedButton;
			}
			set
			{
				if (_selectedButton != value)
				{
					_selectedButton = value;
					this.Invalidate();
					this.OnButtonClicked(EventArgs.Empty);
				}
			}
		}

		#endregion

		#region Properties.Services

		/*
		 * LayoutManager
		 */

		private INuGenNavigationBarLayoutManager _layoutManager;

		/// <summary>
		/// </summary>
		/// <exception cref="NuGenServiceNotFoundException"/>
		protected INuGenNavigationBarLayoutManager LayoutManager
		{
			get
			{
				if (_layoutManager == null)
				{
					Debug.Assert(this.ServiceProvider != null, "this.ServiceProvider != null");
					_layoutManager = this.ServiceProvider.GetService<INuGenNavigationBarLayoutManager>();

					if (_layoutManager == null)
					{
						throw new NuGenServiceNotFoundException<INuGenNavigationBarLayoutManager>();
					}
				}

				return _layoutManager;
			}
		}

		/*
		 * Renderer
		 */

		private INuGenNavigationBarRenderer _renderer;

		/// <summary>
		/// </summary>
		/// <exception cref="NuGenServiceNotFoundException"/>
		protected INuGenNavigationBarRenderer Renderer
		{
			get
			{
				if (_renderer == null)
				{
					Debug.Assert(this.ServiceProvider != null, "this.ServiceProvider != null");
					_renderer = this.ServiceProvider.GetService<INuGenNavigationBarRenderer>();

					if (_renderer == null)
					{
						throw new NuGenServiceNotFoundException<INuGenNavigationBarRenderer>();
					}
				}

				return _renderer;
			}
		}

		#endregion

		#region Methods.Protected.Overridden

		/*
		 * OnMouseClick
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseClick"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);

			_rightClickedButton = null;
			NuGenNavigationButton currentButton = this.Buttons[e.Location];

			if (currentButton != null)
			{
				if (e.Button == MouseButtons.Left)
				{
					this.SelectedButton = currentButton;
				}
				else if (e.Button == MouseButtons.Right)
				{
					_rightClickedButton = currentButton;
					this.Invalidate();
				}
			}
			else
			{
				// TODO: Context menu creation here.
			}
		}

		/*
		 * OnMouseDown
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			_isResizing = this.GetGripRectangle().Contains(e.Location);
		}

		/*
		 * OnMouseLeave
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			if (_rightClickedButton == null)
			{
				_hoveringButton = null;
				_dropDownHovering = false;
				this.Invalidate();
			}
		}

		/*
		 * OnMouseMove
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			_hoveringButton = null;
			_dropDownHovering = false;

			int buttonHeight = this.GetButtonHeight();

			if (_isResizing)
			{
				if (e.Y < -buttonHeight && _canGrow)
				{
					this.Height += buttonHeight;
				}
				else if (e.Y > buttonHeight && _canShrink)
				{
					this.Height -= buttonHeight;
				}
			}
			else
			{
				NuGenNavigationButton buttonUnderCursor = null;

				if (this.GetGripRectangle().Contains(e.Location))
				{
					this.Cursor = Cursors.SizeNS;
				}
				else if (this.GetDropDownRectangle().Contains(e.Location))
				{
					this.Cursor = Cursors.Hand;
					_dropDownHovering = true;
					this.Invalidate();

					// TODO: Adjust tooltip.
				}
				else if ((buttonUnderCursor = this.Buttons[e.Location]) != null)
				{
					this.Cursor = Cursors.Hand;
					_hoveringButton = buttonUnderCursor;
					this.Invalidate();

					// TODO: Adjust tooltip.
				}
				else
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		/*
		 * OnMouseUp
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			_isResizing = false;
		}

		/*
		 * OnPaint
		 */

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{

			Graphics g = e.Graphics;
			Rectangle bounds = this.ClientRectangle;
			NuGenControlState currentState = this.StateTracker.GetControlState();

			/* Border */

			NuGenPaintParams borderPaintParams = new NuGenPaintParams(this, g, bounds, currentState);
			this.Renderer.DrawBorder(borderPaintParams);

			/* Grip */

			NuGenPaintParams gripPaintParams = new NuGenPaintParams(this, g, this.GetGripRectangle(), currentState);
			this.Renderer.DrawGrip(gripPaintParams);

			/* Large buttons */

			int syncLargeButtons = 0;
			int iterateLargeButtons;

			for (iterateLargeButtons = 0; iterateLargeButtons < this.Buttons.Count; iterateLargeButtons++)
			{
				NuGenNavigationButton button = this.Buttons[iterateLargeButtons];

				if (button.Visible)
				{
					Rectangle rect = new Rectangle(0, syncLargeButtons * this.GetButtonHeight() + this.GetGripHeight(), this.ClientRectangle.Width, this.GetButtonHeight());
					button.Bounds = rect;
					button.IsLarge = true;
					this.DrawButton(g, button);

					if (syncLargeButtons == _maxLargeButtonCount)
					{
						break;
					}
					
					syncLargeButtons++;
				}
			}

			/* Bottom container */

			NuGenPaintParams bottomContainerPaintParams = new NuGenPaintParams(this, g, this.GetBottomContainerRectangle(), currentState);
			this.Renderer.DrawBackground(bottomContainerPaintParams);

			/* Small buttons */

			int startX = this.ClientRectangle.Width - this.GetDropDownRectangle().Width - _maxSmallButtonCount * this.GetSmallButtonWidth();
			int syncSmallButtons = 0;
			int iterateSmallButtons;

			for (iterateSmallButtons = iterateLargeButtons; iterateSmallButtons < this.Buttons.Count; iterateSmallButtons++)
			{
				if (syncSmallButtons == _maxSmallButtonCount)
				{
					break;
				}

				NuGenNavigationButton button = this.Buttons[iterateSmallButtons];

				if (button.Visible)
				{
					Rectangle rect = new Rectangle(startX + (syncSmallButtons * this.GetSmallButtonWidth()), this.GetBottomContainerRectangle().Y, this.GetSmallButtonWidth(), this.GetBottomContainerRectangle().Height);
					button.Bounds = rect;
					button.IsLarge = false;
					this.DrawButton(g, button);
					syncSmallButtons++;
				}
			}

			for (int i = iterateSmallButtons; i < this.Buttons.Count; i++)
			{
				this.Buttons[i].Bounds = Rectangle.Empty;
			}

			/* DropDown */

			this.DrawDropDown(g);

			/* Bottom container border */

			this.Renderer.DrawBorder(bottomContainerPaintParams);
		}

		/*
		 * SetBoundsCore
		 */

		/// <summary>
		/// Performs the work of setting the specified bounds of this control.
		/// </summary>
		/// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left"></see> property value of the control.</param>
		/// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top"></see> property value of the control.</param>
		/// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width"></see> property value of the control.</param>
		/// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height"></see> property value of the control.</param>
		/// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified"></see> values.</param>
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if ((specified & BoundsSpecified.Height) != BoundsSpecified.None)
			{
				int bottomContainerHeight = this.GetBottomContainerRectangle().Height;
				int buttonHeight = this.GetButtonHeight();
				int gripHeight = this.GetGripRectangle().Height;

				_maxLargeButtonCount = (int)Math.Floor(
					(double)((height - bottomContainerHeight - gripHeight) / buttonHeight)
				);

				int visibleCount = this.Buttons.GetVisibleCount();

				if (visibleCount < _maxLargeButtonCount)
				{
					_maxLargeButtonCount = visibleCount;
				}

				_canShrink = _maxLargeButtonCount != 0;
				_canGrow = _maxLargeButtonCount < visibleCount;

				height = _maxLargeButtonCount * buttonHeight + gripHeight + bottomContainerHeight;

			}

			if ((specified & BoundsSpecified.Width) != BoundsSpecified.None)
			{
				int bottomContainerLeftMargin = this.GetBottomContainerLeftMargin();
				int dropDownWidth = this.GetDropDownRectangle().Width;
				int smallButtonWidth = this.GetSmallButtonWidth();

				_maxSmallButtonCount = (int)Math.Floor(
					(double)((width - dropDownWidth - bottomContainerLeftMargin) / smallButtonWidth)
				);

				int visibleCount = this.Buttons.GetVisibleCount();

				if (visibleCount - _maxLargeButtonCount <= 0)
				{
					_maxSmallButtonCount = 0;
				}

				if (_maxSmallButtonCount > (visibleCount - _maxLargeButtonCount))
				{
					_maxSmallButtonCount = visibleCount - _maxLargeButtonCount;
				}
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}

		#endregion

		#region Methods.Drawing

		/*
		 * DrawButton
		 */

		private void DrawButton(Graphics g, NuGenNavigationButton button)
		{
			Debug.Assert(g != null, "g != null");
			Debug.Assert(button != null, "button != null");

			NuGenControlState buttonState = this.StateTracker.GetControlState();

			if (button == _hoveringButton)
			{
				buttonState = button == _selectedButton ? NuGenControlState.Pressed : NuGenControlState.Hot;
			}
			else
			{
				if (button == _selectedButton)
				{
					buttonState = NuGenControlState.Pressed;
				}
			}

			this.Renderer.DrawBackground(new NuGenPaintParams(this, g, button.Bounds, buttonState));

			if (button.IsLarge)
			{
				NuGenItemPaintParams paintParams = new NuGenItemPaintParams(this, g, button.Bounds, buttonState);
				paintParams.ContentAlign = ContentAlignment.MiddleLeft;
				paintParams.Font = this.Font;
				paintParams.ForeColor = this.ForeColor;
				paintParams.Image = button.Image;
				paintParams.Text = button.Text;

				this.Renderer.DrawLargeButtonBody(paintParams);
				this.Renderer.DrawButtonBorder(new NuGenPaintParams(this, g, button.Bounds, buttonState));
			}
			else
			{
				this.Renderer.DrawSmallButtonBody(new NuGenImagePaintParams(this, g, button.Bounds, buttonState, button.Image));
			}
		}

		/*
		 * DrawDropDown
		 */

		private void DrawDropDown(Graphics g)
		{
			Debug.Assert(g != null, "g != null");
			NuGenControlState state = this.StateTracker.GetControlState();

			if (_dropDownHovering)
			{
				state = NuGenControlState.Hot;
			}

			NuGenPaintParams dropDownPaintParams = new NuGenPaintParams(this, g, this.GetDropDownRectangle(), state);

			this.Renderer.DrawBackground(dropDownPaintParams);
			this.Renderer.DrawDropDownArrow(dropDownPaintParams);
		}

		#endregion

		#region Methods.Layout

		/*
		 * GetBottomContainerLeftMargin
		 */

		private int GetBottomContainerLeftMargin()
		{
			return this.LayoutManager.GetBottomContainerLeftMargin();
		}

		/*
		 * GetBottomContainerRectangle
		 */

		private Rectangle GetBottomContainerRectangle()
		{
			int buttonHeight = this.GetButtonHeight();

			return new Rectangle(
				0,
				this.ClientRectangle.Bottom - buttonHeight,
				this.ClientRectangle.Width,
				buttonHeight
			);
		}

		/*
		 * GetButtonHeight
		 */

		private int GetButtonHeight()
		{
			return this.LayoutManager.GetButtonHeight();
		}

		/*
		 * GetDropDownRectangle
		 */

		private Rectangle GetDropDownRectangle()
		{
			int buttonHeight = this.GetButtonHeight();
			int smallButtonWidth = this.GetSmallButtonWidth();

			return new Rectangle(
				this.ClientRectangle.Width - smallButtonWidth,
				this.ClientRectangle.Height - buttonHeight,
				smallButtonWidth,
				buttonHeight
			);
		}

		/*
		 * GetGripHeight
		 */

		private int GetGripHeight()
		{
			return this.LayoutManager.GetGripHeight();
		}

		/*
		 * GetGripRectangle
		 */

		private Rectangle GetGripRectangle()
		{
			return new Rectangle(
				this.ClientRectangle.Left,
				this.ClientRectangle.Top,
				this.ClientRectangle.Width,
				this.GetGripHeight()
			);
		}

		/*
		 * GetSmallButtonWidth
		 */

		private int GetSmallButtonWidth()
		{
			return this.LayoutManager.GetSmallButtonWidth();
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NuGenNavigationBar"/> class.
		/// </summary>
		/// <param name="serviceProvider"><para>Requires:</para>
		/// <para><see cref="INuGenControlStateTracker"/></para>
		/// <para><see cref="INuGenNavigationBarRenderer"/></para>
		/// <para><see cref="INuGenNavigationBarLayoutManager"/></para>
		/// </param>
		/// <exception cref="ArgumentNullException"><paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
		public NuGenNavigationBar(INuGenServiceProvider serviceProvider)
			: base(serviceProvider)
		{
			this.Font = new Font("Tahoma", 8, FontStyle.Bold);

			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			this.SetStyle(ControlStyles.ResizeRedraw, true);
		}

		#endregion
	}
}
