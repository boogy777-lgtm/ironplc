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
	// Token: 0x02000194 RID: 404
	[TypeGuid("{CCE3884F-E865-411B-995E-4431D439EF1A}")]
	[StorageVersion("3.5.16.0")]
	public class LDateAndTimeType : IECType, _ILDateAndTimeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D50 RID: 7504 RVA: 0x00050EDB File Offset: 0x0004FEDB
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LDateAndTime);
		}

		// Token: 0x170007B3 RID: 1971
		// (get) Token: 0x06001D51 RID: 7505 RVA: 0x00050EE7 File Offset: 0x0004FEE7
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LDateAndTime;
			}
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x00050EEB File Offset: 0x0004FEEB
		public override void Accept(ITypeVisitor typvis)
		{
			(typvis as ITypeVisitor2).visit(this);
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x00050EFC File Offset: 0x0004FEFC
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ulong num = 0UL;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LDATE_AND_TIME value");
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

		// Token: 0x06001D55 RID: 7509 RVA: 0x00050F48 File Offset: 0x0004FF48
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

		// Token: 0x06001D56 RID: 7510 RVA: 0x00050F7C File Offset: 0x0004FF7C
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
