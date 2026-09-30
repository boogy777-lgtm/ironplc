using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000163 RID: 355
	[TypeGuid("{BAAB3EB4-190D-4e0e-B286-5D3A6081FCAD}")]
	[StorageVersion("3.4.1.0")]
	public class SafeIntType : IntType, _ISafeIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C1E RID: 7198 RVA: 0x0004EDBE File Offset: 0x0004DDBE
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeInt);
		}
	}
}
