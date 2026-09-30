using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CODESYS.WhiteParseTrees.Factories;
using CODESYS.WhiteParseTrees.Nodes.Expressions;
using CODESYS.WhiteParseTrees.Nodes.Statements;

namespace CODESYS.WhiteParseTrees.Nodes.Factories.Builder
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class CaseLabelBuilder : ISwitchCaseLabelBuilder, IStatementBuilder<IWhiteCaseLabelStatement>, ICaseExpressionListStep
	{
		[System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		[field: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
		private IEnumerable<IWhiteExpression> CaseExpressionList
		{
			[return: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			get;
			[param: System.Runtime.CompilerServices.Nullable(new byte[] { 2, 1 })]
			set;
		}

		private IColonToken Colon { get; set; }

		public CaseLabelBuilder()
		{
			Colon = TokenFactory<IColonToken>.Create(":");
		}

		public IWhiteCaseLabelStatement Build()
		{
			if (CaseExpressionList == null)
			{
				throw new BuilderException("CaseExpressionList not assigned");
			}
			return new WhiteCaseLabelStatement(CaseExpressionList, Colon);
		}

		public ISwitchCaseLabelBuilder WithCaseExpressionList(IList<IWhiteExpression> caseExpressionList)
		{
			for (int i = 0; i < caseExpressionList.Count; i++)
			{
				IWhiteExpression whiteExpression = caseExpressionList[i];
				if (i != 0 && !(whiteExpression is ILeadByCommaExpression))
				{
					caseExpressionList[i] = new WhiteLeadByCommaExpression(TokenFactory<ICommaToken>.Create(","), whiteExpression);
				}
			}
			CaseExpressionList = caseExpressionList;
			return this;
		}
	}
}
