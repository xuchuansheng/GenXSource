// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Robert Zaunere" email=""/>
//     <version>$Revision: 1634 $</version>
// </file>

using System;
using System.Windows.Forms;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Gui.ClassBrowser
{
	public static class ProjectNodeBuilders
	{
		/// <summary>
		/// This method builds a ClassBrowserNode Tree out of a given combine.
		/// </summary>
		public static TreeNode AddProjectNode(ExtTreeView classBrowser, IProject project)
		{
			IProjectNodeBuilder projectNodeBuilder = null;
			foreach (IProjectNodeBuilder nodeBuilder in AddInTree.BuildItems("/SharpDevelop/Views/ClassBrowser/ProjectNodeBuilders", null, true))
			{
				if (nodeBuilder.CanBuildProjectTree(project))
				{
					projectNodeBuilder = nodeBuilder;
					break;
				}
			}
			if (projectNodeBuilder != null)
			{
				return projectNodeBuilder.AddProjectNode(classBrowser, project);
			}

			throw new NotImplementedException("can't create node builder for project type " + project.Language);
		}
	}
}
