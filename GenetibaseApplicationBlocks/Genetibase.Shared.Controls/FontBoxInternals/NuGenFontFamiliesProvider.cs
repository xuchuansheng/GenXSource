/* -----------------------------------------------
 * NuGenFontFamiliesProvider.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Genetibase.Shared.Controls.FontBoxInternals
{
	/// <summary>
	/// Provides methods to retrieve font families and font sample images.
	/// </summary>
	public sealed class NuGenFontFamiliesProvider : INuGenFontFamiliesProvider
	{
		#region INuGenFontFamiliesProvider Members

		/*
		 * FillWithFontNames
		 */

		/// <summary>
		/// </summary>
		/// <param name="collectionToFill"></param>
		public void FillWithFontNames(out List<string> collectionToFill)
		{
			collectionToFill = new List<string>();

			foreach (FontFamily font in FontFamily.Families)
			{
				if (font.IsStyleAvailable(FontStyle.Regular))
				{
					collectionToFill.Add(font.Name);
				}
			}
		}

		/*
		 * FillWithFontSamples
		 */

		/// <summary>
		/// </summary>
		/// <param name="fontNames"></param>
		/// <param name="imageListToFill"></param>
		/// <exception cref="ArgumentNullException">
		/// <para><paramref name="fontNames"/> is <see langword="null"/>.</para>
		/// </exception>
		public void FillWithFontSamples(List<string> fontNames, out ImageList imageListToFill)
		{
			if (fontNames == null)
			{
				throw new ArgumentNullException("fontNames");
			}

			Size imageSize = new Size(28, 16);

			imageListToFill = new ImageList();
			imageListToFill.ImageSize = imageSize;
			Rectangle imageRectangle = new Rectangle(new Point(0, 0), imageSize);

			foreach (string fontName in fontNames)
			{
				Image fontSample = new Bitmap(imageRectangle.Width, imageRectangle.Height);

				using (Graphics g = Graphics.FromImage(fontSample))
				using (Font font = new Font(fontName, 10))
				{
					g.DrawString("ab", font, SystemBrushes.WindowText, imageRectangle);
				}

				imageListToFill.Images.Add(fontSample);
			}
		}

		#endregion
	}
}
