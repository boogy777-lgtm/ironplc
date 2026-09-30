using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000042 RID: 66
	[ReleasedClass]
	public class ObjectTakeOverRequiredEventArgs : EventArgs
	{
		// Token: 0x06000119 RID: 281 RVA: 0x00003E08 File Offset: 0x00002008
		public ObjectTakeOverRequiredEventArgs(IProjectSourceNode node)
		{
			this._node = node;
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00003E17 File Offset: 0x00002017
		public IProjectSourceNode Node
		{
			get
			{
				return this._node;
			}
		}

		// Token: 0x04000044 RID: 68
		private readonly IProjectSourceNode _node;
	}
}
