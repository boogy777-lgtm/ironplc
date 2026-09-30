using System;
using System.Diagnostics;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000196 RID: 406
	[TypeGuid("{1AA80806-D5C0-4FDB-8088-A45838714596}")]
	[StorageVersion("3.5.16.0")]
	public class LTimeOfDayType : IECType, _ILTimeOfDayType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D60 RID: 7520 RVA: 0x000510B6 File Offset: 0x000500B6
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LTimeOfDay);
		}

		// Token: 0x170007B5 RID: 1973
		// (get) Token: 0x06001D61 RID: 7521 RVA: 0x000510C2 File Offset: 0x000500C2
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LTimeOfDay;
			}
		}

		// Token: 0x06001D62 RID: 7522 RVA: 0x000510C6 File Offset: 0x000500C6
		public override void Accept(ITypeVisitor typvis)
		{
			(typvis as ITypeVisitor2).visit(this);
		}

		// Token: 0x06001D63 RID: 7523 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001D64 RID: 7524 RVA: 0x000510D4 File Offset: 0x000500D4
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ulong num = 0UL;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LTIME_OF_DAY value");
			}
			for (int i = 0; i < 8; i++)
			{
				num |= (ulong)raw[i] << 8 * i;
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				BitHelper.Swap(ref num);
			}
			return (long)num;
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x00051120 File Offset: 0x00050120
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

		// Token: 0x06001D66 RID: 7526 RVA: 0x00051154 File Offset: 0x00050154
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] bytes = BitConverter.GetBytes((long)value);
			Debug.Assert(bytes.Length == 8);
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
