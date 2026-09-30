using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000165 RID: 357
	[TypeGuid("{61231FA8-DCE5-492e-A3BE-71A05A475390}")]
	[StorageVersion("3.4.1.0")]
	public class SafeDWordType : DWordType, _ISafeDWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C22 RID: 7202 RVA: 0x0004EDE6 File Offset: 0x0004DDE6
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeDWord);
		}
	}
}
