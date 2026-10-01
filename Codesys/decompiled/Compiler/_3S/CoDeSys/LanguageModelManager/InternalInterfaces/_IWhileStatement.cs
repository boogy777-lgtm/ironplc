using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IWhileStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IWhileStatement
	{
		_IExpression _Condition { get; set; }

		_IStatement _Controlled { get; set; }
	}
}
