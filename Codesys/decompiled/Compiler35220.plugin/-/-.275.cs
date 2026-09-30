using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0013;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x020002DD RID: 733
	internal sealed class \u0006 : \u0013.\u0001
	{
		// Token: 0x06002BFB RID: 11259 RVA: 0x0009A680 File Offset: 0x00098880
		private \u0006(\u0019.\u000F \u001B\u0004) : base(\u001B\u0004)
		{
		}

		// Token: 0x06002BFC RID: 11260 RVA: 0x0009A68C File Offset: 0x0009888C
		public static void \u0001(IScope5 \u0002, _ICompiledPOU \u0003, _ICompileContext \u0004, _IExprement \u0005)
		{
			\u0019.\u000F u000F = new \u0019.\u000F();
			\u0015.\u0006 u = new \u0015.\u0006(u000F);
			u000F.Traverser = u;
			u000F.Comcon = \u0004;
			u000F.Scope = \u0002;
			\u0005.Accept(u);
		}

		// Token: 0x06002BFD RID: 11261 RVA: 0x0009A6C0 File Offset: 0x000988C0
		public override void visit(_ISequenceStatement seq)
		{
			IList<_IStatement> statementList = seq._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				seq._StatementList[i].Accept(this);
				if (this.ReplacedStatement != null)
				{
					seq.Replace(this.ReplacedStatement, i);
				}
				this.ReplacedStatement = null;
			}
		}

		// Token: 0x17000796 RID: 1942
		// (get) Token: 0x06002BFE RID: 11262 RVA: 0x0009A714 File Offset: 0x00098914
		// (set) Token: 0x06002BFF RID: 11263 RVA: 0x0009A71C File Offset: 0x0009891C
		public _IStatement ReplacedStatement { get; set; }

		// Token: 0x0400085A RID: 2138
		[CompilerGenerated]
		private new _IStatement \u0001;
	}
}
