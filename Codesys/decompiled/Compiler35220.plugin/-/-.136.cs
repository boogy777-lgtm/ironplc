using System;
using System.Collections.Generic;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0004
{
	// Token: 0x02000189 RID: 393
	internal sealed class \u0005
	{
		// Token: 0x06001B6A RID: 7018 RVA: 0x0005AEC0 File Offset: 0x000590C0
		public IType \u0001(string \u0002)
		{
			if (\u0002 == null)
			{
				throw new ArgumentNullException("input");
			}
			IType result;
			if (!this.\u0001.TryGetValue(\u0002, out result))
			{
				if (this.\u0001 == null)
				{
					this.\u0001 = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateScanner("", false, false, false, false);
					this.\u0001 = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateParser(this.\u0001);
				}
				this.\u0001.Initialize(\u0002);
				result = (this.\u0001[\u0002] = this.\u0001.ParseTypeDeclaration());
			}
			return result;
		}

		// Token: 0x040004BE RID: 1214
		private IScanner \u0001;

		// Token: 0x040004BF RID: 1215
		private IParser \u0001;

		// Token: 0x040004C0 RID: 1216
		private readonly Dictionary<string, IType> \u0001 = new Dictionary<string, IType>();
	}
}
