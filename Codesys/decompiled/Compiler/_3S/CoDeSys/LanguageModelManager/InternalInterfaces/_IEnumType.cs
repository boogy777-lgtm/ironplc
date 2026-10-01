using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IEnumType : _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IEnumType2, IEnumType
	{
		new int SignatureId { get; set; }

		_IType _Base { get; set; }

		_IVariableExpression _DefaultValue { get; set; }

		ISignature GetSignature(IScope scope);
	}
}
