// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="none" email=""/>
//     <version>$Revision: 935 $</version>
// </file>

using System;
using System.IO;
using NUnit.Framework;
using ICSharpCode.NRefactory.Parser;
using ICSharpCode.NRefactory.Parser.VB;
using ICSharpCode.NRefactory.PrettyPrinter;

namespace ICSharpCode.NRefactory.Tests.Lexer.VB
{
	[TestFixture]
	public sealed class LiteralsTests
	{
		ILexer GenerateLexer(StringReader sr)
		{
			return ParserFactory.CreateLexer(SupportedLanguage.VBNet, sr);
		}
		
		Token GetSingleToken(string text)
		{
			ILexer lexer = GenerateLexer(new StringReader(text));
			Token t = lexer.NextToken();
			Assert.AreEqual(Tokens.EOL, lexer.NextToken().kind, "Tokens.EOL");
			Assert.AreEqual(Tokens.EOF, lexer.NextToken().kind, "Tokens.EOF");
			Assert.AreEqual("", lexer.Errors.ErrorOutput);
			return t;
		}
		
		void CheckToken(string text, int tokenType, object val)
		{
			Token t = GetSingleToken(text);
			Assert.AreEqual(tokenType, t.kind, "Tokens.Literal");
			Assert.IsNotNull(t.literalValue, "literalValue is null");
			Assert.AreEqual(val, t.literalValue, "literalValue");
		}
		
		[Test]
		public void TestSingleDigit()
		{
			CheckToken("5", Tokens.LiteralInteger, 5);
		}
		
		[Test]
		public void TestZero()
		{
			CheckToken("0", Tokens.LiteralInteger, 0);
		}
		
		[Test]
		public void TestInteger()
		{
			CheckToken("15", Tokens.LiteralInteger, 15);
			CheckToken("8581", Tokens.LiteralInteger, 8581);
		}
		
		[Test]
		public void TestHexadecimalInteger()
		{
			CheckToken("&H10", Tokens.LiteralInteger, 0x10);
			CheckToken("&H10&", Tokens.LiteralInteger, 0x10);
			CheckToken("&h3ff&", Tokens.LiteralInteger, 0x3ff);
		}
		
		[Test]
		public void TestIncompleteHexadecimal()
		{
			ILexer lexer = GenerateLexer(new StringReader("&H\r\nabc"));
			Token t = lexer.NextToken();
			Assert.AreEqual(Tokens.LiteralInteger, t.kind);
			Assert.AreEqual(0, (int)t.literalValue);
			Assert.AreEqual(Tokens.EOL, lexer.NextToken().kind, "Tokens.EOL (1)");
			Assert.AreEqual(Tokens.Identifier, lexer.NextToken().kind, "Tokens.Identifier");
			Assert.AreEqual(Tokens.EOL, lexer.NextToken().kind, "Tokens.EOL (2)");
			Assert.AreEqual(Tokens.EOF, lexer.NextToken().kind, "Tokens.EOF");
			Assert.AreNotEqual("", lexer.Errors.ErrorOutput);
		}
		
		[Test]
		public void TestStringLiterals()
		{
			CheckToken("\"\"", Tokens.LiteralString, "");
			CheckToken("\"Hello, World!\"", Tokens.LiteralString, "Hello, World!");
			CheckToken("\"\"\"\"", Tokens.LiteralString, "\"");
		}
		
		[Test]
		public void TestCharacterLiterals()
		{
			CheckToken("\" \"c", Tokens.LiteralCharacter, ' ');
			CheckToken("\"!\"c", Tokens.LiteralCharacter, '!');
			CheckToken("\"\"\"\"c", Tokens.LiteralCharacter, '"');
		}
		
		[Test]
		public void TestDateLiterals()
		{
			CheckToken("# 8/23/1970 #", Tokens.LiteralDate, new DateTime(1970, 8, 23, 0, 0, 0));
			CheckToken("#8/23/1970#", Tokens.LiteralDate, new DateTime(1970, 8, 23, 0, 0, 0));
			CheckToken("# 8/23/1970  3:45:39AM #", Tokens.LiteralDate, new DateTime(1970, 8, 23, 3, 45, 39));
			CheckToken("# 3:45:39AM #", Tokens.LiteralDate, new DateTime(1, 1, 1, 3, 45, 39));
			CheckToken("# 3:45:39  PM #", Tokens.LiteralDate, new DateTime(1, 1, 1, 15, 45, 39));
			CheckToken("# 3:45:39 #", Tokens.LiteralDate, new DateTime(1, 1, 1, 3, 45, 39));
			CheckToken("# 13:45:39 #", Tokens.LiteralDate, new DateTime(1, 1, 1, 13, 45, 39));
			CheckToken("# 1AM #", Tokens.LiteralDate, new DateTime(1, 1, 1, 1, 0, 0));
		}
		
		[Test]
		public void TestDouble()
		{
			CheckToken("1.0", Tokens.LiteralDouble, 1.0);
			CheckToken("1.1", Tokens.LiteralDouble, 1.1);
			CheckToken("2e-5", Tokens.LiteralDouble, 2e-5);
			CheckToken("2.0e-5", Tokens.LiteralDouble, 2e-5);
			CheckToken("2e5", Tokens.LiteralDouble, 2e5);
			CheckToken("2.2e5", Tokens.LiteralDouble, 2.2e5);
			CheckToken("2e+5", Tokens.LiteralDouble, 2e5);
			CheckToken("2.2e+5", Tokens.LiteralDouble, 2.2e5);
			
			CheckToken("1r", Tokens.LiteralDouble, 1.0);
			CheckToken("1.0r", Tokens.LiteralDouble, 1.0);
			CheckToken("1.1r", Tokens.LiteralDouble, 1.1);
			CheckToken("2e-5r", Tokens.LiteralDouble, 2e-5);
			CheckToken("2.0e-5r", Tokens.LiteralDouble, 2e-5);
			CheckToken("2e5r", Tokens.LiteralDouble, 2e5);
			CheckToken("2.2e5r", Tokens.LiteralDouble, 2.2e5);
			CheckToken("2e+5r", Tokens.LiteralDouble, 2e5);
			CheckToken("2.2e+5r", Tokens.LiteralDouble, 2.2e5);
		}
		
		[Test]
		public void TestSingle()
		{
			CheckToken("1f", Tokens.LiteralSingle, 1.0f);
			CheckToken("1.0f", Tokens.LiteralSingle, 1.0f);
			CheckToken("1.1f", Tokens.LiteralSingle, 1.1f);
			CheckToken("2e-5f", Tokens.LiteralSingle, 2e-5f);
			CheckToken("2.0e-5f", Tokens.LiteralSingle, 2e-5f);
			CheckToken("2e5f", Tokens.LiteralSingle, 2e5f);
			CheckToken("2.2e5f", Tokens.LiteralSingle, 2.2e5f);
			CheckToken("2e+5f", Tokens.LiteralSingle, 2e5f);
			CheckToken("2.2e+5f", Tokens.LiteralSingle, 2.2e5f);
		}
		
		[Test]
		public void TestDecimal()
		{
			CheckToken("1d", Tokens.LiteralDecimal, 1m);
			CheckToken("1.0d", Tokens.LiteralDecimal, 1.0m);
			CheckToken("1.1d", Tokens.LiteralDecimal, 1.1m);
			CheckToken("2e-5d", Tokens.LiteralDecimal, 2e-5m);
			CheckToken("2.0e-5d", Tokens.LiteralDecimal, 2.0e-5m);
			CheckToken("2e5d", Tokens.LiteralDecimal, 2e5m);
			CheckToken("2.2e5d", Tokens.LiteralDecimal, 2.2e5m);
			CheckToken("2e+5d", Tokens.LiteralDecimal, 2e5m);
			CheckToken("2.2e+5d", Tokens.LiteralDecimal, 2.2e5m);
		}
	}
}
