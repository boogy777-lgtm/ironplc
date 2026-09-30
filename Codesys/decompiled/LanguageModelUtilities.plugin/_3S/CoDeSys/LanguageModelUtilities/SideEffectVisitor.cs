using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal sealed class SideEffectVisitor : EmptyVisitor
	{
		internal bool HasSideEffects { get; private set; }

		public override void visit(IAssignmentExpression assign)
		{
			HasSideEffects = true;
			base.Traverser.Abort = true;
		}

		public override void visit(ICallExpression call)
		{
			HasSideEffects = true;
			base.Traverser.Abort = true;
		}

		public override void visit(INewExpression newexp)
		{
			HasSideEffects = true;
			base.Traverser.Abort = true;
		}
	}
}
