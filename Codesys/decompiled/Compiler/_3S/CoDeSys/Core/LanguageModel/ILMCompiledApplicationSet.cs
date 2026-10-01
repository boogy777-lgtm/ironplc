using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCompiledApplicationSet : ILMPouSet
	{
		IEnumerable<ICompiledPOU4> AllPOUs { get; }

		IEnumerable<ICompiledPOU4> POUsToCompile { get; }

		IEnumerable<ISignature4> AllSignaturesFlat { get; }

		IDirectVariableCrossRefTable DirectVariableTable { get; }

		long TimeStamp { get; }

		uint CheckSumCode { get; }

		uint CheckSumData { get; }

		bool ContainsOnlineChangeCode { get; }

		bool FastOnlineChange { get; }

		ILMCompiledApplicationSet ParentSet { get; }

		ILibraryTable2 LibraryTable { get; }

		ISignature GetSignatureById(int nId);

		ICompiledPOU GetCompiledPOUById(int nId);

		bool IsDefined(string stDefineIdent);

		bool DefineHasValue(string stDefineIdent, string stValue);

		void Define(string stDefineIdent, string stValue);

		void Undefine(string stDefineIdent);
	}
}
