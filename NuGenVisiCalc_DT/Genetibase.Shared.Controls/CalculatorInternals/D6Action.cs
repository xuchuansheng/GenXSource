/* -----------------------------------------------
 * D6Action.cs
 * Copyright � 2007 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Genetibase.Shared.Controls.CalculatorInternals
{
	internal sealed class D6Action : IAction
	{
		public State GetState(State currentState)
		{
			return LogicManager.ProcessDigit(currentState, 6);
		}
	}
}
