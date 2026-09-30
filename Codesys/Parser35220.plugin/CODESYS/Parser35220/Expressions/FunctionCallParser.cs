using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000040 RID: 64
	internal readonly struct FunctionCallParser
	{
		// Token: 0x06000468 RID: 1128 RVA: 0x000135D7 File Offset: 0x000117D7
		internal FunctionCallParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000469 RID: 1129 RVA: 0x000135E0 File Offset: 0x000117E0
		private ParserContext Context { get; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x0600046A RID: 1130 RVA: 0x000135E8 File Offset: 0x000117E8
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x0600046B RID: 1131 RVA: 0x000135F5 File Offset: 0x000117F5
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x0600046C RID: 1132 RVA: 0x00013602 File Offset: 0x00011802
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0001360F File Offset: 0x0001180F
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x0600046E RID: 1134 RVA: 0x0001361D File Offset: 0x0001181D
		private Operator ParseReSyncST(params Operator[] ops)
		{
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x0600046F RID: 1135 RVA: 0x0001362B File Offset: 0x0001182B
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x00013638 File Offset: 0x00011838
		internal static _IExpression ParseFunctionCall(ParserContext context, _IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			return context.FunctionCallParser.ParseFunctionCall(exp, out bError, currentToken, ref endToken);
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x00013658 File Offset: 0x00011858
		private _IExpression ParseFunctionCall(_IExpression exp, out bool bError, _IToken currentToken, ref _IToken endToken)
		{
			bError = false;
			_ICallExpression icallExpression = this.LMItemFactory.CreateCallExpression(exp, currentToken);
			IToken position;
			if (this.Scanner.TryNextOperator(168, out position))
			{
				return icallExpression;
			}
			this.Scanner.SetPosition(position);
			while (!this.ParseOperand(ref bError, ref endToken, icallExpression))
			{
			}
			return icallExpression;
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x000136A8 File Offset: 0x000118A8
		private bool ParseOperand(ref bool bError, ref _IToken endToken, _ICallExpression call)
		{
			string empty = string.Empty;
			bool flag = false;
			IToken token;
			if (this.Next(out token) == 13)
			{
				IToken token2;
				if (this.Next(out token2) == 15)
				{
					Operator @operator = this.Scanner.GetOperator(token2);
					bool result;
					if (this.ParseWeirdAssignment(call, @operator, token, ref empty, ref flag, out result))
					{
						return result;
					}
				}
				else
				{
					this.Scanner.SetPosition(token);
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			bool bErrorLocal;
			_IExpression iexpression = this.ExpressionParser.ParseAssignment(out bErrorLocal);
			_IVariableExpression ivariableExpression = null;
			if (empty != string.Empty)
			{
				ivariableExpression = this.LMItemFactory.CreateVariableExpression(empty, token);
			}
			if (flag)
			{
				call.AddOutput(ivariableExpression, iexpression);
			}
			else
			{
				call.AddParam(iexpression, ivariableExpression);
			}
			return this.CheckForBreak(ref bError, ref endToken, call, bErrorLocal);
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x00013764 File Offset: 0x00011964
		private bool ParseWeirdAssignment(_ICallExpression call, Operator opHelp, IToken tokenTest, ref string stIdent, ref bool bOutput, out bool bDone)
		{
			if (opHelp == 164 || opHelp == 174)
			{
				stIdent = this.Scanner.GetIdentifier(tokenTest);
				bOutput = (opHelp == 174);
				IToken position;
				if (this.Scanner.TryNextOperator(out opHelp, out position))
				{
					if (this.CheckForFinalComma(call, opHelp, tokenTest, stIdent, out bDone))
					{
						return true;
					}
					if (this.CheckForEmptyAssignment(call, opHelp, tokenTest, stIdent))
					{
						bDone = true;
						return true;
					}
					bDone = false;
					this.Scanner.SetPosition(position);
				}
				else
				{
					this.Scanner.SetPosition(position);
				}
			}
			else
			{
				this.Scanner.SetPosition(tokenTest);
			}
			bDone = false;
			return false;
		}

		// Token: 0x06000474 RID: 1140 RVA: 0x00013803 File Offset: 0x00011A03
		private bool CheckForEmptyAssignment(_ICallExpression call, Operator opHelp, IToken tokenTest, string stIdent)
		{
			if (opHelp == 168)
			{
				call.AddEmptyAssign(this.LMItemFactory.CreateVariableExpression(stIdent, tokenTest));
				return true;
			}
			return false;
		}

		// Token: 0x06000475 RID: 1141 RVA: 0x00013824 File Offset: 0x00011A24
		private bool CheckForFinalComma(_ICallExpression call, Operator opHelp, IToken tokenTest, string stIdent, out bool bDone)
		{
			if (opHelp != 171)
			{
				bDone = false;
				return false;
			}
			IToken position;
			if (this.Scanner.TryNextOperator(168, out position))
			{
				bDone = true;
				return true;
			}
			this.Scanner.SetPosition(position);
			call.AddEmptyAssign(this.LMItemFactory.CreateVariableExpression(stIdent, tokenTest));
			bDone = false;
			return true;
		}

		// Token: 0x06000476 RID: 1142 RVA: 0x00013880 File Offset: 0x00011A80
		private bool CheckForBreak(ref bool bError, ref _IToken endToken, _ICallExpression call, bool bErrorLocal)
		{
			Operator @operator;
			IToken token;
			if (bErrorLocal)
			{
				this.Scanner.ParseReSyncToken(out @operator, out token, new Operator[]
				{
					171,
					168
				});
			}
			else
			{
				this.Scanner.TryNextOperator(out @operator, out token);
			}
			if (@operator == 168)
			{
				endToken = (token as _IToken);
				return true;
			}
			if (@operator != 171)
			{
				if (!bErrorLocal)
				{
					this.ErrorHandler.AddErrorSTWithToken(call, token, 2, new object[]
					{
						this.Scanner.GetOperatorText(171),
						this.Scanner.GetOperatorText(168),
						this.Scanner.GetTokenText(token)
					});
					@operator = this.ParseReSyncST(new Operator[]
					{
						171,
						168
					});
					if (@operator == 171)
					{
						return false;
					}
					if (@operator == 168)
					{
						return true;
					}
				}
				if (token != null)
				{
					this.Scanner.SetPosition(token);
				}
				bError = true;
				return true;
			}
			IToken position;
			if (this.Scanner.TryNextOperator(168, out position))
			{
				return true;
			}
			this.Scanner.SetPosition(position);
			return false;
		}
	}
}
