using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000164 RID: 356
	[TypeGuid("{82D61303-824C-41f0-9DD2-CE2DD37952F5}")]
	[StorageVersion("3.4.1.0")]
	public class SafeUIntType : UIntType, _ISafeUIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C20 RID: 7200 RVA: 0x0004EDD2 File Offset: 0x0004DDD2
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeUInt);
		}
	}
}
