using System;
using System.Runtime.CompilerServices;
using \u0011;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0016
{
	// Token: 0x0200012C RID: 300
	internal sealed class \u0006 : ILMTransitionUserCodeAnalyzerService
	{
		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x0600158E RID: 5518 RVA: 0x0003E830 File Offset: 0x0003CA30
		internal static \u0006 Singleton { get; } = new \u0006();

		// Token: 0x0600158F RID: 5519 RVA: 0x0003E838 File Offset: 0x0003CA38
		private \u0006()
		{
		}

		// Token: 0x06001590 RID: 5520 RVA: 0x0003E840 File Offset: 0x0003CA40
		public ITransitionUserCodeAnalyzationResult \u0001(string \u0002, string \u0003)
		{
			ISequenceStatement2 sequenceStatement = ((ILanguageModelBuilder8)APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder()).ParseSTSnippet(\u0002, true);
			\u0005 u = new \u0005(sequenceStatement, \u0002, \u0003);
			sequenceStatement.AcceptVisitor(u);
			return new \u0004
			{
				StringForAnalyzation = u.StringForAnalyzation,
				HasExplicitTransitionAssignment = (u.ExplicitTransitionAssignment != null),
				SingleExpression = u.SingleExpression,
				CountStatements = u.TopLevelStatements.Length
			};
		}

		// Token: 0x06001591 RID: 5521 RVA: 0x0003E8B8 File Offset: 0x0003CAB8
		internal \u0004 \u0001(ISequenceStatement2 \u0002, string \u0003)
		{
			\u0005 u = new \u0005(\u0002, null, \u0003);
			\u0002.AcceptVisitor(u);
			return new \u0004
			{
				StringForAnalyzation = u.StringForAnalyzation,
				HasExplicitTransitionAssignment = (u.ExplicitTransitionAssignment != null),
				SingleExpression = u.SingleExpression,
				CountStatements = u.TopLevelStatements.Length,
				OwningSequenceStatement = u.OwningSequenceStatement,
				StatementPosition = u.StatementPosition
			};
		}

		// Token: 0x040003BC RID: 956
		[CompilerGenerated]
		private static readonly \u0006 \u0001;
	}
}
