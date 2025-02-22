// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1943 $</version>
// </file>

using System;
using System.Collections.Generic;

namespace ICSharpCode.SharpDevelop.Dom
{
	/// <summary>
	/// Type parameter of a generic class/method.
	/// </summary>
	public class DefaultTypeParameter : ITypeParameter
	{
		public static readonly IList<ITypeParameter> EmptyTypeParameterList = new List<ITypeParameter>().AsReadOnly();
		
		string name;
		IMethod method;
		IClass targetClass;
		int index;
		List<IReturnType> constraints = new List<IReturnType>();
		
		public string Name {
			get {
				return name;
			}
		}
		
		public int Index {
			get {
				return index;
			}
		}
		
		public IMethod Method {
			get {
				return method;
			}
		}
		
		public IClass Class {
			get {
				return targetClass;
			}
		}
		
		public IList<IReturnType> Constraints {
			get {
				return constraints;
			}
		}
		
		public IList<IAttribute> Attributes {
			get {
				return DefaultAttribute.EmptyAttributeList;
			}
		}
		
		bool hasConstructableConstraint = false;
		bool hasReferenceTypeConstraint = false;
		bool hasValueTypeConstraint = false;
		
		/// <summary>
		/// Gets/Sets if the type parameter has the 'new()' constraint.
		/// </summary>
		public bool HasConstructableConstraint {
			get { return hasConstructableConstraint; }
			set { hasConstructableConstraint = value; }
		}
		
		/// <summary>
		/// Gets/Sets if the type parameter has the 'class' constraint.
		/// </summary>
		public bool HasReferenceTypeConstraint {
			get { return hasReferenceTypeConstraint; }
			set { hasReferenceTypeConstraint = value; }
		}
		
		/// <summary>
		/// Gets/Sets if the type parameter has the 'struct' constraint.
		/// </summary>
		public bool HasValueTypeConstraint {
			get { return hasValueTypeConstraint; }
			set { hasValueTypeConstraint = value; }
		}
		
		public DefaultTypeParameter(IMethod method, string name, int index)
		{
			this.method = method;
			this.targetClass = method.DeclaringType;
			this.name = name;
			this.index = index;
		}
		
		public DefaultTypeParameter(IMethod method, Type type)
		{
			this.method = method;
			this.targetClass = method.DeclaringType;
			this.name = type.Name;
			this.index = type.GenericParameterPosition;
		}
		
		public DefaultTypeParameter(IClass targetClass, string name, int index)
		{
			this.targetClass = targetClass;
			this.name = name;
			this.index = index;
		}
		
		public DefaultTypeParameter(IClass targetClass, Type type)
		{
			this.targetClass = targetClass;
			this.name = type.Name;
			this.index = type.GenericParameterPosition;
		}
		
		public override bool Equals(object obj)
		{
			DefaultTypeParameter tp = obj as DefaultTypeParameter;
			if (tp == null) return false;
			if (tp.index != index) return false;
			if (tp.name != name) return false;
			if (tp.hasConstructableConstraint != hasConstructableConstraint) return false;
			if (tp.hasReferenceTypeConstraint != hasReferenceTypeConstraint) return false;
			if (tp.hasValueTypeConstraint != hasValueTypeConstraint) return false;
			if (tp.method != method) {
				if (tp.method == null || method == null) return false;
				if (tp.method.FullyQualifiedName == method.FullyQualifiedName) return false;
			} else {
				if (tp.targetClass.FullyQualifiedName == targetClass.FullyQualifiedName) return false;
			}
			return true;
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public override string ToString()
		{
			return String.Format("[{0}: {1}]", GetType().Name, name);
		}
		
		public static DefaultClass GetDummyClassForTypeParameter(ITypeParameter p)
		{
			DefaultClass c = new DefaultClass(p.Class.CompilationUnit, p.Name);
			if (p.Method != null) {
				c.Region = new DomRegion(p.Method.Region.BeginLine, p.Method.Region.BeginColumn);
			} else {
				c.Region = new DomRegion(p.Class.Region.BeginLine, p.Class.Region.BeginColumn);
			}
			c.Modifiers = ModifierEnum.Public;
			if (p.HasValueTypeConstraint) {
				c.ClassType = ClassType.Struct;
			} else if (p.HasConstructableConstraint) {
				c.ClassType = ClassType.Class;
			} else {
				c.ClassType = ClassType.Interface;
			}
			return c;
		}
	}
}
