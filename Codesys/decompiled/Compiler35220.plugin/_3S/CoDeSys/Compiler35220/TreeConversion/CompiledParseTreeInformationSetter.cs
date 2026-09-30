using System;
using \u0008;
using \u000E;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.TreeConversion
{
	// Token: 0x0200001E RID: 30
	public class CompiledParseTreeInformationSetter : SimpleGreenTreeVisitor
	{
		// Token: 0x06000552 RID: 1362 RVA: 0x0000AE20 File Offset: 0x00009020
		public static void SetInformationInParseTree(_IExprement exp, ICompactedCompiledParseTreeInformation info)
		{
			CompiledParseTreeInformationSetter u = new CompiledParseTreeInformationSetter(info);
			global::\u0008.\u0001 ivisit = new global::\u0008.\u0001
			{
				Visitor = u
			};
			exp.Accept(ivisit);
		}

		// Token: 0x06000553 RID: 1363 RVA: 0x0000AE48 File Offset: 0x00009048
		private CompiledParseTreeInformationSetter(ICompactedCompiledParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x06000554 RID: 1364 RVA: 0x0000AE58 File Offset: 0x00009058
		private void \u0001(_IExprement \u0002, int \u0003)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			ICompiledExpressionTypeInfo compiledExpressionTypeInfo;
			if (iexpression != null && this.\u0001.TypeInfoTable != null && this.\u0001.TypeInfoTable.TryGetValue(\u0003, out compiledExpressionTypeInfo))
			{
				iexpression._CompiledType = compiledExpressionTypeInfo.CompiledType;
				if (iexpression is _IVariableExpression)
				{
					iexpression.SignatureId = compiledExpressionTypeInfo.SignatureId;
					iexpression.VariableId = compiledExpressionTypeInfo.VariableId;
				}
			}
		}

		// Token: 0x06000555 RID: 1365 RVA: 0x0000AEC0 File Offset: 0x000090C0
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			global::\u000E.\u0001.\u0002(exp, nExprementId, this.\u0001.SourcePosTable, this.\u0001.LengthTable);
			global::\u000E.\u0001.\u0002(exp, nExprementId, this.\u0001.MessageTable);
			this.\u0001(exp, nExprementId);
		}

		// Token: 0x04000047 RID: 71
		private readonly ICompactedCompiledParseTreeInformation \u0001;
	}
}
