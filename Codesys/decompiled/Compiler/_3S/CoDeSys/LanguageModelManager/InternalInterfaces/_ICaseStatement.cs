using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICaseStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseStatement
	{
		_IExpression _Switch { get; set; }

		IList<_ICase> _Cases { get; }

		_IStatement _Else { get; set; }

		void AddCase(_ICase case1);
	}
}
