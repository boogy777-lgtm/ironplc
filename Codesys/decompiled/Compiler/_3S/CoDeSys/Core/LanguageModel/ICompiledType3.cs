using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledType3 : ICompiledType2, ICompiledType, IType, IArchivable
	{
		ICompiledType EffectiveType { get; }

		bool IsEqual(ICompiledType type, IScope scope);
	}
}
