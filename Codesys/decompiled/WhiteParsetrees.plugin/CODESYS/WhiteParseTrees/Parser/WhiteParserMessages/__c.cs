using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.WhiteParseTrees.Resources;

namespace CODESYS.WhiteParseTrees.Parser
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public static class WhiteParserMessages
	{
		public static string UnexpectedToken(IWhiteToken tokenFound, string expected)
		{
			string text = string.Format(WhiteParserMessageStrings.UnexpectedToken, tokenFound.Text);
			if (string.IsNullOrWhiteSpace(expected))
			{
				return text;
			}
			return string.Format(WhiteParserMessageStrings.UnexpectedTokenExpected, text, expected);
		}

		public static string UnexpectedToken(IWhiteToken tokenFound)
		{
			return UnexpectedToken(tokenFound, string.Empty);
		}

		public static string UnexpectedTokenExpectedOneOf(IWhiteToken tokenFound, string expectedCommaSeparatedList)
		{
			return string.Format(WhiteParserMessageStrings.ExpectedOneOf, UnexpectedToken(tokenFound), expectedCommaSeparatedList);
		}

		private static string UnexpectedToken(IWhiteToken tokenFound, IEnumerable<string> expectedSeq)
		{
			string text = string.Join(", ", expectedSeq);
			if (expectedSeq.Count() == 1)
			{
				return UnexpectedToken(tokenFound, text);
			}
			return UnexpectedTokenExpectedOneOf(tokenFound, text);
		}

		public static string UnexpectedToken(IWhiteToken tokenFound, IEnumerable<WhiteTokenType> expectedTokenTypes)
		{
			return UnexpectedToken(tokenFound, expectedTokenTypes.Select((WhiteTokenType tt) => GetEnumName(tt)));
		}

		public static string UnexpectedToken(IWhiteToken tokenFound, WhiteTokenType expectedTokenType)
		{
			return UnexpectedToken(tokenFound, new WhiteTokenType[1] { expectedTokenType });
		}

		public static string UnexpectedToken(IWhiteToken tokenFound, Type T)
		{
			return UnexpectedToken(tokenFound, GetFriendlyStringForTokenInterface(T));
		}

		public static string UnexpectedOperator(Operator op, Operator? expectedOp)
		{
			string enumName = GetEnumName(op);
			string text = string.Format(WhiteParserMessageStrings.UnexpectedOperator, enumName);
			if (!expectedOp.HasValue)
			{
				return text;
			}
			return string.Format(WhiteParserMessageStrings.UnexpectedOperator, text, expectedOp);
		}

		public static string UnexpectedEndOfInput()
		{
			return WhiteParserMessageStrings.UnexpectedEndOfInput;
		}

		public static string InternalError(string contextInfo)
		{
			return string.Format(WhiteParserMessageStrings.InternalError, contextInfo);
		}

		public static string FailedToParseDeclaration()
		{
			return WhiteParserMessageStrings.FailedToParseDeclaration;
		}

		public static string FailedToParseAssignmentExpression()
		{
			return WhiteParserMessageStrings.FailedToParseAssignmentExpression;
		}

		public static string ParsingStatementFailed()
		{
			return WhiteParserMessageStrings.FailedToParseStatement;
		}

		private static string GetEnumName(Enum member)
		{
			return Enum.GetName(member.GetType(), member);
		}

		private static string GetFriendlyStringForTokenInterface(Type tokenInterface)
		{
			string[] array = tokenInterface.FullName.Split('.');
			string text = array[array.Length - 1];
			if (text.StartsWith("IWhite"))
			{
				text = text.Substring("IWhite".Length);
			}
			else if (text.StartsWith("I"))
			{
				text = text.Substring(1);
			}
			return text.Replace("Token", "");
		}
	}
}
