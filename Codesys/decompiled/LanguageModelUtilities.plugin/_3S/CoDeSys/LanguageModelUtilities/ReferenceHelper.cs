using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class ReferenceHelper
	{
		internal static LList<int> GetImplicitDeclarers(ISignature6 signature)
		{
			LList<int> val = new LList<int>();
			if (signature.POUType == Operator.Method || signature.POUType == Operator.Function)
			{
				val.Add(signature.PrecompileId);
				ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(signature.PrecompileParentId);
				val = GetParentDeclarers(signature, val, signatureForPrecompileID);
			}
			return val;
		}

		private static LList<int> GetParentDeclarers(ISignature6 signature, LList<int> declarers, ISignature6 parent)
		{
			if (parent != null)
			{
				foreach (int item in CollectDescendants(parent.PrecompileId, new HashSet<int>()))
				{
					ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(item);
					if (signatureForPrecompileID != null)
					{
						ISignature[] subSignatures = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(signatureForPrecompileID).GetSubSignatures(signatureForPrecompileID.ObjectGuid);
						for (int i = 0; i < subSignatures.Length; i++)
						{
							ISignature6 signature2 = (ISignature6)subSignatures[i];
							if (signature2.POUType == signature.POUType && signature2.Name == signature.Name)
							{
								declarers.Add(signature2.PrecompileId);
								declarers = Enumerable.ToLList<int>(((IEnumerable<int>)declarers).Union(signature2.PrecompileCallerIds).Union(signature2.PrecompileDeclarerIds));
							}
						}
					}
				}
				return declarers;
			}
			return declarers;
		}

		internal static HashSet<int> CollectDescendants(int signatureId, HashSet<int> hsRecursionGuard)
		{
			HashSet<int> hashSet = new HashSet<int>();
			if (hsRecursionGuard.Contains(signatureId))
			{
				return hashSet;
			}
			hsRecursionGuard.Add(signatureId);
			ILanguageModelManager22 languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			ISignature6 signatureForPrecompileID = languageModelMgr.GetSignatureForPrecompileID(signatureId);
			if (signatureForPrecompileID == null)
			{
				return hashSet;
			}
			foreach (int precompileCallerId in signatureForPrecompileID.PrecompileCallerIds)
			{
				if (!hashSet.Contains(precompileCallerId))
				{
					ISignature6 signatureForPrecompileID2 = languageModelMgr.GetSignatureForPrecompileID(precompileCallerId);
					if (signatureForPrecompileID2 == null || (signatureForPrecompileID2.PrecompileBaseSignatureId != signatureId && !signatureForPrecompileID2.InterfaceIds.Contains(signatureId)))
					{
						continue;
					}
					hashSet.Add(precompileCallerId);
					foreach (int item in CollectDescendants(precompileCallerId, hsRecursionGuard))
					{
						hashSet.Add(item);
					}
					continue;
				}
				return hashSet;
			}
			return hashSet;
		}
	}
}
