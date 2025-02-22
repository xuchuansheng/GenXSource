/* -----------------------------------------------
 * INuGenScrollBarRenderer.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Text;

namespace Genetibase.Shared.Controls.ScrollBarInternals
{
	/// <summary>
	/// </summary>
	public interface INuGenScrollBarRenderer
	{
		/// <summary>
		/// </summary>
		void DrawScrollButton(NuGenPaintParams paintParams);

		/// <summary>
		/// </summary>
		void DrawScrollTrack(NuGenPaintParams paintParams);

		/// <summary>
		/// </summary>
		void DrawSizeBox(NuGenPaintParams paintParams);
	}
}
