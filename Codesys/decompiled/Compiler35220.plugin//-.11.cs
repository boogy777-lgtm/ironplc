using System;
using \u0005;
using \u0006;
using \u0008;
using \u000E;
using \u0011;
using \u0014;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0081
{
	// Token: 0x02000288 RID: 648
	internal sealed class \u0010 : IReplacer
	{
		// Token: 0x060028CE RID: 10446 RVA: 0x0008EDB4 File Offset: 0x0008CFB4
		private \u0010()
		{
			this.\u0001 = null;
		}

		// Token: 0x060028CF RID: 10447 RVA: 0x0008EDC4 File Offset: 0x0008CFC4
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = false;
			IReplacer replacer = this.\u0001 as IReplacer;
			if (replacer != null)
			{
				replacer.ReplaceCode(cpou);
				return;
			}
			cpou.GetParseTree().Accept(this.\u0001);
		}

		// Token: 0x060028D0 RID: 10448 RVA: 0x0008EE00 File Offset: 0x0008D000
		public void \u0001(_IExprement \u0002)
		{
			this.\u0001 = false;
			\u0002.Accept(this.\u0001);
		}

		// Token: 0x060028D1 RID: 10449 RVA: 0x0008EE18 File Offset: 0x0008D018
		public static \u0081.\u0010 \u0001(ISpecificExpressionReplacer \u0002, global::\u000E.\u0011 \u0003)
		{
			\u0081.\u0010 u = new \u0081.\u0010();
			u.\u0001 = new global::\u0008.\u0010(u, \u0002, \u0003);
			return u;
		}

		// Token: 0x060028D2 RID: 10450 RVA: 0x0008EE30 File Offset: 0x0008D030
		public static \u0081.\u0010 \u0001(IExpressionAndStatementReplacer \u0002, global::\u000E.\u0011 \u0003)
		{
			\u0081.\u0010 u = new \u0081.\u0010();
			u.\u0001 = new global::\u0006.\u0007(u, \u0002, \u0003);
			return u;
		}

		// Token: 0x060028D3 RID: 10451 RVA: 0x0008EE48 File Offset: 0x0008D048
		public static \u0081.\u0010 \u0002(ISpecificExpressionReplacer \u0002, global::\u000E.\u0011 \u0003)
		{
			\u0081.\u0010 u = new \u0081.\u0010();
			u.\u0001 = new global::\u0014.\u0011(u, \u0002, \u0003);
			return u;
		}

		// Token: 0x060028D4 RID: 10452 RVA: 0x0008EE60 File Offset: 0x0008D060
		public static \u0081.\u0010 \u0003(ISpecificExpressionReplacer \u0002, global::\u000E.\u0011 \u0003)
		{
			\u0081.\u0010 u = new \u0081.\u0010();
			u.\u0001 = new global::\u0011.\u0012(u, \u0002, \u0003);
			return u;
		}

		// Token: 0x060028D5 RID: 10453 RVA: 0x0008EE78 File Offset: 0x0008D078
		public static \u0081.\u0010 \u0001(global::\u0011.\u000F \u0002, global::\u000E.\u0011 \u0003)
		{
			\u0081.\u0010 u = new \u0081.\u0010();
			u.\u0001 = new global::\u0005.\u0006(u, \u0002, \u0003);
			return u;
		}

		// Token: 0x04000784 RID: 1924
		public bool \u0001;

		// Token: 0x04000785 RID: 1925
		private global::\u0008.\u0010 \u0001;
	}
}
