using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016C RID: 364
	[TypeGuid("{C265D12C-1803-4346-9C30-B951F1566820}")]
	[StorageVersion("3.5.16.0")]
	public class SafeRealType : RealType, _ISafeRealType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C30 RID: 7216 RVA: 0x0004EE72 File Offset: 0x0004DE72
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeReal);
		}
	}
}
