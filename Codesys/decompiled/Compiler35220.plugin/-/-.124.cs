using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0002;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001D
{
	// Token: 0x02000173 RID: 371
	internal sealed class \u0003 : \u0006
	{
		// Token: 0x060018F7 RID: 6391 RVA: 0x0004DD04 File Offset: 0x0004BF04
		public \u0003(EmptyVisitor351900 \u0080\u0003) : base(\u0080\u0003)
		{
		}

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x060018F8 RID: 6392 RVA: 0x0004DD10 File Offset: 0x0004BF10
		// (set) Token: 0x060018F9 RID: 6393 RVA: 0x0004DD18 File Offset: 0x0004BF18
		public _IStatement CurrentStatement { get; set; }

		// Token: 0x060018FA RID: 6394 RVA: 0x0004DD24 File Offset: 0x0004BF24
		public override void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				statementList[i] = base.\u0001(statementList[i]);
				this.CurrentStatement = statementList[i];
				base.\u0002(statementList[i]);
			}
		}

		// Token: 0x0400046C RID: 1132
		[CompilerGenerated]
		private new _IStatement \u0001;
	}
}
