using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016B RID: 363
	[TypeGuid("{20492A4D-17AB-4b0b-AEF2-0B63D9BAC223}")]
	[StorageVersion("3.4.1.0")]
	public class SafeTimeType : TimeType, _ISafeTimeType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C2E RID: 7214 RVA: 0x0004EE5E File Offset: 0x0004DE5E
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeTime);
		}
	}
}
