using System;
using System.Runtime.CompilerServices;
using \u0004;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u007F;

namespace \u001E
{
	// Token: 0x020002C4 RID: 708
	internal sealed class \u0012 : AbstractStatementReplacer, IReplacer
	{
		// Token: 0x17000778 RID: 1912
		// (get) Token: 0x06002B14 RID: 11028 RVA: 0x00097EB8 File Offset: 0x000960B8
		private StatementReplacerVisitor Visitor { get; }

		// Token: 0x06002B15 RID: 11029 RVA: 0x00097EC0 File Offset: 0x000960C0
		private \u0012(global::\u000E.\u0011 \u0083\u0005)
		{
			this.Visitor = new StatementReplacerVisitor(this, \u0083\u0005);
			this.\u0001 = \u0083\u0005._Scope;
		}

		// Token: 0x06002B16 RID: 11030 RVA: 0x00097EF0 File Offset: 0x000960F0
		internal static ReplacerController \u0001(global::\u000E.\u0011 \u0002)
		{
			return new ReplacerController(new \u001E.\u0012(\u0002), null);
		}

		// Token: 0x06002B17 RID: 11031 RVA: 0x00097F00 File Offset: 0x00096100
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.Visitor.ReplaceCode(cpou);
		}

		// Token: 0x06002B18 RID: 11032 RVA: 0x00097F10 File Offset: 0x00096110
		public override _IStatement visit(_IExpressionStatement expstat)
		{
			if (global::\u0004.\u0004.\u0001(expstat) && !this.\u0001.\u0001(expstat._Expr, this.\u0001))
			{
				_IEmptyStatement iemptyStatement = global::\u0019.\u0003.\u0001();
				if (expstat.MessagesList != null)
				{
					foreach (_ICompilerMessage cm in expstat.MessagesList)
					{
						iemptyStatement.AddMessage(cm);
					}
				}
				return iemptyStatement;
			}
			return null;
		}

		// Token: 0x04000829 RID: 2089
		[CompilerGenerated]
		private readonly StatementReplacerVisitor \u0001;

		// Token: 0x0400082A RID: 2090
		private readonly \u007F.\u000F \u0001 = new \u007F.\u000F();

		// Token: 0x0400082B RID: 2091
		private readonly IScope5 \u0001;
	}
}
