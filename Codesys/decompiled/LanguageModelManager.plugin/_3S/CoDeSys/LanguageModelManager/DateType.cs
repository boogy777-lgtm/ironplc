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
	// Token: 0x02000191 RID: 401
	[TypeGuid("{80dc2303-8251-4747-b7f7-a402ed859801}")]
	[StorageVersion("3.3.0.0")]
	public class DateType : IECType, _IDateType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D38 RID: 7480 RVA: 0x00050BD4 File Offset: 0x0004FBD4
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Date);
		}

		// Token: 0x170007B0 RID: 1968
		// (get) Token: 0x06001D39 RID: 7481 RVA: 0x00050BDD File Offset: 0x0004FBDD
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Date;
			}
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x00050BE1 File Offset: 0x0004FBE1
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001D3C RID: 7484 RVA: 0x00050BEC File Offset: 0x0004FBEC
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = 0U;
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a DATE value");
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

		// Token: 0x06001D3D RID: 7485 RVA: 0x00050C68 File Offset: 0x0004FC68
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

		// Token: 0x06001D3E RID: 7486 RVA: 0x00050C9C File Offset: 0x0004FC9C
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
