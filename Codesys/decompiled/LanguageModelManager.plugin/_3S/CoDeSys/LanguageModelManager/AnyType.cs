using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A6 RID: 422
	[TypeGuid("{542741e8-6286-44d0-b8a7-7787ef87430a}")]
	[StorageVersion("3.3.0.0")]
	public class AnyType : IECType, _IAnyType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EA5 RID: 7845 RVA: 0x00054BFA File Offset: 0x00053BFA
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.Any);
		}

		// Token: 0x17000803 RID: 2051
		// (get) Token: 0x06001EA6 RID: 7846 RVA: 0x00054C02 File Offset: 0x00053C02
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Any;
			}
		}

		// Token: 0x06001EA7 RID: 7847 RVA: 0x00054C06 File Offset: 0x00053C06
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EA8 RID: 7848 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EA9 RID: 7849 RVA: 0x00054C0F File Offset: 0x00053C0F
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EAA RID: 7850 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x00054C1B File Offset: 0x00053C1B
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyType.ConvertToRaw not implemented yet");
		}
	}
}
