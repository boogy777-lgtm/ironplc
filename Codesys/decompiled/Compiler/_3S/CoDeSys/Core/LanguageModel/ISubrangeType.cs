using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISubrangeType : IType, IArchivable
	{
		IExpression LowerBorder { get; }

		IExpression UpperBorder { get; }
	}
}
