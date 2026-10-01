using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000099 RID: 153
	[ReleasedClass]
	public class StructuredViewCopyNodeEventArgs : EventArgs
	{
		// Token: 0x06000273 RID: 627 RVA: 0x00004C67 File Offset: 0x00002E67
		public StructuredViewCopyNodeEventArgs(ISVNode svNode)
		{
			this._svNode = svNode;
		}

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000274 RID: 628 RVA: 0x00004C76 File Offset: 0x00002E76
		public ISVNode SVNode
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x040000F0 RID: 240
		private ISVNode _svNode;
	}
}
