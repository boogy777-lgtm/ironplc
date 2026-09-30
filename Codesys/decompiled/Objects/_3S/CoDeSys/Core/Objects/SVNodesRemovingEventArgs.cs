using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B0 RID: 176
	[ReleasedClass]
	public class SVNodesRemovingEventArgs : EventArgs
	{
		// Token: 0x060002D5 RID: 725 RVA: 0x00004FBF File Offset: 0x000031BF
		public SVNodesRemovingEventArgs(ArrayList alSVNodesToDelete)
		{
			this._alSVNodes = alSVNodesToDelete;
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060002D6 RID: 726 RVA: 0x00004FCE File Offset: 0x000031CE
		public ArrayList SVNodesToDelete
		{
			get
			{
				return this._alSVNodes;
			}
		}

		// Token: 0x04000118 RID: 280
		private ArrayList _alSVNodes;
	}
}
