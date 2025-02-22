﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 2105 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser
{
	/// <summary>
	/// This class reperesents the base class for all nodes in the
	/// class browser.
	/// </summary>
	public class ProjectNode : AbstractProjectNode
	{
		protected ProjectNode() : base()
		{
		}
		
		public ProjectNode(IProject project) : base(project)
		{
			sortOrder = 0;
			
			Text = Project.Name;
			SetIcon(IconService.GetImageForProjectType(Project.Language));
			Nodes.Add(new TreeNode(StringParser.Parse("${res:ICSharpCode.SharpDevelop.Gui.Pads.ClassScout.LoadingNode}")));
		}
		
		public override void UpdateParseInformation(ICompilationUnit oldUnit, ICompilationUnit unit)
		{
			Dictionary<string, IClass> classDictionary      = new Dictionary<string, IClass>();
			Dictionary<string, bool>   wasUpdatedDictionary = new Dictionary<string, bool>();
			
			if (oldUnit != null) {
				foreach (IClass c in oldUnit.Classes) {
					classDictionary[c.FullyQualifiedName] = c.GetCompoundClass();
					wasUpdatedDictionary[c.FullyQualifiedName] = false;
				}
			}
			if (unit != null) {
				foreach (IClass c in unit.Classes) {
					TreeNode  path = GetNodeByPath(c.Namespace, true);
					ClassNode node = GetNodeByName(path.Nodes, c.Name) as ClassNode;
					if (node != null) {
						node.Class = c.GetCompoundClass();
					} else {
						new ClassNode(Project, c.GetCompoundClass()).AddTo(path);
					}
					wasUpdatedDictionary[c.FullyQualifiedName] = true;
				}
			}
			foreach (KeyValuePair<string, bool> entry in wasUpdatedDictionary) {
				if (!entry.Value) {
					IClass c = classDictionary[entry.Key];
					
					TreeNode path = GetNodeByPath(c.Namespace, true);
					ClassNode node = GetNodeByName(path.Nodes, c.Name) as ClassNode;
					
					if (node != null) {
						CompoundClass cc = c as CompoundClass;
						if (cc != null) {
							node.Class = cc; // update members after part has been removed
						} else {
							path.Nodes.Remove(node);
							RemoveEmptyNamespace(path);
						}
					}
				}
			}
		}
		
		void RemoveEmptyNamespace(TreeNode path)
		{
			if ((path.Tag is string) && path.Nodes.Count == 0) {
				TreeNode parent = path.Parent;
				parent.Nodes.Remove(path);
				RemoveEmptyNamespace(parent);
			}
		}
		
		protected override void Initialize()
		{
			base.Initialize();
			IProjectContent projectContent = ParserService.GetProjectContent(Project);
			
			if (projectContent != null) {
				Nodes.Clear();
				ReferenceFolderNode referencesNode = new ReferenceFolderNode(Project);
				referencesNode.AddTo(this);
				projectContent.ReferencedContentsChanged += delegate { WorkbenchSingleton.SafeThreadAsyncCall(referencesNode.UpdateReferenceNodes); };
				foreach (ProjectItem item in Project.GetItemsOfType(ItemType.Compile)) {
					ParseInformation parseInformation = ParserService.GetParseInformation(item.FileName);
					if (parseInformation != null) {
						InsertParseInformation(parseInformation.BestCompilationUnit as ICompilationUnit);
					}
				}
			}
		}
		
		void InsertParseInformation(ICompilationUnit unit)
		{
			foreach (IClass c in unit.Classes) {
				TreeNode path = GetNodeByPath(c.Namespace, true);
				TreeNode node = GetNodeByName(path.Nodes, c.Name);
				if (node == null) {
					new ClassNode(Project, c.GetCompoundClass()).AddTo(path);
				}
			}
		}
		
		protected virtual string StripRootNamespace(string directory)
		{
			if (Project != null) {
				// TODO: Give user the option to always show the root namespace
				string rootNamespace = Project.RootNamespace;
				if (directory.StartsWith(rootNamespace)) {
					directory = directory.Substring(rootNamespace.Length);
				}
			}
			return directory;
		}

		public override TreeNode GetNodeByPath(string directory, bool create)
		{
			directory = StripRootNamespace(directory);
			
			string[] treepath   = directory.Split(new char[] { '.' });
			TreeNodeCollection curcollection = Nodes;
			TreeNode           curnode       = this;
			
			foreach (string path in treepath) {
				if (path.Length == 0 || path[0] == '.') {
					continue;
				}

				TreeNode node = GetNodeByName(curcollection, path);
				if (node == null) {
					if (create) {
						ExtTreeNode newnode = new ExtTreeNode();
						newnode.Tag      = path;
						newnode.Text     = path;
						newnode.ImageIndex = newnode.SelectedImageIndex = ClassBrowserIconService.NamespaceIndex;
						curcollection.Add(newnode);
						curnode       = newnode;
						curcollection = curnode.Nodes;
						continue;
					} else {
						return null;
					}
				}
				curnode       = node;
				curcollection = curnode.Nodes;
			}
			return curnode;
		}
		
		static TreeNode GetNodeByName(TreeNodeCollection collection, string name)
		{
			foreach (TreeNode node in collection) {
				// we don't want the reference folder node otherwise the namespace 'References' won't be shown correctly :)
				if (!(node is ReferenceFolderNode) && node.Text == name) {
					return node;
				}
			}
			return null;
		}
	}
}
