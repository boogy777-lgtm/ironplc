using System;
using \u0008;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x0200001B RID: 27
	public class PrecompileParseTreeInformationCollector : SimpleGreenTreeVisitor
	{
		// Token: 0x06000541 RID: 1345 RVA: 0x0000A874 File Offset: 0x00008A74
		public static void CollectParseTreeInformation(_IExprement exp, ICompactedParseTreeInformation info)
		{
			PrecompileParseTreeInformationCollector precompileParseTreeInformationCollector = new PrecompileParseTreeInformationCollector(info);
			\u0001 ivisit = new \u0001
			{
				Visitor = precompileParseTreeInformationCollector
			};
			exp.Accept(ivisit);
			info.SourcePosTable = precompileParseTreeInformationCollector.\u0001.ToArray();
			info.LengthTable = precompileParseTreeInformationCollector.\u0001.ToArray();
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x0000A8C0 File Offset: 0x00008AC0
		private PrecompileParseTreeInformationCollector(ICompactedParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x0000A8E8 File Offset: 0x00008AE8
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

		// Token: 0x06000544 RID: 1348 RVA: 0x0000A97C File Offset: 0x00008B7C
		private void \u0002(_IExprement \u0002, int \u0003)
		{
			if (\u0002.MessagesList != null && \u0002.MessagesList.Count > 0)
			{
				this.\u0001.MessageTable[\u0003] = \u0002.MessagesList;
			}
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x0000A9AC File Offset: 0x00008BAC
		private void \u0003(_IExprement \u0002, int \u0003)
		{
			if (\u0002 is _IVariableExpression)
			{
				_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
				if (ivariableExpression.PrecompileSignatureId >= 0 || ivariableExpression.PrecompileVariableId >= 0)
				{
					this.\u0001.TypeInfoTable[\u0003] = new PrecompileExpressionTypeInfo(ivariableExpression.PrecompileSignatureId, ivariableExpression.PrecompileVariableId);
				}
			}
			if (\u0002 is _INewExpression)
			{
				_IUserdefType iuserdefType = (\u0002 as _INewExpression)._TypeToCast as _IUserdefType;
				if (iuserdefType != null && iuserdefType.SignatureId >= 0)
				{
					this.\u0001.TypeInfoTable[\u0003] = new PrecompileExpressionTypeInfo(iuserdefType.SignatureId, -1);
				}
			}
			if (\u0002 is _IHasTypeExpression)
			{
				_IUserdefType iuserdefType2 = (\u0002 as _IHasTypeExpression).ReferencedType as _IUserdefType;
				if (iuserdefType2 != null && iuserdefType2.SignatureId >= 0)
				{
					this.\u0001.TypeInfoTable[\u0003] = new PrecompileExpressionTypeInfo(iuserdefType2.SignatureId, -1);
				}
			}
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x0000AA80 File Offset: 0x00008C80
		private void \u0004(_IExprement \u0002, int \u0003)
		{
			_IStatement2 istatement = \u0002 as _IStatement2;
			if (istatement != null && istatement.Flags != (StatementFlag)0L)
			{
				(this.\u0001 as ICompactedParseTreeInformation2).StatementFlagTable[\u0003] = istatement.Flags;
			}
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x0000AABC File Offset: 0x00008CBC
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			this.\u0001(exp, nExprementId);
			this.\u0002(exp, nExprementId);
			this.\u0003(exp, nExprementId);
			this.\u0004(exp, nExprementId);
		}

		// Token: 0x04000040 RID: 64
		private readonly ICompactedParseTreeInformation \u0001;

		// Token: 0x04000041 RID: 65
		private readonly LList<long> \u0001 = new LList<long>();

		// Token: 0x04000042 RID: 66
		private readonly LList<short> \u0001 = new LList<short>();
	}
}
