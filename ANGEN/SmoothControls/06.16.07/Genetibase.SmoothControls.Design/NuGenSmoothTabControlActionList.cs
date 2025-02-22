/* -----------------------------------------------
 * NuGenSmoothTabControlActionList.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using Genetibase.Shared.Controls;
using Genetibase.Shared.Controls.Design;
using Genetibase.SmoothControls.Properties;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace Genetibase.SmoothControls.Design
{
	/// <summary>
	/// </summary>
	internal sealed class NuGenSmoothTabControlActionList : NuGenTabControlActionList
	{
		public override void AddTabPage()
		{
			this.CreateTabPage<NuGenSmoothTabPage>();
		}

		public NuGenSmoothTabControlActionList(NuGenTabControl tabControl)
			: base(tabControl)
		{
		}
	}
}
