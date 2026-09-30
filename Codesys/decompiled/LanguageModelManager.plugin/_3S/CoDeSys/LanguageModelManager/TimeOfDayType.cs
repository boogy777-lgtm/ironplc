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
	// Token: 0x02000195 RID: 405
	[TypeGuid("{cf26b7be-0f44-4dda-af18-2657035f6b45}")]
	[StorageVersion("3.3.0.0")]
	public class TimeOfDayType : IECType, _ITimeOfDayType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D58 RID: 7512 RVA: 0x00050FAB File Offset: 0x0004FFAB
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.TimeOfDay);
		}

		// Token: 0x170007B4 RID: 1972
		// (get) Token: 0x06001D59 RID: 7513 RVA: 0x00050FB4 File Offset: 0x0004FFB4
		public override TypeClass Class
		{
			get
			{
				return TypeClass.TimeOfDay;
			}
		}

		// Token: 0x06001D5A RID: 7514 RVA: 0x00050FB8 File Offset: 0x0004FFB8
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x00050FC4 File Offset: 0x0004FFC4
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = 0U;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a TIME_OF_DAY value");
			}
			for (int i = 0; i < 4; i++)
			{
				num |= (uint)((uint)raw[i] << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				BitHelper.Swap(ref num);
			}
			DateTime dateTime = new DateTime(0L);
			dateTime = dateTime.Add(new TimeSpan((long)((ulong)num * 1000UL * 10UL)));
			return dateTime;
		}

		// Token: 0x06001D5D RID: 7517 RVA: 0x00051034 File Offset: 0x00050034
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

		// Token: 0x06001D5E RID: 7518 RVA: 0x00051068 File Offset: 0x00050068
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] bytes = BitConverter.GetBytes((int)(((DateTime)value).Subtract(new DateTime(0L)).Ticks / 10000L));
			Debug.Assert(bytes.Length == 4);
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
