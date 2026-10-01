using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016D RID: 365
	[TypeGuid("{EA9439E1-4CC1-46DE-B605-C1016904D1F1}")]
	[StorageVersion("3.5.16.0")]
	public class SafeLRealType : LRealType, _ISafeLRealType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C32 RID: 7218 RVA: 0x0004EE86 File Offset: 0x0004DE86
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeLReal);
		}
	}
}
