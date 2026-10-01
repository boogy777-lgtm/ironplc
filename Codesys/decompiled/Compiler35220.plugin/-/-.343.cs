using System;
using System.Linq;
using \u0018;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0019
{
	// Token: 0x02000387 RID: 903
	internal sealed class \u0013 : \u001E
	{
		// Token: 0x0600349C RID: 13468 RVA: 0x000CF8A0 File Offset: 0x000CDAA0
		public bool \u0001(\u0018.\u0010 \u0002)
		{
			foreach (_ICompiledPOU icompiledPOU in Enumerable.ToReadOnlyCollectionWrapper<_ICompiledPOU>(\u0002.ComconNew._GetAllCompiledPOUs()))
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, false);
				icompiledPOU.SetFlag(CompiledPOUFlags.Typified, true);
			}
			foreach (_ISignature isignature in \u0002.ComconNew.GetAllSignaturesFlatEx().OfType<_ISignature>())
			{
				isignature.SetFlag(SignatureFlag.Located, true);
				isignature.SetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy, false);
			}
			return true;
		}
	}
}
