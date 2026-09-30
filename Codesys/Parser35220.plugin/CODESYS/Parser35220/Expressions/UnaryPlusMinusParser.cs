using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200004D RID: 77
	internal readonly struct UnaryPlusMinusParser
	{
		// Token: 0x0600051C RID: 1308 RVA: 0x00016105 File Offset: 0x00014305
		private UnaryPlusMinusParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x0600051D RID: 1309 RVA: 0x0001610E File Offset: 0x0001430E
		private ParserContext Context { get; }

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x0600051E RID: 1310 RVA: 0x00016116 File Offset: 0x00014316
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x0600051F RID: 1311 RVA: 0x00016123 File Offset: 0x00014323
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000520 RID: 1312 RVA: 0x00016130 File Offset: 0x00014330
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000521 RID: 1313 RVA: 0x0001613E File Offset: 0x0001433E
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000522 RID: 1314 RVA: 0x0001614B File Offset: 0x0001434B
		private _ILiteralExpression CreateLiteralExpression(IToken token)
		{
			return this.LMItemFactory.CreateLiteralExpression(this.Context, token);
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x00016160 File Offset: 0x00014360
		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			UnaryPlusMinusParser unaryPlusMinusParser = new UnaryPlusMinusParser(context);
			return unaryPlusMinusParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x00016183 File Offset: 0x00014383
		private _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseUnaryPlusMinusOperator(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x00016194 File Offset: 0x00014394
		private _IExpression ParseUnaryPlusMinusOperator(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			IToken token2;
			_IExpression iexpression;
			if (this.Next(out token2) == 14 || token2.Type == 16)
			{
				_ILiteralExpression iliteralExpression = this.CreateLiteralExpression(token2);
				if (op == 158)
				{
					iliteralExpression.Negative = true;
					if (iliteralExpression.LongValue != 0L)
					{
						iliteralExpression.LongValue = 0L - iliteralExpression.LongValue;
					}
					else if (iliteralExpression.ULongValue != 0UL)
					{
						iliteralExpression.LongValue = (long)(0UL - iliteralExpression.ULongValue);
						iliteralExpression.ConstantType = 33;
					}
					else
					{
						iliteralExpression.RealValue = 0.0 - iliteralExpression.RealValue;
					}
				}
				iexpression = iliteralExpression;
				endToken = (token2 as _IToken);
				return iexpression;
			}
			this.Scanner.SetPosition(token2);
			iexpression = (this.ExpressionParser.ParseSTOperandWithPosition(out bError, null, out endToken) ?? this.LMItemFactory.CreateErrorExpression(token2));
			if (op == 158)
			{
				short positionLength = Helper.CalculateLength(startToken, endToken);
				_IOperatorExpression ioperatorExpression = this.LMItemFactory.CreateOperatorExpression(158, token);
				_ILiteralExpression iliteralExpression2 = this.LMItemFactory.CreateLiteralExpression(0L, 7, token);
				ioperatorExpression.AddOperand(iliteralExpression2);
				ioperatorExpression.AddOperand(iexpression);
				iexpression = ioperatorExpression;
				iexpression.PositionLength = positionLength;
				return iexpression;
			}
			return iexpression;
		}
	}
}
