using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000187 RID: 391
	[TypeGuid("{C43C8B26-159D-4B0D-9B51-2804D60525E3}")]
	[StorageVersion("3.5.4.30")]
	public class XDIntType : DIntType, _IXDIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CD8 RID: 7384 RVA: 0x0004FF40 File Offset: 0x0004EF40
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
