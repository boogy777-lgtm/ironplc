using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200015F RID: 351
	[TypeGuid("{B37D6263-53B4-4647-A749-4D0BA4189798}")]
	[StorageVersion("3.4.1.0")]
	public class SafeByteType : ByteType, _ISafeByteType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, ISafetyType
	{
		// Token: 0x06001C16 RID: 7190 RVA: 0x0004ED6E File Offset: 0x0004DD6E
		public override string ToString()
		{
			return CompilerProxy.GetTextOfOperator(Operator.SafeByte);
		}
	}
}
