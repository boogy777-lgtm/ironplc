using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000168 RID: 360
	[TypeGuid("{3E8C93A8-BCB7-43c1-84DA-88910A0F5B8A}")]
	[StorageVersion("3.4.1.0")]
	public class SafeLWordType : LWordType, _ISafeLWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C28 RID: 7208 RVA: 0x0004EE22 File Offset: 0x0004DE22
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeLWord);
		}
	}
}
