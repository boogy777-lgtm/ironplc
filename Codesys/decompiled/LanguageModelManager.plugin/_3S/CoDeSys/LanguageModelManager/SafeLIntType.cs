using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000169 RID: 361
	[TypeGuid("{5CB228B7-B9AF-466d-BCB9-1219651BB842}")]
	[StorageVersion("3.4.1.0")]
	public class SafeLIntType : LIntType, _ISafeLIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C2A RID: 7210 RVA: 0x0004EE36 File Offset: 0x0004DE36
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeLInt);
		}
	}
}
