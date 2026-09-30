using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017E RID: 382
	[TypeGuid("{af63b19d-1ce0-4d63-a098-6d4c02ca35da}")]
	[StorageVersion("3.3.0.0")]
	public class UIntType : IECType, _IUIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CA9 RID: 7337 RVA: 0x0004FC74 File Offset: 0x0004EC74
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.UInt);
		}

		// Token: 0x1700079B RID: 1947
		// (get) Token: 0x06001CAA RID: 7338 RVA: 0x0004FC7D File Offset: 0x0004EC7D
		public override TypeClass Class
		{
			get
			{
				return TypeClass.UInt;
			}
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x0004FC81 File Offset: 0x0004EC81
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x0004F1C1 File Offset: 0x0004E1C1
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 2;
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x0004FC8C File Offset: 0x0004EC8C
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ushort num = 0;
			if (raw.Length != 2)
			{
				throw new InvalidCastException("Invalid length for a UINT value");
			}
			for (int i = 0; i < 2; i++)
			{
				num |= (ushort)(raw[i] << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x0004FCE0 File Offset: 0x0004ECE0
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

		// Token: 0x06001CAF RID: 7343 RVA: 0x0004FD14 File Offset: 0x0004ED14
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			ushort num = checked((ushort)TypeHelper.GetNumericValue(value, scope));
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
