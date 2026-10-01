using System;
using System.Collections;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IRiscCompiledCode : ICompiledCode, ICloneable
	{
		uint CurrentOffset { get; }

		[Obsolete("No longer supported, function will return null! IRiscCompiledCode2.InstructionList instead")]
		ArrayList Instructions { get; }

		[Obsolete("Use IRiscCompiledCode2.LabelTable instead")]
		Hashtable Labels { get; }

		int AppendEntry(IRiscCompiledCodeEntry entry);

		string DefineLabel(string stLabel);

		string UseLabel(string stLabel);

		void DefineBreakpoint(IExprement expr);

		void Generate(ICompiledPOU cpou);

		int GetOffset(int iIndex);

		int GetDistance(int iIndexEntry1, int iIndexEntry2);

		IRiscCompiledCodeEntry GetEntry(int i);
	}
}
