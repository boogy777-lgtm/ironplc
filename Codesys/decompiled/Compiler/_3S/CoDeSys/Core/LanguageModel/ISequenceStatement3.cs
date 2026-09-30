using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISequenceStatement3 : ISequenceStatement2, ISequenceStatement, IStatement, IExprement
	{
		IEnumerable<IStatement> StatementList { get; }
	}
}
