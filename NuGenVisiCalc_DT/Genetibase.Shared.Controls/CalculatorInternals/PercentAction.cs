/* -----------------------------------------------
 * PercentAction.cs
 * Copyright � 2007 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Genetibase.Shared.Controls.CalculatorInternals
{
	internal sealed class PercentAction : IAction
	{
		public State GetState(State currentState)
		{
			currentState.CurrentValue = currentState.PreviousValue * (currentState.CurrentValue / 100.0);
			currentState.IsCalculated = true;
			return currentState;
		}
	}
}