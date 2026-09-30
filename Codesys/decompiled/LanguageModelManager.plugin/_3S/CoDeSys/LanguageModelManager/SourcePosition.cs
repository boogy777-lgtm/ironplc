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
	// Token: 0x0200013A RID: 314
	[TypeGuid("{dd880747-d8b1-4503-8be5-8126a9d323f4}")]
	[StorageVersion("3.3.0.0")]
	public class SourcePosition : GenericObject2, _ISourcePosition, ISourcePosition
	{
		// Token: 0x1700071A RID: 1818
		// (get) Token: 0x06001ABC RID: 6844 RVA: 0x0004C371 File Offset: 0x0004B371
		// (set) Token: 0x06001ABD RID: 6845 RVA: 0x0004C37E File Offset: 0x0004B37E
		[DefaultSerialization("ProjectHandle")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public int ProjectHandleToSave
		{
			get
			{
				return this._objId._ProjectHandle;
			}
			set
			{
				if (this._objId == null)
				{
					this._objId = new ObjectIdentification();
				}
				this._objId._ProjectHandle = value;
			}
		}

		// Token: 0x1700071B RID: 1819
		// (get) Token: 0x06001ABE RID: 6846 RVA: 0x0004C39F File Offset: 0x0004B39F
		// (set) Token: 0x06001ABF RID: 6847 RVA: 0x0004C3AC File Offset: 0x0004B3AC
		[DefaultSerialization("Guid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		public Guid ObjectGuidToSave
		{
			get
			{
				return this._objId._guidObject;
			}
			set
			{
				if (this._objId == null)
				{
					this._objId = new ObjectIdentification();
				}
				this._objId._guidObject = value;
			}
		}

		// Token: 0x06001AC0 RID: 6848 RVA: 0x0004C3D0 File Offset: 0x0004B3D0
		public static void ClearHashtable()
		{
			object obj = SourcePosition.lockHashtableObjects;
			lock (obj)
			{
				SourcePosition.s_hashtableObjects.Clear();
			}
		}

		// Token: 0x06001AC1 RID: 6849 RVA: 0x0004C414 File Offset: 0x0004B414
		public SourcePosition(int nProjectHandle, Guid objectGuid, long nPosition, short sPositionOffset, short nLength)
		{
			this._Position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			this._nLength = nLength;
			this._objId = new ObjectIdentification(objectGuid, nProjectHandle);
			ObjectIdentification objId = null;
			object obj = SourcePosition.lockHashtableObjects;
			lock (obj)
			{
				if (SourcePosition.s_hashtableObjects.TryGetValue(this._objId, ref objId))
				{
					this._objId = objId;
				}
				else
				{
					SourcePosition.s_hashtableObjects[this._objId] = this._objId;
				}
			}
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x0004C4AC File Offset: 0x0004B4AC
		public SourcePosition()
		{
			this._objId = null;
			this._Position = MinimalPosition.CreateMinimalPosition(0L, 0);
			this._nLength = 0;
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x0004C4D0 File Offset: 0x0004B4D0
		public IMinimalPosition GetMinimalPosition()
		{
			return this._Position;
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x0004C4D8 File Offset: 0x0004B4D8
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			ObjectIdentification objectIdentification = null;
			object obj = SourcePosition.lockHashtableObjects;
			lock (obj)
			{
				if (SourcePosition.s_hashtableObjects.TryGetValue(this._objId, ref objectIdentification))
				{
					this._objId = SourcePosition.s_hashtableObjects[objectIdentification];
				}
				else
				{
					SourcePosition.s_hashtableObjects[this._objId] = this._objId;
				}
			}
		}

		// Token: 0x1700071C RID: 1820
		// (get) Token: 0x06001AC5 RID: 6853 RVA: 0x0004C558 File Offset: 0x0004B558
		// (set) Token: 0x06001AC6 RID: 6854 RVA: 0x0004C598 File Offset: 0x0004B598
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		private long PositionToSave
		{
			get
			{
				return new IntegerUnion
				{
					m_long = this._Position.EditorPosition,
					m_short3 = this._Position.PositionOffset
				}.m_long;
			}
			set
			{
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this._Position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x1700071D RID: 1821
		// (get) Token: 0x06001AC7 RID: 6855 RVA: 0x0004C5BC File Offset: 0x0004B5BC
		public static SourcePosition Empty
		{
			get
			{
				return new SourcePosition(-1, Guid.Empty, 0L, 0, 0);
			}
		}

		// Token: 0x1700071E RID: 1822
		// (get) Token: 0x06001AC8 RID: 6856 RVA: 0x0004C5CD File Offset: 0x0004B5CD
		public bool IsHidden
		{
			get
			{
				return this._Position.EditorPosition == 0L;
			}
		}

		// Token: 0x06001AC9 RID: 6857 RVA: 0x0004C5E0 File Offset: 0x0004B5E0
		public void SetObjectIdentification(int nHandle, Guid messageGuid)
		{
			this._objId = new ObjectIdentification(messageGuid, nHandle);
			ObjectIdentification objId = null;
			object obj = SourcePosition.lockHashtableObjects;
			lock (obj)
			{
				if (SourcePosition.s_hashtableObjects.TryGetValue(this._objId, ref objId))
				{
					this._objId = objId;
				}
				else
				{
					SourcePosition.s_hashtableObjects[this._objId] = this._objId;
				}
			}
		}

		// Token: 0x1700071F RID: 1823
		// (get) Token: 0x06001ACA RID: 6858 RVA: 0x0004C65C File Offset: 0x0004B65C
		public int ProjectHandle
		{
			get
			{
				if (this._objId == null)
				{
					return -1;
				}
				return this._objId._ProjectHandle;
			}
		}

		// Token: 0x17000720 RID: 1824
		// (get) Token: 0x06001ACB RID: 6859 RVA: 0x0004C673 File Offset: 0x0004B673
		public Guid ObjectGuid
		{
			get
			{
				if (this._objId == null)
				{
					return Guid.Empty;
				}
				return this._objId._guidObject;
			}
		}

		// Token: 0x17000721 RID: 1825
		// (get) Token: 0x06001ACC RID: 6860 RVA: 0x0004C68E File Offset: 0x0004B68E
		public long Position
		{
			get
			{
				return this._Position.EditorPosition;
			}
		}

		// Token: 0x17000722 RID: 1826
		// (get) Token: 0x06001ACD RID: 6861 RVA: 0x0004C69B File Offset: 0x0004B69B
		public short PositionOffset
		{
			get
			{
				return this._Position.PositionOffset;
			}
		}

		// Token: 0x17000723 RID: 1827
		// (get) Token: 0x06001ACE RID: 6862 RVA: 0x0004C6A8 File Offset: 0x0004B6A8
		// (set) Token: 0x06001ACF RID: 6863 RVA: 0x0004C6B0 File Offset: 0x0004B6B0
		public short Length
		{
			get
			{
				return this._nLength;
			}
			set
			{
				this._nLength = value;
			}
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x0004C6BC File Offset: 0x0004B6BC
		public override bool Equals(object obj)
		{
			ISourcePosition sourcePosition = obj as ISourcePosition;
			return sourcePosition != null && (this.ProjectHandle == sourcePosition.ProjectHandle && this.ObjectGuid == sourcePosition.ObjectGuid && this.Position == sourcePosition.Position && this.PositionOffset == sourcePosition.PositionOffset) && this.Length == sourcePosition.Length;
		}

		// Token: 0x06001AD1 RID: 6865 RVA: 0x0004C724 File Offset: 0x0004B724
		public override int GetHashCode()
		{
			IntegerUnion integerUnion = default(IntegerUnion);
			integerUnion.m_long = this._Position.EditorPosition;
			integerUnion.m_short3 = this._Position.PositionOffset;
			return integerUnion.GetHashCode();
		}

		// Token: 0x17000724 RID: 1828
		// (get) Token: 0x06001AD2 RID: 6866 RVA: 0x0004C76C File Offset: 0x0004B76C
		public long PositionCombination
		{
			get
			{
				return new IntegerUnion
				{
					m_long = this._Position.EditorPosition,
					m_short3 = this._Position.PositionOffset
				}.m_long;
			}
		}

		// Token: 0x04000595 RID: 1429
		private static object lockHashtableObjects = new object();

		// Token: 0x04000596 RID: 1430
		private static LDictionary<ObjectIdentification, ObjectIdentification> s_hashtableObjects = new LDictionary<ObjectIdentification, ObjectIdentification>();

		// Token: 0x04000597 RID: 1431
		private const int s_HiddenPosition = 0;

		// Token: 0x04000598 RID: 1432
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private short _nLength;

		// Token: 0x04000599 RID: 1433
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition _Position;

		// Token: 0x0400059A RID: 1434
		[Obfuscation(Feature = "rename")]
		private ObjectIdentification _objId;
	}
}
