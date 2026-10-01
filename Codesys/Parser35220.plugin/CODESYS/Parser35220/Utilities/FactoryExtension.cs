using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x0200001B RID: 27
	internal static class FactoryExtension
	{
		// Token: 0x060001F7 RID: 503 RVA: 0x0000BCE1 File Offset: 0x00009EE1
		internal static _IMultipleIndexInitialization CreateMultipleIndexInitialization(this _ILanguageModelBuilder6 factory, IExpression expNumber, IExpression expValue)
		{
			return factory.CreateMultipleIndexInitialisation(null, expNumber, expValue) as _IMultipleIndexInitialization;
		}

		// Token: 0x060001F8 RID: 504 RVA: 0x0000BCF1 File Offset: 0x00009EF1
		internal static _IMultipleIndexInitialization CreateMultipleIndexInitialization(this _ILanguageModelBuilder6 factory, IToken token)
		{
			return factory.CreateMultipleIndexInitialisation(token);
		}

		// Token: 0x060001F9 RID: 505 RVA: 0x0000BCFA File Offset: 0x00009EFA
		internal static _IToken Empty(this ITokenFactory factory)
		{
			return factory.CreateEmptyToken();
		}

		// Token: 0x060001FA RID: 506 RVA: 0x0000BD02 File Offset: 0x00009F02
		private static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, ulong ulVal, TypeClass tc)
		{
			return factory.CreateLiteralExpression(null, ulVal, tc) as _ILiteralExpression;
		}

		// Token: 0x060001FB RID: 507 RVA: 0x0000BD12 File Offset: 0x00009F12
		internal static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, long lVal)
		{
			return factory.CreateLiteralExpression(null, lVal) as _ILiteralExpression;
		}

		// Token: 0x060001FC RID: 508 RVA: 0x0000BD24 File Offset: 0x00009F24
		internal static _ILiteralExpression CreateLiteralExpression(this _ILanguageModelBuilder6 factory, ParserContext context, IToken token)
		{
			bool flag = false;
			_ILiteralExpression iliteralExpression = null;
			TypeClass typeClass = 29;
			IScanner9 scanner = context.Scanner;
			switch (token.Type)
			{
			case 1:
			{
				bool boolean = scanner.GetBoolean(token);
				long num = 0L;
				if (boolean)
				{
					num = 1L;
				}
				iliteralExpression = factory.CreateLiteralExpression(num, 0, token);
				break;
			}
			case 5:
			{
				DateTime dt;
				scanner.GetDate(token, ref dt, ref flag);
				uint num2 = Helper.DateTimeToMs1970(dt, ref flag);
				typeClass = 19;
				iliteralExpression = factory.CreateLiteralExpression((long)((ulong)num2), typeClass, token);
				break;
			}
			case 6:
			{
				DateTime dt2;
				scanner.GetDateAndTime(token, ref dt2, ref flag);
				uint num3 = Helper.DateTimeToMs1970(dt2, ref flag);
				typeClass = 20;
				iliteralExpression = factory.CreateLiteralExpression((long)((ulong)num3), typeClass, token);
				break;
			}
			case 9:
			{
				string doubleByteString = scanner.GetDoubleByteString(token);
				typeClass = 17;
				iliteralExpression = factory.CreateLiteralExpression(doubleByteString, typeClass, token);
				break;
			}
			case 10:
			{
				uint num4;
				scanner.GetDuration(token, ref num4, ref flag);
				typeClass = 18;
				iliteralExpression = factory.CreateLiteralExpression((long)((ulong)num4), typeClass, token);
				break;
			}
			case 11:
			{
				ulong num5;
				scanner.GetLDuration(token, ref num5, ref flag);
				typeClass = 37;
				iliteralExpression = factory.CreateLiteralExpression(num5, typeClass, token);
				break;
			}
			case 14:
			{
				ulong num6;
				bool flag2;
				Operator @operator;
				int num7;
				scanner.GetInteger(token, ref num6, ref flag2, ref @operator, ref flag, ref num7);
				long num8 = (long)num6;
				if (flag2)
				{
					num8 = -num8;
				}
				typeClass = ((@operator == null) ? 33 : context.TypeTable.GetTypeByOperator(@operator));
				iliteralExpression = factory.CreateLiteralExpression(num8, typeClass, token, num7, flag2);
				break;
			}
			case 16:
			{
				Operator @operator;
				double num9;
				scanner.GetReal(token, ref num9, ref @operator, ref flag);
				typeClass = ((@operator == null) ? 35 : context.TypeTable.GetTypeByOperator(@operator));
				iliteralExpression = factory.CreateLiteralExpression(num9, typeClass, token);
				break;
			}
			case 17:
				FactoryExtension.CreateSingleByteStringOrUCharLiteralExpression(factory, context, token, out iliteralExpression, out typeClass);
				break;
			case 18:
			{
				DateTime dateTime;
				scanner.GetTimeOfDay(token, ref dateTime, ref flag);
				int num10 = ((dateTime.Hour * 60 + dateTime.Minute) * 60 + dateTime.Second) * 1000 + dateTime.Millisecond;
				typeClass = 21;
				iliteralExpression = factory.CreateLiteralExpression((long)num10, typeClass, token);
				break;
			}
			case 22:
			{
				string xbyteString = scanner.GetXByteString(token);
				typeClass = 42;
				iliteralExpression = factory.CreateLiteralExpression(xbyteString, typeClass, token);
				break;
			}
			case 23:
			{
				long num11;
				scanner.GetLDate(token, ref num11, ref flag);
				typeClass = 46;
				iliteralExpression = factory.CreateLiteralExpression(num11, typeClass, token);
				break;
			}
			case 24:
			{
				long num12;
				scanner.GetLTimeOfDay(token, ref num12, ref flag);
				typeClass = 48;
				iliteralExpression = factory.CreateLiteralExpression(num12, typeClass, token);
				break;
			}
			case 25:
			{
				long num13;
				scanner.GetLDateAndTime(token, ref num13, ref flag);
				typeClass = 47;
				iliteralExpression = factory.CreateLiteralExpression(num13, typeClass, token);
				break;
			}
			}
			if (flag)
			{
				context.ErrorHandler.AddErrorSTWithToken(iliteralExpression, token, 1, new object[]
				{
					scanner.GetTokenText(token),
					scanner.GetOperatorText(context.TypeTable.GetOperatorByType(typeClass))
				});
			}
			return iliteralExpression;
		}

		// Token: 0x060001FD RID: 509 RVA: 0x0000C008 File Offset: 0x0000A208
		private static void CreateSingleByteStringOrUCharLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc)
		{
			StringEncoding stringEncoding;
			bool flag;
			string singleByteString = context.Scanner.GetSingleByteString(token, ref stringEncoding, ref flag);
			if (flag)
			{
				FactoryExtension.CreateUCharLiteralExpression(factory, context, token, out literalExp, out tc, singleByteString);
				return;
			}
			FactoryExtension.CreateSingleByteStringLiteralExpression(factory, context, token, out literalExp, out tc, singleByteString, stringEncoding);
		}

		// Token: 0x060001FE RID: 510 RVA: 0x0000C044 File Offset: 0x0000A244
		private static void CreateSingleByteStringLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc, string str, StringEncoding stringEncoding)
		{
			tc = 16;
			literalExp = factory.CreateLiteralExpression(str, tc, token, stringEncoding);
			if (stringEncoding == null && !context.CompileOptions.UTF8Encoding && !Helper.CheckEncoding(str))
			{
				context.ErrorHandler.AddWarningST(literalExp, 555, new object[]
				{
					(str.Length > 7) ? str.Substring(0, 7) : str
				});
			}
			if (stringEncoding == 1 && context._bReportSP18Feature)
			{
				context.AddUnsupportedFeatureError(literalExp, Strings.CompilerFeature_UTF8_Strings, ParserContext.CompilerVersion18);
			}
		}

		// Token: 0x060001FF RID: 511 RVA: 0x0000C0D4 File Offset: 0x0000A2D4
		private static void CreateUCharLiteralExpression(_ILanguageModelBuilder6 factory, ParserContext context, IToken token, out _ILiteralExpression literalExp, out TypeClass tc, string str)
		{
			ulong num = (ulong)char.ConvertToUtf32(str, 0);
			tc = 12;
			literalExp = factory.CreateLiteralExpression(num, tc, token);
			if (context._bReportSP18Feature)
			{
				context.AddUnsupportedFeatureError(literalExp, Strings.CompilerFeature_UChar_Literals, ParserContext.CompilerVersion18);
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x0000C118 File Offset: 0x0000A318
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
				_IOperatorExpression ioperatorExpression = factory.CreateOperatorExpression(op, token);
				ioperatorExpression.AddOperand(extop);
				if (Helper.IsPrefixOperator(op))
				{
					ioperatorExpression._Position = extop._Position;
					extop._Position = null;
				}
				extop = ioperatorExpression;
			}
			extop.AddOperand(exp2);
		}
	}
}
