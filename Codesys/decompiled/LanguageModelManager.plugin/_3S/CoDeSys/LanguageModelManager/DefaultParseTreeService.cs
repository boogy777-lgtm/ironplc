using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000035 RID: 53
	internal class DefaultParseTreeService : IParseTreeService
	{
		// Token: 0x06000296 RID: 662 RVA: 0x00009D50 File Offset: 0x00008D50
		public IParseTreeProvider CreateParseTreeProvider(_ICompiledPOU2 cpou, IGreenTreeConverter converter, ITreeFactory treeFactory)
		{
			return new StandardParseTreeProvider(cpou, converter, treeFactory);
		}
	}
}
