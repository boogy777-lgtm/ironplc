using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompiledType5 : ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		bool IsEqualPreCompile(ICompiledType type, IScope scope);
	}
}
