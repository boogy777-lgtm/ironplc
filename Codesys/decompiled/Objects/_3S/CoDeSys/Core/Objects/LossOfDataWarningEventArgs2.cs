using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000044 RID: 68
	[ReleasedClass]
	public class LossOfDataWarningEventArgs2 : LossOfDataWarningEventArgs
	{
		// Token: 0x06000121 RID: 289 RVA: 0x00003E65 File Offset: 0x00002065
		public LossOfDataWarningEventArgs2(int nProjectHandle, Guid objectGuid, Type type) : base(nProjectHandle, objectGuid, type)
		{
		}

		// Token: 0x06000122 RID: 290 RVA: 0x00003E70 File Offset: 0x00002070
		public void SetUndoOperation()
		{
			this._bUndoOperation = true;
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x06000123 RID: 291 RVA: 0x00003E79 File Offset: 0x00002079
		public bool UndoOperation
		{
			get
			{
				return this._bUndoOperation;
			}
		}

		// Token: 0x04000049 RID: 73
		private bool _bUndoOperation;
	}
}
