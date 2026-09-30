using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000091 RID: 145
	[ReleasedClass]
	public class PSPasteSkipEventArgs : EventArgs
	{
		// Token: 0x0600024B RID: 587 RVA: 0x00004A20 File Offset: 0x00002C20
		public PSPasteSkipEventArgs(string[] objectNames)
		{
			this._objectNames = objectNames;
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x0600024C RID: 588 RVA: 0x00004A2F File Offset: 0x00002C2F
		public string[] ObjectNames
		{
			get
			{
				return this._objectNames;
			}
		}

		// Token: 0x040000D4 RID: 212
		private string[] _objectNames;
	}
}
