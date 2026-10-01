using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AC RID: 428
	[TypeGuid("{dcf8f7b2-30b4-4360-b7b6-a839c56fd061}")]
	[StorageVersion("3.3.0.0")]
	public class AnyNumType : IECType, _IAnyNumType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001ECD RID: 7885 RVA: 0x00054DA3 File Offset: 0x00053DA3
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyNum);
		}

		// Token: 0x17000809 RID: 2057
		// (get) Token: 0x06001ECE RID: 7886 RVA: 0x00054DAB File Offset: 0x00053DAB
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyNum;
			}
		}

		// Token: 0x06001ECF RID: 7887 RVA: 0x00054DAF File Offset: 0x00053DAF
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001ED0 RID: 7888 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x00054DB8 File Offset: 0x00053DB8
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyNumType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001ED3 RID: 7891 RVA: 0x00054DC4 File Offset: 0x00053DC4
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyNumType.ConvertToRaw not implemented yet");
		}
	}
}
