using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal readonly struct TypeDeclarationParser
	{
		private readonly _ITypeDeclarationStatement _tds;

		private ParserContext Context { get; }

		private DeclarationParser DeclarationParser => Context.DeclarationParser;

		private TypeParser TypeParser => Context.TypeParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private TypeDeclarationParser(ParserContext context, IToken token)
		{
			Context = context;
			_tds = Context.LMItemFactory.CreateTypeDeclarationStatement(token);
		}

		internal static _ITypeDeclarationStatement ParseTypeDeclaration(ParserContext context, IToken token)
		{
			return new TypeDeclarationParser(context, token).ParseTypeDeclaration();
		}

		private Operator MatchOperator(_IExprement exp, params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, exp, ops);
		}

		private void Next(out IToken token)
		{
			Scanner.Next(out token);
		}

		private _IExpression ParseInitialisation()
		{
			return ExpressionParser.ParseInitialisation();
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _ITypeDeclarationStatement ParseTypeDeclaration()
		{
			Next(out var token);
			SignatureFlag signatureFlag = ReadAccessSpecifiers(token);
			Next(out token);
			if (token.Type != TokenType.Identifier)
			{
				AddErrorST(_tds, MessageId.Err_IdentifierExpected, Scanner.GetTokenText(token));
				return _tds;
			}
			string identifier = Scanner.GetIdentifier(token);
			_tds.Name = identifier;
			_IVariableExpression nameExpression = LMItemFactory.CreateVariableExpression(identifier, token);
			_tds.NameExpression = nameExpression;
			Operator opTest = MatchOperator(_tds, Operator.Colon, Operator.Extends);
			opTest = ReadBaseType(opTest);
			if (opTest != Operator.Colon)
			{
				return _tds;
			}
			Next(out token);
			bool bAlias = true;
			if (token.Type == TokenType.Operator)
			{
				if (ReadKindOfDeclarationReturnDone(token, signatureFlag, identifier, ref bAlias))
				{
					return _tds;
				}
			}
			else
			{
				Scanner.SetPosition(token);
			}
			if (bAlias)
			{
				_tds.Flags = SignatureFlag.Alias;
				_tds.Type = TypeParser.ParseType();
			}
			_tds.Flags |= signatureFlag;
			if (MatchOperator(_tds, Operator.Assign, Operator.Semicolon) == Operator.Assign)
			{
				_IExpression initial = ParseInitialisation();
				_tds.Initial = initial;
				MatchOperator(_tds, Operator.Semicolon);
			}
			return _tds;
		}

		private bool ReadKindOfDeclarationReturnDone(IToken token, SignatureFlag sfAccess, string stName, ref bool bAlias)
		{
			switch (Scanner.GetOperator(token))
			{
			case Operator.Struct:
				_tds.Flags = SignatureFlag.Structure | sfAccess;
				_tds.Declarations = DeclarationParser.ParseVariableList(token);
				MatchOperator(_tds, Operator.EndStruct);
				return true;
			case Operator.Union:
				_tds.Flags = SignatureFlag.Structure | SignatureFlag.Union | sfAccess;
				_tds.Declarations = DeclarationParser.ParseVariableList(token);
				MatchOperator(_tds, Operator.EndUnion);
				return true;
			case Operator.LeftParenthesis:
				_tds.Flags = SignatureFlag.Enum | sfAccess;
				_tds.Declarations = EnumListParser.ParseEnumList(Context, token, stName);
				bAlias = false;
				break;
			default:
				Scanner.SetPosition(token);
				break;
			}
			return false;
		}

		private Operator ReadBaseType(Operator opTest)
		{
			if (opTest == Operator.Extends)
			{
				_tds.Extends = ExpressionParser.ParseQualifiedNameExpression(_tds);
				opTest = MatchOperator(_tds, Operator.Colon);
			}
			return opTest;
		}

		private SignatureFlag ReadAccessSpecifiers(IToken token)
		{
			SignatureFlag sfAccess = SignatureFlag.None;
			sfAccess = ReadPublicPrivatProtected(token, sfAccess);
			return ReadFinalAbstract(sfAccess);
		}

		private SignatureFlag ReadFinalAbstract(SignatureFlag sfAccess)
		{
			Next(out var token);
			if (token.Type == TokenType.Operator)
			{
				switch (Scanner.GetOperator(token))
				{
				case Operator.Abstract:
					sfAccess |= SignatureFlag.Abstract;
					break;
				case Operator.Final:
					sfAccess |= SignatureFlag.Final;
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
			return sfAccess;
		}

		private SignatureFlag ReadPublicPrivatProtected(IToken token, SignatureFlag sfAccess)
		{
			if (token.Type == TokenType.Operator)
			{
				if (Scanner.GetOperator(token) == Operator.Internal)
				{
					sfAccess = SignatureFlag.Internal;
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
			return sfAccess;
		}
	}
}
