using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IEnumType2 : IEnumType, IType, IArchivable
	{
		int SignatureId { get; }

		ICompiledType BaseType { get; }
	}
}
