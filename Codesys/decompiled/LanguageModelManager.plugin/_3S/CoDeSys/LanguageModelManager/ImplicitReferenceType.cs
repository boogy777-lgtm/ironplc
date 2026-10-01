using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000199 RID: 409
	[TypeGuid("{FD75E514-8A3D-499C-B50D-9A8B398D0A99}")]
	[StorageVersion("3.5.9.0")]
	public class ImplicitReferenceType : ReferenceType, _IImplicitReferenceType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, _IReferenceType, IReferenceType2, IReferenceType
	{
		// Token: 0x06001D91 RID: 7569 RVA: 0x000516F9 File Offset: 0x000506F9
		public ImplicitReferenceType()
		{
		}

		// Token: 0x06001D92 RID: 7570 RVA: 0x00051701 File Offset: 0x00050701
		public ImplicitReferenceType(_IType typeBase) : base(typeBase)
		{
		}
	}
}
