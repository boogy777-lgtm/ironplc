using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ISpecialSizeType
	{
		int CompatibilitySize { get; }

		ICompiledType CodegeneratorType { get; }
	}
}
