using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IArrayInitialization : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IArrayInitialization
	{
		IList<_IExpression> _InitValues { get; }

		_IExpression this[int i] { get; set; }

		bool DefaultInitializationDone { get; set; }

		IList<_IExpression> GetFlatList(IScope scope, out bool bValid);

		void AddInitValue(_IExpression exp);
	}
}
