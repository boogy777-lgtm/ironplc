using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct StructureInitializationParser
	{
		private ParserContext Context { get; }

		private InitializationParser InitializationParser { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private StructureInitializationParser(ParserContext context, InitializationParser initializationParser)
		{
			Context = context;
			InitializationParser = initializationParser;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private void ParseReSyncST(params Operator[] ops)
		{
			Scanner.ParseReSyncST(ops);
		}

		private void ParseReSyncIF()
		{
			Scanner.ParseReSyncIF();
		}

		private Operator MatchOperator(params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError: true, ops);
		}

		private void MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			Scanner.MatchOperator(ErrorHandler, null, bGenerateError, ops);
		}

		internal static _IExpression ParseStructInitialisation(ParserContext context, InitializationParser initializationParser, ref bool bBreakInit, bool bInterface)
		{
			return new StructureInitializationParser(context, initializationParser).ParseStructInitialisation(ref bBreakInit, bInterface);
		}

		private _IExpression ParseStructInitialisation(ref bool bBreakInit, bool bInterface)
		{
			Next(out var token);
			Scanner.SetPosition(token);
			if (CheckForEmptyOrNoStructureInitialization(bInterface, token, out var tokenPosition, out var structureInitialisation))
			{
				return structureInitialisation;
			}
			_IStructureInitialization iStructureInitialization = LMItemFactory.CreateStructureInitialisation(tokenPosition);
			do
			{
				if (!AssertIdentifier(ref bBreakInit, bInterface, iStructureInitialization, out var tokenIdentifier))
				{
					return iStructureInitialization;
				}
				_IVariableExpression expLValue = LMItemFactory.CreateVariableExpression(Scanner.GetIdentifier(tokenIdentifier), tokenIdentifier);
				if (!AssertAssignment(ref bBreakInit, bInterface, iStructureInitialization, out var tokenAssignment))
				{
					return iStructureInitialization;
				}
				_IExpression rValue = InitializationParser.ParseInitialisation(ref bBreakInit);
				if (bBreakInit)
				{
					return iStructureInitialization;
				}
				_IAssignmentExpression iAssignmentExpression = LMItemFactory.CreateAssignmentExpression(expLValue, tokenAssignment);
				iAssignmentExpression._RValue = rValue;
				iStructureInitialization.AddInitValue(iAssignmentExpression);
			}
			while (MatchOperator(Operator.Comma, Operator.RightParenthesis) == Operator.Comma);
			return iStructureInitialization;
		}

		private bool AssertAssignment(ref bool bBreakInit, bool bInterface, _IStructureInitialization structinit, out IToken tokenAssignment)
		{
			if (Next(out tokenAssignment) != TokenType.Operator || Scanner.GetOperator(tokenAssignment) != Operator.Assign)
			{
				if (bInterface)
				{
					ErrorHandler.AddError(tokenAssignment, MessageId.Err_OperatorExpected, Operator.Assign, Scanner.GetTokenText(tokenAssignment));
				}
				else
				{
					ErrorHandler.AddErrorST(structinit, MessageId.Err_OperatorExpected, Operator.Assign, Scanner.GetTokenText(tokenAssignment));
				}
				if (bInterface)
				{
					ParseReSyncIF();
				}
				else
				{
					ParseReSyncST();
				}
				bBreakInit = true;
				return false;
			}
			return true;
		}

		private bool AssertIdentifier(ref bool bBreakInit, bool bInterface, _IStructureInitialization structinit, out IToken tokenIdentifier)
		{
			if (Next(out tokenIdentifier) != TokenType.Identifier)
			{
				if (bInterface)
				{
					ErrorHandler.AddError(tokenIdentifier, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(tokenIdentifier));
				}
				else
				{
					ErrorHandler.AddErrorST(structinit, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(tokenIdentifier));
				}
				if (bInterface)
				{
					ParseReSyncIF();
				}
				else
				{
					ParseReSyncST();
				}
				bBreakInit = true;
				return false;
			}
			return true;
		}

		private bool CheckForEmptyOrNoStructureInitialization(bool bInterface, IToken tokenSave, out IToken tokenPosition, out _IExpression structureInitialisation)
		{
			structureInitialisation = null;
			if (!NextTokenIsStructureOperator(out tokenPosition))
			{
				Scanner.SetPosition(tokenSave);
				if (!bInterface)
				{
					structureInitialisation = null;
					return true;
				}
				if (!LookaheadForStructureInitialization(out tokenPosition, out var tokenIdent))
				{
					Scanner.SetPosition(tokenSave);
					if (LookaheadForEmptyInitialization(out tokenPosition))
					{
						structureInitialisation = LMItemFactory.CreateStructureInitialisation(tokenPosition);
						return true;
					}
					Scanner.SetPosition(tokenSave);
					structureInitialisation = null;
					return true;
				}
				Scanner.SetPosition(tokenIdent);
			}
			else
			{
				MatchOperator(true, Operator.LeftParenthesis);
				if (LookaheadForEmptyStructureInitialization(out var tokenHelp))
				{
					structureInitialisation = LMItemFactory.CreateStructureInitialisation(tokenPosition);
					return true;
				}
				Scanner.SetPosition(tokenHelp);
			}
			return false;
		}

		private bool LookaheadForEmptyStructureInitialization(out IToken tokenHelp)
		{
			if (Next(out tokenHelp) == TokenType.Operator)
			{
				return Scanner.GetOperator(tokenHelp) == Operator.RightParenthesis;
			}
			return false;
		}

		private bool LookaheadForEmptyInitialization(out IToken tokenPosition)
		{
			if (Next(out tokenPosition) == TokenType.Operator && Scanner.GetOperator(tokenPosition) == Operator.LeftParenthesis && Next(out var token) == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.RightParenthesis;
			}
			return false;
		}

		private bool LookaheadForStructureInitialization(out IToken tokenPosition, out IToken tokenIdent)
		{
			tokenPosition = null;
			tokenIdent = null;
			if (Next(out tokenPosition) == TokenType.Operator && Scanner.GetOperator(tokenPosition) == Operator.LeftParenthesis && Next(out tokenIdent) == TokenType.Identifier && Next(out var token) == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.Assign;
			}
			return false;
		}

		private bool NextTokenIsStructureOperator(out IToken tokenPosition)
		{
			if (Next(out tokenPosition) == TokenType.Operator)
			{
				return Scanner.GetOperator(tokenPosition) == Operator.Struct;
			}
			return false;
		}
	}
}
