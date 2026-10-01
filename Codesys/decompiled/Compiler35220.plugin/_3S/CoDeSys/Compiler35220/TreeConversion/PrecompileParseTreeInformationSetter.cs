using System;
using \u0008;
using \u000E;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x0200001D RID: 29
	public class PrecompileParseTreeInformationSetter : SimpleGreenTreeVisitor
	{
		// Token: 0x0600054E RID: 1358 RVA: 0x0000AC78 File Offset: 0x00008E78
		public static void SetInformationInParseTree(_IExprement exp, ICompactedParseTreeInformation info)
		{
			PrecompileParseTreeInformationSetter u = new PrecompileParseTreeInformationSetter(info);
			global::\u0008.\u0001 ivisit = new global::\u0008.\u0001
			{
				Visitor = u
			};
			exp.Accept(ivisit);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0000ACA0 File Offset: 0x00008EA0
		private PrecompileParseTreeInformationSetter(ICompactedParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x0000ACB0 File Offset: 0x00008EB0
		private void \u0001(_IExprement \u0002, int \u0003)
		{
			if (\u0002 is _IVariableExpression && this.\u0001.TypeInfoTable != null)
			{
				_IVariableExpression ivariableExpression = \u0002 as _IVariableExpression;
				IPrecompileTypeInfo precompileTypeInfo;
				if (this.\u0001.TypeInfoTable.TryGetValue(\u0003, out precompileTypeInfo))
				{
					ivariableExpression.PrecompileSignatureId = precompileTypeInfo.PrecompileSignatureId;
					ivariableExpression.PrecompileVariableId = precompileTypeInfo.PrecompileVariableId;
				}
			}
			if (\u0002 is _INewExpression)
			{
				_IUserdefType iuserdefType = (\u0002 as _INewExpression)._TypeToCast as _IUserdefType;
				IPrecompileTypeInfo precompileTypeInfo2;
				if (this.\u0001.TypeInfoTable.TryGetValue(\u0003, out precompileTypeInfo2))
				{
					Debug.\u0001(iuserdefType != null);
					iuserdefType.SignatureId = precompileTypeInfo2.PrecompileSignatureId;
					(iuserdefType.NameExpression as _IExpression).PrecompileSignatureId = iuserdefType.SignatureId;
				}
			}
			if (\u0002 is _IHasTypeExpression)
			{
				_IUserdefType iuserdefType2 = (\u0002 as _IHasTypeExpression).ReferencedType as _IUserdefType;
				IPrecompileTypeInfo precompileTypeInfo3;
				if (iuserdefType2 != null && this.\u0001.TypeInfoTable.TryGetValue(\u0003, out precompileTypeInfo3))
				{
					Debug.\u0001(iuserdefType2 != null);
					iuserdefType2.SignatureId = precompileTypeInfo3.PrecompileSignatureId;
					(iuserdefType2.NameExpression as _IExpression).PrecompileSignatureId = iuserdefType2.SignatureId;
				}
			}
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x0000ADC4 File Offset: 0x00008FC4
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			global::\u000E.\u0001.\u0001(exp, nExprementId, this.\u0001.SourcePosTable, this.\u0001.LengthTable);
			global::\u000E.\u0001.\u0001(exp, nExprementId, this.\u0001.MessageTable);
			global::\u000E.\u0001.\u0001(exp, nExprementId, (this.\u0001 as ICompactedParseTreeInformation2).StatementFlagTable);
			this.\u0001(exp, nExprementId);
		}

		// Token: 0x04000046 RID: 70
		private readonly ICompactedParseTreeInformation \u0001;
	}
}
