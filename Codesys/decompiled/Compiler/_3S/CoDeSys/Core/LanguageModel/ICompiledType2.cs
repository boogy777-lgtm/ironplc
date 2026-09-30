using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledType2 : ICompiledType, IType, IArchivable
	{
		bool IsCompatible(ICompiledType type, IScope2 scope);
	}
}
