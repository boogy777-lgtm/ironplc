using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPragmaIfStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaIfStatement
	{
		_IExpression Condition { get; set; }

		_IStatement IfThen { get; set; }

		_IStatement IfElse { get; set; }

		IList<_IPragmaElseIf> ElseIf { get; }

		void AddElseIf(_IPragmaElseIf elseIf);
	}
}
