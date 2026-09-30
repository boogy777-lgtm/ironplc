using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IStandardTraverser : IExprementVisitor2, IExprementVisitor, IExprementVisitor3590
	{
		AccessFlag TopOfStack { get; }

		bool Abort { get; set; }

		bool ImplicitOn { get; }

		bool CheckLicenseOperatorFound { get; }

		void Reset(IExprementVisitorNoTraversion exp);

		void Push(AccessFlag access);

		void Pop();
	}
}
