using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICompiledExpressionTypeInfo
	{
		int SignatureId { get; set; }

		int VariableId { get; set; }

		_IType CompiledType { get; set; }
	}
}
