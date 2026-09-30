using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200009F RID: 159
	[ReleasedClass]
	public class StructuredViewPasteSkipEventArgs : EventArgs
	{
		// Token: 0x0600028C RID: 652 RVA: 0x00004D30 File Offset: 0x00002F30
		public StructuredViewPasteSkipEventArgs(string[] objectNames)
		{
			this._objectNames = objectNames;
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600028D RID: 653 RVA: 0x00004D3F File Offset: 0x00002F3F
		public string[] ObjectNames
		{
			get
			{
				return this._objectNames;
			}
		}

		// Token: 0x040000F9 RID: 249
		private string[] _objectNames;
	}
}
