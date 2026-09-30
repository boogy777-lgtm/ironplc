using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000041 RID: 65
	internal readonly struct ImplicitCastOperatorParser
	{
		// Token: 0x06000477 RID: 1143 RVA: 0x0001399C File Offset: 0x00011B9C
		private ImplicitCastOperatorParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000478 RID: 1144 RVA: 0x000139A5 File Offset: 0x00011BA5
		private ParserContext Context { get; }

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000479 RID: 1145 RVA: 0x000139AD File Offset: 0x00011BAD
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x0600047A RID: 1146 RVA: 0x000139BA File Offset: 0x00011BBA
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x0600047B RID: 1147 RVA: 0x000139C7 File Offset: 0x00011BC7
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x0600047C RID: 1148 RVA: 0x000139D4 File Offset: 0x00011BD4
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x0600047D RID: 1149 RVA: 0x000139E1 File Offset: 0x00011BE1
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x0600047E RID: 1150 RVA: 0x000139EE File Offset: 0x00011BEE
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x0600047F RID: 1151 RVA: 0x000139FC File Offset: 0x00011BFC
		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			ImplicitCastOperatorParser implicitCastOperatorParser = new ImplicitCastOperatorParser(context);
			return implicitCastOperatorParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x00013A1F File Offset: 0x00011C1F
		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return this.ParseImplicitCastOperator(out bError, token, startToken, out endToken);
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x00013A30 File Offset: 0x00011C30
		private _IExpression ParseImplicitCastOperator(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_ICastExpression icastExpression = this.LMItemFactory.CreateCastExpression();
			IToken token2;
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 167)
			{
				this.ErrorHandler.AddErrorSTWithToken(icastExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(167),
					this.Scanner.GetTokenText(token2)
				});
			}
			_IExpression baseExpression = this.ExpressionParser.ParseSTOperand(out bError);
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 171)
			{
				this.ErrorHandler.AddErrorSTWithToken(icastExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(171),
					this.Scanner.GetTokenText(token2)
				});
			}
			_IExpression expWithType = this.ExpressionParser.ParseSTOperand(out bError);
			if (bError)
			{
				ICompiledType compiledType = this.TypeParser.ParseType();
				icastExpression.ExplicitelySpecifiedType = compiledType;
				if (compiledType != null)
				{
					expWithType = null;
					bError = false;
				}
			}
			if (this.Next(out token2) != 15 || this.Scanner.GetOperator(token2) != 168)
			{
				this.ErrorHandler.AddErrorSTWithToken(icastExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(token2)
				});
			}
			endToken = (token2 as _IToken);
			icastExpression.ExpWithType = expWithType;
			icastExpression.BaseExpression = baseExpression;
			return this.ExpressionParser.ParseVarAccess(icastExpression, out bError, startToken, ref endToken);
		}
	}
}
