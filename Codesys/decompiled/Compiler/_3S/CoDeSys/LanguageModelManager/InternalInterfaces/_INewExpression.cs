using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _INewExpression : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, INewExpression
	{
		_IType _TypeToCast { get; set; }

		_IExpression _Count { get; set; }

		IList<IAssignmentExpression> _FBInitParams { get; }

		bool PositionOK { get; set; }

		void AddFBInitParam(IAssignmentExpression assexp);

		int ElementCount(out bool bValid);
	}
}
