using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct PragmaVersionOperandParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private PragmaVersionOperandParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParsePragmaVersionOperand(ParserContext context, _IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			return new PragmaVersionOperandParser(context).ParseVersionPragmaOperand(verexpr, bUseWarningInsteadOfError);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private void AddErrorSTAndAdjustSourcePosition(_IExprement exp, MessageId nErrorId, IToken token, params object[] args)
		{
			IMinimalPosition minimalPosition2 = (exp.PositionIntern = LMItemFactory.CreateMinimalPosition(exp.PositionIntern.EditorPosition, (short)(token.PositionOffset + 1)));
			exp.PositionLength = (short)token.Length;
			AddErrorST(exp, nErrorId, args);
		}

		private void AddWarningSTAndAdjustSourcePosition(_IExprement exp, MessageId nErrorId, IToken token, params object[] args)
		{
			IMinimalPosition minimalPosition2 = (exp.PositionIntern = LMItemFactory.CreateMinimalPosition(exp.PositionIntern.EditorPosition, (short)(token.PositionOffset + 1)));
			exp.PositionLength = (short)token.Length;
			ErrorHandler.AddWarningST(exp, nErrorId, args);
		}

		private _IExpression ParseVersionPragmaOperand(_IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_OperatorExpected, token.OrgToken, "(", PragmaScanner.GetTokenText(token));
				return verexpr;
			}
			Operator @operator = Operator.None;
			bool flag = false;
			bool flag2 = false;
			if (TokenType.Operator == PragmaScanner.OrgScanner.GetNext(out var token2))
			{
				flag = true;
				@operator = PragmaScanner.OrgScanner.GetOperator(token2);
				flag2 = IsComparisonOperator(@operator);
			}
			if (!flag2)
			{
				if (flag && bUseWarningInsteadOfError)
				{
					AddWarningSTAndAdjustSourcePosition(verexpr, MessageId.Err_ComparisonOperatorExpected, token2, PragmaScanner.OrgScanner.GetTokenText(token2));
				}
				else
				{
					AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_ComparisonOperatorExpected, token2, PragmaScanner.OrgScanner.GetTokenText(token2));
				}
				return verexpr;
			}
			verexpr.SetOpComparison(@operator);
			if (CheckNextOperator(out var ptoken, PragmaOperator.Comma))
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_OperatorExpected, ptoken.OrgToken, ",", PragmaScanner.GetTokenText(ptoken));
				return verexpr;
			}
			if (PragmaScanner.GetNext(out ptoken) != PragmaTokenType.SingleByteString)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_StringLiteralExpected, ptoken.OrgToken, PragmaScanner.GetTokenText(ptoken));
				return verexpr;
			}
			if (SetVersionAndReturnError(verexpr, ptoken))
			{
				return verexpr;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_OperatorExpected, token.OrgToken, ")", PragmaScanner.GetTokenText(token));
			}
			return verexpr;
		}

		private bool SetVersionAndReturnError(_IVersionComparisonSupportingExpression verexpr, IPragmaToken ptoken)
		{
			try
			{
				verexpr.SetVersionToTest(new Version(ptoken.String));
			}
			catch (ArgumentOutOfRangeException)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_VersionPartNegative, ptoken.OrgToken, PragmaScanner.GetTokenText(ptoken));
				return true;
			}
			catch (ArgumentException)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_VersionInvalidFormat, ptoken.OrgToken, PragmaScanner.GetTokenText(ptoken));
				return true;
			}
			catch (OverflowException)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_VersionOverflow, ptoken.OrgToken, PragmaScanner.GetTokenText(ptoken));
				return true;
			}
			catch (FormatException)
			{
				AddErrorSTAndAdjustSourcePosition(verexpr, MessageId.Err_VersionInvalidFormat, ptoken.OrgToken, PragmaScanner.GetTokenText(ptoken));
				return true;
			}
			return false;
		}

		private bool CheckNextOperator(out IPragmaToken ptoken, PragmaOperator op)
		{
			if (PragmaScanner.GetNext(out ptoken) == PragmaTokenType.Operator)
			{
				return ptoken.Operator != op;
			}
			return true;
		}

		private bool IsComparisonOperator(Operator operatorToCheck)
		{
			if ((uint)(operatorToCheck - 134) <= 5u || (uint)(operatorToCheck - 175) <= 5u)
			{
				return true;
			}
			return false;
		}
	}
}
