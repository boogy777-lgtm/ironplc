using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0008;
using \u0019;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Messaging
{
	// Token: 0x0200039B RID: 923
	internal sealed class PragmaVisitor : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x170008C9 RID: 2249
		// (get) Token: 0x0600357E RID: 13694 RVA: 0x000D680C File Offset: 0x000D4A0C
		// (set) Token: 0x0600357F RID: 13695 RVA: 0x000D6814 File Offset: 0x000D4A14
		public List<ISourcePosition> UnusedStatementPositions { get; private set; }

		// Token: 0x06003580 RID: 13696 RVA: 0x000D6820 File Offset: 0x000D4A20
		public PragmaVisitor(_IPreCompileContext precom, string stCompilerDefines, string name)
		{
			this.\u0001 = name;
			this.\u0001 = precom;
			foreach (object obj in this.\u0001.DefineTable.Keys)
			{
				string text = (string)obj;
				this.\u0001[text] = (this.\u0001.DefineTable[text] as string);
			}
			if (!string.IsNullOrEmpty(stCompilerDefines))
			{
				global::\u0008.\u0005.\u0001(stCompilerDefines, this.\u0001);
			}
			this.ConditionalCompilationAllowed = true;
		}

		// Token: 0x06003581 RID: 13697 RVA: 0x000D68E4 File Offset: 0x000D4AE4
		public void \u0001(_ISequenceStatement \u0002)
		{
			if (this.\u0001)
			{
				return;
			}
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				_IExprement iexprement = statementList[i];
				this.\u0001.Push(null);
				iexprement.Accept(this);
				if (this.\u0001.Peek() != null)
				{
					\u0002.Replace(this.\u0001.Peek(), i);
				}
				this.\u0001.Pop();
			}
		}

		// Token: 0x06003582 RID: 13698 RVA: 0x000D6958 File Offset: 0x000D4B58
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			if (\u0002.Condition is IErrorExpression)
			{
				IList<_ICompilerMessage> messagesList = \u0002.Condition.MessagesList;
				if (messagesList != null && messagesList.Count > 0)
				{
					return;
				}
			}
			if (!this.ConditionalCompilationAllowed && !PragmaVisitor.\u0001(\u0002.Condition))
			{
				this.\u0001(\u0002.Condition, MessageId.Err_NotSupportedInInterface);
				return;
			}
			\u0002.Condition.Accept(this);
			bool flag;
			if (PragmaVisitor.\u0002(\u0002.Condition))
			{
				flag = this.\u0001(\u0002.IfThen);
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					flag |= this.\u0001(ipragmaElseIf.Controlled);
				}
				if (\u0002.IfElse != null)
				{
					flag |= this.\u0001(\u0002.IfElse);
				}
			}
			else
			{
				flag = false;
			}
			if (!flag)
			{
				this.\u0003(\u0002);
			}
		}

		// Token: 0x06003583 RID: 13699 RVA: 0x000D6A4C File Offset: 0x000D4C4C
		public void \u0001(_ICompiledPOU \u0002)
		{
			_IStatement parseTree = \u0002.GetParseTree();
			if (parseTree != null)
			{
				parseTree.Accept(this);
			}
		}

		// Token: 0x06003584 RID: 13700 RVA: 0x000D6A6C File Offset: 0x000D4C6C
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06003585 RID: 13701 RVA: 0x000D6A7C File Offset: 0x000D4C7C
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06003586 RID: 13702 RVA: 0x000D6A8C File Offset: 0x000D4C8C
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06003587 RID: 13703 RVA: 0x000D6A9C File Offset: 0x000D4C9C
		public void \u0001(_ICaseStatement \u0002)
		{
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06003588 RID: 13704 RVA: 0x000D6B00 File Offset: 0x000D4D00
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06003589 RID: 13705 RVA: 0x000D6B70 File Offset: 0x000D4D70
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
			_IVariableDeclarationListStatement u = this.\u0001;
			this.\u0001 = \u0002;
			\u0002.VariableDeclaration.Accept(this);
			this.\u0001 = u;
		}

		// Token: 0x0600358A RID: 13706 RVA: 0x000D6BA0 File Offset: 0x000D4DA0
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
			this.\u0001 = \u0002.Name;
			\u0002.Declarations.Accept(this);
		}

		// Token: 0x0600358B RID: 13707 RVA: 0x000D6BBC File Offset: 0x000D4DBC
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
			this.\u0001 = \u0002.Name;
			\u0002.Declarations.Accept(this);
		}

		// Token: 0x0600358C RID: 13708 RVA: 0x000D6BD8 File Offset: 0x000D4DD8
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
			foreach (_IEnumDeclarationStatement ienumDeclarationStatement in \u0002.Enums)
			{
				ienumDeclarationStatement.Accept(this);
			}
		}

		// Token: 0x0600358D RID: 13709 RVA: 0x000D6C24 File Offset: 0x000D4E24
		public void \u0001(_IDefineReference \u0002)
		{
			\u0002.Value = this.\u0001.ContainsKey(\u0002.Define);
		}

		// Token: 0x0600358E RID: 13710 RVA: 0x000D6C40 File Offset: 0x000D4E40
		public void \u0001(_IVariableReference \u0002)
		{
			\u0002.Value = false;
			SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(new CheckerScope(null, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool));
			\u0002.InstancePath.Accept(simpleTypeInferrer);
			if (simpleTypeInferrer.DerivedVariable != null)
			{
				\u0002.Value = true;
			}
		}

		// Token: 0x0600358F RID: 13711 RVA: 0x000D6C90 File Offset: 0x000D4E90
		public void \u0001(_ITypeReference \u0002)
		{
			SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(new CheckerScope(null, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool));
			\u0002.InstancePath.Accept(simpleTypeInferrer);
			if (simpleTypeInferrer.DerivedSignature != null)
			{
				\u0002.Value = true;
			}
		}

		// Token: 0x06003590 RID: 13712 RVA: 0x000D6CDC File Offset: 0x000D4EDC
		public void \u0001(_IPouReference \u0002)
		{
			\u0002.InstancePath.Accept(this);
			\u0002.Value = false;
			IPrecompileScope2 precompileScope = this.\u0001.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2;
			if (precompileScope != null)
			{
				ISignature signature = precompileScope.FindSignatureGlobal(\u0002.InstancePath);
				if (signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.Function || signature.POUType == Operator.FunctionBlock || Operator.Interface == signature.POUType))
				{
					\u0002.Value = true;
				}
			}
		}

		// Token: 0x06003591 RID: 13713 RVA: 0x000D6D54 File Offset: 0x000D4F54
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06003592 RID: 13714 RVA: 0x000D6D58 File Offset: 0x000D4F58
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06003593 RID: 13715 RVA: 0x000D6D5C File Offset: 0x000D4F5C
		public void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
			_IPragmaExpression ipragmaExpression = \u0002.ItemReference as _IPragmaExpression;
			if (ipragmaExpression != null)
			{
				\u0002.Value = ipragmaExpression.Value;
			}
		}

		// Token: 0x06003594 RID: 13716 RVA: 0x000D6D90 File Offset: 0x000D4F90
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			if (!\u0002.Operands.All(new Func<_IExpression, bool>(PragmaVisitor.<>c.<>9.\u0001)))
			{
				\u0002.Value = false;
				return;
			}
			bool[] array = new bool[\u0002.Operands.Count];
			int num = 0;
			foreach (_IPragmaExpression ipragmaExpression in \u0002.Operands.OfType<_IPragmaExpression>())
			{
				ipragmaExpression.Accept(this);
				array[num] = ipragmaExpression.Value;
				num++;
			}
			\u0002.Value = false;
			PragmaOperator code = \u0002.Code;
			if (code - PragmaOperator.Or > 1)
			{
				if (code == PragmaOperator.Not)
				{
					\u0002.Value = !array[0];
					return;
				}
			}
			else
			{
				this.\u0001(\u0002, array);
			}
		}

		// Token: 0x06003595 RID: 13717 RVA: 0x000D6E68 File Offset: 0x000D5068
		private void \u0001(_IPragmaOperatorExpression \u0002, bool[] \u0003)
		{
			\u0002.Value = \u0003[0];
			for (int i = 1; i < \u0003.Length; i++)
			{
				PragmaOperator code = \u0002.Code;
				if (code != PragmaOperator.Or)
				{
					if (code == PragmaOperator.And)
					{
						\u0002.Value = (\u0002.Value && \u0003[i]);
					}
				}
				else
				{
					\u0002.Value = (\u0002.Value || \u0003[i]);
				}
			}
		}

		// Token: 0x06003596 RID: 13718 RVA: 0x000D6EC8 File Offset: 0x000D50C8
		public void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			if (ipragmaExpression != null && !ipragmaExpression.Value)
			{
				ipragmaExpression.AddError(\u0002.ErrorOutput);
			}
		}

		// Token: 0x06003597 RID: 13719 RVA: 0x000D6F04 File Offset: 0x000D5104
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06003598 RID: 13720 RVA: 0x000D6F08 File Offset: 0x000D5108
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			if (\u0002.DefineReference != null)
			{
				\u0002.Value = ProjectDefines.IsInProjectDefined(\u0002.DefineReference.Define);
			}
		}

		// Token: 0x06003599 RID: 13721 RVA: 0x000D6F28 File Offset: 0x000D5128
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
			Version u = Helper.\u0001(this.\u0001.GetTargetSettings());
			this.\u0001(\u0002, \u0002.OpComparison, u, \u0002.VersionToTest);
		}

		// Token: 0x0600359A RID: 13722 RVA: 0x000D6F5C File Offset: 0x000D515C
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
			this.\u0001(\u0002, \u0002.OpComparison, APEnvironmentFacade.Instance.CompilerVersionToUseInternal(), \u0002.VersionToTest);
		}

		// Token: 0x0600359B RID: 13723 RVA: 0x000D6F7C File Offset: 0x000D517C
		public void \u0001(_IDefineStatement \u0002)
		{
			if (\u0002.Define)
			{
				this.\u0001[\u0002.Ident] = \u0002.Value;
				return;
			}
			if (this.\u0001.ContainsKey(\u0002.Ident))
			{
				this.\u0001.Remove(\u0002.Ident);
			}
		}

		// Token: 0x0600359C RID: 13724 RVA: 0x000D6FD0 File Offset: 0x000D51D0
		public void \u0001(_IXRefExpression \u0002)
		{
			if (\u0002.XRef != null)
			{
				\u0002.XRef.Accept(this);
			}
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x0600359D RID: 13725 RVA: 0x000D6FFC File Offset: 0x000D51FC
		public void \u0001(_IHasTypeExpression \u0002)
		{
			if (\u0002.Variable != null)
			{
				\u0002.Variable.Accept(this);
			}
		}

		// Token: 0x0600359E RID: 13726 RVA: 0x000D7014 File Offset: 0x000D5214
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x0600359F RID: 13727 RVA: 0x000D7018 File Offset: 0x000D5218
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			\u0002.Value = false;
			_IItemReference iitemReference = \u0002.ItemReference as _IItemReference;
			if (iitemReference != null)
			{
				iitemReference.Accept(this);
				IPrecompileScope2 scope = this.\u0001.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2;
				if (iitemReference.Value && iitemReference.HasAttribute(\u0002.Attribute, scope))
				{
					\u0002.Value = true;
				}
			}
		}

		// Token: 0x060035A0 RID: 13728 RVA: 0x000D7078 File Offset: 0x000D5278
		public void \u0001(_IHasValueExpression \u0002)
		{
			\u0002.Value = false;
			string a;
			if (this.\u0001.TryGetValue(\u0002.Define, ref a))
			{
				\u0002.Value = (a == \u0002.DefineValue);
			}
		}

		// Token: 0x060035A1 RID: 13729 RVA: 0x000D70B4 File Offset: 0x000D52B4
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
			if (\u0002._Constant != null)
			{
				\u0002._Constant.Accept(this);
			}
			if (\u0002._ConstantValue != null)
			{
				\u0002._ConstantValue.Accept(this);
			}
		}

		// Token: 0x060035A2 RID: 13730 RVA: 0x000D70E0 File Offset: 0x000D52E0
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			if (\u0002 != null)
			{
				\u0002._Constant.Accept(this);
			}
		}

		// Token: 0x060035A3 RID: 13731 RVA: 0x000D70F4 File Offset: 0x000D52F4
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x060035A4 RID: 13732 RVA: 0x000D70F8 File Offset: 0x000D52F8
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x060035A5 RID: 13733 RVA: 0x000D70FC File Offset: 0x000D52FC
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x060035A6 RID: 13734 RVA: 0x000D7100 File Offset: 0x000D5300
		public void \u0001(_IAssignmentExpression \u0002)
		{
		}

		// Token: 0x060035A7 RID: 13735 RVA: 0x000D7104 File Offset: 0x000D5304
		public void \u0001(_IReturnStatement \u0002)
		{
		}

		// Token: 0x060035A8 RID: 13736 RVA: 0x000D7108 File Offset: 0x000D5308
		public void \u0001(_IJumpStatement \u0002)
		{
		}

		// Token: 0x060035A9 RID: 13737 RVA: 0x000D710C File Offset: 0x000D530C
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x060035AA RID: 13738 RVA: 0x000D7110 File Offset: 0x000D5310
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x060035AB RID: 13739 RVA: 0x000D7114 File Offset: 0x000D5314
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x060035AC RID: 13740 RVA: 0x000D7118 File Offset: 0x000D5318
		public void \u0001(_IExpressionStatement \u0002)
		{
		}

		// Token: 0x060035AD RID: 13741 RVA: 0x000D711C File Offset: 0x000D531C
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x060035AE RID: 13742 RVA: 0x000D7120 File Offset: 0x000D5320
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x060035AF RID: 13743 RVA: 0x000D7124 File Offset: 0x000D5324
		public void \u0001(_ICallExpression \u0002)
		{
		}

		// Token: 0x060035B0 RID: 13744 RVA: 0x000D7128 File Offset: 0x000D5328
		public void \u0001(_IOperatorExpression \u0002)
		{
		}

		// Token: 0x060035B1 RID: 13745 RVA: 0x000D712C File Offset: 0x000D532C
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x060035B2 RID: 13746 RVA: 0x000D7130 File Offset: 0x000D5330
		public void \u0001(_INewExpression \u0002)
		{
		}

		// Token: 0x060035B3 RID: 13747 RVA: 0x000D7134 File Offset: 0x000D5334
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x060035B4 RID: 13748 RVA: 0x000D7138 File Offset: 0x000D5338
		public void \u0001(_IConversionExpression \u0002)
		{
		}

		// Token: 0x060035B5 RID: 13749 RVA: 0x000D713C File Offset: 0x000D533C
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x060035B6 RID: 13750 RVA: 0x000D7140 File Offset: 0x000D5340
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x060035B7 RID: 13751 RVA: 0x000D7144 File Offset: 0x000D5344
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x060035B8 RID: 13752 RVA: 0x000D7148 File Offset: 0x000D5348
		public void \u0001(_IAddressExpression \u0002)
		{
		}

		// Token: 0x060035B9 RID: 13753 RVA: 0x000D714C File Offset: 0x000D534C
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x060035BA RID: 13754 RVA: 0x000D7150 File Offset: 0x000D5350
		public void \u0001(_IVariableExpression \u0002)
		{
		}

		// Token: 0x060035BB RID: 13755 RVA: 0x000D7154 File Offset: 0x000D5354
		public void \u0001(_IIndexAccessExpression \u0002)
		{
		}

		// Token: 0x060035BC RID: 13756 RVA: 0x000D7158 File Offset: 0x000D5358
		public void \u0001(_ICompoAccessExpression \u0002)
		{
		}

		// Token: 0x060035BD RID: 13757 RVA: 0x000D715C File Offset: 0x000D535C
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
		}

		// Token: 0x060035BE RID: 13758 RVA: 0x000D7160 File Offset: 0x000D5360
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x060035BF RID: 13759 RVA: 0x000D7164 File Offset: 0x000D5364
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
		}

		// Token: 0x060035C0 RID: 13760 RVA: 0x000D7168 File Offset: 0x000D5368
		public void \u0001(_ISystemScopeExpression \u0002)
		{
		}

		// Token: 0x060035C1 RID: 13761 RVA: 0x000D716C File Offset: 0x000D536C
		public void \u0001(_IPoolScopeExpression \u0002)
		{
		}

		// Token: 0x060035C2 RID: 13762 RVA: 0x000D7170 File Offset: 0x000D5370
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x060035C3 RID: 13763 RVA: 0x000D7174 File Offset: 0x000D5374
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x060035C4 RID: 13764 RVA: 0x000D7178 File Offset: 0x000D5378
		public void \u0001(_ICaseRangeExpression \u0002)
		{
		}

		// Token: 0x060035C5 RID: 13765 RVA: 0x000D717C File Offset: 0x000D537C
		public void \u0001(_ICaseLabelStatement \u0002)
		{
		}

		// Token: 0x060035C6 RID: 13766 RVA: 0x000D7180 File Offset: 0x000D5380
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060035C7 RID: 13767 RVA: 0x000D7184 File Offset: 0x000D5384
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060035C8 RID: 13768 RVA: 0x000D7188 File Offset: 0x000D5388
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060035C9 RID: 13769 RVA: 0x000D718C File Offset: 0x000D538C
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060035CA RID: 13770 RVA: 0x000D7190 File Offset: 0x000D5390
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060035CB RID: 13771 RVA: 0x000D7194 File Offset: 0x000D5394
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x060035CC RID: 13772 RVA: 0x000D7198 File Offset: 0x000D5398
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x060035CD RID: 13773 RVA: 0x000D719C File Offset: 0x000D539C
		public void \u0001(_IPartialAccessExpression \u0002)
		{
		}

		// Token: 0x170008CA RID: 2250
		// (get) Token: 0x060035CE RID: 13774 RVA: 0x000D71A0 File Offset: 0x000D53A0
		// (set) Token: 0x060035CF RID: 13775 RVA: 0x000D71A8 File Offset: 0x000D53A8
		public bool ConditionalCompilationAllowed { get; set; }

		// Token: 0x060035D0 RID: 13776 RVA: 0x000D71B4 File Offset: 0x000D53B4
		private void \u0002(_IPragmaIfStatement \u0002)
		{
			bool flag = false;
			if (\u0002.IfThen != null)
			{
				_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
				if (ipragmaExpression != null && ipragmaExpression.Value)
				{
					flag = true;
				}
				else
				{
					this.\u0002(\u0002.IfThen as _ISequenceStatement);
				}
			}
			flag = this.\u0001(\u0002, flag);
			if (\u0002.IfElse != null && flag)
			{
				this.\u0002(\u0002.IfElse as _ISequenceStatement);
			}
		}

		// Token: 0x060035D1 RID: 13777 RVA: 0x000D721C File Offset: 0x000D541C
		private void \u0002(_ISequenceStatement \u0002)
		{
			if (this.UnusedStatementPositions == null)
			{
				this.UnusedStatementPositions = new List<ISourcePosition>();
			}
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				if (istatement.Position != null)
				{
					this.UnusedStatementPositions.Add(istatement.Position);
				}
			}
		}

		// Token: 0x060035D2 RID: 13778 RVA: 0x000D7290 File Offset: 0x000D5490
		private bool \u0001(_IPragmaIfStatement \u0002, bool \u0003)
		{
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				if (ipragmaElseIf != null)
				{
					_IPragmaExpression ipragmaExpression = ipragmaElseIf.Condition as _IPragmaExpression;
					if (ipragmaExpression != null && ipragmaExpression.Value)
					{
						\u0003 = true;
					}
					else
					{
						this.\u0002(ipragmaElseIf.Controlled as _ISequenceStatement);
					}
				}
			}
			return \u0003;
		}

		// Token: 0x060035D3 RID: 13779 RVA: 0x000D7308 File Offset: 0x000D5508
		private static bool \u0001(_IExpression \u0002)
		{
			if (\u0002 is _ICompilerVersionExpression || \u0002 is _IProjectDefinedExpression || \u0002 is _IRuntimeVersionExpression)
			{
				return true;
			}
			_IPragmaOperatorExpression ipragmaOperatorExpression = \u0002 as _IPragmaOperatorExpression;
			return ipragmaOperatorExpression != null && ipragmaOperatorExpression.Operands.All(new Func<_IExpression, bool>(PragmaVisitor.\u0001));
		}

		// Token: 0x060035D4 RID: 13780 RVA: 0x000D7354 File Offset: 0x000D5554
		private static bool \u0002(_IExpression \u0002)
		{
			return PragmaVisitor.\u0001.\u0001(\u0002);
		}

		// Token: 0x060035D5 RID: 13781 RVA: 0x000D735C File Offset: 0x000D555C
		private static bool \u0001(VarFlag \u0002, VarFlag \u0003)
		{
			return (\u0002 & \u0003) > VarFlag.None;
		}

		// Token: 0x060035D6 RID: 13782 RVA: 0x000D7368 File Offset: 0x000D5568
		private static bool \u0001(VarFlag \u0002)
		{
			return !PragmaVisitor.\u0001(\u0002, VarFlag.Structure | VarFlag.Union) && PragmaVisitor.\u0001(\u0002, VarFlag.Local | VarFlag.Temp | VarFlag.Static);
		}

		// Token: 0x060035D7 RID: 13783 RVA: 0x000D7388 File Offset: 0x000D5588
		private bool \u0001(_IStatement \u0002)
		{
			_ISequenceStatement isequenceStatement = \u0002 as _ISequenceStatement;
			if (isequenceStatement != null)
			{
				bool flag = false;
				foreach (_IStatement u in isequenceStatement._StatementList)
				{
					flag |= this.\u0001(u);
				}
				return flag;
			}
			_IVariableDeclarationListStatement ivariableDeclarationListStatement = \u0002 as _IVariableDeclarationListStatement;
			if (ivariableDeclarationListStatement == null)
			{
				if (!(\u0002 is _IVariableDeclarationStatement))
				{
					if (!(\u0002 is _ICommentStatement) && !(\u0002 is _IErrorStatement) && !(\u0002 is _IPragmaIfStatement) && !(\u0002 is _IPragmaStatement))
					{
						this.\u0001(\u0002, MessageId.Err_ProjectDefinedNotSupportedFor, new string[]
						{
							this.\u0001
						});
						return true;
					}
					return false;
				}
				else
				{
					if (this.\u0001 != null && PragmaVisitor.\u0001(this.\u0001.Flags))
					{
						return false;
					}
					this.\u0001(\u0002, MessageId.Err_ProjectDefinedNotSupportedFor, new string[]
					{
						this.\u0001
					});
					return true;
				}
			}
			else
			{
				if (PragmaVisitor.\u0001(ivariableDeclarationListStatement.Flags))
				{
					return false;
				}
				this.\u0001(\u0002, MessageId.Err_ProjectDefinedNotSupportedFor, new string[]
				{
					this.\u0001
				});
				return true;
			}
		}

		// Token: 0x060035D8 RID: 13784 RVA: 0x000D74B4 File Offset: 0x000D56B4
		private void \u0003(_IPragmaIfStatement \u0002)
		{
			_IPragmaExpression ipragmaExpression = \u0002.Condition as _IPragmaExpression;
			if (ipragmaExpression != null && ipragmaExpression.Value)
			{
				\u0002.IfThen.Accept(this);
				Debug.\u0001(this.\u0001.Pop() == null);
				this.\u0001.Push(\u0002.IfThen);
				this.\u0002(\u0002);
				return;
			}
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaElseIf.Condition.Accept(this);
				ipragmaExpression = (ipragmaElseIf.Condition as _IPragmaExpression);
				if (ipragmaExpression != null && ipragmaExpression.Value)
				{
					ipragmaElseIf.Controlled.Accept(this);
					Debug.\u0001(this.\u0001.Pop() == null);
					this.\u0001.Push(ipragmaElseIf.Controlled);
					this.\u0002(\u0002);
					return;
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
				Debug.\u0001(this.\u0001.Pop() == null);
				this.\u0001.Push(\u0002.IfElse);
				this.\u0002(\u0002);
				return;
			}
			Debug.\u0001(this.\u0001.Pop() == null);
			this.\u0001.Push(\u0019.\u0003.\u0001());
		}

		// Token: 0x060035D9 RID: 13785 RVA: 0x000D7608 File Offset: 0x000D5808
		private void \u0001(_IExprement \u0002, MessageId \u0003, params string[] \u0004)
		{
			this.\u0001 = true;
			global::\u0003.\u0006.\u0002(\u0002, \u0003, \u0004);
		}

		// Token: 0x060035DA RID: 13786 RVA: 0x000D7628 File Offset: 0x000D5828
		private void \u0001(_IExprement \u0002, MessageId \u0003)
		{
			this.\u0001 = true;
			global::\u0003.\u0006.\u0002(\u0002, \u0003, Array.Empty<object>());
		}

		// Token: 0x060035DB RID: 13787 RVA: 0x000D7640 File Offset: 0x000D5840
		private void \u0001(_IPragmaExpression \u0002, Operator \u0003, Version \u0004, Version \u0005)
		{
			switch (\u0003)
			{
			case Operator.Eq:
				goto IL_85;
			case Operator.Ne:
				goto IL_94;
			case Operator.Ge:
				break;
			case Operator.Gt:
				goto IL_58;
			case Operator.Le:
				goto IL_76;
			case Operator.Lt:
				goto IL_67;
			default:
				switch (\u0003)
				{
				case Operator.Less:
					goto IL_67;
				case Operator.Greater:
					goto IL_58;
				case Operator.LessEqual:
					goto IL_76;
				case Operator.GreaterEqual:
					break;
				case Operator.Equal:
					goto IL_85;
				case Operator.NotEqual:
					goto IL_94;
				default:
					return;
				}
				break;
			}
			\u0002.Value = (\u0004 >= \u0005);
			return;
			IL_58:
			\u0002.Value = (\u0004 > \u0005);
			return;
			IL_67:
			\u0002.Value = (\u0004 < \u0005);
			return;
			IL_76:
			\u0002.Value = (\u0004 <= \u0005);
			return;
			IL_85:
			\u0002.Value = (\u0004 == \u0005);
			return;
			IL_94:
			\u0002.Value = (\u0004 != \u0005);
		}

		// Token: 0x04000A6D RID: 2669
		private string \u0001;

		// Token: 0x04000A6E RID: 2670
		private readonly _IPreCompileContext \u0001;

		// Token: 0x04000A6F RID: 2671
		private readonly LStack<_IStatement> \u0001 = new LStack<_IStatement>();

		// Token: 0x04000A70 RID: 2672
		private readonly LDictionary<string, string> \u0001 = new LDictionary<string, string>();

		// Token: 0x04000A71 RID: 2673
		[CompilerGenerated]
		private List<ISourcePosition> \u0001;

		// Token: 0x04000A72 RID: 2674
		private bool \u0001;

		// Token: 0x04000A73 RID: 2675
		private _IVariableDeclarationListStatement \u0001;

		// Token: 0x04000A74 RID: 2676
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x0200039C RID: 924
		private sealed class \u0001 : EmptyVisitor352000, IExprementVisitorNoTraversion, IExprementVisitorNoTraversion352000, IExprementVisitorNoTraversion351900, IExprementVisitorNoTraversion351800, IExprementVisitorNoTraversion351500, IExprementVisitorNoTraversion351400, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590
		{
			// Token: 0x060035DC RID: 13788 RVA: 0x000D76F0 File Offset: 0x000D58F0
			private \u0001()
			{
			}

			// Token: 0x060035DD RID: 13789 RVA: 0x000D76F8 File Offset: 0x000D58F8
			internal static bool \u0001(_IExpression \u0002)
			{
				return new PragmaVisitor.\u0001().\u0001(\u0002);
			}

			// Token: 0x060035DE RID: 13790 RVA: 0x000D7708 File Offset: 0x000D5908
			private bool \u0001(_IExpression \u0002)
			{
				this.\u0001 = false;
				StandardTraverser ivisit = new StandardTraverser(this);
				\u0002.Accept(ivisit);
				return this.\u0001;
			}

			// Token: 0x060035DF RID: 13791 RVA: 0x000D7730 File Offset: 0x000D5930
			public override void visit(_IProjectDefinedExpression projectDefinedExpression)
			{
				this.\u0001 = true;
			}

			// Token: 0x04000A75 RID: 2677
			private bool \u0001;
		}
	}
}
