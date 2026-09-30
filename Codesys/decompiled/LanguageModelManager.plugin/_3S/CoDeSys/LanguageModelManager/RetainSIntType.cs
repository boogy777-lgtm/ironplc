using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000174 RID: 372
	[TypeGuid("{9023C61A-5C24-4bb1-BC8D-C30CB0803B37}")]
	[StorageVersion("3.3.1.0")]
	public class RetainSIntType : SIntType, _IRetainSIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISpecialSizeType
	{
		// Token: 0x1700078F RID: 1935
		// (get) Token: 0x06001C59 RID: 7257 RVA: 0x0004F26A File Offset: 0x0004E26A
		public int CompatibilitySize
		{
			get
			{
				return TypeTable.GetSize(TypeClass.SInt, null);
			}
		}

		// Token: 0x17000790 RID: 1936
		// (get) Token: 0x06001C5A RID: 7258 RVA: 0x0004F273 File Offset: 0x0004E273
		public ICompiledType CodegeneratorType
		{
			get
			{
				return TypeTable.Int;
			}
		}

		// Token: 0x06001C5B RID: 7259 RVA: 0x0004F27A File Offset: 0x0004E27A
		public override int Size(IScope scope)
		{
			return TypeTable.GetSize(TypeClass.Int, scope);
		}

		// Token: 0x06001C5C RID: 7260 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001C5D RID: 7261 RVA: 0x0004F283 File Offset: 0x0004E283
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for a SINT value");
			}
			if (byteOrder == ByteOrder.Intel)
			{
				return (sbyte)raw[0];
			}
			return (sbyte)raw[1];
		}

		// Token: 0x06001C5E RID: 7262 RVA: 0x0004F2AC File Offset: 0x0004E2AC
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

		// Token: 0x06001C5F RID: 7263 RVA: 0x0004F2E0 File Offset: 0x0004E2E0
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			Int16Union int16Union = default(Int16Union);
			int16Union.m_short = checked((short)TypeHelper.GetNumericValue(value, scope));
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
