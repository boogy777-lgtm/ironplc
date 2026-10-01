using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001A
{
	// Token: 0x0200010A RID: 266
	internal sealed class \u0004 : EmptyVisitor351900
	{
		// Token: 0x060013CA RID: 5066 RVA: 0x00038E14 File Offset: 0x00037014
		private \u0004(int \u0087\u0002, int \u0088\u0002)
		{
			this.\u0001 = \u0087\u0002;
			this.\u0002 = \u0088\u0002;
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x00038E38 File Offset: 0x00037038
		internal static IList<ICodePosition> \u0001(_IStatement \u0002, int \u0003, int \u0004)
		{
			\u0004 u = new \u0004(\u0003, \u0004);
			StandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x00038E60 File Offset: 0x00037060
		internal static IList<ICodePosition> \u0001(_IExprement \u0002, int \u0003, int \u0004, AccessFlag \u0005)
		{
			\u0004 u = new \u0004(\u0003, \u0004);
			StandardTraverser ivisit = new StandardTraverser(u, \u0005);
			\u0002.Accept(ivisit);
			return u.\u0001;
		}

		// Token: 0x060013CD RID: 5069 RVA: 0x00038E88 File Offset: 0x00037088
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (base.Traverser.ImplicitOn)
			{
				access |= AccessFlag.Implicit;
			}
			if (variable.SignatureId == this.\u0001 && variable.VariableId == this.\u0002)
			{
				this.\u0001.Add(\u0003.\u0001(variable.Position, access));
			}
		}

		// Token: 0x04000352 RID: 850
		private LList<ICodePosition> \u0001 = new LList<ICodePosition>();

		// Token: 0x04000353 RID: 851
		private int \u0001;

		// Token: 0x04000354 RID: 852
		private int \u0002;
	}
}
