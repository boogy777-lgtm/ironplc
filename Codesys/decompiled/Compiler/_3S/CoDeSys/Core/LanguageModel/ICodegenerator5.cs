using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICodegenerator5 : ICodegenerator4, ICodegenerator3, ICodegenerator2, ICodegenerator
	{
		int GetExternalInputSize(int iIndexInput, IVariable varInput);
	}
}
