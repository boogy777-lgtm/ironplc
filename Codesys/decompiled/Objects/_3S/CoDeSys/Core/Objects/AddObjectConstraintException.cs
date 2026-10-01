using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B2 RID: 178
	[ReleasedClass]
	public class AddObjectConstraintException : ObjectManagerException
	{
		// Token: 0x060002DB RID: 731 RVA: 0x00004FD6 File Offset: 0x000031D6
		[Obsolete("Use the other constructor.", true)]
		public AddObjectConstraintException(int nProjectHandle, string stName, Guid parentObjectGuid) : base(nProjectHandle, Guid.Empty, string.Empty)
		{
			this._stName = stName;
			this._parentObjectGuid = parentObjectGuid;
		}

		// Token: 0x060002DC RID: 732 RVA: 0x00004FF7 File Offset: 0x000031F7
		public AddObjectConstraintException(int nProjectHandle, string stName, Guid parentObjectGuid, string stParentObjectFullName) : base(nProjectHandle, Guid.Empty, string.Empty, string.Empty)
		{
			this._stName = stName;
			this._parentObjectGuid = parentObjectGuid;
			this._stParentObjectFullName = stParentObjectFullName;
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060002DD RID: 733 RVA: 0x00005025 File Offset: 0x00003225
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060002DE RID: 734 RVA: 0x0000502D File Offset: 0x0000322D
		public Guid ParentObjectGuid
		{
			get
			{
				return this._parentObjectGuid;
			}
		}

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x060002DF RID: 735 RVA: 0x00005038 File Offset: 0x00003238
		public override string Message
		{
			get
			{
				string arg = (this._parentObjectGuid != Guid.Empty) ? this._stParentObjectFullName : "<root>";
				return string.Format(Resources.AddObjectConstraintException, this._stName, arg);
			}
		}

		// Token: 0x04000119 RID: 281
		private string _stName;

		// Token: 0x0400011A RID: 282
		private Guid _parentObjectGuid;

		// Token: 0x0400011B RID: 283
		private string _stParentObjectFullName;
	}
}
