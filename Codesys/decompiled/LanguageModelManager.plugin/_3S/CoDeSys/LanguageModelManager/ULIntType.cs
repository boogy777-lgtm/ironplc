using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000189 RID: 393
	[TypeGuid("{ef19db71-9ad6-4d2a-9535-1a0f103c369d}")]
	[StorageVersion("3.3.0.0")]
	public class ULIntType : IECType, _IULIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CDD RID: 7389 RVA: 0x0004FF6F File Offset: 0x0004EF6F
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.ULInt);
		}

		// Token: 0x170007A0 RID: 1952
		// (get) Token: 0x06001CDE RID: 7390 RVA: 0x0004FF78 File Offset: 0x0004EF78
		public override TypeClass Class
		{
			get
			{
				return TypeClass.ULInt;
			}
		}

		// Token: 0x06001CDF RID: 7391 RVA: 0x0004FF7C File Offset: 0x0004EF7C
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x0004FF88 File Offset: 0x0004EF88
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ulong num = 0UL;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LINT value");
			}
			for (int i = 0; i < 8; i++)
			{
				num |= (ulong)raw[i] << 8 * i;
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0004FFDC File Offset: 0x0004EFDC
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

		// Token: 0x06001CE3 RID: 7395 RVA: 0x00050010 File Offset: 0x0004F010
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			ulong unsignedNumericValue = TypeHelper.GetUnsignedNumericValue(value, scope);
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					(byte)(unsignedNumericValue & 255UL),
					(byte)(unsignedNumericValue >> 8 & 255UL),
					(byte)(unsignedNumericValue >> 16 & 255UL),
					(byte)(unsignedNumericValue >> 24 & 255UL),
					(byte)(unsignedNumericValue >> 32 & 255UL),
					(byte)(unsignedNumericValue >> 40 & 255UL),
					(byte)(unsignedNumericValue >> 48 & 255UL),
					(byte)(unsignedNumericValue >> 56 & 255UL)
				};
			}
			return new byte[]
			{
				(byte)(unsignedNumericValue >> 56 & 255UL),
				(byte)(unsignedNumericValue >> 48 & 255UL),
				(byte)(unsignedNumericValue >> 40 & 255UL),
				(byte)(unsignedNumericValue >> 32 & 255UL),
				(byte)(unsignedNumericValue >> 24 & 255UL),
				(byte)(unsignedNumericValue >> 16 & 255UL),
				(byte)(unsignedNumericValue >> 8 & 255UL),
				(byte)(unsignedNumericValue & 255UL)
			};
		}
	}
}
