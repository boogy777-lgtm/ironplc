using System;
using System.Collections.Generic;
using \u000E;
using \u0011;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200026F RID: 623
	public class CheckPointerReplacer : AbstractReplacer, IReplacer
	{
		// Token: 0x060027C9 RID: 10185 RVA: 0x0008A1B8 File Offset: 0x000883B8
		internal CheckPointerReplacer(IScope5 scope, _ICompileContext comcon, CheckFunctions checkFunctions, global::\u000E.\u0011 context)
		{
			if (scope == null)
			{
				throw new ArgumentNullException("scope");
			}
			this.\u0001 = scope;
			if (comcon == null)
			{
				throw new ArgumentNullException("comcon");
			}
			this.\u0001 = comcon;
			if (checkFunctions == null)
			{
				throw new ArgumentNullException("checkFunctions");
			}
			this.\u0001 = checkFunctions;
			this.\u0001 = context;
			this.\u0001 = \u0081.\u0010.\u0001(this, context);
		}

		// Token: 0x060027CA RID: 10186 RVA: 0x0008A224 File Offset: 0x00088424
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.\u0001 = cpou;
			if (this.\u0001.\u0001(this.\u0001, cpou, false, this.\u0001))
			{
				this.\u0001.ReplaceCode(cpou);
			}
		}

		// Token: 0x060027CB RID: 10187 RVA: 0x0008A254 File Offset: 0x00088454
		public override _IExpression ReplaceDeRefAccessExpression(_IDeRefAccessExpression deRefAccessExpression, bool bReadAccess)
		{
			this.\u0001(deRefAccessExpression, bReadAccess);
			return deRefAccessExpression;
		}

		// Token: 0x060027CC RID: 10188 RVA: 0x0008A260 File Offset: 0x00088460
		private void \u0001(_IDeRefAccessExpression \u0002, bool \u0003)
		{
			bool flag = true;
			IVariable variable = \u0002._Base.GetVariable(this.\u0001);
			if (variable != null && variable.GetFlag(VarFlag.Inout))
			{
				flag = false;
			}
			if (this.\u0001.m_stCheckPointerFun != null && flag && this.\u0001.\u0001(this.\u0001, this.\u0001, this.\u0001.\u0001, this.\u0001) && \u0002.Base.ToString() != IdentifierConstants.InstancePointer && !(\u0002.Base is _IThisExpression) && !(\u0002.Base is _IBaseExpression) && this.\u0001.CheckForCheckFunHide(this.\u0001, this.\u0001.m_stCheckPointerFun))
			{
				long num = (long)\u0002.Type.Size(this.\u0001);
				int num2 = Locator.\u0002(\u0002.Type.DeRefType, this.\u0001.DataManager.MinSize, this.\u0001);
				bool u = !\u0003;
				IMinimalPosition position = \u0002._Base._Position;
				_ISignature isignature = (_ISignature)this.\u0001.GetSignature(this.\u0001.m_stCheckPointerFun);
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature.Name);
				iuserdefType.SignatureId = isignature.Id;
				_IExpression iexpression = (_IExpression)new global::\u0011.\u0006(this.\u0001.m_stCheckPointerFun).\u0002();
				iexpression.Type = iuserdefType;
				_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
				icallExpression.Type = \u0002._Base.Type;
				IList<_IVariable> allVariables = isignature.AllVariables;
				icallExpression.AddParam(\u0002._Base.Duplicate() as _IExpression, global::\u0019.\u0003.\u0001(allVariables[1], isignature));
				_ILiteralExpression iliteralExpression = global::\u0019.\u0003.\u0001(num);
				iliteralExpression._CompiledType = TypeTable.DInt;
				icallExpression.AddParam(iliteralExpression, global::\u0019.\u0003.\u0001(allVariables[2], isignature));
				_ILiteralExpression iliteralExpression2 = global::\u0019.\u0003.\u0001((long)num2);
				iliteralExpression2._CompiledType = TypeTable.DInt;
				icallExpression.AddParam(iliteralExpression2, global::\u0019.\u0003.\u0001(allVariables[3], isignature));
				_ILiteralExpression iliteralExpression3 = global::\u0019.\u0003.\u0001(u);
				iliteralExpression3._CompiledType = TypeTable.Bool;
				icallExpression.AddParam(iliteralExpression3, global::\u0019.\u0003.\u0001(allVariables[4], isignature));
				icallExpression.SetPositionIntern(position);
				OptionalInputsProvider.\u0001(this.\u0001, icallExpression);
				\u0002._Base = icallExpression;
			}
		}

		// Token: 0x060027CD RID: 10189 RVA: 0x0008A4C4 File Offset: 0x000886C4
		public override _IExpression ReplaceIndexAccessExpression(_IIndexAccessExpression indexAccessExpression, bool bReadAccess)
		{
			IVariable variable;
			if (indexAccessExpression._Var.Type.Class == TypeClass.Pointer && this.\u0001.m_stCheckPointerFun != null && this.\u0001.\u0001(this.\u0001, this.\u0001, this.\u0001.\u0001, this.\u0001) && this.\u0001.CheckForCheckFunHide(this.\u0001, this.\u0001.m_stCheckPointerFun) && indexAccessExpression.NumAccesses == 1 && !CheckBoundsInserter.IsVarLenArray(indexAccessExpression, this.\u0001, out variable))
			{
				long num = (long)indexAccessExpression.Type.Size(this.\u0001);
				int num2 = Locator.\u0002(indexAccessExpression._Var.Type.DeRefType, this.\u0001.DataManager.MinSize, this.\u0001);
				bool u = !bReadAccess;
				IMinimalPosition position = indexAccessExpression._Position;
				_ISignature isignature = (_ISignature)this.\u0001.GetSignature(this.\u0001.m_stCheckPointerFun);
				_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(isignature.Name);
				iuserdefType.SignatureId = isignature.Id;
				_IExpression iexpression = (_IExpression)new global::\u0011.\u0006(this.\u0001.m_stCheckPointerFun).\u0002();
				iexpression.Type = iuserdefType;
				_ICallExpression icallExpression = global::\u0019.\u0003.\u0001(iexpression, Token.Empty);
				icallExpression.Type = indexAccessExpression._Var.Type;
				IList<_IVariable> allVariables = isignature.AllVariables;
				_IExpression exp = this.\u0001(indexAccessExpression);
				icallExpression.AddParam(exp, global::\u0019.\u0003.\u0001(allVariables[1], isignature));
				_ILiteralExpression iliteralExpression = global::\u0019.\u0003.\u0001(num);
				iliteralExpression._CompiledType = TypeTable.DInt;
				icallExpression.AddParam(iliteralExpression, global::\u0019.\u0003.\u0001(allVariables[2], isignature));
				_ILiteralExpression iliteralExpression2 = global::\u0019.\u0003.\u0001((long)num2);
				iliteralExpression2._CompiledType = TypeTable.DInt;
				icallExpression.AddParam(iliteralExpression2, global::\u0019.\u0003.\u0001(allVariables[3], isignature));
				_ILiteralExpression iliteralExpression3 = global::\u0019.\u0003.\u0001(u);
				iliteralExpression3._CompiledType = TypeTable.Bool;
				icallExpression.AddParam(iliteralExpression3, global::\u0019.\u0003.\u0001(allVariables[4], isignature));
				icallExpression.SetPositionIntern(position);
				OptionalInputsProvider.\u0001(this.\u0001, icallExpression);
				_IDeRefAccessExpression ideRefAccessExpression = global::\u0019.\u0003.\u0001();
				ideRefAccessExpression.Type = indexAccessExpression.Type;
				ideRefAccessExpression._Base = icallExpression;
				return ideRefAccessExpression;
			}
			return indexAccessExpression;
		}

		// Token: 0x060027CE RID: 10190 RVA: 0x0008A708 File Offset: 0x00088908
		private _IExpression \u0001(_IIndexAccessExpression \u0002)
		{
			TypeClass @class = \u0002.Accesses[0].Type.Class;
			_ILiteralExpression iliteralExpression = global::\u0019.\u0003.\u0001((long)\u0002._Var.Type.BaseType.Size(this.\u0001), @class);
			iliteralExpression._CompiledType = TypeTable.Get(@class);
			_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Times, iliteralExpression, \u0002.Accesses[0]);
			ioperatorExpression._CompiledType = TypeTable.Get(@class);
			_IOperatorExpression ioperatorExpression2 = global::\u0019.\u0003.\u0001(Operator.Plus, \u0002._Var, ioperatorExpression);
			ioperatorExpression2._CompiledType = global::\u0019.\u0003.\u0001(TypeTable.Byte);
			return ioperatorExpression2;
		}

		// Token: 0x04000753 RID: 1875
		private readonly IScope5 \u0001;

		// Token: 0x04000754 RID: 1876
		private readonly _ICompileContext \u0001;

		// Token: 0x04000755 RID: 1877
		private readonly CheckFunctions \u0001;

		// Token: 0x04000756 RID: 1878
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x04000757 RID: 1879
		private readonly \u0081.\u0010 \u0001;

		// Token: 0x04000758 RID: 1880
		private _ICompiledPOU \u0001;
	}
}
