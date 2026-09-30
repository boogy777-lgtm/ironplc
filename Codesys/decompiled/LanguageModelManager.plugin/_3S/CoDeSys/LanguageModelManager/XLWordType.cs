using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000182 RID: 386
	[TypeGuid("{859036BA-E550-4F5C-9B70-16CA9202D0CE}")]
	[StorageVersion("3.5.2.0")]
	public class XLWordType : LWordType, _IXLWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CC2 RID: 7362 RVA: 0x0004FED4 File Offset: 0x0004EED4
		public override void Accept(ITypeVisitor typvis)
		{
			ITypeVisitor3 typeVisitor = typvis as ITypeVisitor3;
			if (typeVisitor != null)
			{
				typeVisitor.visit(this);
				return;
			}
			typvis.visit(this);
		}
	}
}
