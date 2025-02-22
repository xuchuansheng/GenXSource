/* -----------------------------------------------
 * NuGenLimit.cs
 * Copyright � 2007 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Text;

namespace Genetibase.Shared
{
	/// <summary>
	/// Stores a pair of integers representing the minimum and maximum values. 
	/// </summary>
	public struct NuGenLimit
	{
		/*
		 * Maximum
		 */

		private int _maximum;

		/// <summary>
		/// </summary>
		public int Maximum
		{
			get
			{
				return _maximum;
			}
			set
			{
				_maximum = value;
			}
		}

		/*
		 * Minimum
		 */

		private int _minimum;

		/// <summary>
		/// </summary>
		public int Minimum
		{
			get
			{
				return _minimum;
			}
			set
			{
				_minimum = value;
			}
		}

		/*
		 * GetLimitedValue
		 */

		/// <summary>
		/// </summary>
		/// <param name="valueToLimit"></param>
		/// <returns></returns>
		public int GetLimitedValue(int valueToLimit)
		{
			return Math.Min(this.Maximum, Math.Max(this.Minimum, valueToLimit));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NuGenLimit"/> structure.
		/// </summary>
		public NuGenLimit(int minimum, int maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}
	}
}
