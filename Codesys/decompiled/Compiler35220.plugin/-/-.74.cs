using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x02000101 RID: 257
	internal sealed class \u0005
	{
		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x0600131F RID: 4895 RVA: 0x00034FD4 File Offset: 0x000331D4
		public IType DerivedTypeNoDeref
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06001320 RID: 4896 RVA: 0x00034FDC File Offset: 0x000331DC
		// (set) Token: 0x06001321 RID: 4897 RVA: 0x00035010 File Offset: 0x00033210
		public IType DerivedType
		{
			get
			{
				if (this.\u0001 == null)
				{
					return null;
				}
				if (this.\u0001.Class == TypeClass.Enum)
				{
					return this.\u0001;
				}
				return (this.\u0001 as _IType).DeRefType;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06001322 RID: 4898 RVA: 0x0003501C File Offset: 0x0003321C
		// (set) Token: 0x06001323 RID: 4899 RVA: 0x00035024 File Offset: 0x00033224
		public AccessFlag Access
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06001324 RID: 4900 RVA: 0x00035030 File Offset: 0x00033230
		// (set) Token: 0x06001325 RID: 4901 RVA: 0x00035038 File Offset: 0x00033238
		public IPrecompileScope2 Scope
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06001326 RID: 4902 RVA: 0x00035044 File Offset: 0x00033244
		// (set) Token: 0x06001327 RID: 4903 RVA: 0x0003504C File Offset: 0x0003324C
		public IPrecompileScope2 DerivedScope
		{
			get
			{
				return this.\u0002;
			}
			set
			{
				this.\u0002 = value;
			}
		}

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x06001328 RID: 4904 RVA: 0x00035058 File Offset: 0x00033258
		// (set) Token: 0x06001329 RID: 4905 RVA: 0x00035060 File Offset: 0x00033260
		public ISignature CurrentSignature
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x0600132A RID: 4906 RVA: 0x0003506C File Offset: 0x0003326C
		// (set) Token: 0x0600132B RID: 4907 RVA: 0x00035074 File Offset: 0x00033274
		public IVariable CurrentVariable
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x170004BC RID: 1212
		// (get) Token: 0x0600132C RID: 4908 RVA: 0x00035080 File Offset: 0x00033280
		// (set) Token: 0x0600132D RID: 4909 RVA: 0x00035088 File Offset: 0x00033288
		public VarFlag VarFlag
		{
			get
			{
				return this.\u0001;
			}
			set
			{
				this.\u0001 = value;
			}
		}

		// Token: 0x04000329 RID: 809
		private IType \u0001;

		// Token: 0x0400032A RID: 810
		private AccessFlag \u0001 = AccessFlag.Unknown;

		// Token: 0x0400032B RID: 811
		private VarFlag \u0001 = VarFlag.Local;

		// Token: 0x0400032C RID: 812
		private IPrecompileScope2 \u0001;

		// Token: 0x0400032D RID: 813
		private IPrecompileScope2 \u0002;

		// Token: 0x0400032E RID: 814
		private ISignature \u0001;

		// Token: 0x0400032F RID: 815
		private IVariable \u0001;
	}
}
