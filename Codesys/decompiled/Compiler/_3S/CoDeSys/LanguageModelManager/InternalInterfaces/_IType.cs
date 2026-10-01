using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IType : ICompiledType5, ICompiledType4, ICompiledType3, ICompiledType2, ICompiledType, IType, IArchivable
	{
		bool IsCompiled { get; set; }

		_IType Duplicate { get; }

		string GetConstantString(IScope scope);

		_IType _Duplicate(bool bDeep);

		ICompiledType GetComponent(int i, IScope5 scope);

		string[] GetComponents(IScope5 scope, out bool bValid);

		void Accept(ITypeVisitor typvis);

		int GetNumOfElements(IScope5 scope);

		int SizeChecked(IScope scope, out bool bValid);

		string ToUpperString();
	}
}
