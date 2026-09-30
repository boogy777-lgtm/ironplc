using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000180 RID: 384
	[TypeGuid("{70846456-DF4C-4590-852C-2C68229A68A1}")]
	[StorageVersion("3.5.0.0")]
	public class XWordType : IECType, _IXWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CB9 RID: 7353 RVA: 0x0004FE92 File Offset: 0x0004EE92
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.__XWord);
		}

		// Token: 0x1700079D RID: 1949
		// (get) Token: 0x06001CBA RID: 7354 RVA: 0x0004FE9E File Offset: 0x0004EE9E
		public override TypeClass Class
		{
			get
			{
				return TypeClass.XWord;
			}
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0004FEA2 File Offset: 0x0004EEA2
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}
	}
}
