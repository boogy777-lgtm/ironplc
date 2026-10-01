using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner
	{
		bool IncludeComments { get; set; }

		bool IncludePragmas { get; set; }

		bool IncludeWhitespaces { get; set; }

		bool IncludeEndOfLines { get; set; }

		bool IgnoreCase { get; set; }

		bool AllowNestedComments { get; set; }

		bool AllowMultipleUnderlines { get; set; }

		int SourceOffset { get; }

		IToken CurrentToken { get; }

		void Initialize(string stInput);

		void SetPosition(IToken token);

		TokenType GetNext(out IToken token);

		int Match(TokenType TokenType, bool bExceptEndOfInputAfterThat, out IToken token);

		string GetTokenText(IToken token);

		bool GetBoolean(IToken token);

		string GetPragma(IToken token);

		string GetComment(IToken token);

		string GetDocComment(IToken token);

		void GetDate(IToken token, out DateTime value, out bool bOverflow);

		void GetDateAndTime(IToken token, out DateTime value, out bool bOverflow);

		void GetDirectVariable(IToken token, out DirectVariableLocation location, out DirectVariableSize size, out int[] components, out bool bOverflow);

		void GetIncompleteDirectVariable(IToken token, out DirectVariableLocation location);

		string GetDoubleByteString(IToken token);

		void GetDuration(IToken token, out uint nDuration, out bool bOverflow);

		void GetLDuration(IToken token, out ulong ulDuration, out bool bOverflow);

		string GetEndOfLine(IToken token);

		string GetIdentifier(IToken token);

		void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow);

		Operator GetOperator(IToken token);

		void GetConversion(IToken token, out Operator sourceType, out Operator destType);

		void GetReal(IToken token, out double dValue, out Operator type, out bool bOverflow);

		string GetSingleByteString(IToken token);

		void GetTimeOfDay(IToken token, out DateTime value, out bool bOverflow);

		string GetWhitespace(IToken token);

		string GetError(IToken token);

		string GetOperatorText(Operator op);

		string GetOperatorText(Operator op, bool bShort);

		Operator[] GetDataTypes();

		string[] GetConversionOperators();

		Operator[] GetKeywords(IECLanguage language);

		Operator[] GetOperators(IECLanguage language);
	}
}
