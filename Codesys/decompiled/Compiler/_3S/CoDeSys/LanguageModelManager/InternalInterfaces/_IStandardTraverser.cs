using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _IStandardTraverser : IStandardTraverser, IExprementVisitor2, IExprementVisitor, IExprementVisitor3590
	{
		bool InLeftSideOfCompoAccess { get; }
	}
}
