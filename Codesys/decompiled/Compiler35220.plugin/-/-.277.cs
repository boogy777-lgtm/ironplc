using System;
using System.Runtime.CompilerServices;
using \u0006;
using \u0011;
using \u0015;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0019
{
	// Token: 0x020002E0 RID: 736
	internal sealed class \u000F : global::\u0006.\u0002
	{
		// Token: 0x1700079B RID: 1947
		// (get) Token: 0x06002C20 RID: 11296 RVA: 0x0009AE68 File Offset: 0x00099068
		// (set) Token: 0x06002C21 RID: 11297 RVA: 0x0009AE70 File Offset: 0x00099070
		public \u0015.\u0006 Traverser { get; set; }

		// Token: 0x1700079C RID: 1948
		// (get) Token: 0x06002C22 RID: 11298 RVA: 0x0009AE7C File Offset: 0x0009907C
		// (set) Token: 0x06002C23 RID: 11299 RVA: 0x0009AE84 File Offset: 0x00099084
		public _ICompileContext Comcon { get; set; }

		// Token: 0x1700079D RID: 1949
		// (get) Token: 0x06002C24 RID: 11300 RVA: 0x0009AE90 File Offset: 0x00099090
		// (set) Token: 0x06002C25 RID: 11301 RVA: 0x0009AE98 File Offset: 0x00099098
		public IScope5 Scope { get; internal set; }

		// Token: 0x06002C26 RID: 11302 RVA: 0x0009AEA4 File Offset: 0x000990A4
		public override void \u0001(_IPragmaIfStatement \u0002)
		{
			this.Traverser.ReplacedStatement = global::\u0011.\u0013.\u0001(\u0002, this.Scope, this.Comcon, false);
		}

		// Token: 0x06002C27 RID: 11303 RVA: 0x0009AEC4 File Offset: 0x000990C4
		public override void \u0001(_IDefineStatement \u0002)
		{
			if (\u0002.Define)
			{
				this.Scope.Define(\u0002.Ident, \u0002.Value);
				return;
			}
			this.Scope.Undefine(\u0002.Ident);
		}

		// Token: 0x06002C28 RID: 11304 RVA: 0x0009AEF8 File Offset: 0x000990F8
		public override void \u0001(_IPragmaAssertion \u0002)
		{
			_IPragmaExpression ipragmaExpression = (_IPragmaExpression)\u0002.Condition;
			\u0082.\u0012.\u0001(ipragmaExpression, this.Scope, this.Comcon, false);
			if (!ipragmaExpression.Value)
			{
				ipragmaExpression.AddError(\u0002.ErrorOutput, MessageId.None);
			}
		}

		// Token: 0x04000861 RID: 2145
		[CompilerGenerated]
		private new \u0015.\u0006 \u0001;

		// Token: 0x04000862 RID: 2146
		[CompilerGenerated]
		private new _ICompileContext \u0001;

		// Token: 0x04000863 RID: 2147
		[CompilerGenerated]
		private new IScope5 \u0001;
	}
}
