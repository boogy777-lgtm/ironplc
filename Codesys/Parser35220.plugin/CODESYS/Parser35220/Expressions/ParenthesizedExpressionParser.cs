using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000047 RID: 71
	internal readonly struct ParenthesizedExpressionParser
	{
		// Token: 0x060004D3 RID: 1235 RVA: 0x000152FA File Offset: 0x000134FA
		internal ParenthesizedExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x060004D4 RID: 1236 RVA: 0x00015303 File Offset: 0x00013503
		private ParserContext Context { get; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x060004D5 RID: 1237 RVA: 0x0001530B File Offset: 0x0001350B
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x060004D6 RID: 1238 RVA: 0x00015318 File Offset: 0x00013518
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060004D7 RID: 1239 RVA: 0x00015325 File Offset: 0x00013525
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x060004D8 RID: 1240 RVA: 0x00015333 File Offset: 0x00013533
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x060004D9 RID: 1241 RVA: 0x00015340 File Offset: 0x00013540
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060004DA RID: 1242 RVA: 0x0001534D File Offset: 0x0001354D
		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			this.Next(out tokenPos);
			this.Scanner.SetPosition(tokenPos);
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x00015370 File Offset: 0x00013570
		internal static _IExpression ParseParenthesizedExpressionStatic(ParserContext context, out bool bError, _IToken token, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			return context.ParenthesizedExpressionParser.ParseParenthesizedExpression(out bError, token, out endToken, out lenghtOfExpWithoutParanthesis);
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00015390 File Offset: 0x00013590
		private _IExpression ParseParenthesizedExpression(out bool bError, _IToken token, out _IToken endToken, out short lenghtOfExpWithoutParanthesis)
		{
			bError = false;
			endToken = token;
			lenghtOfExpWithoutParanthesis = 0;
			_IExpression iexpression = this.ExpressionParser.ParseAssignment(out bError);
			if (iexpression == null)
			{
				iexpression = this.LMItemFactory.CreateErrorExpression(token);
			}
			else
			{
				IOperatorExpression operatorExpression = iexpression as IOperatorExpression;
				if (operatorExpression != null && !Helper.IsPrefixOperator(operatorExpression.Code))
				{
					iexpression.SetPosition(token);
				}
			}
			IToken token2;
			if (bError)
			{
				if (this.ParseReSyncST(out token2, new Operator[]
				{
					168
				}) != 168)
				{
					bError = true;
					this.Scanner.SetPosition(token2);
				}
			}
			else if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 168)
			{
				this.ErrorHandler.AddErrorSTWithToken(iexpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(token2)
				});
				bError = true;
				this.Scanner.SetPosition(token2);
			}
			endToken = (token2 as _IToken);
			lenghtOfExpWithoutParanthesis = iexpression.LengthIntern;
			return iexpression;
		}
	}
}
