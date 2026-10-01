using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016A RID: 362
	[TypeGuid("{5EF0AB70-3CDA-4a6d-86BC-B846CA6E9ABA}")]
	[StorageVersion("3.4.1.0")]
	public class SafeULIntType : ULIntType, _ISafeULIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C2C RID: 7212 RVA: 0x0004EE4A File Offset: 0x0004DE4A
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeULInt);
		}
	}
}
