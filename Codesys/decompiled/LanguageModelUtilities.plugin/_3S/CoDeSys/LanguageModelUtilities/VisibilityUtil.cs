using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class VisibilityUtil
	{
		internal static bool IsHiddenSignature(ISignature signature, IAccessInfo accInfo, GUIHidingFlags flags)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(signature, flags);
		}
	}
}
