using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200004B RID: 75
	internal readonly struct ThisAndBaseExpressionParser
	{
		// Token: 0x0600050D RID: 1293 RVA: 0x00015F7A File Offset: 0x0001417A
		private ThisAndBaseExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x0600050E RID: 1294 RVA: 0x00015F83 File Offset: 0x00014183
		private ParserContext Context { get; }

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x00015F8B File Offset: 0x0001418B
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000510 RID: 1296 RVA: 0x00015F98 File Offset: 0x00014198
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x00015FA8 File Offset: 0x000141A8
		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			ThisAndBaseExpressionParser thisAndBaseExpressionParser = new ThisAndBaseExpressionParser(context);
			return thisAndBaseExpressionParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x00015FCB File Offset: 0x000141CB
		private _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			if (op == 121)
			{
				return this.ParseSuperExpression(out bError, token, startToken, out endToken);
			}
			return this.ParseThisExpression(out bError, token, startToken, out endToken);
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x00015FEC File Offset: 0x000141EC
		private _IExpression ParseSuperExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IBaseExpression exp = this.LMItemFactory.CreateBaseExpression(token);
			return this.ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}

		// Token: 0x06000514 RID: 1300 RVA: 0x0001601C File Offset: 0x0001421C
		private _IExpression ParseThisExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			_IThisExpression exp = this.LMItemFactory.CreateThisExpression(token);
			return this.ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
		}
	}
}
