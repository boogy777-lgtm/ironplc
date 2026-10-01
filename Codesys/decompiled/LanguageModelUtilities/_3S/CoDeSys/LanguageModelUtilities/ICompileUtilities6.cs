using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface ICompileUtilities6 : ICompileUtilities5, ICompileUtilities4, ICompileUtilities3, ICompileUtilities2, ICompileUtilities
	{
		void VisitAllVariables(IVariableVisitor visitor, IExprement expToVisit);
	}
}
