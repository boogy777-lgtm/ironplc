using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000042 RID: 66
	internal readonly struct InfixOperationParser
	{
		// Token: 0x06000482 RID: 1154 RVA: 0x00013BB2 File Offset: 0x00011DB2
		internal InfixOperationParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000483 RID: 1155 RVA: 0x00013BBB File Offset: 0x00011DBB
		private ParserContext Context { get; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000484 RID: 1156 RVA: 0x00013BC3 File Offset: 0x00011DC3
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x06000485 RID: 1157 RVA: 0x00013BD0 File Offset: 0x00011DD0
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x06000486 RID: 1158 RVA: 0x00013BDD File Offset: 0x00011DDD
		private OperandParser OperandParser
		{
			get
			{
				return this.Context.OperandParser;
			}
		}

		// Token: 0x06000487 RID: 1159 RVA: 0x00013BEA File Offset: 0x00011DEA
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x00013BF8 File Offset: 0x00011DF8
		private static bool IsMulOperator(Operator op)
		{
			return op == 159 || op == 161 || op == 126 || op == 260 || op == 261 || op == 262;
		}

		// Token: 0x06000489 RID: 1161 RVA: 0x00013C29 File Offset: 0x00011E29
		private static bool IsAddOperator(Operator op)
		{
			return op == 157 || op == 158 || op == 258 || op == 259;
		}

		// Token: 0x0600048A RID: 1162 RVA: 0x00013C50 File Offset: 0x00011E50
		private _IExpression ParseMULExp(out bool bError)
		{
			_IOperatorExpression ioperatorExpression = null;
			_IExpression iexpression = this.OperandParser.ParseSTOperand(out bError);
			if (iexpression == null)
			{
				return null;
			}
			Operator op;
			IToken token;
			for (bool flag = this.Scanner.TryNextOperator(out op, out token); flag && InfixOperationParser.IsMulOperator(op); flag = this.Scanner.TryNextOperator(out op, out token))
			{
				_IExpression iexpression2 = this.OperandParser.ParseSTOperand(out bError);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddOperandHelp(ref ioperatorExpression, iexpression, iexpression2, op, token);
			}
			this.Scanner.SetPosition(token);
			if (ioperatorExpression == null)
			{
				return iexpression;
			}
			return ioperatorExpression;
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00013CD4 File Offset: 0x00011ED4
		private _IExpression ParseADDExp(out bool bError)
		{
			_IOperatorExpression ioperatorExpression = null;
			_IExpression iexpression = this.ParseMULExp(out bError);
			if (iexpression == null)
			{
				return null;
			}
			Operator op;
			IToken token;
			for (bool flag = this.Scanner.TryNextOperator(out op, out token); flag && InfixOperationParser.IsAddOperator(op); flag = this.Scanner.TryNextOperator(out op, out token))
			{
				_IExpression iexpression2 = this.ParseMULExp(out bError);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddOperandHelp(ref ioperatorExpression, iexpression, iexpression2, op, token);
			}
			this.Scanner.SetPosition(token);
			if (ioperatorExpression == null)
			{
				return iexpression;
			}
			return ioperatorExpression;
		}

		// Token: 0x0600048C RID: 1164 RVA: 0x00013D50 File Offset: 0x00011F50
		private _IExpression ParseCompareExp(out bool bError)
		{
			_IOperatorExpression ioperatorExpression = null;
			_IExpression iexpression = this.ParseADDExp(out bError);
			if (iexpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = this.Next(out token);
			Operator @operator;
			while (tokenType == 15 && ((@operator = this.Scanner.GetOperator(token)) == 179 || @operator == 180 || @operator == 175 || @operator == 177 || @operator == 176 || @operator == 178))
			{
				_IExpression iexpression2 = this.ParseADDExp(out bError);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddOperandHelp(ref ioperatorExpression, iexpression, iexpression2, @operator, token);
				tokenType = this.Next(out token);
			}
			this.Scanner.SetPosition(token);
			if (ioperatorExpression == null)
			{
				return iexpression;
			}
			return ioperatorExpression;
		}

		// Token: 0x0600048D RID: 1165 RVA: 0x00013DF4 File Offset: 0x00011FF4
		private _IExpression ParseANDExp(out bool bError)
		{
			_IOperatorExpression ioperatorExpression = null;
			_IExpression iexpression = this.ParseCompareExp(out bError);
			if (iexpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = this.Next(out token);
			Operator @operator;
			while (tokenType == 15 && ((@operator = this.Scanner.GetOperator(token)) == 127 || @operator == 234))
			{
				_IExpression iexpression2 = this.ParseCompareExp(out bError);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddOperandHelp(ref ioperatorExpression, iexpression, iexpression2, @operator, token);
				tokenType = this.Next(out token);
			}
			this.Scanner.SetPosition(token);
			if (ioperatorExpression == null)
			{
				return iexpression;
			}
			return ioperatorExpression;
		}

		// Token: 0x0600048E RID: 1166 RVA: 0x00013E74 File Offset: 0x00012074
		internal static _IExpression ParseORExp(ParserContext context, out bool bError)
		{
			return context.InfixOperationParser.ParseORExp(out bError);
		}

		// Token: 0x0600048F RID: 1167 RVA: 0x00013E90 File Offset: 0x00012090
		private _IExpression ParseORExp(out bool bError)
		{
			_IOperatorExpression ioperatorExpression = null;
			_IExpression iexpression = this.ParseANDExp(out bError);
			if (iexpression == null)
			{
				return null;
			}
			IToken token;
			TokenType tokenType = this.Next(out token);
			Operator @operator;
			while (tokenType == 15 && ((@operator = this.Scanner.GetOperator(token)) == 129 || @operator == 235 || @operator == 131))
			{
				_IExpression iexpression2 = this.ParseANDExp(out bError);
				if (iexpression2 == null)
				{
					return null;
				}
				this.AddOperandHelp(ref ioperatorExpression, iexpression, iexpression2, @operator, token);
				tokenType = this.Next(out token);
			}
			this.Scanner.SetPosition(token);
			if (ioperatorExpression == null)
			{
				return iexpression;
			}
			return ioperatorExpression;
		}

		// Token: 0x06000490 RID: 1168 RVA: 0x00013F1C File Offset: 0x0001211C
		private void AddOperandHelp(ref _IOperatorExpression extop, _IExpression exp1, _IExpression exp2, Operator op, IToken token)
		{
			this.LMItemFactory.AddOperandHelp(ref extop, exp1, exp2, op, token);
		}
	}
}
