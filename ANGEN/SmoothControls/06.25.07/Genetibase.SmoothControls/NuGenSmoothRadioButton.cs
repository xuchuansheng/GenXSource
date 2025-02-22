/* -----------------------------------------------
 * NuGenSmoothRadioButton.cs
 * Copyright � 2006 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using Genetibase.Shared;
using Genetibase.Shared.ComponentModel;
using Genetibase.Shared.Controls;
using Genetibase.Shared.Controls.RadioButtonInternals;
using Genetibase.Shared.Windows;
using Genetibase.SmoothControls.Properties;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Genetibase.SmoothControls
{
	/// <summary>
	/// <seealso cref="RadioButton"/>
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(NuGenSmoothRadioButton), "Resources.NuGenIcon.png")]
	[System.ComponentModel.DesignerCategory("Code")]
	public class NuGenSmoothRadioButton : NuGenRadioButton
	{
		#region Properties.Hidden

		/*
		 * BackgroundImage
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage
		{
			get
			{
				return base.BackgroundImage;
			}
			set
			{
				base.BackgroundImage = value;
			}
		}

		/*
		 * BackgroundImageLayout
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout
		{
			get
			{
				return base.BackgroundImageLayout;
			}
			set
			{
				base.BackgroundImageLayout = value;
			}
		}

		/*
		 * FlatAppearance
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new FlatButtonAppearance FlatAppearance
		{
			get
			{
				return base.FlatAppearance;
			}
		}

		/*
		 * Image
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new Image Image
		{
			get
			{
				return base.Image;
			}
			set
			{
				base.Image = value;
			}
		}

		/*
		 * ImageAlign
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ContentAlignment ImageAlign
		{
			get
			{
				return base.ImageAlign;
			}
			set
			{
				base.ImageAlign = value;
			}
		}

		/*
		 * ImageIndex
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int ImageIndex
		{
			get
			{
				return base.ImageIndex;
			}
			set
			{
				base.ImageIndex = value;
			}
		}

		/*
		 * ImageKey
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new string ImageKey
		{
			get
			{
				return base.ImageKey;
			}
			set
			{
				base.ImageKey = value;
			}
		}

		/*
		 * ImageList
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImageList ImageList
		{
			get
			{
				return base.ImageList;
			}
			set
			{
				base.ImageList = value;
			}
		}

		/*
		 * TextImageRelation
		 */

		/// <summary>
		/// Do not use this property. Any value will not affect the appearance.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new TextImageRelation TextImageRelation
		{
			get
			{
				return base.TextImageRelation;
			}
			set
			{
				base.TextImageRelation = value;
			}
		}

		#endregion

		/*
		 * DefaultSize
		 */

		private static readonly Size _defaultSize = new Size(165, 24);

		/// <summary>
		/// </summary>
		protected override Size DefaultSize
		{
			get
			{
				return _defaultSize;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NuGenSmoothRadioButton"/> class.
		/// </summary>
		public NuGenSmoothRadioButton()
			: this(NuGenSmoothServiceManager.RadioButtonServiceProvider)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NuGenSmoothRadioButton"/> class.
		/// </summary>
		/// <param name="serviceProvider">
		/// Requires:<para/>
		/// <see cref="INuGenRadioButtonRenderer"/><para/>
		/// <see cref="INuGenButtonStateTracker"/><para/>
		/// <see cref="INuGenControlStateTracker"/><para/>
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <para>
		///		<paramref name="serviceProvider"/> is <see langword="null"/>.
		/// </para>
		/// </exception>
		public NuGenSmoothRadioButton(INuGenServiceProvider serviceProvider)
			: base(serviceProvider)
		{
		}
	}
}
