using System;
using System.Collections.Generic;
using \u001C;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0080
{
	// Token: 0x02000305 RID: 773
	internal sealed class \u0014 : \u0011
	{
		// Token: 0x06002F03 RID: 12035 RVA: 0x000B11C0 File Offset: 0x000AF3C0
		internal \u0014(_ICompileContext \u0001\u0008)
		{
			this.\u0001 = \u0001\u0008;
		}

		// Token: 0x06002F04 RID: 12036 RVA: 0x000B11D0 File Offset: 0x000AF3D0
		public _ISignature4 \u0001(_ISignature4 \u0002)
		{
			return this.\u0001.GetSignatureById(\u0002.BaseSignatureId) as _ISignature4;
		}

		// Token: 0x06002F05 RID: 12037 RVA: 0x000B11E8 File Offset: 0x000AF3E8
		public IEnumerable<string> \u0001(_ISignature4 \u0002)
		{
			return \u0002.GetOverloadedNames();
		}

		// Token: 0x06002F06 RID: 12038 RVA: 0x000B11F0 File Offset: 0x000AF3F0
		public bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			Guid a = Guid.NewGuid();
			if (\u0003.HasAttribute("shadowed"))
			{
				Guid.TryParse(\u0003.GetAttributeValue("shadowed"), out a);
			}
			return \u0003.Name == \u0002.Name || a == \u0002.ObjectGuid;
		}

		// Token: 0x06002F07 RID: 12039 RVA: 0x000B1244 File Offset: 0x000AF444
		public IList<_ISignature> \u0001(_ISignature4 \u0002, string \u0003)
		{
			return \u0002.GetOverloadedSignatures(\u0003);
		}

		// Token: 0x06002F08 RID: 12040 RVA: 0x000B1250 File Offset: 0x000AF450
		public ICompiledType \u0001(_IExpression \u0002, int \u0003)
		{
			return \u0002._CompiledType;
		}

		// Token: 0x06002F09 RID: 12041 RVA: 0x000B1258 File Offset: 0x000AF458
		public string \u0001(_ISignature \u0002)
		{
			return \u0002.GetSearchName(this.\u0001);
		}

		// Token: 0x040008F4 RID: 2292
		private readonly _ICompileContext \u0001;
	}
}
