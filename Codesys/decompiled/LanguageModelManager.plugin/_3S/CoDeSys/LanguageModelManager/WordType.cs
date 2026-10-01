using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000176 RID: 374
	[TypeGuid("{d3df9a8e-760e-4747-9783-699d53cec752}")]
	[StorageVersion("3.3.0.0")]
	public class WordType : IECType, _IWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C69 RID: 7273 RVA: 0x0004F3E2 File Offset: 0x0004E3E2
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Word);
		}

		// Token: 0x17000793 RID: 1939
		// (get) Token: 0x06001C6A RID: 7274 RVA: 0x0004F3EB File Offset: 0x0004E3EB
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Word;
			}
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x0004F3EE File Offset: 0x0004E3EE
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C6C RID: 7276 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001C6D RID: 7277 RVA: 0x0004F3F8 File Offset: 0x0004E3F8
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for word value");
			}
			ushort num = (ushort)((int)raw[0] | (int)raw[1] << 8);
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001C6E RID: 7278 RVA: 0x0004F438 File Offset: 0x0004E438
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

		// Token: 0x06001C6F RID: 7279 RVA: 0x0004F46C File Offset: 0x0004E46C
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			ushort num = checked((ushort)TypeHelper.GetUnsignedNumericValue(value, scope));
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					(byte)(num & 255),
					(byte)(num >> 8 & 255)
				};
			}
			return new byte[]
			{
				(byte)(num >> 8 & 255),
				(byte)(num & 255)
			};
		}
	}
}
