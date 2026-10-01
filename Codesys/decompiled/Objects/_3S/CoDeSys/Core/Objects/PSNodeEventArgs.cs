using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000090 RID: 144
	[ReleasedClass]
	public class PSNodeEventArgs : EventArgs
	{
		// Token: 0x06000249 RID: 585 RVA: 0x00004A09 File Offset: 0x00002C09
		public PSNodeEventArgs(IPSNode node)
		{
			this._node = node;
		}

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x0600024A RID: 586 RVA: 0x00004A18 File Offset: 0x00002C18
		public IPSNode Node
		{
			get
			{
				return this._node;
			}
		}

		// Token: 0x040000D3 RID: 211
		private IPSNode _node;
	}
}
