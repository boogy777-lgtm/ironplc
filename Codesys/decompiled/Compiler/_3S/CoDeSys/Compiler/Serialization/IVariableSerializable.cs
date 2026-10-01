using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface IVariableSerializable
	{
		long PositionToSave { get; set; }

		string OrgName { get; set; }

		ICompiledType OriginalType { get; set; }

		VarFlag Flags { get; set; }

		int Id { get; set; }

		IDataLocation DataLocation { get; set; }

		_IExpression _Initial { get; set; }

		IDirectVariable Address { get; set; }

		IAssignmentExpression[] InputAssignments { get; set; }

		string[] Attributes { get; }

		ICrossReference[] CrossReferences { get; }

		void SetCrossReferences(IList<ICrossReferenceSerializable> crossrefs);

		void SetAttributes(IList<KeyValuePair<string, string>> attributes);

		string GetAttributeValue(string stAttribute);
	}
}
