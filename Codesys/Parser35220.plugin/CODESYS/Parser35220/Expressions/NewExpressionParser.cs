using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Declaration;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000045 RID: 69
	internal readonly struct NewExpressionParser
	{
		// Token: 0x060004B0 RID: 1200 RVA: 0x000143F8 File Offset: 0x000125F8
		private NewExpressionParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060004B1 RID: 1201 RVA: 0x00014401 File Offset: 0x00012601
		private ParserContext Context { get; }

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060004B2 RID: 1202 RVA: 0x00014409 File Offset: 0x00012609
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060004B3 RID: 1203 RVA: 0x00014416 File Offset: 0x00012616
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00014423 File Offset: 0x00012623
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x060004B5 RID: 1205 RVA: 0x00014431 File Offset: 0x00012631
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060004B6 RID: 1206 RVA: 0x0001443E File Offset: 0x0001263E
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060004B7 RID: 1207 RVA: 0x0001444B File Offset: 0x0001264B
		private TypeParser TypeParser
		{
			get
			{
				return this.Context.TypeParser;
			}
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x00014458 File Offset: 0x00012658
		private Operator ParseReSyncST(params Operator[] ops)
		{
			return this.Scanner.ParseReSyncST(ops);
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00014468 File Offset: 0x00012668
		public static _IExpression Parse(ParserContext context, out bool bError, _IToken token, out _IToken endToken)
		{
			NewExpressionParser newExpressionParser = new NewExpressionParser(context);
			return newExpressionParser.Parse(out bError, token, out endToken);
		}

		// Token: 0x060004BA RID: 1210 RVA: 0x00014487 File Offset: 0x00012687
		private _IExpression Parse(out bool bError, _IToken token, out _IToken endToken)
		{
			return this.ParseNewOperator(out bError, token, out endToken);
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x00014494 File Offset: 0x00012694
		private _IExpression ParseNewOperator(out bool bError, _IToken token, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_INewExpression inewExpression = this.LMItemFactory.CreateNewExpression(null, null, token);
			IToken token2;
			if (!this.Scanner.TryNextOperator(167, out token2))
			{
				this.ErrorHandler.AddErrorSTWithToken(inewExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(167),
					this.Scanner.GetTokenText(token2)
				});
			}
			_IType itype = this.TypeParser.ParseType();
			if (itype == null)
			{
				this.ErrorHandler.AddErrorSTWithToken(inewExpression, token2, 248, Array.Empty<object>());
			}
			if (this.Scanner.TryNextOperator(167, out token2))
			{
				this.ParseInputsForIniCall(bError, ref endToken, inewExpression);
			}
			else
			{
				this.Scanner.SetPosition(token2);
			}
			_IExpression count;
			if (this.Scanner.TryNextOperator(171, out token2))
			{
				count = this.ExpressionParser.ParseAssignment(out bError);
			}
			else
			{
				this.Scanner.SetPosition(token2);
				count = this.LMItemFactory.CreateLiteralExpression(1L);
			}
			if (!this.Scanner.TryNextOperator(168, out token2))
			{
				this.ErrorHandler.AddErrorSTWithToken(inewExpression, token2, 6, new object[]
				{
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(token2)
				});
			}
			endToken = (token2 as _IToken);
			inewExpression._Count = count;
			inewExpression._TypeToCast = itype;
			return inewExpression;
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x000145F0 File Offset: 0x000127F0
		private void ParseInputsForIniCall(bool bError, ref _IToken endToken, _INewExpression newexp)
		{
			IToken position;
			if (!this.Scanner.TryNextOperator(168, out position))
			{
				this.Scanner.SetPosition(position);
				IToken token;
				string text;
				while (this.ParseIdentifier(newexp, out token, out text))
				{
					_IErrorExpression ierrorExpression = null;
					this.CheckForAssignmentOperator(newexp, bError, out ierrorExpression);
					bool bErrorLocal;
					_IExpression rvalue = this.ParseInitialValue(out bErrorLocal);
					_IVariableExpression ivariableExpression = this.LMItemFactory.CreateVariableExpression(text, token);
					_IAssignmentExpression iassignmentExpression = this.LMItemFactory.CreateAssignmentExpression(ivariableExpression);
					iassignmentExpression._RValue = rvalue;
					newexp.AddFBInitParam(iassignmentExpression);
					Operator nextOperator = this.GetNextOperator(bErrorLocal, ref token);
					if (nextOperator == 168)
					{
						endToken = (token as _IToken);
						return;
					}
					if (nextOperator != 171 && !this.HandleError(newexp, bErrorLocal, token))
					{
						break;
					}
				}
			}
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x000146A8 File Offset: 0x000128A8
		private Operator GetNextOperator(bool bErrorLocal, ref IToken tokenTest)
		{
			Operator result = 0;
			if (bErrorLocal)
			{
				result = this.ParseReSyncST(new Operator[]
				{
					171,
					168
				});
			}
			else if (this.Next(out tokenTest) == 15)
			{
				result = this.Scanner.GetOperator(tokenTest);
			}
			return result;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x000146F4 File Offset: 0x000128F4
		private bool HandleError(_INewExpression newexp, bool bErrorLocal, IToken tokenTest)
		{
			if (!bErrorLocal)
			{
				this.ErrorHandler.AddErrorSTWithToken(newexp, tokenTest, 2, new object[]
				{
					this.Scanner.GetOperatorText(171),
					this.Scanner.GetOperatorText(168),
					this.Scanner.GetTokenText(tokenTest)
				});
				Operator @operator = this.ParseReSyncST(new Operator[]
				{
					171,
					168
				});
				if (@operator == 171)
				{
					return true;
				}
				if (@operator == 168)
				{
					return false;
				}
			}
			this.Scanner.SetPosition(tokenTest);
			return false;
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x0001478C File Offset: 0x0001298C
		private _IExpression ParseInitialValue(out bool bErrorLocal)
		{
			_IExpression iexpression = this.ExpressionParser.ParseInitialisation();
			bErrorLocal = false;
			if (iexpression is IStructureInitialization)
			{
				this.ErrorHandler.AddErrorST(iexpression, 303, Array.Empty<object>());
				bErrorLocal = true;
			}
			else if (iexpression is IArrayInitialization)
			{
				this.ErrorHandler.AddErrorST(iexpression, 304, Array.Empty<object>());
				bErrorLocal = true;
			}
			return iexpression;
		}

		// Token: 0x060004C0 RID: 1216 RVA: 0x000147F0 File Offset: 0x000129F0
		private bool ParseIdentifier(_INewExpression newexp, out IToken tokenTest, out string stIdent)
		{
			tokenTest = null;
			stIdent = null;
			if (this.Next(out tokenTest) != 13)
			{
				this.ErrorHandler.AddErrorSTWithToken(newexp, tokenTest, 26, new object[]
				{
					this.Scanner.GetTokenText(tokenTest)
				});
				this.ParseReSyncST(new Operator[]
				{
					168
				});
				return false;
			}
			stIdent = this.Scanner.GetIdentifier(tokenTest);
			return true;
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0001485C File Offset: 0x00012A5C
		private void CheckForAssignmentOperator(_IExprement exprement, bool bErrorLocal, out _IErrorExpression exprError)
		{
			exprError = null;
			Operator @operator = 164;
			IToken token;
			if (!this.Scanner.TryNextOperator(@operator, out token))
			{
				if (!bErrorLocal)
				{
					exprError = this.LMItemFactory.CreateErrorExpression(token);
					this.ErrorHandler.AddErrorSTWithToken(exprement, token, 6, new object[]
					{
						this.Scanner.GetOperatorText(@operator),
						this.Scanner.GetTokenText(token)
					});
					this.Scanner.SetPosition(token);
				}
				if (this.ParseReSyncST(new Operator[]
				{
					@operator
				}) != @operator)
				{
					if (bErrorLocal)
					{
						this.ErrorHandler.AddErrorSTWithToken(exprement, token, 6, new object[]
						{
							this.Scanner.GetOperatorText(@operator),
							this.Scanner.GetTokenText(token)
						});
					}
					this.Scanner.SetPosition(token);
				}
			}
		}
	}
}
