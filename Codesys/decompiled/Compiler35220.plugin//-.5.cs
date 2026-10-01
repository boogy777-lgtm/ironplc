using System;
using \u0012;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u007F
{
	// Token: 0x020001C6 RID: 454
	internal sealed class \u0005 : \u0002
	{
		// Token: 0x0600209C RID: 8348 RVA: 0x0006F0E8 File Offset: 0x0006D2E8
		public \u0005() : base(false)
		{
		}

		// Token: 0x0600209D RID: 8349 RVA: 0x0006F0F4 File Offset: 0x0006D2F4
		public override void \u0001(_IVariableExpression \u0002, AccessFlag \u0003)
		{
			base.\u0001(\u0002, \u0003);
			base.Writer.Write(\u0002.VariableId);
			base.Writer.Write(\u0002.SignatureId);
		}
	}
}
