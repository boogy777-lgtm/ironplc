using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayDimension2 : IArrayDimension
	{
		int LowerBorderInt(out bool bValid, IPrecompileScope scope);

		int UpperBorderInt(out bool bValid, IPrecompileScope scope);

		int Range(out bool bValid, IPrecompileScope scope);
	}
}
