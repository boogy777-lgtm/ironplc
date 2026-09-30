using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal readonly struct EnumListParser
	{
		private ParserContext Context { get; }

		private IScanner9 Scanner => Context.Scanner;

		private StatementParser StatementParser => Context.StatementParser;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder7 LMItemFactory => Context.LMItemFactory;

		private ITypeTable3 TypeTable => Context.TypeTable;

		private TypeParser TypeParser => Context.TypeParser;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private Operator MatchOperator(_IExprement exprement, params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, exprement, ops);
		}

		private EnumListParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IEnumDeclarationListStatement ParseEnumList(ParserContext context, IToken tokenParenthesis, string stEnumName)
		{
			return new EnumListParser(context).ParseEnumList(tokenParenthesis, stEnumName);
		}

		private _IEnumDeclarationListStatement ParseEnumList(IToken tokenParenthesis, string stEnumName)
		{
			_IEnumDeclarationListStatement iEnumDeclarationListStatement = LMItemFactory.CreateEnumDeclarationListStatement(tokenParenthesis);
			Operator @operator;
			do
			{
				_IExpression expInit = null;
				_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement();
				if (!GetIdentifierToken(iSequenceStatement, iEnumDeclarationListStatement, out var identifierToken))
				{
					break;
				}
				string identifier = Scanner.GetIdentifier(identifierToken);
				IToken nextTokenAndCollectPragmaAndCommentStatements = GetNextTokenAndCollectPragmaAndCommentStatements(iSequenceStatement);
				Scanner.SetPosition(nextTokenAndCollectPragmaAndCommentStatements);
				@operator = MatchOperator(iEnumDeclarationListStatement, Operator.Assign, Operator.Comma, Operator.RightParenthesis);
				if (@operator == Operator.Assign)
				{
					IToken currentToken = Scanner.CurrentToken;
					expInit = ExpressionParser.ParseInitialisationExp(out var _);
					@operator = MatchOperator(iEnumDeclarationListStatement, Operator.Comma, Operator.RightParenthesis);
					if (@operator != 0)
					{
						Scanner.SetPosition(currentToken);
						ParseEndOfDeclaration(stEnumName, iSequenceStatement, iEnumDeclarationListStatement, @operator);
					}
				}
				if (iSequenceStatement._StatementList.Count < 1)
				{
					iSequenceStatement = null;
				}
				iEnumDeclarationListStatement.AddEnumDeclaration(identifier, expInit, iSequenceStatement, identifierToken);
			}
			while (@operator == Operator.Comma);
			Scanner.Next(out var token);
			Scanner.SetPosition(token);
			if (token.Type == TokenType.Operator && Scanner.GetOperator(token) != Operator.Semicolon && Scanner.GetOperator(token) != Operator.Assign)
			{
				iEnumDeclarationListStatement._BaseType = TypeParser.ParseType() ?? TypeTable.Int;
			}
			return iEnumDeclarationListStatement;
		}

		private void ParseEndOfDeclaration(string stEnumName, _ISequenceStatement seqAttributesEtc, _IEnumDeclarationListStatement edls, Operator opMatch)
		{
			bool flag = true;
			int num = 0;
			TokenType tokenType = TokenType.None;
			bool flag2;
			do
			{
				bool num2 = TokenType.Pragma == tokenType;
				tokenType = Scanner.Next(out var token, flag, bWithComment: true);
				if (num2 && !flag)
				{
					flag = true;
				}
				flag2 = false;
				switch (tokenType)
				{
				case TokenType.End:
					flag2 = true;
					break;
				case TokenType.Comment:
				case TokenType.DocComment:
					ParseComment(seqAttributesEtc, token);
					break;
				case TokenType.Pragma:
					ParsePragma(stEnumName, edls, token);
					flag = false;
					break;
				case TokenType.Operator:
				{
					Operator @operator = Scanner.GetOperator(token);
					if (@operator == opMatch && num == 0)
					{
						flag2 = true;
					}
					else
					{
						num = CalculateParenthesisBalance(num, @operator);
					}
					break;
				}
				}
			}
			while (!flag2);
		}

		private static int CalculateParenthesisBalance(int paranthesisBalance, Operator opTest)
		{
			switch (opTest)
			{
			case Operator.LeftParenthesis:
				return paranthesisBalance + 1;
			case Operator.RightParenthesis:
				return paranthesisBalance - 1;
			default:
				return paranthesisBalance;
			}
		}

		private void ParsePragma(string stEnumName, _IEnumDeclarationListStatement edls, IToken token)
		{
			Scanner.SetPosition(token);
			if (StatementParser.ParseSTStatement(out var _, bTopLevel: false) is _IPragmaIfStatement iPragmaIfStatement && iPragmaIfStatement.ConditionExpression is _IProjectDefinedExpression && stEnumName != null)
			{
				ErrorHandler.AddErrorSTWithToken(edls, token, MessageId.Err_ProjectDefinedNotSupportedFor, stEnumName);
			}
			Scanner.SetPosition(token);
		}

		private void ParseComment(_ISequenceStatement seqAttributesEtc, IToken token)
		{
			Scanner.SetPosition(token);
			bool bError;
			_IStatement sm = StatementParser.ParseSTStatement(out bError, bTopLevel: false);
			if (bError)
			{
				Scanner.ParseReSyncIF();
			}
			seqAttributesEtc.Add(sm);
		}

		private bool GetIdentifierToken(_ISequenceStatement seqAttributesEtc, _IEnumDeclarationListStatement edls, out IToken identifierToken)
		{
			identifierToken = GetNextTokenAndCollectPragmaAndCommentStatements(seqAttributesEtc);
			if (identifierToken.Type != TokenType.Identifier)
			{
				ErrorHandler.AddErrorSTWithToken(edls, identifierToken, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(identifierToken));
				return false;
			}
			return true;
		}

		private IToken GetNextTokenAndCollectPragmaAndCommentStatements(_ISequenceStatement seqAttributesEtc)
		{
			IToken token;
			while (true)
			{
				TokenType tokenType = Scanner.Next(out token, bWithPragma: true, bWithComment: true);
				if (tokenType != TokenType.Comment && tokenType != TokenType.DocComment && tokenType != TokenType.Pragma)
				{
					break;
				}
				Scanner.SetPosition(token);
				bool bError;
				_IStatement sm = StatementParser.ParseSTStatement(out bError, bTopLevel: false);
				if (bError)
				{
					Scanner.ParseReSyncIF();
				}
				seqAttributesEtc.Add(sm);
			}
			return token;
		}
	}
}
