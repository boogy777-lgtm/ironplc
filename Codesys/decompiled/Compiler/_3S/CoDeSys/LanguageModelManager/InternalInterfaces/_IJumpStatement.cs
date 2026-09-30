using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IJumpStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IJumpStatement
	{
		_IExpression _Condition { get; set; }

		new string Label { get; set; }
	}
}
