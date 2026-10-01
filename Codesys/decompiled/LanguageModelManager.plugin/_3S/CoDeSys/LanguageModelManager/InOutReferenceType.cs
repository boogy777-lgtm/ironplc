using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200019A RID: 410
	[TypeGuid("{62F14E25-8C68-4D8C-98CA-3067A13656F7}")]
	[StorageVersion("3.5.9.0")]
	public class InOutReferenceType : ReferenceType, _IInOutReferenceType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, _IReferenceType, IReferenceType2, IReferenceType
	{
		// Token: 0x06001D93 RID: 7571 RVA: 0x000516F9 File Offset: 0x000506F9
		public InOutReferenceType()
		{
		}

		// Token: 0x06001D94 RID: 7572 RVA: 0x00051701 File Offset: 0x00050701
		public InOutReferenceType(_IType typeBase) : base(typeBase)
		{
		}
	}
}
