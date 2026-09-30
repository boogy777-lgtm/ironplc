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
	// Token: 0x0200018F RID: 399
	[TypeGuid("{2c47afd5-45a7-4318-a91c-e6ccae571e57}")]
	[StorageVersion("3.3.0.0")]
	public class TimeType : IECType, _ITimeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D28 RID: 7464 RVA: 0x00050A43 File Offset: 0x0004FA43
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Time);
		}

		// Token: 0x170007AE RID: 1966
		// (get) Token: 0x06001D29 RID: 7465 RVA: 0x00050A4C File Offset: 0x0004FA4C
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Time;
			}
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x00050A50 File Offset: 0x0004FA50
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001D2C RID: 7468 RVA: 0x00050A5C File Offset: 0x0004FA5C
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = 0U;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a TIME value");
			}
			for (int i = 0; i < 4; i++)
			{
				num |= (uint)((uint)raw[i] << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				BitHelper.Swap(ref num);
			}
			return (long)((ulong)num);
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x00050AA8 File Offset: 0x0004FAA8
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

		// Token: 0x06001D2E RID: 7470 RVA: 0x00050ADC File Offset: 0x0004FADC
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] bytes = BitConverter.GetBytes((int)TypeHelper.GetNumericValue(value, scope));
			Debug.Assert(bytes.Length == 4);
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
