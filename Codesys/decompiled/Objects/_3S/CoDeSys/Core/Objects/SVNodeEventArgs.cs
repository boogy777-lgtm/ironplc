using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A3 RID: 163
	[ReleasedClass]
	public class SVNodeEventArgs : EventArgs
	{
		// Token: 0x060002A2 RID: 674 RVA: 0x00004E59 File Offset: 0x00003059
		public SVNodeEventArgs(ISVNode svNode, int nIndex)
		{
			this._svNode = svNode;
			this._nIndex = nIndex;
		}

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x00004E6F File Offset: 0x0000306F
		public ISVNode Node
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x060002A4 RID: 676 RVA: 0x00004E77 File Offset: 0x00003077
		public int Index
		{
			get
			{
				return this._nIndex;
			}
		}

		// Token: 0x04000104 RID: 260
		private ISVNode _svNode;

		// Token: 0x04000105 RID: 261
		private int _nIndex;
	}
}
