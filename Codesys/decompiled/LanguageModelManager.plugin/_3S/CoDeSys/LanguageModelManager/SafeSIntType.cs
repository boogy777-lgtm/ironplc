using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000160 RID: 352
	[TypeGuid("{E4E2F4F6-BFEF-48f1-A1A3-D7F1C90A536D}")]
	[StorageVersion("3.4.1.0")]
	public class SafeSIntType : SIntType, _ISafeSIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C18 RID: 7192 RVA: 0x0004ED82 File Offset: 0x0004DD82
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeSInt);
		}
	}
}
