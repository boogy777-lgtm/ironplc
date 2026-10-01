using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMPreCompileSmartCodingService3 : ILMPreCompileSmartCodingService2, ILMPreCompileSmartCodingService
	{
		string WriteExprement(IExprement exprement, WriteExprementFlags flags);
	}
}
