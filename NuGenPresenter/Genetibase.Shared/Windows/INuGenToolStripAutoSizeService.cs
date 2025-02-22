/* -----------------------------------------------
 * INuGenToolStripAutoSizeService.cs
 * Copyright � 2006 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using System;
using System.Windows.Forms;

namespace Genetibase.Shared.Windows
{
	/// <summary>
	/// </summary>
	public interface INuGenToolStripAutoSizeService
	{
		/// <summary>
		/// </summary>
		/// <param name="targetItem"></param>
		/// <param name="minWidth"></param>
		/// <returns></returns>
		void SetNewWidth(ToolStripItem targetItem, int minWidth);
	}
}
