using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000221 RID: 545
	internal sealed class InheritanceInformation
	{
		// Token: 0x06002431 RID: 9265 RVA: 0x0007C40C File Offset: 0x0007A60C
		public InheritanceInformation(IScope globalScope)
		{
			if (globalScope == null)
			{
				throw new ArgumentNullException("globalScope");
			}
			this.\u0001 = globalScope;
		}

		// Token: 0x06002432 RID: 9266 RVA: 0x0007C438 File Offset: 0x0007A638
		private _ISignature[] \u0001(_ISignature \u0002)
		{
			_ISignature[] result;
			if (!this.\u0001.TryGetValue(\u0002.Id, out result))
			{
				this.\u0001[\u0002.Id] = Array.Empty<_ISignature>();
				result = (this.\u0001[\u0002.Id] = this.\u0002(\u0002));
			}
			return result;
		}

		// Token: 0x06002433 RID: 9267 RVA: 0x0007C48C File Offset: 0x0007A68C
		private _ISignature[] \u0002(_ISignature \u0002)
		{
			Dictionary<int, _ISignature> dictionary = null;
			foreach (int nId in \u0002.DeclarerIds)
			{
				_ISignature isignature = (_ISignature)this.\u0001[nId];
				if (isignature != null && InheritanceInformation.\u0001(isignature, \u0002))
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<int, _ISignature>();
					}
					dictionary[isignature.Id] = isignature;
					foreach (_ISignature isignature2 in this.\u0001(isignature))
					{
						dictionary[isignature2.Id] = isignature2;
					}
				}
			}
			if (dictionary == null)
			{
				return Array.Empty<_ISignature>();
			}
			return dictionary.Values.ToArray<_ISignature>();
		}

		// Token: 0x06002434 RID: 9268 RVA: 0x0007C538 File Offset: 0x0007A738
		public IEnumerable<_ISignature> \u0001(_ISignature \u0002)
		{
			if (\u0002.ParentSignatureId != Helper.InvalidId)
			{
				\u0002 = (_ISignature)this.\u0001[\u0002.ParentSignatureId];
			}
			return this.\u0001(\u0002).Where(new Func<_ISignature, bool>(InheritanceInformation.<>c.<>9.\u0001));
		}

		// Token: 0x06002435 RID: 9269 RVA: 0x0007C598 File Offset: 0x0007A798
		private static bool \u0001(ISignature \u0002, ISignature \u0003)
		{
			return \u0002.BaseSignatureId == \u0003.Id || \u0002.InterfaceIds.Contains(\u0003.Id);
		}

		// Token: 0x0400066C RID: 1644
		private readonly Dictionary<int, _ISignature[]> \u0001 = new Dictionary<int, _ISignature[]>();

		// Token: 0x0400066D RID: 1645
		private readonly IScope \u0001;
	}
}
