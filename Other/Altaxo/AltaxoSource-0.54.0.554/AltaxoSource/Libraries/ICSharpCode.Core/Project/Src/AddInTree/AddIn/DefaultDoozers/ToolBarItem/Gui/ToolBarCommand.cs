﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1336 $</version>
// </file>

using System;
using System.Windows.Forms;

namespace ICSharpCode.Core
{
#if ModifiedForAltaxo
  // ToolStripButton instead of ToolStripMenuItem is neccessary in order to show the Checked state of an item
  public class ToolBarCommand : ToolStripButton, IStatusUpdate
#else
	public class ToolBarCommand : ToolStripMenuItem, IStatusUpdate
#endif
	{
		object caller;
		Codon codon;
		ICommand menuCommand = null;
		
		public ToolBarCommand(Codon codon, object caller, bool createCommand)
		{
			this.RightToLeft = RightToLeft.Inherit;
			this.caller        = caller;
			this.codon         = codon;
			
			if (createCommand) {
				menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			
			if (codon.Properties.Contains("label")){
				Text = StringParser.Parse(codon.Properties["label"]);
			}
			if (Image == null && codon.Properties.Contains("icon")) {
				Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
			}
			
			UpdateStatus();
			UpdateText();
		}
		
		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if (menuCommand == null) {
				menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
			}
			if (menuCommand != null) {
				menuCommand.Owner = caller;
				menuCommand.Run();
			}
		}
		
		public virtual void UpdateStatus()
		{
			if (codon != null) {
				ConditionFailedAction failedAction = codon.GetFailedAction(caller);
				this.Visible = failedAction != ConditionFailedAction.Exclude;
				bool isEnabled = failedAction != ConditionFailedAction.Disable;
				if (isEnabled && menuCommand != null && menuCommand is IMenuCommand) {
					isEnabled = ((IMenuCommand)menuCommand).IsEnabled;
				}
#if ModifiedForAltaxo
        // set/reset the Checked state of a item (a ToolStripMenuItem can not (!) show the Checked state, one has to use a ToolStripButton
        if (isEnabled && menuCommand != null && menuCommand is ICheckableMenuCommand)
        {
          base.Checked = ((ICheckableMenuCommand)menuCommand).IsChecked;
        }
#endif
				this.Enabled = isEnabled;
				
				if (this.Visible && codon.Properties.Contains("icon")) {
					Image = ResourceService.GetBitmap(StringParser.Parse(codon.Properties["icon"]));
				}
			}
		}
		
		public virtual void UpdateText()
		{
			if (codon != null) {
				if (codon.Properties.Contains("tooltip")) {
					ToolTipText = StringParser.Parse(codon.Properties["tooltip"]);
				}
				
				if (codon.Properties.Contains("label")){
					Text = StringParser.Parse(codon.Properties["label"]);
				}
			}
		}
	}
}
