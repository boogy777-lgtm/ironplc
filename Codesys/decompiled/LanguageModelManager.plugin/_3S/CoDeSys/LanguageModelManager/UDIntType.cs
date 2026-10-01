using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017F RID: 383
	[TypeGuid("{223d196a-c1d7-4283-aea6-270599207584}")]
	[StorageVersion("3.3.0.0")]
	public class UDIntType : IECType, _IUDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CB1 RID: 7345 RVA: 0x0004FD6A File Offset: 0x0004ED6A
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.UDInt);
		}

		// Token: 0x1700079C RID: 1948
		// (get) Token: 0x06001CB2 RID: 7346 RVA: 0x0004FD73 File Offset: 0x0004ED73
		public override TypeClass Class
		{
			get
			{
				return TypeClass.UDInt;
			}
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x0004FD77 File Offset: 0x0004ED77
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x0004FD80 File Offset: 0x0004ED80
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = 0U;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a UDINT value");
			}
			for (int i = 0; i < 4; i++)
			{
				num |= (uint)((uint)raw[i] << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x0004FDD0 File Offset: 0x0004EDD0
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

		// Token: 0x06001CB7 RID: 7351 RVA: 0x0004FE04 File Offset: 0x0004EE04
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = checked((uint)TypeHelper.GetNumericValue(value, scope));
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					(byte)(num & 255U),
					(byte)(num >> 8 & 255U),
					(byte)(num >> 16 & 255U),
					(byte)(num >> 24 & 255U)
				};
			}
			return new byte[]
			{
				(byte)(num >> 24 & 255U),
				(byte)(num >> 16 & 255U),
				(byte)(num >> 8 & 255U),
				(byte)(num & 255U)
			};
		}
	}
}
