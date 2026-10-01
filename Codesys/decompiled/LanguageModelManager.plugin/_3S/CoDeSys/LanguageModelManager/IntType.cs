using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017A RID: 378
	[TypeGuid("{2de3951e-05c3-42d9-a8c2-0f9e6cd2c60e}")]
	[StorageVersion("3.3.0.0")]
	public class IntType : IECType, _IIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C89 RID: 7305 RVA: 0x0004F829 File Offset: 0x0004E829
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Int);
		}

		// Token: 0x17000797 RID: 1943
		// (get) Token: 0x06001C8A RID: 7306 RVA: 0x0004F832 File Offset: 0x0004E832
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Int;
			}
		}

		// Token: 0x06001C8B RID: 7307 RVA: 0x0004F835 File Offset: 0x0004E835
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C8C RID: 7308 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001C8D RID: 7309 RVA: 0x0004F840 File Offset: 0x0004E840
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			short num = 0;
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for a INT value");
			}
			for (int i = 0; i < 2; i++)
			{
				int num2 = (int)raw[i] << 8 * i;
				num = (short)((ushort)num | (ushort)num2);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001C8E RID: 7310 RVA: 0x0004F894 File Offset: 0x0004E894
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

		// Token: 0x06001C8F RID: 7311 RVA: 0x0004F8C8 File Offset: 0x0004E8C8
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			short num = checked((short)TypeHelper.GetNumericValue(value, scope));
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
