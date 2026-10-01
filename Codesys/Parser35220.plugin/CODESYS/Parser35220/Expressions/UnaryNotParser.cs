using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200004C RID: 76
	internal readonly struct UnaryNotParser
	{
		// Token: 0x06000515 RID: 1301 RVA: 0x0001604A File Offset: 0x0001424A
		private UnaryNotParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x06000516 RID: 1302 RVA: 0x00016053 File Offset: 0x00014253
		private ParserContext Context { get; }

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x06000517 RID: 1303 RVA: 0x0001605B File Offset: 0x0001425B
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000134 RID: 308
		// (get) Token: 0x06000518 RID: 1304 RVA: 0x00016068 File Offset: 0x00014268
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000519 RID: 1305 RVA: 0x00016078 File Offset: 0x00014278
		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			UnaryNotParser unaryNotParser = new UnaryNotParser(context);
			return unaryNotParser.Parse(out bError, token, startToken, out endToken);
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x0001609A File Offset: 0x0001429A
		private _IExpression Parse(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseUnaryNotOperator(out bError, token, startToken, out endToken);
		}

		// Token: 0x0600051B RID: 1307 RVA: 0x000160A8 File Offset: 0x000142A8
		private _IExpression ParseUnaryNotOperator(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_IExpression iexpression = this.ExpressionParser.ParseSTOperandWithPosition(out bError, null, out endToken) ?? this.LMItemFactory.CreateErrorExpression(token);
			short positionLength = Helper.CalculateLength(startToken, endToken);
			_IOperatorExpression ioperatorExpression = this.LMItemFactory.CreateOperatorExpression(133, token);
			ioperatorExpression.AddOperand(iexpression);
			ioperatorExpression.PositionLength = positionLength;
			return ioperatorExpression;
		}
	}
}
