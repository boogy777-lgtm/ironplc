using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000181 RID: 385
	[TypeGuid("{119284D1-95B7-462D-B06E-48AD65601F7B}")]
	[StorageVersion("3.5.2.0")]
	public class XDWordType : DWordType, _IXDWordType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CC0 RID: 7360 RVA: 0x0004FEAC File Offset: 0x0004EEAC
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
