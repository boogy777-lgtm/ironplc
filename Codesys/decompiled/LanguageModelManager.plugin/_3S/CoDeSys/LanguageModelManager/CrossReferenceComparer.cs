using System;
using System.Collections;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000149 RID: 329
	internal class CrossReferenceComparer : IComparer
	{
		// Token: 0x06001B5A RID: 7002 RVA: 0x0004DB04 File Offset: 0x0004CB04
		public int Compare(object x, object y)
		{
			ICrossReference crossReference;
			ICrossReference crossReference2;
			try
			{
				crossReference = (ICrossReference)x;
				crossReference2 = (ICrossReference)y;
			}
			catch
			{
				return 0;
			}
			if (crossReference.CodeId < crossReference2.CodeId)
			{
				return -1;
			}
			if (crossReference.CodeId == crossReference2.CodeId)
			{
				return 0;
			}
			return 1;
		}
	}
}
