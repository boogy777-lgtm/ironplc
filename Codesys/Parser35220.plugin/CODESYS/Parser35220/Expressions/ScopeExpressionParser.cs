using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000049 RID: 73
	internal readonly struct ScopeExpressionParser
	{
		// Token: 0x060004EC RID: 1260 RVA: 0x000157EC File Offset: 0x000139EC
		private ScopeExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x000157F5 File Offset: 0x000139F5
		private ParserContext Context { get; }

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x060004EE RID: 1262 RVA: 0x000157FD File Offset: 0x000139FD
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x0001580A File Offset: 0x00013A0A
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x060004F0 RID: 1264 RVA: 0x00015817 File Offset: 0x00013A17
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x060004F1 RID: 1265 RVA: 0x00015824 File Offset: 0x00013A24
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x060004F2 RID: 1266 RVA: 0x00015831 File Offset: 0x00013A31
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x060004F3 RID: 1267 RVA: 0x00015840 File Offset: 0x00013A40
		public static _IExpression ParseStatic(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			ScopeExpressionParser scopeExpressionParser = new ScopeExpressionParser(context);
			return scopeExpressionParser.Parse(out bError, op, token, startToken, out endToken);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00015864 File Offset: 0x00013A64
		public _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			if (op <= 162)
			{
				if (op == 2)
				{
					this.Context.Scanner.MatchOperator(this.Context.ErrorHandler, new Operator[]
					{
						162
					});
					return this.ParseCopyScopeExpression(out bError, token, startToken, out endToken);
				}
				if (op == 162)
				{
					return this.ParseGlobalScopeExpression(out bError, token, startToken, out endToken);
				}
			}
			else
			{
				if (op == 192)
				{
					this.Context.Scanner.MatchOperator(this.Context.ErrorHandler, new Operator[]
					{
						162
					});
					return this.ParseSystemScopeExpression(out bError, token, startToken, out endToken);
				}
				if (op == 251)
				{
					this.Context.Scanner.MatchOperator(this.Context.ErrorHandler, new Operator[]
					{
						162
					});
					return this.ParsePoolScopeExpression(out bError, token, startToken, out endToken);
				}
			}
			bError = true;
			endToken = token;
			return null;
		}

		// Token: 0x060004F5 RID: 1269 RVA: 0x0001595C File Offset: 0x00013B5C
		private _IExpression ParsePoolScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			IToken token2;
			_IExpression result;
			if (this.Next(out token2) != 13)
			{
				_IPoolScopeExpression ipoolScopeExpression = this.LMItemFactory.CreatePoolScopeExpression(this.LMItemFactory.CreateErrorExpression(), token);
				this.ErrorHandler.AddErrorSTWithToken(ipoolScopeExpression, token2, 26, new object[]
				{
					this.Scanner.GetTokenText(token2)
				});
				result = ipoolScopeExpression;
				bError = true;
			}
			else
			{
				string identifier = this.Scanner.GetIdentifier(token2);
				_IVariableExpression ivariableExpression = this.LMItemFactory.CreateVariableExpression(identifier, token2);
				_IPoolScopeExpression exp = this.LMItemFactory.CreatePoolScopeExpression(ivariableExpression, token);
				result = this.ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
			}
			return result;
		}

		// Token: 0x060004F6 RID: 1270 RVA: 0x000159FC File Offset: 0x00013BFC
		private _IExpression ParseGlobalScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			IToken token2;
			_IExpression result;
			if (this.Next(out token2) != 13)
			{
				_IGlobalScopeExpression iglobalScopeExpression = this.LMItemFactory.CreateGlobalScopeExpression(this.LMItemFactory.CreateErrorExpression(), token);
				this.ErrorHandler.AddErrorSTWithToken(iglobalScopeExpression, token2, 26, new object[]
				{
					this.Scanner.GetTokenText(token2)
				});
				result = iglobalScopeExpression;
				bError = true;
			}
			else
			{
				string identifier = this.Scanner.GetIdentifier(token2);
				_IVariableExpression ivariableExpression = this.LMItemFactory.CreateVariableExpression(identifier, token2);
				_IGlobalScopeExpression exp = this.LMItemFactory.CreateGlobalScopeExpression(ivariableExpression, token);
				result = this.ExpressionParser.ParseVarAccess(exp, out bError, startToken, ref endToken);
			}
			return result;
		}

		// Token: 0x060004F7 RID: 1271 RVA: 0x00015A9C File Offset: 0x00013C9C
		private _IExpression ParseSystemScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			_IExpression iexpression = this.ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return this.LMItemFactory.CreateSystemScopeExpression(iexpression, token);
		}

		// Token: 0x060004F8 RID: 1272 RVA: 0x00015AC6 File Offset: 0x00013CC6
		private _IExpression ParseCopyScopeExpression(out bool bError, _IToken token, _IToken startToken, out _IToken endToken)
		{
			endToken = token;
			bError = false;
			_ICopyScopeExpression icopyScopeExpression = this.LMItemFactory.CreateCopyScopeExpression(null, token);
			icopyScopeExpression._Base = this.ExpressionParser.ParseSTOperandWithPosition(out bError, startToken, out endToken);
			return icopyScopeExpression;
		}
	}
}
