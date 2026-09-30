using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000173 RID: 371
	[TypeGuid("{4C62DBF4-9E80-4d9a-861D-29CD433569AC}")]
	[StorageVersion("3.3.1.0")]
	public class RetainByteType : ByteType, _IRetainByteType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISpecialSizeType
	{
		// Token: 0x1700078D RID: 1933
		// (get) Token: 0x06001C51 RID: 7249 RVA: 0x0004F1B8 File Offset: 0x0004E1B8
		public int CompatibilitySize
		{
			get
			{
				return TypeTable.GetSize(TypeClass.Byte, null);
			}
		}

		// Token: 0x1700078E RID: 1934
		// (get) Token: 0x06001C52 RID: 7250 RVA: 0x0004F04D File Offset: 0x0004E04D
		public ICompiledType CodegeneratorType
		{
			get
			{
				return TypeTable.Word;
			}
		}

		// Token: 0x06001C53 RID: 7251 RVA: 0x0004F054 File Offset: 0x0004E054
		public override int Size(IScope scope)
		{
			return TypeTable.GetSize(TypeClass.Word, scope);
		}

		// Token: 0x06001C54 RID: 7252 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x0004F1C9 File Offset: 0x0004E1C9
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for byte value");
			}
			if (byteOrder == ByteOrder.Intel)
			{
				return raw[0];
			}
			return raw[1];
		}

		// Token: 0x06001C56 RID: 7254 RVA: 0x0004F1F0 File Offset: 0x0004E1F0
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			bool result;
			try
			{
				this.ConvertToRaw(value, byteOrder, scope);
				result = true;
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06001C57 RID: 7255 RVA: 0x0004F224 File Offset: 0x0004E224
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			Int16Union int16Union = default(Int16Union);
			int16Union.m_ushort = checked((ushort)TypeHelper.GetNumericValue(value, scope));
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					int16Union.m_byte0
				};
			}
			return new byte[]
			{
				int16Union.m_byte1
			};
		}
	}
}
