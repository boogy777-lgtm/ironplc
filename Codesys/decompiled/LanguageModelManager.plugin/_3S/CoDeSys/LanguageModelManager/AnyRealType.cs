using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AE RID: 430
	[TypeGuid("{7b7be6f6-26a8-4c50-b6b6-65b5ecc565be}")]
	[StorageVersion("3.3.0.0")]
	public class AnyRealType : IECType, _IAnyRealType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EDD RID: 7901 RVA: 0x00054DFD File Offset: 0x00053DFD
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyReal);
		}

		// Token: 0x1700080B RID: 2059
		// (get) Token: 0x06001EDE RID: 7902 RVA: 0x00054E06 File Offset: 0x00053E06
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyReal;
			}
		}

		// Token: 0x06001EDF RID: 7903 RVA: 0x00054E0A File Offset: 0x00053E0A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x00054E13 File Offset: 0x00053E13
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyReal.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x00054DF1 File Offset: 0x00053DF1
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyRealType.ConvertToRaw not implemented yet");
		}
	}
}
