using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ISequenceStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ISequenceStatement3, ISequenceStatement2, ISequenceStatement
	{
		IList<_IStatement> _StatementList { get; }

		void Add(_IStatement sm);

		void Replace(_IStatement sm, int iPosition);
	}
}
