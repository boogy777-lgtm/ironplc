using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICommentStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICommentStatement
	{
		bool DocComment { get; set; }

		new string Text { get; set; }
	}
}
