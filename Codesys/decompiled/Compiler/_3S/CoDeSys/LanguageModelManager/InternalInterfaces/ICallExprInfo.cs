using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface ICallExprInfo : IExprInfo
	{
		int IdCalledSignature { get; set; }

		KindOfCall KindOfCall { get; set; }

		_IExpression ExpLoadTargetAddress { get; set; }

		_IAssignmentExpression InstanceAssignment { get; set; }
	}
}
