using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000170 RID: 368
	[TypeGuid("{bad068f5-d75d-4104-a3db-b92ab7fd8e73}")]
	[StorageVersion("3.3.0.0")]
	public class ByteType : IECType, _IByteType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001C40 RID: 7232 RVA: 0x0004EFB5 File Offset: 0x0004DFB5
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Byte);
		}

		// Token: 0x1700078A RID: 1930
		// (get) Token: 0x06001C41 RID: 7233 RVA: 0x0004EFBE File Offset: 0x0004DFBE
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Byte;
			}
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x0004EFC1 File Offset: 0x0004DFC1
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x0004EFCA File Offset: 0x0004DFCA
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return raw.Length == 1;
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x0004EFD2 File Offset: 0x0004DFD2
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			if (raw.Length != 1)
			{
				throw new InvalidCastException("Invalid length for byte value");
			}
			return raw[0];
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x0004EFF0 File Offset: 0x0004DFF0
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

		// Token: 0x06001C46 RID: 7238 RVA: 0x0004F024 File Offset: 0x0004E024
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			byte b = checked((byte)TypeHelper.GetUnsignedNumericValue(value, scope));
			return new byte[]
			{
				b
			};
		}
	}
}
