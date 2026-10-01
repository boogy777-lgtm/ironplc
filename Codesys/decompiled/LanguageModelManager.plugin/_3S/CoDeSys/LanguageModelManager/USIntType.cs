using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200017D RID: 381
	[TypeGuid("{8805e1b7-ec8d-4bf0-9194-dcb2b29ff03f}")]
	[StorageVersion("3.3.0.0")]
	public class USIntType : IECType, _IUSIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CA1 RID: 7329 RVA: 0x0004FBF1 File Offset: 0x0004EBF1
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.USInt);
		}

		// Token: 0x1700079A RID: 1946
		// (get) Token: 0x06001CA2 RID: 7330 RVA: 0x00010208 File Offset: 0x0000F208
		public override TypeClass Class
		{
			get
			{
				return TypeClass.USInt;
			}
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x0004FBFA File Offset: 0x0004EBFA
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CA4 RID: 7332 RVA: 0x0004EFCA File Offset: 0x0004DFCA
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 1;
		}

		// Token: 0x06001CA5 RID: 7333 RVA: 0x0004FC03 File Offset: 0x0004EC03
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 1)
			{
				throw new InvalidCastException("Invalid length for a USINT value");
			}
			return raw[0];
		}

		// Token: 0x06001CA6 RID: 7334 RVA: 0x0004FC20 File Offset: 0x0004EC20
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

		// Token: 0x06001CA7 RID: 7335 RVA: 0x0004FC54 File Offset: 0x0004EC54
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte b = checked((byte)TypeHelper.GetNumericValue(value, scope));
			return new byte[]
			{
				b
			};
		}
	}
}
