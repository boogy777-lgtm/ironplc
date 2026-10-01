using System;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000153 RID: 339
	internal class CompilerSettings
	{
		// Token: 0x1700076F RID: 1903
		// (get) Token: 0x06001B94 RID: 7060 RVA: 0x0004E231 File Offset: 0x0004D231
		// (set) Token: 0x06001B95 RID: 7061 RVA: 0x0004E239 File Offset: 0x0004D239
		public bool AllowNestedComments
		{
			get
			{
				return this.m_bAllowNestedComments;
			}
			set
			{
				this.m_bAllowNestedComments = value;
			}
		}

		// Token: 0x040005D1 RID: 1489
		private bool m_bAllowNestedComments = true;
	}
}
