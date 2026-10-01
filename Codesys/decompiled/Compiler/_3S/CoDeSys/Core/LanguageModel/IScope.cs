using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScope
	{
		ISignature this[int nId] { get; }

		ISignature[] All { get; }

		IScope GlobalScope { get; }

		ISignature LocalSignature { get; }

		int Id { get; }

		string Name { get; }

		IScope GetScopeById(int nId);

		IVariable[] FindVariable(string stName, out ISignature[] signWithVar);

		IVariable[] FindVariable(string stName);

		ISignature[] FindSignature(string stName);

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use FindSignature in IScope2 instead")]
		ISignature[] FindSignature(IQualifiedNameExpression qne);

		bool FindDeclaration(string stIdent, out IVariable[] variable, out ISignature[] signature, out IScope scope);

		void Define(string stDefineIdent, string stValue);

		void Undefine(string stDefineIdent);

		bool IsDefined(string stDefineIdent);

		bool DefineHasValue(string stDefineIdent, string stValue);
	}
}
