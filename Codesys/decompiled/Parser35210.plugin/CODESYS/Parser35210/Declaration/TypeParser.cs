using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal class TypeParser
	{
		private ParserContext Context { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private ITypeTable3 TypeTable => Context.TypeTable;

		private ITokenFactory TokenFactory => Context.TokenFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		internal TypeParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private void ParseReSyncIF()
		{
			Scanner.ParseReSyncIF();
		}

		private Operator MatchOperator(params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError: true, ops);
		}

		private Operator MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError, ops);
		}

		private void AddErrorIF(IToken tokenPos, MessageId ErrorId, params object[] args)
		{
			ErrorHandler.AddError(tokenPos, ErrorId, args);
		}

		private _IType ParseSubrangeType(_IType iectypebase)
		{
			Next(out var token, bWithPragma: true, bWithComment: true);
			if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.LeftParenthesis)
			{
				bool bError;
				_IExpression expLower = ExpressionParser.ParseAssignment(out bError);
				MatchOperator(Operator.Range);
				_IExpression expUpper = ExpressionParser.ParseAssignment(out bError);
				MatchOperator(Operator.RightParenthesis);
				_ISubrangeType iSubrangeType = LMItemFactory.CreateSubrangeType(expLower, expUpper);
				iSubrangeType._Base = iectypebase;
				return iSubrangeType;
			}
			Scanner.SetPosition(token);
			return iectypebase;
		}

		public _IType ParseType()
		{
			return ParseType(bTop: true, bTry: false, TokenFactory.Empty());
		}

		public _IType TryParseType()
		{
			return ParseType(bTop: true, bTry: true, TokenFactory.Empty());
		}

		private _IType ParseType(bool bTop, bool bTry)
		{
			return ParseType(bTop, bTry, TokenFactory.Empty());
		}

		public _IType ParseType(bool bTop, bool bTry, IToken tokenError)
		{
			IToken token;
			switch (Next(out token))
			{
			case TokenType.Operator:
			{
				Operator @operator = Scanner.GetOperator(token);
				_IType iType2 = HandleOperatorCase(@operator, bTry, bTop, token, tokenError);
				if (iType2 != null)
				{
					return iType2;
				}
				return null;
			}
			case TokenType.Identifier:
			{
				Scanner.SetPosition(token);
				_IType iType = ParseUserdefType();
				return iType ?? HandleTryCase(bTry, token);
			}
			default:
				return HandleTryCase(bTry, token);
			}
		}

		private _IType HandleOperatorCase(Operator opGlobal, bool bTry, bool bTop, IToken token, IToken tokenError)
		{
			HashSet<Operator> obj = new HashSet<Operator>
			{
				Operator.__UXInt,
				Operator.__XWord,
				Operator.__XInt,
				Operator.__Lazy,
				Operator.Bool,
				Operator.Byte,
				Operator.SInt,
				Operator.USInt,
				Operator.Int,
				Operator.UInt,
				Operator.Word,
				Operator.DInt,
				Operator.UDInt,
				Operator.DWord,
				Operator.LInt,
				Operator.ULInt,
				Operator.LWord,
				Operator.Real,
				Operator.LReal,
				Operator.Time,
				Operator.LTime,
				Operator.Date,
				Operator.LDate,
				Operator.DateAndTime,
				Operator.LDateAndTime,
				Operator.TimeOfDay,
				Operator.LTimeOfDay,
				Operator.Bit,
				Operator.SafeBool,
				Operator.SafeByte,
				Operator.SafeSInt,
				Operator.SafeUSInt,
				Operator.SafeWord,
				Operator.SafeInt,
				Operator.SafeUInt,
				Operator.SafeDWord,
				Operator.SafeDInt,
				Operator.SafeUDInt,
				Operator.SafeLWord,
				Operator.SafeLInt,
				Operator.SafeULInt,
				Operator.SafeTime,
				Operator.SafeReal,
				Operator.SafeLReal
			};
			HashSet<Operator> hashSet = new HashSet<Operator>
			{
				Operator.Any,
				Operator.AnyBit,
				Operator.AnyDate,
				Operator.AnyInt,
				Operator.AnyNum,
				Operator.AnyReal,
				Operator.AnyString
			};
			if (obj.Contains(opGlobal) || hashSet.Contains(opGlobal))
			{
				_IType iType = TypeTable.Get(opGlobal);
				if (!TypeTable.IsInteger(iType.Class))
				{
					return iType;
				}
				if (hashSet.Contains(opGlobal) && !bTop)
				{
					AddErrorIF(token, MessageId.Err_AnyTypeOnlyInFunction, iType.ToString());
				}
				return ParseSubrangeType(iType);
			}
			switch (opGlobal)
			{
			case Operator.Pointer:
				return ParsePointerType(token, tokenError);
			case Operator.Reference:
				return ParseReferenceType(bTop, token, tokenError);
			case Operator.__XString:
				return ParseXStringType();
			case Operator.WString:
				return ParseWStringType();
			case Operator.String:
				return ParseStringType();
			case Operator.__Vector:
				return ParseVectorType();
			case Operator.Array:
				return ParseArrayType(bTop, token, tokenError);
			case Operator.Params:
			{
				if (!bTop)
				{
					AddErrorIF(token, MessageId.Err_UnexpectedTokenFound, Scanner.GetOperatorText(Operator.Params));
				}
				AddErrorIF(token, MessageId.Err_UnexpectedTokenFound, Scanner.GetOperatorText(Operator.Params));
				MatchOperator(Operator.LeftParenthesis);
				bool bError;
				_IExpression count = ExpressionParser.ParseAssignment(out bError);
				MatchOperator(Operator.RightParenthesis);
				MatchOperator(Operator.Of);
				_IType typeBase = ParseType(bTop: false, bTry: false);
				return LMItemFactory.CreateParamsType(typeBase, count);
			}
			case Operator.LeftParenthesis:
			{
				_IEnumDeclarationListStatement enumdecls = ParseEnumList(token, null);
				return LMItemFactory.CreateImplicitEnumerationType(enumdecls, "__IMPLICIT__ENUM");
			}
			case Operator.__SystemScope:
			case Operator.__PoolScope:
			{
				if (Next(out var token2) == TokenType.Operator && Scanner.GetOperator(token2) == Operator.Period)
				{
					_IExpression expBase = ExpressionParser.ParseQualifiedNameExpression(null);
					_IExpression iExpression2;
					if (opGlobal != Operator.__PoolScope)
					{
						_IExpression iExpression = LMItemFactory.CreateSystemScopeExpression(expBase, token);
						iExpression2 = iExpression;
					}
					else
					{
						_IExpression iExpression = LMItemFactory.CreatePoolScopeExpression(expBase, token);
						iExpression2 = iExpression;
					}
					_IExpression expname = iExpression2;
					return LMItemFactory.CreateUserdefType(expname);
				}
				break;
			}
			}
			return HandleTryCase(bTry, token);
		}

		private _IType HandleTryCase(bool bTry, IToken token)
		{
			if (!bTry)
			{
				AddErrorIF(token, MessageId.Err_TypeExpected, Scanner.GetTokenText(token));
				ParseReSyncIF();
			}
			return null;
		}

		public _IType ParseUserdefType()
		{
			_IExpression iExpression = ExpressionParser.ParseQualifiedNameExpression(null);
			if (iExpression == null)
			{
				return null;
			}
			if (!Scanner.CheckOptionalOperator(Operator.Less))
			{
				return LMItemFactory.CreateUserdefType(iExpression);
			}
			return ParseGenericUserdefType(iExpression);
		}

		private _IType ParseGenericUserdefType(_IExpression qne)
		{
			IGenericUserdefType genericUserdefType = LMItemFactory.CreateGenericUserdefType(qne);
			Operator @operator;
			do
			{
				_IExpression iExpression;
				if (Scanner.CheckOptionalOperator(Operator.LeftParenthesis))
				{
					iExpression = ExpressionParser.ParseExpression() as _IExpression;
					MatchOperator(Operator.RightParenthesis);
				}
				else
				{
					iExpression = ExpressionParser.ParseOperand() as _IExpression;
				}
				if (iExpression != null)
				{
					genericUserdefType.AddGenericConstantInitialization(iExpression);
				}
				@operator = MatchOperator(Operator.Comma, Operator.Greater);
			}
			while (@operator == Operator.Comma);
			if (Context._bReportSP18Feature)
			{
				_IExpression iExpression2 = genericUserdefType.GenericConstantsInitializations.FirstOrDefault();
				if (iExpression2 != null)
				{
					Context.AddUnsupportedFeatureError(iExpression2, Strings.CompilerFeature_GenericConstantVariable, ParserContext.CompilerVersion18);
				}
			}
			return genericUserdefType;
		}

		private _IType ParseVectorType()
		{
			MatchOperator(Operator.LeftBracket);
			bool bError;
			_IExpression iExpression = ExpressionParser.ParseAssignment(out bError);
			if (bError)
			{
				AddErrorIF(Scanner.CurrentToken, MessageId.Err_VectorSizeNotValid);
				iExpression = null;
			}
			MatchOperator(Operator.RightBracket);
			MatchOperator(Operator.Of);
			_IType iType = ParseType(bTop: false, bTry: false);
			if (iType == null || iExpression == null)
			{
				return null;
			}
			return LMItemFactory.CreateVectorType(iType, iExpression);
		}

		private _IType ParseXStringType()
		{
			Next(out var token);
			_IXStringType iXStringType = LMItemFactory.CreateXStringtype();
			Scanner.SetPosition(token);
			if (MatchOperator(false, Operator.LeftParenthesis) == Operator.None)
			{
				Scanner.SetPosition(token);
				return iXStringType;
			}
			bool bError;
			_IExpression length = ExpressionParser.ParseAssignment(out bError);
			if (bError)
			{
				AddErrorIF(token, MessageId.Err_StringSizeExpected);
				return iXStringType;
			}
			iXStringType.Length = length;
			MatchOperator(Operator.RightParenthesis);
			return iXStringType;
		}

		private _IType ParseWStringType()
		{
			Next(out var token);
			_IWStringType iWStringType = LMItemFactory.CreateWStringType();
			Scanner.SetPosition(token);
			if (MatchOperator(false, Operator.LeftParenthesis) == Operator.None)
			{
				Scanner.SetPosition(token);
				return iWStringType;
			}
			bool bError;
			_IExpression length = ExpressionParser.ParseAssignment(out bError);
			if (bError)
			{
				AddErrorIF(token, MessageId.Err_StringSizeExpected);
				return iWStringType;
			}
			iWStringType.Length = length;
			MatchOperator(Operator.RightParenthesis);
			return iWStringType;
		}

		private _IType ParseStringType()
		{
			Next(out var token, bWithPragma: false, bWithComment: true);
			_IStringType iStringType = LMItemFactory.CreateStringType();
			Scanner.SetPosition(token);
			if (MatchOperator(false, Operator.LeftParenthesis, Operator.LeftBracket) == Operator.None)
			{
				Scanner.SetPosition(token);
				return iStringType;
			}
			bool bError;
			_IExpression length = ExpressionParser.ParseAssignment(out bError);
			if (bError)
			{
				AddErrorIF(token, MessageId.Err_StringSizeExpected);
				return iStringType;
			}
			iStringType.Length = length;
			MatchOperator(Operator.RightParenthesis, Operator.RightBracket);
			return iStringType;
		}

		private _IType ParseArrayType(bool bTop, IToken token, IToken tokenError)
		{
			MatchOperator(Operator.LeftBracket);
			if (Scanner.GetNext(out var token2) == TokenType.Operator && Scanner.GetOperator(token2) == Operator.Times)
			{
				if (!bTop)
				{
					AddErrorIF(token2, MessageId.Err_VarLengthArrayTopLevel);
				}
				int num = 1;
				_IVariableLengthArrayType iVariableLengthArrayType = LMItemFactory.CreateVariableLengthArrayType();
				while (MatchOperator(Operator.Comma, Operator.RightBracket) == Operator.Comma)
				{
					num++;
					MatchOperator(Operator.Times);
				}
				iVariableLengthArrayType.Dimensions = num;
				MatchOperator(Operator.Of);
				iVariableLengthArrayType._Base = ParseType(bTop: false, bTry: false);
				if (iVariableLengthArrayType._Base == null)
				{
					return null;
				}
				return iVariableLengthArrayType;
			}
			Scanner.SetPosition(token2);
			_IArrayType iArrayType = LMItemFactory.CreateArrayType();
			do
			{
				bool bError;
				_IExpression expLower = ExpressionParser.ParseAssignment(out bError);
				MatchOperator(Operator.Range);
				_IExpression expUpper = ExpressionParser.ParseAssignment(out bError);
				iArrayType.AddDimension(expLower, expUpper);
			}
			while (MatchOperator(Operator.RightBracket, Operator.Comma) == Operator.Comma);
			MatchOperator(Operator.Of);
			_IType iType2 = (iArrayType._Base = ParseType(bTop: false, bTry: false));
			if (iArrayType._Base == null)
			{
				return null;
			}
			if (iType2.Class == TypeClass.Bit)
			{
				AddErrorIF((tokenError.Length == 0) ? token : tokenError, MessageId.Err_NoArrayOfBit);
			}
			return iArrayType;
		}

		private _IType ParseReferenceType(bool bTop, IToken token, IToken tokenError)
		{
			MatchOperator(Operator.To);
			_IReferenceType iReferenceType = LMItemFactory.CreateReferenceType();
			_IType iType = ParseType(bTop: false, bTry: false);
			if (iType == null)
			{
				return null;
			}
			if (iType.Class == TypeClass.Bit)
			{
				AddErrorIF((tokenError.Length == 0) ? token : tokenError, MessageId.Err_NoReferenceToBits);
			}
			iReferenceType._Base = iType;
			if (!bTop)
			{
				AddErrorIF(token, MessageId.Err_ReferenceNotAllowed);
			}
			return iReferenceType;
		}

		private _IType ParsePointerType(IToken token, IToken tokenError)
		{
			MatchOperator(Operator.To);
			_IPointerType iPointerType = LMItemFactory.CreatePointerType();
			_IType iType = ParseType(bTop: false, bTry: false);
			if (iType == null)
			{
				return null;
			}
			if (iType.Class == TypeClass.Bit)
			{
				AddErrorIF((tokenError.Length == 0) ? token : tokenError, MessageId.Err_NoPointerToBit);
			}
			iPointerType._Base = iType;
			return iPointerType;
		}

		private _IEnumDeclarationListStatement ParseEnumList(IToken tokenParenthesis, string stEnumName)
		{
			return EnumListParser.ParseEnumList(Context, tokenParenthesis, stEnumName);
		}
	}
}
