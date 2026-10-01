using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMServiceProvider4 : ILMServiceProvider3, ILMServiceProvider2, ILMServiceProvider
	{
		ILMCallTreeService CallTreeService { get; }
	}
}
