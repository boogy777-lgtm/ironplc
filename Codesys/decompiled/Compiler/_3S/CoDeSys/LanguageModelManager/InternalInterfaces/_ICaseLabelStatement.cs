using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICaseLabelStatement : _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, ICaseLabelStatement
	{
		IList<_IExpression> _cases { get; }

		_IExpression this[int i] { get; set; }

		void AddCase(_IExpression expCase);
	}
}
