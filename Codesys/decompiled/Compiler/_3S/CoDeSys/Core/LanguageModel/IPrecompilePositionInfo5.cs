using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompilePositionInfo5 : IPrecompilePositionInfo4, IPrecompilePositionInfo3, IPrecompilePositionInfo2, IPrecompilePositionInfo
	{
		int SignatureIDAtCodePosition { get; }
	}
}
