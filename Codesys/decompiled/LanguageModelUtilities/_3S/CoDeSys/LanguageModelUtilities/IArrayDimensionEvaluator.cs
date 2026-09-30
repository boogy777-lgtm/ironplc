using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IArrayDimensionEvaluator
	{
		void EvaluateArrayDimension(IArrayDimension dim, out int iLowerBorder, out int iUpperBorder);
	}
}
