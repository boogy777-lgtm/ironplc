using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct PragmaOperandParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private PragmaOperandParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpression ParsePragmaORExp(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new PragmaOperandParser(context).ParsePragmaORExp(out bError, tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IExpression ParseVersionPragmaOperand(_IVersionComparisonSupportingExpression verexpr, bool bUseWarningInsteadOfError)
		{
			return PragmaVersionOperandParser.ParsePragmaVersionOperand(Context, verexpr, bUseWarningInsteadOfError);
		}

		private _IExpression ParsePragmaOperand(out bool bError, IToken tokenPragma)
		{
			bError = false;
			IPragmaToken token;
			PragmaTokenType next = PragmaScanner.GetNext(out token);
			_IExpression iExpression = null;
			if (next != PragmaTokenType.Operator)
			{
				return null;
			}
			switch (token.Operator)
			{
			case PragmaOperator.LeftParenthesis:
				iExpression = ParsePragmaORExp(out bError, tokenPragma) ?? LMItemFactory.CreateErrorExpression(tokenPragma);
				if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.RightParenthesis)
				{
					AddErrorST(iExpression, MessageId.Err_OperatorExpected, Scanner.GetOperatorText(Operator.RightParenthesis), PragmaScanner.GetTokenText(token));
				}
				break;
			case PragmaOperator.Not:
			{
				_IExpression exp = ParsePragmaOperand(out bError, tokenPragma) ?? LMItemFactory.CreateErrorExpression(tokenPragma);
				_IPragmaOperatorExpression iPragmaOperatorExpression = LMItemFactory.CreatePragmaOperatorExpression(PragmaOperator.Not, tokenPragma);
				iPragmaOperatorExpression.AddOperand(exp);
				iExpression = iPragmaOperatorExpression;
				break;
			}
			case PragmaOperator.defined:
				iExpression = ParseDefinedPragma(out bError, tokenPragma);
				break;
			case PragmaOperator.project_defined:
				iExpression = ParseProjectDefinedPragma(out bError, tokenPragma);
				if (!bError && Context._bReportSP20Feature)
				{
					_IErrorExpression iErrorExpression = LMItemFactory.CreateErrorExpression();
					iErrorExpression.SetPositionIntern(iExpression.PositionIntern);
					iErrorExpression.LengthIntern = iExpression.LengthIntern;
					iExpression = iErrorExpression;
					Context.AddUnsupportedFeatureError(iExpression, Strings.CompilerFeature_ProjectDefines, ParserContext.CompilerVersion20);
				}
				break;
			case PragmaOperator.xref:
				iExpression = ParseXRefPragma(out bError, tokenPragma);
				break;
			case PragmaOperator.hastype:
				iExpression = ParseHasTypePragma(out bError, tokenPragma);
				break;
			case PragmaOperator.isenumtype:
				iExpression = ParseIsEnumTypePragma(tokenPragma);
				break;
			case PragmaOperator.hasattribute:
				iExpression = ParseHasAttributePragma(out bError, tokenPragma);
				break;
			case PragmaOperator.CompilerVersionPragma:
			{
				_ICompilerVersionExpression verexpr2 = LMItemFactory.CreateCompilerVersionExpression(tokenPragma);
				return ParseVersionPragmaOperand(verexpr2, bUseWarningInsteadOfError: true);
			}
			case PragmaOperator.RuntimeVersionPragma:
			{
				_IRuntimeVersionExpression verexpr = LMItemFactory.CreateRuntimeVersionExpression(tokenPragma);
				return ParseVersionPragmaOperand(verexpr, bUseWarningInsteadOfError: false);
			}
			case PragmaOperator.hasvalue:
				iExpression = ParseHasValuePragma(tokenPragma);
				break;
			case PragmaOperator.hasconstantvalue:
				iExpression = ParseHasConstantValuePragma(tokenPragma);
				break;
			case PragmaOperator.hasconstanttype:
				iExpression = ParseHasConstantTypePragma(tokenPragma);
				break;
			}
			return iExpression;
		}

		private _IExpression ParseDefinedPragma(out bool bError, IToken tokenPragma)
		{
			return DefinedPragmaOperandParser.ParseDefinedPragma(Context, out bError, tokenPragma);
		}

		private _IExpression ParseProjectDefinedPragma(out bool bError, IToken tokenPragma)
		{
			return DefinedPragmaOperandParser.ParseProjectDefinedPragma(Context, out bError, tokenPragma);
		}

		private _IExpression ParseXRefPragma(out bool bError, IToken tokenPragma)
		{
			return XRefPragmaOperandParser.ParseXRefPragma(Context, out bError, tokenPragma);
		}

		private _IExpression ParseHasTypePragma(out bool bError, IToken tokenPragma)
		{
			return HasTypePragmaParser.ParseHasTypePragma(Context, out bError, tokenPragma);
		}

		private _IExpression ParseIsEnumTypePragma(IToken tokenPragma)
		{
			return HasTypePragmaParser.ParseIsEnumTypePragma(Context, tokenPragma);
		}

		private _IExpression ParseHasAttributePragma(out bool bError, IToken tokenPragma)
		{
			return HasAttributePragmaParser.ParseHasAttributePragma(Context, out bError, tokenPragma);
		}

		private _IExpression ParseHasValuePragma(IToken tokenPragma)
		{
			return HasValuePragmaParser.ParseHasValuePragma(Context, tokenPragma);
		}

		private _IExpression ParseHasConstantValuePragma(IToken tokenPragma)
		{
			return HasConstantValueOrTypePragmaParser.ParseHasConstantValuePragma(Context, tokenPragma);
		}

		private _IExpression ParseHasConstantTypePragma(IToken tokenPragma)
		{
			return HasConstantValueOrTypePragmaParser.ParseHasConstantTypePragma(Context, tokenPragma);
		}

		private _IExpression ParsePragmaANDExp(out bool bError, IToken tokenPragma)
		{
			_IPragmaOperatorExpression extop = null;
			_IExpression iExpression = ParsePragmaOperand(out bError, tokenPragma);
			if (iExpression == null)
			{
				return null;
			}
			IPragmaToken token;
			PragmaTokenType next = PragmaScanner.GetNext(out token);
			while (next == PragmaTokenType.Operator && token.Operator == PragmaOperator.And)
			{
				_IExpression iExpression2 = ParsePragmaOperand(out bError, tokenPragma);
				if (iExpression2 == null)
				{
					return null;
				}
				AddPragmaOperandHelp(ref extop, iExpression, iExpression2, token.Operator, tokenPragma);
				next = PragmaScanner.GetNext(out token);
			}
			PragmaScanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private _IExpression ParsePragmaORExp(out bool bError, IToken tokenPragma)
		{
			_IPragmaOperatorExpression extop = null;
			_IExpression iExpression = ParsePragmaANDExp(out bError, tokenPragma);
			if (iExpression == null)
			{
				return null;
			}
			IPragmaToken token;
			PragmaTokenType next = PragmaScanner.GetNext(out token);
			while (next == PragmaTokenType.Operator && token.Operator == PragmaOperator.Or)
			{
				_IExpression iExpression2 = ParsePragmaANDExp(out bError, tokenPragma);
				if (iExpression2 == null)
				{
					return null;
				}
				AddPragmaOperandHelp(ref extop, iExpression, iExpression2, token.Operator, tokenPragma);
				next = PragmaScanner.GetNext(out token);
			}
			PragmaScanner.SetPosition(token);
			if (extop == null)
			{
				return iExpression;
			}
			return extop;
		}

		private void AddPragmaOperandHelp(ref _IPragmaOperatorExpression extop, _IExpression exp1, _IExpression exp2, PragmaOperator op, IToken tokenPragma)
		{
			if (exp2 != null)
			{
				if (extop == null)
				{
					extop = LMItemFactory.CreatePragmaOperatorExpression(op, tokenPragma);
					extop.AddOperand(exp1);
				}
				else if (extop.Code != op)
				{
					_IPragmaOperatorExpression iPragmaOperatorExpression = LMItemFactory.CreatePragmaOperatorExpression(op, tokenPragma);
					iPragmaOperatorExpression.AddOperand(extop);
					extop = iPragmaOperatorExpression;
				}
				extop.AddOperand(exp2);
			}
		}
	}
}
