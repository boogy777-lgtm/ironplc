using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileService3 : ILMCompileService2, ILMCompileService
	{
		event AfterUnsuccessfullGenerateCodeEventHandler AfterUnsuccessfullGenerateCode;

		void OnAfterUnsuccessfullGenerateCode(AfterUnsuccessfullGenerateCodeEventArgs e);
	}
}
