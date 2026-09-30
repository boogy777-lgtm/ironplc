using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000185 RID: 389
	[TypeGuid("{B297BF25-ACD4-4B2F-BC57-473CB93F7D4C}")]
	[StorageVersion("3.5.2.0")]
	public class XULIntType : ULIntType, _IXULIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CCE RID: 7374 RVA: 0x0004FF1C File Offset: 0x0004EF1C
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}
	}
}
