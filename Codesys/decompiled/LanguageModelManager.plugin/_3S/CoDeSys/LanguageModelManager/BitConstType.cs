using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200016F RID: 367
	[TypeGuid("{4843ac80-bf56-4163-a5d4-aef6b1846075}")]
	[StorageVersion("3.3.0.0")]
	public class BitConstType : BitType, _IBitConstType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x17000789 RID: 1929
		// (get) Token: 0x06001C3C RID: 7228 RVA: 0x0004EFA0 File Offset: 0x0004DFA0
		public override TypeClass Class
		{
			get
			{
				return TypeClass.BitConst;
			}
		}

		// Token: 0x06001C3D RID: 7229 RVA: 0x0004EFA4 File Offset: 0x0004DFA4
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}
	}
}
