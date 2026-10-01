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
	// Token: 0x02000193 RID: 403
	[TypeGuid("{6c1d7ed1-429a-4b0d-84d7-fc2a63a0c431}")]
	[StorageVersion("3.3.0.0")]
	public class DateAndTimeType : IECType, _IDateAndTimeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D48 RID: 7496 RVA: 0x00050DBF File Offset: 0x0004FDBF
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.DateAndTime);
		}

		// Token: 0x170007B2 RID: 1970
		// (get) Token: 0x06001D49 RID: 7497 RVA: 0x00050DC8 File Offset: 0x0004FDC8
		public override TypeClass Class
		{
			get
			{
				return TypeClass.DateAndTime;
			}
		}

		// Token: 0x06001D4A RID: 7498 RVA: 0x00050DCC File Offset: 0x0004FDCC
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D4B RID: 7499 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001D4C RID: 7500 RVA: 0x00050DD8 File Offset: 0x0004FDD8
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = 0U;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a DATE_AND_TIME value");
			}
			for (int i = 0; i < 4; i++)
			{
				num |= (uint)((uint)raw[i] << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				BitHelper.Swap(ref num);
			}
			DateTime dateTime = new DateTime(1970, 1, 1);
			dateTime = dateTime.Add(new TimeSpan((long)((ulong)num * 1000UL * 1000UL * 10UL)));
			return dateTime;
		}

		// Token: 0x06001D4D RID: 7501 RVA: 0x00050E54 File Offset: 0x0004FE54
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

		// Token: 0x06001D4E RID: 7502 RVA: 0x00050E88 File Offset: 0x0004FE88
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] bytes = BitConverter.GetBytes((int)(((DateTime)value).Subtract(new DateTime(1970, 1, 1)).Ticks / 10000000L));
			Debug.Assert(bytes.Length == 4);
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
