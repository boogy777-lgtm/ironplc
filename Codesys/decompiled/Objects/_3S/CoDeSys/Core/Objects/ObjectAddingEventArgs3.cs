using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200004C RID: 76
	[ReleasedClass]
	public class ObjectAddingEventArgs3 : ObjectAddingEventArgs2
	{
		// Token: 0x06000137 RID: 311 RVA: 0x00003FD4 File Offset: 0x000021D4
		public ObjectAddingEventArgs3(int nProjectHandle, Guid parentObjectGuid, IObject obj, string stName, int nIndex, IPastedObject pastedObject, Guid newGuid) : base(nProjectHandle, parentObjectGuid, obj, stName, nIndex, pastedObject)
		{
			this._newGuid = newGuid;
		}

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x06000138 RID: 312 RVA: 0x00003FED File Offset: 0x000021ED
		public Guid NewGuid
		{
			get
			{
				return this._newGuid;
			}
		}

		// Token: 0x0400005E RID: 94
		private readonly Guid _newGuid;
	}
}
