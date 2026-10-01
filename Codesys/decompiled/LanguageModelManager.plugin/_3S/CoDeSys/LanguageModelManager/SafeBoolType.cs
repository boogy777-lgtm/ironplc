using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200015E RID: 350
	[TypeGuid("{744CC434-A4F8-470b-8FAC-80D0F95074D8}")]
	[StorageVersion("3.4.1.0")]
	public class SafeBoolType : BoolType, _ISafeBoolType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C14 RID: 7188 RVA: 0x0004ED62 File Offset: 0x0004DD62
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeBool);
		}
	}
}
