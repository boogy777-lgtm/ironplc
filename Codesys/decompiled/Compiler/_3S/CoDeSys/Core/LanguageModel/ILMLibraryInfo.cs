using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMLibraryInfo
	{
		string Identification { get; set; }

		string Namespace { get; set; }

		bool PublishSymbols { get; set; }

		bool SystemLibrary { get; set; }

		string DefaultNamespace { get; set; }

		bool LinkAllContent { get; set; }

		bool LinkInSimulation { get; set; }

		bool QualifiedOnly { get; set; }

		bool SystemApplication { get; set; }

		ILibParameterTable ParamTable { get; set; }
	}
}
