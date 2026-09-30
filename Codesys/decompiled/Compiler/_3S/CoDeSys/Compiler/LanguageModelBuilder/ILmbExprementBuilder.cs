using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.LanguageModelBuilder
{
	[ReleasedInterface]
	public interface ILmbExprementBuilder<out T>
	{
		T Build();
	}
}
