using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IIsEnumTypeExpression : IExpression2, IExpression, IExprement
	{
		ICompiledType ReferencedType { get; }
	}
}
