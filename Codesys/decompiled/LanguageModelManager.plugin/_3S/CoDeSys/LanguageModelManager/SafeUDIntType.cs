using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000167 RID: 359
	[TypeGuid("{A2212F9A-09DE-4287-A118-814E0E072DF4}")]
	[StorageVersion("3.4.1.0")]
	public class SafeUDIntType : UDIntType, _ISafeUDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C26 RID: 7206 RVA: 0x0004EE0E File Offset: 0x0004DE0E
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeUDInt);
		}
	}
}
