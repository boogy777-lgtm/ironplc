using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompileLibInfo
	{
		string Identification { get; }

		string Namespace { get; }

		bool PublishSymbols { get; }
	}
}
