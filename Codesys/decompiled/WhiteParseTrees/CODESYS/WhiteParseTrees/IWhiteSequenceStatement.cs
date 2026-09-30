using System.Collections;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteSequenceStatement : IWhiteStatement, IWhiteExprement, INode, IStatementSyntax, IList<IWhiteStatement>, ICollection<IWhiteStatement>, IEnumerable<IWhiteStatement>, IEnumerable
	{
	}
}
