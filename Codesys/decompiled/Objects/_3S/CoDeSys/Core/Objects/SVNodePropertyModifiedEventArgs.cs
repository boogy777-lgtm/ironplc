using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A9 RID: 169
	[ReleasedClass]
	public class SVNodePropertyModifiedEventArgs : EventArgs
	{
		// Token: 0x060002BA RID: 698 RVA: 0x00004EFA File Offset: 0x000030FA
		public SVNodePropertyModifiedEventArgs(ISVNode svNode, IObjectProperty oldProperty, IObjectProperty newProperty)
		{
			this._svNode = svNode;
			this._oldProperty = oldProperty;
			this._newProperty = newProperty;
		}

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x060002BB RID: 699 RVA: 0x00004F17 File Offset: 0x00003117
		public ISVNode Node
		{
			get
			{
				return this._svNode;
			}
		}

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060002BC RID: 700 RVA: 0x00004F1F File Offset: 0x0000311F
		public IObjectProperty OldProperty
		{
			get
			{
				return this._oldProperty;
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060002BD RID: 701 RVA: 0x00004F27 File Offset: 0x00003127
		public IObjectProperty NewProperty
		{
			get
			{
				return this._newProperty;
			}
		}

		// Token: 0x0400010D RID: 269
		private ISVNode _svNode;

		// Token: 0x0400010E RID: 270
		private IObjectProperty _oldProperty;

		// Token: 0x0400010F RID: 271
		private IObjectProperty _newProperty;
	}
}
