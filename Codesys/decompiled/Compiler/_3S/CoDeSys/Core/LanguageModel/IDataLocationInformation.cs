using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDataLocationInformation
	{
		ICompiledPOU CompiledPOU { get; }

		IBreakpoint Breakpoint { get; }

		ISourcePosition SourcePosition { get; }

		string InstancePath { get; }

		string EditorInstancePath { get; }

		IVarRef VarRef { get; }
	}
}
