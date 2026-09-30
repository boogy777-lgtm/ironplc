using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompileOptions2 : ILMCompileOptions
	{
		bool UTF8Encoding { get; set; }
	}
}
