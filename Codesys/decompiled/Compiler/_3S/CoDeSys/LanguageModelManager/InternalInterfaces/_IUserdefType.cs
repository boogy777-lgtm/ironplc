using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IUserdefType : _IType, ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable, IUserdefType2, IUserdefType
	{
		new int SignatureId { get; set; }

		int ScopeId { get; set; }

		new IExpression NameExpression { get; set; }

		ISignature GetSignature(IScope scope);

		IScope2 GetScope(IScope2 scope);
	}
}
