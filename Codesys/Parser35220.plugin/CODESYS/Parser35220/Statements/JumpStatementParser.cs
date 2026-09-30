using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000027 RID: 39
	internal readonly struct JumpStatementParser
	{
		// Token: 0x0600029B RID: 667 RVA: 0x0000DDB9 File Offset: 0x0000BFB9
		private JumpStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000DDC4 File Offset: 0x0000BFC4
		internal static _IJumpStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			JumpStatementParser jumpStatementParser = new JumpStatementParser(context);
			return jumpStatementParser.ParseJump(out bError, token);
		}

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600029D RID: 669 RVA: 0x0000DDE2 File Offset: 0x0000BFE2
		private ParserContext Context { get; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600029E RID: 670 RVA: 0x0000DDEA File Offset: 0x0000BFEA
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x0600029F RID: 671 RVA: 0x0000DDF7 File Offset: 0x0000BFF7
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x060002A0 RID: 672 RVA: 0x0000DE09 File Offset: 0x0000C009
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060002A1 RID: 673 RVA: 0x0000DE16 File Offset: 0x0000C016
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000DE23 File Offset: 0x0000C023
		private void Next(out IToken token)
		{
			this.Scanner.Next(out token);
		}

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x0000DE32 File Offset: 0x0000C032
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060002A4 RID: 676 RVA: 0x0000DE3F File Offset: 0x0000C03F
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x060002A5 RID: 677 RVA: 0x0000DE4D File Offset: 0x0000C04D
		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000DE64 File Offset: 0x0000C064
		private _IJumpStatement ParseJump(out bool bError, IToken tokenJump)
		{
			string text = string.Empty;
			bError = false;
			_IJumpStatement ijumpStatement = this.LMItemFactory.CreateJumpStatement(text, tokenJump);
			IToken token;
			this.Next(out token);
			if (token.Type == 15 && this.Scanner.GetOperator(token) == 167)
			{
				_IExpression iexpression = this.ParseAssignExp(out bError) ?? this.LMItemFactory.CreateErrorExpression(token);
				_IErrorExpression ierrorExpression;
				this.CheckForOperator(iexpression, 168, bError, out ierrorExpression);
				this.Next(out token);
				_IJumpStatement ijumpStatement2 = ijumpStatement;
				_IExpression iexpression2 = ierrorExpression;
				ijumpStatement2._Condition = (iexpression2 ?? iexpression);
			}
			if (token.Type != 13)
			{
				this.AddErrorSTWithToken(ijumpStatement, token, 114, new object[]
				{
					this.Scanner.GetTokenText(token)
				});
				text = "Invalid: " + this.Scanner.GetTokenText(token);
			}
			else
			{
				text = this.Scanner.GetIdentifier(token);
			}
			ijumpStatement.Label = text;
			return ijumpStatement;
		}
	}
}
