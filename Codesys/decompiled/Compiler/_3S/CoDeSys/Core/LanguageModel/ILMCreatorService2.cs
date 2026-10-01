using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCreatorService2 : ILMCreatorService
	{
		IRawSTParser CreateRawSTParser(string stText, bool bImplicit);

		IRawSTParser CreateRawSTParser(char[] stText, bool bImplicit);
	}
}
