using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000161 RID: 353
	[TypeGuid("{F020B93C-18BE-48ef-B33F-BF2E46F8CD01}")]
	[StorageVersion("3.4.1.0")]
	public class SafeUSIntType : USIntType, _ISafeUSIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C1A RID: 7194 RVA: 0x0004ED96 File Offset: 0x0004DD96
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeUSInt);
		}
	}
}
