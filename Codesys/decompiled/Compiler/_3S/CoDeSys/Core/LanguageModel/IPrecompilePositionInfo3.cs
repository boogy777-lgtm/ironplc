using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IPrecompilePositionInfo3 : IPrecompilePositionInfo2, IPrecompilePositionInfo
	{
		int VariableIDAtCodePosition { get; }
	}
}
