using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000023 RID: 35
	internal readonly struct ConditionalCallParser
	{
		// Token: 0x06000261 RID: 609 RVA: 0x0000D228 File Offset: 0x0000B428
		private ConditionalCallParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0000D234 File Offset: 0x0000B434
		internal static _IExpressionStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			ConditionalCallParser conditionalCallParser = new ConditionalCallParser(context);
			return conditionalCallParser.ParseConditionalCall(out bError, token);
		}

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000263 RID: 611 RVA: 0x0000D252 File Offset: 0x0000B452
		private ParserContext Context { get; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000264 RID: 612 RVA: 0x0000D25A File Offset: 0x0000B45A
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x06000265 RID: 613 RVA: 0x0000D267 File Offset: 0x0000B467
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000266 RID: 614 RVA: 0x0000D279 File Offset: 0x0000B479
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000267 RID: 615 RVA: 0x0000D286 File Offset: 0x0000B486
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000D293 File Offset: 0x0000B493
		private Operator MatchOperator(_IExprement exp, bool bGenerateError, params Operator[] ops)
		{
			return this.Scanner.MatchOperator(this.Context.ErrorHandler, exp, bGenerateError, ops);
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000269 RID: 617 RVA: 0x0000D2AE File Offset: 0x0000B4AE
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x0600026A RID: 618 RVA: 0x0000D2BB File Offset: 0x0000B4BB
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x0600026B RID: 619 RVA: 0x0000D2C9 File Offset: 0x0000B4C9
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600026C RID: 620 RVA: 0x0000D2E0 File Offset: 0x0000B4E0
		private _IExpressionStatement ParseConditionalCall(out bool bError, IToken tokenCalc)
		{
			bError = false;
			_IExpression iexpression = this.LMItemFactory.CreateNullExpression(tokenCalc);
			_IErrorExpression ierrorExpression = null;
			this.CheckForOperator(iexpression, 167, bError, out ierrorExpression);
			_IExpression iexpression2 = this.ParseAssignExp(out bError);
			if (iexpression2 == null)
			{
				iexpression2 = this.LMItemFactory.CreateErrorExpression(tokenCalc);
			}
			else
			{
				_ILiteralExpression iliteralExpression = iexpression2 as _ILiteralExpression;
				if (iliteralExpression != null && iliteralExpression.LiteralValue.KindOf == 4 && iliteralExpression.LiteralValue.Bool)
				{
					iexpression2 = null;
				}
			}
			if (iexpression2 != null)
			{
				iexpression2.MessagesList = iexpression.MessagesList;
			}
			this.CheckForOperator(iexpression, 171, bError, out ierrorExpression);
			_ICallExpression icallExpression = this.ExpressionParser.ParseSTOperand(out bError) as _ICallExpression;
			if (this.MatchOperator(iexpression, bError, new Operator[]
			{
				171,
				168
			}) == 171)
			{
				_IType expectedType = this.Context.TypeParser.ParseType();
				if (icallExpression != null)
				{
					icallExpression.ExpectedType = expectedType;
				}
				this.CheckForOperator(iexpression, 168, bError, out ierrorExpression);
			}
			if (icallExpression == null)
			{
				_IExpressionStatement iexpressionStatement = this.LMItemFactory.CreateExpressionStatement(iexpression2, tokenCalc);
				this.AddErrorST(iexpressionStatement, 115, Array.Empty<object>());
				return iexpressionStatement;
			}
			icallExpression._Condition = iexpression2;
			return this.LMItemFactory.CreateExpressionStatement(icallExpression, tokenCalc);
		}
	}
}
