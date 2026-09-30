using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVectorType : IType, IArchivable
	{
		IType Base { get; }

		IExpression Dimension { get; }

		int DimensionInt(IScope scope, out bool bValid);
	}
}
