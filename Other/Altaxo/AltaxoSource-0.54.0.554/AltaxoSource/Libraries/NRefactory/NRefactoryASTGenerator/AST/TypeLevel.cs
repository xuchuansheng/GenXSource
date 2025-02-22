// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1899 $</version>
// </file>

using System;
using System.Collections.Generic;

namespace NRefactoryASTGenerator.Ast
{
	class VariableDeclaration : AbstractNode
	{
		string     name;
		Expression initializer;
		TypeReference typeReference;
		Expression fixedArrayInitialization;
		
		public VariableDeclaration(string name) {}
		public VariableDeclaration(string name, Expression initializer) {}
		public VariableDeclaration(string name, Expression initializer, TypeReference typeReference) {}
	}
	
	class ConstructorDeclaration : ParametrizedNode
	{
		ConstructorInitializer constructorInitializer;
		BlockStatement         body;
		
		public ConstructorDeclaration(string name, Modifiers modifier,
		                              List<ParameterDeclarationExpression> parameters,
		                              List<AttributeSection> attributes)
			: base(modifier, attributes, name, parameters)
		{}
		
		public ConstructorDeclaration(string name, Modifiers modifier,
		                              List<ParameterDeclarationExpression> parameters,
		                              ConstructorInitializer constructorInitializer,
		                              List<AttributeSection> attributes)
			: base(modifier, attributes, name, parameters)
		{}
	}
	
	enum ConstructorInitializerType { None }
	
	[ImplementNullable]
	class ConstructorInitializer : AbstractNode
	{
		ConstructorInitializerType constructorInitializerType;
		List<Expression>           arguments;
	}
	
	[ImplementNullable(NullableImplementation.Abstract)]
	abstract class EventAddRemoveRegion : AttributedNode
	{
		BlockStatement block;
		List<ParameterDeclarationExpression> parameters;
		
		public EventAddRemoveRegion(List<AttributeSection> attributes) : base(attributes) {}
	}
	
	[ImplementNullable(NullableImplementation.CompleteAbstract)]
	class EventAddRegion : EventAddRemoveRegion
	{
		public EventAddRegion(List<AttributeSection> attributes) : base(attributes) {}
	}
	
	[ImplementNullable(NullableImplementation.CompleteAbstract)]
	class EventRemoveRegion : EventAddRemoveRegion
	{
		public EventRemoveRegion(List<AttributeSection> attributes) : base(attributes) {}
	}
	
	[ImplementNullable(NullableImplementation.CompleteAbstract)]
	class EventRaiseRegion : EventAddRemoveRegion
	{
		public EventRaiseRegion(List<AttributeSection> attributes) : base(attributes) {}
	}
	
	class InterfaceImplementation : AbstractNode
	{
		TypeReference interfaceType;
		[QuestionMarkDefault]
		string memberName;
		
		public InterfaceImplementation(TypeReference interfaceType, string memberName) {}
	}
	
	[IncludeBoolProperty("HasAddRegion",    "return !addRegion.IsNull;")]
	[IncludeBoolProperty("HasRemoveRegion", "return !removeRegion.IsNull;")]
	[IncludeBoolProperty("HasRaiseRegion",  "return !raiseRegion.IsNull;")]
	class EventDeclaration : ParametrizedNode
	{
		TypeReference typeReference;
		List<InterfaceImplementation> interfaceImplementations;
		EventAddRegion addRegion;
		EventRemoveRegion removeRegion;
		EventRaiseRegion raiseRegion;
		Location bodyStart;
		Location bodyEnd;
		Expression initializer;
		
		public EventDeclaration(TypeReference typeReference, string name, Modifiers modifier, List<AttributeSection> attributes, List<ParameterDeclarationExpression> parameters)
			: base(modifier, attributes, name, parameters)
		{ }
		
		// for VB:
		public EventDeclaration(TypeReference typeReference, Modifiers modifier, List<ParameterDeclarationExpression> parameters, List<AttributeSection> attributes, string name, List<InterfaceImplementation> interfaceImplementations)
			: base(modifier, attributes, name, parameters)
		{ }
	}
	
	[IncludeMember(@"
		public TypeReference GetTypeForField(int fieldIndex)
		{
			if (!typeReference.IsNull) {
				return typeReference;
			}
			return ((VariableDeclaration)Fields[fieldIndex]).TypeReference;
		}")]
	[IncludeMember(@"
		public VariableDeclaration GetVariableDeclaration(string variableName)
		{
			foreach (VariableDeclaration variableDeclaration in Fields) {
				if (variableDeclaration.Name == variableName) {
					return variableDeclaration;
				}
			}
			return null;
		}")]
	class FieldDeclaration : AttributedNode
	{
		TypeReference             typeReference;
		List<VariableDeclaration> fields;
		
		// for enum members
		public FieldDeclaration(List<AttributeSection> attributes) : base(attributes) {}
		
		// for all other cases
		public FieldDeclaration(List<AttributeSection> attributes, TypeReference typeReference, Modifiers modifier)
			: base(modifier, attributes)
		{}
	}
	
	class MethodDeclaration : ParametrizedNode
	{
		TypeReference    typeReference;
		BlockStatement   body;
		List<string>     handlesClause;
		List<InterfaceImplementation> interfaceImplementations;
		List<TemplateDefinition> templates;
		
		public MethodDeclaration(string name, Modifiers modifier, TypeReference typeReference, List<ParameterDeclarationExpression> parameters, List<AttributeSection> attributes) : base(modifier, attributes, name, parameters) {}
	}
	
	enum ConversionType { None }
	enum OverloadableOperatorType { None }
	
	[IncludeBoolProperty("IsConversionOperator", "return conversionType != ConversionType.None;")]
	[FixOperatorDeclarationAttribute]
	class OperatorDeclaration : MethodDeclaration
	{
		ConversionType conversionType;
		List<AttributeSection> returnTypeAttributes;
		OverloadableOperatorType overloadableOperator;
		
		public OperatorDeclaration(Modifiers modifier,
		                           List<AttributeSection> attributes,
		                           List<ParameterDeclarationExpression> parameters,
		                           TypeReference typeReference,
		                           ConversionType conversionType)
			: base(null, modifier, typeReference, parameters, attributes)
		{}
		
		public OperatorDeclaration(Modifiers modifier,
		                           List<AttributeSection> attributes,
		                           List<ParameterDeclarationExpression> parameters,
		                           TypeReference typeReference,
		                           OverloadableOperatorType overloadableOperator)
			: base(null, modifier, typeReference, parameters, attributes)
		{}
	}
	
	[IncludeBoolProperty("HasGetRegion", "return !getRegion.IsNull;")]
	[IncludeBoolProperty("HasSetRegion", "return !setRegion.IsNull;")]
	[IncludeBoolProperty("IsReadOnly", "return HasGetRegion && !HasSetRegion;")]
	[IncludeBoolProperty("IsWriteOnly", "return !HasGetRegion && HasSetRegion;")]
	[IncludeMember(@"
		internal PropertyDeclaration(string name, TypeReference typeReference, Modifiers modifier, List<AttributeSection> attributes) : this(modifier, attributes, name, null)
		{
			this.TypeReference = typeReference;
			if ((modifier & Modifiers.ReadOnly) != Modifiers.ReadOnly) {
				this.SetRegion = new PropertySetRegion(null, null);
			}
			if ((modifier & Modifiers.WriteOnly) != Modifiers.WriteOnly) {
				this.GetRegion = new PropertyGetRegion(null, null);
			}
		}")]
	class PropertyDeclaration : ParametrizedNode
	{
		List<InterfaceImplementation> interfaceImplementations;
		TypeReference     typeReference;
		Location          bodyStart;
		Location          bodyEnd;
		PropertyGetRegion getRegion;
		PropertySetRegion setRegion;
		
		public PropertyDeclaration(Modifiers modifier, List<AttributeSection> attributes,
		                           string name, List<ParameterDeclarationExpression> parameters)
			: base(modifier, attributes, name, parameters)
		{}
	}
	
	[ImplementNullable(NullableImplementation.Abstract)]
	abstract class PropertyGetSetRegion : AttributedNode
	{
		// can be null if only the definition is there (interface declaration)
		BlockStatement block;
		
		public PropertyGetSetRegion(BlockStatement block, List<AttributeSection> attributes) : base(attributes) {}
	}
	
	[ImplementNullable(NullableImplementation.CompleteAbstract)]
	class PropertyGetRegion : PropertyGetSetRegion
	{
		public PropertyGetRegion(BlockStatement block, List<AttributeSection> attributes) : base(block, attributes) {}
	}
	
	[ImplementNullable(NullableImplementation.CompleteAbstract)]
	class PropertySetRegion : PropertyGetSetRegion
	{
		List<ParameterDeclarationExpression> parameters;
		
		public PropertySetRegion(BlockStatement block, List<AttributeSection> attributes) : base(block, attributes) {}
	}
	
	class DestructorDeclaration : AttributedNode
	{
		string         name;
		BlockStatement body;
		
		public DestructorDeclaration(string name, Modifiers modifier, List<AttributeSection> attributes) : base(modifier, attributes) {}
	}
	
	[IncludeBoolProperty("HasGetRegion", "return !getRegion.IsNull;")]
	[IncludeBoolProperty("HasSetRegion", "return !setRegion.IsNull;")]
	[IncludeBoolProperty("IsReadOnly", "return HasGetRegion && !HasSetRegion;")]
	[IncludeBoolProperty("IsWriteOnly", "return !HasGetRegion && HasSetRegion;")]
	class IndexerDeclaration : AttributedNode
	{
		List<ParameterDeclarationExpression> parameters;
		List<InterfaceImplementation> interfaceImplementations;
		TypeReference     typeReference;
		Location          bodyStart;
		Location          bodyEnd;
		PropertyGetRegion getRegion;
		PropertySetRegion setRegion;
		
		public IndexerDeclaration(Modifiers modifier, List<ParameterDeclarationExpression> parameters, List<AttributeSection> attributes)
			: base(modifier, attributes)
		{}
		
		public IndexerDeclaration(TypeReference typeReference, List<ParameterDeclarationExpression> parameters, Modifiers modifier, List<AttributeSection> attributes)
			: base(modifier, attributes)
		{}
	}
	
	enum CharsetModifier { None }
	
	class DeclareDeclaration : ParametrizedNode
	{
		string          alias;
		string          library;
		CharsetModifier charset;
		TypeReference   typeReference;
		
		public DeclareDeclaration(string name, Modifiers modifier, TypeReference typeReference, List<ParameterDeclarationExpression> parameters, List<AttributeSection> attributes, string library, string alias, CharsetModifier charset)
			: base(modifier, attributes, name, parameters)
		{}
	}
}
