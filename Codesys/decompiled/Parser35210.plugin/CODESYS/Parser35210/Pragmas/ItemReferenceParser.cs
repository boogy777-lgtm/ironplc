using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Pragmas
{
	internal readonly struct ItemReferenceParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private PragmaStatementParser PragmaParser => Context.PragmaStatementParser;

		private IPragmaScanner PragmaScanner => PragmaParser.PragmaScanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ItemReferenceParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IItemReference ParseItemReference(ParserContext context, out bool bError, IToken tokenPragma)
		{
			return new ItemReferenceParser(context).ParseItemReference(out bError, tokenPragma);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IItemReference ParseItemReference(out bool bError, IToken tokenPragma)
		{
			bError = false;
			IPragmaToken token;
			PragmaTokenType next = PragmaScanner.GetNext(out token);
			_IItemReference result = null;
			switch (next)
			{
			case PragmaTokenType.Operator:
				switch (token.Operator)
				{
				case PragmaOperator.variable:
					result = ParseInstancePath(out bError, tokenPragma);
					break;
				case PragmaOperator.type:
					result = ParseTypeReference(tokenPragma);
					break;
				case PragmaOperator.pou:
					result = ParsePOUReference(tokenPragma);
					break;
				case PragmaOperator.task:
					result = ParseTaskReference(tokenPragma);
					break;
				case PragmaOperator.resource:
					result = ParseResourceReference(tokenPragma);
					break;
				}
				break;
			case PragmaTokenType.Identifier:
				result = LMItemFactory.CreateDefineReference(tokenPragma, token.Identifier);
				break;
			}
			return result;
		}

		private _IItemReference ParseResourceReference(IToken tokenPragma)
		{
			_IResourceReference iResourceReference = LMItemFactory.CreateResourceReference(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Colon)
			{
				AddErrorST(iResourceReference, MessageId.Err_OperatorExpected, ":", PragmaScanner.GetTokenText(token));
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Identifier)
			{
				AddErrorST(iResourceReference, MessageId.Err_IdentifierExpected, PragmaScanner.GetTokenText(token));
			}
			else
			{
				iResourceReference.ResourceName = token.Identifier;
			}
			return iResourceReference;
		}

		private _IItemReference ParseTaskReference(IToken tokenPragma)
		{
			_ITaskReference iTaskReference = LMItemFactory.CreateTaskReference(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Colon)
			{
				AddErrorST(iTaskReference, MessageId.Err_OperatorExpected, ":", PragmaScanner.GetTokenText(token));
			}
			if (PragmaScanner.GetNext(out token) != PragmaTokenType.Identifier)
			{
				AddErrorST(iTaskReference, MessageId.Err_IdentifierExpected, PragmaScanner.GetTokenText(token));
			}
			else
			{
				iTaskReference.TaskName = token.Identifier;
			}
			return iTaskReference;
		}

		private _IItemReference ParsePOUReference(IToken tokenPragma)
		{
			_IPouReference iPouReference = LMItemFactory.CreatePouReference(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Colon)
			{
				AddErrorST(iPouReference, MessageId.Err_OperatorExpected, ":", PragmaScanner.GetTokenText(token));
			}
			iPouReference.InstancePath = ParsePragmaQualifiedNameExpression(iPouReference) ?? LMItemFactory.CreateVariableExpression(string.Empty);
			return iPouReference;
		}

		private _IItemReference ParseTypeReference(IToken tokenPragma)
		{
			_ITypeReference iTypeReference = LMItemFactory.CreateTypeReference(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Colon)
			{
				AddErrorST(iTypeReference, MessageId.Err_OperatorExpected, ":", PragmaScanner.GetTokenText(token));
			}
			iTypeReference.InstancePath = ParsePragmaQualifiedNameExpression(iTypeReference);
			return iTypeReference;
		}

		private _IItemReference ParseInstancePath(out bool bError, IToken tokenPragma)
		{
			_IVariableReference iVariableReference = LMItemFactory.CreateVariableReference(tokenPragma);
			if (PragmaScanner.GetNext(out var token) != PragmaTokenType.Operator || token.Operator != PragmaOperator.Colon)
			{
				AddErrorST(iVariableReference, MessageId.Err_OperatorExpected, ":", PragmaScanner.GetTokenText(token));
			}
			iVariableReference.InstancePath = ParsePragmaInstancePath(out bError);
			return iVariableReference;
		}

		private _IExpression ParsePragmaInstancePath(out bool bError)
		{
			IScanner9 scanner = Context.Scanner;
			Context.Scanner = PragmaScanner.OrgScanner;
			_IExpression iExpression = Context.ExpressionParser.ParseSTOperand(out bError);
			Context.Scanner = scanner;
			if (iExpression != null)
			{
				new PragmaSourcePositionAdjuster(scanner.CurrentToken.Position, PragmaScanner.PositionOffset, Context.TokenFactory).AdjustSourcePositions(iExpression);
			}
			return iExpression;
		}

		private _IExpression ParsePragmaQualifiedNameExpression(_IExprement expForError)
		{
			IScanner9 scanner = Context.Scanner;
			Context.Scanner = PragmaScanner.OrgScanner;
			_IExpression iExpression = Context.ExpressionParser.ParseQualifiedNameExpression(expForError);
			Context.Scanner = scanner;
			if (iExpression != null)
			{
				new PragmaSourcePositionAdjuster(scanner.CurrentToken.Position, PragmaScanner.PositionOffset, Context.TokenFactory).AdjustSourcePositions(iExpression);
			}
			return iExpression;
		}
	}
}
