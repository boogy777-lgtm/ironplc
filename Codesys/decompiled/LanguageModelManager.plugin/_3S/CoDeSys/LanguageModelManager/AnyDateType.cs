using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001A9 RID: 425
	[TypeGuid("{eb8b442f-272a-4afb-aabe-c0221acf625d}")]
	[StorageVersion("3.3.0.0")]
	public class AnyDateType : IECType, _IAnyDateType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EB6 RID: 7862 RVA: 0x00054C5C File Offset: 0x00053C5C
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyDate);
		}

		// Token: 0x17000805 RID: 2053
		// (get) Token: 0x06001EB7 RID: 7863 RVA: 0x00054C64 File Offset: 0x00053C64
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyDate;
			}
		}

		// Token: 0x06001EB8 RID: 7864 RVA: 0x00054C68 File Offset: 0x00053C68
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EB9 RID: 7865 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EBA RID: 7866 RVA: 0x00054C71 File Offset: 0x00053C71
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyDateType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EBB RID: 7867 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EBC RID: 7868 RVA: 0x00054C7D File Offset: 0x00053C7D
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyDateType.ConvertToRaw not implemented yet");
		}
	}
}
