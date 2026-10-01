using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000AB RID: 171
	[ReleasedClass]
	public class SVNodeRenamedEventArgs : EventArgs
	{
		// Token: 0x060002C2 RID: 706 RVA: 0x00004F2F File Offset: 0x0000312F
		public SVNodeRenamedEventArgs(ISVNode svNode, string stOldName, string stNewName)
		{
			this._svNode = svNode;
			this._stOldName = stOldName;
			this._stNewName = stNewName;
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060002C3 RID: 707 RVA: 0x00004F4C File Offset: 0x0000314C
		public ISVNode Node
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060002C4 RID: 708 RVA: 0x00004F54 File Offset: 0x00003154
		public string OldName
		{
			get
			{
				return this._stOldName;
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060002C5 RID: 709 RVA: 0x00004F5C File Offset: 0x0000315C
		public string NewName
		{
			get
			{
				return this._stNewName;
			}
		}

		// Token: 0x04000110 RID: 272
		private ISVNode _svNode;

		// Token: 0x04000111 RID: 273
		private string _stOldName;

		// Token: 0x04000112 RID: 274
		private string _stNewName;
	}
}
