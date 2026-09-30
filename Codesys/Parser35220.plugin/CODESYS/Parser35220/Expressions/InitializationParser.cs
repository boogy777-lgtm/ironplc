using System;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Expressions
{
	// Token: 0x02000043 RID: 67
	internal class InitializationParser
	{
		// Token: 0x06000491 RID: 1169 RVA: 0x00013F30 File Offset: 0x00012130
		internal InitializationParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x06000492 RID: 1170 RVA: 0x00013F3F File Offset: 0x0001213F
		// (set) Token: 0x06000493 RID: 1171 RVA: 0x00013F47 File Offset: 0x00012147
		internal bool InSTCode { get; set; }

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000494 RID: 1172 RVA: 0x00013F50 File Offset: 0x00012150
		private ParserContext Context { get; }

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x06000495 RID: 1173 RVA: 0x00013F58 File Offset: 0x00012158
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x06000496 RID: 1174 RVA: 0x00013F65 File Offset: 0x00012165
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x06000497 RID: 1175 RVA: 0x00013F72 File Offset: 0x00012172
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000498 RID: 1176 RVA: 0x00013F7F File Offset: 0x0001217F
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x06000499 RID: 1177 RVA: 0x00013F90 File Offset: 0x00012190
		public _IExpression ParseInitialisation()
		{
			bool flag = false;
			return this.ParseInitialisation(ref flag);
		}

		// Token: 0x0600049A RID: 1178 RVA: 0x00013FA8 File Offset: 0x000121A8
		internal _IExpression ParseInitialisation(ref bool bBreakInit)
		{
			_IExpression iexpression = this.ParseStructInitialisation(ref bBreakInit, true);
			if (bBreakInit)
			{
				return iexpression;
			}
			if (iexpression != null)
			{
				return iexpression;
			}
			iexpression = this.ParseArrayInitialisation(ref bBreakInit);
			if (bBreakInit)
			{
				return iexpression;
			}
			if (iexpression != null)
			{
				return iexpression;
			}
			bool flag;
			return this.ExpressionParser.ParseAssignment(out flag);
		}

		// Token: 0x0600049B RID: 1179 RVA: 0x00013FEC File Offset: 0x000121EC
		internal _IExpression ParseInitialisationExp(out bool bError)
		{
			_IExpression iexpression = this.ExpressionParser.ParseORExp(out bError);
			IToken token;
			int num = this.Next(out token);
			Operator @operator = 0;
			if ((num == 15 && (@operator = this.Scanner.GetOperator(token)) == 164) || @operator == 165 || @operator == 166 || @operator == 185 || (@operator == 189 && !this.InSTCode))
			{
				_IExpression rvalue = this.ParseAnyInitialisationExpression(out bError);
				_IAssignmentExpression iassignmentExpression = this.LMItemFactory.CreateAssignmentExpression(iexpression, token);
				iassignmentExpression._RValue = rvalue;
				iassignmentExpression.KindOf = @operator;
				iexpression = iassignmentExpression;
			}
			else
			{
				this.Scanner.SetPosition(token);
			}
			return iexpression;
		}

		// Token: 0x0600049C RID: 1180 RVA: 0x00014084 File Offset: 0x00012284
		private _IExpression ParseAnyInitialisationExpression(out bool bError)
		{
			bError = false;
			bool flag = false;
			_IExpression iexpression = this.ParseStructInitialisation(ref flag, false);
			if (iexpression != null)
			{
				return iexpression;
			}
			iexpression = this.ParseArrayInitialisation(ref flag);
			if (iexpression != null)
			{
				return iexpression;
			}
			return this.ExpressionParser.ParseAssignment(out bError);
		}

		// Token: 0x0600049D RID: 1181 RVA: 0x000140BF File Offset: 0x000122BF
		private _IExpression ParseStructInitialisation(ref bool bBreakInit, bool bInterface)
		{
			return StructureInitializationParser.ParseStructInitialisation(this.Context, this, ref bBreakInit, bInterface);
		}

		// Token: 0x0600049E RID: 1182 RVA: 0x000140CF File Offset: 0x000122CF
		private _IExpression ParseArrayInitialisation(ref bool bBreakInit)
		{
			return ArrayInitializationParser.ParseArrayInitialisation(this.Context, this, ref bBreakInit);
		}
	}
}
