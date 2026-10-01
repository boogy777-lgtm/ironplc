using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IReferenceType2 : IReferenceType, IType, IArchivable
	{
		ICompiledType OriginalBase { get; }
	}
}
