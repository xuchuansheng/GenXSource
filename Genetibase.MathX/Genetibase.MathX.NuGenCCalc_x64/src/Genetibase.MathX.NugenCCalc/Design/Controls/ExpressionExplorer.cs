using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Genetibase.MathX.Core;

namespace Genetibase.MathX.NugenCCalc.Design.Controls
{
	/// <summary>
	/// Summary description for ExpressionExplorer.
	/// </summary>
	[ToolboxItem(false)]
	public class ExpressionExplorer : System.Windows.Forms.UserControl
	{
		private bool _is3DDesigner = false;
		public event ItemActionEventHandler OnItemAction;

		private System.Windows.Forms.TreeView tvExpressions;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.ContextMenu contextMenuTreeView;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.MenuItem menuItem_Create;
		private System.Windows.Forms.MenuItem menuItem_Delete;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem_Use;
		private System.Windows.Forms.MenuItem menuItem_Edit;

		private FunctionCollection _expressions = null;
		private FunctionParameters _currentItem = null;

		public ExpressionExplorer()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		public ExpressionExplorer(FunctionCollection expressions) : this()
		{
			_expressions = expressions;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExpressionExplorer));
			this.tvExpressions = new System.Windows.Forms.TreeView();
			this.contextMenuTreeView = new System.Windows.Forms.ContextMenu();
			this.menuItem_Create = new System.Windows.Forms.MenuItem();
			this.menuItem_Edit = new System.Windows.Forms.MenuItem();
			this.menuItem_Delete = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem_Use = new System.Windows.Forms.MenuItem();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.SuspendLayout();
			// 
			// tvExpressions
			// 
			this.tvExpressions.ContextMenu = this.contextMenuTreeView;
			this.tvExpressions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvExpressions.ImageList = this.imageList;
			this.tvExpressions.Location = new System.Drawing.Point(0, 0);
			this.tvExpressions.Name = "tvExpressions";
			this.tvExpressions.Size = new System.Drawing.Size(144, 269);
			this.tvExpressions.TabIndex = 0;
			this.tvExpressions.Click += new System.EventHandler(this.tvExpressions_Click);
			this.tvExpressions.DoubleClick += new System.EventHandler(this.tvExpressions_DoubleClick);
			this.tvExpressions.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvExpressions_AfterSelect);
			// 
			// contextMenuTreeView
			// 
			this.contextMenuTreeView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.menuItem_Create,
																								this.menuItem_Edit,
																								this.menuItem_Delete,
																								this.menuItem1,
																								this.menuItem_Use});
			this.contextMenuTreeView.Popup += new System.EventHandler(this.contextMenuTreeView_Popup);
			// 
			// menuItem_Create
			// 
			this.menuItem_Create.Index = 0;
			this.menuItem_Create.Text = "Create";
			this.menuItem_Create.Click += new System.EventHandler(this.menuItem_Click);
			// 
			// menuItem_Edit
			// 
			this.menuItem_Edit.Index = 1;
			this.menuItem_Edit.Text = "Edit";
			this.menuItem_Edit.Click += new System.EventHandler(this.menuItem_Click);
			// 
			// menuItem_Delete
			// 
			this.menuItem_Delete.Index = 2;
			this.menuItem_Delete.Text = "Delete";
			this.menuItem_Delete.Click += new System.EventHandler(this.menuItem_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 3;
			this.menuItem1.Text = "-";
			// 
			// menuItem_Use
			// 
			this.menuItem_Use.Index = 4;
			this.menuItem_Use.Text = "Use It!";
			this.menuItem_Use.Click += new System.EventHandler(this.menuItem_Click);
			// 
			// imageList
			// 
			this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
			this.imageList.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// ExpressionExplorer
			// 
			this.Controls.Add(this.tvExpressions);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
			this.Name = "ExpressionExplorer";
			this.Size = new System.Drawing.Size(144, 269);
			this.ResumeLayout(false);

		}
		#endregion

		public bool Is3DDesigner
		{
			set
			{
				if (_is3DDesigner == value)
					return;
				_is3DDesigner = value;
			}
		}

		/// <summary>
		/// Set collection of expression for this tree
		/// </summary>
		public FunctionCollection Expressions
		{
			set
			{
				_expressions = value;
			}
		}

		/// <summary>
		/// Get selected expression
		/// </summary>
		public FunctionParameters Selected
		{
			get
			{
				return _currentItem;
			}
		}

		/// <summary>
		/// Create tree nodes
		/// </summary>
		public void Init()
		{
			try
			{
				if (this._expressions == null)
					return;

				_currentItem = null;
				this.tvExpressions.Nodes.Clear();
				// create root nodes
				TreeNode root = new TreeNode();
				root.Text = "Code Expressions";
				root.ImageIndex = 0;
				root.SelectedImageIndex = 1;
				FunctionNode node = null;


				if (_is3DDesigner)
				{
					foreach(FunctionParameters expression in _expressions.Functions3D)
					{
						node = new FunctionNode(expression);
						switch(expression.CodeLanguage)
						{
							case CodeLanguage.CSharp:
								node.ImageIndex = 2;
								node.SelectedImageIndex = 2;
								break;
							case CodeLanguage.VBNET:
								node.ImageIndex = 3;
								node.SelectedImageIndex = 3;
								break;
						}

						root.Nodes.Add(node);

					}
				}
				else
				{
					foreach(FunctionParameters expression in _expressions.Functions2D)
					{
						node = new FunctionNode(expression);
						switch(expression.CodeLanguage)
						{
							case CodeLanguage.CSharp:
								node.ImageIndex = 2;
								node.SelectedImageIndex = 2;
								break;
							case CodeLanguage.VBNET:
								node.ImageIndex = 3;
								node.SelectedImageIndex = 3;
								break;
						}

						root.Nodes.Add(node);

					}
				}


				this.tvExpressions.Nodes.Add(root);
				this.tvExpressions.ExpandAll();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, "Unknown error. Error:" + ex.Message, "Expression repository view", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}



		private void menuItem_Click(object sender, System.EventArgs e)
		{
			MenuItem item = (MenuItem)sender;
			switch(item.Text)
			{
				case "Create":
					ItemAction(null, "Create");
					break;
				case "Edit":
					ItemAction(_currentItem, "Edit");
					break;
				case "Delete":
					ItemAction(_currentItem, "Delete");
					break;
				case "Use It!":
					ItemAction(_currentItem, "Use It!");
					break;
				default:
					MessageBox.Show(this,"Unknown menu", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					break;
			}
		}


		private void ItemAction (FunctionParameters item, string action)
		{
			if (OnItemAction!=null)
			{
				OnItemAction(this, new ItemEventArgs(item,action));
			}
		}


		private void tvExpressions_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			try
			{
				FunctionNode FunctionNode = e.Node as FunctionNode;

				_currentItem = null;

				if (FunctionNode != null) 
					_currentItem = (FunctionParameters)FunctionNode.Item;
				else
					tvExpressions.SelectedNode = e.Node.NextVisibleNode;				
			}
			catch(Exception ex)
			{
				MessageBox.Show(this,"Can't get node tag. Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Show or hide some context menu items
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void contextMenuTreeView_Popup(object sender, System.EventArgs e)
		{
			FunctionParameters expression = null;
			if (this.tvExpressions.SelectedNode != null)
			{
				expression = ((FunctionNode)this.tvExpressions.SelectedNode).Item as FunctionParameters;
			}

			try
			{
				if (expression == null) 
				{
					_currentItem = null;
					this.menuItem_Delete.Visible = false;
					this.menuItem_Edit.Visible = false;
					this.menuItem_Use.Visible = false;
					this.menuItem1.Visible = false;
				}
				else
				{
					_currentItem = expression;
					this.menuItem_Delete.Visible = true;
					this.menuItem_Edit.Visible = true;
					this.menuItem_Use.Visible = true;
					this.menuItem1.Visible = true;
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(this,"Can't get node tag. Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void tvExpressions_DoubleClick(object sender, System.EventArgs e)
		{
			try
			{
				FunctionNode FunctionNode = tvExpressions.GetNodeAt(tvExpressions.PointToClient(Cursor.Position)) as FunctionNode;

				_currentItem = null;

				if (FunctionNode != null) 
				{
					_currentItem = FunctionNode.Item;
					tvExpressions.SelectedNode = FunctionNode;
					ItemAction(_currentItem, "Edit");
				}
			}
			catch(Exception ex)
			{
				MessageBox.Show(this,"Can't get node tag. Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void tvExpressions_Click(object sender, System.EventArgs e)
		{
			FunctionNode FunctionNode = tvExpressions.GetNodeAt(tvExpressions.PointToClient(Cursor.Position)) as FunctionNode;
			if (FunctionNode == null)
				return;
			tvExpressions.SelectedNode = FunctionNode;
		}

	}
}