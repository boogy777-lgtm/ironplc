using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000132 RID: 306
	[TypeGuid("{fc62c228-7004-44c0-b1b8-b0d168ccb4ca}")]
	[StorageVersion("3.3.0.0")]
	[StructLayout(LayoutKind.Explicit)]
	public struct IntegerUnion : IArchivable2, IArchivable
	{
		// Token: 0x06001A93 RID: 6803 RVA: 0x0004BE51 File Offset: 0x0004AE51
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Value", this.m_long);
		}

		// Token: 0x06001A94 RID: 6804 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void BeforeSerialize()
		{
		}

		// Token: 0x06001A95 RID: 6805 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void AfterDeserialize()
		{
		}

		// Token: 0x17000712 RID: 1810
		// (get) Token: 0x06001A96 RID: 6806 RVA: 0x0004BE64 File Offset: 0x0004AE64
		public string[] SerializableValueNames
		{
			get
			{
				return new string[]
				{
					"Value"
				};
			}
		}

		// Token: 0x06001A97 RID: 6807 RVA: 0x0004BE74 File Offset: 0x0004AE74
		public object GetSerializableValue(string stValueName)
		{
			if (stValueName == "Value")
			{
				return this.m_long;
			}
			return null;
		}

		// Token: 0x06001A98 RID: 6808 RVA: 0x0004BE90 File Offset: 0x0004AE90
		public void SetSerializableValue(string stValueName, object value)
		{
			if (stValueName == "Value")
			{
				this.m_long = (long)value;
			}
		}

		// Token: 0x06001A99 RID: 6809 RVA: 0x0004BEAC File Offset: 0x0004AEAC
		internal void GetBytes(byte[] byteArray)
		{
			if (byteArray.Length != 0)
			{
				byteArray[0] = this.m_byte0;
			}
			if (byteArray.Length > 1)
			{
				byteArray[1] = this.m_byte1;
			}
			if (byteArray.Length > 2)
			{
				byteArray[2] = this.m_byte2;
			}
			if (byteArray.Length > 3)
			{
				byteArray[3] = this.m_byte3;
			}
			if (byteArray.Length > 4)
			{
				byteArray[4] = this.m_byte4;
			}
			if (byteArray.Length > 5)
			{
				byteArray[5] = this.m_byte5;
			}
			if (byteArray.Length > 6)
			{
				byteArray[6] = this.m_byte6;
			}
			if (byteArray.Length > 7)
			{
				byteArray[7] = this.m_byte7;
			}
		}

		// Token: 0x06001A9A RID: 6810 RVA: 0x0004BF30 File Offset: 0x0004AF30
		internal void SetBytes(byte[] byteArray)
		{
			if (byteArray.Length != 0)
			{
				this.m_byte0 = byteArray[0];
			}
			if (byteArray.Length > 1)
			{
				this.m_byte1 = byteArray[1];
			}
			if (byteArray.Length > 2)
			{
				this.m_byte2 = byteArray[2];
			}
			if (byteArray.Length > 3)
			{
				this.m_byte3 = byteArray[3];
			}
			if (byteArray.Length > 4)
			{
				this.m_byte4 = byteArray[4];
			}
			if (byteArray.Length > 5)
			{
				this.m_byte5 = byteArray[5];
			}
			if (byteArray.Length > 6)
			{
				this.m_byte6 = byteArray[6];
			}
			if (byteArray.Length > 7)
			{
				this.m_byte7 = byteArray[7];
			}
		}

		// Token: 0x06001A9B RID: 6811 RVA: 0x0004BFB3 File Offset: 0x0004AFB3
		public string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return this.SerializableValueNames;
		}

		// Token: 0x06001A9C RID: 6812 RVA: 0x0004BFBB File Offset: 0x0004AFBB
		public object GetSerializableValue(string stValueName, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return this.GetSerializableValue(stValueName);
		}

		// Token: 0x04000575 RID: 1397
		[DefaultSerialization("long")]
		[StorageVersion("3.3.0.0")]
		[FieldOffset(0)]
		public long m_long;

		// Token: 0x04000576 RID: 1398
		[FieldOffset(0)]
		public ulong m_ulong;

		// Token: 0x04000577 RID: 1399
		[FieldOffset(0)]
		public int m_int0;

		// Token: 0x04000578 RID: 1400
		[FieldOffset(4)]
		public int m_int1;

		// Token: 0x04000579 RID: 1401
		[FieldOffset(0)]
		public uint m_uint0;

		// Token: 0x0400057A RID: 1402
		[FieldOffset(4)]
		public uint m_uint1;

		// Token: 0x0400057B RID: 1403
		[FieldOffset(0)]
		public short m_short0;

		// Token: 0x0400057C RID: 1404
		[FieldOffset(2)]
		public short m_short1;

		// Token: 0x0400057D RID: 1405
		[FieldOffset(4)]
		public short m_short2;

		// Token: 0x0400057E RID: 1406
		[FieldOffset(6)]
		public short m_short3;

		// Token: 0x0400057F RID: 1407
		[FieldOffset(0)]
		public ushort m_ushort0;

		// Token: 0x04000580 RID: 1408
		[FieldOffset(2)]
		public ushort m_ushort1;

		// Token: 0x04000581 RID: 1409
		[FieldOffset(4)]
		public ushort m_ushort2;

		// Token: 0x04000582 RID: 1410
		[FieldOffset(6)]
		public ushort m_ushort3;

		// Token: 0x04000583 RID: 1411
		[FieldOffset(0)]
		public byte m_byte0;

		// Token: 0x04000584 RID: 1412
		[FieldOffset(1)]
		public byte m_byte1;

		// Token: 0x04000585 RID: 1413
		[FieldOffset(2)]
		public byte m_byte2;

		// Token: 0x04000586 RID: 1414
		[FieldOffset(3)]
		public byte m_byte3;

		// Token: 0x04000587 RID: 1415
		[FieldOffset(4)]
		public byte m_byte4;

		// Token: 0x04000588 RID: 1416
		[FieldOffset(5)]
		public byte m_byte5;

		// Token: 0x04000589 RID: 1417
		[FieldOffset(6)]
		public byte m_byte6;

		// Token: 0x0400058A RID: 1418
		[FieldOffset(7)]
		public byte m_byte7;
	}
}
