using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;

namespace CODESYS.Parser35210.Utilities
{
	internal static class FactoryExtension
	{
		internal static _IMultipleIndexInitialization CreateMultipleIndexInitialization(this _ILanguageModelBuilder6 factory, IExpression expNumber, IExpression expValue)
		{
			return factory.CreateMultipleIndexInitialisation(null, expNumber, expValue) as _IMultipleIndexInitialization;
		}

		internal static _IMultipleIndexInitialization CreateMultipleIndexInitialization(this _ILanguageModelBuilder6 factory, IToken token)
		{
			return factory.CreateMultipleIndexInitialisation(token);
		}

		internal static _IToken Empty(this ITokenFactory factory)
		{
			return factory.CreateEmptyToken();
		}

		private static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, ulong ulVal, TypeClass tc)
		{
			return factory.CreateLiteralExpression(null, ulVal, tc) as _ILiteralExpression;
		}

		internal static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, long lVal)
		{
			return factory.CreateLiteralExpression(null, lVal) as _ILiteralExpression;
		}

		internal static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, ParserContext context, IToken token)
		{
			bool bOverflow = false;
			_ILiteralExpression literalExp = null;
			TypeClass tc = TypeClass.None;
			IScanner9 scanner = context.Scanner;
			Operator type;
			switch (token.Type)
			{
			case TokenType.Boolean:
			{
				bool boolean = scanner.GetBoolean(token);
				long lVal = 0L;
				if (boolean)
				{
					lVal = 1L;
				}
				literalExp = factory.CreateLiteralExpression(lVal, TypeClass.Bool, token);
				break;
			}
			case TokenType.Integer:
			{
				scanner.GetInteger(token, out var nValue, out var bSign, out type, out bOverflow, out var nBase);
				long num = (long)nValue;
				if (bSign)
				{
					num = -num;
				}
				tc = ((type == Operator.None) ? TypeClass.AnyInt : context.TypeTable.GetTypeByOperator(type));
				literalExp = factory.CreateLiteralExpression(num, tc, token, nBase, bSign);
				break;
			}
			case TokenType.Duration:
			{
				scanner.GetDuration(token, out var nDuration, out bOverflow);
				tc = TypeClass.Time;
				literalExp = factory.CreateLiteralExpression(nDuration, tc, token);
				break;
			}
			case TokenType.LDuration:
			{
				scanner.GetLDuration(token, out var ulDuration, out bOverflow);
				tc = TypeClass.LTime;
				literalExp = factory.CreateLiteralExpression(ulDuration, tc, token);
				break;
			}
			case TokenType.Date:
			{
				scanner.GetDate(token, out var value6, out bOverflow);
				uint num4 = Helper.DateTimeToMs1970(value6, ref bOverflow);
				tc = TypeClass.Date;
				literalExp = factory.CreateLiteralExpression(num4, tc, token);
				break;
			}
			case TokenType.LDate:
			{
				scanner.GetLDate(token, out var value5, out bOverflow);
				tc = TypeClass.LDate;
				literalExp = factory.CreateLiteralExpression(value5, tc, token);
				break;
			}
			case TokenType.DateAndTime:
			{
				scanner.GetDateAndTime(token, out var value4, out bOverflow);
				uint num3 = Helper.DateTimeToMs1970(value4, ref bOverflow);
				tc = TypeClass.DateAndTime;
				literalExp = factory.CreateLiteralExpression(num3, tc, token);
				break;
			}
			case TokenType.LDateAndTime:
			{
				scanner.GetLDateAndTime(token, out var value3, out bOverflow);
				tc = TypeClass.LDateAndTime;
				literalExp = factory.CreateLiteralExpression(value3, tc, token);
				break;
			}
			case TokenType.TimeOfDay:
			{
				scanner.GetTimeOfDay(token, out var value2, out bOverflow);
				int num2 = ((value2.Hour * 60 + value2.Minute) * 60 + value2.Second) * 1000 + value2.Millisecond;
				tc = TypeClass.TimeOfDay;
				literalExp = factory.CreateLiteralExpression(num2, tc, token);
				break;
			}
			case TokenType.LTimeOfDay:
			{
				scanner.GetLTimeOfDay(token, out var value, out bOverflow);
				tc = TypeClass.LTimeOfDay;
				literalExp = factory.CreateLiteralExpression(value, tc, token);
				break;
			}
			case TokenType.SingleByteString:
				CreateSingleByteStringOrUCharLiteralExpression(factory, context, token, out literalExp, out tc);
				break;
			case TokenType.DoubleByteString:
			{
				string doubleByteString = scanner.GetDoubleByteString(token);
				tc = TypeClass.WString;
				literalExp = factory.CreateLiteralExpression(doubleByteString, tc, token);
				break;
			}
			case TokenType.XByteString:
			{
				string xByteString = scanner.GetXByteString(token);
				tc = TypeClass.XString;
				literalExp = factory.CreateLiteralExpression(xByteString, tc, token);
				break;
			}
			case TokenType.Real:
			{
				scanner.GetReal(token, out var dValue, out type, out bOverflow);
				tc = ((type == Operator.None) ? TypeClass.AnyReal : context.TypeTable.GetTypeByOperator(type));
				literalExp = factory.CreateLiteralExpression(dValue, tc, token);
				break;
			}
			}
			if (bOverflow)
			{
				context.ErrorHandler.AddErrorSTWithToken(literalExp, token, MessageId.Err_ConstantOverflow, scanner.GetTokenText(token), scanner.GetOperatorText(context.TypeTable.GetOperatorByType(tc)));
			}
			return literalExp;
		}

		private static void CreateSingleByteStringOrUCharLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc)
		{
			StringEncoding stringEncoding;
			bool bIsUChar;
			string singleByteString = context.Scanner.GetSingleByteString(token, out stringEncoding, out bIsUChar);
			if (bIsUChar)
			{
				CreateUCharLiteralExpression(factory, context, token, out literalExp, out tc, singleByteString);
			}
			else
			{
				CreateSingleByteStringLiteralExpression(factory, context, token, out literalExp, out tc, singleByteString, stringEncoding);
			}
		}

		private static void CreateSingleByteStringLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc, string str, StringEncoding stringEncoding)
		{
			tc = TypeClass.String;
			literalExp = factory.CreateLiteralExpression(str, tc, token, stringEncoding);
			if (stringEncoding == StringEncoding.Default && !context.CompileOptions.UTF8Encoding && !Helper.CheckEncoding(str))
			{
				context.ErrorHandler.AddWarningST(literalExp, MessageId.Wrn_NonAsciiStringLiteral, (str.Length > 7) ? str.Substring(0, 7) : str);
			}
			if (stringEncoding == StringEncoding.UTF8 && context._bReportSP18Feature)
			{
				context.AddUnsupportedFeatureError(literalExp, Strings.CompilerFeature_UTF8_Strings, ParserContext.CompilerVersion18);
			}
		}

		private static void CreateUCharLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc, string str)
		{
			ulong ulVal = (uint)char.ConvertToUtf32(str, 0);
			tc = TypeClass.UDInt;
			literalExp = factory.CreateLiteralExpression(ulVal, tc, token);
			if (context._bReportSP18Feature)
			{
				context.AddUnsupportedFeatureError(literalExp, Strings.CompilerFeature_UChar_Literals, ParserContext.CompilerVersion18);
			}
		}

		internal static void AddOperandHelp(this _ILanguageModelBuilder6 factory, ref _IOperatorExpression extop, _IExpression exp1, _IExpression exp2, Operator op, IToken token)
		{
			if (exp2 == null)
			{
				return;
			}
			if (extop == null)
			{
				extop = factory.CreateOperatorExpression(op, token);
				extop.AddOperand(exp1);
				if (!Helper.IsPrefixOperator(op))
				{
					extop._Position = exp1._Position;
				}
			}
			else
			{
				_IOperatorExpression iOperatorExpression = factory.CreateOperatorExpression(op, token);
				iOperatorExpression.AddOperand(extop);
				if (Helper.IsPrefixOperator(op))
				{
					iOperatorExpression._Position = extop._Position;
					extop._Position = null;
				}
				extop = iOperatorExpression;
			}
			extop.AddOperand(exp2);
		}
	}
}
