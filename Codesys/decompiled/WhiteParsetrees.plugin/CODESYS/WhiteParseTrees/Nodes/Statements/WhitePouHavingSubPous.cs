using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Statements
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public abstract class WhitePouHavingSubPous : WhiteStatement
	{
		protected bool _bImplementationBeforeSubPOUs;

		public IWhiteSequenceStatement BeforeDeclarationStatements { get; }

		public IWhiteSequenceStatement Implementation { get; }

		public IEnumerable<IWhitePOUSyntax> SubPOUs { get; }

		protected WhitePouHavingSubPous(IWhiteSequenceStatement seqBefore, IWhiteSequenceStatement implementationAndSubPOUs)
		{
			BeforeDeclarationStatements = seqBefore;
			SubPOUs = implementationAndSubPOUs.OfType<IWhitePOUSyntax>().ToArray();
			Implementation = implementationAndSubPOUs;
			RemoveSubPOUsFromImplementation();
		}

		private void RemoveSubPOUsFromImplementation()
		{
			int num = -1;
			int num2 = -1;
			int num3 = Implementation.Count - 1;
			while (0 <= num3)
			{
				if (Implementation[num3] is IWhitePOU || Implementation[num3] is IWhiteErrorPOU)
				{
					Implementation.RemoveAt(num3);
					if (-1 == num)
					{
						num = num3;
					}
				}
				else if (-1 == num2)
				{
					num2 = num3;
				}
				num3--;
			}
			_bImplementationBeforeSubPOUs = num2 < num;
		}
	}
}
