using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000186 RID: 390
	[TypeGuid("{C23D607D-555B-48EE-A8DE-545D45246583}")]
	[StorageVersion("3.5.4.30")]
	public class XIntType : IECType, _IXIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CD1 RID: 7377 RVA: 0x0004FF25 File Offset: 0x0004EF25
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.__XInt);
		}

		// Token: 0x1700079F RID: 1951
		// (get) Token: 0x06001CD2 RID: 7378 RVA: 0x0004FF31 File Offset: 0x0004EF31
		public override TypeClass Class
		{
			get
			{
				return TypeClass.XInt;
			}
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0004FF35 File Offset: 0x0004EF35
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CD4 RID: 7380 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}
	}
}
