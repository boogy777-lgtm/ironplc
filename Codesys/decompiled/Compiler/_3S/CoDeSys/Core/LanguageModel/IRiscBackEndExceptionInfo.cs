using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndExceptionInfo
	{
		void CodeCall(IRegister RegAddress, bool bExternal, bool bGenerateExceptionInfo);
	}
}
