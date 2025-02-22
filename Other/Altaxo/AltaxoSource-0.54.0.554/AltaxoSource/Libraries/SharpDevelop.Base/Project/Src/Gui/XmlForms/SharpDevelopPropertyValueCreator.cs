﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Mike Krüger" email="mike@icsharpcode.net"/>
//     <version>$Revision: 1965 $</version>
// </file>

using System;
using System.Drawing;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Gui.XmlForms
{
	public class SharpDevelopPropertyValueCreator : IPropertyValueCreator
	{
		public bool CanCreateValueForType(Type propertyType)
		{
			return propertyType == typeof(Icon) || propertyType == typeof(Image);
		}
		
		public object CreateValue(Type propertyType, string valueString)
		{
			
			if (propertyType == typeof(Icon)) {
				return ResourceService.GetIcon(valueString);
			}
			
			if (propertyType == typeof(Image)) {
				return ResourceService.GetBitmap(valueString);
			}
			
			return null;
		}
	}
}
