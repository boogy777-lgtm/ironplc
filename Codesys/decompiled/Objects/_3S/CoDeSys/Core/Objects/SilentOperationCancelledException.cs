using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000CC RID: 204
	[ReleasedClass]
	public class SilentOperationCancelledException : ApplicationException
	{
		// Token: 0x0600033F RID: 831 RVA: 0x000056C5 File Offset: 0x000038C5
		public SilentOperationCancelledException(Exception innerException) : base(string.Empty, innerException)
		{
		}

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x06000340 RID: 832 RVA: 0x000056D3 File Offset: 0x000038D3
		public override string Message
		{
			get
			{
				return base.InnerException.Message;
			}
		}
	}
}
