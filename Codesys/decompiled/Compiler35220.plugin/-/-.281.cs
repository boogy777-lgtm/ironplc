using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;

namespace \u0017
{
	// Token: 0x020002ED RID: 749
	internal sealed class \u0015 : IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x06002D9B RID: 11675 RVA: 0x000A7D08 File Offset: 0x000A5F08
		public \u0015(int \u001D\u0004, IScope \u009B\u0002, _ICompileContext \u0001\u0002, bool \u001E\u0004, Guid \u001F\u0004)
		{
			this.\u0001 = \u001D\u0004;
			this.Scope = \u009B\u0002;
			this.\u0001 = \u0001\u0002;
			this.\u0001 = \u001E\u0004;
			this.\u0001 = \u001F\u0004;
			this.\u0001._GetDirectVariableTable().SetNumberOfTasks(this.\u0001.TaskList.Count);
		}

		// Token: 0x06002D9C RID: 11676 RVA: 0x000A7D6C File Offset: 0x000A5F6C
		private void \u0001(AccessFlag \u0002)
		{
			this.\u0001.Push(new \u0015.\u0001
			{
				\u0001 = 0,
				\u0002 = -1,
				\u0003 = 0,
				\u0001 = new \u0083.\u0001(null, \u0002, this.\u0001.ApplicationGuid, Guid.Empty)
			});
		}

		// Token: 0x170007BC RID: 1980
		// (get) Token: 0x06002D9D RID: 11677 RVA: 0x000A7DBC File Offset: 0x000A5FBC
		private IScope Scope { get; }

		// Token: 0x170007BD RID: 1981
		// (get) Token: 0x06002D9E RID: 11678 RVA: 0x000A7DC4 File Offset: 0x000A5FC4
		private \u0015.\u0001 TopOfStack
		{
			get
			{
				if (this.\u0001.Count > 0)
				{
					return this.\u0001.Peek();
				}
				return null;
			}
		}

		// Token: 0x170007BE RID: 1982
		// (get) Token: 0x06002D9F RID: 11679 RVA: 0x000A7DE4 File Offset: 0x000A5FE4
		// (set) Token: 0x06002DA0 RID: 11680 RVA: 0x000A7DEC File Offset: 0x000A5FEC
		public bool Errors { get; private set; }

		// Token: 0x06002DA1 RID: 11681 RVA: 0x000A7DF8 File Offset: 0x000A5FF8
		private void \u0001()
		{
			this.\u0001.Pop();
		}

		// Token: 0x06002DA2 RID: 11682 RVA: 0x000A7E08 File Offset: 0x000A6008
		private void \u0001(ref int \u0002, out bool \u0003, IList<_IArrayDimension> \u0004, ref int \u0005, int \u0006, _IExpression \u0007)
		{
			\u0003 = false;
			if (!\u0007.IsConstant(this.Scope, false))
			{
				return;
			}
			ILiteralValue literalValue = \u0007.Literal(this.Scope);
			if (literalValue == null)
			{
				return;
			}
			bool flag;
			int @int = literalValue.GetInt(out flag);
			if (!flag)
			{
				return;
			}
			int num = \u0004[\u0006].LowerBorderInt(out flag, this.Scope);
			if (!flag)
			{
				return;
			}
			int num2 = \u0004[\u0006].Range(out flag, this.Scope);
			if (!flag)
			{
				return;
			}
			\u0002 += (@int - num) * \u0005;
			\u0005 *= num2;
			\u0003 = true;
		}

		// Token: 0x06002DA3 RID: 11683 RVA: 0x000A7E98 File Offset: 0x000A6098
		private bool \u0001(_IVariableExpression \u0002, IVariable \u0003)
		{
			int num = \u0002.Type.Size(this.Scope);
			if (\u0003.Address.Size == DirectVariableSize.X)
			{
				num = 0;
			}
			IMessage message;
			bool flag;
			IDataLocation dataLocation = Locator.\u0001(this.\u0001, out message, out flag, ((_IVariable)\u0003)._SourcePosition, \u0003.Address, (IVariable2)\u0003);
			if (dataLocation == null || this.\u0001)
			{
				return true;
			}
			_IDataLocation idataLocation = (_IDataLocation)\u0019.\u0003.\u0001(dataLocation.Area, dataLocation.Offset, dataLocation.BitNr);
			int num2 = num;
			if (this.TopOfStack.\u0002 != -1)
			{
				num2 = this.TopOfStack.\u0002 * 8;
				if (num2 == 0)
				{
					num2 = 1;
				}
				idataLocation.Offset += this.TopOfStack.\u0001;
				idataLocation.BitNr = (byte)this.TopOfStack.\u0003;
			}
			foreach (byte nTaskId in this.Scope[this.\u0001].TaskReferenceList)
			{
				this.\u0001._GetDirectVariableTable().AddProcessImageLocation(idataLocation, num2, (int)nTaskId, \u0003.Address.Location, this.TopOfStack.\u0001.Access);
			}
			_IAddressCodePosition iaddressCodePosition = \u0019.\u0003.\u0001(\u0002.Position, this.TopOfStack.\u0001.Access, num);
			iaddressCodePosition.VariableName = \u0002.Name;
			this.\u0001.AddAddressCrossReference(\u0003.Address, this.\u0001, iaddressCodePosition);
			return false;
		}

		// Token: 0x06002DA4 RID: 11684 RVA: 0x000A800C File Offset: 0x000A620C
		public void \u0001(_IAddressExpression \u0002)
		{
			_ISignature isignature = (_ISignature)this.Scope[this.\u0001];
			byte[] taskReferenceList = isignature.TaskReferenceList;
			IMessage message;
			bool flag;
			IDataLocation dataLocation = Locator.\u0001(this.\u0001, out message, out flag, \u0002.Position, \u0002.DirectAddress, null);
			if (!this.\u0001 && dataLocation != null && !flag)
			{
				foreach (byte nTaskId in taskReferenceList)
				{
					this.\u0001._GetDirectVariableTable().AddProcessImageLocation(dataLocation, \u0002.Type.Size(this.Scope) * 8, (int)nTaskId, \u0002.DirectAddress.Location, this.TopOfStack.\u0001.Access);
				}
				this.\u0001.AddAddressCrossReference(\u0002.DirectAddress, this.\u0001, \u0002.Position as _ISourcePosition, this.TopOfStack.\u0001.Access);
			}
			if (message != null)
			{
				\u0002.AddMessage(message.Text, \u0002._Position, message.Severity, \u0002.PositionLength, MessageId.None);
				this.Errors = (message.Severity == Severity.Error || message.Severity == Severity.FatalError);
				IMessage4 message2 = message as IMessage4;
				if (message2 != null)
				{
					isignature.AddMessage(message2);
				}
			}
		}

		// Token: 0x06002DA5 RID: 11685 RVA: 0x000A8150 File Offset: 0x000A6350
		public void \u0001(_IVariableExpression \u0002)
		{
			ISignature signature = this.Scope[\u0002.SignatureId];
			IVariable variable = (signature != null) ? signature[\u0002.VariableId] : null;
			if (variable == null)
			{
				return;
			}
			if (variable.Address != null && this.\u0001(\u0002, variable))
			{
				return;
			}
			if (variable.DataLocation != null && variable.DataLocation.IsRelativ)
			{
				this.TopOfStack.\u0001 += variable.DataLocation.Offset;
				if (this.TopOfStack.\u0002 == -1)
				{
					this.TopOfStack.\u0002 = variable.CompiledType.Size(this.Scope);
				}
			}
		}

		// Token: 0x06002DA6 RID: 11686 RVA: 0x000A81F4 File Offset: 0x000A63F4
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.\u0001(AccessFlag.Unknown);
			if (!\u0002.GetFlag(CompiledPOUFlags.ContainsDirVarAccess) || \u0002.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
			{
				return;
			}
			_IStatement parseTree = \u0002.GetParseTree();
			if (!CompilerPhase4_Typechecker.\u0001(\u0002, parseTree, this.\u0001))
			{
				parseTree.Accept(this);
			}
		}

		// Token: 0x06002DA7 RID: 11687 RVA: 0x000A8244 File Offset: 0x000A6444
		public void \u0001(_ISignature \u0002)
		{
			if (\u0002 != null)
			{
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					_IExprement iexprement = ivariable.Initial as _IExprement;
					if (iexprement != null)
					{
						this.\u0001(AccessFlag.Read);
						iexprement.Accept(this);
						this.\u0001();
					}
				}
			}
		}

		// Token: 0x06002DA8 RID: 11688 RVA: 0x000A82B0 File Offset: 0x000A64B0
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002DA9 RID: 11689 RVA: 0x000A82D8 File Offset: 0x000A64D8
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002DAA RID: 11690 RVA: 0x000A8300 File Offset: 0x000A6500
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			this.\u0001(AccessFlag.Read);
			\u0002._UpperBound.Accept(this);
			this.\u0001();
			if (\u0002.By != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._By.Accept(this);
				this.\u0001();
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06002DAB RID: 11691 RVA: 0x000A8360 File Offset: 0x000A6560
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				istatement.Accept(this);
			}
		}

		// Token: 0x06002DAC RID: 11692 RVA: 0x000A83AC File Offset: 0x000A65AC
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(AccessFlag.Read);
				ielseIf._Condition.Accept(this);
				this.\u0001();
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x06002DAD RID: 11693 RVA: 0x000A8450 File Offset: 0x000A6650
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Expr.Accept(this);
			this.\u0001();
		}

		// Token: 0x06002DAE RID: 11694 RVA: 0x000A846C File Offset: 0x000A666C
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(AccessFlag.Write);
			\u0002._LValue.Accept(this);
			this.\u0001();
			this.\u0001(AccessFlag.Read);
			\u0002._RValue.Accept(this);
			this.\u0001();
		}

		// Token: 0x06002DAF RID: 11695 RVA: 0x000A84A0 File Offset: 0x000A66A0
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(AccessFlag.Call);
			\u0002._Callee.Accept(this);
			this.\u0001();
			if (\u0002._Condition != null)
			{
				this.\u0001(AccessFlag.Read);
				\u0002._Condition.Accept(this);
				this.\u0001();
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				if (iexpression != null)
				{
					this.\u0001(AccessFlag.Read);
					iexpression.Accept(this);
					this.\u0001();
				}
			}
			foreach (_IExpression iexpression2 in \u0002.OutputExpressions)
			{
				if (iexpression2 != null)
				{
					this.\u0001(AccessFlag.Write);
					iexpression2.Accept(this);
					this.\u0001();
				}
			}
			foreach (_IExpression iexpression3 in \u0002.Inputs)
			{
				if (iexpression3 != null)
				{
					this.\u0001(AccessFlag.Write);
					iexpression3.Accept(this);
					this.\u0001();
				}
			}
			foreach (_IExpression iexpression4 in \u0002.Outputs)
			{
				if (iexpression4 != null)
				{
					this.\u0001(AccessFlag.Read);
					iexpression4.Accept(this);
					this.\u0001();
				}
			}
		}

		// Token: 0x06002DB0 RID: 11696 RVA: 0x000A8620 File Offset: 0x000A6820
		public void \u0001(_IOperatorExpression \u0002)
		{
			foreach (_IExprement iexprement in \u0002._OperandsList)
			{
				if (\u0002.Code == Operator.Adr || \u0002.Code == Operator.__RefAdr)
				{
					this.TopOfStack.\u0001 = new \u0083.\u0001(\u0019.\u0003.\u0001(), AccessFlag.Write, this.\u0001.ApplicationGuid, this.\u0001);
				}
				iexprement.Accept(this);
			}
		}

		// Token: 0x06002DB1 RID: 11697 RVA: 0x000A86AC File Offset: 0x000A68AC
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x06002DB2 RID: 11698 RVA: 0x000A86BC File Offset: 0x000A68BC
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in \u0002._FBInitParams.Cast<_IAssignmentExpression>())
				{
					iassignmentExpression.Accept(this);
				}
			}
		}

		// Token: 0x06002DB3 RID: 11699 RVA: 0x000A8720 File Offset: 0x000A6920
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(AccessFlag.Read);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
			this.\u0001();
			int num = 0;
			int u = 0;
			bool flag = true;
			ICompiledType type = \u0002._Var.Type;
			ICompiledType compiledType = (type != null) ? type.DeRefType : null;
			if (compiledType != null && compiledType.Class == TypeClass.Array)
			{
				_IArrayType iarrayType = compiledType as _IArrayType;
				if (iarrayType != null)
				{
					IList<_IArrayDimension> dimensions = iarrayType._Dimensions;
					int num2 = iarrayType.BaseType.Size(this.Scope);
					u = num2;
					for (int j = dimensions.Count - 1; j >= 0; j--)
					{
						_IExpression access = \u0002.GetAccess(j);
						this.\u0001(ref num, out flag, dimensions, ref num2, j, access);
					}
				}
			}
			if (flag)
			{
				this.TopOfStack.\u0001 += num;
				if (this.TopOfStack.\u0002 == -1)
				{
					this.TopOfStack.\u0002 = u;
				}
			}
			\u0002._Var.Accept(this);
		}

		// Token: 0x06002DB4 RID: 11700 RVA: 0x000A8824 File Offset: 0x000A6A24
		public void \u0001(_ILiteralExpression \u0002)
		{
			this.TopOfStack.\u0002 = 0;
			this.TopOfStack.\u0003 = (int)\u0002.LongValue;
		}

		// Token: 0x06002DB5 RID: 11701 RVA: 0x000A8844 File Offset: 0x000A6A44
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			\u0002._Right.Accept(this);
			\u0002._Left.Accept(this);
		}

		// Token: 0x06002DB6 RID: 11702 RVA: 0x000A8860 File Offset: 0x000A6A60
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002DB7 RID: 11703 RVA: 0x000A8870 File Offset: 0x000A6A70
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002DB8 RID: 11704 RVA: 0x000A8880 File Offset: 0x000A6A80
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002DB9 RID: 11705 RVA: 0x000A8890 File Offset: 0x000A6A90
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002DBA RID: 11706 RVA: 0x000A88A0 File Offset: 0x000A6AA0
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002DBB RID: 11707 RVA: 0x000A88B0 File Offset: 0x000A6AB0
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access.Accept(this);
		}

		// Token: 0x06002DBC RID: 11708 RVA: 0x000A88C0 File Offset: 0x000A6AC0
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x06002DBD RID: 11709 RVA: 0x000A88DC File Offset: 0x000A6ADC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06002DBE RID: 11710 RVA: 0x000A8928 File Offset: 0x000A6B28
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			\u0002._Switch.Accept(this);
			this.\u0001();
			foreach (_ICase icase in \u0002._Cases)
			{
				this.\u0001(AccessFlag.Read);
				icase._Label.Accept(this);
				this.\u0001();
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x06002DBF RID: 11711 RVA: 0x000A89C0 File Offset: 0x000A6BC0
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			this.\u0001();
		}

		// Token: 0x06002DC0 RID: 11712 RVA: 0x000A89E4 File Offset: 0x000A6BE4
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001(AccessFlag.Read);
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			this.\u0001();
		}

		// Token: 0x06002DC1 RID: 11713 RVA: 0x000A8A08 File Offset: 0x000A6C08
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			_IPragmaExpression ipragmaExpression = (_IPragmaExpression)\u0002.Condition;
			if (ipragmaExpression.ValueStillUndecided)
			{
				return;
			}
			if (ipragmaExpression.Value)
			{
				\u0002.IfThen.Accept(this);
				return;
			}
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				ipragmaExpression = (ipragmaElseIf.Condition as _IPragmaExpression);
				if (ipragmaExpression != null && ipragmaExpression.Value)
				{
					ipragmaElseIf.Controlled.Accept(this);
					return;
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06002DC2 RID: 11714 RVA: 0x000A8AB4 File Offset: 0x000A6CB4
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06002DC3 RID: 11715 RVA: 0x000A8AB8 File Offset: 0x000A6CB8
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06002DC4 RID: 11716 RVA: 0x000A8ABC File Offset: 0x000A6CBC
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06002DC5 RID: 11717 RVA: 0x000A8AC0 File Offset: 0x000A6CC0
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06002DC6 RID: 11718 RVA: 0x000A8AC4 File Offset: 0x000A6CC4
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06002DC7 RID: 11719 RVA: 0x000A8AC8 File Offset: 0x000A6CC8
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06002DC8 RID: 11720 RVA: 0x000A8ACC File Offset: 0x000A6CCC
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06002DC9 RID: 11721 RVA: 0x000A8AD0 File Offset: 0x000A6CD0
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06002DCA RID: 11722 RVA: 0x000A8AD4 File Offset: 0x000A6CD4
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06002DCB RID: 11723 RVA: 0x000A8AD8 File Offset: 0x000A6CD8
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06002DCC RID: 11724 RVA: 0x000A8ADC File Offset: 0x000A6CDC
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06002DCD RID: 11725 RVA: 0x000A8AE0 File Offset: 0x000A6CE0
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06002DCE RID: 11726 RVA: 0x000A8AE4 File Offset: 0x000A6CE4
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06002DCF RID: 11727 RVA: 0x000A8AE8 File Offset: 0x000A6CE8
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002DD0 RID: 11728 RVA: 0x000A8AEC File Offset: 0x000A6CEC
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002DD1 RID: 11729 RVA: 0x000A8AF0 File Offset: 0x000A6CF0
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002DD2 RID: 11730 RVA: 0x000A8AF4 File Offset: 0x000A6CF4
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002DD3 RID: 11731 RVA: 0x000A8AF8 File Offset: 0x000A6CF8
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002DD4 RID: 11732 RVA: 0x000A8AFC File Offset: 0x000A6CFC
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002DD5 RID: 11733 RVA: 0x000A8B00 File Offset: 0x000A6D00
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x06002DD6 RID: 11734 RVA: 0x000A8B04 File Offset: 0x000A6D04
		public void \u0001(_IArrayInitialization \u0002)
		{
		}

		// Token: 0x06002DD7 RID: 11735 RVA: 0x000A8B08 File Offset: 0x000A6D08
		public void \u0001(_IStructureInitialization \u0002)
		{
		}

		// Token: 0x06002DD8 RID: 11736 RVA: 0x000A8B0C File Offset: 0x000A6D0C
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06002DD9 RID: 11737 RVA: 0x000A8B10 File Offset: 0x000A6D10
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x06002DDA RID: 11738 RVA: 0x000A8B14 File Offset: 0x000A6D14
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x06002DDB RID: 11739 RVA: 0x000A8B18 File Offset: 0x000A6D18
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x06002DDC RID: 11740 RVA: 0x000A8B1C File Offset: 0x000A6D1C
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06002DDD RID: 11741 RVA: 0x000A8B20 File Offset: 0x000A6D20
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06002DDE RID: 11742 RVA: 0x000A8B24 File Offset: 0x000A6D24
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x06002DDF RID: 11743 RVA: 0x000A8B28 File Offset: 0x000A6D28
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06002DE0 RID: 11744 RVA: 0x000A8B2C File Offset: 0x000A6D2C
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06002DE1 RID: 11745 RVA: 0x000A8B30 File Offset: 0x000A6D30
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06002DE2 RID: 11746 RVA: 0x000A8B34 File Offset: 0x000A6D34
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06002DE3 RID: 11747 RVA: 0x000A8B38 File Offset: 0x000A6D38
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06002DE4 RID: 11748 RVA: 0x000A8B3C File Offset: 0x000A6D3C
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06002DE5 RID: 11749 RVA: 0x000A8B40 File Offset: 0x000A6D40
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06002DE6 RID: 11750 RVA: 0x000A8B44 File Offset: 0x000A6D44
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06002DE7 RID: 11751 RVA: 0x000A8B48 File Offset: 0x000A6D48
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06002DE8 RID: 11752 RVA: 0x000A8B4C File Offset: 0x000A6D4C
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06002DE9 RID: 11753 RVA: 0x000A8B50 File Offset: 0x000A6D50
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06002DEA RID: 11754 RVA: 0x000A8B54 File Offset: 0x000A6D54
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x06002DEB RID: 11755 RVA: 0x000A8B58 File Offset: 0x000A6D58
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x040008AC RID: 2220
		private readonly Stack<\u0015.\u0001> \u0001 = new Stack<\u0015.\u0001>();

		// Token: 0x040008AD RID: 2221
		private readonly _ICompileContext \u0001;

		// Token: 0x040008AE RID: 2222
		private readonly bool \u0001;

		// Token: 0x040008AF RID: 2223
		private readonly Guid \u0001;

		// Token: 0x040008B0 RID: 2224
		private readonly int \u0001;

		// Token: 0x040008B1 RID: 2225
		[CompilerGenerated]
		private readonly IScope \u0001;

		// Token: 0x040008B2 RID: 2226
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x020002EE RID: 750
		private sealed class \u0001
		{
			// Token: 0x040008B3 RID: 2227
			public \u0083.\u0001 \u0001;

			// Token: 0x040008B4 RID: 2228
			public int \u0001;

			// Token: 0x040008B5 RID: 2229
			public int \u0002;

			// Token: 0x040008B6 RID: 2230
			public int \u0003;
		}
	}
}
