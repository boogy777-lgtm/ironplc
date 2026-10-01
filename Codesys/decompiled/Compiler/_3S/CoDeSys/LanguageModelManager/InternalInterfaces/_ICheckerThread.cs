using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICheckerThread
	{
		void Enable();

		void Disable();

		void FinishPrecompileChecks();

		void Clear();

		void TryStart();

		void EnablePrecompileChecksInNoUIMode();

		void DisablePrecompileChecksInNoUIMode();
	}
}
