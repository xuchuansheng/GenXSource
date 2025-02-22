/* -----------------------------------------------
 * MdiStateEventArgs.cs
 * Copyright � 2007 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Genetibase.NuGenVisiCalc
{
	internal sealed class MdiStateEventArgs : EventArgs
	{
		/*
		 * IsEmpty
		 */

		private Boolean _isEmpty;

		public Boolean IsEmpty
		{
			get
			{
				return _isEmpty;
			}
		}

		public MdiStateEventArgs(Boolean isEmpty)
		{
			_isEmpty = isEmpty;
		}
	}
}
