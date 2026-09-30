using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IImplicitReferenceVariable
	{
		int SignatureId { get; }

		_IVariable Var { get; }
	}
}
