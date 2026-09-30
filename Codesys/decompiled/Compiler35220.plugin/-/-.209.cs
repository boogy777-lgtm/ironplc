using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using \u0003;
using \u0007;
using \u000E;
using \u0011;
using \u0012;
using \u0014;
using \u0016;
using \u0017;
using \u0018;
using \u0019;
using \u001A;
using \u001B;
using \u001C;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0004
{
	// Token: 0x02000242 RID: 578
	internal sealed class \u000E : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x170006FA RID: 1786
		// (get) Token: 0x060025CE RID: 9678 RVA: 0x000832EC File Offset: 0x000814EC
		private LateCodeGenerator LateCodeGenerator { get; }

		// Token: 0x170006FB RID: 1787
		// (get) Token: 0x060025CF RID: 9679 RVA: 0x000832F4 File Offset: 0x000814F4
		private IScope5 ScopeRef { get; }

		// Token: 0x170006FC RID: 1788
		// (get) Token: 0x060025D0 RID: 9680 RVA: 0x000832FC File Offset: 0x000814FC
		public _ICompileContext CompCon { get; }

		// Token: 0x170006FD RID: 1789
		// (get) Token: 0x060025D1 RID: 9681 RVA: 0x00083304 File Offset: 0x00081504
		// (set) Token: 0x060025D2 RID: 9682 RVA: 0x0008330C File Offset: 0x0008150C
		public IScope5 _Scope { get; set; }

		// Token: 0x170006FE RID: 1790
		// (get) Token: 0x060025D3 RID: 9683 RVA: 0x00083318 File Offset: 0x00081518
		// (set) Token: 0x060025D4 RID: 9684 RVA: 0x00083320 File Offset: 0x00081520
		public int NMaxScratchSize { get; set; }

		// Token: 0x170006FF RID: 1791
		// (get) Token: 0x060025D5 RID: 9685 RVA: 0x0008332C File Offset: 0x0008152C
		// (set) Token: 0x060025D6 RID: 9686 RVA: 0x00083334 File Offset: 0x00081534
		public _ICompiledPOU Cpou { get; private set; }

		// Token: 0x17000700 RID: 1792
		// (get) Token: 0x060025D7 RID: 9687 RVA: 0x00083340 File Offset: 0x00081540
		// (set) Token: 0x060025D8 RID: 9688 RVA: 0x00083348 File Offset: 0x00081548
		public int NMaxParamSize { get; set; }

		// Token: 0x060025D9 RID: 9689 RVA: 0x00083354 File Offset: 0x00081554
		public \u000E(IScope5 \u0015\u0004, _IDataManager \u0006\u0004, IExprementReplacer[] \u0011\u0006, global::\u000E.\u0011 \u0083\u0005)
		{
			this.\u0001 = \u0083\u0005._Scope;
			this.\u0001 = \u0006\u0004;
			this.CompCon = \u0083\u0005.Comcon;
			this.\u0001 = \u0083\u0005.CodeGen;
			this._Scope = this.\u0001;
			this.\u0001.\u0001();
			this.LateCodeGenerator = new LateCodeGenerator(this.CompCon);
			this.ScopeRef = \u0015\u0004;
			this.\u0001 = \u0011\u0006;
			this.\u0001 = new global::\u0016.\u000F(this, \u0083\u0005);
			this.\u0001 = new global::\u001B.\u0006(this);
		}

		// Token: 0x060025DA RID: 9690 RVA: 0x000833F4 File Offset: 0x000815F4
		public void \u0002(_ICompiledPOU \u0002)
		{
			this.Cpou = \u0002;
			\u0002.ScratchSize = 0;
			this.NMaxScratchSize = 0;
			this.NMaxParamSize = 0;
			\u0002.GetParseTree().Accept(this);
			while (this.NMaxScratchSize % this.\u0001.StackAlignment != 0)
			{
				int num = this.NMaxScratchSize;
				this.NMaxScratchSize = num + 1;
			}
			\u0002.ScratchSize = this.NMaxScratchSize;
			this.NMaxScratchSize = 0;
			\u0002.MaxParamSize = this.NMaxParamSize;
			this.NMaxParamSize = 0;
			this.Cpou = null;
		}

		// Token: 0x060025DB RID: 9691 RVA: 0x0008347C File Offset: 0x0008167C
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060025DC RID: 9692 RVA: 0x000834A8 File Offset: 0x000816A8
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
		}

		// Token: 0x060025DD RID: 9693 RVA: 0x000834D4 File Offset: 0x000816D4
		public void \u0001(_IForStatement \u0002)
		{
			\u0002._CounterStart.Accept(this);
			_IExpression condition = \u0002._Condition;
			if (condition != null)
			{
				condition.Accept(this);
			}
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._UpperBound.Accept(this);
			this.\u0001();
			_IExpression counter = \u0002._Counter;
			if (counter != null)
			{
				counter.Accept(this);
			}
			if (\u0002.By != null)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				\u0002._By.Accept(this);
				this.\u0001();
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060025DE RID: 9694 RVA: 0x00083560 File Offset: 0x00081760
		public _IExpression \u0001(string \u0002, global::\u0011.\u0007 \u0003, bool \u0004)
		{
			_IExpression iexpression = this.LateCodeGenerator.Pass2GenerateExpression(\u0002, this._Scope, this.Cpou, \u0004);
			this.\u0001(\u0003, iexpression);
			return iexpression;
		}

		// Token: 0x060025DF RID: 9695 RVA: 0x00083590 File Offset: 0x00081790
		private void \u0001(global::\u0011.\u0007 \u0002, _IExprement \u0003)
		{
			if (\u0002 == null)
			{
				return;
			}
			this.\u0001.\u0001(\u0002);
			\u0003.Accept(this);
			this.\u0001.\u0002();
		}

		// Token: 0x060025E0 RID: 9696 RVA: 0x000835B4 File Offset: 0x000817B4
		public \u0001 \u0001<\u0001>(\u0001 \u0002, global::\u0011.\u0007 \u0003, bool \u0004, bool \u0005, bool \u0006) where \u0001 : _IExprement
		{
			this.LateCodeGenerator.Pass2AttributeExprement(\u0002, this._Scope, this.Cpou, \u0004, \u0005, \u0006);
			IExprementReplacer[] u = this.\u0001;
			for (int i = 0; i < u.Length; i++)
			{
				u[i].ReplaceExprement(\u0002, this.Cpou);
			}
			this.\u0001(\u0003, \u0002);
			return \u0002;
		}

		// Token: 0x060025E1 RID: 9697 RVA: 0x0008361C File Offset: 0x0008181C
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				try
				{
					this.\u0001.\u0001();
					this.\u0001.\u0001();
					istatement.Accept(this);
					this.\u0001.\u0001();
				}
				catch (Exception u)
				{
					this.\u0001(istatement, u);
				}
			}
		}

		// Token: 0x060025E2 RID: 9698 RVA: 0x000836A4 File Offset: 0x000818A4
		private void \u0001(_IExprement \u0002, Exception \u0003)
		{
			string text = string.Format("Internal error in _IStatement (maybe try to simplify expressions): {0}:{1}{2}", \u0002.ToString(), Environment.NewLine, \u0003);
			Debug.\u0001(false, text);
			\u0002.AddError(text);
			try
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.CompCon[this.Cpou.SignatureId].LibraryPath);
				Guid objectGuid = this.Cpou.ObjectGuid;
				IMinimalPosition position = \u0002._Position;
				long u = (position != null) ? position.EditorPosition : -1L;
				IMinimalPosition position2 = \u0002._Position;
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(projectHandle, objectGuid, u, (position2 != null) ? position2.PositionOffset : 0, \u0002.LengthIntern), text, Severity.FatalError, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory, message);
			}
			catch
			{
			}
		}

		// Token: 0x060025E3 RID: 9699 RVA: 0x00083778 File Offset: 0x00081978
		public void \u0001(_IAssignmentExpression \u0002)
		{
			\u0002.Info = new global::\u0018.\u0005();
			\u0002.Info.HasSideEffect = true;
			int u = this.TopOfStack.CurrentScratchOffset;
			if (this.TopOfStack.AccessModeFlags == AccessModeFlags.OutParameter)
			{
				IExpression u2 = this.TopOfStack.Callee;
				this.\u0001(0, null, null, AccessModeFlags.Read | AccessModeFlags.Parameter);
				this.TopOfStack.Callee = u2;
			}
			else
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
			}
			this.TopOfStack.CurrentScratchOffset = u;
			\u0002._RValue.Accept(this);
			this.\u0001();
			if (this.TopOfStack.AccessModeFlags == AccessModeFlags.Parameter)
			{
				IExpression u3 = this.TopOfStack.Callee;
				if (\u0002._LValue.Type.Class == TypeClass.Reference)
				{
					this.\u0001(0, null, null, AccessModeFlags.Write | AccessModeFlags.Address | AccessModeFlags.Parameter);
				}
				else
				{
					this.\u0001(0, null, null, AccessModeFlags.Write | AccessModeFlags.Parameter);
				}
				this.TopOfStack.NoReferenceDeRef = true;
				this.TopOfStack.Callee = u3;
			}
			else if (\u0002.KindOf == Operator.SetAssign)
			{
				this.\u0001(0, null, null, AccessModeFlags.Set);
			}
			else if (\u0002.KindOf == Operator.ResetAssign)
			{
				this.\u0001(0, null, null, AccessModeFlags.Reset);
			}
			else
			{
				this.\u0001(0, null, null, AccessModeFlags.Write);
			}
			\u0002._LValue.Accept(this);
			this.\u0001();
		}

		// Token: 0x060025E4 RID: 9700 RVA: 0x000838B8 File Offset: 0x00081AB8
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Condition.Accept(this);
			this.\u0001();
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				ielseIf._Condition.Accept(this);
				this.\u0001();
				ielseIf._Controlled.Accept(this);
			}
			if (\u0002._IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x060025E5 RID: 9701 RVA: 0x00083960 File Offset: 0x00081B60
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				\u0002._Condition.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x060025E6 RID: 9702 RVA: 0x00083988 File Offset: 0x00081B88
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				\u0002._Condition.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x060025E7 RID: 9703 RVA: 0x000839B0 File Offset: 0x00081BB0
		public void \u0001(_IPragmaStatement \u0002)
		{
			int nId;
			if (\u0002.IsLocalSignaturePragma(out nId))
			{
				this._Scope.LocalSignature = this._Scope[nId];
				if (this.ScopeRef != null)
				{
					this.ScopeRef.LocalSignature = this.ScopeRef[nId];
				}
			}
		}

		// Token: 0x060025E8 RID: 9704 RVA: 0x00083A00 File Offset: 0x00081C00
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Expr.Accept(this);
			this.\u0001();
		}

		// Token: 0x060025E9 RID: 9705 RVA: 0x00083A20 File Offset: 0x00081C20
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001.\u0001(\u0002);
		}

		// Token: 0x060025EA RID: 9706 RVA: 0x00083A30 File Offset: 0x00081C30
		public void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			Operator code = \u0002.Code;
			if (code <= Operator.Lt)
			{
				if (code <= Operator.Max)
				{
					if (code != Operator.Adr)
					{
						if (code - Operator.Limit > 2)
						{
							goto IL_A1;
						}
						goto IL_98;
					}
				}
				else
				{
					if (code - Operator.Mux <= 1)
					{
						this.\u0003(\u0002);
						goto IL_A8;
					}
					if (code - Operator.Eq > 5)
					{
						goto IL_A1;
					}
					goto IL_98;
				}
			}
			else if (code <= Operator.NotEqual)
			{
				if (code != Operator.TestAndSet)
				{
					if (code - Operator.Less > 5)
					{
						goto IL_A1;
					}
					goto IL_98;
				}
			}
			else if (code != Operator.__RefAdr)
			{
				if (code == Operator.__FCall)
				{
					this.\u0004(\u0002);
					return;
				}
				goto IL_A1;
			}
			this.\u0001(0, null, null, AccessModeFlags.ReadAddress);
			operandsList[0].Accept(this);
			this.\u0001();
			goto IL_A8;
			IL_98:
			this.\u0002(\u0002);
			goto IL_A8;
			IL_A1:
			this.\u0001(operandsList);
			IL_A8:
			\u0002.Info = global::\u0004.\u000E.\u0001(operandsList);
		}

		// Token: 0x060025EB RID: 9707 RVA: 0x00083AF4 File Offset: 0x00081CF4
		private static global::\u0018.\u0005 \u0001(IList<_IExpression> \u0002)
		{
			int num = 0;
			global::\u0018.\u0005 u = new global::\u0018.\u0005();
			foreach (_IExpression iexpression in \u0002)
			{
				if (iexpression.Info != null && iexpression.Info.NestingDepth > num)
				{
					num = iexpression.Info.NestingDepth;
				}
				if (iexpression.Info != null && iexpression.Info.HasSideEffect)
				{
					u.HasSideEffect = true;
				}
			}
			u.NestingDepth = num + 1;
			return u;
		}

		// Token: 0x060025EC RID: 9708 RVA: 0x00083B88 File Offset: 0x00081D88
		private void \u0002(_IOperatorExpression \u0002)
		{
			Debug.\u0001(\u0002._OperandsList.Count > 0);
			if (\u0084.\u0004.\u0001(\u0002[0].Type, this._Scope, this.\u0001))
			{
				this.\u0001(\u0002._OperandsList);
				return;
			}
			int num = this.TopOfStack.CurrentScratchOffset;
			foreach (_IExprement iexprement in \u0002._OperandsList)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				this.TopOfStack.CurrentScratchOffset = num;
				iexprement.Accept(this);
				num = this.TopOfStack.CurrentScratchOffset;
				this.\u0001();
			}
			Operator code = \u0002.Code;
			if (code - Operator.Limit <= 2)
			{
				bool flag = this.\u0001.\u0001(CodegeneratorProperties.PositiveStackGrow);
				int num2 = \u0002.Type.Size(this._Scope);
				if (flag)
				{
					\u0002.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
				}
				this.TopOfStack.CurrentScratchOffset += num2;
				if (!flag)
				{
					\u0002.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
				}
				this.NMaxScratchSize = Math.Max(Math.Max(this.TopOfStack.CurrentScratchOffset, num), this.NMaxScratchSize);
				return;
			}
			this.NMaxScratchSize = Math.Max(this.NMaxScratchSize, num);
		}

		// Token: 0x060025ED RID: 9709 RVA: 0x00083CE4 File Offset: 0x00081EE4
		private void \u0003(_IOperatorExpression \u0002)
		{
			if (\u0084.\u0004.\u0001(\u0002.Type, this._Scope, this.\u0001))
			{
				this.\u0001(\u0002._OperandsList);
				return;
			}
			int u = this.TopOfStack.CurrentScratchOffset;
			foreach (_IExprement iexprement in \u0002._OperandsList)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				this.TopOfStack.CurrentScratchOffset = u;
				iexprement.Accept(this);
				this.\u0001();
			}
			bool flag = this.\u0001.\u0001(CodegeneratorProperties.PositiveStackGrow);
			int num = \u0002.Type.Size(this._Scope);
			if (flag)
			{
				\u0002.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
			}
			this.TopOfStack.CurrentScratchOffset += num;
			if (!flag)
			{
				\u0002.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
			}
			this.NMaxScratchSize = Math.Max(this.TopOfStack.CurrentScratchOffset, this.NMaxScratchSize);
		}

		// Token: 0x060025EE RID: 9710 RVA: 0x00083DF4 File Offset: 0x00081FF4
		private void \u0001(IList<_IExpression> \u0002)
		{
			foreach (_IExprement iexprement in \u0002)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				iexprement.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x060025EF RID: 9711 RVA: 0x00083E4C File Offset: 0x0008204C
		private void \u0004(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_IExpression iexpression = operandsList[1];
			iexpression.Type = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(TypeTable.Byte));
			iexpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
			iexpression.Type = global::\u0019.\u0003.\u0001(TypeTable.Byte);
			operandsList[1] = iexpression;
			foreach (_IExprement iexprement in operandsList)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				iexprement.Accept(this);
				this.\u0001();
			}
			_IExpression iexpression2 = operandsList[0];
			_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(iexpression2, iexpression), Token.Empty);
			ISignature signature = ((_IUserdefType)iexpression2.Type).GetSignature(this._Scope);
			for (int i = 2; i < operandsList.Count; i++)
			{
				_IVariable ivariable = (_IVariable)signature.AllInputs[i - 2];
				_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(ivariable.VersionedName);
				ivariableExpression.VariableId = ivariable.Id;
				ivariableExpression.SignatureId = signature.Id;
				ivariableExpression.Type = ivariable._Type;
				this.\u0001(0, null, null, AccessModeFlags.Write | AccessModeFlags.Parameter);
				ivariableExpression.Accept(this);
				this.\u0001();
				icallExpression.AddParam(operandsList[i], ivariableExpression);
			}
			if (\u0002.Type != null && !\u0084.\u0004.\u0001(\u0002.Type, this._Scope, this.\u0001))
			{
				Debug.\u0001(signature.Outputs.Length != 0);
				_IVariable ivariable2 = (_IVariable)signature.Outputs[0];
				bool flag = this.\u0001.\u0001(CodegeneratorProperties.PositiveStackGrow);
				if (flag)
				{
					icallExpression.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
				}
				this.TopOfStack.CurrentScratchOffset += ivariable2._Type.Size(this._Scope);
				if (!flag)
				{
					icallExpression.ScratchOffset = this.TopOfStack.CurrentScratchOffset;
				}
				this.NMaxScratchSize = Math.Max(this.TopOfStack.CurrentScratchOffset, this.NMaxScratchSize);
			}
			icallExpression.CallInfo = new global::\u0012.\u0011
			{
				IdCalledSignature = signature.Id,
				KindOfCall = KindOfCall.StaticFunctionCall,
				ExpLoadTargetAddress = iexpression,
				InstanceAssignment = null
			};
			\u0002.AddOperand(icallExpression);
		}

		// Token: 0x060025F0 RID: 9712 RVA: 0x00084094 File Offset: 0x00082294
		public void \u0001(_ICastExpression \u0002)
		{
			\u0002.BaseExpression.Accept(this);
		}

		// Token: 0x060025F1 RID: 9713 RVA: 0x000840A4 File Offset: 0x000822A4
		public void \u0001(_IConversionExpression \u0002)
		{
			if (this.TopOfStack.AccessModeFlags.HasFlag(AccessModeFlags.Parameter))
			{
				this.\u0001(0, null, null, this.TopOfStack.AccessModeFlags);
			}
			else
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
			}
			\u0002._Exp.Accept(this);
			this.\u0001();
		}

		// Token: 0x060025F2 RID: 9714 RVA: 0x00084100 File Offset: 0x00082300
		public void \u0001(_ILiteralExpression \u0002)
		{
			\u0002.Type = global::\u0004.\u000E.\u0001(\u0002.Type);
		}

		// Token: 0x060025F3 RID: 9715 RVA: 0x00084114 File Offset: 0x00082314
		public void \u0001(_IAddressExpression \u0002)
		{
			global::\u0018.\u0004 u = global::\u0018.\u0004.\u0001();
			\u0002.Info = u;
			IMessage message;
			bool flag;
			u.DataLocation = Locator.\u0001(this.CompCon, out message, out flag, \u0002.Position, \u0002.DirectAddress, null);
			u.AccessMode.SetAccessMode(this.TopOfStack.AccessModeFlags, true);
			u.CompiledType = \u0002.Type;
			if (u.DataLocation.IsBitLocation)
			{
				u.WithoutBit = (_IAddressExpression)\u0002.Duplicate();
				global::\u0018.\u0004 u2 = global::\u0018.\u0004.\u0001();
				u.WithoutBit.Info = u2;
				u2.DataLocation = global::\u0019.\u0003.\u0001(u.DataLocation.Area, u.DataLocation.Offset);
				u2.AccessMode.SetAccessMode(AccessModeFlags.Read, true);
				u2.CompiledType = TypeTable.Byte;
			}
		}

		// Token: 0x060025F4 RID: 9716 RVA: 0x000841DC File Offset: 0x000823DC
		public void \u0001(_IVariableExpression \u0002)
		{
			if (\u0002.VariableId == Helper.InvalidId)
			{
				this.\u0003(\u0002);
				return;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x060025F5 RID: 9717 RVA: 0x000841FC File Offset: 0x000823FC
		private void \u0002(_IVariableExpression \u0002)
		{
			int variableId = \u0002.VariableId;
			_ISignature isignature = (_ISignature)this._Scope[\u0002.SignatureId];
			\u001D.\u0007 u = \u001D.\u0007.\u0001();
			\u0002.VarInfo = u;
			_IVariable ivariable = (_IVariable)isignature[variableId];
			\u0002.Type = global::\u0004.\u000E.\u0001(\u0002.Type);
			ICompiledType compiledType = global::\u0004.\u000E.\u0001(this.TopOfStack.CurrentType ?? ivariable.CompiledType);
			if (ivariable.GetFlag(VarFlag.ReplacedConstant))
			{
				u.VariableId = ivariable.Id;
				u.SignatureId = isignature.Id;
				u.CompiledType = compiledType;
				return;
			}
			int u2 = 0;
			if (ivariable.GetFlag(VarFlag.Absolut))
			{
				u2 = (int)ivariable.DataLocation.Area;
			}
			u.Area = u2;
			u.Address = ivariable.DataLocation.Offset;
			u.Offset = this.TopOfStack.Offset;
			u.VariableId = ivariable.Id;
			u.SignatureId = isignature.Id;
			u.CompiledType = compiledType;
			u.PackMode = this.TopOfStack.CurrentPackMode;
			if (this.TopOfStack.IndexInfo != null)
			{
				this.TopOfStack.IndexInfo.\u0001(this);
			}
			u.IndexInfo = this.TopOfStack.IndexInfo;
			u.BitNr = (ivariable.DataLocation.IsBitLocation ? ivariable.DataLocation.BitNr : this.TopOfStack.BitOffset);
			u.AccessMode.SetAccessMode(this.TopOfStack.AccessModeFlags, true);
			this.\u0001(u);
			if (compiledType.Class == TypeClass.Reference)
			{
				this.TopOfStack.VirtualFunctionCall = true;
			}
		}

		// Token: 0x060025F6 RID: 9718 RVA: 0x000843A4 File Offset: 0x000825A4
		private void \u0001(\u001D.\u0007 \u0002)
		{
			if (\u0002.PackMode >= 0 && \u0002.BitNr >= 0 && \u0002.BitNr <= 63)
			{
				if (!this.TopOfStack.AccessModeFlags.GetFlag(AccessModeFlags.Write) && !this.TopOfStack.AccessModeFlags.GetFlag(AccessModeFlags.Set) && !this.TopOfStack.AccessModeFlags.GetFlag(AccessModeFlags.Reset))
				{
					return;
				}
				if (!(this.\u0001 is IRiscFrontEnd))
				{
					return;
				}
				int num = \u0002.CompiledType.Size(this._Scope);
				if (num > 1 && num <= 8)
				{
					if (this._Scope.Codegenerator.MotorolaByteOrder)
					{
						\u0002.Offset += num - 1 - (int)(this.TopOfStack.BitOffset / 8);
						\u0002.BitNr = this.TopOfStack.BitOffset % 8;
						\u0002.CompiledType = TypeTable.Byte;
						return;
					}
					\u0002.Offset += (int)(this.TopOfStack.BitOffset / 8);
					\u0002.BitNr = this.TopOfStack.BitOffset % 8;
					\u0002.CompiledType = TypeTable.Byte;
				}
			}
		}

		// Token: 0x060025F7 RID: 9719 RVA: 0x000844CC File Offset: 0x000826CC
		private void \u0003(_IVariableExpression \u0002)
		{
			int signatureId = \u0002.SignatureId;
			_ISignature isignature = this._Scope[signatureId] as _ISignature;
			\u001D.\u0007 u = \u001D.\u0007.\u0001();
			\u0002.VarInfo = u;
			if (isignature == null)
			{
				u.DoGeneration = false;
				return;
			}
			if (this.TopOfStack.AccessModeFlags.GetFlag(AccessModeFlags.Call))
			{
				int num = this.TopOfStack.SignatureId;
				if (num == Helper.InvalidId)
				{
					num = signatureId;
					this.TopOfStack.SignatureId = signatureId;
				}
				u.AccessMode.SetAccessMode(this.TopOfStack.AccessModeFlags, true);
				u.SignatureToCall = num;
				if (isignature.CanSignatureBeVirtual(this._Scope))
				{
					this.TopOfStack.VirtualFunctionCall |= this._Scope.LocalSignature.IsMethodInInheritanceChainOf(isignature, this._Scope);
					return;
				}
			}
			else
			{
				u.POUType = isignature.POUType;
				u.CompiledType = \u0002.Type;
			}
		}

		// Token: 0x060025F8 RID: 9720 RVA: 0x000845B0 File Offset: 0x000827B0
		public static ICompiledType \u0001(ICompiledType \u0002)
		{
			if (\u0002 != null && (\u0002.Class == TypeClass.Enum || \u0002.Class == TypeClass.Subrange))
			{
				\u0002 = \u0002.DeRefType;
			}
			return \u0002;
		}

		// Token: 0x060025F9 RID: 9721 RVA: 0x000845D4 File Offset: 0x000827D4
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001.\u0001(\u0002);
		}

		// Token: 0x060025FA RID: 9722 RVA: 0x000845E4 File Offset: 0x000827E4
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			global::\u0003.\u000F u000F = new global::\u0003.\u000F();
			\u0002.Info = u000F;
			\u0002.Info.DoGeneration = false;
			_IVariableExpression ivariableExpression = \u0002._Right as _IVariableExpression;
			_ISignature isignature = null;
			IVariable variable = null;
			if (ivariableExpression != null)
			{
				int signatureId = ivariableExpression.SignatureId;
				int variableId = ivariableExpression.VariableId;
				ISignature u = this._Scope[signatureId];
				if (variableId == Helper.InvalidId)
				{
					this.\u0001(\u0002, u000F, signatureId, u);
					return;
				}
				isignature = (_ISignature)this._Scope[signatureId];
				variable = isignature[variableId];
				if (ivariableExpression.Type.Class == TypeClass.Enum || ivariableExpression.Type.Class == TypeClass.Subrange)
				{
					ivariableExpression.Type = ivariableExpression.Type.DeRefType;
					\u0002.Type = \u0002.Type.DeRefType;
				}
			}
			bool flag = variable != null && variable.GetFlag(VarFlag.ReplacedConstant);
			if (!flag)
			{
				flag = (variable != null && variable.GetFlag(VarFlag.Constant));
			}
			if (this.\u0001(\u0002, ivariableExpression, flag))
			{
				return;
			}
			Debug.\u0001(variable != null);
			if (this.\u0001(variable, u000F, \u0002))
			{
				return;
			}
			if (this.\u0002(variable, u000F, \u0002))
			{
				return;
			}
			this.\u0001(\u0002, u000F, isignature, variable);
			global::\u0004.\u000E.\u0001(\u0002);
		}

		// Token: 0x060025FB RID: 9723 RVA: 0x00084718 File Offset: 0x00082918
		private void \u0001(_ICompoAccessExpression \u0002, global::\u0003.\u000F \u0003, _ISignature \u0004, IVariable \u0005)
		{
			int u = this.TopOfStack.Offset;
			this.TopOfStack.Offset += \u0005.DataLocation.Offset;
			if (\u0005.DataLocation.IsBitLocation)
			{
				this.TopOfStack.BitOffset = \u0005.DataLocation.BitNr;
			}
			if (\u0004 != null && \u0004.PackMode >= 0 && \u0004.PackMode <= 4)
			{
				this.TopOfStack.CurrentPackMode = \u0004.PackMode;
			}
			if (this.TopOfStack.CurrentType == null)
			{
				this.TopOfStack.CurrentType = \u0002._Right.Type;
			}
			AccessModeFlags accessModeFlags = this.TopOfStack.AccessModeFlags;
			if (accessModeFlags.HasFlag(AccessModeFlags.Call))
			{
				this.TopOfStack.AccessModeFlags = AccessModeFlags.Read;
			}
			if (accessModeFlags.HasFlag(AccessModeFlags.Parameter))
			{
				IVariable variable = \u0002._Right.GetVariable(this._Scope);
				if (variable != null && variable.GetFlag(VarFlag.RelativeInstance) && !variable.GetFlag(VarFlag.Union) && !\u0002._Left.GetVariable(this._Scope).IsSpecialParameter())
				{
					this.TopOfStack.AccessModeFlags &= ~AccessModeFlags.Parameter;
				}
			}
			\u0002._Left.Accept(this);
			ICompiledType compiledType = \u0005.CompiledType;
			if (compiledType != null && compiledType.Class == TypeClass.Array)
			{
				compiledType = \u0084.\u0004.\u0001(compiledType);
			}
			this.\u0001(\u0002, compiledType, accessModeFlags);
			this.TopOfStack.VirtualFunctionCall = false;
			\u001D.\u0007 u2 = \u0002._Left.Info as \u001D.\u0007;
			if (u2 != null && u2.POUType != Operator.None)
			{
				this.TopOfStack.Offset = u;
				\u0002._Right.Accept(this);
				\u0003.GenerateRight = true;
			}
		}

		// Token: 0x060025FC RID: 9724 RVA: 0x000848D8 File Offset: 0x00082AD8
		private void \u0001(_ICompoAccessExpression \u0002, ICompiledType \u0003, AccessModeFlags \u0004)
		{
			bool flag = \u0003 != null && \u0003.Class == TypeClass.Userdef;
			if (\u0004.HasFlag(AccessModeFlags.Call) && !flag)
			{
				string u = string.Format("Internal Error {0}, problem with callee {1}", 6, \u0002.ToString());
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.None);
				IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
				if (this.\u0001.LocalSignature != null)
				{
					icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(this.\u0001.LocalSignature.LibraryPath);
					icompilerMessage.ObjectGuid = this.\u0001.LocalSignature.ObjectGuid;
				}
				APEnvironmentFacade.Instance.AddMessage(messageCategory, icompilerMessage);
			}
		}

		// Token: 0x060025FD RID: 9725 RVA: 0x000849A0 File Offset: 0x00082BA0
		private static void \u0001(_ICompoAccessExpression \u0002)
		{
			_IDeRefAccessExpression ideRefAccessExpression = \u0002._Left as _IDeRefAccessExpression;
			if (ideRefAccessExpression != null)
			{
				_IVariableExpression ivariableExpression = ideRefAccessExpression._Base as _IVariableExpression;
				if (ivariableExpression != null && ivariableExpression.Name == IdentifierConstants.InstancePointer)
				{
					((global::\u0014.\u000F)ideRefAccessExpression.Info).InstanceAccess = true;
				}
			}
		}

		// Token: 0x060025FE RID: 9726 RVA: 0x000849F0 File Offset: 0x00082BF0
		private bool \u0001(_ICompoAccessExpression \u0002, IVariableExpression \u0003, bool \u0004)
		{
			if (TypeTable.IsNumber(\u0002._Left.Type.DeRefType.Class) && (\u0003 == null || \u0004))
			{
				byte u = (byte)Helper.\u0001(\u0002._Right.Literal(this._Scope));
				this.TopOfStack.CurrentType = \u0002._Left.Type.DeRefType;
				this.TopOfStack.BitOffset = u;
				this.TopOfStack.Offset = 0;
				\u0002._Left.Accept(this);
				return true;
			}
			return false;
		}

		// Token: 0x060025FF RID: 9727 RVA: 0x00084A7C File Offset: 0x00082C7C
		private bool \u0001(IVariable \u0002, global::\u0003.\u000F \u0003, _ICompoAccessExpression \u0004)
		{
			if (\u0002.GetFlag(VarFlag.ReplacedConstant))
			{
				\u0003.GenerateRight = true;
				\u0004._Right.Accept(this);
				return true;
			}
			return false;
		}

		// Token: 0x06002600 RID: 9728 RVA: 0x00084AA0 File Offset: 0x00082CA0
		private bool \u0002(IVariable \u0002, global::\u0003.\u000F \u0003, _ICompoAccessExpression \u0004)
		{
			if (!\u0002.DataLocation.IsRelativ)
			{
				\u0003.GenerateRight = true;
				\u0004._Right.Accept(this);
				return true;
			}
			return false;
		}

		// Token: 0x06002601 RID: 9729 RVA: 0x00084AC8 File Offset: 0x00082CC8
		private void \u0001(_ICompoAccessExpression \u0002, global::\u0003.\u000F \u0003, int \u0004, ISignature \u0005)
		{
			if (this.TopOfStack.AccessModeFlags.HasFlag(AccessModeFlags.Call))
			{
				int num = \u0004;
				if (\u0004 == Helper.InvalidId)
				{
					num = this.TopOfStack.SignatureId;
				}
				this.\u0003(num);
				this.TopOfStack.AccessModeFlags = AccessModeFlags.Read;
				\u0002._Left.Accept(this);
				bool u = this.TopOfStack.VirtualFunctionCall;
				this.\u0001();
				this.TopOfStack.SignatureId = num;
				this.TopOfStack.VirtualFunctionCall = u;
				ISignature signature = this._Scope[num];
				if (signature != null && (signature.GetFlag(SignatureFlag.Final) || signature.GetFlag(SignatureFlag.Private)))
				{
					this.TopOfStack.VirtualFunctionCall = false;
					return;
				}
			}
			else
			{
				if (\u0005 != null && \u0005.GetFlag(SignatureFlag.Action))
				{
					\u0002._Left.Accept(this);
					return;
				}
				\u0002._Right.Accept(this);
				\u0003.GenerateRight = true;
			}
		}

		// Token: 0x06002602 RID: 9730 RVA: 0x00084BC8 File Offset: 0x00082DC8
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			if (\u0002.Type.Class == TypeClass.Enum || \u0002.Type.Class == TypeClass.Subrange)
			{
				\u0002.Type = \u0002.Type.DeRefType;
			}
			global::\u0014.\u000F u000F = new global::\u0014.\u000F();
			u000F.AccessMode.SetAccessMode(this.TopOfStack.AccessModeFlags, true);
			u000F.BitNr = this.TopOfStack.BitOffset;
			u000F.CompiledType = this.TopOfStack.CurrentType;
			u000F.PackMode = this.TopOfStack.CurrentPackMode;
			if (u000F.CompiledType == null)
			{
				u000F.CompiledType = \u0002.Type;
			}
			u000F.Offset = this.TopOfStack.Offset;
			if (this.TopOfStack.IndexInfo != null)
			{
				this.TopOfStack.IndexInfo.\u0001(this);
			}
			u000F.IndexInfo = this.TopOfStack.IndexInfo;
			u000F.PackMode = this.TopOfStack.CurrentPackMode;
			\u0002.DeRefInfo = u000F;
			this.\u0001(0, null, null, AccessModeFlags.Read);
			this.TopOfStack.BitOffset = byte.MaxValue;
			this.TopOfStack.IndexInfo = null;
			\u0002._Base.Accept(this);
			this.\u0001();
			_IVariableExpression ivariableExpression = \u0002._Base as _IVariableExpression;
			if (ivariableExpression != null && ivariableExpression.NoVirtual)
			{
				this.TopOfStack.VirtualFunctionCall = false;
				return;
			}
			this.TopOfStack.VirtualFunctionCall = true;
		}

		// Token: 0x06002603 RID: 9731 RVA: 0x00084D28 File Offset: 0x00082F28
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			IScope5 scope = this._Scope;
			this._Scope = ((global::\u0007.\u0005)this._Scope)._CopyScope;
			bool flag = false;
			if (this._Scope.LocalSignature == null)
			{
				this._Scope.LocalSignature = scope.LocalSignature;
				flag = true;
			}
			if (this.TopOfStack != null && this.TopOfStack.AccessModeFlags.HasFlag(AccessModeFlags.Address))
			{
				this.\u0001(0, null, null, this.TopOfStack.AccessModeFlags);
			}
			else
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
			}
			\u0002._Base.Accept(this);
			this.\u0001();
			this._Scope = scope;
			if (flag)
			{
				this.ScopeRef.LocalSignature = null;
			}
		}

		// Token: 0x06002604 RID: 9732 RVA: 0x00084DE4 File Offset: 0x00082FE4
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002605 RID: 9733 RVA: 0x00084DF4 File Offset: 0x00082FF4
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002606 RID: 9734 RVA: 0x00084E04 File Offset: 0x00083004
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002607 RID: 9735 RVA: 0x00084E14 File Offset: 0x00083014
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			\u0002._Access.Accept(this);
		}

		// Token: 0x06002608 RID: 9736 RVA: 0x00084E24 File Offset: 0x00083024
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x06002609 RID: 9737 RVA: 0x00084E34 File Offset: 0x00083034
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Low.Accept(this);
			this.\u0001();
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._High.Accept(this);
			this.\u0001();
		}

		// Token: 0x0600260A RID: 9738 RVA: 0x00084E70 File Offset: 0x00083070
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExprement iexprement in \u0002._cases)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				iexprement.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x0600260B RID: 9739 RVA: 0x00084ECC File Offset: 0x000830CC
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002._Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x0600260C RID: 9740 RVA: 0x00084F48 File Offset: 0x00083148
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Number.Accept(this);
			this.\u0001();
			this.\u0001(0, null, null, AccessModeFlags.Read);
			\u0002._Value.Accept(this);
			this.\u0001();
		}

		// Token: 0x0600260D RID: 9741 RVA: 0x00084F84 File Offset: 0x00083184
		public void \u0001(_IArrayInitialization \u0002)
		{
			foreach (_IExprement iexprement in \u0002._InitValues)
			{
				this.\u0001(0, null, null, AccessModeFlags.Read);
				iexprement.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x0600260E RID: 9742 RVA: 0x00084FE0 File Offset: 0x000831E0
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x0600260F RID: 9743 RVA: 0x0008502C File Offset: 0x0008322C
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			if (\u0002.PartSize == DirectVariableSize.X)
			{
				byte u = (byte)\u0002.PartOffset;
				this.TopOfStack.CurrentType = \u0002._Left._CompiledType;
				this.TopOfStack.BitOffset = u;
			}
			else
			{
				this.TopOfStack.CurrentType = (this.TopOfStack.CurrentType ?? \u0002._CompiledType);
				int num;
				int num2;
				\u0002.GetByteOffsetAndSize(this.\u0001, out num, out num2);
				this.TopOfStack.Offset += num;
			}
			\u0002._Left.Accept(this);
		}

		// Token: 0x06002610 RID: 9744 RVA: 0x000850C0 File Offset: 0x000832C0
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
		}

		// Token: 0x06002611 RID: 9745 RVA: 0x000850C4 File Offset: 0x000832C4
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06002612 RID: 9746 RVA: 0x000850C8 File Offset: 0x000832C8
		public void \u0001(_IBaseExpression \u0002)
		{
		}

		// Token: 0x06002613 RID: 9747 RVA: 0x000850CC File Offset: 0x000832CC
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06002614 RID: 9748 RVA: 0x000850D0 File Offset: 0x000832D0
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06002615 RID: 9749 RVA: 0x000850D4 File Offset: 0x000832D4
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06002616 RID: 9750 RVA: 0x000850D8 File Offset: 0x000832D8
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06002617 RID: 9751 RVA: 0x000850DC File Offset: 0x000832DC
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06002618 RID: 9752 RVA: 0x000850E0 File Offset: 0x000832E0
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x06002619 RID: 9753 RVA: 0x000850E4 File Offset: 0x000832E4
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x0600261A RID: 9754 RVA: 0x000850E8 File Offset: 0x000832E8
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x0600261B RID: 9755 RVA: 0x000850EC File Offset: 0x000832EC
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600261C RID: 9756 RVA: 0x000850F0 File Offset: 0x000832F0
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x0600261D RID: 9757 RVA: 0x000850F4 File Offset: 0x000832F4
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x0600261E RID: 9758 RVA: 0x000850F8 File Offset: 0x000832F8
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x0600261F RID: 9759 RVA: 0x000850FC File Offset: 0x000832FC
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06002620 RID: 9760 RVA: 0x00085100 File Offset: 0x00083300
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x06002621 RID: 9761 RVA: 0x00085104 File Offset: 0x00083304
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06002622 RID: 9762 RVA: 0x00085108 File Offset: 0x00083308
		public void \u0001(_IHasTypeExpression \u0002)
		{
		}

		// Token: 0x06002623 RID: 9763 RVA: 0x0008510C File Offset: 0x0008330C
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06002624 RID: 9764 RVA: 0x00085110 File Offset: 0x00083310
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06002625 RID: 9765 RVA: 0x00085114 File Offset: 0x00083314
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06002626 RID: 9766 RVA: 0x00085118 File Offset: 0x00083318
		public void \u0001(_INewExpression \u0002)
		{
		}

		// Token: 0x06002627 RID: 9767 RVA: 0x0008511C File Offset: 0x0008331C
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06002628 RID: 9768 RVA: 0x00085120 File Offset: 0x00083320
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06002629 RID: 9769 RVA: 0x00085124 File Offset: 0x00083324
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x0600262A RID: 9770 RVA: 0x00085128 File Offset: 0x00083328
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x0600262B RID: 9771 RVA: 0x0008512C File Offset: 0x0008332C
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x0600262C RID: 9772 RVA: 0x00085130 File Offset: 0x00083330
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x0600262D RID: 9773 RVA: 0x00085134 File Offset: 0x00083334
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x0600262E RID: 9774 RVA: 0x00085138 File Offset: 0x00083338
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x0600262F RID: 9775 RVA: 0x0008513C File Offset: 0x0008333C
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06002630 RID: 9776 RVA: 0x00085140 File Offset: 0x00083340
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06002631 RID: 9777 RVA: 0x00085144 File Offset: 0x00083344
		public void \u0001(_IThisExpression \u0002)
		{
		}

		// Token: 0x06002632 RID: 9778 RVA: 0x00085148 File Offset: 0x00083348
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002633 RID: 9779 RVA: 0x0008514C File Offset: 0x0008334C
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06002634 RID: 9780 RVA: 0x00085150 File Offset: 0x00083350
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x06002635 RID: 9781 RVA: 0x00085154 File Offset: 0x00083354
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06002636 RID: 9782 RVA: 0x00085158 File Offset: 0x00083358
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06002637 RID: 9783 RVA: 0x0008515C File Offset: 0x0008335C
		public void \u0001(_IVariableReference \u0002)
		{
		}

		// Token: 0x06002638 RID: 9784 RVA: 0x00085160 File Offset: 0x00083360
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06002639 RID: 9785 RVA: 0x00085164 File Offset: 0x00083364
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x17000701 RID: 1793
		// (get) Token: 0x0600263A RID: 9786 RVA: 0x00085168 File Offset: 0x00083368
		public global::\u0011.\u0007 TopOfStack
		{
			get
			{
				return this.\u0001.\u0002() ?? new global::\u0011.\u0007();
			}
		}

		// Token: 0x0600263B RID: 9787 RVA: 0x00085180 File Offset: 0x00083380
		private void \u0003(int \u0002)
		{
			this.\u0001(this.TopOfStack.Offset, this.TopOfStack.IndexInfo, this.TopOfStack.CurrentType, this.TopOfStack.AccessModeFlags, \u0002);
		}

		// Token: 0x0600263C RID: 9788 RVA: 0x000851B8 File Offset: 0x000833B8
		public void \u0001(int \u0002, global::\u0017.\u0011 \u0003, ICompiledType \u0004, AccessModeFlags \u0005)
		{
			this.\u0001(\u0002, \u0003, \u0004, \u0005, this.TopOfStack.SignatureId);
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x000851D0 File Offset: 0x000833D0
		private void \u0001(int \u0002, global::\u0017.\u0011 \u0003, ICompiledType \u0004, AccessModeFlags \u0005, int \u0006)
		{
			byte u = this.TopOfStack.BitOffset;
			bool u2 = this.TopOfStack.VirtualFunctionCall;
			\u001D.\u0007 u3 = this.TopOfStack.VarExpInfo;
			bool u4 = false;
			if (this.TopOfStack != null)
			{
				u4 = this.TopOfStack.NoReferenceDeRef;
			}
			global::\u0011.\u0007 u5 = this.\u0001.\u0001();
			u5.Offset = \u0002;
			u5.CurrentType = \u0004;
			u5.BitOffset = u;
			u5.AccessModeFlags = \u0005;
			u5.SignatureId = \u0006;
			u5.VirtualFunctionCall = u2;
			u5.IndexInfo = \u0003;
			u5.VarExpInfo = u3;
			u5.NoReferenceDeRef = u4;
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x00085264 File Offset: 0x00083464
		public void \u0001()
		{
			this.\u0001.\u0002();
		}

		// Token: 0x040006E5 RID: 1765
		private readonly IScope5 \u0001;

		// Token: 0x040006E6 RID: 1766
		private readonly _IDataManager \u0001;

		// Token: 0x040006E7 RID: 1767
		private readonly \u001C.\u000F \u0001 = new \u001C.\u000F();

		// Token: 0x040006E8 RID: 1768
		public readonly ICodegenerator \u0001;

		// Token: 0x040006E9 RID: 1769
		[CompilerGenerated]
		private readonly LateCodeGenerator \u0001;

		// Token: 0x040006EA RID: 1770
		[CompilerGenerated]
		private readonly IScope5 \u0002;

		// Token: 0x040006EB RID: 1771
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x040006EC RID: 1772
		private readonly IExprementReplacer[] \u0001;

		// Token: 0x040006ED RID: 1773
		private readonly global::\u0016.\u000F \u0001;

		// Token: 0x040006EE RID: 1774
		private readonly global::\u001B.\u0006 \u0001;

		// Token: 0x040006EF RID: 1775
		[CompilerGenerated]
		private IScope5 \u0003;

		// Token: 0x040006F0 RID: 1776
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006F1 RID: 1777
		[CompilerGenerated]
		private _ICompiledPOU \u0001;

		// Token: 0x040006F2 RID: 1778
		[CompilerGenerated]
		private int \u0002;
	}
}
