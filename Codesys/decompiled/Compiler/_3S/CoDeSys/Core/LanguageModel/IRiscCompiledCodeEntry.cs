using System.IO;
using System.Text;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCodeEntry
	{
		uint CodeSize { get; }

		IBreakpoint Breakpoint { get; set; }

		string ErrorMessage { get; }

		void WriteCode(BinaryWriter bw, IRiscBackEnd BackEnd);

		bool IsInstruction();

		bool ResolveLabel(int nDistance, string stLabel, IRiscBackEnd BackEnd);

		void AddRelocations(IRiscRelocationList rl, int nCurrentOffset);

		void AddBreakpointSuccessors(int nCurrentOffset, IRiscCompiledCode rcc);

		bool Disassemble(uint uiStartAddress, StringBuilder sb, IRiscBackEnd BackEnd);
	}
}
