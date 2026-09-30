using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020001AD RID: 429
	[TypeGuid("{5ca6504d-a8ee-43a5-b5be-9928ae1dd51a}")]
	[StorageVersion("3.3.0.0")]
	public class LazyType : IECType, _ILazyType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001ED5 RID: 7893 RVA: 0x00054DD0 File Offset: 0x00053DD0
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.__Lazy);
		}

		// Token: 0x1700080A RID: 2058
		// (get) Token: 0x06001ED6 RID: 7894 RVA: 0x00054DD8 File Offset: 0x00053DD8
		public override TypeClass Class
		{
			get
			{
				return TypeClass.Lazy;
			}
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x00054DDC File Offset: 0x00053DDC
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x00054DE5 File Offset: 0x00053DE5
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("__Lazy.ConvertRaw will never be implemented");
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x00054DF1 File Offset: 0x00053DF1
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			throw new NotImplementedException("AnyRealType.ConvertToRaw not implemented yet");
		}
	}
}
