using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AF RID: 431
	[TypeGuid("{238EF524-0DA1-4C40-A80A-7085F1295A55}")]
	[StorageVersion("3.5.8.0")]
	public class AnyStringType : IECType, _IAnyStringType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001EE5 RID: 7909 RVA: 0x00054E1F File Offset: 0x00053E1F
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.AnyString);
		}

		// Token: 0x1700080C RID: 2060
		// (get) Token: 0x06001EE6 RID: 7910 RVA: 0x00054E2B File Offset: 0x00053E2B
		public override TypeClass Class
		{
			get
			{
				return TypeClass.AnyString;
			}
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x00054E2F File Offset: 0x00053E2F
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001EE8 RID: 7912 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EE9 RID: 7913 RVA: 0x00054E38 File Offset: 0x00053E38
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyStringType.ConvertRaw not implemented yet");
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x00054E44 File Offset: 0x00053E44
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyStringType.ConvertToRaw not implemented yet");
		}
	}
}
