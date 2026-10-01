using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IVariable : IVariable5, IVariable4, IVariable3, IVariable2, IVariable
	{
		bool IsVarInoutConstant { get; }

		ICompiledType CompiledTypeInternal { get; }

		_IType _Type { get; set; }

		_ISourcePosition _SourcePosition { get; }

		new IExpression Initial { get; set; }

		_IExpression _Initial { get; set; }

		new IDataLocation DataLocation { get; set; }

		string VersionedName { get; }

		bool IsProperty { get; }

		bool IsPropertyMonitor { get; }

		VarFlag Flags { get; }

		new string Name { get; set; }

		new string Comment { get; set; }

		string DocuComment { get; set; }

		new int Id { get; set; }

		new IDirectVariable Address { get; set; }

		new IAssignmentExpression[] InputAssignments { get; set; }

		new int PrecompileId { get; set; }

		void SetFlag(VarFlag vf, bool bSet);

		void SetInitial(IExpression expInitial);

		void SetType(ICompiledType ctype);

		void AddCrossReference(int nCodeId, ICodePosition copos);

		void SetInputAssignments(ICollection<_IAssignmentExpression> inputs);

		bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes);

		bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope);

		bool InitialValueEquals(IVariable varCompile);

		void AddPrecompileCrossReference(int nCodeId);

		void SetAttributeValue(string stAttribute, string stValue);

		_IVariable Duplicate(bool bDeep);

		void RemoveAttribute(string stAttribute);
	}
}
