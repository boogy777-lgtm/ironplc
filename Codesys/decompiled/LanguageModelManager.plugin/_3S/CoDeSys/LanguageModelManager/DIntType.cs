using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017B RID: 379
	[TypeGuid("{d383106b-f62a-4320-9b73-1ad1cdb4256b}")]
	[StorageVersion("3.3.0.0")]
	public class DIntType : IECType, _IDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C91 RID: 7313 RVA: 0x0004F91E File Offset: 0x0004E91E
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.DInt);
		}

		// Token: 0x17000798 RID: 1944
		// (get) Token: 0x06001C92 RID: 7314 RVA: 0x0004F927 File Offset: 0x0004E927
		public override TypeClass Class
		{
			get
			{
				return TypeClass.DInt;
			}
		}

		// Token: 0x06001C93 RID: 7315 RVA: 0x0004F92A File Offset: 0x0004E92A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C94 RID: 7316 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001C95 RID: 7317 RVA: 0x0004F934 File Offset: 0x0004E934
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			int num = 0;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a DINT value");
			}
			for (int i = 0; i < 4; i++)
			{
				num |= (int)raw[i] << 8 * i;
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001C96 RID: 7318 RVA: 0x0004F984 File Offset: 0x0004E984
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

		// Token: 0x06001C97 RID: 7319 RVA: 0x0004F9B8 File Offset: 0x0004E9B8
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			int num = checked((int)TypeHelper.GetNumericValue(value, scope));
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					(byte)(num & 255),
					(byte)(num >> 8 & 255),
					(byte)(num >> 16 & 255),
					(byte)(num >> 24 & 255)
				};
			}
			return new byte[]
			{
				(byte)(num >> 24 & 255),
				(byte)(num >> 16 & 255),
				(byte)(num >> 8 & 255),
				(byte)(num & 255)
			};
		}
	}
}
