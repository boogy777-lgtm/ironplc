using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000188 RID: 392
	[TypeGuid("{0562E96E-43D1-48DC-B97D-DE3C57E8F69A}")]
	[StorageVersion("3.5.4.30")]
	public class XLIntType : LIntType, _IXLIntType, _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		// Token: 0x06001CDA RID: 7386 RVA: 0x0004FF66 File Offset: 0x0004EF66
		public override void Accept(ITypeVisitor typvis)
		{
			typvis.visit(this);
		}
	}
}
