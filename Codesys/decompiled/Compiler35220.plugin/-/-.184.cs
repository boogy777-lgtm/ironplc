using System;
using \u0002;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0015
{
	// Token: 0x020001FC RID: 508
	internal sealed class \u0005 : \u0006
	{
		// Token: 0x06002201 RID: 8705 RVA: 0x00075C70 File Offset: 0x00073E70
		public \u0005(EmptyVisitor351900 \u0080\u0003) : base(\u0080\u0003)
		{
		}

		// Token: 0x06002202 RID: 8706 RVA: 0x00075C7C File Offset: 0x00073E7C
		public override void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002._RValue = base.\u0001(\u0002._RValue);
			base.\u0002(\u0002._RValue);
			\u0002._LValue = base.\u0001(\u0002._LValue);
			base.\u0002(\u0002._LValue);
		}
	}
}
