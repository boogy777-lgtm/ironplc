using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200003D RID: 61
	internal readonly struct ConversionExpressionParser
	{
		// Token: 0x06000440 RID: 1088 RVA: 0x00012D34 File Offset: 0x00010F34
		private ConversionExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000441 RID: 1089 RVA: 0x00012D3D File Offset: 0x00010F3D
		private ParserContext Context { get; }

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000442 RID: 1090 RVA: 0x00012D45 File Offset: 0x00010F45
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000443 RID: 1091 RVA: 0x00012D52 File Offset: 0x00010F52
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x00012D5F File Offset: 0x00010F5F
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000445 RID: 1093 RVA: 0x00012D6D File Offset: 0x00010F6D
		private ITypeTable3 TypeTable
		{
			get
			{
				return this.Context.TypeTable;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000446 RID: 1094 RVA: 0x00012D7A File Offset: 0x00010F7A
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x06000447 RID: 1095 RVA: 0x00012D87 File Offset: 0x00010F87
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000448 RID: 1096 RVA: 0x00012D94 File Offset: 0x00010F94
		private Operator ParseReSyncST(params Operator[] ops)
		{
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x06000449 RID: 1097 RVA: 0x00012DA2 File Offset: 0x00010FA2
		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			this.Next(out tokenPos);
			this.Scanner.SetPosition(tokenPos);
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x0600044A RID: 1098 RVA: 0x00012DC8 File Offset: 0x00010FC8
		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			ConversionExpressionParser conversionExpressionParser = new ConversionExpressionParser(context);
			return conversionExpressionParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x0600044B RID: 1099 RVA: 0x00012DEB File Offset: 0x00010FEB
		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseConversionExpression(out bError, token, out endToken);
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x00012DF8 File Offset: 0x00010FF8
		private _IExpression ParseConversionExpression(out bool bError, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			Operator @operator;
			Operator operator2;
			this.Scanner.GetConversion(token, ref @operator, ref operator2);
			_IConversionExpression iconversionExpression = this.LMItemFactory.CreateConversionExpression(this.TypeTable.GetTypeByOperator(@operator), this.TypeTable.GetTypeByOperator(operator2), token);
			IToken token2;
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 167)
			{
				this.ErrorHandler.AddErrorSTWithToken(iconversionExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(167),
					this.Scanner.GetTokenText(token2)
				});
				Operator operator3 = this.ParseReSyncST(new Operator[]
				{
					167,
					168
				});
				if (operator3 == 168)
				{
					return iconversionExpression;
				}
				if (operator3 != 167)
				{
					this.Scanner.SetPosition(token2);
					bError = true;
					return iconversionExpression;
				}
			}
			bool flag;
			_IExpression iexpression = this.ExpressionParser.ParseAssignment(out flag);
			iconversionExpression._Exp = iexpression;
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 168)
			{
				this.ErrorHandler.AddErrorSTWithToken(iconversionExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(token2)
				});
				flag = true;
			}
			else
			{
				endToken = (token2 as _IToken);
			}
			IToken position;
			if ((iexpression == null || flag) && this.ParseReSyncST(out position, new Operator[]
			{
				168
			}) != 168)
			{
				this.Scanner.SetPosition(position);
				bError = true;
			}
			return iconversionExpression;
		}
	}
}
