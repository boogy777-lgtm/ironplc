using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Compiler.Serialization
{
	[ReleasedInterface]
	public interface ICompileOptionsSerializable
	{
		bool ReplaceConstantsToSave { get; set; }

		int[] CompilerVersionToSave { get; set; }

		bool UnicodeToSave { get; set; }

		bool LoggingInBreakpointsToSave { get; set; }

		bool CompileOptionsChanged();
	}
}
