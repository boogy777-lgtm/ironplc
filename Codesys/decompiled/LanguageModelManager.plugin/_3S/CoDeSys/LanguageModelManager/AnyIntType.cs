using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AA RID: 426
	[TypeGuid("{dc50ac9a-3c18-4aff-84f3-315e9d8644fd}")]
	[StorageVersion("3.3.0.0")]
	public class AnyIntType : IECType, _IAnyIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EBE RID: 7870 RVA: 0x00054C89 File Offset: 0x00053C89
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyInt);
		}

		// Token: 0x17000806 RID: 2054
		// (get) Token: 0x06001EBF RID: 7871 RVA: 0x00054C91 File Offset: 0x00053C91
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyInt;
			}
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x00054C95 File Offset: 0x00053C95
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EC1 RID: 7873 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EC2 RID: 7874 RVA: 0x00054C9E File Offset: 0x00053C9E
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyIntType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EC3 RID: 7875 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EC4 RID: 7876 RVA: 0x00054CAA File Offset: 0x00053CAA
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyIntType.ConvertToRaw not implemented yet");
		}
	}
}
