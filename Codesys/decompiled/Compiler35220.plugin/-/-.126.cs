using System;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x02000176 RID: 374
	internal sealed class \u0007 : EmptyVisitor351900
	{
		// Token: 0x0600195C RID: 6492 RVA: 0x0004EDD0 File Offset: 0x0004CFD0
		private \u0007(MacroInfoProvider \u0081\u0003)
		{
			this.\u0001 = new \u001D.\u0003(this);
			this.\u0001 = \u0081\u0003;
		}

		// Token: 0x0600195D RID: 6493 RVA: 0x0004EDEC File Offset: 0x0004CFEC
		public static void \u0001(ISequenceStatement \u0002, MacroInfoProvider \u0003)
		{
			global::\u0008.\u0007 u = new global::\u0008.\u0007(\u0003);
			((_ISequenceStatement)\u0002).Accept(u.\u0001);
		}

		// Token: 0x0600195E RID: 6494 RVA: 0x0004EE14 File Offset: 0x0004D014
		private bool \u0001(Operator \u0002, out string \u0003)
		{
			if (\u0002 == Operator.__PouName)
			{
				\u0003 = this.\u0001.Name;
				return true;
			}
			if (\u0002 != Operator.__Position)
			{
				\u0003 = "";
				return false;
			}
			_IStatement istatement = this.\u0001.CurrentStatement;
			if (((istatement != null) ? istatement.Position : null) != null)
			{
				\u0003 = this.\u0001.GetPositionText(this.\u0001.CurrentStatement.Position.PositionCombination);
			}
			else
			{
				\u0003 = "";
			}
			return true;
		}

		// Token: 0x0600195F RID: 6495 RVA: 0x0004EE98 File Offset: 0x0004D098
		public override void visit(_IOperatorExpression op)
		{
			string text;
			if (this.\u0001(op.Code, out text))
			{
				_ILiteralExpression iliteralExpression = \u0019.\u0003.\u0001(text ?? string.Empty, TypeClass.String);
				iliteralExpression.PositionIntern = op.PositionIntern;
				iliteralExpression.LengthIntern = op.LengthIntern;
				this.\u0001.\u0001(iliteralExpression);
			}
		}

		// Token: 0x04000471 RID: 1137
		private readonly \u001D.\u0003 \u0001;

		// Token: 0x04000472 RID: 1138
		private readonly MacroInfoProvider \u0001;
	}
}
