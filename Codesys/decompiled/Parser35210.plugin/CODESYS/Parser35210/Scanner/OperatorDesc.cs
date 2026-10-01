using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser35210.Scanner
{
	internal class OperatorDesc
	{
		public Operator Operator { get; }

		public OperatorFlags Flags { get; }

		public OperatorDesc(Operator op, OperatorFlags flags)
		{
			Operator = op;
			Flags = flags;
		}
	}
}
