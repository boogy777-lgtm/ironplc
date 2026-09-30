using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IPragmaElseIf
	{
		_IExpression Condition { get; set; }

		_IStatement Controlled { get; set; }

		_IElseIf CreateElseIf();

		_IPragmaElseIf Duplicate();
	}
}
