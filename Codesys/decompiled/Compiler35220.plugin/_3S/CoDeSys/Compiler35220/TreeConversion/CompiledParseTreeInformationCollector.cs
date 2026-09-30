using System;
using \u0008;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x0200001C RID: 28
	public class CompiledParseTreeInformationCollector : SimpleGreenTreeVisitor
	{
		// Token: 0x06000548 RID: 1352 RVA: 0x0000AAE0 File Offset: 0x00008CE0
		public static void CollectParseTreeInformation(_IExprement exp, ICompactedCompiledParseTreeInformation info)
		{
			CompiledParseTreeInformationCollector compiledParseTreeInformationCollector = new CompiledParseTreeInformationCollector(info);
			\u0001 ivisit = new \u0001
			{
				Visitor = compiledParseTreeInformationCollector
			};
			exp.Accept(ivisit);
			info.SourcePosTable = compiledParseTreeInformationCollector.\u0001.ToArray();
			info.LengthTable = compiledParseTreeInformationCollector.\u0001.ToArray();
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x0000AB2C File Offset: 0x00008D2C
		private CompiledParseTreeInformationCollector(ICompactedCompiledParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0000AB54 File Offset: 0x00008D54
		private void \u0001(_IExprement \u0002, int \u0003)
		{
			IMinimalPosition position = \u0002._Position;
			while (this.\u0001.Count <= \u0003)
			{
				this.\u0001.Add(-1L);
			}
			while (this.\u0001.Count <= \u0003)
			{
				this.\u0001.Add(-1);
			}
			if (\u0002 is IPositionExprement && position != null)
			{
				long num = PositionHelper.CombinePosition(position.EditorPosition, position.PositionOffset);
				this.\u0001[\u0003] = num;
			}
			if (\u0002 is ILengthExprement)
			{
				this.\u0001[\u0003] = \u0002.LengthIntern;
			}
		}

		// Token: 0x0600054B RID: 1355 RVA: 0x0000ABE8 File Offset: 0x00008DE8
		private void \u0002(_IExprement \u0002, int \u0003)
		{
			if (\u0002.MessagesList != null && \u0002.MessagesList.Count > 0)
			{
				this.\u0001.MessageTable[\u0003] = \u0002.MessagesList;
			}
		}

		// Token: 0x0600054C RID: 1356 RVA: 0x0000AC18 File Offset: 0x00008E18
		private void \u0003(_IExprement \u0002, int \u0003)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				this.\u0001.TypeInfoTable[\u0003] = new CompiledExpressionTypeInfo(iexpression.SignatureId, iexpression.VariableId, (_IType)iexpression._CompiledType);
			}
		}

		// Token: 0x0600054D RID: 1357 RVA: 0x0000AC5C File Offset: 0x00008E5C
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			this.\u0001(exp, nExprementId);
			this.\u0002(exp, nExprementId);
			this.\u0003(exp, nExprementId);
		}

		// Token: 0x04000043 RID: 67
		private readonly ICompactedCompiledParseTreeInformation \u0001;

		// Token: 0x04000044 RID: 68
		private readonly LList<long> \u0001 = new LList<long>();

		// Token: 0x04000045 RID: 69
		private readonly LList<short> \u0001 = new LList<short>();
	}
}
