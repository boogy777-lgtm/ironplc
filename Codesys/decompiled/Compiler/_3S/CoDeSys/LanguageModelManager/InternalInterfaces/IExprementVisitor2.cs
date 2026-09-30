using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IExprementVisitor2 : IExprementVisitor
	{
		void visit(_ITryCatchStatement trycatchstatement);
	}
}
