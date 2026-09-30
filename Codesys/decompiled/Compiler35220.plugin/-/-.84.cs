using System;
using \u0019;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x02000111 RID: 273
	internal sealed class \u0004 : EmptyVisitor351900
	{
		// Token: 0x0600142F RID: 5167 RVA: 0x0003B198 File Offset: 0x00039398
		private \u0004(int \u009A\u0002, IScope \u009B\u0002)
		{
			this.\u0001 = \u009A\u0002;
			this.\u0001 = \u009B\u0002;
		}

		// Token: 0x06001430 RID: 5168 RVA: 0x0003B1B0 File Offset: 0x000393B0
		internal static void \u0001(_IExprement \u0002, int \u0003, IScope \u0004)
		{
			StandardTraverser ivisit = new StandardTraverser(new \u0004(\u0003, \u0004));
			\u0002.Accept(ivisit);
		}

		// Token: 0x06001431 RID: 5169 RVA: 0x0003B1D4 File Offset: 0x000393D4
		public override void visit(_IVariableExpression variable, AccessFlag access)
		{
			if (base.Traverser.ImplicitOn)
			{
				access |= AccessFlag.Implicit;
			}
			_IVariable ivariable = variable.GetVariable(this.\u0001) as _IVariable;
			if (ivariable == null)
			{
				return;
			}
			ivariable.AddCrossReference(this.\u0001, \u0003.\u0001(variable.Position, access));
		}

		// Token: 0x04000371 RID: 881
		private readonly int \u0001;

		// Token: 0x04000372 RID: 882
		private readonly IScope \u0001;
	}
}
