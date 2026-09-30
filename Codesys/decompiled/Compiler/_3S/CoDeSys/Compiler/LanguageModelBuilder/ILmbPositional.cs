using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder
{
	[ReleasedInterface]
	public interface ILmbPositional<out TNext>
	{
		TNext At(IExprementPosition position);
	}
}
