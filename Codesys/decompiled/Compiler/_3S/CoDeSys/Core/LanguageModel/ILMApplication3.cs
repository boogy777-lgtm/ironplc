using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMApplication3 : ILMApplication2, ILMApplication
	{
		bool DeviceApplication { get; set; }

		bool GenerateContent { get; set; }
	}
}
