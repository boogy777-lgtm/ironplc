using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200004B RID: 75
	[ReleasedClass]
	public class ObjectAddingEventArgs2 : ObjectAddingEventArgs
	{
		// Token: 0x06000135 RID: 309 RVA: 0x00003FB5 File Offset: 0x000021B5
		public ObjectAddingEventArgs2(int nProjectHandle, Guid parentObjectGuid, IObject obj, string stName, int nIndex, IPastedObject pastedObject) : base(nProjectHandle, parentObjectGuid, obj, stName, nIndex)
		{
			this._pastedObject = pastedObject;
		}

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x06000136 RID: 310 RVA: 0x00003FCC File Offset: 0x000021CC
		public IPastedObject PastedObject
		{
			get
			{
				return this._pastedObject;
			}
		}

		// Token: 0x0400005D RID: 93
		private IPastedObject _pastedObject;
	}
}
