using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x0200003E RID: 62
	internal readonly struct CurrentTaskExpressionParser
	{
		// Token: 0x0600044D RID: 1101 RVA: 0x00012F89 File Offset: 0x00011189
		private CurrentTaskExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x0600044E RID: 1102 RVA: 0x00012F92 File Offset: 0x00011192
		private ParserContext Context { get; }

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x0600044F RID: 1103 RVA: 0x00012F9A File Offset: 0x0001119A
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x06000450 RID: 1104 RVA: 0x00012FA7 File Offset: 0x000111A7
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x00012FB4 File Offset: 0x000111B4
		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			CurrentTaskExpressionParser currentTaskExpressionParser = new CurrentTaskExpressionParser(context);
			return currentTaskExpressionParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x00012FD7 File Offset: 0x000111D7
		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseCurrentTaskExpression(out bError, token, startToken, out endToken);
		}

		// Token: 0x06000453 RID: 1107 RVA: 0x00012FE8 File Offset: 0x000111E8
		private _IExpression ParseCurrentTaskExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			this.Context.Scanner.MatchOperator(this.Context.ErrorHandler, new Operator[]
			{
				162
			});
			_IExpression iexpression = this.ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return this.LMItemFactory.CreateCurrentTaskExpression(iexpression, token);
		}
	}
}
