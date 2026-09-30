using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A7 RID: 167
	[ReleasedClass]
	public class SVNodeMovedEventArgs : EventArgs
	{
		// Token: 0x060002B0 RID: 688 RVA: 0x00004EA5 File Offset: 0x000030A5
		public SVNodeMovedEventArgs(ISVNode svNode, ISVNode svOldParent, ISVNode svNewParent, int nOldIndex, int nNewIndex)
		{
			this._svNode = svNode;
			this._svOldParent = svOldParent;
			this._svNewParent = svNewParent;
			this._nOldIndex = nOldIndex;
			this._nNewIndex = nNewIndex;
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x060002B1 RID: 689 RVA: 0x00004ED2 File Offset: 0x000030D2
		public ISVNode Node
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x060002B2 RID: 690 RVA: 0x00004EDA File Offset: 0x000030DA
		public ISVNode OldParent
		{
			get
			{
				return this._svOldParent;
			}
		}

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x060002B3 RID: 691 RVA: 0x00004EE2 File Offset: 0x000030E2
		public ISVNode NewParent
		{
			get
			{
				return this._svNewParent;
			}
		}

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x060002B4 RID: 692 RVA: 0x00004EEA File Offset: 0x000030EA
		public int OldIndex
		{
			get
			{
				return this._nOldIndex;
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x060002B5 RID: 693 RVA: 0x00004EF2 File Offset: 0x000030F2
		public int NewIndex
		{
			get
			{
				return this._nNewIndex;
			}
		}

		// Token: 0x04000108 RID: 264
		private ISVNode _svNode;

		// Token: 0x04000109 RID: 265
		private ISVNode _svOldParent;

		// Token: 0x0400010A RID: 266
		private ISVNode _svNewParent;

		// Token: 0x0400010B RID: 267
		private int _nOldIndex;

		// Token: 0x0400010C RID: 268
		private int _nNewIndex;
	}
}
