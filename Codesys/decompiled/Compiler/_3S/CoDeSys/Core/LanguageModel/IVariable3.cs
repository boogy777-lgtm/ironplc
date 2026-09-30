using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IVariable3 : IVariable2, IVariable
	{
		IAssignmentExpression[] InputAssignments { get; }
	}
}
