using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000166 RID: 358
	[TypeGuid("{E8B04FC2-6D9E-45ae-A8B8-366DDD1843A3}")]
	[StorageVersion("3.4.1.0")]
	public class SafeDIntType : DIntType, _ISafeDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C24 RID: 7204 RVA: 0x0004EDFA File Offset: 0x0004DDFA
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeDInt);
		}
	}
}
