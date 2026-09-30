using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IStructureInitialization : _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IStructureInitialization
	{
		IList<_IAssignmentExpression> _CompoInits { get; }

		bool DefaultInitializationDone { get; set; }

		void AddInitValue(_IAssignmentExpression assign);
	}
}
