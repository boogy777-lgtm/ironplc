using System;
using System.Linq;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000029 RID: 41
	internal readonly struct ReturnStatementParser
	{
		// Token: 0x060002B6 RID: 694 RVA: 0x0000E177 File Offset: 0x0000C377
		private ReturnStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060002B7 RID: 695 RVA: 0x0000E180 File Offset: 0x0000C380
		internal static _IReturnStatement Parse(ParserContext context, out bool bError, IToken tokenTry)
		{
			ReturnStatementParser returnStatementParser = new ReturnStatementParser(context);
			return returnStatementParser.ParseReturn(out bError, tokenTry);
		}

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060002B8 RID: 696 RVA: 0x0000E19E File Offset: 0x0000C39E
		private ParserContext Context { get; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060002B9 RID: 697 RVA: 0x0000E1A6 File Offset: 0x0000C3A6
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x060002BA RID: 698 RVA: 0x0000E1B3 File Offset: 0x0000C3B3
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060002BB RID: 699 RVA: 0x0000E1C0 File Offset: 0x0000C3C0
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x060002BC RID: 700 RVA: 0x0000E1CD File Offset: 0x0000C3CD
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060002BD RID: 701 RVA: 0x0000E1DC File Offset: 0x0000C3DC
		private _IReturnStatement ParseReturn(out bool bError, IToken tokenRet)
		{
			bError = false;
			_IReturnStatement ireturnStatement = this.LMItemFactory.CreateReturnStatement(tokenRet);
			IToken token;
			this.Scanner.Next(out token);
			if (token.Type == 15 && this.Scanner.GetOperator(token) == 167)
			{
				_IExpression iexpression = this.ExpressionParser.ParseAssignExp(out bError) ?? this.LMItemFactory.CreateErrorExpression(token);
				_IErrorExpression ierrorExpression;
				this.StatementParser.CheckForOperator(iexpression, 168, bError, out ierrorExpression);
				if (ierrorExpression != null)
				{
					foreach (_ICompilerMessage icompilerMessage in from m in iexpression.MessagesList
					where 2 == m.Severity
					select m)
					{
						ierrorExpression.AddError(icompilerMessage.Text, icompilerMessage.MessageId);
					}
					ireturnStatement._Condition = ierrorExpression;
				}
				else
				{
					ireturnStatement._Condition = iexpression;
				}
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return ireturnStatement;
		}
	}
}
