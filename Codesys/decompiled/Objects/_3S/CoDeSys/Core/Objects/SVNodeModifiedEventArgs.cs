using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A5 RID: 165
	[ReleasedClass]
	public class SVNodeModifiedEventArgs : EventArgs
	{
		// Token: 0x060002A9 RID: 681 RVA: 0x00004E7F File Offset: 0x0000307F
		public SVNodeModifiedEventArgs(ISVNode svNode, object editor)
		{
			this._svNode = svNode;
			this._editor = editor;
		}

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x060002AA RID: 682 RVA: 0x00004E95 File Offset: 0x00003095
		public ISVNode Node
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x060002AB RID: 683 RVA: 0x00004E9D File Offset: 0x0000309D
		public object Editor
		{
			get
			{
				return this._editor;
			}
		}

		// Token: 0x04000106 RID: 262
		private ISVNode _svNode;

		// Token: 0x04000107 RID: 263
		private object _editor;
	}
}
