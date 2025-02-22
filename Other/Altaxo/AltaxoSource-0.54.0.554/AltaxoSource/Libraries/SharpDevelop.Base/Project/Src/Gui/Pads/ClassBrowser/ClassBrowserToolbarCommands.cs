﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Windows.Forms;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser
{
	public class ClassBrowserNavigateBackward : AbstractMenuCommand
	{
		public override bool IsEnabled {
			get {
				return ClassBrowserPad.Instance.CanNavigateBackward;
			}
		}
		public override void Run()
		{
			ClassBrowserPad.Instance.NavigateBackward();
		}
	}
	
	public class ClassBrowserNavigateForward : AbstractMenuCommand
	{
		public override bool IsEnabled {
			get {
				return ClassBrowserPad.Instance.CanNavigateForward;
			}
		}
		public override void Run()
		{
			ClassBrowserPad.Instance.NavigateForward();
		}
	}
	
	#region Class browser filter
	public class SelectClassBrowserFilter : AbstractMenuCommand
	{
		ToolBarDropDownButton dropDownButton;
		
		public override void Run()
		{
		}
		
		protected override void OnOwnerChanged(EventArgs e)
		{
			base.OnOwnerChanged(e);
			dropDownButton = (ToolBarDropDownButton)Owner;
			ToolStripItem[] items = (ToolStripItem[])(AddInTree.GetTreeNode("/SharpDevelop/Pads/ClassBrowser/Toolbar/SelectFilter").BuildChildItems(this)).ToArray(typeof(ToolStripItem));
			foreach (ToolStripItem item in items) {
				if (item is IStatusUpdate) {
					((IStatusUpdate)item).UpdateStatus();
				}
			}
			
			dropDownButton.DropDownItems.AddRange(items);
		}
	}
	
	public class ShowBaseAndDerivedTypes : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowBaseAndDerivedTypes) == ClassBrowserFilter.ShowBaseAndDerivedTypes;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowBaseAndDerivedTypes;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowBaseAndDerivedTypes;
				}
			}
		}
	}
	
	
	public class ShowProjectReferences : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowProjectReferences) == ClassBrowserFilter.ShowProjectReferences;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowProjectReferences;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowProjectReferences;
				}
			}
		}
	}
	
	public class ShowPublicMembers : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowPublic) == ClassBrowserFilter.ShowPublic;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowPublic;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowPublic;
				}
			}
		}
	}
	
	public class ShowProtectedMembers : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowProtected) == ClassBrowserFilter.ShowProtected;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowProtected;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowProtected;
				}
			}
		}
	}
	
	public class ShowPrivateMembers : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowPrivate) == ClassBrowserFilter.ShowPrivate;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowPrivate;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowPrivate;
				}
			}
		}
	}
	
	public class ShowOtherMembers : AbstractCheckableMenuCommand
	{
		public override bool IsChecked {
			get {
				return (ClassBrowserPad.Instance.Filter & ClassBrowserFilter.ShowOther) == ClassBrowserFilter.ShowOther;
			}
			set {
				if (value) {
					ClassBrowserPad.Instance.Filter |= ClassBrowserFilter.ShowOther;
				} else {
					ClassBrowserPad.Instance.Filter &= ~ClassBrowserFilter.ShowOther;
				}
			}
		}
	}
	
	#endregion
	
	
	#region ClassBrowser search strip commands
	public class ClassBrowserSearchTerm : AbstractComboBoxCommand
	{
		ComboBox comboBox;
		
		protected override void OnOwnerChanged(EventArgs e)
		{
			base.OnOwnerChanged(e);
			ToolBarComboBox toolbarItem = (ToolBarComboBox)Owner;
			comboBox = toolbarItem.ComboBox;
			comboBox.DropDownStyle = ComboBoxStyle.DropDown;
			comboBox.TextChanged  += ComboBoxTextChanged;
		}
		
		void ComboBoxTextChanged(object sender, EventArgs e)
		{
			ClassBrowserPad.Instance.SearchTerm = comboBox.Text;
			Run(); // TODO: Enable live search via option
		}
		
		public override void Run()
		{
			ClassBrowserPad.Instance.StartSearch();
		}
	}
	
	public class ClassBrowserCommitSearch : AbstractMenuCommand
	{
		public override void Run()
		{
			ClassBrowserPad.Instance.StartSearch();
		}
	}
	
	public class ClassBrowserCancelSearch : AbstractMenuCommand
	{
		public override bool IsEnabled {
			get {
				return ClassBrowserPad.Instance.IsInSearchMode;
			}
		}
		public override void Run()
		{
			ClassBrowserPad.Instance.CancelSearch();
		}
	}
	#endregion
}
