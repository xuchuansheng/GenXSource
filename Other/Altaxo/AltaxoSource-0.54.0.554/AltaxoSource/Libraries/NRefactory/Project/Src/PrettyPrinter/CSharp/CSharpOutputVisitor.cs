// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 2198 $</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.CSharp;

namespace ICSharpCode.NRefactory.PrettyPrinter
{
	public sealed class CSharpOutputVisitor : IOutputAstVisitor
	{
		Errors                errors             = new Errors();
		CSharpOutputFormatter outputFormatter;
		PrettyPrintOptions    prettyPrintOptions = new PrettyPrintOptions();
		NodeTracker           nodeTracker;
		Stack<WithStatement> withExpressionStack = new Stack<WithStatement>();
		bool printFullSystemType;
		
		public string Text {
			get {
				return outputFormatter.Text;
			}
		}
		
		public Errors Errors {
			get {
				return errors;
			}
		}
		
		AbstractPrettyPrintOptions IOutputAstVisitor.Options {
			get { return prettyPrintOptions; }
		}
		
		public PrettyPrintOptions Options {
			get { return prettyPrintOptions; }
		}
		
		public IOutputFormatter OutputFormatter {
			get {
				return outputFormatter;
			}
		}
		
		public NodeTracker NodeTracker {
			get {
				return nodeTracker;
			}
		}
		
		public CSharpOutputVisitor()
		{
			outputFormatter = new CSharpOutputFormatter(prettyPrintOptions);
			nodeTracker     = new NodeTracker(this);
		}
		
		void Error(INode node, string message)
		{
			outputFormatter.PrintText(" // ERROR: " + message + Environment.NewLine);
			errors.Error(node.StartLocation.Y, node.StartLocation.X, message);
		}
		
		void NotSupported(INode node)
		{
			Error(node, "Not supported in C#: " + node.GetType().Name);
		}
		
		#region ICSharpCode.NRefactory.Parser.IASTVisitor interface implementation
		public object VisitCompilationUnit(CompilationUnit compilationUnit, object data)
		{
			nodeTracker.TrackedVisitChildren(compilationUnit, data);
			outputFormatter.EndFile();
			return null;
		}
		
		/// <summary>
		/// Converts type name to primitive type name. Returns null if typeString is not
		/// a primitive type.
		/// </summary>
		static string ConvertTypeString(string typeString)
		{
			string primitiveType;
			if (TypeReference.PrimitiveTypesCSharpReverse.TryGetValue(typeString, out primitiveType))
				return primitiveType;
			else
				return typeString;
		}
		
		void PrintTemplates(List<TemplateDefinition> templates)
		{
			if (templates.Count == 0) return;
			outputFormatter.PrintToken(Tokens.LessThan);
			for (int i = 0; i < templates.Count; i++) {
				if (i > 0) PrintFormattedComma();
				outputFormatter.PrintIdentifier(templates[i].Name);
			}
			outputFormatter.PrintToken(Tokens.GreaterThan);
		}
		
		public object VisitTypeReference(TypeReference typeReference, object data)
		{
			if (typeReference == TypeReference.ClassConstraint) {
				outputFormatter.PrintToken(Tokens.Class);
			} else if (typeReference == TypeReference.StructConstraint) {
				outputFormatter.PrintToken(Tokens.Struct);
			} else if (typeReference == TypeReference.NewConstraint) {
				outputFormatter.PrintToken(Tokens.New);
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
			} else {
				PrintTypeReferenceWithoutArray(typeReference);
				if (typeReference.IsArrayType) {
					PrintArrayRank(typeReference.RankSpecifier, 0);
				}
			}
			return null;
		}
		
		void PrintArrayRank(int[] rankSpecifier, int startRankIndex)
		{
			for (int i = startRankIndex; i < rankSpecifier.Length; ++i) {
				outputFormatter.PrintToken(Tokens.OpenSquareBracket);
				if (this.prettyPrintOptions.SpacesWithinBrackets) {
					outputFormatter.Space();
				}
				for (int j = 0; j < rankSpecifier[i]; ++j) {
					outputFormatter.PrintToken(Tokens.Comma);
				}
				if (this.prettyPrintOptions.SpacesWithinBrackets) {
					outputFormatter.Space();
				}
				outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			}
		}
		
		void PrintTypeReferenceWithoutArray(TypeReference typeReference)
		{
			if (typeReference.IsGlobal) {
				outputFormatter.PrintText("global::");
			}
			if (typeReference.Type == null || typeReference.Type.Length == 0) {
				outputFormatter.PrintText("void");
			} else if (typeReference.SystemType == "System.Nullable" && typeReference.GenericTypes != null
			           && typeReference.GenericTypes.Count == 1 && !typeReference.IsGlobal)
			{
				nodeTracker.TrackedVisit(typeReference.GenericTypes[0], null);
				outputFormatter.PrintText("?");
			} else {
				if (typeReference.SystemType.Length > 0) {
					if (printFullSystemType || typeReference.IsGlobal) {
						outputFormatter.PrintIdentifier(typeReference.SystemType);
					} else {
						outputFormatter.PrintText(ConvertTypeString(typeReference.SystemType));
					}
				} else {
					outputFormatter.PrintText(typeReference.Type);
				}
				if (typeReference.GenericTypes != null && typeReference.GenericTypes.Count > 0) {
					outputFormatter.PrintToken(Tokens.LessThan);
					AppendCommaSeparatedList(typeReference.GenericTypes);
					outputFormatter.PrintToken(Tokens.GreaterThan);
				}
			}
			for (int i = 0; i < typeReference.PointerNestingLevel; ++i) {
				outputFormatter.PrintToken(Tokens.Times);
			}
		}
		
		public object VisitInnerClassTypeReference(InnerClassTypeReference innerClassTypeReference, object data)
		{
			nodeTracker.TrackedVisit(innerClassTypeReference.BaseType, data);
			outputFormatter.PrintToken(Tokens.Dot);
			return VisitTypeReference((TypeReference)innerClassTypeReference, data);
		}
		
		#region Global scope
		void VisitAttributes(ICollection attributes, object data)
		{
			if (attributes == null || attributes.Count <= 0) {
				return;
			}
			foreach (AttributeSection section in attributes) {
				nodeTracker.TrackedVisit(section, data);
			}
		}
		void PrintFormattedComma()
		{
			if (this.prettyPrintOptions.SpacesBeforeComma) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.Comma);
			if (this.prettyPrintOptions.SpacesAfterComma) {
				outputFormatter.Space();
			}
		}
		
		public object VisitAttributeSection(AttributeSection attributeSection, object data)
		{
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.OpenSquareBracket);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			if (!string.IsNullOrEmpty(attributeSection.AttributeTarget)) {
				outputFormatter.PrintText(attributeSection.AttributeTarget);
				outputFormatter.PrintToken(Tokens.Colon);
				outputFormatter.Space();
			}
			Debug.Assert(attributeSection.Attributes != null);
			for (int j = 0; j < attributeSection.Attributes.Count; ++j) {
				nodeTracker.TrackedVisit((INode)attributeSection.Attributes[j], data);
				if (j + 1 < attributeSection.Attributes.Count) {
					PrintFormattedComma();
				}
			}
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitAttribute(ICSharpCode.NRefactory.Ast.Attribute attribute, object data)
		{
			outputFormatter.PrintIdentifier(attribute.Name);
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			this.AppendCommaSeparatedList(attribute.PositionalArguments);
			
			if (attribute.NamedArguments != null && attribute.NamedArguments.Count > 0) {
				if (attribute.PositionalArguments.Count > 0) {
					PrintFormattedComma();
				}
				for (int i = 0; i < attribute.NamedArguments.Count; ++i) {
					nodeTracker.TrackedVisit((INode)attribute.NamedArguments[i], data);
					if (i + 1 < attribute.NamedArguments.Count) {
						PrintFormattedComma();
					}
				}
			}
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitNamedArgumentExpression(NamedArgumentExpression namedArgumentExpression, object data)
		{
			outputFormatter.PrintIdentifier(namedArgumentExpression.Name);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Assign);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(namedArgumentExpression.Expression, data);
			return null;
		}
		
		public object VisitUsing(Using @using, object data)
		{
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.Using);
			outputFormatter.Space();
			
			outputFormatter.PrintIdentifier(@using.Name);
			
			if (@using.IsAlias) {
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Assign);
				outputFormatter.Space();
				printFullSystemType = true;
				nodeTracker.TrackedVisit(@using.Alias, data);
				printFullSystemType = false;
			}
			
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitUsingDeclaration(UsingDeclaration usingDeclaration, object data)
		{
			foreach (Using u in usingDeclaration.Usings) {
				nodeTracker.TrackedVisit(u, data);
			}
			return null;
		}
		
		public object VisitNamespaceDeclaration(NamespaceDeclaration namespaceDeclaration, object data)
		{
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.Namespace);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(namespaceDeclaration.Name);
			
			outputFormatter.BeginBrace(this.prettyPrintOptions.NamespaceBraceStyle);
			
			nodeTracker.TrackedVisitChildren(namespaceDeclaration, data);
			
			outputFormatter.EndBrace();
			
			return null;
		}
		
		
		void OutputEnumMembers(TypeDeclaration typeDeclaration, object data)
		{
			for (int i = 0; i < typeDeclaration.Children.Count; i++) {
				FieldDeclaration fieldDeclaration = (FieldDeclaration)typeDeclaration.Children[i];
				nodeTracker.BeginNode(fieldDeclaration);
				VariableDeclaration f = (VariableDeclaration)fieldDeclaration.Fields[0];
				VisitAttributes(fieldDeclaration.Attributes, data);
				outputFormatter.Indent();
				outputFormatter.PrintIdentifier(f.Name);
				if (f.Initializer != null && !f.Initializer.IsNull) {
					outputFormatter.Space();
					outputFormatter.PrintToken(Tokens.Assign);
					outputFormatter.Space();
					nodeTracker.TrackedVisit(f.Initializer, data);
				}
				if (i < typeDeclaration.Children.Count - 1) {
					outputFormatter.PrintToken(Tokens.Comma);
				}
				outputFormatter.NewLine();
				nodeTracker.EndNode(fieldDeclaration);
			}
		}
		
		TypeDeclaration currentType = null;
		
		public object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
		{
			VisitAttributes(typeDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(typeDeclaration.Modifier);
			switch (typeDeclaration.Type) {
				case ClassType.Enum:
					outputFormatter.PrintToken(Tokens.Enum);
					break;
				case ClassType.Interface:
					outputFormatter.PrintToken(Tokens.Interface);
					break;
				case ClassType.Struct:
					outputFormatter.PrintToken(Tokens.Struct);
					break;
				default:
					outputFormatter.PrintToken(Tokens.Class);
					break;
			}
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(typeDeclaration.Name);
			
			PrintTemplates(typeDeclaration.Templates);
			
			if (typeDeclaration.BaseTypes != null && typeDeclaration.BaseTypes.Count > 0) {
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Colon);
				outputFormatter.Space();
				for (int i = 0; i < typeDeclaration.BaseTypes.Count; ++i) {
					if (i > 0) {
						PrintFormattedComma();
					}
					nodeTracker.TrackedVisit(typeDeclaration.BaseTypes[i], data);
				}
			}
			
			foreach (TemplateDefinition templateDefinition in typeDeclaration.Templates) {
				nodeTracker.TrackedVisit(templateDefinition, data);
			}
			
			switch (typeDeclaration.Type) {
				case ClassType.Enum:
					outputFormatter.BeginBrace(this.prettyPrintOptions.EnumBraceStyle);
					break;
				case ClassType.Interface:
					outputFormatter.BeginBrace(this.prettyPrintOptions.InterfaceBraceStyle);
					break;
				case ClassType.Struct:
					outputFormatter.BeginBrace(this.prettyPrintOptions.StructBraceStyle);
					break;
				default:
					outputFormatter.BeginBrace(this.prettyPrintOptions.ClassBraceStyle);
					break;
			}
			
			TypeDeclaration oldType = currentType;
			currentType = typeDeclaration;
			if (typeDeclaration.Type == ClassType.Enum) {
				OutputEnumMembers(typeDeclaration, data);
			} else {
				nodeTracker.TrackedVisitChildren(typeDeclaration, data);
			}
			currentType = oldType;
			outputFormatter.EndBrace();
			
			return null;
		}
		
		public object VisitTemplateDefinition(TemplateDefinition templateDefinition, object data)
		{
			if (templateDefinition.Bases.Count == 0)
				return null;
			
			outputFormatter.Space();
			outputFormatter.PrintText("where");
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(templateDefinition.Name);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Colon);
			outputFormatter.Space();
			
			for (int i = 0; i < templateDefinition.Bases.Count; ++i) {
				nodeTracker.TrackedVisit(templateDefinition.Bases[i], data);
				if (i + 1 < templateDefinition.Bases.Count) {
					PrintFormattedComma();
				}
			}
			return null;
		}
		
		public object VisitDelegateDeclaration(DelegateDeclaration delegateDeclaration, object data)
		{
			VisitAttributes(delegateDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(delegateDeclaration.Modifier);
			outputFormatter.PrintToken(Tokens.Delegate);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(delegateDeclaration.ReturnType, data);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(delegateDeclaration.Name);
			PrintTemplates(delegateDeclaration.Templates);
			if (prettyPrintOptions.BeforeDelegateDeclarationParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(delegateDeclaration.Parameters);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			foreach (TemplateDefinition templateDefinition in delegateDeclaration.Templates) {
				nodeTracker.TrackedVisit(templateDefinition, data);
			}
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitOptionDeclaration(OptionDeclaration optionDeclaration, object data)
		{
			NotSupported(optionDeclaration);
			return null;
		}
		#endregion
		
		#region Type level
		public object VisitFieldDeclaration(FieldDeclaration fieldDeclaration, object data)
		{
			if (!fieldDeclaration.TypeReference.IsNull) {
				VisitAttributes(fieldDeclaration.Attributes, data);
				outputFormatter.Indent();
				OutputModifier(fieldDeclaration.Modifier);
				nodeTracker.TrackedVisit(fieldDeclaration.TypeReference, data);
				outputFormatter.Space();
				AppendCommaSeparatedList(fieldDeclaration.Fields);
				outputFormatter.PrintToken(Tokens.Semicolon);
				outputFormatter.NewLine();
			} else {
				for (int i = 0; i < fieldDeclaration.Fields.Count; i++) {
					VisitAttributes(fieldDeclaration.Attributes, data);
					outputFormatter.Indent();
					OutputModifier(fieldDeclaration.Modifier);
					nodeTracker.TrackedVisit(fieldDeclaration.GetTypeForField(i), data);
					outputFormatter.Space();
					nodeTracker.TrackedVisit(fieldDeclaration.Fields[i], data);
					outputFormatter.PrintToken(Tokens.Semicolon);
					outputFormatter.NewLine();
				}
			}
			return null;
		}
		
		public object VisitVariableDeclaration(VariableDeclaration variableDeclaration, object data)
		{
			outputFormatter.PrintIdentifier(variableDeclaration.Name);
			if (!variableDeclaration.FixedArrayInitialization.IsNull) {
				outputFormatter.PrintToken(Tokens.OpenSquareBracket);
				nodeTracker.TrackedVisit(variableDeclaration.FixedArrayInitialization, data);
				outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			}
			if (!variableDeclaration.Initializer.IsNull) {
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Assign);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(variableDeclaration.Initializer, data);
			}
			return null;
		}
		
		public object VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration, object data)
		{
			VisitAttributes(propertyDeclaration.Attributes, data);
			outputFormatter.Indent();
			propertyDeclaration.Modifier &= ~Modifiers.ReadOnly;
			OutputModifier(propertyDeclaration.Modifier);
			nodeTracker.TrackedVisit(propertyDeclaration.TypeReference, data);
			outputFormatter.Space();
			if (propertyDeclaration.InterfaceImplementations.Count > 0) {
				nodeTracker.TrackedVisit(propertyDeclaration.InterfaceImplementations[0].InterfaceType, data);
				outputFormatter.PrintToken(Tokens.Dot);
			}
			outputFormatter.PrintIdentifier(propertyDeclaration.Name);
			
			outputFormatter.BeginBrace(this.prettyPrintOptions.PropertyBraceStyle);
			
			nodeTracker.TrackedVisit(propertyDeclaration.GetRegion, data);
			nodeTracker.TrackedVisit(propertyDeclaration.SetRegion, data);
			
			outputFormatter.EndBrace();
			return null;
		}
		
		public object VisitPropertyGetRegion(PropertyGetRegion propertyGetRegion, object data)
		{
			this.VisitAttributes(propertyGetRegion.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(propertyGetRegion.Modifier);
			outputFormatter.PrintText("get");
			OutputBlockAllowInline(propertyGetRegion.Block, prettyPrintOptions.PropertyGetBraceStyle);
			return null;
		}
		
		public object VisitPropertySetRegion(PropertySetRegion propertySetRegion, object data)
		{
			this.VisitAttributes(propertySetRegion.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(propertySetRegion.Modifier);
			outputFormatter.PrintText("set");
			OutputBlockAllowInline(propertySetRegion.Block, prettyPrintOptions.PropertySetBraceStyle);
			return null;
		}
		
		public object VisitEventDeclaration(EventDeclaration eventDeclaration, object data)
		{
			VisitAttributes(eventDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(eventDeclaration.Modifier);
			outputFormatter.PrintToken(Tokens.Event);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(eventDeclaration.TypeReference, data);
			outputFormatter.Space();
			
			if (eventDeclaration.InterfaceImplementations.Count > 0) {
				nodeTracker.TrackedVisit(eventDeclaration.InterfaceImplementations[0].InterfaceType, data);
				outputFormatter.PrintToken(Tokens.Dot);
			}
			
			outputFormatter.PrintIdentifier(eventDeclaration.Name);
			
			if (!eventDeclaration.Initializer.IsNull) {
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Assign);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(eventDeclaration.Initializer, data);
			}
			
			if (eventDeclaration.AddRegion.IsNull && eventDeclaration.RemoveRegion.IsNull) {
				outputFormatter.PrintToken(Tokens.Semicolon);
				outputFormatter.NewLine();
			} else {
				outputFormatter.BeginBrace(this.prettyPrintOptions.PropertyBraceStyle);
				nodeTracker.TrackedVisit(eventDeclaration.AddRegion, data);
				nodeTracker.TrackedVisit(eventDeclaration.RemoveRegion, data);
				outputFormatter.EndBrace();
			}
			return null;
		}
		
		public object VisitEventAddRegion(EventAddRegion eventAddRegion, object data)
		{
			VisitAttributes(eventAddRegion.Attributes, data);
			outputFormatter.Indent();
			outputFormatter.PrintText("add");
			OutputBlockAllowInline(eventAddRegion.Block, prettyPrintOptions.EventAddBraceStyle);
			return null;
		}
		
		public object VisitEventRemoveRegion(EventRemoveRegion eventRemoveRegion, object data)
		{
			VisitAttributes(eventRemoveRegion.Attributes, data);
			outputFormatter.Indent();
			outputFormatter.PrintText("remove");
			OutputBlockAllowInline(eventRemoveRegion.Block, prettyPrintOptions.EventRemoveBraceStyle);
			return null;
		}
		
		public object VisitEventRaiseRegion(EventRaiseRegion eventRaiseRegion, object data)
		{
			// VB.NET only
			NotSupported(eventRaiseRegion);
			return null;
		}
		
		public object VisitParameterDeclarationExpression(ParameterDeclarationExpression parameterDeclarationExpression, object data)
		{
			VisitAttributes(parameterDeclarationExpression.Attributes, data);
			OutputModifier(parameterDeclarationExpression.ParamModifier, parameterDeclarationExpression);
			nodeTracker.TrackedVisit(parameterDeclarationExpression.TypeReference, data);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(parameterDeclarationExpression.ParameterName);
			return null;
		}
		
		public object VisitMethodDeclaration(MethodDeclaration methodDeclaration, object data)
		{
			VisitAttributes(methodDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(methodDeclaration.Modifier);
			nodeTracker.TrackedVisit(methodDeclaration.TypeReference, data);
			outputFormatter.Space();
			if (methodDeclaration.InterfaceImplementations.Count > 0) {
				nodeTracker.TrackedVisit(methodDeclaration.InterfaceImplementations[0].InterfaceType, data);
				outputFormatter.PrintToken(Tokens.Dot);
			}
			outputFormatter.PrintIdentifier(methodDeclaration.Name);
			
			PrintMethodDeclaration(methodDeclaration);
			return null;
		}
		
		void PrintMethodDeclaration(MethodDeclaration methodDeclaration)
		{
			PrintTemplates(methodDeclaration.Templates);
			if (prettyPrintOptions.BeforeMethodDeclarationParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(methodDeclaration.Parameters);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			foreach (TemplateDefinition templateDefinition in methodDeclaration.Templates) {
				nodeTracker.TrackedVisit(templateDefinition, null);
			}
			OutputBlock(methodDeclaration.Body, this.prettyPrintOptions.MethodBraceStyle);
		}
		
		public object VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration, object data)
		{
			VisitAttributes(operatorDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(operatorDeclaration.Modifier);
			
			if (operatorDeclaration.IsConversionOperator) {
				if (operatorDeclaration.ConversionType == ConversionType.Implicit) {
					outputFormatter.PrintToken(Tokens.Implicit);
				} else {
					outputFormatter.PrintToken(Tokens.Explicit);
				}
			} else {
				nodeTracker.TrackedVisit(operatorDeclaration.TypeReference, data);
			}
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Operator);
			outputFormatter.Space();
			
			if (operatorDeclaration.IsConversionOperator) {
				nodeTracker.TrackedVisit(operatorDeclaration.TypeReference, data);
			} else {
				switch (operatorDeclaration.OverloadableOperator) {
					case OverloadableOperatorType.Add:
						outputFormatter.PrintToken(Tokens.Plus);
						break;
					case OverloadableOperatorType.BitNot:
						outputFormatter.PrintToken(Tokens.BitwiseComplement);
						break;
					case OverloadableOperatorType.BitwiseAnd:
						outputFormatter.PrintToken(Tokens.BitwiseAnd);
						break;
					case OverloadableOperatorType.BitwiseOr:
						outputFormatter.PrintToken(Tokens.BitwiseOr);
						break;
					case OverloadableOperatorType.Concat:
						outputFormatter.PrintToken(Tokens.Plus);
						break;
					case OverloadableOperatorType.Decrement:
						outputFormatter.PrintToken(Tokens.Decrement);
						break;
					case OverloadableOperatorType.Divide:
					case OverloadableOperatorType.DivideInteger:
						outputFormatter.PrintToken(Tokens.Div);
						break;
					case OverloadableOperatorType.Equality:
						outputFormatter.PrintToken(Tokens.Equal);
						break;
					case OverloadableOperatorType.ExclusiveOr:
						outputFormatter.PrintToken(Tokens.Xor);
						break;
					case OverloadableOperatorType.GreaterThan:
						outputFormatter.PrintToken(Tokens.GreaterThan);
						break;
					case OverloadableOperatorType.GreaterThanOrEqual:
						outputFormatter.PrintToken(Tokens.GreaterEqual);
						break;
					case OverloadableOperatorType.Increment:
						outputFormatter.PrintToken(Tokens.Increment);
						break;
					case OverloadableOperatorType.InEquality:
						outputFormatter.PrintToken(Tokens.NotEqual);
						break;
					case OverloadableOperatorType.IsTrue:
						outputFormatter.PrintToken(Tokens.True);
						break;
					case OverloadableOperatorType.IsFalse:
						outputFormatter.PrintToken(Tokens.False);
						break;
					case OverloadableOperatorType.LessThan:
						outputFormatter.PrintToken(Tokens.LessThan);
						break;
					case OverloadableOperatorType.LessThanOrEqual:
						outputFormatter.PrintToken(Tokens.LessEqual);
						break;
					case OverloadableOperatorType.Like:
						outputFormatter.PrintText("Like");
						break;
					case OverloadableOperatorType.Modulus:
						outputFormatter.PrintToken(Tokens.Mod);
						break;
					case OverloadableOperatorType.Multiply:
						outputFormatter.PrintToken(Tokens.Times);
						break;
					case OverloadableOperatorType.Not:
						outputFormatter.PrintToken(Tokens.Not);
						break;
					case OverloadableOperatorType.Power:
						outputFormatter.PrintText("Power");
						break;
					case OverloadableOperatorType.ShiftLeft:
						outputFormatter.PrintToken(Tokens.ShiftLeft);
						break;
					case OverloadableOperatorType.ShiftRight:
						outputFormatter.PrintToken(Tokens.GreaterThan);
						outputFormatter.PrintToken(Tokens.GreaterThan);
						break;
					case OverloadableOperatorType.Subtract:
						outputFormatter.PrintToken(Tokens.Minus);
						break;
					default:
						Error(operatorDeclaration, operatorDeclaration.OverloadableOperator.ToString() + " is not supported as overloadable operator");
						break;
				}
			}
			
			PrintMethodDeclaration(operatorDeclaration);
			return null;
		}
		
		public object VisitInterfaceImplementation(InterfaceImplementation interfaceImplementation, object data)
		{
			throw new InvalidOperationException();
		}
		
		public object VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data)
		{
			VisitAttributes(constructorDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(constructorDeclaration.Modifier);
			if (currentType != null) {
				outputFormatter.PrintIdentifier(currentType.Name);
			} else {
				outputFormatter.PrintIdentifier(constructorDeclaration.Name);
			}
			if (prettyPrintOptions.BeforeConstructorDeclarationParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(constructorDeclaration.Parameters);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			nodeTracker.TrackedVisit(constructorDeclaration.ConstructorInitializer, data);
			OutputBlock(constructorDeclaration.Body, this.prettyPrintOptions.ConstructorBraceStyle);
			return null;
		}
		
		public object VisitConstructorInitializer(ConstructorInitializer constructorInitializer, object data)
		{
			if (constructorInitializer.IsNull) {
				return null;
			}
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Colon);
			outputFormatter.Space();
			if (constructorInitializer.ConstructorInitializerType == ConstructorInitializerType.Base) {
				outputFormatter.PrintToken(Tokens.Base);
			} else {
				outputFormatter.PrintToken(Tokens.This);
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(constructorInitializer.Arguments);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitIndexerDeclaration(IndexerDeclaration indexerDeclaration, object data)
		{
			VisitAttributes(indexerDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(indexerDeclaration.Modifier);
			nodeTracker.TrackedVisit(indexerDeclaration.TypeReference, data);
			outputFormatter.Space();
			if (indexerDeclaration.InterfaceImplementations.Count > 0) {
				nodeTracker.TrackedVisit(indexerDeclaration.InterfaceImplementations[0].InterfaceType, data);
				outputFormatter.PrintToken(Tokens.Dot);
			}
			outputFormatter.PrintToken(Tokens.This);
			outputFormatter.PrintToken(Tokens.OpenSquareBracket);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			AppendCommaSeparatedList(indexerDeclaration.Parameters);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			outputFormatter.NewLine();
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.OpenCurlyBrace);
			outputFormatter.NewLine();
			++outputFormatter.IndentationLevel;
			nodeTracker.TrackedVisit(indexerDeclaration.GetRegion, data);
			nodeTracker.TrackedVisit(indexerDeclaration.SetRegion, data);
			--outputFormatter.IndentationLevel;
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.CloseCurlyBrace);
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitDestructorDeclaration(DestructorDeclaration destructorDeclaration, object data)
		{
			VisitAttributes(destructorDeclaration.Attributes, data);
			outputFormatter.Indent();
			OutputModifier(destructorDeclaration.Modifier);
			outputFormatter.PrintToken(Tokens.BitwiseComplement);
			if (currentType != null)
				outputFormatter.PrintIdentifier(currentType.Name);
			else
				outputFormatter.PrintIdentifier(destructorDeclaration.Name);
			if (prettyPrintOptions.BeforeConstructorDeclarationParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			OutputBlock(destructorDeclaration.Body, this.prettyPrintOptions.DestructorBraceStyle);
			return null;
		}
		
		public object VisitDeclareDeclaration(DeclareDeclaration declareDeclaration, object data)
		{
			NotSupported(declareDeclaration);
			return null;
		}
		#endregion
		
		#region Statements
		
		void OutputBlock(BlockStatement blockStatement, BraceStyle braceStyle)
		{
			nodeTracker.BeginNode(blockStatement);
			if (blockStatement.IsNull) {
				outputFormatter.PrintToken(Tokens.Semicolon);
				outputFormatter.NewLine();
			} else {
				outputFormatter.BeginBrace(braceStyle);
				foreach (Statement stmt in blockStatement.Children) {
					outputFormatter.Indent();
					nodeTracker.TrackedVisit(stmt, null);
					if (!outputFormatter.LastCharacterIsNewLine)
						outputFormatter.NewLine();
				}
				outputFormatter.EndBrace();
			}
			nodeTracker.EndNode(blockStatement);
		}
		
		void OutputBlockAllowInline(BlockStatement blockStatement, BraceStyle braceStyle)
		{
			OutputBlockAllowInline(blockStatement, braceStyle, true);
		}
		
		void OutputBlockAllowInline(BlockStatement blockStatement, BraceStyle braceStyle, bool useNewLine)
		{
			if (!blockStatement.IsNull
			    && (
			    	blockStatement.Children.Count == 0
			    	|| blockStatement.Children.Count == 1
			    	&& (blockStatement.Children[0] is ExpressionStatement
			    	    || blockStatement.Children[0] is ReturnStatement
			    	   )))
			{
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.OpenCurlyBrace);
				outputFormatter.Space();
				if (blockStatement.Children.Count != 0) {
					bool doIndent  = outputFormatter.DoIndent;
					bool doNewLine = outputFormatter.DoNewLine;
					outputFormatter.DoIndent  = false;
					outputFormatter.DoNewLine = false;
					
					nodeTracker.TrackedVisit(blockStatement.Children[0], null);
					
					outputFormatter.DoIndent  = doIndent;
					outputFormatter.DoNewLine = doNewLine;
					
					outputFormatter.Space();
				}
				outputFormatter.PrintToken(Tokens.CloseCurlyBrace);
				if (useNewLine) {
					outputFormatter.NewLine();
				}
			} else {
				OutputBlock(blockStatement, braceStyle);
			}
		}
		
		public object VisitBlockStatement(BlockStatement blockStatement, object data)
		{
			if (data is BraceStyle)
				OutputBlock(blockStatement, (BraceStyle)data);
			else
				OutputBlock(blockStatement, BraceStyle.NextLine);
			return null;
		}
		
		public object VisitAddHandlerStatement(AddHandlerStatement addHandlerStatement, object data)
		{
			nodeTracker.TrackedVisit(addHandlerStatement.EventExpression, data);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.PlusAssign);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(addHandlerStatement.HandlerExpression, data);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitRemoveHandlerStatement(RemoveHandlerStatement removeHandlerStatement, object data)
		{
			nodeTracker.TrackedVisit(removeHandlerStatement.EventExpression, data);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.MinusAssign);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(removeHandlerStatement.HandlerExpression, data);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitRaiseEventStatement(RaiseEventStatement raiseEventStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.If);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			outputFormatter.PrintIdentifier(raiseEventStatement.EventName);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.NotEqual);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Null);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			outputFormatter.BeginBrace(BraceStyle.EndOfLine);
			
			outputFormatter.Indent();
			outputFormatter.PrintIdentifier(raiseEventStatement.EventName);
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			this.AppendCommaSeparatedList(raiseEventStatement.Arguments);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			outputFormatter.PrintToken(Tokens.Semicolon);
			
			outputFormatter.NewLine();
			outputFormatter.EndBrace();
			
			return null;
		}
		
		public object VisitEraseStatement(EraseStatement eraseStatement, object data)
		{
			for (int i = 0; i < eraseStatement.Expressions.Count; i++) {
				if (i > 0) {
					outputFormatter.NewLine();
					outputFormatter.Indent();
				}
				nodeTracker.TrackedVisit(eraseStatement.Expressions[i], data);
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Assign);
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.Null);
				outputFormatter.PrintToken(Tokens.Semicolon);
			}
			return null;
		}
		
		public object VisitErrorStatement(ErrorStatement errorStatement, object data)
		{
			NotSupported(errorStatement);
			return null;
		}
		
		public object VisitOnErrorStatement(OnErrorStatement onErrorStatement, object data)
		{
			NotSupported(onErrorStatement);
			return null;
		}
		
		public object VisitReDimStatement(ReDimStatement reDimStatement, object data)
		{
			NotSupported(reDimStatement);
			return null;
		}
		
		public object VisitExpressionStatement(ExpressionStatement expressionStatement, object data)
		{
			nodeTracker.TrackedVisit(expressionStatement.Expression, data);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitLocalVariableDeclaration(LocalVariableDeclaration localVariableDeclaration, object data)
		{
			for (int i = 0; i < localVariableDeclaration.Variables.Count; ++i) {
				VariableDeclaration v = (VariableDeclaration)localVariableDeclaration.Variables[i];
				if (i > 0) {
					outputFormatter.NewLine();
					outputFormatter.Indent();
				}
				OutputModifier(localVariableDeclaration.Modifier);
				nodeTracker.TrackedVisit(localVariableDeclaration.GetTypeForVariable(i) ?? new TypeReference("object"), data);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(v, data);
				outputFormatter.PrintToken(Tokens.Semicolon);
			}
			return null;
		}
		
		public object VisitEmptyStatement(EmptyStatement emptyStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitYieldStatement(YieldStatement yieldStatement, object data)
		{
			Debug.Assert(yieldStatement != null);
			Debug.Assert(yieldStatement.Statement != null);
			outputFormatter.PrintText("yield");
			outputFormatter.Space();
			nodeTracker.TrackedVisit(yieldStatement.Statement, data);
			return null;
		}
		
		public object VisitReturnStatement(ReturnStatement returnStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Return);
			if (!returnStatement.Expression.IsNull) {
				outputFormatter.Space();
				nodeTracker.TrackedVisit(returnStatement.Expression, data);
			}
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitIfElseStatement(IfElseStatement ifElseStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.If);
			if (this.prettyPrintOptions.IfParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(ifElseStatement.Condition, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			PrintIfSection(ifElseStatement.TrueStatement);
			
			foreach (ElseIfSection elseIfSection in ifElseStatement.ElseIfSections) {
				nodeTracker.TrackedVisit(elseIfSection, data);
			}
			
			if (ifElseStatement.HasElseStatements) {
				outputFormatter.Indent();
				outputFormatter.PrintToken(Tokens.Else);
				PrintIfSection(ifElseStatement.FalseStatement);
			}
			
			return null;
		}
		
		void PrintIfSection(List<Statement> statements)
		{
			if (statements.Count != 1 || !(statements[0] is BlockStatement)) {
				outputFormatter.Space();
			}
			if (statements.Count != 1) {
				outputFormatter.PrintToken(Tokens.OpenCurlyBrace);
			}
			foreach (Statement stmt in statements) {
				nodeTracker.TrackedVisit(stmt, null);
			}
			if (statements.Count != 1) {
				outputFormatter.PrintToken(Tokens.CloseCurlyBrace);
			}
			if (statements.Count != 1 || !(statements[0] is BlockStatement)) {
				outputFormatter.Space();
			}
		}
		
		public object VisitElseIfSection(ElseIfSection elseIfSection, object data)
		{
			outputFormatter.PrintToken(Tokens.Else);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.If);
			if (prettyPrintOptions.IfParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(elseIfSection.Condition, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(elseIfSection.EmbeddedStatement);
			
			return null;
		}
		
		public object VisitForStatement(ForStatement forStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.For);
			if (this.prettyPrintOptions.ForParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			outputFormatter.DoIndent = false;
			outputFormatter.DoNewLine = false;
			outputFormatter.EmitSemicolon = false;
			for (int i = 0; i < forStatement.Initializers.Count; ++i) {
				INode node = (INode)forStatement.Initializers[i];
				nodeTracker.TrackedVisit(node, data);
				if (i + 1 < forStatement.Initializers.Count) {
					outputFormatter.PrintToken(Tokens.Comma);
				}
			}
			outputFormatter.EmitSemicolon = true;
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.EmitSemicolon = false;
			if (!forStatement.Condition.IsNull) {
				if (this.prettyPrintOptions.SpacesAfterSemicolon) {
					outputFormatter.Space();
				}
				nodeTracker.TrackedVisit(forStatement.Condition, data);
			}
			outputFormatter.EmitSemicolon = true;
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.EmitSemicolon = false;
			if (forStatement.Iterator != null && forStatement.Iterator.Count > 0) {
				if (this.prettyPrintOptions.SpacesAfterSemicolon) {
					outputFormatter.Space();
				}
				
				for (int i = 0; i < forStatement.Iterator.Count; ++i) {
					INode node = (INode)forStatement.Iterator[i];
					nodeTracker.TrackedVisit(node, data);
					if (i + 1 < forStatement.Iterator.Count) {
						outputFormatter.PrintToken(Tokens.Comma);
					}
				}
			}
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			outputFormatter.EmitSemicolon = true;
			outputFormatter.DoNewLine     = true;
			outputFormatter.DoIndent      = true;
			
			WriteEmbeddedStatement(forStatement.EmbeddedStatement);
			
			return null;
		}
		
		void WriteEmbeddedStatement(Statement statement)
		{
			if (statement is BlockStatement) {
				nodeTracker.TrackedVisit(statement, prettyPrintOptions.StatementBraceStyle);
			} else {
				++outputFormatter.IndentationLevel;
				outputFormatter.NewLine();
				nodeTracker.TrackedVisit(statement, null);
				outputFormatter.NewLine();
				--outputFormatter.IndentationLevel;
			}
		}
		
		public object VisitLabelStatement(LabelStatement labelStatement, object data)
		{
			outputFormatter.PrintIdentifier(labelStatement.Label);
			outputFormatter.PrintToken(Tokens.Colon);
			return null;
		}
		
		public object VisitGotoStatement(GotoStatement gotoStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Goto);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(gotoStatement.Label);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitSwitchStatement(SwitchStatement switchStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Switch);
			if (this.prettyPrintOptions.SwitchParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(switchStatement.SwitchExpression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.OpenCurlyBrace);
			outputFormatter.NewLine();
			++outputFormatter.IndentationLevel;
			foreach (SwitchSection section in switchStatement.SwitchSections) {
				nodeTracker.TrackedVisit(section, data);
			}
			--outputFormatter.IndentationLevel;
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.CloseCurlyBrace);
			return null;
		}
		
		public object VisitSwitchSection(SwitchSection switchSection, object data)
		{
			foreach (CaseLabel label in switchSection.SwitchLabels) {
				nodeTracker.TrackedVisit(label, data);
			}
			
			++outputFormatter.IndentationLevel;
			foreach (Statement stmt in switchSection.Children) {
				outputFormatter.Indent();
				nodeTracker.TrackedVisit(stmt, data);
				outputFormatter.NewLine();
			}
			
			// Check if a 'break' should be auto inserted.
			if (switchSection.Children.Count == 0 ||
			    !(switchSection.Children[switchSection.Children.Count - 1] is BreakStatement ||
			      switchSection.Children[switchSection.Children.Count - 1] is ContinueStatement ||
			      switchSection.Children[switchSection.Children.Count - 1] is ReturnStatement)) {
				outputFormatter.Indent();
				outputFormatter.PrintToken(Tokens.Break);
				outputFormatter.PrintToken(Tokens.Semicolon);
				outputFormatter.NewLine();
			}
			
			--outputFormatter.IndentationLevel;
			return null;
		}
		
		public object VisitCaseLabel(CaseLabel caseLabel, object data)
		{
			outputFormatter.Indent();
			if (caseLabel.IsDefault) {
				outputFormatter.PrintToken(Tokens.Default);
			} else {
				outputFormatter.PrintToken(Tokens.Case);
				outputFormatter.Space();
				if (caseLabel.BinaryOperatorType != BinaryOperatorType.None) {
					Error(caseLabel, String.Format("Case labels with binary operators are unsupported : {0}", caseLabel.BinaryOperatorType));
				}
				nodeTracker.TrackedVisit(caseLabel.Label, data);
			}
			outputFormatter.PrintToken(Tokens.Colon);
			if (!caseLabel.ToExpression.IsNull) {
				PrimitiveExpression pl = caseLabel.Label as PrimitiveExpression;
				PrimitiveExpression pt = caseLabel.ToExpression as PrimitiveExpression;
				if (pl != null && pt != null && pl.Value is int && pt.Value is int) {
					int plv = (int)pl.Value;
					int prv = (int)pt.Value;
					if (plv < prv && plv + 12 > prv) {
						for (int i = plv + 1; i <= prv; i++) {
							outputFormatter.NewLine();
							outputFormatter.Indent();
							outputFormatter.PrintToken(Tokens.Case);
							outputFormatter.Space();
							outputFormatter.PrintText(i.ToString(NumberFormatInfo.InvariantInfo));
							outputFormatter.PrintToken(Tokens.Colon);
						}
					} else {
						outputFormatter.PrintText(" // TODO: to ");
						nodeTracker.TrackedVisit(caseLabel.ToExpression, data);
					}
				} else {
					outputFormatter.PrintText(" // TODO: to ");
					nodeTracker.TrackedVisit(caseLabel.ToExpression, data);
				}
			}
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitBreakStatement(BreakStatement breakStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Break);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitStopStatement(StopStatement stopStatement, object data)
		{
			outputFormatter.PrintText("System.Diagnostics.Debugger.Break()");
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitResumeStatement(ResumeStatement resumeStatement, object data)
		{
			NotSupported(resumeStatement);
			return null;
		}
		
		public object VisitEndStatement(EndStatement endStatement, object data)
		{
			outputFormatter.PrintText("System.Environment.Exit(0)");
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitContinueStatement(ContinueStatement continueStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Continue);
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitGotoCaseStatement(GotoCaseStatement gotoCaseStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Goto);
			outputFormatter.Space();
			if (gotoCaseStatement.IsDefaultCase) {
				outputFormatter.PrintToken(Tokens.Default);
			} else {
				outputFormatter.PrintToken(Tokens.Case);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(gotoCaseStatement.Expression, data);
			}
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		void PrintLoopCheck(DoLoopStatement doLoopStatement)
		{
			outputFormatter.PrintToken(Tokens.While);
			if (this.prettyPrintOptions.WhileParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			
			if (doLoopStatement.ConditionType == ConditionType.Until) {
				outputFormatter.PrintToken(Tokens.Not);
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
			}
			
			if (doLoopStatement.Condition.IsNull) {
				outputFormatter.PrintToken(Tokens.True);
			} else {
				nodeTracker.TrackedVisit(doLoopStatement.Condition, null);
			}
			
			if (doLoopStatement.ConditionType == ConditionType.Until) {
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
			}
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
		}
		
		public object VisitDoLoopStatement(DoLoopStatement doLoopStatement, object data)
		{
			if (doLoopStatement.ConditionPosition == ConditionPosition.None) {
				Error(doLoopStatement, String.Format("Unknown condition position for loop : {0}.", doLoopStatement));
			}
			
			if (doLoopStatement.ConditionPosition == ConditionPosition.Start) {
				PrintLoopCheck(doLoopStatement);
			} else {
				outputFormatter.PrintToken(Tokens.Do);
			}
			
			WriteEmbeddedStatement(doLoopStatement.EmbeddedStatement);
			
			if (doLoopStatement.ConditionPosition == ConditionPosition.End) {
				outputFormatter.Indent();
				PrintLoopCheck(doLoopStatement);
				outputFormatter.PrintToken(Tokens.Semicolon);
				outputFormatter.NewLine();
			}
			
			return null;
		}
		
		public object VisitForeachStatement(ForeachStatement foreachStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Foreach);
			if (this.prettyPrintOptions.ForeachParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(foreachStatement.TypeReference, data);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(foreachStatement.VariableName);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.In);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(foreachStatement.Expression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(foreachStatement.EmbeddedStatement);
			
			return null;
		}
		
		public object VisitLockStatement(LockStatement lockStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Lock);
			if (this.prettyPrintOptions.LockParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(lockStatement.LockExpression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(lockStatement.EmbeddedStatement);
			
			return null;
		}
		
		public object VisitUsingStatement(UsingStatement usingStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Using);
			if (this.prettyPrintOptions.UsingParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			outputFormatter.DoIndent = false;
			outputFormatter.DoNewLine = false;
			outputFormatter.EmitSemicolon = false;
			
			nodeTracker.TrackedVisit(usingStatement.ResourceAcquisition, data);
			outputFormatter.DoIndent = true;
			outputFormatter.DoNewLine = true;
			outputFormatter.EmitSemicolon = true;
			
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(usingStatement.EmbeddedStatement);
			
			return null;
		}
		
		public object VisitWithStatement(WithStatement withStatement, object data)
		{
			withExpressionStack.Push(withStatement);
			nodeTracker.TrackedVisit(withStatement.Body, BraceStyle.EndOfLine);
			withExpressionStack.Pop();
			return null;
		}
		
		public object VisitTryCatchStatement(TryCatchStatement tryCatchStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Try);
			WriteEmbeddedStatement(tryCatchStatement.StatementBlock);
			
			foreach (CatchClause catchClause in tryCatchStatement.CatchClauses) {
				nodeTracker.TrackedVisit(catchClause, data);
			}
			
			if (!tryCatchStatement.FinallyBlock.IsNull) {
				outputFormatter.Indent();
				outputFormatter.PrintToken(Tokens.Finally);
				WriteEmbeddedStatement(tryCatchStatement.FinallyBlock);
			}
			return null;
		}
		
		public object VisitCatchClause(CatchClause catchClause, object data)
		{
			outputFormatter.Indent();
			outputFormatter.PrintToken(Tokens.Catch);
			
			if (!catchClause.TypeReference.IsNull) {
				if (this.prettyPrintOptions.CatchParentheses) {
					outputFormatter.Space();
				}
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
				outputFormatter.PrintIdentifier(catchClause.TypeReference.Type);
				if (catchClause.VariableName.Length > 0) {
					outputFormatter.Space();
					outputFormatter.PrintIdentifier(catchClause.VariableName);
				}
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
			}
			WriteEmbeddedStatement(catchClause.StatementBlock);
			return null;
		}
		
		public object VisitThrowStatement(ThrowStatement throwStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Throw);
			if (!throwStatement.Expression.IsNull) {
				outputFormatter.Space();
				nodeTracker.TrackedVisit(throwStatement.Expression, data);
			}
			outputFormatter.PrintToken(Tokens.Semicolon);
			return null;
		}
		
		public object VisitFixedStatement(FixedStatement fixedStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Fixed);
			if (this.prettyPrintOptions.FixedParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(fixedStatement.TypeReference, data);
			outputFormatter.Space();
			AppendCommaSeparatedList(fixedStatement.PointerDeclarators);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(fixedStatement.EmbeddedStatement);
			return null;
		}
		
		public object VisitUnsafeStatement(UnsafeStatement unsafeStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Unsafe);
			WriteEmbeddedStatement(unsafeStatement.Block);
			return null;
		}
		
		public object VisitCheckedStatement(CheckedStatement checkedStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Checked);
			WriteEmbeddedStatement(checkedStatement.Block);
			return null;
		}
		
		public object VisitUncheckedStatement(UncheckedStatement uncheckedStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.Unchecked);
			WriteEmbeddedStatement(uncheckedStatement.Block);
			return null;
		}
		
		public object VisitExitStatement(ExitStatement exitStatement, object data)
		{
			if (exitStatement.ExitType == ExitType.Function || exitStatement.ExitType == ExitType.Sub || exitStatement.ExitType == ExitType.Property) {
				outputFormatter.PrintToken(Tokens.Return);
			} else {
				outputFormatter.PrintToken(Tokens.Break);
			}
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.PrintText(" // TODO: might not be correct. Was : Exit " + exitStatement.ExitType);
			outputFormatter.NewLine();
			return null;
		}
		
		public object VisitForNextStatement(ForNextStatement forNextStatement, object data)
		{
			outputFormatter.PrintToken(Tokens.For);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			if (!forNextStatement.TypeReference.IsNull) {
				nodeTracker.TrackedVisit(forNextStatement.TypeReference, data);
				outputFormatter.Space();
			}
			outputFormatter.PrintIdentifier(forNextStatement.VariableName);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Assign);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(forNextStatement.Start, data);
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(forNextStatement.VariableName);
			outputFormatter.Space();
			PrimitiveExpression pe = forNextStatement.Step as PrimitiveExpression;
			if ((pe == null || !(pe.Value is int) || ((int)pe.Value) >= 0)
			    && !(forNextStatement.Step is UnaryOperatorExpression))
				outputFormatter.PrintToken(Tokens.LessEqual);
			else
				outputFormatter.PrintToken(Tokens.GreaterEqual);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(forNextStatement.End, data);
			outputFormatter.PrintToken(Tokens.Semicolon);
			outputFormatter.Space();
			outputFormatter.PrintIdentifier(forNextStatement.VariableName);
			if (forNextStatement.Step.IsNull) {
				outputFormatter.PrintToken(Tokens.Increment);
			} else {
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.PlusAssign);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(forNextStatement.Step, data);
			}
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			
			WriteEmbeddedStatement(forNextStatement.EmbeddedStatement);
			return null;
		}
		#endregion
		
		#region Expressions
		public object VisitClassReferenceExpression(ClassReferenceExpression classReferenceExpression, object data)
		{
			NotSupported(classReferenceExpression);
			return null;
		}
		
		static string ConvertCharLiteral(char ch)
		{
			if (ch == '\'') return "\\'";
			return ConvertChar(ch);
		}
		
		static string ConvertChar(char ch)
		{
			switch (ch) {
				case '\\':
					return "\\\\";
				case '\0':
					return "\\0";
				case '\a':
					return "\\a";
				case '\b':
					return "\\b";
				case '\f':
					return "\\f";
				case '\n':
					return "\\n";
				case '\r':
					return "\\r";
				case '\t':
					return "\\t";
				case '\v':
					return "\\v";
				default:
					if (char.IsControl(ch)) {
						return "\\u" + (int)ch;
					} else {
						return ch.ToString();
					}
			}
		}
		
		static string ConvertString(string str)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char ch in str) {
				if (ch == '"')
					sb.Append("\\\"");
				else
					sb.Append(ConvertChar(ch));
			}
			return sb.ToString();
		}
		
		public object VisitPrimitiveExpression(PrimitiveExpression primitiveExpression, object data)
		{
			if (primitiveExpression.Value == null) {
				outputFormatter.PrintToken(Tokens.Null);
				return null;
			}
			
			object val = primitiveExpression.Value;
			
			if (val is bool) {
				if ((bool)val) {
					outputFormatter.PrintToken(Tokens.True);
				} else {
					outputFormatter.PrintToken(Tokens.False);
				}
				return null;
			}
			
			if (val is string) {
				outputFormatter.PrintText('"' + ConvertString(val.ToString()) + '"');
				return null;
			}
			
			if (val is char) {
				outputFormatter.PrintText("'" + ConvertCharLiteral((char)val) + "'");
				return null;
			}
			
			if (val is decimal) {
				outputFormatter.PrintText(((decimal)val).ToString(NumberFormatInfo.InvariantInfo) + "m");
				return null;
			}
			
			if (val is float) {
				outputFormatter.PrintText(((float)val).ToString(NumberFormatInfo.InvariantInfo) + "f");
				return null;
			}
			
			if (val is double) {
				string text = ((double)val).ToString(NumberFormatInfo.InvariantInfo);
				if (text.IndexOf('.') < 0 && text.IndexOf('E') < 0)
					outputFormatter.PrintText(text + ".0");
				else
					outputFormatter.PrintText(text);
				return null;
			}
			
			if (val is IFormattable) {
				outputFormatter.PrintText(((IFormattable)val).ToString(null, NumberFormatInfo.InvariantInfo));
				if (val is uint || val is ulong) {
					outputFormatter.PrintText("u");
				}
				if (val is long || val is ulong) {
					outputFormatter.PrintText("l");
				}
			} else {
				outputFormatter.PrintText(val.ToString());
			}
			
			return null;
		}
		
		static bool IsNullLiteralExpression(Expression expr)
		{
			PrimitiveExpression pe = expr as PrimitiveExpression;
			if (pe == null) return false;
			return pe.Value == null;
		}
		
		public object VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression, object data)
		{
			// VB-operators that require special representation:
			switch (binaryOperatorExpression.Op) {
				case BinaryOperatorType.ReferenceEquality:
				case BinaryOperatorType.ReferenceInequality:
					if (IsNullLiteralExpression(binaryOperatorExpression.Left) || IsNullLiteralExpression(binaryOperatorExpression.Right)) {
						// prefer a == null to object.ReferenceEquals(a, null)
						break;
					}
					
					if (binaryOperatorExpression.Op == BinaryOperatorType.ReferenceInequality)
						outputFormatter.PrintToken(Tokens.Not);
					outputFormatter.PrintText("object.ReferenceEquals");
					if (prettyPrintOptions.BeforeMethodCallParentheses) {
						outputFormatter.Space();
					}
					
					outputFormatter.PrintToken(Tokens.OpenParenthesis);
					nodeTracker.TrackedVisit(binaryOperatorExpression.Left, data);
					PrintFormattedComma();
					nodeTracker.TrackedVisit(binaryOperatorExpression.Right, data);
					outputFormatter.PrintToken(Tokens.CloseParenthesis);
					return null;
				case BinaryOperatorType.Power:
					outputFormatter.PrintText("Math.Pow");
					if (prettyPrintOptions.BeforeMethodCallParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.OpenParenthesis);
					nodeTracker.TrackedVisit(binaryOperatorExpression.Left, data);
					PrintFormattedComma();
					nodeTracker.TrackedVisit(binaryOperatorExpression.Right, data);
					outputFormatter.PrintToken(Tokens.CloseParenthesis);
					return null;
			}
			nodeTracker.TrackedVisit(binaryOperatorExpression.Left, data);
			switch (binaryOperatorExpression.Op) {
				case BinaryOperatorType.Add:
				case BinaryOperatorType.Concat: // translate Concatenation to +
					if (prettyPrintOptions.AroundAdditiveOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Plus);
					if (prettyPrintOptions.AroundAdditiveOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.Subtract:
					if (prettyPrintOptions.AroundAdditiveOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Minus);
					if (prettyPrintOptions.AroundAdditiveOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.Multiply:
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Times);
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.Divide:
				case BinaryOperatorType.DivideInteger:
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Div);
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.Modulus:
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Mod);
					if (prettyPrintOptions.AroundMultiplicativeOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.ShiftLeft:
					if (prettyPrintOptions.AroundShiftOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.ShiftLeft);
					if (prettyPrintOptions.AroundShiftOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.ShiftRight:
					if (prettyPrintOptions.AroundShiftOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.GreaterThan);
					outputFormatter.PrintToken(Tokens.GreaterThan);
					if (prettyPrintOptions.AroundShiftOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.BitwiseAnd:
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.BitwiseAnd);
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.BitwiseOr:
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.BitwiseOr);
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.ExclusiveOr:
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Xor);
					if (prettyPrintOptions.AroundBitwiseOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.LogicalAnd:
					if (prettyPrintOptions.AroundLogicalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.LogicalAnd);
					if (prettyPrintOptions.AroundLogicalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.LogicalOr:
					if (prettyPrintOptions.AroundLogicalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.LogicalOr);
					if (prettyPrintOptions.AroundLogicalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				case BinaryOperatorType.Equality:
				case BinaryOperatorType.ReferenceEquality:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.Equal);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.GreaterThan:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.GreaterThan);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.GreaterThanOrEqual:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.GreaterEqual);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.InEquality:
				case BinaryOperatorType.ReferenceInequality:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.NotEqual);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.LessThan:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.LessThan);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.LessThanOrEqual:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.LessEqual);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
				case BinaryOperatorType.NullCoalescing:
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					outputFormatter.PrintToken(Tokens.DoubleQuestion);
					if (prettyPrintOptions.AroundRelationalOperatorParentheses) {
						outputFormatter.Space();
					}
					break;
					
				default:
					Error(binaryOperatorExpression, String.Format("Unknown binary operator {0}", binaryOperatorExpression.Op));
					return null;
			}
			nodeTracker.TrackedVisit(binaryOperatorExpression.Right, data);
			return null;
		}
		
		public object VisitParenthesizedExpression(ParenthesizedExpression parenthesizedExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(parenthesizedExpression.Expression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitInvocationExpression(InvocationExpression invocationExpression, object data)
		{
			nodeTracker.TrackedVisit(invocationExpression.TargetObject, data);
			
			if (invocationExpression.TypeArguments != null && invocationExpression.TypeArguments.Count > 0) {
				outputFormatter.PrintToken(Tokens.LessThan);
				AppendCommaSeparatedList(invocationExpression.TypeArguments);
				outputFormatter.PrintToken(Tokens.GreaterThan);
			}
			
			if (prettyPrintOptions.BeforeMethodCallParentheses) {
				outputFormatter.Space();
			}
			
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(invocationExpression.Arguments);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitIdentifierExpression(IdentifierExpression identifierExpression, object data)
		{
			outputFormatter.PrintIdentifier(identifierExpression.Identifier);
			return null;
		}
		
		public object VisitTypeReferenceExpression(TypeReferenceExpression typeReferenceExpression, object data)
		{
			nodeTracker.TrackedVisit(typeReferenceExpression.TypeReference, data);
			return null;
		}
		
		public object VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression, object data)
		{
			switch (unaryOperatorExpression.Op) {
				case UnaryOperatorType.BitNot:
					outputFormatter.PrintToken(Tokens.BitwiseComplement);
					break;
				case UnaryOperatorType.Decrement:
					outputFormatter.PrintToken(Tokens.Decrement);
					break;
				case UnaryOperatorType.Increment:
					outputFormatter.PrintToken(Tokens.Increment);
					break;
				case UnaryOperatorType.Minus:
					outputFormatter.PrintToken(Tokens.Minus);
					break;
				case UnaryOperatorType.Not:
					outputFormatter.PrintToken(Tokens.Not);
					break;
				case UnaryOperatorType.Plus:
					outputFormatter.PrintToken(Tokens.Plus);
					break;
				case UnaryOperatorType.PostDecrement:
					nodeTracker.TrackedVisit(unaryOperatorExpression.Expression, data);
					outputFormatter.PrintToken(Tokens.Decrement);
					return null;
				case UnaryOperatorType.PostIncrement:
					nodeTracker.TrackedVisit(unaryOperatorExpression.Expression, data);
					outputFormatter.PrintToken(Tokens.Increment);
					return null;
				case UnaryOperatorType.Star:
					outputFormatter.PrintToken(Tokens.Times);
					break;
				case UnaryOperatorType.BitWiseAnd:
					outputFormatter.PrintToken(Tokens.BitwiseAnd);
					break;
				default:
					Error(unaryOperatorExpression, String.Format("Unknown unary operator {0}", unaryOperatorExpression.Op));
					return null;
			}
			nodeTracker.TrackedVisit(unaryOperatorExpression.Expression, data);
			return null;
		}
		
		public object VisitAssignmentExpression(AssignmentExpression assignmentExpression, object data)
		{
			nodeTracker.TrackedVisit(assignmentExpression.Left, data);
			if (this.prettyPrintOptions.AroundAssignmentParentheses) {
				outputFormatter.Space();
			}
			switch (assignmentExpression.Op) {
				case AssignmentOperatorType.Assign:
					outputFormatter.PrintToken(Tokens.Assign);
					break;
				case AssignmentOperatorType.Add:
					outputFormatter.PrintToken(Tokens.PlusAssign);
					break;
				case AssignmentOperatorType.Subtract:
					outputFormatter.PrintToken(Tokens.MinusAssign);
					break;
				case AssignmentOperatorType.Multiply:
					outputFormatter.PrintToken(Tokens.TimesAssign);
					break;
				case AssignmentOperatorType.Divide:
				case AssignmentOperatorType.DivideInteger:
					outputFormatter.PrintToken(Tokens.DivAssign);
					break;
				case AssignmentOperatorType.ShiftLeft:
					outputFormatter.PrintToken(Tokens.ShiftLeftAssign);
					break;
				case AssignmentOperatorType.ShiftRight:
					outputFormatter.PrintToken(Tokens.GreaterThan);
					outputFormatter.PrintToken(Tokens.GreaterEqual);
					break;
				case AssignmentOperatorType.ExclusiveOr:
					outputFormatter.PrintToken(Tokens.XorAssign);
					break;
				case AssignmentOperatorType.Modulus:
					outputFormatter.PrintToken(Tokens.ModAssign);
					break;
				case AssignmentOperatorType.BitwiseAnd:
					outputFormatter.PrintToken(Tokens.BitwiseAndAssign);
					break;
				case AssignmentOperatorType.BitwiseOr:
					outputFormatter.PrintToken(Tokens.BitwiseOrAssign);
					break;
				case AssignmentOperatorType.Power:
					outputFormatter.PrintToken(Tokens.Assign);
					if (this.prettyPrintOptions.AroundAssignmentParentheses) {
						outputFormatter.Space();
					}
					VisitBinaryOperatorExpression(new BinaryOperatorExpression(assignmentExpression.Left,
					                                                           BinaryOperatorType.Power,
					                                                           assignmentExpression.Right), data);
					return null;
				default:
					Error(assignmentExpression, String.Format("Unknown assignment operator {0}", assignmentExpression.Op));
					return null;
			}
			if (this.prettyPrintOptions.AroundAssignmentParentheses) {
				outputFormatter.Space();
			}
			nodeTracker.TrackedVisit(assignmentExpression.Right, data);
			return null;
		}
		
		public object VisitSizeOfExpression(SizeOfExpression sizeOfExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Sizeof);
			if (prettyPrintOptions.SizeOfParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(sizeOfExpression.TypeReference, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitTypeOfExpression(TypeOfExpression typeOfExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Typeof);
			if (prettyPrintOptions.TypeOfParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(typeOfExpression.TypeReference, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Default);
			if (prettyPrintOptions.TypeOfParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(defaultValueExpression.TypeReference, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitTypeOfIsExpression(TypeOfIsExpression typeOfIsExpression, object data)
		{
			nodeTracker.TrackedVisit(typeOfIsExpression.Expression, data);
			outputFormatter.Space();
			outputFormatter.PrintToken(Tokens.Is);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(typeOfIsExpression.TypeReference, data);
			return null;
		}
		
		public object VisitAddressOfExpression(AddressOfExpression addressOfExpression, object data)
		{
			// C# 2.0 can reference methods directly
			return nodeTracker.TrackedVisit(addressOfExpression.Expression, data);
		}
		
		public object VisitAnonymousMethodExpression(AnonymousMethodExpression anonymousMethodExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Delegate);
			
			if (anonymousMethodExpression.Parameters.Count > 0 || anonymousMethodExpression.HasParameterList) {
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
				AppendCommaSeparatedList(anonymousMethodExpression.Parameters);
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
			}
			OutputBlockAllowInline(anonymousMethodExpression.Body, this.prettyPrintOptions.MethodBraceStyle, false);
			return null;
		}
		
		public object VisitCheckedExpression(CheckedExpression checkedExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Checked);
			if (prettyPrintOptions.CheckedParentheses) {
				outputFormatter.Space();
			}
			
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(checkedExpression.Expression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitUncheckedExpression(UncheckedExpression uncheckedExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Unchecked);
			if (prettyPrintOptions.UncheckedParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			nodeTracker.TrackedVisit(uncheckedExpression.Expression, data);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitPointerReferenceExpression(PointerReferenceExpression pointerReferenceExpression, object data)
		{
			nodeTracker.TrackedVisit(pointerReferenceExpression.TargetObject, data);
			outputFormatter.PrintToken(Tokens.Pointer);
			outputFormatter.PrintIdentifier(pointerReferenceExpression.Identifier);
			return null;
		}
		
		public object VisitCastExpression(CastExpression castExpression, object data)
		{
			if (castExpression.CastType == CastType.TryCast) {
				nodeTracker.TrackedVisit(castExpression.Expression, data);
				outputFormatter.Space();
				outputFormatter.PrintToken(Tokens.As);
				outputFormatter.Space();
				nodeTracker.TrackedVisit(castExpression.CastTo, data);
			} else {
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
				nodeTracker.TrackedVisit(castExpression.CastTo, data);
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
				if (this.prettyPrintOptions.SpacesAfterTypecast) {
					outputFormatter.Space();
				}
				nodeTracker.TrackedVisit(castExpression.Expression, data);
			}
			return null;
		}
		
		public object VisitStackAllocExpression(StackAllocExpression stackAllocExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.Stackalloc);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(stackAllocExpression.TypeReference, data);
			outputFormatter.PrintToken(Tokens.OpenSquareBracket);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			nodeTracker.TrackedVisit(stackAllocExpression.Expression, data);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			return null;
		}
		
		public object VisitIndexerExpression(IndexerExpression indexerExpression, object data)
		{
			nodeTracker.TrackedVisit(indexerExpression.TargetObject, data);
			outputFormatter.PrintToken(Tokens.OpenSquareBracket);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			AppendCommaSeparatedList(indexerExpression.Indexes);
			if (this.prettyPrintOptions.SpacesWithinBrackets) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.CloseSquareBracket);
			return null;
		}
		
		public object VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.This);
			return null;
		}
		
		public object VisitBaseReferenceExpression(BaseReferenceExpression baseReferenceExpression, object data) {
			outputFormatter.PrintToken(Tokens.Base);
			return null;
		}
		
		public object VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.New);
			outputFormatter.Space();
			nodeTracker.TrackedVisit(objectCreateExpression.CreateType, data);
			if (prettyPrintOptions.NewParentheses) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.OpenParenthesis);
			AppendCommaSeparatedList(objectCreateExpression.Parameters);
			outputFormatter.PrintToken(Tokens.CloseParenthesis);
			return null;
		}
		
		public object VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.New);
			outputFormatter.Space();
			PrintTypeReferenceWithoutArray(arrayCreateExpression.CreateType);
			
			if (arrayCreateExpression.Arguments.Count > 0) {
				outputFormatter.PrintToken(Tokens.OpenSquareBracket);
				if (this.prettyPrintOptions.SpacesWithinBrackets) {
					outputFormatter.Space();
				}
				for (int i = 0; i < arrayCreateExpression.Arguments.Count; ++i) {
					if (i > 0) PrintFormattedComma();
					nodeTracker.TrackedVisit(arrayCreateExpression.Arguments[i], data);
				}
				if (this.prettyPrintOptions.SpacesWithinBrackets) {
					outputFormatter.Space();
				}
				outputFormatter.PrintToken(Tokens.CloseSquareBracket);
				PrintArrayRank(arrayCreateExpression.CreateType.RankSpecifier, 1);
			} else {
				PrintArrayRank(arrayCreateExpression.CreateType.RankSpecifier, 0);
			}
			
			if (!arrayCreateExpression.ArrayInitializer.IsNull) {
				outputFormatter.Space();
				nodeTracker.TrackedVisit(arrayCreateExpression.ArrayInitializer, data);
			}
			return null;
		}
		
		public object VisitFieldReferenceExpression(FieldReferenceExpression fieldReferenceExpression, object data)
		{
			Expression target = fieldReferenceExpression.TargetObject;
			if (target.IsNull && withExpressionStack.Count > 0) {
				target = ((WithStatement)withExpressionStack.Peek()).Expression;
			}
			
			if (target is BinaryOperatorExpression || target is CastExpression) {
				outputFormatter.PrintToken(Tokens.OpenParenthesis);
			}
			nodeTracker.TrackedVisit(target, data);
			if (target is BinaryOperatorExpression || target is CastExpression) {
				outputFormatter.PrintToken(Tokens.CloseParenthesis);
			}
			outputFormatter.PrintToken(Tokens.Dot);
			outputFormatter.PrintIdentifier(fieldReferenceExpression.FieldName);
			return null;
		}
		
		public object VisitDirectionExpression(DirectionExpression directionExpression, object data)
		{
			switch (directionExpression.FieldDirection) {
				case FieldDirection.Out:
					outputFormatter.PrintToken(Tokens.Out);
					outputFormatter.Space();
					break;
				case FieldDirection.Ref:
					outputFormatter.PrintToken(Tokens.Ref);
					outputFormatter.Space();
					break;
			}
			nodeTracker.TrackedVisit(directionExpression.Expression, data);
			return null;
		}
		
		public object VisitArrayInitializerExpression(ArrayInitializerExpression arrayInitializerExpression, object data)
		{
			outputFormatter.PrintToken(Tokens.OpenCurlyBrace);
			this.AppendCommaSeparatedList(arrayInitializerExpression.CreateExpressions);
			outputFormatter.PrintToken(Tokens.CloseCurlyBrace);
			return null;
		}
		
		public object VisitConditionalExpression(ConditionalExpression conditionalExpression, object data)
		{
			nodeTracker.TrackedVisit(conditionalExpression.Condition, data);
			if (this.prettyPrintOptions.ConditionalOperatorBeforeConditionSpace) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.Question);
			if (this.prettyPrintOptions.ConditionalOperatorAfterConditionSpace) {
				outputFormatter.Space();
			}
			nodeTracker.TrackedVisit(conditionalExpression.TrueExpression, data);
			if (this.prettyPrintOptions.ConditionalOperatorBeforeSeparatorSpace) {
				outputFormatter.Space();
			}
			outputFormatter.PrintToken(Tokens.Colon);
			if (this.prettyPrintOptions.ConditionalOperatorAfterSeparatorSpace) {
				outputFormatter.Space();
			}
			nodeTracker.TrackedVisit(conditionalExpression.FalseExpression, data);
			return null;
		}
		
		#endregion
		#endregion
		
		void OutputModifier(ParameterModifiers modifier, INode node)
		{
			switch (modifier) {
				case ParameterModifiers.None:
				case ParameterModifiers.In:
					break;
				case ParameterModifiers.Out:
					outputFormatter.PrintToken(Tokens.Out);
					outputFormatter.Space();
					break;
				case ParameterModifiers.Params:
					outputFormatter.PrintToken(Tokens.Params);
					outputFormatter.Space();
					break;
				case ParameterModifiers.Ref:
					outputFormatter.PrintToken(Tokens.Ref);
					outputFormatter.Space();
					break;
				case ParameterModifiers.Optional:
					Error(node, String.Format("Optional parameters aren't supported in C#"));
					break;
				default:
					Error(node, String.Format("Unsupported modifier : {0}", modifier));
					break;
			}
		}
		
		void OutputModifier(Modifiers modifier)
		{
			ArrayList tokenList = new ArrayList();
			if ((modifier & Modifiers.Unsafe) != 0) {
				tokenList.Add(Tokens.Unsafe);
			}
			if ((modifier & Modifiers.Public) != 0) {
				tokenList.Add(Tokens.Public);
			}
			if ((modifier & Modifiers.Private) != 0) {
				tokenList.Add(Tokens.Private);
			}
			if ((modifier & Modifiers.Protected) != 0) {
				tokenList.Add(Tokens.Protected);
			}
			if ((modifier & Modifiers.Static) != 0) {
				tokenList.Add(Tokens.Static);
			}
			if ((modifier & Modifiers.Internal) != 0) {
				tokenList.Add(Tokens.Internal);
			}
			if ((modifier & Modifiers.Override) != 0) {
				tokenList.Add(Tokens.Override);
			}
			if ((modifier & Modifiers.Abstract) != 0) {
				tokenList.Add(Tokens.Abstract);
			}
			if ((modifier & Modifiers.Virtual) != 0) {
				tokenList.Add(Tokens.Virtual);
			}
			if ((modifier & Modifiers.New) != 0) {
				tokenList.Add(Tokens.New);
			}
			if ((modifier & Modifiers.Sealed) != 0) {
				tokenList.Add(Tokens.Sealed);
			}
			if ((modifier & Modifiers.Extern) != 0) {
				tokenList.Add(Tokens.Extern);
			}
			if ((modifier & Modifiers.Const) != 0) {
				tokenList.Add(Tokens.Const);
			}
			if ((modifier & Modifiers.ReadOnly) != 0) {
				tokenList.Add(Tokens.Readonly);
			}
			if ((modifier & Modifiers.Volatile) != 0) {
				tokenList.Add(Tokens.Volatile);
			}
			if ((modifier & Modifiers.Fixed) != 0) {
				tokenList.Add(Tokens.Fixed);
			}
			outputFormatter.PrintTokenList(tokenList);
			
			if ((modifier & Modifiers.Partial) != 0) {
				outputFormatter.PrintText("partial ");
			}
		}
		
		public void AppendCommaSeparatedList<T>(ICollection<T> list) where T : class, INode
		{
			if (list != null) {
				int i = 0;
				foreach (T node in list) {
					nodeTracker.TrackedVisit(node, null);
					if (i + 1 < list.Count) {
						PrintFormattedComma();
					}
					if ((i + 1) % 10 == 0) {
						outputFormatter.NewLine();
						outputFormatter.Indent();
					}
					i++;
				}
			}
		}
	}
}
