﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2124 $</version>
// </file>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using Microsoft.Build.BuildEngine;

namespace ICSharpCode.SharpDevelop.Project
{
	/// <summary>
	/// A project item is based either on an MSBuild build item, or "manually" saves the
	/// type/include/metadata. The project item is strictly bound to it's parent project.
	/// The MSBuild build item is used while the item is added to the project (IsAddedToProject
	/// is true). During that time, Include may not be an empty string.
	/// However, prior to the item being added to the project, Include may be an empty string
	/// (this is also the default for new items created using the (IProject, ItemType) constructor.
	/// </summary>
	public abstract class ProjectItem : LocalizedObject, IDisposable, ICloneable
	{
		IProject project;
		volatile string fileNameCache;
		
		// either use: (bound mode)
		BuildItem buildItem;
		
		// or: (virtual mode)
		string virtualInclude;
		ItemType virtualItemType;
		Dictionary<string, string> virtualMetadata = new Dictionary<string, string>();
		
		protected ProjectItem(IProject project, BuildItem buildItem)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			this.project = project;
			this.buildItem = buildItem;
		}
		
		protected ProjectItem(IProject project, ItemType itemType)
			: this(project, itemType, null)
		{
		}
		
		protected ProjectItem(IProject project, ItemType itemType, string include)
		{
			this.project = project;
			this.virtualItemType = itemType;
			this.virtualInclude = include ?? "";
			this.virtualMetadata = new Dictionary<string, string>();
		}
		
		[Browsable(false)]
		public IProject Project {
			get {
				return project;
			}
		}
		
		/// <summary>
		/// Gets the object used for synchronization. This is project.SyncRoot for items inside a project; or
		/// virtualMetadata for items without project.
		/// </summary>
		object SyncRoot {
			get {
				if (project != null)
					return project.SyncRoot;
				else
					return virtualMetadata;
			}
		}
		
		/// <summary>
		/// Gets if the item is added to it's owner project.
		/// </summary>
		[Browsable(false)]
		public bool IsAddedToProject {
			get {
				return buildItem != null;
			}
		}
		
		[Browsable(false)]
		internal BuildItem BuildItem {
			get { return buildItem; }
			set {
				if (project is AbstractProject) {
					((AbstractProject)project).ClearFindFileCache();
				}
				
				if (value != null) {
					virtualMetadata = null;
					virtualItemType = default(ItemType);
					virtualInclude = null;
				} else {
					virtualItemType = this.ItemType;
					virtualInclude = this.Include;
					virtualMetadata = new Dictionary<string, string>();
					foreach (string name in this.MetadataNames) {
						virtualMetadata[name] = this.GetMetadata(name);
					}
				}
				buildItem = value;
			}
		}
		
		[Browsable(false)]
		public ItemType ItemType {
			get {
				lock (SyncRoot) {
					if (buildItem != null)
						return new ItemType(buildItem.Name);
					else
						return virtualItemType;
				}
			}
			set {
				lock (SyncRoot) {
					if (buildItem != null)
						buildItem.Name = value.ToString();
					else
						virtualItemType = value;
				}
			}
		}
		
		[Browsable(false)]
		public string Include {
			get {
				lock (SyncRoot) {
					if (buildItem != null)
						return buildItem.FinalItemSpec;
					else
						return virtualInclude;
				}
			}
			set {
				lock (SyncRoot) {
					if (project is AbstractProject) {
						((AbstractProject)project).ClearFindFileCache();
					}
					
					if (buildItem != null)
						buildItem.Include = MSBuildInternals.Escape(value);
					else
						virtualInclude = value ?? "";
					fileNameCache = null;
				}
			}
		}
		
		#region Metadata access
		public bool HasMetadata(string metadataName)
		{
			lock (SyncRoot) {
				if (buildItem != null)
					return buildItem.HasMetadata(metadataName);
				else
					return virtualMetadata.ContainsKey(metadataName);
			}
		}
		
		/// <summary>
		/// Gets the evaluated value of the metadata item with the specified name.
		/// Returns an empty string for non-existing meta data items.
		/// </summary>
		public string GetEvaluatedMetadata(string metadataName)
		{
			lock (SyncRoot) {
				if (buildItem != null) {
					return buildItem.GetEvaluatedMetadata(metadataName) ?? "";
				} else {
					string val;
					virtualMetadata.TryGetValue(metadataName, out val);
					if (val == null)
						return "";
					else
						return MSBuildInternals.Unescape(val);
				}
			}
		}
		
		/// <summary>
		/// Gets the value of the metadata item with the specified name.
		/// Returns defaultValue for non-existing meta data items.
		/// </summary>
		public T GetEvaluatedMetadata<T>(string metadataName, T defaultValue)
		{
			return GenericConverter.FromString(GetEvaluatedMetadata(metadataName), defaultValue);
		}
		
		/// <summary>
		/// Gets the escaped/unevaluated value of the metadata item with the specified name.
		/// Returns an empty string for non-existing meta data items.
		/// </summary>
		public string GetMetadata(string metadataName)
		{
			lock (SyncRoot) {
				if (buildItem != null) {
					return buildItem.GetMetadata(metadataName) ?? "";
				} else {
					string val;
					virtualMetadata.TryGetValue(metadataName, out val);
					return val ?? "";
				}
			}
		}
		
		/// <summary>
		/// Sets the value of the specified meta data item. The value is escaped before
		/// setting it to ensure characters like ';' or '$' are not interpreted by MSBuild.
		/// Setting value to null or an empty string results in removing the metadata item.
		/// </summary>
		public void SetEvaluatedMetadata(string metadataName, string value)
		{
			if (string.IsNullOrEmpty(value)) {
				RemoveMetadata(metadataName);
			} else {
				lock (SyncRoot) {
					if (buildItem != null)
						buildItem.SetMetadata(metadataName, value, true);
					else
						virtualMetadata[metadataName] = MSBuildInternals.Escape(value);
				}
			}
		}
		
		/// <summary>
		/// Sets the value of the specified meta data item. The value is escaped before
		/// setting it to ensure characters like ';' or '$' are not interpreted by MSBuild.
		/// </summary>
		public void SetEvaluatedMetadata<T>(string metadataName, T value)
		{
			SetEvaluatedMetadata(metadataName, GenericConverter.ToString(value));
		}
		
		/// <summary>
		/// Sets the value of the specified meta data item.
		/// Setting value to null or an empty string results in removing the metadata item.
		/// </summary>
		public void SetMetadata(string metadataName, string value)
		{
			if (string.IsNullOrEmpty(value)) {
				RemoveMetadata(metadataName);
			} else {
				lock (SyncRoot) {
					if (buildItem != null)
						buildItem.SetMetadata(metadataName, value);
					else
						virtualMetadata[metadataName] = value;
				}
			}
		}
		
		/// <summary>
		/// Removes the specified meta data item.
		/// </summary>
		public void RemoveMetadata(string metadataName)
		{
			lock (SyncRoot) {
				if (buildItem != null)
					buildItem.RemoveMetadata(metadataName);
				else
					virtualMetadata.Remove(metadataName);
			}
		}
		
		/// <summary>
		/// Gets the names of all existing meta data items on this project item. The resulting collection
		/// is a copy that will not be affected by future changes to the project item.
		/// </summary>
		[Browsable(false)]
		public IEnumerable<string> MetadataNames {
			get {
				lock (SyncRoot) {
					if (buildItem != null)
						return MSBuildInternals.GetCustomMetadataNames(buildItem);
					else
						return Linq.ToArray(virtualMetadata.Keys);
				}
			}
		}
		#endregion
		
		/// <summary>
		/// Copies all meta data from this item to the target item.
		/// </summary>
		public virtual void CopyMetadataTo(ProjectItem targetItem)
		{
			lock (SyncRoot) {
				lock (targetItem.SyncRoot) {
					if (this.buildItem != null && targetItem.buildItem != null) {
						this.buildItem.CopyCustomMetadataTo(targetItem.buildItem);
					} else {
						foreach (string name in this.MetadataNames) {
							targetItem.SetMetadata(name, this.GetMetadata(name));
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Clones this project item. Unless overridden, cloning works by cloning the underlying
		/// MSBuild item and creating a new project item for it.
		/// Using the default Clone() implementation requires that the item is has the Project
		/// property set - cloning a ProjectItem without a project will result in a NotSupportedException.
		/// </summary>
		public virtual ProjectItem Clone()
		{
			if (this.Project != null) {
				return CloneFor(this.Project);
			} else {
				throw new NotSupportedException();
			}
		}
		
		/// <summary>
		/// Clones this project item by cloning the underlying
		/// MSBuild item and creating a new project item in the target project for it.
		/// </summary>
		public ProjectItem CloneFor(IProject targetProject)
		{
			if (targetProject == null)
				throw new ArgumentNullException("project");
			
			// use CreateProjectItem to ensure the clone has the same class
			//  (derived from ProjectItem)
			ProjectItem copy = targetProject.CreateProjectItem(CloneBuildItem());
			// remove reference to cloned item, leaving an unbound project item
			copy.BuildItem = null;
			return copy;
			
		}
		
		BuildItem CloneBuildItem()
		{
			lock (SyncRoot) {
				if (buildItem != null) {
					return buildItem.Clone();
				} else {
					BuildItem dummyItem = new BuildItem(this.ItemType.ToString(), this.Include);
					foreach (string name in this.MetadataNames) {
						dummyItem.SetMetadata(name, this.GetMetadata(name));
					}
					return dummyItem;
				}
			}
		}
		
		object ICloneable.Clone()
		{
			return this.Clone();
		}
		
		/// <summary>
		/// Gets/Sets the full path of the file represented by "Include".
		/// For ProjectItems that are not assigned to any project, the getter returns the value of Include
		/// and the setter throws a NotSupportedException.
		/// </summary>
		[Browsable(false)]
		public virtual string FileName {
			get {
				if (project == null) {
					return this.Include;
				}
				string fileName = this.fileNameCache;
				if (fileName == null) {
					lock (SyncRoot) {
						fileName = Path.Combine(project.Directory, this.Include);
						try {
							if (Path.IsPathRooted(fileName)) {
								fileName = Path.GetFullPath(fileName);
							}
						} catch {}
						fileNameCache = fileName;
					}
				}
				return fileName;
			}
			set {
				if (project == null) {
					throw new NotSupportedException("Not supported for items without project.");
				}
				this.Include = FileUtility.GetRelativePath(project.Directory, value);
			}
		}
		
		bool disposed;
		
		public virtual void Dispose()
		{
			disposed = true;
		}
		
		[Browsable(false)]
		public bool IsDisposed {
			get { return disposed; }
		}
		
		public override string ToString()
		{
			return String.Format("[{0}: <{1} Include='{2}'>]",
			                     GetType().Name, this.ItemType.ItemName, this.Include);
		}
		
		public override void InformSetValue(LocalizedPropertyDescriptor localizedPropertyDescriptor, object component, object value)
		{
			base.InformSetValue(localizedPropertyDescriptor, component, value);
			if (project != null) {
				project.Save();
			}
		}
	}
}
