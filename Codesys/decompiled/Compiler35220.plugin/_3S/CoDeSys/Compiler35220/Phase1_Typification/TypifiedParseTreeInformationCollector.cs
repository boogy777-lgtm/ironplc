using System;
using \u0008;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000336 RID: 822
	public class TypifiedParseTreeInformationCollector : SimpleGreenTreeVisitor
	{
		// Token: 0x060031B5 RID: 12725 RVA: 0x000BFF84 File Offset: 0x000BE184
		public static void CollectParseTreeInformation(_IExprement exp, CompactedTypifiedParseTreeInformation info)
		{
			TypifiedParseTreeInformationCollector u = new TypifiedParseTreeInformationCollector(info);
			\u0001 ivisit = new \u0001
			{
				Visitor = u
			};
			exp.Accept(ivisit);
		}

		// Token: 0x060031B6 RID: 12726 RVA: 0x000BFFAC File Offset: 0x000BE1AC
		private TypifiedParseTreeInformationCollector(CompactedTypifiedParseTreeInformation info)
		{
			this.\u0001 = info;
		}

		// Token: 0x060031B7 RID: 12727 RVA: 0x000BFFBC File Offset: 0x000BE1BC
		private void \u0001(_IExprement \u0002, int \u0003)
		{
			if (\u0002.MessagesList != null && \u0002.MessagesList.Count > 0)
			{
				this.\u0001.MessageTable[\u0003] = \u0002.MessagesList;
			}
		}

		// Token: 0x060031B8 RID: 12728 RVA: 0x000BFFEC File Offset: 0x000BE1EC
		private void \u0002(_IExprement \u0002, int \u0003)
		{
			_IExpression iexpression = \u0002 as _IExpression;
			if (iexpression != null)
			{
				if (iexpression.SignatureId < 0 && iexpression.VariableId < 0)
				{
					if (iexpression._CompiledType != null)
					{
						this.\u0001.TypeInfoTable[\u0003] = iexpression._CompiledType;
						return;
					}
				}
				else
				{
					this.\u0001.TypeInfoTable[\u0003] = new CompiledExpressionTypeInfo(iexpression.SignatureId, iexpression.VariableId, (_IType)iexpression._CompiledType);
				}
			}
		}

		// Token: 0x060031B9 RID: 12729 RVA: 0x000C0064 File Offset: 0x000BE264
		private void \u0003(_IExprement \u0002, int \u0003)
		{
			_IPragmaIfStatement ipragmaIfStatement = \u0002 as _IPragmaIfStatement;
			if (ipragmaIfStatement == null)
			{
				return;
			}
			_IPragmaExpression ipragmaExpression = ipragmaIfStatement.Condition as _IPragmaExpression;
			if (ipragmaExpression == null)
			{
				this.\u0001.ConditionalPragmaValueTable.Add(\u0003, -1);
				return;
			}
			if (ipragmaExpression.ValueStillUndecided)
			{
				this.\u0001.ConditionalPragmaValueTable.Add(\u0003, -1);
				return;
			}
			int num = -1;
			if (ipragmaExpression.Value)
			{
				num = 0;
			}
			else
			{
				for (int i = 0; i < ipragmaIfStatement.ElseIfs.Length; i++)
				{
					_IPragmaExpression ipragmaExpression2 = (ipragmaIfStatement.ElseIfs[i] as _IElseIf).Condition as _IPragmaExpression;
					if (ipragmaExpression2 == null)
					{
						this.\u0001.ConditionalPragmaValueTable.Add(\u0003, -1);
						return;
					}
					if (ipragmaExpression2.Value)
					{
						num = i + 1;
						break;
					}
				}
				if (num == -1)
				{
					num = ipragmaIfStatement.ElseIfs.Length + 1;
				}
			}
			this.\u0001.ConditionalPragmaValueTable.Add(\u0003, num);
		}

		// Token: 0x060031BA RID: 12730 RVA: 0x000C013C File Offset: 0x000BE33C
		public void HandleVariableExprAccessFlag(_IExprement exp, int nExprementId)
		{
			if (exp is _IVariableExpression)
			{
				_IVariableExpression ivariableExpression = exp as _IVariableExpression;
				VarExprFlag varExprFlag = VarExprFlag.None;
				if (ivariableExpression.GetFlag(VarExprFlag.WriteAccess))
				{
					varExprFlag |= VarExprFlag.WriteAccess;
				}
				if (ivariableExpression.GetFlag(VarExprFlag.InitializingWriteAccess))
				{
					varExprFlag |= VarExprFlag.InitializingWriteAccess;
				}
				this.\u0001.VarExprFlagTable.Add(nExprementId, varExprFlag);
			}
		}

		// Token: 0x060031BB RID: 12731 RVA: 0x000C0184 File Offset: 0x000BE384
		public void HandleAssignmentExprInfo(_IExprement exp, int nExprementId)
		{
			if (exp is _IAssignmentExpression)
			{
				_IAssignmentExpression iassignmentExpression = exp as _IAssignmentExpression;
				if (iassignmentExpression.Info is IPropertyAssignmentExprInfo)
				{
					if (this.\u0001.AssignmentExprInfoTable == null)
					{
						this.\u0001.AssignmentExprInfoTable = new LDictionary<int, IPropertyAssignmentExprInfo>();
					}
					this.\u0001.AssignmentExprInfoTable.Add(nExprementId, iassignmentExpression.Info as IPropertyAssignmentExprInfo);
				}
			}
		}

		// Token: 0x060031BC RID: 12732 RVA: 0x000C01E8 File Offset: 0x000BE3E8
		public void HandleNewExpressionTypeToCast(_IExprement exp, int nExprementId)
		{
			if (exp is _INewExpression)
			{
				_INewExpression inewExpression = exp as _INewExpression;
				if (this.\u0001.NewExpressionTypeToCastTable == null)
				{
					this.\u0001.NewExpressionTypeToCastTable = new LDictionary<int, _IType>();
				}
				this.\u0001.NewExpressionTypeToCastTable.Add(nExprementId, inewExpression._TypeToCast);
			}
		}

		// Token: 0x060031BD RID: 12733 RVA: 0x000C0238 File Offset: 0x000BE438
		public override void HandleExprement(_IExprement exp, int nExprementId)
		{
			this.\u0001(exp, nExprementId);
			this.\u0002(exp, nExprementId);
			this.\u0003(exp, nExprementId);
			this.HandleVariableExprAccessFlag(exp, nExprementId);
			this.HandleAssignmentExprInfo(exp, nExprementId);
			this.HandleNewExpressionTypeToCast(exp, nExprementId);
		}

		// Token: 0x04000963 RID: 2403
		private readonly CompactedTypifiedParseTreeInformation \u0001;
	}
}
