/* -----------------------------------------------
 * Structs.cs
 * Copyright � 2006 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using System;
using System.Drawing;
using System.Globalization;

namespace Genetibase.WinApi
{
	/// <summary>
	/// Defines the coordinates of the upper-left and lower-right corners of a rectangle.
	/// </summary>
	public struct RECT
	{
		#region Properties.Public

		/*
		 * Height
		 */

		/// <summary>
		/// Gets the height of this rectangle.
		/// </summary>
		public Int32 Height
		{
			get
			{
				return this.bottom - this.top;
			}
		}

		/*
		 * Location
		 */

		/// <summary>
		/// Gets the location of this rectangle.
		/// </summary>
		public Point Location
		{
			get
			{
				return new Point(this.left, this.top);
			}
		}

		/*
		 * Size
		 */

		/// <summary>
		/// Gets the size of this rectangle.
		/// </summary>
		public Size Size
		{
			get
			{
				return new Size(this.Width, this.Height);
			}
		}

		/*
		 * Width
		 */

		/// <summary>
		/// Gets the width of this rectangle.
		/// </summary>
		public Int32 Width
		{
			get
			{
				return this.right - this.left;
			}
		}

		#endregion

		#region Methods.Public.Overridden

		/// <summary>
		/// Indicates whether this <see cref="RECT"/> and a specified Object are equal.
		/// </summary>
		/// <param name="obj">Another Object to compare to.</param>
		/// <returns>
		/// true if obj and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		public override Boolean Equals(Object obj)
		{
			if (obj is RECT)
			{
				RECT rect = (RECT)obj;

				if (
					rect.bottom == this.bottom
					&& rect.left == this.left
					&& rect.right == this.right
					)
				{
					return rect.top == this.top;
				}
			}
			else if (obj is Rectangle)
			{
				Rectangle rect = (Rectangle)obj;

				if (
					rect.Location == this.Location
					)
				{
					return rect.Size == this.Size;
				}
			}

			return false;
		}

		/*
		 * GetHashCode
		 */

		/// <summary>
		/// Calculates hash code for this <see cref="RECT"/> structure.
		/// </summary>
		/// <returns></returns>
		public override Int32 GetHashCode()
		{
			return ((this.left ^ ((this.top << 13) | (this.top >> 0x13))) ^ ((this.right << 0x1A) | (this.right >> 6))) ^ ((this.bottom << 7) | (this.bottom >> 0x19));
		}

		/*
		 * ToString
		 */

		/// <summary>
		/// Returns String representation of this <see cref="RECT"/> structure.
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			return String.Format(
				CultureInfo.InvariantCulture,
				"{Left={0},Top={1},Right={2},Bottom={3}}",
				this.left,
				this.top,
				this.right,
				this.bottom
				);
		}

		#endregion

		#region Methods.Operators

		/// <summary>
		/// </summary>
		public static implicit operator RECT(Rectangle rect)
		{
			return new RECT(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// </summary>
		public static implicit operator Rectangle(RECT rect)
		{
			return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
		}

		/// <summary>
		/// </summary>
		public static Boolean operator ==(RECT left, RECT right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// </summary>
		public static Boolean operator !=(RECT left, RECT right)
		{
			return !left.Equals(right);
		}

		#endregion

		/// <summary>
		/// Specifies the x-coordinate of the upper-left corner of the rectangle. 
		/// </summary>
		public Int32 left;

		/// <summary>
		/// Specifies the y-coordinate of the upper-left corner of the rectangle. 
		/// </summary>
		public Int32 top;

		/// <summary>
		/// Specifies the x-coordinate of the lower-right corner of the rectangle. 
		/// </summary>
		public Int32 right;

		/// <summary>
		/// Specifies the y-coordinate of the lower-right corner of the rectangle. 
		/// </summary>
		public Int32 bottom;

		/// <summary>
		/// Initializes a new instance of the <see cref="RECT"/> structure.
		/// </summary>
		/// <param name="left">Specifies the x-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="top">Specifies the y-coordinate of the upper-left corner of the rectangle.</param>
		/// <param name="right">Specifies the x-coordinate of the lower-right corner of the rectangle.</param>
		/// <param name="bottom">Specifies the y-coordinate of the lower-right corner of the rectangle.</param>
		public RECT(Int32 left, Int32 top, Int32 right, Int32 bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
	}
}
