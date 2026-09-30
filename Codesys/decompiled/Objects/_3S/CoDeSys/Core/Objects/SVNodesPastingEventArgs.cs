using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000AE RID: 174
	[ReleasedClass]
	public class SVNodesPastingEventArgs : EventArgs
	{
		// Token: 0x060002CD RID: 717 RVA: 0x00004F8A File Offset: 0x0000318A
		public SVNodesPastingEventArgs(ArrayList alObjectsToPaste, int nProjectHandle, Guid guidSelectedNode)
		{
			this._alObjectsToPaste = alObjectsToPaste;
			this._nProjectHandle = nProjectHandle;
			this._guidSelectedNode = guidSelectedNode;
		}

		// Token: 0x17000100 RID: 256
		// (get) Token: 0x060002CE RID: 718 RVA: 0x00004FA7 File Offset: 0x000031A7
		public ArrayList ObjectsToPaste
		{
			get
			{
				return this._alObjectsToPaste;
			}
		}

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x060002CF RID: 719 RVA: 0x00004FAF File Offset: 0x000031AF
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x060002D0 RID: 720 RVA: 0x00004FB7 File Offset: 0x000031B7
		public Guid SelectedNodeGuid
		{
			get
			{
				return this._guidSelectedNode;
			}
		}

		// Token: 0x04000115 RID: 277
		private ArrayList _alObjectsToPaste;

		// Token: 0x04000116 RID: 278
		private int _nProjectHandle;

		// Token: 0x04000117 RID: 279
		private Guid _guidSelectedNode;
	}
}
