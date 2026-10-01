using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000BA RID: 186
	[ReleasedClass]
	public class MoveObjectConstraintException : ObjectManagerException
	{
		// Token: 0x060002F9 RID: 761 RVA: 0x0000524F File Offset: 0x0000344F
		[Obsolete("Use the other constructor.", true)]
		public MoveObjectConstraintException(int nProjectHandle, Guid objectGuid, Guid oldParentObjectGuid, Guid newParentObjectGuid) : base(nProjectHandle, objectGuid, string.Empty)
		{
			this._oldParentObjectGuid = oldParentObjectGuid;
			this._newParentObjectGuid = newParentObjectGuid;
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0000526D File Offset: 0x0000346D
		public MoveObjectConstraintException(int nProjectHandle, Guid objectGuid, Guid oldParentObjectGuid, Guid newParentObjectGuid, string objectName, string oldParentFullName, string newParentFullName) : base(nProjectHandle, objectGuid, string.Empty, objectName)
		{
			this._oldParentObjectGuid = oldParentObjectGuid;
			this._newParentObjectGuid = newParentObjectGuid;
			this._oldParentFullName = oldParentFullName;
			this._newParentFullName = newParentFullName;
		}

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060002FB RID: 763 RVA: 0x0000529D File Offset: 0x0000349D
		public Guid OldParentObjectGuid
		{
			get
			{
				return this._oldParentObjectGuid;
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060002FC RID: 764 RVA: 0x000052A5 File Offset: 0x000034A5
		public Guid NewParentObjectGuid
		{
			get
			{
				return this._newParentObjectGuid;
			}
		}

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060002FD RID: 765 RVA: 0x000052B0 File Offset: 0x000034B0
		public override string Message
		{
			get
			{
				string arg = (this._oldParentObjectGuid != Guid.Empty) ? this._oldParentFullName : "<root>";
				string arg2 = (this._newParentObjectGuid != Guid.Empty) ? this._newParentFullName : "<root>";
				return string.Format(Resources.MoveObjectConstraintException, base.ObjectName, arg, arg2);
			}
		}

		// Token: 0x0400011F RID: 287
		private Guid _oldParentObjectGuid;

		// Token: 0x04000120 RID: 288
		private Guid _newParentObjectGuid;

		// Token: 0x04000121 RID: 289
		private string _oldParentFullName;

		// Token: 0x04000122 RID: 290
		private string _newParentFullName;
	}
}
