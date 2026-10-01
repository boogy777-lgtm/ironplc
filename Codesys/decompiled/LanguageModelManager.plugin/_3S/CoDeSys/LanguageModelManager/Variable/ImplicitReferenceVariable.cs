using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Variable
{
	// Token: 0x020001B4 RID: 436
	[TypeGuid("{C223CC6A-91D8-4ECF-861D-08EDE1761F3C}")]
	[StorageVersion("3.5.9.0")]
	public class ImplicitReferenceVariable : GenericObject2, _IImplicitReferenceVariable
	{
		// Token: 0x06001F61 RID: 8033 RVA: 0x0005650C File Offset: 0x0005550C
		public ImplicitReferenceVariable()
		{
			this.SignatureId = -1;
		}

		// Token: 0x06001F62 RID: 8034 RVA: 0x00056522 File Offset: 0x00055522
		public ImplicitReferenceVariable(int nSignatureId, _IVariable variable)
		{
			this.SignatureId = nSignatureId;
			this.Var = variable;
		}

		// Token: 0x1700083B RID: 2107
		// (get) Token: 0x06001F63 RID: 8035 RVA: 0x0005653F File Offset: 0x0005553F
		// (set) Token: 0x06001F64 RID: 8036 RVA: 0x00056547 File Offset: 0x00055547
		[DefaultSerialization("Variable")]
		[StorageVersion("3.5.9.0")]
		[StorageDefaultValue(null)]
		public _IVariable Var
		{
			get
			{
				return this.var;
			}
			set
			{
				this.var = value;
			}
		}

		// Token: 0x1700083C RID: 2108
		// (get) Token: 0x06001F65 RID: 8037 RVA: 0x00056550 File Offset: 0x00055550
		// (set) Token: 0x06001F66 RID: 8038 RVA: 0x00056558 File Offset: 0x00055558
		[DefaultSerialization("ContainingSign")]
		[StorageVersion("3.5.9.0")]
		[StorageDefaultValue(-1)]
		public int SignatureId
		{
			get
			{
				return this._SignatureId;
			}
			set
			{
				this._SignatureId = value;
			}
		}

		// Token: 0x04000623 RID: 1571
		private int _SignatureId = -1;

		// Token: 0x04000624 RID: 1572
		private _IVariable var;
	}
}
