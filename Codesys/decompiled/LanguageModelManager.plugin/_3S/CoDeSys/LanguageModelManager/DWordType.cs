using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000177 RID: 375
	[TypeGuid("{34951a1b-8cb0-4c0a-8fa0-271c0a67fbd7}")]
	[StorageVersion("3.3.0.0")]
	public class DWordType : IECType, _IDWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C71 RID: 7281 RVA: 0x0004F4C2 File Offset: 0x0004E4C2
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.DWord);
		}

		// Token: 0x17000794 RID: 1940
		// (get) Token: 0x06001C72 RID: 7282 RVA: 0x000331A2 File Offset: 0x000321A2
		public override TypeClass Class
		{
			get
			{
				return TypeClass.DWord;
			}
		}

		// Token: 0x06001C73 RID: 7283 RVA: 0x0004F4CB File Offset: 0x0004E4CB
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C74 RID: 7284 RVA: 0x0004F4D4 File Offset: 0x0004E4D4
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 4;
		}

		// Token: 0x06001C75 RID: 7285 RVA: 0x0004F4DC File Offset: 0x0004E4DC
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 4)
			{
				throw new InvalidCastException("Invalid length for a dword value");
			}
			uint num = 0U;
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

		// Token: 0x06001C76 RID: 7286 RVA: 0x0004F52C File Offset: 0x0004E52C
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

		// Token: 0x06001C77 RID: 7287 RVA: 0x0004F560 File Offset: 0x0004E560
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			uint num = checked((uint)TypeHelper.GetUnsignedNumericValue(value, scope));
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
