using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000183 RID: 387
	[TypeGuid("{D81AE1B4-24B7-48E2-A115-05DF1B05ECDC}")]
	[StorageVersion("3.5.0.0")]
	public class UXIntType : IECType, _IUXIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CC5 RID: 7365 RVA: 0x0004FEFA File Offset: 0x0004EEFA
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.__UXInt);
		}

		// Token: 0x1700079E RID: 1950
		// (get) Token: 0x06001CC6 RID: 7366 RVA: 0x0004FF06 File Offset: 0x0004EF06
		public override TypeClass Class
		{
			get
			{
				return TypeClass.UXInt;
			}
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x0004FF0A File Offset: 0x0004EF0A
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CC9 RID: 7369 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override object ConvertRaw(byte[] raw, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x00004E6B File Offset: 0x00003E6B
		public override bool CanConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return false;
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x00005F0F File Offset: 0x00004F0F
		public override byte[] ConvertToRaw(object value, ByteOrder byteOrder, IScope5 scope)
		{
			return null;
		}
	}
}
