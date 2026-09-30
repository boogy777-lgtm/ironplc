using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000178 RID: 376
	[TypeGuid("{1a5bd4e4-ab1d-4514-adeb-22e2eeddcce6}")]
	[StorageVersion("3.3.0.0")]
	public class LWordType : IECType, _ILWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C79 RID: 7289 RVA: 0x0004F5EE File Offset: 0x0004E5EE
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LWord);
		}

		// Token: 0x17000795 RID: 1941
		// (get) Token: 0x06001C7A RID: 7290 RVA: 0x0004F5F7 File Offset: 0x0004E5F7
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LWord;
			}
		}

		// Token: 0x06001C7B RID: 7291 RVA: 0x0004F5FA File Offset: 0x0004E5FA
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x0004F60C File Offset: 0x0004E60C
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a lword value");
			}
			ulong num = 0UL;
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

		// Token: 0x06001C7E RID: 7294 RVA: 0x0004F660 File Offset: 0x0004E660
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

		// Token: 0x06001C7F RID: 7295 RVA: 0x0004F694 File Offset: 0x0004E694
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
