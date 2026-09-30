using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{3B084046-3555-4A84-9E2F-F0F8FFCE980E}")]
	public sealed class Public : AbstractPOUWithMethods, IProgramBuilder, IPouBuilder, IHasVarDeclaration, IHasAttributes
	{
		protected override Operator PouType => Operator.Program;
	}
}
