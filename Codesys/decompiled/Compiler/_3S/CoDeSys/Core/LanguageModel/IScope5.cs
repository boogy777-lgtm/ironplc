using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScope5 : IScope4, IScope3, IScope2, IScope
	{
		IList<ISignature> this[string stName] { get; }

		bool CopyScope { get; set; }

		ICodegenerator Codegenerator { get; }

		IScope5 SystemScope { get; }

		ICompileContext ApplicationContext { get; }

		int MostLocalSignatureId { get; }

		ISignature MethodSignature { get; set; }

		new ISignature LocalSignature { get; set; }

		new string Name { get; set; }

		string DisplayName { get; }

		bool LocalScope { get; set; }

		bool FindListBeforeVariable { get; set; }

		bool FindDeclaration(IExpression exp, out IVariable[] variable, out ISignature[] signature, out IScope scope);

		IVariable[] FindVariableGlobal(string stName, out ISignature[] signsWithVar);

		IVariable FindVariableLocal(string stName, out ISignature signWithVar);

		ISignature FindFirstSignature(string stName);

		ISignature FindSignature(IEnumType etype);

		IScope5 CreateLocalScope(ISignature sign);

		ISignature FindSignatureLocal(string stName);

		void SetLocalSignature(ISignature sign);

		void InitFriend();
	}
}
