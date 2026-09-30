using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000184 RID: 388
	[TypeGuid("{1B39B304-FF51-4801-9F54-9501ADDEFD53}")]
	[StorageVersion("3.5.2.0")]
	public class XUDIntType : UDIntType, _IXUDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CCC RID: 7372 RVA: 0x0004FF13 File Offset: 0x0004EF13
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}
	}
}
