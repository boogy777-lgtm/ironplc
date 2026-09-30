using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariable
	{
		IType Type { get; }

		int Id { get; }

		ICompiledType CompiledType { get; }

		ICompiledType OriginalType { get; }

		IExpression Initial { get; }

		string Name { get; }

		string OrgName { get; }

		IDirectVariable Address { get; }

		string[] Attributes { get; }

		IDataLocation DataLocation { get; }

		ICrossReference[] CrossReferences { get; }

		string Comment { get; }

		ISourcePosition SourcePosition { get; }

		IVariable Duplicate();

		bool HasFlag(VarFlag vfFlag);

		bool GetFlag(VarFlag vfFlag);

		bool HasAttribute(string stAttribute);

		string GetAttributeValue(string stAttribute);

		bool IsEqual(IVariable varRight, bool bCompareInitValues);
	}
}
