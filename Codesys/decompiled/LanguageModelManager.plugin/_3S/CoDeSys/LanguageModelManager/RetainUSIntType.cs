using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000175 RID: 373
	[TypeGuid("{77BD2429-5F54-4fc4-8EC2-0C462C219F22}")]
	[StorageVersion("3.3.1.0")]
	public class RetainUSIntType : USIntType, _IRetainUSIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISpecialSizeType
	{
		// Token: 0x17000791 RID: 1937
		// (get) Token: 0x06001C61 RID: 7265 RVA: 0x0004F326 File Offset: 0x0004E326
		public int CompatibilitySize
		{
			get
			{
				return TypeTable.GetSize(TypeClass.USInt, null);
			}
		}

		// Token: 0x17000792 RID: 1938
		// (get) Token: 0x06001C62 RID: 7266 RVA: 0x0004F330 File Offset: 0x0004E330
		public ICompiledType CodegeneratorType
		{
			get
			{
				return TypeTable.UInt;
			}
		}

		// Token: 0x06001C63 RID: 7267 RVA: 0x0004F337 File Offset: 0x0004E337
		public override int Size(IScope scope)
		{
			return TypeTable.GetSize(TypeClass.UInt, scope);
		}

		// Token: 0x06001C64 RID: 7268 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001C65 RID: 7269 RVA: 0x0004F341 File Offset: 0x0004E341
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for a USINT value");
			}
			if (byteOrder == ByteOrder.Intel)
			{
				return raw[0];
			}
			return raw[1];
		}

		// Token: 0x06001C66 RID: 7270 RVA: 0x0004F368 File Offset: 0x0004E368
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

		// Token: 0x06001C67 RID: 7271 RVA: 0x0004F39C File Offset: 0x0004E39C
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
