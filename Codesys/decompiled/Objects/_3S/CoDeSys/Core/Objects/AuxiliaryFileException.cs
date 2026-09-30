using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B3 RID: 179
	[ReleasedClass]
	public class AuxiliaryFileException : ObjectManagerException
	{
		// Token: 0x060002E0 RID: 736 RVA: 0x00005076 File Offset: 0x00003276
		public AuxiliaryFileException() : base(-1, Guid.Empty, string.Empty, string.Empty)
		{
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000508E File Offset: 0x0000328E
		public AuxiliaryFileException(Exception ex) : base(-1, Guid.Empty, ex.Message, string.Empty)
		{
		}

		// Token: 0x17000107 RID: 263
		// (get) Token: 0x060002E2 RID: 738 RVA: 0x000050A7 File Offset: 0x000032A7
		public override string Message
		{
			get
			{
				return this._stReason;
			}
		}
	}
}
