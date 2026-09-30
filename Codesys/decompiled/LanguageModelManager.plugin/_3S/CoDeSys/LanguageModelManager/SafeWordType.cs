using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000162 RID: 354
	[TypeGuid("{984EFF49-02B9-4abf-A6B3-10B90338F8EC}")]
	[StorageVersion("3.4.1.0")]
	public class SafeWordType : WordType, _ISafeWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C1C RID: 7196 RVA: 0x0004EDAA File Offset: 0x0004DDAA
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeWord);
		}
	}
}
