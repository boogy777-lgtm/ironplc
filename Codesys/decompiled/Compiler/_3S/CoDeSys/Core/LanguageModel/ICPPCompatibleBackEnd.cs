using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICPPCompatibleBackEnd
	{
		void InitCPPInstancePointer(int nInstancePtrOffset);

		int GetExternalInputGranularity(IType type);
	}
}
