using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000044 RID: 68
	internal readonly struct MinMaxOperatorParser
	{
		// Token: 0x0600049F RID: 1183 RVA: 0x000140DE File Offset: 0x000122DE
		private MinMaxOperatorParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060004A0 RID: 1184 RVA: 0x000140E7 File Offset: 0x000122E7
		private ParserContext Context { get; }

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060004A1 RID: 1185 RVA: 0x000140EF File Offset: 0x000122EF
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060004A2 RID: 1186 RVA: 0x000140FC File Offset: 0x000122FC
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x060004A3 RID: 1187 RVA: 0x00014109 File Offset: 0x00012309
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060004A4 RID: 1188 RVA: 0x00014116 File Offset: 0x00012316
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x00014123 File Offset: 0x00012323
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00014131 File Offset: 0x00012331
		private Operator ParseReSyncST(out IToken tokenPos, params Operator[] ops)
		{
			this.Next(out tokenPos);
			this.Scanner.SetPosition(tokenPos);
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x00014154 File Offset: 0x00012354
		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			MinMaxOperatorParser minMaxOperatorParser = new MinMaxOperatorParser(context);
			return minMaxOperatorParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00014177 File Offset: 0x00012377
		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseMinMaxOperator(out bError, op, token, out endToken);
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00014184 File Offset: 0x00012384
		private _IExpression ParseMinMaxOperator(out bool bError, Operator op, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			IToken token2;
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 167)
			{
				_IOperatorExpression result = this.CreateOpExpressionWithError(op, token, token2);
				if (this.AttemptResync(out token2) == 168)
				{
					return result;
				}
				this.ResyncOutside(token2);
				bError = true;
				return result;
			}
			else
			{
				this.Next(out token2);
				if (token2.Type == 15 && this.Scanner.GetOperator(token2) == 168)
				{
					endToken = (token2 as _IToken);
					return null;
				}
				this.Scanner.SetPosition(token2);
				return this.ParseOperands(ref bError, op, token, ref endToken);
			}
		}

		// Token: 0x060004AA RID: 1194 RVA: 0x00014228 File Offset: 0x00012428
		private _IOperatorExpression ParseOperands(ref bool bError, Operator op, _IToken token, ref _IToken endToken)
		{
			_IOperatorExpression result = null;
			bool flag;
			_IExpression expOperand = this.ExpressionParser.ParseAssignment(out flag);
			IToken token2;
			IToken tokenHelp;
			for (;;)
			{
				Operator @operator;
				if (!this.Scanner.TryNextOperator(out @operator, out token2) || this.NotExpectedOperatorsFound(token2, ref result, op, token))
				{
					@operator = this.AttemptResync(out tokenHelp);
					if (@operator != 171 && @operator != 168)
					{
						break;
					}
				}
				if (@operator == 168)
				{
					goto Block_4;
				}
				this.ProcessOperands(ref result, token2, expOperand, op, token);
			}
			this.ResyncOutside(tokenHelp);
			bError = true;
			return result;
			Block_4:
			endToken = (token2 as _IToken);
			return result;
		}

		// Token: 0x060004AB RID: 1195 RVA: 0x000142B0 File Offset: 0x000124B0
		private bool NotExpectedOperatorsFound(IToken tokenHelp, ref _IOperatorExpression opexp, Operator op, IToken token)
		{
			Operator @operator = this.Scanner.GetOperator(tokenHelp);
			if (@operator != 171 && @operator != 168)
			{
				if (opexp == null)
				{
					opexp = this.LMItemFactory.CreateOperatorExpression(op, token);
				}
				this.ErrorHandler.AddErrorSTWithToken(opexp, tokenHelp, 2, new object[]
				{
					this.Scanner.GetOperatorText(171),
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(tokenHelp)
				});
				return true;
			}
			return false;
		}

		// Token: 0x060004AC RID: 1196 RVA: 0x0001433B File Offset: 0x0001253B
		private Operator AttemptResync(out IToken tokenHelp)
		{
			Operator[] array = new Operator[3];
			RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.0F804606BBAE4409CF04F7EAD25A0F6E0AA52F2C1779915C2E37AE5AD840520F).FieldHandle);
			return this.ParseReSyncST(out tokenHelp, array);
		}

		// Token: 0x060004AD RID: 1197 RVA: 0x00014358 File Offset: 0x00012558
		private void ProcessOperands(ref _IOperatorExpression opexp, IToken tokenHelp, _IExpression expOperand1, Operator op, IToken token)
		{
			bool flag;
			_IExpression exp = this.ExpressionParser.ParseAssignment(out flag) ?? this.LMItemFactory.CreateErrorExpression(tokenHelp);
			this.LMItemFactory.AddOperandHelp(ref opexp, expOperand1, exp, op, token);
		}

		// Token: 0x060004AE RID: 1198 RVA: 0x00014398 File Offset: 0x00012598
		private _IOperatorExpression CreateOpExpressionWithError(Operator op, IToken token, IToken tokenHelp)
		{
			_IOperatorExpression ioperatorExpression = this.LMItemFactory.CreateOperatorExpression(op, token);
			this.ErrorHandler.AddErrorSTWithToken(ioperatorExpression, tokenHelp, 6, new object[]
			{
				this.Scanner.GetOperatorText(167),
				this.Scanner.GetTokenText(tokenHelp)
			});
			return ioperatorExpression;
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x000143EA File Offset: 0x000125EA
		private void ResyncOutside(IToken tokenHelp)
		{
			this.Scanner.SetPosition(tokenHelp);
		}
	}
}
