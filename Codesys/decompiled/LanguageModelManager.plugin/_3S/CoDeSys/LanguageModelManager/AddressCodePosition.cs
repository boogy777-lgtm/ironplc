using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200014A RID: 330
	[TypeGuid("{4f3d0f01-a493-4b2d-964b-38a0fad363d8}")]
	[StorageVersion("3.3.0.0")]
	public class AddressCodePosition : GenericObject2, _IAddressCodePosition, IAddressCodePosition, IAddressCodePositionSerializable
	{
		// Token: 0x06001B5C RID: 7004 RVA: 0x0004DB5C File Offset: 0x0004CB5C
		public AddressCodePosition()
		{
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x0004DB6F File Offset: 0x0004CB6F
		public AddressCodePosition(ISourcePosition sp, AccessFlag access, int nTypeSize)
		{
			this.m_access = access;
			this.m_nTypeSize = nTypeSize;
			this.m_Position = MinimalPosition.CreateMinimalPosition(sp);
		}

		// Token: 0x17000758 RID: 1880
		// (get) Token: 0x06001B5E RID: 7006 RVA: 0x0004DB9C File Offset: 0x0004CB9C
		// (set) Token: 0x06001B5F RID: 7007 RVA: 0x0004DBDC File Offset: 0x0004CBDC
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		public long PositionToSave
		{
			get
			{
				return new IntegerUnion
				{
					m_long = this.m_Position.EditorPosition,
					m_short3 = this.m_Position.PositionOffset
				}.m_long;
			}
			set
			{
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this.m_Position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x17000759 RID: 1881
		// (get) Token: 0x06001B60 RID: 7008 RVA: 0x0004DC00 File Offset: 0x0004CC00
		public long EditorPosition
		{
			get
			{
				return this.m_Position.EditorPosition;
			}
		}

		// Token: 0x1700075A RID: 1882
		// (get) Token: 0x06001B61 RID: 7009 RVA: 0x0004DC0D File Offset: 0x0004CC0D
		public short PositionOffset
		{
			get
			{
				return this.m_Position.PositionOffset;
			}
		}

		// Token: 0x1700075B RID: 1883
		// (get) Token: 0x06001B62 RID: 7010 RVA: 0x0004DC1A File Offset: 0x0004CC1A
		// (set) Token: 0x06001B63 RID: 7011 RVA: 0x0004DC22 File Offset: 0x0004CC22
		public AccessFlag Access
		{
			get
			{
				return this.m_access;
			}
			set
			{
				this.m_access = value;
			}
		}

		// Token: 0x1700075C RID: 1884
		// (get) Token: 0x06001B64 RID: 7012 RVA: 0x0004DC2B File Offset: 0x0004CC2B
		// (set) Token: 0x06001B65 RID: 7013 RVA: 0x0004DC33 File Offset: 0x0004CC33
		public int TypeSize
		{
			get
			{
				return this.m_nTypeSize;
			}
			set
			{
				this.m_nTypeSize = value;
			}
		}

		// Token: 0x1700075D RID: 1885
		// (get) Token: 0x06001B66 RID: 7014 RVA: 0x0004DC3C File Offset: 0x0004CC3C
		// (set) Token: 0x06001B67 RID: 7015 RVA: 0x0004DC44 File Offset: 0x0004CC44
		public string VariableName
		{
			get
			{
				return this.m_stVariableName;
			}
			set
			{
				this.m_stVariableName = value;
			}
		}

		// Token: 0x040005BF RID: 1471
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("AccessFlag")]
		[StorageVersion("3.3.0.0")]
		private AccessFlag m_access;

		// Token: 0x040005C0 RID: 1472
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_Position;

		// Token: 0x040005C1 RID: 1473
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("TypeSize")]
		[StorageVersion("3.3.0.0")]
		private int m_nTypeSize;

		// Token: 0x040005C2 RID: 1474
		private string m_stVariableName = string.Empty;
	}
}
