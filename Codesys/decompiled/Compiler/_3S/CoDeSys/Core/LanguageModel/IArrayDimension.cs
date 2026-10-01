using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IArrayDimension
	{
		IExpression LowerBorder { get; }

		IExpression UpperBorder { get; }

		int LowerBorderInt(out bool bValid, IScope scope);

		int UpperBorderInt(out bool bValid, IScope scope);

		int Range(out bool bValid, IScope scope);
	}
}
