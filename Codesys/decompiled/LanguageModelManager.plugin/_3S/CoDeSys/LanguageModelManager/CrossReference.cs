using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000148 RID: 328
	[TypeGuid("{1a437ea4-e044-4436-aadf-167535004673}")]
	[StorageVersion("3.3.0.0")]
	public class CrossReference : GenericObject2, ICrossReference, ICrossReferenceSerializable
	{
		// Token: 0x06001B53 RID: 6995 RVA: 0x0000AC39 File Offset: 0x00009C39
		public CrossReference()
		{
		}

		// Token: 0x06001B54 RID: 6996 RVA: 0x0004DAD8 File Offset: 0x0004CAD8
		public CrossReference(int nCodeId)
		{
			this.m_nCodeId = nCodeId;
		}

		// Token: 0x17000754 RID: 1876
		[Obsolete("removed for performance reasons")]
		public ICodePosition this[int n]
		{
			get
			{
				throw new NotSupportedException("this function is no longer supported due to memory optimization: use ICompiledPOU.GetVariableAccesses instead.");
			}
		}

		// Token: 0x17000755 RID: 1877
		// (get) Token: 0x06001B56 RID: 6998 RVA: 0x0004DAE7 File Offset: 0x0004CAE7
		[Obsolete("removed for performance reasons")]
		public int NumPositions
		{
			get
			{
				throw new NotSupportedException("this function is no longer supported due to memory optimization: use ICompiledPOU.GetVariableAccesses instead.");
			}
		}

		// Token: 0x17000756 RID: 1878
		// (get) Token: 0x06001B57 RID: 6999 RVA: 0x0004DAF3 File Offset: 0x0004CAF3
		// (set) Token: 0x06001B58 RID: 7000 RVA: 0x0004DAFB File Offset: 0x0004CAFB
		public int CodeId
		{
			get
			{
				return this.m_nCodeId;
			}
			set
			{
				this.m_nCodeId = value;
			}
		}

		// Token: 0x17000757 RID: 1879
		// (get) Token: 0x06001B59 RID: 7001 RVA: 0x0004DAE7 File Offset: 0x0004CAE7
		[Obsolete("removed for performance reasons")]
		public ICodePosition[] Positions
		{
			get
			{
				throw new NotSupportedException("this function is no longer supported due to memory optimization: use ICompiledPOU.GetVariableAccesses instead.");
			}
		}

		// Token: 0x040005BE RID: 1470
		[DefaultSerialization("CodeId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nCodeId;
	}
}
