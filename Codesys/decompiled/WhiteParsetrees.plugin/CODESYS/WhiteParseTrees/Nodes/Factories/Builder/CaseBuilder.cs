using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class CaseBuilder : ISwitchCaseBuilder, IStatementBuilder<IWhiteCaseStatement>, ISwitchCaseStep, ISwitchCaseElseStep, ISwithEndCaseStep, ISwithAddCaseStep
	{
		private ICaseToken Case { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteExpression Switch
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IOfToken Of { get; set; }

		private IEnumerable<IWhiteCase> Cases { get; set; }

		private IElseToken ElseToken { get; set; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		private IWhiteSequenceStatement Else
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		private IEndCaseToken EndCase { get; set; }

		public CaseBuilder()
		{
			Case = TokenFactory<ICaseToken>.Create("CASE");
			Of = TokenFactory<IOfToken>.Create("OF");
			ElseToken = TokenFactory<IElseToken>.Create("ELSE");
			EndCase = TokenFactory<IEndCaseToken>.Create("END_CASE");
			Cases = new List<IWhiteCase>();
		}

		public IWhiteCaseStatement Build()
		{
			IElseToken elsetoken = ((Else != null) ? ElseToken : null);
			if (Switch == null)
			{
				throw new BuilderException("Switch not assigned");
			}
			return new WhiteCaseStatement(Case, Switch, Of, Cases, elsetoken, Else, EndCase);
		}

		public ISwithAddCaseStep WithSwitch(IWhiteExpression switchStatement)
		{
			Switch = switchStatement;
			return this;
		}

		public ISwithEndCaseStep AddCase(IWhiteCase switchCase)
		{
			((List<IWhiteCase>)Cases).Add(switchCase);
			return this;
		}

		public ISwitchCaseElseStep EndCases()
		{
			return this;
		}

		public ISwitchCaseBuilder EndCasesWithoutElse()
		{
			return this;
		}

		public ISwitchCaseBuilder WithElse(IWhiteSequenceStatement switchElse)
		{
			Else = switchElse;
			return this;
		}
	}
}
