using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0001
{
	// Token: 0x020000FF RID: 255
	internal sealed class \u0003 : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x060012B9 RID: 4793 RVA: 0x00033A84 File Offset: 0x00031C84
		public \u0003(WhatToFind \u0015\u0002, ISourcePosition \u0016\u0002)
		{
			this.\u0001 = \u0015\u0002;
			this.\u0001 = \u0016\u0002;
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x060012BA RID: 4794 RVA: 0x00033AA8 File Offset: 0x00031CA8
		private AccessFlag TopOfStack
		{
			get
			{
				if (this.\u0001.Count == 0)
				{
					return AccessFlag.None;
				}
				return this.\u0001.Peek();
			}
		}

		// Token: 0x060012BB RID: 4795 RVA: 0x00033AC4 File Offset: 0x00031CC4
		private void \u0001(AccessFlag \u0002)
		{
			this.\u0001.Push(\u0002);
		}

		// Token: 0x060012BC RID: 4796 RVA: 0x00033AD4 File Offset: 0x00031CD4
		private AccessFlag \u0002()
		{
			return this.\u0001.Pop();
		}

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x060012BD RID: 4797 RVA: 0x00033AE4 File Offset: 0x00031CE4
		public _IExprement FoundExprement
		{
			get
			{
				if (this.\u0001 is _IErrorStatement)
				{
					return null;
				}
				return this.\u0001;
			}
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x060012BE RID: 4798 RVA: 0x00033AFC File Offset: 0x00031CFC
		public _IExprement FoundTypeExprement
		{
			get
			{
				if (this.\u0002 is _IErrorStatement)
				{
					return null;
				}
				return this.\u0002;
			}
		}

		// Token: 0x060012BF RID: 4799 RVA: 0x00033B14 File Offset: 0x00031D14
		public void \u0001(_ISignature \u0002)
		{
			_IExpression iexpression = \u0002.NameExpression as _IExpression;
			if (iexpression != null)
			{
				iexpression.Accept(this);
			}
			_IExpression iexpression2 = \u0002.BaseExpression as _IExpression;
			if (iexpression2 != null)
			{
				iexpression2.Accept(this);
			}
			IExpression[] interfaceExpressions = \u0002.InterfaceExpressions;
			for (int i = 0; i < interfaceExpressions.Length; i++)
			{
				((_IExpression)interfaceExpressions[i]).Accept(this);
			}
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
				{
					string attributeValue = ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID);
					if (attributeValue == null || new Guid(attributeValue) != this.\u0001.ObjectGuid)
					{
						continue;
					}
				}
				_IExpression iexpression3 = ivariable.Initial as _IExpression;
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
				if (ivariable.Type != null)
				{
					IType u = ((_IType)ivariable.Type).DeRefType;
					if (ivariable.Type.Class == TypeClass.Subrange)
					{
						u = (_IType)ivariable.Type;
					}
					this.\u0001(u);
				}
				if (ivariable.InputAssignments != null)
				{
					IAssignmentExpression[] inputAssignments = ivariable.InputAssignments;
					for (int i = 0; i < inputAssignments.Length; i++)
					{
						_IAssignmentExpression iassignmentExpression = inputAssignments[i] as _IAssignmentExpression;
						if (iassignmentExpression != null)
						{
							iassignmentExpression.Accept(this);
						}
					}
				}
				if (this.\u0001(ivariable.SourcePosition) && (!ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID) || !(new Guid(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID)) != this.\u0001.ObjectGuid)))
				{
					_IVariableExpression ivariableExpression = \u0003.\u0001(ivariable.OrgName);
					ivariableExpression.SignatureId = \u0002.PrecompileId;
					ivariableExpression.VariableId = ivariable.PrecompileId;
					ivariableExpression.PositionIntern = (ivariable.SourcePosition as _ISourcePosition).GetMinimalPosition();
					ivariableExpression.LengthIntern = ivariable.SourcePosition.Length;
					ivariableExpression.Accept(this);
				}
			}
		}

		// Token: 0x060012C0 RID: 4800 RVA: 0x00033D10 File Offset: 0x00031F10
		private void \u0001(IType \u0002)
		{
			this.\u0001(AccessFlag.Type);
			if (\u0002.Class == TypeClass.Userdef)
			{
				this.\u0001(\u0002 as IUserdefType);
			}
			else if (\u0002.Class == TypeClass.Array)
			{
				this.\u0001(\u0002 as IArrayType);
			}
			else if (\u0002.Class == TypeClass.String)
			{
				_IStringType istringType = \u0002 as _IStringType;
				if (istringType.Length != null)
				{
					istringType.Length.Accept(this);
				}
			}
			else if (\u0002.Class == TypeClass.WString)
			{
				_IWStringType iwstringType = \u0002 as _IWStringType;
				if (iwstringType.Length != null)
				{
					iwstringType.Length.Accept(this);
				}
			}
			else if (\u0002.Class == TypeClass.Subrange)
			{
				_ISubrangeType isubrangeType = \u0002 as _ISubrangeType;
				isubrangeType._LowerBorder.Accept(this);
				isubrangeType._UpperBorder.Accept(this);
			}
			else if (\u0002.Class == TypeClass.Pointer)
			{
				_IPointerType ipointerType = \u0002 as _IPointerType;
				this.\u0001(ipointerType.BaseType);
			}
			else if (\u0002.Class == TypeClass.Reference)
			{
				_IReferenceType ireferenceType = \u0002 as _IReferenceType;
				this.\u0001(ireferenceType.BaseType);
			}
			this.\u0002();
		}

		// Token: 0x060012C1 RID: 4801 RVA: 0x00033E20 File Offset: 0x00032020
		private void \u0001(IArrayType \u0002)
		{
			_IArrayType iarrayType = \u0002 as _IArrayType;
			this.\u0001(iarrayType._Base);
			foreach (_IArrayDimension iarrayDimension in iarrayType._Dimensions)
			{
				iarrayDimension._LowerBorder.Accept(this);
				iarrayDimension._UpperBorder.Accept(this);
			}
		}

		// Token: 0x060012C2 RID: 4802 RVA: 0x00033E90 File Offset: 0x00032090
		private void \u0001(IUserdefType \u0002)
		{
			IGenericUserdefType genericUserdefType = \u0002 as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				foreach (_IExpression iexpression in genericUserdefType.GenericConstantsInitializations)
				{
					iexpression.Accept(this);
				}
			}
			((\u0002 as _IUserdefType).NameExpression as _IExpression).Accept(this);
		}

		// Token: 0x060012C3 RID: 4803 RVA: 0x00033EFC File Offset: 0x000320FC
		public void \u0001(_ICompiledPOU \u0002)
		{
			\u0002.GetParseTree().Accept(this);
		}

		// Token: 0x060012C4 RID: 4804 RVA: 0x00033F0C File Offset: 0x0003210C
		private bool \u0001(ISourcePosition \u0002)
		{
			return \u0002 != null && \u0002.Position == this.\u0001.Position && \u0002.PositionOffset <= this.\u0001.PositionOffset && \u0002.PositionOffset + \u0002.Length >= this.\u0001.PositionOffset + this.\u0001.Length;
		}

		// Token: 0x060012C5 RID: 4805 RVA: 0x00033F74 File Offset: 0x00032174
		public bool \u0002(ISourcePosition \u0002)
		{
			return \u0002 != null && \u0002.Position == this.\u0001.Position && \u0002.PositionOffset == this.\u0001.PositionOffset && \u0002.Length == this.\u0001.Length;
		}

		// Token: 0x060012C6 RID: 4806 RVA: 0x00033FC4 File Offset: 0x000321C4
		public void \u0001(_IWhileStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060012C7 RID: 4807 RVA: 0x00033FE0 File Offset: 0x000321E0
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			\u0002._Condition.Accept(this);
		}

		// Token: 0x060012C8 RID: 4808 RVA: 0x00033FFC File Offset: 0x000321FC
		public void \u0001(_IForStatement \u0002)
		{
			if (\u0002.CounterStart != null)
			{
				\u0002._CounterStart.Accept(this);
			}
			\u0002._UpperBound.Accept(this);
			if (\u0002.By != null)
			{
				\u0002._By.Accept(this);
			}
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x060012C9 RID: 4809 RVA: 0x0003404C File Offset: 0x0003224C
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x060012CA RID: 4810 RVA: 0x00034050 File Offset: 0x00032250
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x060012CB RID: 4811 RVA: 0x00034054 File Offset: 0x00032254
		private bool \u0001(_IStatement \u0002)
		{
			return this.\u0001 == null && (this.\u0001 == WhatToFind.Statement || this.\u0001 == WhatToFind.EnclosingExprement) && this.\u0001(\u0002.Position);
		}

		// Token: 0x060012CC RID: 4812 RVA: 0x00034084 File Offset: 0x00032284
		private bool \u0001()
		{
			return (this.\u0001 == WhatToFind.CallExpression || this.\u0001 == WhatToFind.EnclosingCall) && this.\u0001 != null && !(this.\u0001 is _ICallExpression);
		}

		// Token: 0x060012CD RID: 4813 RVA: 0x000340B4 File Offset: 0x000322B4
		public void \u0001(_ISequenceStatement \u0002)
		{
			foreach (_IStatement istatement in \u0002._StatementList)
			{
				bool flag = this.\u0001 == null;
				istatement.Accept(this);
				bool flag2 = this.\u0001 != null && flag;
				if (this.\u0001 == WhatToFind.Statement && flag2 && !(this.\u0001 is IStatement))
				{
					this.\u0001 = istatement;
					break;
				}
				bool flag3 = this.\u0001 != null;
				if (this.\u0001(istatement))
				{
					this.\u0001 = istatement;
					break;
				}
				if (this.\u0001())
				{
					this.\u0001 = null;
				}
				if (flag3)
				{
					break;
				}
			}
		}

		// Token: 0x060012CE RID: 4814 RVA: 0x00034170 File Offset: 0x00032370
		public void \u0001(_IAssignmentExpression \u0002)
		{
			bool flag = this.\u0001 == null;
			\u0002._RValue.Accept(this);
			\u0002._LValue.Accept(this);
			if (this.\u0001 == WhatToFind.EnclosingExprement && this.\u0001 != null && !(this.\u0001 is _IAssignmentExpression) && !(this.\u0001 is _ICallExpression) && flag)
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012CF RID: 4815 RVA: 0x000341DC File Offset: 0x000323DC
		public void \u0001(_IIfStatement \u0002)
		{
			\u0002._Condition.Accept(this);
			\u0002._IfThen.Accept(this);
			if (\u0002._ElseIf != null)
			{
				foreach (_IElseIf ielseIf in \u0002._ElseIf)
				{
					ielseIf._Condition.Accept(this);
					ielseIf._Controlled.Accept(this);
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002._IfElse.Accept(this);
			}
		}

		// Token: 0x060012D0 RID: 4816 RVA: 0x0003426C File Offset: 0x0003246C
		public void \u0001(_IReturnStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060012D1 RID: 4817 RVA: 0x00034284 File Offset: 0x00032484
		public void \u0001(_IJumpStatement \u0002)
		{
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
		}

		// Token: 0x060012D2 RID: 4818 RVA: 0x0003429C File Offset: 0x0003249C
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x060012D3 RID: 4819 RVA: 0x000342A0 File Offset: 0x000324A0
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x060012D4 RID: 4820 RVA: 0x000342A4 File Offset: 0x000324A4
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x060012D5 RID: 4821 RVA: 0x000342A8 File Offset: 0x000324A8
		public void \u0001(_IExpressionStatement \u0002)
		{
			\u0002._Expr.Accept(this);
		}

		// Token: 0x060012D6 RID: 4822 RVA: 0x000342B8 File Offset: 0x000324B8
		public void \u0001(_ICallExpression \u0002)
		{
			bool flag = this.\u0001 == null;
			if (\u0002._Condition != null)
			{
				\u0002._Condition.Accept(this);
			}
			\u0002._Callee.Accept(this);
			if (this.\u0001 == WhatToFind.CallExpression && \u0002._Callee != null && this.\u0001(\u0002._Callee.Position))
			{
				this.\u0001 = \u0002;
				return;
			}
			foreach (_IExpression iexpression in \u0002.ParamExpressions)
			{
				iexpression.Accept(this);
			}
			bool flag2 = this.\u0001 != WhatToFind.EnclosingCall && this.\u0001 == null;
			foreach (_IExpression iexpression2 in \u0002.Inputs)
			{
				if (iexpression2 != null)
				{
					iexpression2.Accept(this);
				}
			}
			foreach (_IExpression iexpression3 in \u0002.Outputs)
			{
				if (iexpression3 != null)
				{
					iexpression3.Accept(this);
				}
			}
			if (this.\u0001 != null && flag2)
			{
				_IExpression iexpression4 = this.\u0001 as _IExpression;
				if (iexpression4 != null)
				{
					_IExpression iexpression5 = \u0002._Callee.Duplicate() as _IExpression;
					if (iexpression5 != null)
					{
						iexpression5._Position = iexpression4._Position;
						iexpression5.PositionLength = iexpression4.PositionLength;
						_ICompoAccessExpression icompoAccessExpression = \u0003.\u0001(iexpression5);
						icompoAccessExpression._Right = iexpression4;
						icompoAccessExpression._Position = iexpression4._Position;
						icompoAccessExpression.PositionLength = iexpression4.PositionLength;
						this.\u0001 = icompoAccessExpression;
					}
				}
			}
			if (this.\u0001 == null)
			{
				IList<_IExpression> outputExpressions = \u0002.OutputExpressions;
				for (int i = 0; i < outputExpressions.Count; i++)
				{
					if (outputExpressions[i] != null)
					{
						outputExpressions[i].Accept(this);
					}
				}
			}
			bool flag3 = this.\u0001 is _ICallExpression && (this.\u0001 == WhatToFind.EnclosingCall || this.\u0001 == WhatToFind.EnclosingExprement);
			bool flag4 = this.\u0001 is _IAssignmentExpression && this.\u0001 == WhatToFind.EnclosingExprement;
			if (flag3 || flag4)
			{
				return;
			}
			if ((this.\u0001 == WhatToFind.EnclosingExprement || this.\u0001 == WhatToFind.EnclosingCall) && this.\u0001 != null && flag)
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012D7 RID: 4823 RVA: 0x00034528 File Offset: 0x00032728
		public void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 0; i < operandsList.Count; i++)
			{
				operandsList[i].Accept(this);
			}
		}

		// Token: 0x060012D8 RID: 4824 RVA: 0x0003455C File Offset: 0x0003275C
		public void \u0001(_ICastExpression \u0002)
		{
		}

		// Token: 0x060012D9 RID: 4825 RVA: 0x00034560 File Offset: 0x00032760
		public void \u0001(_INewExpression \u0002)
		{
			\u0002._Count.Accept(this);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					((_IAssignmentExpression)assignmentExpression).Accept(this);
				}
			}
			this.\u0001(AccessFlag.Type);
			_IUserdefType iuserdefType = \u0002.TypeToCreate as _IUserdefType;
			if (iuserdefType != null)
			{
				((_IExpression)iuserdefType.NameExpression).Accept(this);
			}
			this.\u0002();
		}

		// Token: 0x060012DA RID: 4826 RVA: 0x000345F8 File Offset: 0x000327F8
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x060012DB RID: 4827 RVA: 0x000345FC File Offset: 0x000327FC
		public void \u0001(_IConversionExpression \u0002)
		{
			\u0002._Exp.Accept(this);
		}

		// Token: 0x060012DC RID: 4828 RVA: 0x0003460C File Offset: 0x0003280C
		public void \u0001(_IThisExpression \u0002)
		{
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012DD RID: 4829 RVA: 0x00034624 File Offset: 0x00032824
		public void \u0001(_IBaseExpression \u0002)
		{
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012DE RID: 4830 RVA: 0x0003463C File Offset: 0x0003283C
		public void \u0001(_ILiteralExpression \u0002)
		{
		}

		// Token: 0x060012DF RID: 4831 RVA: 0x00034640 File Offset: 0x00032840
		public void \u0001(_IAddressExpression \u0002)
		{
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012E0 RID: 4832 RVA: 0x00034658 File Offset: 0x00032858
		public void \u0001(_IVariableExpression \u0002)
		{
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
				if (this.TopOfStack == AccessFlag.Type)
				{
					this.\u0002 = \u0002;
				}
			}
		}

		// Token: 0x060012E1 RID: 4833 RVA: 0x00034684 File Offset: 0x00032884
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			\u0002._Var.Accept(this);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				\u0002.GetAccess(i).Accept(this);
			}
			if (this.\u0001 != null || this.\u0001 != WhatToFind.PartialInstancePath)
			{
				return;
			}
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012E2 RID: 4834 RVA: 0x000346E4 File Offset: 0x000328E4
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			if (this.\u0001 != null)
			{
				return;
			}
			\u0002._Left.Accept(this);
			if (this.\u0001 == null)
			{
				\u0002._Right.Accept(this);
				if (this.\u0001 != null)
				{
					this.\u0001 = \u0002;
				}
			}
			if (this.\u0001 != null && this.\u0001 == WhatToFind.WholeInstancePath)
			{
				this.\u0001 = \u0002;
			}
			if (this.\u0001 == null && this.\u0001 == WhatToFind.ExactInstancePath && this.\u0002(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012E3 RID: 4835 RVA: 0x00034768 File Offset: 0x00032968
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			\u0002._Base.Accept(this);
		}

		// Token: 0x060012E4 RID: 4836 RVA: 0x00034778 File Offset: 0x00032978
		public void \u0001(_ICopyScopeExpression \u0002)
		{
		}

		// Token: 0x060012E5 RID: 4837 RVA: 0x0003477C File Offset: 0x0003297C
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			if (this.\u0001 == null)
			{
				\u0002._Base.Accept(this);
				if (this.\u0001 != null)
				{
					this.\u0001 = \u0002;
				}
			}
		}

		// Token: 0x060012E6 RID: 4838 RVA: 0x000347A4 File Offset: 0x000329A4
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			if (this.\u0001 == null)
			{
				\u0002._Base.Accept(this);
				if (this.\u0001 != null)
				{
					this.\u0001 = \u0002;
				}
			}
		}

		// Token: 0x060012E7 RID: 4839 RVA: 0x000347CC File Offset: 0x000329CC
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			if (this.\u0001 == null)
			{
				\u0002._Base.Accept(this);
				if (this.\u0001 != null)
				{
					this.\u0001 = \u0002;
				}
			}
		}

		// Token: 0x060012E8 RID: 4840 RVA: 0x000347F4 File Offset: 0x000329F4
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			if (this.\u0001 == null)
			{
				\u0002._Namespace.Accept(this);
				if (this.\u0001 != null)
				{
					\u0002._Access.Accept(this);
				}
				if (this.\u0001 != null && this.\u0001 == WhatToFind.WholeInstancePath)
				{
					this.\u0001 = \u0002;
				}
			}
		}

		// Token: 0x060012E9 RID: 4841 RVA: 0x00034840 File Offset: 0x00032A40
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x060012EA RID: 4842 RVA: 0x00034844 File Offset: 0x00032A44
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			\u0002._Low.Accept(this);
			\u0002._High.Accept(this);
		}

		// Token: 0x060012EB RID: 4843 RVA: 0x00034860 File Offset: 0x00032A60
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression iexpression in \u0002._cases)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060012EC RID: 4844 RVA: 0x000348AC File Offset: 0x00032AAC
		public void \u0001(_ICaseStatement \u0002)
		{
			\u0002._Switch.Accept(this);
			foreach (_ICase icase in \u0002._Cases)
			{
				icase._Label.Accept(this);
				icase._Controlled.Accept(this);
			}
			if (\u0002.Else != null)
			{
				\u0002._Else.Accept(this);
			}
		}

		// Token: 0x060012ED RID: 4845 RVA: 0x00034928 File Offset: 0x00032B28
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x060012EE RID: 4846 RVA: 0x0003492C File Offset: 0x00032B2C
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x060012EF RID: 4847 RVA: 0x00034930 File Offset: 0x00032B30
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x060012F0 RID: 4848 RVA: 0x00034934 File Offset: 0x00032B34
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x060012F1 RID: 4849 RVA: 0x00034938 File Offset: 0x00032B38
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
			if (this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x060012F2 RID: 4850 RVA: 0x00034950 File Offset: 0x00032B50
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x060012F3 RID: 4851 RVA: 0x00034954 File Offset: 0x00032B54
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060012F4 RID: 4852 RVA: 0x00034958 File Offset: 0x00032B58
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x060012F5 RID: 4853 RVA: 0x0003495C File Offset: 0x00032B5C
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x060012F6 RID: 4854 RVA: 0x00034960 File Offset: 0x00032B60
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x060012F7 RID: 4855 RVA: 0x00034964 File Offset: 0x00032B64
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x060012F8 RID: 4856 RVA: 0x00034968 File Offset: 0x00032B68
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
		}

		// Token: 0x060012F9 RID: 4857 RVA: 0x0003496C File Offset: 0x00032B6C
		public void \u0001(_IArrayInitialization \u0002)
		{
			foreach (_IExpression iexpression in \u0002._InitValues)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x060012FA RID: 4858 RVA: 0x000349B8 File Offset: 0x00032BB8
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
			{
				iassignmentExpression.Accept(this);
			}
		}

		// Token: 0x060012FB RID: 4859 RVA: 0x00034A04 File Offset: 0x00032C04
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x060012FC RID: 4860 RVA: 0x00034A08 File Offset: 0x00032C08
		public void \u0001(_IVariableReference \u0002)
		{
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x060012FD RID: 4861 RVA: 0x00034A1C File Offset: 0x00032C1C
		public void \u0001(_ITypeReference \u0002)
		{
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x060012FE RID: 4862 RVA: 0x00034A30 File Offset: 0x00032C30
		public void \u0001(_IPouReference \u0002)
		{
			_IExpression instancePath = \u0002.InstancePath;
			if (instancePath == null)
			{
				return;
			}
			instancePath.Accept(this);
		}

		// Token: 0x060012FF RID: 4863 RVA: 0x00034A44 File Offset: 0x00032C44
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001300 RID: 4864 RVA: 0x00034A48 File Offset: 0x00032C48
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001301 RID: 4865 RVA: 0x00034A4C File Offset: 0x00032C4C
		public void \u0001(_IDefinedExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x06001302 RID: 4866 RVA: 0x00034A5C File Offset: 0x00032C5C
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002.Operands)
			{
				iexpression.Accept(this);
			}
		}

		// Token: 0x06001303 RID: 4867 RVA: 0x00034AA8 File Offset: 0x00032CA8
		public void \u0001(_IPragmaAssertion \u0002)
		{
			\u0002.Condition.Accept(this);
		}

		// Token: 0x06001304 RID: 4868 RVA: 0x00034AB8 File Offset: 0x00032CB8
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001305 RID: 4869 RVA: 0x00034ABC File Offset: 0x00032CBC
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			\u0002.Condition.Accept(this);
			\u0002.IfThen.Accept(this);
			if (\u0002.ElseIf != null)
			{
				foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
				{
					ipragmaElseIf.Condition.Accept(this);
					ipragmaElseIf.Controlled.Accept(this);
				}
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06001306 RID: 4870 RVA: 0x00034B4C File Offset: 0x00032D4C
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001307 RID: 4871 RVA: 0x00034B50 File Offset: 0x00032D50
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001308 RID: 4872 RVA: 0x00034B54 File Offset: 0x00032D54
		public void \u0001(_IXRefExpression \u0002)
		{
			\u0002.XRef.Accept(this);
			if (\u0002.XRefFrom != null)
			{
				\u0002.XRefFrom.Accept(this);
			}
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x00034B78 File Offset: 0x00032D78
		public void \u0001(_IHasTypeExpression \u0002)
		{
			\u0002.Variable.Accept(this);
		}

		// Token: 0x0600130A RID: 4874 RVA: 0x00034B88 File Offset: 0x00032D88
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x0600130B RID: 4875 RVA: 0x00034B8C File Offset: 0x00032D8C
		public void \u0001(_IHasAttributeExpression \u0002)
		{
			\u0002.ItemReference.Accept(this);
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x00034B9C File Offset: 0x00032D9C
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x00034BA0 File Offset: 0x00032DA0
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x0600130E RID: 4878 RVA: 0x00034BA4 File Offset: 0x00032DA4
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
			\u0002._Constant.Accept(this);
		}

		// Token: 0x0600130F RID: 4879 RVA: 0x00034BB4 File Offset: 0x00032DB4
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001310 RID: 4880 RVA: 0x00034BB8 File Offset: 0x00032DB8
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001311 RID: 4881 RVA: 0x00034BBC File Offset: 0x00032DBC
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			\u0002._Left.Accept(this);
			if (this.\u0001 == null && this.\u0001(\u0002.Position))
			{
				this.\u0001 = \u0002;
			}
		}

		// Token: 0x06001312 RID: 4882 RVA: 0x00034BE8 File Offset: 0x00032DE8
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			_IDefineReference defineReference = \u0002.DefineReference;
			if (defineReference == null)
			{
				return;
			}
			defineReference.Accept(this);
		}

		// Token: 0x04000324 RID: 804
		private WhatToFind \u0001;

		// Token: 0x04000325 RID: 805
		private _IExprement \u0001;

		// Token: 0x04000326 RID: 806
		private _IExprement \u0002;

		// Token: 0x04000327 RID: 807
		private ISourcePosition \u0001;

		// Token: 0x04000328 RID: 808
		private LStack<AccessFlag> \u0001 = new LStack<AccessFlag>();
	}
}
