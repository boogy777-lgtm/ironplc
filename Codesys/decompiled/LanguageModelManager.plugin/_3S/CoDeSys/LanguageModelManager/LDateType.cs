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
	// Token: 0x02000192 RID: 402
	[TypeGuid("{4E904232-D04E-4BF4-B416-72C07C00804C}")]
	[StorageVersion("3.5.16.0")]
	public class LDateType : IECType, _ILDateType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D40 RID: 7488 RVA: 0x00050CEF File Offset: 0x0004FCEF
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LDate);
		}

		// Token: 0x170007B1 RID: 1969
		// (get) Token: 0x06001D41 RID: 7489 RVA: 0x00050CFB File Offset: 0x0004FCFB
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LDate;
			}
		}

		// Token: 0x06001D42 RID: 7490 RVA: 0x00050CFF File Offset: 0x0004FCFF
		public override void Accept(ITypeVisitor typvis)
		{
			(typvis as ITypeVisitor2).visit(this);
		}

		// Token: 0x06001D43 RID: 7491 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001D44 RID: 7492 RVA: 0x00050D10 File Offset: 0x0004FD10
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ulong num = 0UL;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LDATE value");
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

		// Token: 0x06001D45 RID: 7493 RVA: 0x00050D5C File Offset: 0x0004FD5C
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

		// Token: 0x06001D46 RID: 7494 RVA: 0x00050D90 File Offset: 0x0004FD90
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
