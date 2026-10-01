using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.StructuredLanguageModel
{
	[TypeGuid("{1EA376B6-07F7-4A56-8751-206C176ACA10}")]
	public sealed class Interface : FunctionBlock
	{
		protected override Operator PouType => Operator.Interface;
	}
}
