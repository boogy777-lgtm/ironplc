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
	// Token: 0x02000190 RID: 400
	[TypeGuid("{8DB06532-D746-4cb4-83D6-15216B159EED}")]
	[StorageVersion("3.3.0.0")]
	public class LTimeType : IECType, _ILTimeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001D30 RID: 7472 RVA: 0x00050B0D File Offset: 0x0004FB0D
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.LTime);
		}

		// Token: 0x170007AF RID: 1967
		// (get) Token: 0x06001D31 RID: 7473 RVA: 0x00050B16 File Offset: 0x0004FB16
		public override TypeClass Class
		{
			get
			{
				return TypeClass.LTime;
			}
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x00050B1A File Offset: 0x0004FB1A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x0004F603 File Offset: 0x0004E603
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 8;
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x00050B24 File Offset: 0x0004FB24
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			ulong num = 0UL;
			if (raw.Length != 8)
			{
				throw new InvalidCastException("Invalid length for a LTIME value");
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

		// Token: 0x06001D35 RID: 7477 RVA: 0x00050B70 File Offset: 0x0004FB70
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

		// Token: 0x06001D36 RID: 7478 RVA: 0x00050BA4 File Offset: 0x0004FBA4
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte[] bytes = BitConverter.GetBytes(TypeHelper.GetNumericValue(value, scope));
			Debug.Assert(bytes.Length == 8);
			if (byteOrder == ByteOrder.Motorola)
			{
				Array.Reverse(bytes);
			}
			return bytes;
		}
	}
}
