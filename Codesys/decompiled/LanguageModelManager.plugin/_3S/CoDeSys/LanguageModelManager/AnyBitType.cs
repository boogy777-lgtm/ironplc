using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A7 RID: 423
	[TypeGuid("{fb5804fd-7225-4828-84ec-1384ebeb5286}")]
	[StorageVersion("3.3.0.0")]
	public class AnyBitType : IECType, _IAnyBitType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EAD RID: 7853 RVA: 0x00054C27 File Offset: 0x00053C27
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyBit);
		}

		// Token: 0x17000804 RID: 2052
		// (get) Token: 0x06001EAE RID: 7854 RVA: 0x00054C2F File Offset: 0x00053C2F
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyBit;
			}
		}

		// Token: 0x06001EAF RID: 7855 RVA: 0x00054C33 File Offset: 0x00053C33
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EB0 RID: 7856 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EB1 RID: 7857 RVA: 0x00054C3C File Offset: 0x00053C3C
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyBitType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EB2 RID: 7858 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EB3 RID: 7859 RVA: 0x00054C48 File Offset: 0x00053C48
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyBitType.ConvertToRaw not implemented yet");
		}
	}
}
