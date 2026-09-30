using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x02000302 RID: 770
	internal sealed class \u000F : EmptyVisitor351900
	{
		// Token: 0x06002EF5 RID: 12021 RVA: 0x000B1034 File Offset: 0x000AF234
		public bool \u0001(_IExpression \u0002, IScope5 \u0003)
		{
			this.\u0001 = \u0003;
			this.\u0001 = false;
			this.\u0001.Reset(this);
			\u0002.Accept(this.\u0001);
			return this.\u0001;
		}

		// Token: 0x170007DB RID: 2011
		// (get) Token: 0x06002EF6 RID: 12022 RVA: 0x000B1064 File Offset: 0x000AF264
		public override bool DoCallExpression
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170007DC RID: 2012
		// (get) Token: 0x06002EF7 RID: 12023 RVA: 0x000B1068 File Offset: 0x000AF268
		public override bool DoAssignExpression
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06002EF8 RID: 12024 RVA: 0x000B106C File Offset: 0x000AF26C
		public override void visit(_IAssignmentExpression assign)
		{
			this.\u0001 = true;
			this.\u0001.Abort = true;
		}

		// Token: 0x06002EF9 RID: 12025 RVA: 0x000B1084 File Offset: 0x000AF284
		public override void visit(_ICallExpression call)
		{
			this.\u0001 = true;
			this.\u0001.Abort = true;
		}

		// Token: 0x06002EFA RID: 12026 RVA: 0x000B109C File Offset: 0x000AF29C
		public override void visit(_IVariableExpression varExpr, AccessFlag flag)
		{
			IVariable variable = varExpr.GetVariable(this.\u0001);
			if (variable != null && variable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				this.\u0001 = true;
				this.\u0001.Abort = true;
			}
		}

		// Token: 0x040008F0 RID: 2288
		private readonly IStandardTraverser \u0001 = new StandardTraverser();

		// Token: 0x040008F1 RID: 2289
		private bool \u0001;

		// Token: 0x040008F2 RID: 2290
		private IScope5 \u0001;
	}
}
