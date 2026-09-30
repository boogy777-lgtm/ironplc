using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000179 RID: 377
	[TypeGuid("{3b41731c-8281-447e-8300-967d85065431}")]
	[StorageVersion("3.3.0.0")]
	public class SIntType : IECType, _ISIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C81 RID: 7297 RVA: 0x0004F7A1 File Offset: 0x0004E7A1
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SInt);
		}

		// Token: 0x17000796 RID: 1942
		// (get) Token: 0x06001C82 RID: 7298 RVA: 0x0004F7AA File Offset: 0x0004E7AA
		public override TypeClass Class
		{
			get
			{
				return TypeClass.SInt;
			}
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x0004F7AD File Offset: 0x0004E7AD
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C84 RID: 7300 RVA: 0x0004EFCA File Offset: 0x0004DFCA
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 1;
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x0004F7B6 File Offset: 0x0004E7B6
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 1)
			{
				throw new InvalidCastException("Invalid length for a SINT value");
			}
			return (sbyte)raw[0];
		}

		// Token: 0x06001C86 RID: 7302 RVA: 0x0004F7D4 File Offset: 0x0004E7D4
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

		// Token: 0x06001C87 RID: 7303 RVA: 0x0004F808 File Offset: 0x0004E808
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			sbyte b = checked((sbyte)TypeHelper.GetNumericValue(value, scope));
			return new byte[]
			{
				(byte)b
			};
		}
	}
}
