using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal readonly struct VariableDeclarationParser
	{
		private readonly _IVariableDeclarationStatement _vds;

		private ParserContext Context { get; }

		private Operator OpCurrentVariableList => DeclarationParser.OpCurrentVariableList;

		private DeclarationParser DeclarationParser => Context.DeclarationParser;

		private bool AllowPaths => DeclarationParser.AllowPaths;

		private TypeParser TypeParser => Context.TypeParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private VariableDeclarationParser(ParserContext context, IToken token)
		{
			Context = context;
			_vds = Context.LMItemFactory.CreateVariableDeclarationStatement(token);
		}

		internal static _IVariableDeclarationStatement ParseVariableDeclaration(ParserContext context, IToken token)
		{
			return new VariableDeclarationParser(context, token).ParseVariableDeclaration(token);
		}

		private void ParseReSyncIF()
		{
			Scanner.ParseReSyncIF();
		}

		private Operator MatchOperator(_IExprement exp, params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, exp, ops);
		}

		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private _IExpression ParseInitialisation()
		{
			return ExpressionParser.ParseInitialisation();
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IVariableDeclarationStatement ParseVariableDeclaration(IToken tokenIdent)
		{
			Operator opNext = Operator.None;
			Scanner.SetPosition(tokenIdent);
			if (ParseNamesReturnError(ref opNext))
			{
				return _vds;
			}
			if (opNext == Operator.At && ParseAddressReturnError())
			{
				return _vds;
			}
			_vds.Type = TypeParser.ParseType(bTop: true, bTry: false, tokenIdent);
			if (_vds.Type != null)
			{
				Operator op = MatchOperator(_vds, Operator.Semicolon, Operator.Assign, Operator.RefAssign, Operator.LeftParenthesis, Operator.LeftBracket);
				op = TryParseInitInputAssignments(op);
				switch (op)
				{
				case Operator.Assign:
				case Operator.RefAssign:
					_vds.Initial = ParseInitialisation();
					MatchOperator(_vds, Operator.Semicolon);
					_vds.RefAssignInitialisation = op == Operator.RefAssign;
					break;
				default:
					return _vds;
				case Operator.Semicolon:
					break;
				}
			}
			return _vds;
		}

		private Operator TryParseInitInputAssignments(Operator op)
		{
			switch (op)
			{
			case Operator.LeftBracket:
				MatchOperator(_vds, Operator.LeftParenthesis);
				do
				{
					ParseInitialisationAssignments(_vds);
				}
				while (MatchOperator(_vds, Operator.Comma, Operator.RightBracket) == Operator.Comma && MatchOperator(_vds, Operator.LeftParenthesis) == Operator.LeftParenthesis);
				op = MatchOperator(_vds, Operator.Semicolon, Operator.Assign, Operator.RefAssign);
				break;
			case Operator.LeftParenthesis:
				ParseInitialisationAssignments(_vds);
				_vds.OldInputAssigns = true;
				op = MatchOperator(_vds, Operator.Semicolon, Operator.Assign, Operator.RefAssign);
				break;
			}
			return op;
		}

		private bool ParseAddressReturnError()
		{
			if (Next(out var token) == TokenType.DirectVariable)
			{
				Scanner.GetDirectVariable(token, out var location, out var size, out var components, out var _);
				_vds.Address = LMItemFactory.CreateDirectVariable(location, size, components);
			}
			else
			{
				if (token.Type != TokenType.IncompleteDirectVariable)
				{
					AddErrorST(_vds, MessageId.Err_AddressExpected, Scanner.GetTokenText(token));
					ParseReSyncIF();
					return true;
				}
				Scanner.GetIncompleteDirectVariable(token, out var location2);
				_vds.Address = LMItemFactory.CreateIncompleteDirectVariable(location2);
			}
			MatchOperator(_vds, Operator.Colon);
			return false;
		}

		private bool ParseNamesReturnError(ref Operator opNext)
		{
			do
			{
				bool bError = false;
				_IExpression exp = null;
				IToken token;
				if (OpCurrentVariableList == Operator.VarConfig || OpCurrentVariableList == Operator.VarExternal || AllowPaths)
				{
					exp = ExpressionParser.ParseSTOperand(out bError);
				}
				else if (Next(out token) != TokenType.Identifier)
				{
					AddErrorSTWithToken(_vds, token, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token));
					bError = true;
				}
				else
				{
					exp = LMItemFactory.CreateVariableExpression(Scanner.GetIdentifier(token), token);
				}
				if (bError)
				{
					break;
				}
				_vds.AddName(exp);
				opNext = MatchOperator(_vds, Operator.Comma, Operator.At, Operator.Colon);
				if (opNext == Operator.None)
				{
					return true;
				}
			}
			while (opNext == Operator.Comma);
			return false;
		}

		private void ParseInitialisationAssignments(_IVariableDeclarationStatement vds)
		{
			if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.RightParenthesis)
			{
				return;
			}
			Scanner.SetPosition(token);
			IToken tokenTest;
			bool bErrorLocal;
			Operator nextOperator;
			do
			{
				string stIdent = string.Empty;
				tokenTest = ParseInputName(ref stIdent);
				_IExpression iExpression = ParseInitialisation();
				bErrorLocal = CheckForInvalidInitialisation(iExpression);
				_IVariableExpression expVariable = null;
				if (stIdent != string.Empty)
				{
					expVariable = LMItemFactory.CreateVariableExpression(stIdent, tokenTest);
				}
				vds.AddParam(iExpression, expVariable);
				nextOperator = GetNextOperator(bErrorLocal, ref tokenTest);
			}
			while (CheckForContinuation(vds, nextOperator, bErrorLocal, tokenTest));
		}

		private bool CheckForContinuation(_IVariableDeclarationStatement vds, Operator opTest, bool bErrorLocal, IToken tokenTest)
		{
			switch (opTest)
			{
			case Operator.Comma:
			{
				if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.RightParenthesis)
				{
					return false;
				}
				Scanner.SetPosition(token);
				return true;
			}
			case Operator.RightParenthesis:
				return false;
			default:
				if (!bErrorLocal)
				{
					AddErrorSTWithToken(vds, tokenTest, MessageId.Err_Operator1of2Expected, Scanner.GetOperatorText(Operator.Comma), Scanner.GetOperatorText(Operator.RightParenthesis), Scanner.GetTokenText(tokenTest));
					opTest = Scanner.ParseReSyncST(Operator.Comma, Operator.RightParenthesis);
					switch (opTest)
					{
					case Operator.Comma:
						return true;
					case Operator.RightParenthesis:
						return false;
					}
				}
				Scanner.SetPosition(tokenTest);
				return false;
			}
		}

		private Operator GetNextOperator(bool bErrorLocal, ref IToken tokenTest)
		{
			Operator result = Operator.None;
			if (bErrorLocal)
			{
				result = Scanner.ParseReSyncST(Operator.Comma, Operator.RightParenthesis);
			}
			else if (Next(out tokenTest) == TokenType.Operator)
			{
				result = Scanner.GetOperator(tokenTest);
			}
			return result;
		}

		private bool CheckForInvalidInitialisation(_IExpression expParam)
		{
			bool result = false;
			if (expParam is IStructureInitialization)
			{
				AddErrorST(expParam, MessageId.Err_StructureInitialisationNotPossible);
				result = true;
			}
			else if (expParam is IArrayInitialization)
			{
				AddErrorST(expParam, MessageId.Err_ArrayInitialisationNotPossible);
				result = true;
			}
			return result;
		}

		private IToken ParseInputName(ref string stIdent)
		{
			if (Next(out var token) == TokenType.Identifier)
			{
				if (Next(out var token2) == TokenType.Operator)
				{
					if (Scanner.GetOperator(token2) == Operator.Assign)
					{
						stIdent = Scanner.GetIdentifier(token);
					}
					else
					{
						Scanner.SetPosition(token);
					}
				}
				else
				{
					Scanner.SetPosition(token);
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			return token;
		}
	}
}
