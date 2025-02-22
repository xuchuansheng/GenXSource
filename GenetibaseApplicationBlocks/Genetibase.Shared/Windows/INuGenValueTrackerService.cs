/* -----------------------------------------------
 * INuGenValueTrackerService.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Text;

namespace Genetibase.Shared.Windows
{
	/// <summary>
	/// </summary>
	public interface INuGenValueTrackerService
	{
		/// <summary>
		/// </summary>
		/// <returns></returns>
		NuGenValueTracker CreateValueTracker();
	}
}
