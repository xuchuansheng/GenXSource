﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1423 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ICSharpCode.SharpDevelop.Dom.CSharp
{
	public class CSharpAmbience : AbstractAmbience
	{
		public static IDictionary<string, string> TypeConversionTable {
			get { return ICSharpCode.NRefactory.Ast.TypeReference.PrimitiveTypesCSharpReverse; }
		}
		
		static CSharpAmbience instance = new CSharpAmbience();
		
		public static CSharpAmbience Instance {
			get { return instance; }
		}
		
		bool ModifierIsSet(ModifierEnum modifier, ModifierEnum query)
		{
			return (modifier & query) == query;
		}
		
		public override string Convert(ModifierEnum modifier)
		{
			if (ShowAccessibility) {
				if (ModifierIsSet(modifier, ModifierEnum.Public)) {
					return "public ";
				} else if (ModifierIsSet(modifier, ModifierEnum.Private)) {
					return "private ";
				} else if (ModifierIsSet(modifier, ModifierEnum.ProtectedAndInternal)) {
					return "protected internal ";
				} else if (ModifierIsSet(modifier, ModifierEnum.Internal)) {
					return "internal ";
				} else if (ModifierIsSet(modifier, ModifierEnum.Protected)) {
					return "protected ";
				}
			}
			
			return string.Empty;
		}
		
		string GetModifier(IDecoration decoration)
		{
			string ret = "";
			
			if (IncludeHTMLMarkup) {
				ret += "<i>";
			}
			
			if (decoration.IsStatic) {
				ret += "static ";
			} else if (decoration.IsSealed) {
				ret += "sealed ";
			} else if (decoration.IsVirtual) {
				ret += "virtual ";
			} else if (decoration.IsOverride) {
				ret += "override ";
			} else if (decoration.IsNew) {
				ret += "new ";
			}
			
			if (IncludeHTMLMarkup) {
				ret += "</i>";
			}
			
			return ret;
		}
		
		
		public override string Convert(IClass c)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append(Convert(c.Modifiers));
			
			if (IncludeHTMLMarkup) {
				builder.Append("<i>");
			}
			
			if (ShowModifiers) {
				if (c.IsSealed) {
					switch (c.ClassType) {
						case ClassType.Delegate:
						case ClassType.Struct:
						case ClassType.Enum:
							break;
							
						default:
							builder.Append("sealed ");
							break;
					}
				} else if (c.IsAbstract && c.ClassType != ClassType.Interface) {
					builder.Append("abstract ");
				}
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</i>");
			}
			
			if (ShowModifiers) {
				switch (c.ClassType) {
					case ClassType.Delegate:
						builder.Append("delegate");
						break;
					case ClassType.Class:
					case ClassType.Module:
						builder.Append("class");
						break;
					case ClassType.Struct:
						builder.Append("struct");
						break;
					case ClassType.Interface:
						builder.Append("interface");
						break;
					case ClassType.Enum:
						builder.Append("enum");
						break;
				}
				builder.Append(' ');
			}
			if (ShowReturnType && c.ClassType == ClassType.Delegate) {
				foreach(IMethod m in c.Methods) {
					if (m.Name != "Invoke") continue;
					
					builder.Append(Convert(m.ReturnType));
					builder.Append(' ');
				}
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("<b>");
			}
			
			if (UseFullyQualifiedMemberNames) {
				builder.Append(c.FullyQualifiedName);
			} else {
				builder.Append(c.Name);
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</b>");
			}
			if (c.TypeParameters.Count > 0) {
				builder.Append('<');
				for (int i = 0; i < c.TypeParameters.Count; ++i) {
					if (i > 0) builder.Append(", ");
					builder.Append(c.TypeParameters[i].Name);
				}
				builder.Append('>');
			}
			
			if (ShowReturnType && c.ClassType == ClassType.Delegate) {
				builder.Append(" (");
				if (IncludeHTMLMarkup) builder.Append("<br>");
				
				foreach(IMethod m in c.Methods) {
					if (m.Name != "Invoke") continue;
					
					for (int i = 0; i < m.Parameters.Count; ++i) {
						if (IncludeHTMLMarkup) builder.Append("&nbsp;&nbsp;&nbsp;");
						
						builder.Append(Convert(m.Parameters[i]));
						if (i + 1 < m.Parameters.Count) builder.Append(", ");
						
						if (IncludeHTMLMarkup) builder.Append("<br>");
					}
				}
				builder.Append(')');
				
			} else if (ShowInheritanceList) {
				if (c.BaseTypes.Count > 0) {
					builder.Append(" : ");
					for (int i = 0; i < c.BaseTypes.Count; ++i) {
						builder.Append(c.BaseTypes[i]);
						if (i + 1 < c.BaseTypes.Count) {
							builder.Append(", ");
						}
					}
				}
			}
			
			if (IncludeBodies) {
				builder.Append("\n{");
			}
			
			return builder.ToString();
		}
		
		public override string ConvertEnd(IClass c)
		{
			return "}";
		}
		
		public override string Convert(IField field)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append(Convert(field.Modifiers));
			
			if (IncludeHTMLMarkup) {
				builder.Append("<i>");
			}
			
			if (ShowModifiers) {
				if (field.IsConst) {
					builder.Append("const ");
				} else if (field.IsStatic) {
					builder.Append("static ");
				}
				
				if (field.IsReadonly) {
					builder.Append("readonly ");
				}
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</i>");
			}
			
			if (field.ReturnType != null  && ShowReturnType) {
				builder.Append(Convert(field.ReturnType));
				builder.Append(' ');
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("<b>");
			}
			
			if (UseFullyQualifiedMemberNames) {
				builder.Append(field.FullyQualifiedName);
			} else {
				builder.Append(field.Name);
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</b>");
			}
			
			if (IncludeBodies) builder.Append(";");
			
			return builder.ToString();
		}
		
		public override string Convert(IProperty property)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append(Convert(property.Modifiers));
			
			if (ShowModifiers) {
				builder.Append(GetModifier(property));
			}
			
			if (property.ReturnType != null && ShowReturnType) {
				builder.Append(Convert(property.ReturnType));
				builder.Append(' ');
			}
			
			if (property.IsIndexer) {
				if (property.DeclaringType != null) {
					if (UseFullyQualifiedMemberNames) {
						builder.Append(property.DeclaringType.FullyQualifiedName);
					} else {
						builder.Append(property.DeclaringType.Name);
					}
					builder.Append('.');
				}
				builder.Append("this");
			} else {
				if (IncludeHTMLMarkup) {
					builder.Append("<b>");
				}
				if (UseFullyQualifiedMemberNames) {
					builder.Append(property.FullyQualifiedName);
				} else {
					builder.Append(property.Name);
				}
				if (IncludeHTMLMarkup) {
					builder.Append("</b>");
				}
			}
			
			if (property.Parameters.Count > 0) {
				builder.Append(property.IsIndexer ? '[' : '(');
				if (IncludeHTMLMarkup) builder.Append("<br>");
				
				for (int i = 0; i < property.Parameters.Count; ++i) {
					if (IncludeHTMLMarkup) builder.Append("&nbsp;&nbsp;&nbsp;");
					builder.Append(Convert(property.Parameters[i]));
					if (i + 1 < property.Parameters.Count) {
						builder.Append(", ");
					}
					if (IncludeHTMLMarkup) builder.Append("<br>");
				}
				
				builder.Append(property.IsIndexer ? ']' : ')');
			}
			
			if (IncludeBodies) {
				builder.Append(" { ");
				
				if (property.CanGet) {
					builder.Append("get; ");
				}
				if (property.CanSet) {
					builder.Append("set; ");
				}
				
				builder.Append(" } ");
			}
			
			return builder.ToString();
		}
		
		public override string Convert(IEvent e)
		{
			StringBuilder builder = new StringBuilder();
			
			builder.Append(Convert(e.Modifiers));
			
			if (ShowModifiers) {
				builder.Append(GetModifier(e));
			}
			
			builder.Append("event ");
			
			if (e.ReturnType != null && ShowReturnType) {
				builder.Append(Convert(e.ReturnType));
				builder.Append(' ');
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("<b>");
			}
			
			if (UseFullyQualifiedMemberNames) {
				builder.Append(e.FullyQualifiedName);
			} else {
				builder.Append(e.Name);
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</b>");
			}
			
			if (IncludeBodies) builder.Append(";");
			
			return builder.ToString();
		}
		
		public override string Convert(IMethod m)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(Convert(m.Modifiers));
			
			if (ShowModifiers) {
				builder.Append(GetModifier(m));
			}
			
			if (m.ReturnType != null && ShowReturnType) {
				builder.Append(Convert(m.ReturnType));
				builder.Append(' ');
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("<b>");
			}
			
			if (m.IsConstructor) {
				if (m.DeclaringType != null) {
					builder.Append(m.DeclaringType.Name);
				} else {
					builder.Append(m.Name);
				}
			} else {
				if (UseFullyQualifiedMemberNames) {
					builder.Append(m.FullyQualifiedName);
				} else {
					builder.Append(m.Name);
				}
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</b>");
			}
			
			if (m.TypeParameters.Count > 0) {
				builder.Append('<');
				for (int i = 0; i < m.TypeParameters.Count; ++i) {
					if (i > 0) builder.Append(", ");
					builder.Append(m.TypeParameters[i].Name);
				}
				builder.Append('>');
			}
			
			builder.Append("(");
			if (IncludeHTMLMarkup) builder.Append("<br>");
			
			for (int i = 0; i < m.Parameters.Count; ++i) {
				if (IncludeHTMLMarkup) builder.Append("&nbsp;&nbsp;&nbsp;");
				builder.Append(Convert(m.Parameters[i]));
				if (i + 1 < m.Parameters.Count) {
					builder.Append(", ");
				}
				if (IncludeHTMLMarkup) builder.Append("<br>");
			}
			
			builder.Append(')');
			
			if (IncludeBodies) {
				if (m.DeclaringType != null) {
					if (m.DeclaringType.ClassType == ClassType.Interface) {
						builder.Append(";");
					} else {
						builder.Append(" {");
					}
				} else {
					builder.Append(" {");
				}
			}
			return builder.ToString();
		}
		
		public override string ConvertEnd(IMethod m)
		{
			return "}";
		}
		
		public override string Convert(IReturnType returnType)
		{
			if (returnType == null) {
				return String.Empty;
			}
			StringBuilder builder = new StringBuilder();
			
			string fullName = returnType.FullyQualifiedName;
			string shortName;
			if (fullName != null && TypeConversionTable.TryGetValue(fullName, out shortName)) {
				builder.Append(shortName);
			} else {
				if (UseFullyQualifiedNames) {
					builder.Append(fullName);
				} else {
					builder.Append(returnType.Name);
				}
			}
			
			UnpackNestedType(builder, returnType);
			
			return builder.ToString();
		}
		
		void UnpackNestedType(StringBuilder builder, IReturnType returnType)
		{
			if (returnType.IsArrayReturnType) {
				builder.Append('[');
				int dimensions = returnType.CastToArrayReturnType().ArrayDimensions;
				for (int i = 1; i < dimensions; ++i) {
					builder.Append(',');
				}
				builder.Append(']');
				UnpackNestedType(builder, returnType.CastToArrayReturnType().ArrayElementType);
			} else if (returnType.IsConstructedReturnType) {
				UnpackNestedType(builder, returnType.CastToConstructedReturnType().UnboundType);
				builder.Append('<');
				IList<IReturnType> ta = returnType.CastToConstructedReturnType().TypeArguments;
				for (int i = 0; i < ta.Count; ++i) {
					if (i > 0) builder.Append(", ");
					builder.Append(Convert(ta[i]));
				}
				builder.Append('>');
			}
		}
		
		public override string Convert(IParameter param)
		{
			StringBuilder builder = new StringBuilder();
			
			if (IncludeHTMLMarkup) {
				builder.Append("<i>");
			}
			
			if (param.IsRef) {
				builder.Append("ref ");
			} else if (param.IsOut) {
				builder.Append("out ");
			} else if (param.IsParams) {
				builder.Append("params ");
			}
			
			if (IncludeHTMLMarkup) {
				builder.Append("</i>");
			}
			
			builder.Append(Convert(param.ReturnType));
			
			if (ShowParameterNames) {
				builder.Append(' ');
				builder.Append(param.Name);
			}
			return builder.ToString();
		}
		
		public override string WrapAttribute(string attribute)
		{
			return "[" + attribute + "]";
		}
		
		public override string WrapComment(string comment)
		{
			return "// " + comment;
		}

		public override string GetIntrinsicTypeName(string dotNetTypeName)
		{
			string shortName;
			if (TypeConversionTable.TryGetValue(dotNetTypeName, out shortName)) {
				return shortName;
			}
			return dotNetTypeName;
		}
		
	}
}
