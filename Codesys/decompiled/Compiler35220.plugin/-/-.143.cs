using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0002;
using \u000F;
using \u0011;
using \u0016;
using \u0019;
using \u001D;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Features;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0017
{
	// Token: 0x02000193 RID: 403
	internal sealed class \u000E : IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor, IExprementVisitor2, IOperatorExpressionVisitor6, IOperatorExpressionVisitor5, IOperatorExpressionVisitor4, IOperatorExpressionVisitor3, IOperatorExpressionVisitor2, IOperatorExpressionVisitor
	{
		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x06001C69 RID: 7273 RVA: 0x0005C6E0 File Offset: 0x0005A8E0
		// (set) Token: 0x06001C6A RID: 7274 RVA: 0x0005C6E8 File Offset: 0x0005A8E8
		internal _IStatement TypifiedRedParseTree { get; private set; }

		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x06001C6B RID: 7275 RVA: 0x0005C6F4 File Offset: 0x0005A8F4
		// (set) Token: 0x06001C6C RID: 7276 RVA: 0x0005C6FC File Offset: 0x0005A8FC
		private global::\u000F.\u0007 Scope { get; set; }

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x06001C6D RID: 7277 RVA: 0x0005C708 File Offset: 0x0005A908
		// (set) Token: 0x06001C6E RID: 7278 RVA: 0x0005C710 File Offset: 0x0005A910
		private _IPreCompileContext PreComLocal { get; set; }

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x06001C6F RID: 7279 RVA: 0x0005C71C File Offset: 0x0005A91C
		// (set) Token: 0x06001C70 RID: 7280 RVA: 0x0005C724 File Offset: 0x0005A924
		private _ISignature Signature { get; set; }

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x06001C71 RID: 7281 RVA: 0x0005C730 File Offset: 0x0005A930
		// (set) Token: 0x06001C72 RID: 7282 RVA: 0x0005C738 File Offset: 0x0005A938
		private Stack<global::\u0017.\u000E.\u0001> GenericTypeStack { get; set; }

		// Token: 0x06001C73 RID: 7283 RVA: 0x0005C744 File Offset: 0x0005A944
		public \u000E(_IPreCompileContext \u0097\u0002, global::\u000F.\u0007 \u009B\u0002)
		{
			this.PreComLocal = \u0097\u0002;
			this.Scope = \u009B\u0002;
			this.GenericTypeStack = new Stack<global::\u0017.\u000E.\u0001>();
		}

		// Token: 0x06001C74 RID: 7284 RVA: 0x0005C768 File Offset: 0x0005A968
		public static void \u0001(_IExpression \u0002, _IPreCompileContext \u0003)
		{
			global::\u000F.\u0007 u009B_u = new global::\u000F.\u0007(\u0003);
			new global::\u0017.\u000E(\u0003, u009B_u).\u0001(\u0002);
		}

		// Token: 0x06001C75 RID: 7285 RVA: 0x0005C78C File Offset: 0x0005A98C
		public static _IExpression \u0001(_IExpression \u0002, _IPreCompileContext \u0003)
		{
			_IExpression iexpression = (_IExpression)\u0002.Duplicate();
			global::\u0017.\u000E.\u0001(iexpression, \u0003);
			return iexpression;
		}

		// Token: 0x06001C76 RID: 7286 RVA: 0x0005C7A0 File Offset: 0x0005A9A0
		public static void \u0001(_IExpression \u0002, _IPrecompileScope \u0003)
		{
			_IPreCompileContext u = (_IPreCompileContext)APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(\u0003.ApplicationGuid);
			global::\u0017.\u000E.\u0001(\u0002, u);
		}

		// Token: 0x06001C77 RID: 7287 RVA: 0x0005C7D0 File Offset: 0x0005A9D0
		public void \u0001(IStatement \u0002, ISignature \u0003)
		{
			this.Signature = (\u0003 as _ISignature);
			_IStatement istatement = \u0002 as _IStatement;
			if (istatement == null)
			{
				return;
			}
			istatement.Accept(this);
		}

		// Token: 0x06001C78 RID: 7288 RVA: 0x0005C7F0 File Offset: 0x0005A9F0
		public void \u0001(_ICompiledPOU \u0002)
		{
			this.Signature = this.PreComLocal[\u0002.ObjectGuid];
			_IStatement parseTree = \u0002.GetParseTree();
			this.TypifiedRedParseTree = (((parseTree != null) ? parseTree.Duplicate() : null) as _IStatement);
			_IStatement istatement = this.TypifiedRedParseTree;
			if (istatement == null)
			{
				return;
			}
			istatement.Accept(this);
		}

		// Token: 0x06001C79 RID: 7289 RVA: 0x0005C844 File Offset: 0x0005AA44
		public void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = 0; i < statementList.Count; i++)
			{
				_IStatement istatement = statementList[i];
				try
				{
					istatement.Accept(this);
				}
				catch (Exception ex)
				{
					string text = "Internal error in _IStatement: " + istatement.ToString();
					istatement.AddError(text);
					text = "Exception text: " + ex.ToString();
					istatement.AddMessage(text, istatement._Position, Severity.Error, istatement.PositionLength, MessageId.None);
				}
			}
		}

		// Token: 0x06001C7A RID: 7290 RVA: 0x0005C8D0 File Offset: 0x0005AAD0
		private void \u0001(IGenericUserdefType \u0002)
		{
			this.GenericTypeStack.Push(new global::\u0017.\u000E.\u0001
			{
				UserdefTypeIn = \u0002,
				UserdefTypeOut = null
			});
		}

		// Token: 0x06001C7B RID: 7291 RVA: 0x0005C8F0 File Offset: 0x0005AAF0
		private IGenericUserdefType \u0001()
		{
			return this.GenericTypeStack.Pop().UserdefTypeOut;
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x0005C904 File Offset: 0x0005AB04
		private void \u0001(_IExpression \u0002)
		{
			this.\u0001(null);
			if (\u0002 != null)
			{
				\u0002.Accept(this);
			}
			this.\u0001();
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x0005C920 File Offset: 0x0005AB20
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x0005C93C File Offset: 0x0005AB3C
		public void \u0001(_IRepeatStatement \u0002)
		{
			\u0002._Controlled.Accept(this);
			this.\u0001(\u0002._Condition);
		}

		// Token: 0x06001C7F RID: 7295 RVA: 0x0005C958 File Offset: 0x0005AB58
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001(\u0002._CounterStart);
			this.\u0001(\u0002._Condition);
			this.\u0001(\u0002._UpperBound);
			this.\u0001(\u0002._Counter);
			this.\u0001(\u0002._By);
			\u0002._Controlled.Accept(this);
		}

		// Token: 0x06001C80 RID: 7296 RVA: 0x0005C9B0 File Offset: 0x0005ABB0
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06001C81 RID: 7297 RVA: 0x0005C9B4 File Offset: 0x0005ABB4
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06001C82 RID: 7298 RVA: 0x0005C9B8 File Offset: 0x0005ABB8
		public void \u0001(_IAssignmentExpression \u0002)
		{
			this.\u0001(\u0002._LValue);
			this.\u0001(\u0002._RValue);
			\u0002.Type = \u0002._LValue.Type;
			_IVariable ivariable = this.Scope.\u0001(\u0002._LValue);
			if (ivariable != null && ivariable.IsProperty)
			{
				\u0002.Info = new \u001D.\u0006();
			}
		}

		// Token: 0x06001C83 RID: 7299 RVA: 0x0005CA18 File Offset: 0x0005AC18
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
			\u0002._IfThen.Accept(this);
			foreach (_IElseIf ielseIf in \u0002._ElseIf)
			{
				this.\u0001(ielseIf._Condition);
				ielseIf._Controlled.Accept(this);
			}
			_IStatement ifElse = \u0002._IfElse;
			if (ifElse == null)
			{
				return;
			}
			ifElse.Accept(this);
		}

		// Token: 0x06001C84 RID: 7300 RVA: 0x0005CAA0 File Offset: 0x0005ACA0
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
		}

		// Token: 0x06001C85 RID: 7301 RVA: 0x0005CAB0 File Offset: 0x0005ACB0
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001(\u0002._Condition);
		}

		// Token: 0x06001C86 RID: 7302 RVA: 0x0005CAC0 File Offset: 0x0005ACC0
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06001C87 RID: 7303 RVA: 0x0005CAC4 File Offset: 0x0005ACC4
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06001C88 RID: 7304 RVA: 0x0005CAC8 File Offset: 0x0005ACC8
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06001C89 RID: 7305 RVA: 0x0005CACC File Offset: 0x0005ACCC
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(\u0002._Expr);
		}

		// Token: 0x06001C8A RID: 7306 RVA: 0x0005CADC File Offset: 0x0005ACDC
		[ExcludeFromCodeCoverage]
		public void \u0001(_IVariableDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001C8B RID: 7307 RVA: 0x0005CAE0 File Offset: 0x0005ACE0
		[ExcludeFromCodeCoverage]
		public void \u0001(_IVariableDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001C8C RID: 7308 RVA: 0x0005CAE4 File Offset: 0x0005ACE4
		[ExcludeFromCodeCoverage]
		public void \u0001(_IPOUDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001C8D RID: 7309 RVA: 0x0005CAE8 File Offset: 0x0005ACE8
		[ExcludeFromCodeCoverage]
		public void \u0001(_ITypeDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001C8E RID: 7310 RVA: 0x0005CAEC File Offset: 0x0005ACEC
		[ExcludeFromCodeCoverage]
		public void \u0001(_IEnumDeclarationStatement \u0002)
		{
		}

		// Token: 0x06001C8F RID: 7311 RVA: 0x0005CAF0 File Offset: 0x0005ACF0
		[ExcludeFromCodeCoverage]
		public void \u0001(_IEnumDeclarationListStatement \u0002)
		{
		}

		// Token: 0x06001C90 RID: 7312 RVA: 0x0005CAF4 File Offset: 0x0005ACF4
		public void \u0001(_ICallExpression \u0002)
		{
			this.\u0001(\u0002._Condition);
			this.\u0001(null);
			\u0002._Callee.Accept(this);
			IGenericUserdefType u = this.\u0001();
			\u0002.Type = null;
			_IUserdefType iuserdefType = null;
			if (\u0002._Callee.Type != null)
			{
				iuserdefType = (\u0002._Callee.Type.DeRefType as _IUserdefType);
			}
			_ISignature isignature = (iuserdefType != null) ? this.Scope.\u0001(iuserdefType) : null;
			if (iuserdefType != null && isignature != null)
			{
				this.\u0002(\u0002, isignature);
			}
			foreach (_IExpression u2 in \u0002.ParamExpressions)
			{
				this.\u0001(u2);
			}
			foreach (_IExpression u3 in \u0002.OutputExpressions)
			{
				this.\u0001(u3);
			}
			this.\u0001(\u0002, isignature);
			this.\u0001(\u0002, u, isignature);
		}

		// Token: 0x06001C91 RID: 7313 RVA: 0x0005CC08 File Offset: 0x0005AE08
		private void \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			if (\u0003 == null)
			{
				return;
			}
			if (\u0003.POUType != Operator.Function && \u0003.POUType != Operator.Method)
			{
				return;
			}
			IVariable[] outputs = \u0003.Outputs;
			if (outputs != null && outputs.Length != 0)
			{
				\u0002.Type = (outputs[0].Type as ICompiledType);
			}
		}

		// Token: 0x06001C92 RID: 7314 RVA: 0x0005CC50 File Offset: 0x0005AE50
		private void \u0001(_ICallExpression \u0002, IGenericUserdefType \u0003, _ISignature \u0004)
		{
			if (\u0004 == null)
			{
				return;
			}
			global::\u0017.\u000E.\u0001(\u0002, \u0004);
			foreach (_IExprement iexprement in \u0002.Inputs)
			{
				this.\u0001(\u0003);
				iexprement.Accept(this);
				this.\u0001();
			}
		}

		// Token: 0x06001C93 RID: 7315 RVA: 0x0005CCB4 File Offset: 0x0005AEB4
		private static void \u0001(_ICallExpression \u0002, _ISignature \u0003)
		{
			IList<_IExpression> paramExpressions = \u0002.ParamExpressions;
			IVariable[] allInputs = \u0003.AllInputs;
			IList<_IExpression> inputs = \u0002.Inputs;
			for (int i = 0; i < paramExpressions.Count; i++)
			{
				if (inputs[i] == null)
				{
					_IExpression iexpression = global::\u0019.\u0003.\u0001(allInputs[i].OrgName);
					iexpression.PrecompileVariableId = ((_IVariable)allInputs[i]).PrecompileId;
					iexpression.PrecompileSignatureId = \u0003.PrecompileId;
					\u0002.SetFormalParam(iexpression, i);
				}
			}
		}

		// Token: 0x06001C94 RID: 7316 RVA: 0x0005CD30 File Offset: 0x0005AF30
		private void \u0002(_ICallExpression \u0002, ISignature \u0003)
		{
			foreach (_IExpression u in \u0002.Outputs)
			{
				this.\u0001(u);
			}
			foreach (_IExpression u2 in \u0002.EmptyAssigns)
			{
				this.\u0001(u2);
			}
			IVariable[] outputs = \u0003.Outputs;
			if ((\u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method) && outputs.Length >= 1)
			{
				\u0002.Type = outputs[0].CompiledType;
			}
		}

		// Token: 0x06001C95 RID: 7317 RVA: 0x0005CDE8 File Offset: 0x0005AFE8
		private _IType \u0001(Operator \u0002, bool \u0003, int \u0004, IList<_IExpression> \u0005, ICompiledType \u0006)
		{
			return global::\u001D.\u0005.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, this.Scope, null);
		}

		// Token: 0x06001C96 RID: 7318 RVA: 0x0005CE00 File Offset: 0x0005B000
		public void \u0001(_IOperatorExpression \u0002)
		{
			IEnumerable<_IExpression> operandsList = \u0002._OperandsList;
			bool flag = false;
			foreach (_IExpression iexpression in operandsList)
			{
				this.\u0001(iexpression);
				if (((iexpression != null) ? iexpression.Type : null) == null)
				{
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			\u0002.AcceptOperatorVisitor(this);
		}

		// Token: 0x06001C97 RID: 7319 RVA: 0x0005CE6C File Offset: 0x0005B06C
		public void \u0001(_ICastExpression \u0002)
		{
			this.\u0001(\u0002.BaseExpression);
			if (\u0002.ExpWithType != null)
			{
				this.\u0001(\u0002.ExpWithType);
				\u0002.Type = \u0002.ExpWithType.Type;
			}
			if (\u0002.ExplicitelySpecifiedType != null)
			{
				\u0002.Type = \u0002.ExplicitelySpecifiedType;
			}
			\u0002.BaseExpression.Type = \u0002.Type;
		}

		// Token: 0x06001C98 RID: 7320 RVA: 0x0005CED0 File Offset: 0x0005B0D0
		public void \u0001(_INewExpression \u0002)
		{
			this.\u0001(\u0002._Count);
			if (\u0002._FBInitParams != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0002._FBInitParams)
				{
					_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)assignmentExpression;
					this.\u0001(iassignmentExpression.LValue as _IExpression);
					this.\u0001(iassignmentExpression.RValue as _IExpression);
				}
			}
		}

		// Token: 0x06001C99 RID: 7321 RVA: 0x0005CF54 File Offset: 0x0005B154
		public void \u0001(_ITypeExpression \u0002)
		{
		}

		// Token: 0x06001C9A RID: 7322 RVA: 0x0005CF58 File Offset: 0x0005B158
		public void \u0001(_IConversionExpression \u0002)
		{
			switch (\u0002.From)
			{
			case TypeClass.UXInt:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.From = TypeClass.ULInt;
				}
				else
				{
					\u0002.From = TypeClass.UDInt;
				}
				break;
			case TypeClass.XWord:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.From = TypeClass.LWord;
				}
				else
				{
					\u0002.From = TypeClass.DWord;
				}
				break;
			case TypeClass.XInt:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.From = TypeClass.LInt;
				}
				else
				{
					\u0002.From = TypeClass.DInt;
				}
				break;
			}
			switch (\u0002.To)
			{
			case TypeClass.UXInt:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.To = TypeClass.ULInt;
				}
				else
				{
					\u0002.To = TypeClass.UDInt;
				}
				break;
			case TypeClass.XWord:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.To = TypeClass.LWord;
					\u0002._CompiledType = TypeTable.XLWord;
				}
				else
				{
					\u0002.To = TypeClass.DWord;
					\u0002._CompiledType = TypeTable.XDWord;
				}
				break;
			case TypeClass.XInt:
				if (this.Scope.PointerSize == 8)
				{
					\u0002.To = TypeClass.LInt;
				}
				else
				{
					\u0002.To = TypeClass.DInt;
				}
				break;
			}
			this.\u0001(\u0002._Exp);
			if (\u0002.From == TypeClass.Any && \u0002._Exp.Type != null)
			{
				if (\u0002._Exp.Type.DeRefType.Class == TypeClass.BitConst)
				{
					\u0002._Exp.Type = TypeTable.USInt;
				}
				if (\u0002._Exp.Type.DeRefType.Class != TypeClass.Userdef && \u0002._Exp.Type.DeRefType.Class != TypeClass.Pointer && \u0002._Exp.Type.DeRefType.Class != TypeClass.Array)
				{
					\u0002.From = \u0002._Exp.Type.DeRefType.Class;
				}
				else if (\u0002._Exp.Type.DeRefType.Class == TypeClass.Pointer)
				{
					\u0002.From = TypeClass.DWord;
					if (this.Scope.PointerSize == 8)
					{
						\u0002.From = TypeClass.LWord;
					}
				}
			}
			if (\u0002.From == TypeClass.Reference && \u0002.To == TypeClass.Pointer)
			{
				\u0002.Type = global::\u0019.\u0003.\u0001(\u0002._Exp.Type as _IType);
			}
			else if (!TypeTable.IsResolvedXType(\u0002.Type))
			{
				\u0002.Type = TypeTable.Get(\u0002.To);
			}
			if (\u0002.From == TypeClass.AnyNum && \u0002._Exp.Type != null && TypeTable.IsNumber(\u0002._Exp.Type.DeRefType.Class))
			{
				\u0002.From = \u0002._Exp.Type.DeRefType.Class;
			}
		}

		// Token: 0x06001C9B RID: 7323 RVA: 0x0005D208 File Offset: 0x0005B408
		public void \u0001(_IThisExpression \u0002)
		{
			_ISignature isignature = this.Signature;
			if (isignature != null && isignature.PrecompileParentId >= 0)
			{
				isignature = this.Scope.\u0001(isignature.PrecompileParentId);
			}
			if (isignature != null && (isignature.POUType == Operator.FunctionBlock || isignature.GetFlag(SignatureFlag.Structure)))
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature.Name);
				iuserdefType.SignatureId = isignature.PrecompileId;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				\u0002.Type = type;
			}
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x0005D278 File Offset: 0x0005B478
		public void \u0001(_IBaseExpression \u0002)
		{
			_ISignature isignature = this.Signature;
			if (isignature != null)
			{
				_ISignature isignature2 = this.Scope.\u0001(isignature.PrecompileBaseSignatureId);
				if (isignature2 != null && (isignature2.POUType == Operator.FunctionBlock || isignature2.GetFlag(SignatureFlag.Structure)))
				{
					_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature2.Name);
					iuserdefType.SignatureId = isignature2.PrecompileId;
					\u0002.Type = global::\u0019.\u0003.\u0001(iuserdefType);
				}
			}
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x0005D2DC File Offset: 0x0005B4DC
		public void \u0001(_ILiteralExpression \u0002)
		{
			\u0002.Type = Helper.\u0001(\u0002, this.PreComLocal.TypeIsSupported(TypeClass.LReal), this.Scope.TreatLRealAsReal, this.PreComLocal.TypeIsSupported(TypeClass.LInt), this.Scope.TreatInt64AsInt32, this.PreComLocal.IsDefined("NO_UNICODE_SUPPORT"), this.PreComLocal.HasByteSupport());
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x0005D340 File Offset: 0x0005B540
		public void \u0001(_IAddressExpression \u0002)
		{
			\u0002.Type = TypeTable.GetDirectVariableSizeType(\u0002.DirectAddress.Size);
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x0005D358 File Offset: 0x0005B558
		[ExcludeFromCodeCoverage]
		public void \u0001(_IQualifiedNameExpression \u0002)
		{
		}

		// Token: 0x06001CA0 RID: 7328 RVA: 0x0005D35C File Offset: 0x0005B55C
		public void \u0001(_IVariableExpression \u0002)
		{
			IVariable variable = this.Scope.\u0001(\u0002);
			\u0002.Type = (((variable != null) ? variable.Type : null) as ICompiledType);
			\u0002.SetFlag(VarExprFlag.PrecompileTypified, true);
			if (\u0002.Type == null && \u0002.PrecompileVariableId == -1 && \u0002.PrecompileSignatureId != -1)
			{
				_ISignature isignature = this.Scope.\u0001(\u0002.PrecompileSignatureId);
				if (isignature != null)
				{
					_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature.Name);
					iuserdefType.SignatureId = isignature.PrecompileId;
					\u0002.Type = iuserdefType;
				}
			}
			this.\u0001(\u0002, variable);
			this.\u0002(\u0002);
			this.\u0003(\u0002);
			this.\u0002(\u0002);
		}

		// Token: 0x06001CA1 RID: 7329 RVA: 0x0005D404 File Offset: 0x0005B604
		public void \u0001(_IIndexAccessExpression \u0002)
		{
			this.\u0001(\u0002._Var);
			for (int i = 0; i < \u0002.NumAccesses; i++)
			{
				this.\u0001(\u0002.GetAccess(i));
			}
			if (\u0002._Var.Type != null)
			{
				ICompiledType deRefType = \u0002._Var.Type.DeRefType;
				TypeClass @class = deRefType.Class;
				if (@class <= TypeClass.WString)
				{
					if (@class != TypeClass.String)
					{
						if (@class == TypeClass.WString)
						{
							\u0002.Type = TypeTable.Word;
						}
					}
					else
					{
						\u0002.Type = TypeTable.Byte;
					}
				}
				else if (@class == TypeClass.Pointer || @class == TypeClass.Array || @class == TypeClass.__Vector)
				{
					\u0002.Type = deRefType.BaseType;
				}
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06001CA2 RID: 7330 RVA: 0x0005D4B0 File Offset: 0x0005B6B0
		public void \u0001(_ICompoAccessExpression \u0002)
		{
			IGenericUserdefType genericUserdefType = null;
			this.\u0001(null);
			\u0002._Left.Accept(this);
			IGenericUserdefType u = this.\u0001();
			if (\u0002._Left.Type == null)
			{
				this.\u0001(\u0002._Right);
				\u0002.Type = \u0002._Right.Type;
			}
			else if (\u0002._Left.Type.DeRefType.IsInteger)
			{
				this.\u0001(\u0002._Right);
				\u0002.Type = TypeTable.Bit;
			}
			else if (\u0002._Left.Type.DeRefType.Class == TypeClass.Userdef)
			{
				this.\u0001(u);
				\u0002._Right.Accept(this);
				genericUserdefType = this.\u0001();
				if (\u0002._Right.Type != null)
				{
					\u0002.Type = \u0002._Right.Type;
				}
			}
			if (genericUserdefType != null)
			{
				this.GenericTypeStack.Peek().UserdefTypeOut = genericUserdefType;
				return;
			}
			this.GenericTypeStack.Peek().UserdefTypeOut = u;
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x0005D5B0 File Offset: 0x0005B7B0
		public void \u0001(_IDeRefAccessExpression \u0002)
		{
			this.\u0001(\u0002._Base);
			if (\u0002._Base.Type == null)
			{
				return;
			}
			_IPointerType ipointerType = \u0002._Base.Type.DeRefType as _IPointerType;
			if (ipointerType != null)
			{
				\u0002.Type = ipointerType._Base;
			}
			this.\u0002(\u0002);
		}

		// Token: 0x06001CA4 RID: 7332 RVA: 0x0005D604 File Offset: 0x0005B804
		public void \u0001(_ICopyScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x06001CA5 RID: 7333 RVA: 0x0005D624 File Offset: 0x0005B824
		public void \u0001(_IGlobalScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x06001CA6 RID: 7334 RVA: 0x0005D644 File Offset: 0x0005B844
		public void \u0001(_ISystemScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x06001CA7 RID: 7335 RVA: 0x0005D664 File Offset: 0x0005B864
		public void \u0001(_IPoolScopeExpression \u0002)
		{
			this.\u0001(\u0002._Base);
			\u0002.Type = \u0002._Base.Type;
		}

		// Token: 0x06001CA8 RID: 7336 RVA: 0x0005D684 File Offset: 0x0005B884
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
			this.\u0001(\u0002._Access);
			\u0002.Type = \u0002._Access.Type;
		}

		// Token: 0x06001CA9 RID: 7337 RVA: 0x0005D6A4 File Offset: 0x0005B8A4
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x0005D6A8 File Offset: 0x0005B8A8
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x0005D6AC File Offset: 0x0005B8AC
		public void \u0001(_ICaseRangeExpression \u0002)
		{
			this.\u0001(\u0002._Low);
			this.\u0001(\u0002._High);
			ICompiledType deRefType = \u0002._Low.Type.DeRefType;
			ICompiledType deRefType2 = \u0002._High.Type.DeRefType;
			int num = (this.Scope.\u0001(deRefType) > this.Scope.\u0001(deRefType2)) ? this.Scope.\u0001(deRefType) : this.Scope.\u0001(deRefType2);
			if (TypeTable.IsSigned(deRefType.Class) || TypeTable.IsSigned(deRefType2.Class))
			{
				switch (num)
				{
				case 1:
					\u0002.Type = TypeTable.Get(TypeClass.SInt);
					return;
				case 2:
					\u0002.Type = TypeTable.Get(TypeClass.Int);
					return;
				case 3:
					break;
				case 4:
					\u0002.Type = TypeTable.Get(TypeClass.DInt);
					return;
				default:
					if (num != 8)
					{
						return;
					}
					\u0002.Type = TypeTable.Get(TypeClass.LInt);
					return;
				}
			}
			else
			{
				switch (num)
				{
				case 1:
					\u0002.Type = TypeTable.Get(TypeClass.USInt);
					return;
				case 2:
					\u0002.Type = TypeTable.Get(TypeClass.UInt);
					return;
				case 3:
					break;
				case 4:
					\u0002.Type = TypeTable.Get(TypeClass.UDInt);
					return;
				default:
					if (num != 8)
					{
						return;
					}
					\u0002.Type = TypeTable.Get(TypeClass.ULInt);
					break;
				}
			}
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x0005D7EC File Offset: 0x0005B9EC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			foreach (_IExpression u in \u0002._cases)
			{
				this.\u0001(u);
			}
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x0005D83C File Offset: 0x0005BA3C
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(\u0002._Switch);
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

		// Token: 0x06001CAE RID: 7342 RVA: 0x0005D8B8 File Offset: 0x0005BAB8
		[ExcludeFromCodeCoverage]
		public void \u0001(_IErrorExpression \u0002)
		{
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x0005D8BC File Offset: 0x0005BABC
		[ExcludeFromCodeCoverage]
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x0005D8C0 File Offset: 0x0005BAC0
		[ExcludeFromCodeCoverage]
		public void \u0001(_INullExpression \u0002)
		{
		}

		// Token: 0x06001CB1 RID: 7345 RVA: 0x0005D8C4 File Offset: 0x0005BAC4
		[ExcludeFromCodeCoverage]
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x0005D8C8 File Offset: 0x0005BAC8
		[ExcludeFromCodeCoverage]
		public void \u0001(_IMultipleIndexInitialization \u0002)
		{
			this.\u0001(\u0002._Number);
			this.\u0001(\u0002._Value);
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x0005D8E4 File Offset: 0x0005BAE4
		[ExcludeFromCodeCoverage]
		public void \u0001(_IArrayInitialization \u0002)
		{
			IList<_IExpression> initValues = \u0002._InitValues;
			for (int i = 0; i < initValues.Count; i++)
			{
				this.\u0001(initValues[i]);
			}
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x0005D918 File Offset: 0x0005BB18
		[ExcludeFromCodeCoverage]
		public void \u0001(_IStructureInitialization \u0002)
		{
			foreach (_IAssignmentExpression u in \u0002._CompoInits)
			{
				this.\u0001(u);
			}
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x0005D968 File Offset: 0x0005BB68
		[ExcludeFromCodeCoverage]
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			this.\u0001(\u0002.DefineReference);
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x0005D978 File Offset: 0x0005BB78
		[ExcludeFromCodeCoverage]
		public void \u0001(_IDefineReference \u0002)
		{
		}

		// Token: 0x06001CB7 RID: 7351 RVA: 0x0005D97C File Offset: 0x0005BB7C
		[ExcludeFromCodeCoverage]
		public void \u0001(_IVariableReference \u0002)
		{
			this.\u0001(\u0002.InstancePath);
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x0005D98C File Offset: 0x0005BB8C
		[ExcludeFromCodeCoverage]
		public void \u0001(_ITypeReference \u0002)
		{
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x0005D990 File Offset: 0x0005BB90
		[ExcludeFromCodeCoverage]
		public void \u0001(_IPouReference \u0002)
		{
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x0005D994 File Offset: 0x0005BB94
		[ExcludeFromCodeCoverage]
		public void \u0001(_ITaskReference \u0002)
		{
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x0005D998 File Offset: 0x0005BB98
		[ExcludeFromCodeCoverage]
		public void \u0001(_IResourceReference \u0002)
		{
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x0005D99C File Offset: 0x0005BB9C
		[ExcludeFromCodeCoverage]
		public void \u0001(_IDefinedExpression \u0002)
		{
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x0005D9A0 File Offset: 0x0005BBA0
		[ExcludeFromCodeCoverage]
		public void \u0001(_IPragmaOperatorExpression \u0002)
		{
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x0005D9A4 File Offset: 0x0005BBA4
		[ExcludeFromCodeCoverage]
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x0005D9A8 File Offset: 0x0005BBA8
		public void \u0001(_ICompilerVersionExpression \u0002)
		{
		}

		// Token: 0x06001CC0 RID: 7360 RVA: 0x0005D9AC File Offset: 0x0005BBAC
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x0005D9B0 File Offset: 0x0005BBB0
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(\u0002.Condition);
			\u0002.IfThen.Accept(this);
			foreach (_IPragmaElseIf ipragmaElseIf in \u0002.ElseIf)
			{
				this.\u0001(ipragmaElseIf.Condition);
				ipragmaElseIf.Controlled.Accept(this);
			}
			if (\u0002.IfElse != null)
			{
				\u0002.IfElse.Accept(this);
			}
		}

		// Token: 0x06001CC2 RID: 7362 RVA: 0x0005DA3C File Offset: 0x0005BC3C
		[ExcludeFromCodeCoverage]
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06001CC3 RID: 7363 RVA: 0x0005DA40 File Offset: 0x0005BC40
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06001CC4 RID: 7364 RVA: 0x0005DA44 File Offset: 0x0005BC44
		[ExcludeFromCodeCoverage]
		public void \u0001(_IXRefExpression \u0002)
		{
		}

		// Token: 0x06001CC5 RID: 7365 RVA: 0x0005DA48 File Offset: 0x0005BC48
		public void \u0001(_IHasTypeExpression \u0002)
		{
			this.\u0001(\u0002.Variable);
		}

		// Token: 0x06001CC6 RID: 7366 RVA: 0x0005DA58 File Offset: 0x0005BC58
		public void \u0001(_IIsEnumTypeExpression \u0002)
		{
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x0005DA5C File Offset: 0x0005BC5C
		public void \u0001(_IHasAttributeExpression \u0002)
		{
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x0005DA60 File Offset: 0x0005BC60
		public void \u0001(_IHasValueExpression \u0002)
		{
		}

		// Token: 0x06001CC9 RID: 7369 RVA: 0x0005DA64 File Offset: 0x0005BC64
		public void \u0001(_IHasConstantValueExpression \u0002)
		{
		}

		// Token: 0x06001CCA RID: 7370 RVA: 0x0005DA68 File Offset: 0x0005BC68
		public void \u0001(_IHasConstantTypeExpression \u0002)
		{
		}

		// Token: 0x06001CCB RID: 7371 RVA: 0x0005DA6C File Offset: 0x0005BC6C
		public void \u0001(_ITryCatchStatement \u0002)
		{
			\u0002.DefaultTraverse(this);
		}

		// Token: 0x06001CCC RID: 7372 RVA: 0x0005DA78 File Offset: 0x0005BC78
		public void \u0001(_IPartialAccessExpression \u0002)
		{
			this.\u0001(\u0002._Left);
			\u0002._CompiledType = TypeTable.GetDirectVariableSizeType(\u0002.PartSize);
		}

		// Token: 0x06001CCD RID: 7373 RVA: 0x0005DA98 File Offset: 0x0005BC98
		public void \u0002(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x06001CCE RID: 7374 RVA: 0x0005DAA8 File Offset: 0x0005BCA8
		public void \u0003(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 0)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			\u0002.Type = operandsList[0].Type.DeRefType;
		}

		// Token: 0x06001CCF RID: 7375 RVA: 0x0005DAE8 File Offset: 0x0005BCE8
		public void \u0004(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count == 0)
			{
				\u0002.Type = TypeTable.DInt;
				return;
			}
			\u0002.Type = operandsList[0].Type.DeRefType;
		}

		// Token: 0x06001CD0 RID: 7376 RVA: 0x0005DB28 File Offset: 0x0005BD28
		public void \u0005(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06001CD1 RID: 7377 RVA: 0x0005DB38 File Offset: 0x0005BD38
		public void \u0006(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x06001CD2 RID: 7378 RVA: 0x0005DB48 File Offset: 0x0005BD48
		public void \u0007(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0005DB58 File Offset: 0x0005BD58
		[ExcludeFromCodeCoverage]
		public void \u0008(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CD4 RID: 7380 RVA: 0x0005DB68 File Offset: 0x0005BD68
		public void \u000E(_IOperatorExpression \u0002)
		{
			if (\u0002._OperandsList.Count == 3)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x0005DB90 File Offset: 0x0005BD90
		public void \u000F(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x0005DBA0 File Offset: 0x0005BDA0
		public void \u0010(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x0005DBB0 File Offset: 0x0005BDB0
		public void \u0011(_IOperatorExpression \u0002)
		{
			\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.DWord);
		}

		// Token: 0x06001CD8 RID: 7384 RVA: 0x0005DBC4 File Offset: 0x0005BDC4
		public void \u0012(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.ULInt;
		}

		// Token: 0x06001CD9 RID: 7385 RVA: 0x0005DBD4 File Offset: 0x0005BDD4
		public void \u0013(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.UDInt;
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x0005DBE4 File Offset: 0x0005BDE4
		public void \u0014(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06001CDB RID: 7387 RVA: 0x0005DBF4 File Offset: 0x0005BDF4
		public void \u0015(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DWord;
		}

		// Token: 0x06001CDC RID: 7388 RVA: 0x0005DC04 File Offset: 0x0005BE04
		public void \u0016(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CDD RID: 7389 RVA: 0x0005DC14 File Offset: 0x0005BE14
		public void \u0017(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			this.\u0001(operandsList[0]);
		}

		// Token: 0x06001CDE RID: 7390 RVA: 0x0005DC40 File Offset: 0x0005BE40
		public void \u0018(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			if (operandsList.Count > 1)
			{
				this.\u0001(operandsList[1]);
			}
		}

		// Token: 0x06001CDF RID: 7391 RVA: 0x0005DC78 File Offset: 0x0005BE78
		public void \u0019(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			\u0002.Type = TypeTable.Bool;
			if (operandsList.Count > 1)
			{
				this.\u0001(operandsList[1]);
			}
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x0005DCB0 File Offset: 0x0005BEB0
		public void \u001A(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x0005DCC0 File Offset: 0x0005BEC0
		public void \u001B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			_IExpression iexpression = operandsList[0];
			if (((iexpression != null) ? iexpression._CompiledType : null) is _IUserdefType)
			{
				ISignature signature = this.Scope.\u0001((operandsList[0]._CompiledType as _IUserdefType).SignatureId);
				if (signature != null && signature.Outputs.Length != 0)
				{
					\u0002.Type = (signature.Outputs[0].Type as ICompiledType);
				}
			}
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0005DD34 File Offset: 0x0005BF34
		public void \u001C(_IOperatorExpression \u0002)
		{
			foreach (_IExpression u in \u0002._OperandsList)
			{
				this.\u0001(u);
			}
			\u0002.Type = global::\u0019.\u0003.\u0001("__SYSTEM.TYPE_CLASS");
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x0005DD94 File Offset: 0x0005BF94
		public void \u001D(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CE4 RID: 7396 RVA: 0x0005DDA4 File Offset: 0x0005BFA4
		public void \u001E(_IOperatorExpression \u0002)
		{
			\u0002.Type = global::\u0019.\u0003.\u0001("__SYSTEM.VAR_INFO");
		}

		// Token: 0x06001CE5 RID: 7397 RVA: 0x0005DDB8 File Offset: 0x0005BFB8
		public void \u001F(_IOperatorExpression \u0002)
		{
			this.\u007F(\u0002);
		}

		// Token: 0x06001CE6 RID: 7398 RVA: 0x0005DDC4 File Offset: 0x0005BFC4
		public void \u007F(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count > 0)
			{
				if (\u0002.Code == Operator.__RefAdr)
				{
					_IExpression iexpression = operandsList[0];
					bool flag;
					if (iexpression == null)
					{
						flag = false;
					}
					else
					{
						ICompiledType type = iexpression.Type;
						TypeClass? typeClass = (type != null) ? new TypeClass?(type.Class) : null;
						TypeClass typeClass2 = TypeClass.Reference;
						flag = (typeClass.GetValueOrDefault() == typeClass2 & typeClass != null);
					}
					if (flag)
					{
						\u0002.Type = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(operandsList[0].Type as _IType));
						return;
					}
				}
				_IExpression iexpression2 = operandsList[0];
				\u0002.Type = global::\u0019.\u0003.\u0001(((iexpression2 != null) ? iexpression2.Type : null) as _IType);
				return;
			}
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06001CE7 RID: 7399 RVA: 0x0005DE88 File Offset: 0x0005C088
		public void \u0080(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count != 1)
			{
				return;
			}
			int num = -1;
			if (operandsList[0].Type != null)
			{
				num = this.PreComLocal.CalculateTypeSize(this.Signature, operandsList[0].Type);
			}
			if (num > 0)
			{
				if (num > 65535)
				{
					\u0002.Type = TypeTable.UDInt;
					return;
				}
				if (num > 255)
				{
					\u0002.Type = TypeTable.UInt;
					return;
				}
				\u0002.Type = TypeTable.USInt;
				return;
			}
			else
			{
				_IExpression iexpression = operandsList[0];
				ICompiledType compiledType = (iexpression != null) ? iexpression.Type : null;
				if (compiledType != null && TypeTable.IsConcreteType(compiledType.Class) && TypeTable.IsInteger(compiledType.Class))
				{
					\u0002.Type = compiledType;
					return;
				}
				if (compiledType != null && compiledType.Class == TypeClass.Pointer)
				{
					if (this.Scope.PointerSize == 2)
					{
						\u0002.Type = TypeTable.UInt;
						return;
					}
					\u0002.Type = TypeTable.UDInt;
					return;
				}
				else
				{
					if (compiledType != null && TypeTable.IsReal(compiledType.Class))
					{
						\u0002.Type = TypeTable.UDInt;
						return;
					}
					\u0002.Type = TypeTable.UInt;
					return;
				}
			}
		}

		// Token: 0x06001CE8 RID: 7400 RVA: 0x0005DFAC File Offset: 0x0005C1AC
		public void \u0081(_IOperatorExpression \u0002)
		{
			if (8 == this.PreComLocal.PointerSize)
			{
				\u0002.Type = TypeTable.ULInt;
				return;
			}
			\u0002.Type = TypeTable.UDInt;
		}

		// Token: 0x06001CE9 RID: 7401 RVA: 0x0005DFD4 File Offset: 0x0005C1D4
		public void \u0082(_IOperatorExpression \u0002)
		{
			_ISignature isignature = this.Signature;
			if (isignature != null && (isignature.POUType == Operator.FunctionBlock || isignature.GetFlag(SignatureFlag.Structure)))
			{
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature.Name);
				iuserdefType.SignatureId = isignature.PrecompileId;
				_IPointerType type = global::\u0019.\u0003.\u0001(iuserdefType);
				\u0002.Type = type;
			}
		}

		// Token: 0x06001CEA RID: 7402 RVA: 0x0005E024 File Offset: 0x0005C224
		[ExcludeFromCodeCoverage]
		public void \u0083(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x06001CEB RID: 7403 RVA: 0x0005E034 File Offset: 0x0005C234
		[ExcludeFromCodeCoverage]
		public void \u0084(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CEC RID: 7404 RVA: 0x0005E044 File Offset: 0x0005C244
		public void \u0086(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CED RID: 7405 RVA: 0x0005E054 File Offset: 0x0005C254
		public void \u0087(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06001CEE RID: 7406 RVA: 0x0005E064 File Offset: 0x0005C264
		[ExcludeFromCodeCoverage]
		public void \u0088(_IOperatorExpression \u0002)
		{
			\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Byte);
		}

		// Token: 0x06001CEF RID: 7407 RVA: 0x0005E078 File Offset: 0x0005C278
		public void \u0089(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Time;
		}

		// Token: 0x06001CF0 RID: 7408 RVA: 0x0005E088 File Offset: 0x0005C288
		public void \u008A(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.LTime;
		}

		// Token: 0x06001CF1 RID: 7409 RVA: 0x0005E098 File Offset: 0x0005C298
		public void \u008B(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count < 2)
			{
				\u0002.Type = TypeTable.Bool;
				return;
			}
			if (!(operandsList[0] is ILiteralExpression) || TypeTable.IsConcreteType((operandsList[0] as ILiteralExpression).ConstantType))
			{
				_IExpression iexpression = operandsList[0];
				ICompiledType type;
				if (iexpression == null)
				{
					type = null;
				}
				else
				{
					ICompiledType type2 = iexpression.Type;
					type = ((type2 != null) ? type2.DeRefType : null);
				}
				\u0002.Type = type;
				return;
			}
			_IExpression iexpression2 = operandsList[0];
			ICompiledType compiledType;
			if (iexpression2 == null)
			{
				compiledType = null;
			}
			else
			{
				ICompiledType type3 = iexpression2.Type;
				compiledType = ((type3 != null) ? type3.DeRefType : null);
			}
			ICompiledType compiledType2 = compiledType;
			if (compiledType2 != null && TypeTable.IsConcreteType(compiledType2.Class))
			{
				(operandsList[0] as _ILiteralExpression).ConstantType = compiledType2.Class;
				operandsList[0].Type = compiledType2;
				\u0002.Type = operandsList[0].Type;
				return;
			}
			\u0002.Type = TypeTable.UDInt;
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x0005E184 File Offset: 0x0005C384
		public void \u008C(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.DInt;
		}

		// Token: 0x06001CF3 RID: 7411 RVA: 0x0005E194 File Offset: 0x0005C394
		public void \u008D(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CF4 RID: 7412 RVA: 0x0005E1A4 File Offset: 0x0005C3A4
		public void \u008E(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CF5 RID: 7413 RVA: 0x0005E1B4 File Offset: 0x0005C3B4
		public void \u008F(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.UDInt;
		}

		// Token: 0x06001CF6 RID: 7414 RVA: 0x0005E1C4 File Offset: 0x0005C3C4
		public void \u0090(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Int;
		}

		// Token: 0x06001CF7 RID: 7415 RVA: 0x0005E1D4 File Offset: 0x0005C3D4
		public void \u0091(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ITargetSettings targetSettings = this.PreComLocal.GetTargetSettings();
			if (global::\u0016.\u0004.LRealDataType.GetBoolValue(targetSettings))
			{
				\u0002.Type = TypeTable.Real;
				using (IEnumerator<_IExpression> enumerator = operandsList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IExpression iexpression = enumerator.Current;
						if (iexpression.Type == null || iexpression.Type.DeRefType.Class != TypeClass.Real)
						{
							\u0002.Type = TypeTable.LReal;
							break;
						}
					}
					return;
				}
			}
			\u0002.Type = TypeTable.Real;
		}

		// Token: 0x06001CF8 RID: 7416 RVA: 0x0005E274 File Offset: 0x0005C474
		public void \u0092(_IOperatorExpression \u0002)
		{
			\u0002.Type = TypeTable.Bool;
		}

		// Token: 0x06001CF9 RID: 7417 RVA: 0x0005E284 File Offset: 0x0005C484
		public void \u0093(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			bool u = false;
			Operator code = \u0002.Code;
			if (code <= Operator.Not)
			{
				if (code - Operator.And > 5)
				{
					if (code != Operator.Not)
					{
						goto IL_1CB;
					}
					if (operandsList.Count != 0)
					{
						if (operandsList[0] is ILiteralExpression && !TypeTable.IsConcreteType((operandsList[0] as ILiteralExpression).ConstantType))
						{
							_IExpression iexpression = operandsList[0];
							ICompiledType compiledType;
							if (iexpression == null)
							{
								compiledType = null;
							}
							else
							{
								ICompiledType type = iexpression.Type;
								compiledType = ((type != null) ? type.DeRefType : null);
							}
							ICompiledType compiledType2 = compiledType;
							if (compiledType2 != null && TypeTable.IsConcreteType(compiledType2.Class))
							{
								(operandsList[0] as _ILiteralExpression).ConstantType = compiledType2.Class;
								\u0002.Type = compiledType2;
								return;
							}
						}
					}
				}
			}
			else if (code - Operator.Ampersand > 1 && code - Operator.And_Then > 1)
			{
				goto IL_1CB;
			}
			u = true;
			bool flag = true;
			bool flag2 = false;
			ICompiledType compiledType3 = null;
			for (int i = 0; i < operandsList.Count; i++)
			{
				_IExpression iexpression2 = operandsList[i];
				if (iexpression2.Type.DeRefType.Class == TypeClass.Bool || iexpression2.Type.DeRefType.Class == TypeClass.Bit || iexpression2.Type.DeRefType.Class == TypeClass.BitConst)
				{
					flag2 = true;
					if (compiledType3 == null || iexpression2.Type.DeRefType.Class == TypeClass.Bool)
					{
						compiledType3 = iexpression2.Type.DeRefType;
					}
				}
				if (iexpression2.Type.DeRefType.Class != TypeClass.Bool && iexpression2.Type.DeRefType.Class != TypeClass.Bit && iexpression2.Type.DeRefType.Class != TypeClass.BitConst)
				{
					flag = false;
					break;
				}
			}
			if (flag && flag2)
			{
				if (compiledType3 == null)
				{
					\u0002.Type = TypeTable.Get(TypeClass.Bool);
					return;
				}
				\u0002.Type = compiledType3;
				return;
			}
			IL_1CB:
			this.\u0001(\u0002, 0, u, operandsList);
		}

		// Token: 0x06001CFA RID: 7418 RVA: 0x0005E468 File Offset: 0x0005C668
		public void \u0094(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			ICompiledType type = null;
			if (TimeOperationTypeCalculator.\u0001(\u0002, this.Scope, out type))
			{
				\u0002.Type = type;
				return;
			}
			this.\u0001(\u0002, 0, false, operandsList);
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x0005E4A0 File Offset: 0x0005C6A0
		public void \u0095(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			switch (\u0002.Code)
			{
			case Operator.__vcAdd:
			case Operator.__vcSub:
			case Operator.__vcMul:
			case Operator.__vcDiv:
			case Operator.__vcMin:
			case Operator.__vcMax:
				if (operandsList[0].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType;
					return;
				}
				if (operandsList[1].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[1].Type.DeRefType;
					return;
				}
				\u0002.Type = operandsList[0].Type.DeRefType;
				return;
			case Operator.__vcDot:
				if (operandsList[0].Type.DeRefType.Class == TypeClass.__Vector && operandsList[0].Type.DeRefType.Class == TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType.BaseType;
					return;
				}
				\u0002.Type = TypeTable.Get(TypeClass.LReal);
				return;
			case Operator.__vcSqrt:
				if (operandsList[0].Type.DeRefType.Class != TypeClass.__Vector)
				{
					\u0002.Type = operandsList[0].Type.DeRefType;
					return;
				}
				\u0002.Type = operandsList[0].Type.DeRefType;
				return;
			case Operator.__vcSetReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(Operator.Real), global::\u0019.\u0003.\u0001((long)operandsList.Count, TypeClass.Int));
				return;
			case Operator.__vcSetLReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(Operator.LReal), global::\u0019.\u0003.\u0001((long)operandsList.Count, TypeClass.Int));
				return;
			case Operator.__vcLoadReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(TypeClass.Real), operandsList[0]);
				return;
			case Operator.__vcLoadLReal:
				\u0002.Type = global::\u0019.\u0003.\u0001(TypeTable.Get(TypeClass.LReal), operandsList[0]);
				return;
			case Operator.__vcStore:
				if (operandsList[1].Type.DeRefType.Class != TypeClass.__Vector)
				{
					\u0002.Type = operandsList[1].Type;
					return;
				}
				\u0002.Type = operandsList[1].Type;
				return;
			default:
				return;
			}
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x0005E6D4 File Offset: 0x0005C8D4
		internal void \u0001(_IOperatorExpression \u0002, int \u0003, bool \u0004, IList<_IExpression> \u0005)
		{
			if (\u0002.Type == null)
			{
				\u0002.Type = this.\u0001(\u0002.Code, \u0004, \u0003, \u0005, null);
			}
			if (\u0002.Type == null)
			{
				if (\u0005.Count == 0)
				{
					\u0002.Type = TypeTable.Get(TypeClass.Int);
					return;
				}
				\u0002.Type = \u0005[0].Type.DeRefType;
			}
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x0005E738 File Offset: 0x0005C938
		private void \u0096(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			if (operandsList.Count < 2)
			{
				return;
			}
			_IExpression iexpression = operandsList[1];
			\u0002.Type = ((iexpression != null) ? iexpression.Type.DeRefType : null);
			if (\u0002.Type == null)
			{
				return;
			}
			if (TypeTable.IsInteger(\u0002.Type.Class) || TypeTable.IsReal(\u0002.Type.Class))
			{
				ICompiledType compiledType = this.\u0001(\u0002.Code, false, 0, operandsList, null);
				if (compiledType != null)
				{
					\u0002.Type = compiledType;
				}
			}
		}

		// Token: 0x06001CFE RID: 7422 RVA: 0x0005E7BC File Offset: 0x0005C9BC
		private void \u0097(_IOperatorExpression \u0002)
		{
			foreach (_IExpression iexpression in \u0002._OperandsList)
			{
				TypeClass? typeClass = (iexpression != null) ? new TypeClass?(iexpression.Type.DeRefType.Class) : null;
				if (typeClass != null)
				{
					TypeClass valueOrDefault = typeClass.GetValueOrDefault();
					if (valueOrDefault <= TypeClass.TimeOfDay)
					{
						if (valueOrDefault != TypeClass.Bool && valueOrDefault - TypeClass.String > 5)
						{
							goto IL_83;
						}
					}
					else if (valueOrDefault != TypeClass.LTime && valueOrDefault - TypeClass.LDate > 2)
					{
						goto IL_83;
					}
					\u0002.Type = TypeTable.Get(iexpression.Type.DeRefType.Class);
				}
				IL_83:
				if (\u0002.Type != null)
				{
					break;
				}
			}
		}

		// Token: 0x06001CFF RID: 7423 RVA: 0x0005E880 File Offset: 0x0005CA80
		private void \u0098(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			for (int i = 1; i < operandsList.Count; i++)
			{
				_IExpression iexpression = operandsList[i];
				if (iexpression.Type != null)
				{
					bool flag = false;
					TypeClass @class = iexpression.Type.DeRefType.Class;
					if (@class <= TypeClass.Userdef)
					{
						if (@class == TypeClass.Bool)
						{
							goto IL_29C;
						}
						switch (@class)
						{
						case TypeClass.String:
						{
							_IStringType istringType = \u0002.Type as _IStringType;
							flag = true;
							if (istringType == null)
							{
								\u0002.Type = iexpression.Type.DeRefType;
							}
							else
							{
								int num = 0;
								if (istringType.Length == null)
								{
									num = 80;
								}
								else
								{
									_ILiteralValue iliteralValue = this.Scope.\u0001(istringType.Length, true) as _ILiteralValue;
									if (iliteralValue == null)
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
									if (!iliteralValue.GetInt(out num))
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
								}
								_IStringType istringType2 = iexpression.Type.DeRefType as _IStringType;
								int num2 = 0;
								if (istringType2.Length == null)
								{
									num2 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue2 = this.Scope.\u0001(istringType2.Length, true) as _ILiteralValue;
									if (iliteralValue2 == null || !iliteralValue2.GetInt(out num2))
									{
										break;
									}
								}
								if (num2 > num)
								{
									\u0002.Type = iexpression.Type.DeRefType;
								}
							}
							break;
						}
						case TypeClass.WString:
						{
							_IWStringType iwstringType = ((\u0002 != null) ? \u0002.Type : null) as _IWStringType;
							flag = true;
							if (iwstringType == null)
							{
								\u0002.Type = iexpression.Type.DeRefType;
							}
							else
							{
								int num3 = 0;
								if (iwstringType.Length == null)
								{
									num3 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue3 = this.Scope.\u0001(iwstringType.Length, true) as _ILiteralValue;
									if (iliteralValue3 == null)
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
									if (!iliteralValue3.GetInt(out num3))
									{
										\u0002.Type = iexpression.Type.DeRefType;
										break;
									}
								}
								_IWStringType iwstringType2 = iexpression.Type.DeRefType as _IWStringType;
								int num4 = 0;
								if (iwstringType2.Length == null)
								{
									num4 = 80;
								}
								else
								{
									_ILiteralValue iliteralValue4 = this.Scope.\u0001(iwstringType2.Length, true) as _ILiteralValue;
									if (iliteralValue4 == null || !iliteralValue4.GetInt(out num4))
									{
										break;
									}
								}
								if (num4 > num3)
								{
									\u0002.Type = iexpression.Type.DeRefType;
								}
							}
							break;
						}
						case TypeClass.Time:
						case TypeClass.Date:
						case TypeClass.DateAndTime:
						case TypeClass.TimeOfDay:
							goto IL_29C;
						case TypeClass.Pointer:
						case TypeClass.Array:
						case TypeClass.Userdef:
							\u0002.Type = iexpression.Type.DeRefType;
							break;
						}
					}
					else if (@class == TypeClass.LTime || @class - TypeClass.LDate <= 2)
					{
						goto IL_29C;
					}
					IL_2CA:
					if (\u0002.Type == null || flag)
					{
						goto IL_2D5;
					}
					break;
					IL_29C:
					\u0002.Type = TypeTable.Get(iexpression.Type.DeRefType.Class);
					goto IL_2CA;
				}
				IL_2D5:;
			}
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x0005EB74 File Offset: 0x0005CD74
		public void \u0099(_IOperatorExpression \u0002)
		{
			switch (\u0002.Code)
			{
			case Operator.Limit:
				this.\u0096(\u0002);
				return;
			case Operator.Min:
			case Operator.Max:
				this.\u0097(\u0002);
				break;
			case Operator.Mux:
			case Operator.Sel:
				this.\u0098(\u0002);
				break;
			}
			int u = 0;
			if (\u0002.Code == Operator.Mux || \u0002.Code == Operator.Sel)
			{
				u = 1;
			}
			this.\u0001(\u0002, u, false, \u0002._OperandsList);
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x0005EBEC File Offset: 0x0005CDEC
		private void \u0002(_IExpression \u0002)
		{
			if (\u0002.Type != null && \u0002.Type.Class == TypeClass.Userdef)
			{
				_IUserdefType u = \u0002.Type as _IUserdefType;
				_ISignature isignature = this.Scope.\u0001(u);
				if (isignature == null)
				{
					return;
				}
				if (isignature.GetFlag(SignatureFlag.Alias) && isignature.AllVariables.Count == 1)
				{
					\u0002.Type = (isignature.AllVariables[0].Type as _IType);
					this.\u0003(\u0002);
				}
			}
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x0005EC68 File Offset: 0x0005CE68
		private void \u0003(_IExpression \u0002)
		{
			_IUserdefType iuserdefType = \u0002.Type as _IUserdefType;
			if (iuserdefType != null && \u0002.PrecompileVariableId != -1)
			{
				_ISignature isignature = this.Scope.\u0001(iuserdefType.SignatureId);
				if (isignature != null && isignature.GetFlag(SignatureFlag.Enum))
				{
					\u0002.Type = global::\u0019.\u0003.\u0001(isignature.OrgName, isignature.PrecompileId);
				}
			}
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x0005ECC4 File Offset: 0x0005CEC4
		private void \u0001(_IVariableExpression \u0002, IVariable \u0003)
		{
			if (\u0003 != null)
			{
				ICompiledType type = \u0002.Type;
				if (type != null && type.Class == TypeClass.Lazy && \u0003.HasAttribute("inferredtype"))
				{
					string attributeValue = \u0003.GetAttributeValue("inferredtype");
					\u0002.Type = global::\u0011.\u0006.\u0001(attributeValue);
				}
			}
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x0005ED14 File Offset: 0x0005CF14
		private void \u0002(_IVariableExpression \u0002)
		{
			IGenericUserdefType u = this.GenericTypeStack.Peek().UserdefTypeIn;
			\u0002._CompiledType = this.\u0001(\u0002._CompiledType, u);
			IGenericUserdefType genericUserdefType = \u0002.Type as IGenericUserdefType;
			if (genericUserdefType != null)
			{
				this.GenericTypeStack.Peek().UserdefTypeOut = genericUserdefType;
			}
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x0005ED68 File Offset: 0x0005CF68
		private ICompiledType \u0001(ICompiledType \u0002, IGenericUserdefType \u0003)
		{
			_IPrecompileScope3 u001B_u = new CheckerScope(this.PreComLocal.ApplicationGuid, this.Signature, this.PreComLocal, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			Dictionary<string, _IExpression> dictionary = new Dictionary<string, _IExpression>();
			if (\u0003 != null)
			{
				_ISignature isignature = this.Scope.\u0001(\u0003);
				if (isignature == null)
				{
					return \u0002;
				}
				_IVariable[] array = isignature.\u0002().ToArray<_IVariable>();
				int num = 0;
				foreach (_IExpression value in \u0003.GenericConstantsInitializations)
				{
					dictionary.Add(array[num].Name, value);
					num++;
				}
			}
			if (\u0002 == null)
			{
				return null;
			}
			global::\u0001.\u0006 u = new global::\u0001.\u0006(u001B_u, dictionary);
			return GenericTypeReplacer.\u0001((_IType)\u0002, new global::\u0002.\u0002(), u);
		}

		// Token: 0x040004C8 RID: 1224
		[CompilerGenerated]
		private _IStatement \u0001;

		// Token: 0x040004C9 RID: 1225
		[CompilerGenerated]
		private global::\u000F.\u0007 \u0001;

		// Token: 0x040004CA RID: 1226
		[CompilerGenerated]
		private _IPreCompileContext \u0001;

		// Token: 0x040004CB RID: 1227
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x040004CC RID: 1228
		[CompilerGenerated]
		private Stack<global::\u0017.\u000E.\u0001> \u0001;

		// Token: 0x02000194 RID: 404
		private sealed class \u0001
		{
			// Token: 0x170005C7 RID: 1479
			// (get) Token: 0x06001D06 RID: 7430 RVA: 0x0005EE44 File Offset: 0x0005D044
			// (set) Token: 0x06001D07 RID: 7431 RVA: 0x0005EE4C File Offset: 0x0005D04C
			public IGenericUserdefType UserdefTypeIn { get; set; }

			// Token: 0x170005C8 RID: 1480
			// (get) Token: 0x06001D08 RID: 7432 RVA: 0x0005EE58 File Offset: 0x0005D058
			// (set) Token: 0x06001D09 RID: 7433 RVA: 0x0005EE60 File Offset: 0x0005D060
			public IGenericUserdefType UserdefTypeOut { get; set; }

			// Token: 0x040004CD RID: 1229
			[CompilerGenerated]
			private IGenericUserdefType \u0001;

			// Token: 0x040004CE RID: 1230
			[CompilerGenerated]
			private IGenericUserdefType \u0002;
		}
	}
}
