namespace Netron.Neon.OfficePickers
{
    partial class OfficeLineWidthPicker
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lineWidthTip = new System.Windows.Forms.ToolTip(this.components);
            this.mLineWidthDialog = new Netron.Neon.OfficePickers.CustomLineWidthDialog();
            this.SuspendLayout();
            // 
            // lineWidthTip
            // 
            this.lineWidthTip.AutoPopDelay = 5000;
            this.lineWidthTip.InitialDelay = 500;
            this.lineWidthTip.ReshowDelay = 1000;
            // 
            // mLineWidthDialog
            // 
            this.mLineWidthDialog.LineWidth = 1F;
            // 
            // OfficeLineWidthPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "OfficeLineWidthPicker";
            this.Size = new System.Drawing.Size(146, 143);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip lineWidthTip;
        private CustomLineWidthDialog mLineWidthDialog;
    }
}
