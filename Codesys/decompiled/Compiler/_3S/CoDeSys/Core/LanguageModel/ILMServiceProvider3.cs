using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMServiceProvider3 : ILMServiceProvider2, ILMServiceProvider
	{
		ITaskLMService TaskLMService { get; }

		ISingleByteStringEncodingService SingleByteStringEncodingService { get; }
	}
}
