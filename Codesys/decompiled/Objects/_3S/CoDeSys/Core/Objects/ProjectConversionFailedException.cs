using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C8 RID: 200
	[ReleasedClass]
	public class ProjectConversionFailedException : ObjectManagerException
	{
		// Token: 0x06000332 RID: 818 RVA: 0x000055E1 File Offset: 0x000037E1
		public ProjectConversionFailedException(bool bPartially) : base(-1, Guid.Empty, string.Empty, string.Empty)
		{
			this._bPartially = bPartially;
		}

		// Token: 0x06000333 RID: 819 RVA: 0x00005600 File Offset: 0x00003800
		public ProjectConversionFailedException(Exception ex) : base(-1, Guid.Empty, ex.Message, string.Empty)
		{
			this._bPartially = false;
		}

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000334 RID: 820 RVA: 0x00005620 File Offset: 0x00003820
		public bool Partially
		{
			get
			{
				return this._bPartially;
			}
		}

		// Token: 0x17000138 RID: 312
		// (get) Token: 0x06000335 RID: 821 RVA: 0x00005628 File Offset: 0x00003828
		public override string Message
		{
			get
			{
				return this._stReason;
			}
		}

		// Token: 0x0400012B RID: 299
		private bool _bPartially;
	}
}
