using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017C RID: 380
	[TypeGuid("{306eb234-9ade-475b-8492-004f59e739d9}")]
	[StorageVersion("3.3.0.0")]
	public class LIntType : IECType, _ILIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C99 RID: 7321 RVA: 0x0004FA46 File Offset: 0x0004EA46
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LInt);
		}

		// Token: 0x17000799 RID: 1945
		// (get) Token: 0x06001C9A RID: 7322 RVA: 0x0004FA4F File Offset: 0x0004EA4F
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LInt;
			}
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0004FA53 File Offset: 0x0004EA53
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x0004FA5C File Offset: 0x0004EA5C
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			long num = 0L;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LINT value");
			}
			for (int i = 0; i < 8; i++)
			{
				num |= (long)((long)((ulong)raw[i]) << 8 * i);
			}
			if (byteOrder == ByteOrder.Motorola)
			{
				return BitHelper.Swap(num);
			}
			return num;
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x0004FAB0 File Offset: 0x0004EAB0
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

		// Token: 0x06001C9F RID: 7327 RVA: 0x0004FAE4 File Offset: 0x0004EAE4
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			long numericValue = TypeHelper.GetNumericValue(value, scope);
			if (byteOrder == ByteOrder.Intel)
			{
				return new byte[]
				{
					(byte)(numericValue & 255L),
					(byte)(numericValue >> 8 & 255L),
					(byte)(numericValue >> 16 & 255L),
					(byte)(numericValue >> 24 & 255L),
					(byte)(numericValue >> 32 & 255L),
					(byte)(numericValue >> 40 & 255L),
					(byte)(numericValue >> 48 & 255L),
					(byte)(numericValue >> 56 & 255L)
				};
			}
			return new byte[]
			{
				(byte)(numericValue >> 56 & 255L),
				(byte)(numericValue >> 48 & 255L),
				(byte)(numericValue >> 40 & 255L),
				(byte)(numericValue >> 32 & 255L),
				(byte)(numericValue >> 24 & 255L),
				(byte)(numericValue >> 16 & 255L),
				(byte)(numericValue >> 8 & 255L),
				(byte)(numericValue & 255L)
			};
		}
	}
}
