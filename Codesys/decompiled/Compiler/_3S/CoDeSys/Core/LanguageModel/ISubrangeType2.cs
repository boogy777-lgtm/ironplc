using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISubrangeType2 : ISubrangeType, IType, IArchivable
	{
		ICompiledType BaseType { get; }
	}
}
