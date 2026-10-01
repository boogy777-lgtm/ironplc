using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayType : IType, IArchivable
	{
		IType Base { get; }

		IArrayDimension[] Dimensions { get; }
	}
}
