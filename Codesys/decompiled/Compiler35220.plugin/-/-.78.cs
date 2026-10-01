using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u0002
{
	// Token: 0x02000108 RID: 264
	internal sealed class \u0004 : EmptyVisitor351900
	{
		// Token: 0x060013C1 RID: 5057 RVA: 0x00038D40 File Offset: 0x00036F40
		internal static bool \u0001(_IExprement \u0002, LDictionary<string, string> \u0003, bool \u0004)
		{
			if (\u0002 == null)
			{
				return false;
			}
			\u0004 u = new \u0004();
			\u0005 ivisit = new \u0005(u);
			u.\u0001 = \u0003;
			u.\u0002 = false;
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x060013C2 RID: 5058 RVA: 0x00038D78 File Offset: 0x00036F78
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if ((!this.\u0002 || access == AccessFlag.Write) && this.\u0001.ContainsKey(variable.Name.ToUpperInvariant()))
			{
				this.\u0001 = true;
				base.Traverser.Abort = true;
			}
		}

		// Token: 0x0400034B RID: 843
		private LDictionary<string, string> \u0001;

		// Token: 0x0400034C RID: 844
		private bool \u0001;

		// Token: 0x0400034D RID: 845
		private bool \u0002;
	}
}
