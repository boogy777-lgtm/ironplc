using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Statements;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	public class POUDeclarationParser
	{
		private readonly _IPOUDeclarationStatement _pds;

		private ParserContext Context { get; }

		private DeclarationParser DeclarationParser => Context.DeclarationParser;

		private TypeParser TypeParser => Context.TypeParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private StatementParser StatementParser => Context.StatementParser;

		private bool InsideDeclarationVarDecl
		{
			get
			{
				return StatementParser.InsideDeclarationVarDecl;
			}
			set
			{
				StatementParser.InsideDeclarationVarDecl = value;
			}
		}

		private ITokenFactory TokenFactory => Context.TokenFactory;

		private POUDeclarationParser(ParserContext context, IToken token)
		{
			Context = context;
			_pds = Context.LMItemFactory.CreatePOUDeclarationStatement(token);
		}

		internal static _IPOUDeclarationStatement ParsePOUDeclaration(ParserContext context, IToken token, Operator op)
		{
			return new POUDeclarationParser(context, token).ParsePOUDeclaration(op);
		}

		private void MatchOperator(_IExprement exp, params Operator[] ops)
		{
			Scanner.MatchOperator(ErrorHandler, exp, ops);
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IPOUDeclarationStatement ParsePOUDeclaration(Operator opParam)
		{
			bool insideDeclarationVarDecl = InsideDeclarationVarDecl;
			InsideDeclarationVarDecl = false;
			ParsePOUDeclarationInternal(opParam);
			InsideDeclarationVarDecl = insideDeclarationVarDecl;
			return _pds;
		}

		private void ParsePOUDeclarationInternal(Operator opParam)
		{
			_pds.Class = opParam;
			Next(out var token);
			token = ScannAccessSpecifier(token);
			if (!CheckForName(token))
			{
				return;
			}
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(TokenFactory.Empty());
			string identifier = Scanner.GetIdentifier(token);
			_pds.Name = identifier;
			_IVariableExpression nameExpression = LMItemFactory.CreateVariableExpression(identifier, token);
			_pds.NameExpression = nameExpression;
			switch (TryParseGenericVarList(iSequenceStatement, out token))
			{
			case Operator.Colon:
				_pds.Type = TypeParser.ParseType();
				break;
			case Operator.Extends:
				ParseExtendsList();
				break;
			case Operator.Implements:
				ParseImplementsList();
				break;
			default:
				Scanner.SetPosition(token);
				break;
			case Operator.None:
				Scanner.SetPosition(token);
				break;
			}
			while (true)
			{
				_IStatement iStatement = StatementParser.TryParseVariableDeclarationList();
				if (iStatement == null)
				{
					break;
				}
				iSequenceStatement.Add(iStatement);
			}
			_pds.Declarations = iSequenceStatement;
		}

		private void ParseImplementsList()
		{
			_IExpression iExpression = ExpressionParser.ParseQualifiedNameExpression(_pds);
			if (iExpression != null)
			{
				_pds.AddInterfaceImplementation(iExpression);
			}
			if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.Comma)
			{
				ParseImplementsList();
			}
			else
			{
				Scanner.SetPosition(token);
			}
		}

		private void ParseExtendsList()
		{
			_IType iType = TypeParser.ParseUserdefType();
			_IExpression iExpression = null;
			if (iType is IGenericUserdefType)
			{
				iExpression = LMItemFactory.CreateTypeExpression(iType);
			}
			else if (iType is _IUserdefType iUserdefType)
			{
				iExpression = iUserdefType.NameExpression as _IExpression;
			}
			if (iExpression != null)
			{
				_pds.Extends.Add(iExpression);
			}
			if (Next(out var token) == TokenType.Operator)
			{
				if (Scanner.GetOperator(token) == Operator.Implements)
				{
					ParseImplementsList();
				}
				else if (Scanner.GetOperator(token) == Operator.Comma)
				{
					ParseExtendsList();
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

		private Operator TryParseGenericVarList(_ISequenceStatement seq, out IToken token)
		{
			Operator @operator = Operator.None;
			if (Next(out token, bWithPragma: true, bWithComment: false) == TokenType.Operator)
			{
				@operator = Scanner.GetOperator(token);
			}
			if (@operator == Operator.VarGeneric)
			{
				_IStatement iStatement = DeclarationParser.ParseVariableList(token);
				MatchOperator(iStatement, Operator.EndVar);
				@operator = Operator.None;
				if (Next(out token) == TokenType.Operator)
				{
					@operator = Scanner.GetOperator(token);
				}
				if (Context._bReportSP18Feature)
				{
					Context.AddUnsupportedFeatureError(iStatement, Strings.CompilerFeature_GenericConstantVariable, ParserContext.CompilerVersion18);
				}
				seq.Add(iStatement);
			}
			return @operator;
		}

		private bool CheckForName(IToken token)
		{
			if (token.Type != TokenType.Identifier)
			{
				AddErrorST(_pds, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token));
				return false;
			}
			return true;
		}

		private IToken ScannAccessSpecifier(IToken token)
		{
			if (token.Type == TokenType.Operator)
			{
				switch (Scanner.GetOperator(token))
				{
				case Operator.Private:
					_pds.SetAccessFlag(SignatureFlag.Private);
					break;
				case Operator.Protected:
					_pds.SetAccessFlag(SignatureFlag.Protected);
					break;
				case Operator.Internal:
					_pds.SetAccessFlag(SignatureFlag.Internal);
					break;
				case Operator.Abstract:
					_pds.SetAccessFlag(SignatureFlag.Abstract);
					break;
				case Operator.Final:
					_pds.SetAccessFlag(SignatureFlag.Final);
					break;
				default:
					Scanner.SetPosition(token);
					break;
				case Operator.Public:
					break;
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			Next(out token);
			if (token.Type == TokenType.Operator)
			{
				switch (Scanner.GetOperator(token))
				{
				case Operator.Abstract:
					_pds.SetAccessFlag(SignatureFlag.Abstract);
					break;
				case Operator.Final:
					_pds.SetAccessFlag(SignatureFlag.Final);
					break;
				default:
					Scanner.SetPosition(token);
					break;
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			Next(out token);
			return token;
		}
	}
}
