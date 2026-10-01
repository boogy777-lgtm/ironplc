using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompilePositionInfo
	{
		ICodePosition CodePosition { get; }

		int ReferencingPrecompileSignatureId { get; }

		string Name { get; }
	}
}
