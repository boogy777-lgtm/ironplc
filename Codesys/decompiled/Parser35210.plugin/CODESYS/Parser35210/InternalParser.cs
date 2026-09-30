using System;
using System.Collections;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Declaration;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210
{
	internal class InternalParser : IInternalParser
	{
		private bool m_bFirstStatement = true;

		private bool Implicit => StatementParser.Implicit;

		public bool ImplicitAnyway
		{
			get
			{
				return StatementParser.ImplicitAnyway;
			}
			set
			{
				StatementParser.ImplicitAnyway = value;
			}
		}

		private DeclarationParser DeclarationParser => Context.DeclarationParser;

		private StatementParser StatementParser => Context.StatementParser;

		private ParserContext Context { get; }

		private _ILanguageModelBuilder7 LMItemFactory => Context.LMItemFactory;

		private ITokenFactory TokenFactory => Context.TokenFactory;

		public IScanner9 Scanner
		{
			get
			{
				return Context.Scanner;
			}
			set
			{
				Context.Scanner = value;
			}
		}

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		public Guid MessageGuid { get; set; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private void SetInDeclaration(bool bValue)
		{
			StatementParser.InDeclaration = bValue;
		}

		private void SetInLibrary(bool bValue)
		{
			DeclarationParser.Library = bValue;
		}

		public InternalParser(ParserContext context)
		{
			Context = context;
		}

		private void SetInSTCode()
		{
			ExpressionParser.SetInSTCode();
		}

		public _IStatement ParsePragma(out bool bError, IMinimalPosition errorpos, string stPragma)
		{
			return Context.PragmaStatementParser.ParsePragmaIntern(out bError, errorpos, stPragma, TokenFactory.Empty());
		}

		private void Next(out IToken token)
		{
			Scanner.Next(out token);
		}

		public TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private _IStatement ParseST()
		{
			return ParseST(bLibrary: false);
		}

		public _IStatement ParseST(bool bLibrary)
		{
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(TokenFactory.Empty());
			iSequenceStatement.SetFlag(StatementFlag.Library, bLibrary);
			MessageGuid = Guid.Empty;
			SetInLibrary(bLibrary);
			while (true)
			{
				_IStatement iStatement = NextStatement();
				if (iStatement == null)
				{
					break;
				}
				if (m_bFirstStatement && iStatement is _IPragmaStatement iPragmaStatement && iPragmaStatement.Text == "ST_IMPLEMENTATION")
				{
					SetInSTCode();
				}
				m_bFirstStatement = false;
				iSequenceStatement.Add(iStatement);
			}
			if (!StatementParser.InDeclaration)
			{
				CodeStatementChecker.CheckForUnexpectedStatementsInImplementation(iSequenceStatement, Context);
			}
			return iSequenceStatement;
		}

		public _IExpression[] ParseSTSnippet()
		{
			ArrayList arrayList = new ArrayList();
			bool flag = false;
			do
			{
				bool bError;
				_IExpression iExpression = ParseAssignExp(out bError);
				if (iExpression != null && !bError)
				{
					if (flag && iExpression.ToString() == "ERROR")
					{
						flag = false;
					}
					else
					{
						arrayList.Add(iExpression);
					}
					continue;
				}
				Next(out var token);
				if (Scanner.GetTokenText(token) == "!")
				{
					flag = true;
				}
			}
			while (Scanner.CurrentToken.Type != TokenType.End);
			_IExpression[] array = new _IExpression[arrayList.Count];
			arrayList.CopyTo(array);
			return array;
		}

		public void ExtendForLoop(_IForStatement forstatement)
		{
			ForLoopExtender.ExtendForLoop(forstatement, Context.LMItemFactory);
		}

		private _IStatement NextStatement()
		{
			bool bError;
			return ParseSTStatement(out bError, bTopLevel: true);
		}

		public _IExpression ParseAssignExp(out bool bError)
		{
			if (Implicit || ImplicitAnyway)
			{
				return ParseInitialisationExp(out bError);
			}
			return ExpressionParser.ParseAssignment(out bError);
		}

		private _IExpression ParseInitialisationExp(out bool bError)
		{
			return ExpressionParser.ParseInitialisationExp(out bError);
		}

		public _IExpression ParseSTOperand(out bool bError)
		{
			return ExpressionParser.ParseSTOperand(out bError);
		}

		public _IExpression ParseInitialisation()
		{
			return ExpressionParser.ParseInitialisation();
		}

		internal _IStatement ParseSTStatement(out bool bError)
		{
			return ParseSTStatement(out bError, bTopLevel: false);
		}

		private _IStatement ParseSTStatement(out bool bError, bool bTopLevel)
		{
			return StatementParser.ParseSTStatement(out bError, bTopLevel);
		}

		public Operator MatchOperator(params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError: true, ops);
		}

		public _IType ParseType()
		{
			return Context.TypeParser.ParseType(bTop: true, bTry: false, TokenFactory.Empty());
		}

		public _IStatement ParseInterfaceStatement()
		{
			SetInDeclaration(bValue: true);
			_IStatement iStatement = ParseST();
			SetInDeclaration(bValue: false);
			DeclarationStatementChecker.PreventAnyCodeAfterTypeDeclaration(iStatement, Context);
			return iStatement;
		}

		public IPOUSyntax[] ParsePOUs()
		{
			return POUSyntaxParser.ParsePOUSyntax(Context);
		}

		public _ISequenceStatement ParseRawST()
		{
			Context.Scanner.IncludePragmas = true;
			Context.Scanner.IncludeComments = true;
			Context.Scanner.AutoIncrementPositionOnLineBreaks = true;
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(TokenFactory.Empty());
			MessageGuid = Guid.Empty;
			while (true)
			{
				_IStatement iStatement = NextStatement();
				if (iStatement == null)
				{
					break;
				}
				AddStatementAndHandleDeclarations(iStatement, iSequenceStatement);
			}
			return iSequenceStatement;
		}

		private static void AddStatementAndHandleDeclarations(_IStatement statement, _ISequenceStatement sequenceStatement)
		{
			sequenceStatement._StatementList.Add(statement);
			int count = sequenceStatement._StatementList.Count;
			if (statement is _IPOUDeclarationStatement iPOUDeclarationStatement && iPOUDeclarationStatement.Declarations is _ISequenceStatement iSequenceStatement)
			{
				while (!(iSequenceStatement._StatementList.Last() is _IVariableDeclarationListStatement))
				{
					_IStatement iStatement = iSequenceStatement._StatementList.Last();
					iSequenceStatement._StatementList.RemoveAt(iSequenceStatement._StatementList.Count - 1);
					sequenceStatement.InsertStatement(count, iStatement);
					iPOUDeclarationStatement.LengthIntern -= iStatement.LengthIntern;
				}
			}
		}
	}
}
