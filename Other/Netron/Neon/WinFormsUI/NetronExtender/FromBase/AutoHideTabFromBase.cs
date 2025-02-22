
using System;
using System.Drawing;
using System.ComponentModel;
using Netron.Neon.WinFormsUI;
namespace Netron.Neon.WinFormsUI.Netron
{
	[ToolboxItem(false)]
	internal class AutoHideTabFromBase : AutoHideTab
	{
		internal AutoHideTabFromBase(IDockContent content) : base(content)
		{
		}

		private int m_tabX = 0;
		protected internal int TabX
		{
			get	{	return m_tabX;	}
			set	{	m_tabX = value;	}
		}

		private int m_tabWidth = 0;
		protected internal int TabWidth
		{
			get	{	return m_tabWidth;	}
			set	{	m_tabWidth = value;	}
		}

	}
}
