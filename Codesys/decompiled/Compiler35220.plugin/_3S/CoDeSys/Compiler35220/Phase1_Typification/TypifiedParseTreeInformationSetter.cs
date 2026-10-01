using System;
using \u0008;
using \u000E;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000337 RID: 823
	public class TypifiedParseTreeInformationSetter : SimpleGreenTreeVisitor
	{
		// Token: 0x060031BE RID: 12734 RVA: 0x000C026C File Offset: 0x000BE46C
		public static void SetInformationInParseTree(_IExprement exp, CompactedTypifiedParseTreeInformation info)
		{
			TypifiedParseTreeInformationSetter u = new TypifiedParseTreeInformationSetter(info);
			global::\u0008.\u0001 ivisit = new global::\u0008.\u0001
			{
				Visitor = u
			};
			exp.Accept(ivisit);
		}

		// Token: 0x060031BF RID: 12735 RVA: 0x000C0294 File Offset: 0x000BE494
		private TypifiedParseTreeInformationSetter(CompactedTypifiedParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x060031C0 RID: 12736 RVA: 0x000C02A4 File Offset: 0x000BE4A4
		private void \u0001(_IExprement \u0002, int \u0003)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			object obj;
			if (iexpression != null && this.\u0001.TypeInfoTable != null && this.\u0001.TypeInfoTable.TryGetValue(\u0003, out obj))
			{
				if (obj is ICompiledExpressionTypeInfo)
				{
					ICompiledExpressionTypeInfo compiledExpressionTypeInfo = obj as ICompiledExpressionTypeInfo;
					iexpression._CompiledType = compiledExpressionTypeInfo.CompiledType;
					if (iexpression is _IVariableExpression)
					{
						iexpression.SignatureId = compiledExpressionTypeInfo.SignatureId;
						iexpression.VariableId = compiledExpressionTypeInfo.VariableId;
						return;
					}
				}
				else
				{
					iexpression._CompiledType = (obj as _IType);
				}
			}
		}

		// Token: 0x060031C1 RID: 12737 RVA: 0x000C0328 File Offset: 0x000BE528
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			global::\u000E.\u0001.\u0002(exp, nExprementId, this.\u0001.MessageTable);
			this.\u0001(exp, nExprementId);
			this.\u0002(exp, nExprementId);
			this.HandleVariableExprAccessFlag(exp, nExprementId);
			this.HandleAssignmentExprInfo(exp, nExprementId);
			this.HandleNewExpressionTypeToCast(exp, nExprementId);
		}

		// Token: 0x060031C2 RID: 12738 RVA: 0x000C0364 File Offset: 0x000BE564
		private void \u0002(_IExprement \u0002, int \u0003)
		{
			if (\u0002 is _IPragmaIfStatement)
			{
				int num = this.\u0001.ConditionalPragmaValueTable[\u0003];
				_IPragmaIfStatement ipragmaIfStatement = \u0002 as _IPragmaIfStatement;
				_IPragmaExpression ipragmaExpression = ipragmaIfStatement.Condition as _IPragmaExpression;
				if (ipragmaExpression == null)
				{
					return;
				}
				if (num < 0)
				{
					ipragmaExpression.ValueStillUndecided = true;
				}
				else
				{
					ipragmaExpression.Value = (num == 0);
				}
				for (int i = 0; i < ipragmaIfStatement.ElseIfs.Length; i++)
				{
					_IPragmaExpression ipragmaExpression2 = (ipragmaIfStatement.ElseIfs[i] as _IElseIf).Condition as _IPragmaExpression;
					if (ipragmaExpression2 != null)
					{
						if (num < 0)
						{
							ipragmaExpression2.ValueStillUndecided = true;
						}
						else
						{
							ipragmaExpression2.Value = (num == i + 1);
						}
					}
				}
			}
		}

		// Token: 0x060031C3 RID: 12739 RVA: 0x000C0408 File Offset: 0x000BE608
		public void HandleVariableExprAccessFlag(_IExprement exp, int nExprementId)
		{
			if (exp is _IVariableExpression)
			{
				VarExprFlag vfFlag = this.\u0001.VarExprFlagTable[nExprementId];
				(exp as _IVariableExpression).SetFlag(vfFlag, true);
			}
		}

		// Token: 0x060031C4 RID: 12740 RVA: 0x000C043C File Offset: 0x000BE63C
		public void HandleAssignmentExprInfo(_IExprement exp, int nExprementId)
		{
			if (exp is _IAssignmentExpression && this.\u0001.AssignmentExprInfoTable != null)
			{
				_IAssignmentExpression iassignmentExpression = exp as _IAssignmentExpression;
				if (this.\u0001.AssignmentExprInfoTable.ContainsKey(nExprementId))
				{
					iassignmentExpression.Info = this.\u0001.AssignmentExprInfoTable[nExprementId];
				}
			}
		}

		// Token: 0x060031C5 RID: 12741 RVA: 0x000C0490 File Offset: 0x000BE690
		public void HandleNewExpressionTypeToCast(_IExprement exp, int nExprementId)
		{
			if (exp is _INewExpression)
			{
				(exp as _INewExpression)._TypeToCast = this.\u0001.NewExpressionTypeToCastTable[nExprementId];
			}
		}

		// Token: 0x04000964 RID: 2404
		private readonly CompactedTypifiedParseTreeInformation \u0001;
	}
}
