using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200006C RID: 108
	[ReleasedClass]
	public class UnserializableDataErrorEventArgs2 : UnserializableDataErrorEventArgs
	{
		// Token: 0x060001C9 RID: 457 RVA: 0x000044D6 File Offset: 0x000026D6
		public UnserializableDataErrorEventArgs2(int nProjectHandle, Guid objectGuid, Type type) : base(nProjectHandle, objectGuid, type)
		{
		}

		// Token: 0x060001CA RID: 458 RVA: 0x000044E1 File Offset: 0x000026E1
		public void SetUndoOperation()
		{
			this._bUndoOperation = true;
		}

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060001CB RID: 459 RVA: 0x000044EA File Offset: 0x000026EA
		public bool UndoOperation
		{
			get
			{
				return this._bUndoOperation;
			}
		}

		// Token: 0x0400009D RID: 157
		private bool _bUndoOperation;
	}
}
