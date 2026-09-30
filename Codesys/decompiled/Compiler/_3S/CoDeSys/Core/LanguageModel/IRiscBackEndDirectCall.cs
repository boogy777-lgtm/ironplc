using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscBackEndDirectCall
	{
		void CodeGenerateAbsoluteCallRelocation(IRegister RegDest, int nSignatureId);
	}
}
