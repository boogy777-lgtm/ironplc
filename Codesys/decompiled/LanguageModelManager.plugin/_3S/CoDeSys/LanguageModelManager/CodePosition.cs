using System;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000147 RID: 327
	[TypeGuid("{3860031f-2d04-4123-a89a-87e04be50e51}")]
	[StorageVersion("3.3.0.0")]
	public class CodePosition : GenericObject2, ICodePosition2, ICodePosition
	{
		// Token: 0x06001B48 RID: 6984 RVA: 0x0004D99B File Offset: 0x0004C99B
		public CodePosition()
		{
			this.m_access = AccessFlag.None;
			this.m_Position = null;
		}

		// Token: 0x06001B49 RID: 6985 RVA: 0x0004D9B1 File Offset: 0x0004C9B1
		public CodePosition(ISourcePosition sp, AccessFlag access)
		{
			this.m_access = access;
			this.m_Position = MinimalPosition.CreateMinimalPosition(sp);
		}

		// Token: 0x1700074F RID: 1871
		// (get) Token: 0x06001B4A RID: 6986 RVA: 0x0004D9CC File Offset: 0x0004C9CC
		// (set) Token: 0x06001B4B RID: 6987 RVA: 0x0004DA0C File Offset: 0x0004CA0C
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		private long PositionToSave
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

		// Token: 0x17000750 RID: 1872
		// (get) Token: 0x06001B4C RID: 6988 RVA: 0x0004DA30 File Offset: 0x0004CA30
		internal IMinimalPosition Position
		{
			get
			{
				return this.m_Position;
			}
		}

		// Token: 0x17000751 RID: 1873
		// (get) Token: 0x06001B4D RID: 6989 RVA: 0x0004DA38 File Offset: 0x0004CA38
		public long EditorPosition
		{
			get
			{
				return this.m_Position.EditorPosition;
			}
		}

		// Token: 0x17000752 RID: 1874
		// (get) Token: 0x06001B4E RID: 6990 RVA: 0x0004DA45 File Offset: 0x0004CA45
		public short PositionOffset
		{
			get
			{
				return this.m_Position.PositionOffset;
			}
		}

		// Token: 0x17000753 RID: 1875
		// (get) Token: 0x06001B4F RID: 6991 RVA: 0x0004DA52 File Offset: 0x0004CA52
		public AccessFlag Access
		{
			get
			{
				return this.m_access;
			}
		}

		// Token: 0x06001B50 RID: 6992 RVA: 0x0004DA5A File Offset: 0x0004CA5A
		public bool GetAccessFlag(AccessFlag acc)
		{
			return (this.m_access & acc) == acc;
		}

		// Token: 0x06001B51 RID: 6993 RVA: 0x0004DA68 File Offset: 0x0004CA68
		public override bool Equals(object obj)
		{
			CodePosition codePosition = obj as CodePosition;
			return codePosition != null && codePosition.Access == this.m_access && codePosition.Position.Equals(this.m_Position);
		}

		// Token: 0x06001B52 RID: 6994 RVA: 0x0004DAA4 File Offset: 0x0004CAA4
		public override int GetHashCode()
		{
			int num = (this.m_Position != null) ? this.m_Position.GetHashCode() : 0;
			int access = (int)this.m_access;
			int num2 = 449;
			return num * num2 ^ access;
		}

		// Token: 0x040005BC RID: 1468
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("AccessFlag")]
		[StorageVersion("3.3.0.0")]
		private AccessFlag m_access;

		// Token: 0x040005BD RID: 1469
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_Position;
	}
}
