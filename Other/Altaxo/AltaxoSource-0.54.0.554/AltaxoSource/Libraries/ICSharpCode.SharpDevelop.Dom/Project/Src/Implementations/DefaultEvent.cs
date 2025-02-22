﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2066 $</version>
// </file>

using System;

namespace ICSharpCode.SharpDevelop.Dom
{
	public class DefaultEvent : AbstractMember, IEvent
	{
		IMethod  addMethod;
		IMethod  removeMethod;
		IMethod  raiseMethod;
		
		public override string DocumentationTag {
			get {
				return "E:" + this.DotNetName;
			}
		}
		
		public override IMember Clone()
		{
			DefaultEvent de = new DefaultEvent(Name, ReturnType, Modifiers, Region, BodyRegion, DeclaringType);
			foreach (ExplicitInterfaceImplementation eii in InterfaceImplementations) {
				de.InterfaceImplementations.Add(eii.Clone());
			}
			return de;
		}
		
		public DefaultEvent(IClass declaringType, string name) : base(declaringType, name)
		{
		}
		
		public DefaultEvent(string name, IReturnType type, ModifierEnum m, DomRegion region, DomRegion bodyRegion, IClass declaringType) : base(declaringType, name)
		{
			this.ReturnType = type;
			this.Region     = region;
			this.BodyRegion = bodyRegion;
			Modifiers       = (ModifierEnum)m;
			if (Modifiers == ModifierEnum.None) {
				Modifiers = ModifierEnum.Private;
			}
		}
		
		public virtual int CompareTo(IEvent value)
		{
			int cmp;
			
			if(0 != (cmp = base.CompareTo((IDecoration)value)))
				return cmp;
			
			if (FullyQualifiedName != null) {
				return FullyQualifiedName.CompareTo(value.FullyQualifiedName);
			}
			
			return 0;
		}
		
		int IComparable.CompareTo(object value)
		{
			return CompareTo((IEvent)value);
		}
		
		public virtual IMethod AddMethod {
			get {
				return addMethod;
			}
			protected set {
				addMethod = value;
			}
		}
		
		public virtual IMethod RemoveMethod {
			get {
				return removeMethod;
			}
			protected set {
				removeMethod = value;
			}
		}
		
		public virtual IMethod RaiseMethod {
			get {
				return raiseMethod;
			}
			protected set {
				raiseMethod = value;
			}
		}
	}
}
