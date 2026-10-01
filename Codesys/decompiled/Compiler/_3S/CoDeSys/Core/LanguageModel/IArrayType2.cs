using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayType2 : IArrayType, IType, IArchivable
	{
		int GetNumOfElements(IScope5 scope, out bool bValid);

		int[] ToDimensionIndexes(int elementIndex, IScope5 scope, out bool bValid);
	}
}
