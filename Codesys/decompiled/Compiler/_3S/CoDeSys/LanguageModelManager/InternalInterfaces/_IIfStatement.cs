using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IIfStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IIfStatement
	{
		_IExpression _Condition { get; set; }

		_IStatement _IfThen { get; set; }

		_IStatement _IfElse { get; set; }

		IList<_IElseIf> _ElseIf { get; }

		void ClearElseIf();

		void AddElseIf(_IElseIf elseIf);
	}
}
