using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IArrayDimension : IArrayDimension2, IArrayDimension
	{
		_IExpression _LowerBorder { get; set; }

		_IExpression _UpperBorder { get; set; }
	}
}
