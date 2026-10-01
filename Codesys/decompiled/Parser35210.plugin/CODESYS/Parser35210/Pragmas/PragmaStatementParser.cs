using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.PragmaScanner;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	public class PragmaStatementParser
	{
		internal readonly IPragmaScanner PragmaScanner;

		private ParserContext Context { get; }

		private _ILanguageModelBuilder7 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private Guid MessageGuid
		{
			get
			{
				return Context.InternalParser.MessageGuid;
			}
			set
			{
				Context.InternalParser.MessageGuid = value;
			}
		}

		private StatementParser StatementParser => Context.StatementParser;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		internal PragmaStatementParser(ParserContext context)
		{
			_ILanguageModelBuilder8 iLanguageModelBuilder = (_ILanguageModelBuilder8)context.LMItemFactory;
			PragmaScanner = new CODESYS.Parser35210.PragmaScanner.PragmaScanner((IScanner9)iLanguageModelBuilder.CreateScanner(""));
			Context = context;
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		internal _IStatement ParsePragma(out bool bError, IToken token)
		{
			_IStatement iStatement = ParsePragmaIntern(out bError, token);
			if (iStatement == null)
			{
				return CreatePragmaStatement(token);
			}
			return iStatement;
		}

		private _IStatement CreatePragmaStatement(IToken token)
		{
			_IPragmaStatement iPragmaStatement = LMItemFactory.CreatePragmaStatement(token);
			iPragmaStatement.Text = Scanner.GetPragma(token);
			return iPragmaStatement;
		}

		private _IStatement ParsePragmaIntern(out bool bError, IToken tokenPragma)
		{
			string pragma = Scanner.GetPragma(tokenPragma);
			return ParsePragmaIntern(out bError, null, pragma, tokenPragma);
		}

		internal _IStatement ParsePragmaIntern(out bool bError, IMinimalPosition errorpos, string stPragma, IToken tokenPragma)
		{
			PragmaScanner.Reset(stPragma, tokenPragma);
			bError = false;
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator)
			{
				return null;
			}
			switch (token.Operator)
			{
			case PragmaOperator.allowpaths:
				Context.DeclarationParser.AllowPaths = true;
				break;
			case PragmaOperator.opimplicit:
			{
				bool flag = PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.off;
				Scanner.AllowMultipleUnderlines = flag || Context.StatementParser.ImplicitAnyway;
				Context.StatementParser.Implicit = flag;
				return LMItemFactory.CreateImplicitCodeSectionPragma(tokenPragma, stPragma, flag);
			}
			case PragmaOperator.messageguid:
				return ParseMessageGuidPragma(tokenPragma);
			case PragmaOperator.define:
				return ParseDefinePragma(tokenPragma);
			case PragmaOperator.undefine:
				return ParseUndefinePragma(tokenPragma);
			case PragmaOperator.bpdef:
				return ParseBreakPointStatement(tokenPragma);
			case PragmaOperator.error:
				return ParseErrorPragma(tokenPragma, errorpos, Severity.Error);
			case PragmaOperator.fatalerror:
				return ParseErrorPragma(tokenPragma, errorpos, Severity.FatalError);
			case PragmaOperator.warning:
				return ParseWarningPragma(out bError, errorpos, tokenPragma);
			case PragmaOperator.text:
				return ParseTextPragma(out bError, errorpos, tokenPragma, Severity.Text);
			case PragmaOperator.info:
				return ParseTextPragma(out bError, errorpos, tokenPragma, Severity.Information);
			case PragmaOperator.flow:
				StatementParser.Flow = true;
				break;
			case PragmaOperator.noflow:
				StatementParser.Flow = false;
				break;
			case PragmaOperator.bp:
				StatementParser.Bp = true;
				break;
			case PragmaOperator.nobp:
				StatementParser.Bp = false;
				break;
			case PragmaOperator.nobp2:
				StatementParser.DisableBp();
				break;
			case PragmaOperator.bp2:
				StatementParser.EnableBp();
				break;
			case PragmaOperator.If:
				return ParsePragmaIf(out bError, tokenPragma);
			case PragmaOperator.assert:
				return ParsePragmaAssert(out bError, tokenPragma);
			case PragmaOperator.Elsif:
			case PragmaOperator.Else:
			case PragmaOperator.EndIf:
			{
				_IPragmaStatement iPragmaStatement = LMItemFactory.CreatePragmaStatement(tokenPragma);
				iPragmaStatement.Text = stPragma;
				AddErrorST(iPragmaStatement, MessageId.Err_UnexpectedPragmaif, PragmaScanner.GetTokenText(token));
				return iPragmaStatement;
			}
			}
			return null;
		}

		private _IStatement ParseTextPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma, Severity severity)
		{
			return ErrorPragmaParser.ParseTextPragma(Context, out bError, tokenPragma, errorpos, severity);
		}

		private _IStatement ParseWarningPragma(out bool bError, IMinimalPosition errorpos, IToken tokenPragma)
		{
			return ErrorPragmaParser.ParseWarningPragma(Context, out bError, errorpos, tokenPragma);
		}

		private _IStatement ParseBreakPointStatement(IToken tokenPragma)
		{
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Integer)
			{
				return null;
			}
			long integer = token.Integer;
			long invalidPosition = Helper.InvalidPosition;
			if (PragmaScanner.GetNext(out token) == PragmaTokenType.Operator && token.Operator == PragmaOperator.succ && PragmaScanner.GetNext(out token) == PragmaTokenType.Integer)
			{
				return null;
			}
			return LMItemFactory.CreateBreakPointStatement(tokenPragma, integer, invalidPosition);
		}

		private _IStatement ParseUndefinePragma(IToken tokenPragma)
		{
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Identifier)
			{
				return null;
			}
			string identifier = token.Identifier;
			return LMItemFactory.CreateDefineStatement(tokenPragma, bDefine: false, identifier);
		}

		private _IStatement ParseDefinePragma(IToken tokenPragma)
		{
			string stValue = null;
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Identifier)
			{
				return null;
			}
			string identifier = token.Identifier;
			if (PragmaScanner.GetNext(out token) == PragmaTokenType.SingleByteString)
			{
				stValue = token.String;
			}
			return LMItemFactory.CreateDefineStatement(tokenPragma, bDefine: true, identifier, stValue);
		}

		private _IStatement ParseMessageGuidPragma(IToken tokenPragma)
		{
			if (PragmaScanner.GetNext(out var token) == PragmaTokenType.SingleByteString)
			{
				Guid messageGuid;
				try
				{
					messageGuid = new Guid(token.String);
				}
				catch
				{
					messageGuid = Guid.Empty;
				}
				MessageGuid = messageGuid;
				_IMessageGuidPragmaStatement iMessageGuidPragmaStatement = LMItemFactory.CreateMessageGuidPragmaStatement(tokenPragma, MessageGuid);
				iMessageGuidPragmaStatement.Text = Scanner.GetTokenText(tokenPragma);
				return iMessageGuidPragmaStatement;
			}
			return null;
		}

		private _IStatement ParseErrorPragma(IToken tokenPragma, IMinimalPosition errorpos, Severity sev)
		{
			return ErrorPragmaParser.ParseErrorPragma(Context, tokenPragma, errorpos, sev);
		}

		internal _IExpression ParsePragmaORExp(out bool bError, IToken tokenPragma)
		{
			return PragmaOperandParser.ParsePragmaORExp(Context, out bError, tokenPragma);
		}

		private _IStatement ParsePragmaAssert(out bool bError, IToken tokenPragma)
		{
			bError = false;
			string stErrorMessage = string.Empty;
			_IExpression conditionExpression = GetConditionExpression(tokenPragma, ref stErrorMessage);
			return LMItemFactory.CreatePragmaAssertion(conditionExpression, stErrorMessage, tokenPragma);
		}

		private _IExpression GetConditionExpression(IToken tokenPragma, ref string stErrorMessage)
		{
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.LeftParenthesis)
			{
				return null;
			}
			bool bError;
			_IExpression result = ParsePragmaORExp(out bError, tokenPragma);
			if (bError)
			{
				return result;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Comma)
			{
				return result;
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.SingleByteString)
			{
				return result;
			}
			stErrorMessage = token.String;
			return result;
		}

		private _IStatement ParsePragmaIf(out bool bError, IToken tokenPragma)
		{
			return PragmaIfStatementParser.ParsePragmaIfStatement(Context, out bError, tokenPragma);
		}
	}
}
