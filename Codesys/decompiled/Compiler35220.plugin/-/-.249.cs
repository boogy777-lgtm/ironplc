using System;
using System.Runtime.CompilerServices;
using System.Text;
using \u0001;
using \u000E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0006
{
	// Token: 0x020002AF RID: 687
	internal sealed class \u000E
	{
		// Token: 0x17000763 RID: 1891
		// (get) Token: 0x06002AA4 RID: 10916 RVA: 0x00095608 File Offset: 0x00093808
		private IScope5 Scope { get; }

		// Token: 0x17000764 RID: 1892
		// (get) Token: 0x06002AA5 RID: 10917 RVA: 0x00095610 File Offset: 0x00093810
		private ComplexPropertyInfoGenerator ComplexGenerator { get; }

		// Token: 0x06002AA6 RID: 10918 RVA: 0x00095618 File Offset: 0x00093818
		internal \u000E(IScope5 \u009B\u0002, _ICompileContext \u0001\u0002)
		{
			this.Scope = \u009B\u0002;
			this.ComplexGenerator = new ComplexPropertyInfoGenerator(\u009B\u0002, \u0001\u0002);
		}

		// Token: 0x06002AA7 RID: 10919 RVA: 0x00095634 File Offset: 0x00093834
		public _IStatement \u0001(_IOperatorExpression \u0002, global::\u000E.\u0011 \u0003)
		{
			_IExpression iexpression = \u0002[0];
			_IExpression iexpression2 = \u0002[1];
			_IVariable ivariable = iexpression.GetVariable(this.Scope) as _IVariable;
			if (ivariable == null)
			{
				throw new ArgumentException("Not a valid property expression");
			}
			_IStatement istatement = null;
			if (ivariable.IsProperty)
			{
				_ICompoAccessExpression icompoAccessExpression = iexpression as _ICompoAccessExpression;
				if (icompoAccessExpression != null)
				{
					_IType itype = (_IType)icompoAccessExpression._Left._CompiledType;
					_ISignature isignature = (_ISignature)((_IUserdefType)itype.DeRefType).GetSignature(this.Scope);
					bool flag = isignature.POUType == Operator.Interface || isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion);
					bool flag2 = itype.Class == TypeClass.Reference;
					bool flag3 = icompoAccessExpression._Left is _IDeRefAccessExpression;
					if (flag || flag2 || flag3)
					{
						istatement = this.ComplexGenerator.\u0001(((_IVariableExpression)icompoAccessExpression.Right).Name, icompoAccessExpression._Left, iexpression2, \u0003, flag, flag2, flag3);
					}
				}
				if (istatement == null)
				{
					string u = this.\u0001(iexpression, iexpression2, ivariable);
					istatement = \u0003.Generator.\u0001(u, \u0003._Scope, \u0003.CompiledPOU);
				}
			}
			else
			{
				string u2 = string.Format("{0}.pVarAdr := ADR({1})", iexpression2, iexpression);
				istatement = \u0003.Generator.\u0001(u2, \u0003._Scope, \u0003.CompiledPOU);
			}
			return istatement;
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x00095784 File Offset: 0x00093984
		private string \u0001(_IExpression \u0002, _IExpression \u0003, _IVariable \u0004)
		{
			_ICompoAccessExpression icompoAccessExpression = \u0002 as _ICompoAccessExpression;
			int nId = (icompoAccessExpression != null) ? icompoAccessExpression._Right.SignatureId : \u0002.SignatureId;
			ISignature signature = this.Scope[nId];
			if (signature == null)
			{
				throw new ArgumentException("Not a valid property expression");
			}
			\u0081.\u000F u000F = global::\u0001.\u000E.\u0001(this.Scope, \u0004.Name, signature, icompoAccessExpression);
			string arg = u000F.HasGetter ? ("ADR(" + u000F.\u0002 + ")") : "0";
			string arg2 = u000F.HasSetter ? ("ADR(" + u000F.\u0001 + ")") : "0";
			string arg3;
			if (icompoAccessExpression != null)
			{
				if (signature.POUType == Operator.VarGlobal || signature.POUType == Operator.Program)
				{
					arg3 = "DWORD#0";
				}
				else
				{
					string str = "ADR(";
					IExpression left = icompoAccessExpression.Left;
					arg3 = str + ((left != null) ? left.ToString() : null) + ")";
				}
			}
			else if (\u0004.GetFlag(VarFlag.Global))
			{
				arg3 = "0";
			}
			else
			{
				arg3 = "THIS";
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("{implicit on}");
			stringBuilder.AppendLine(string.Format("{0}.PGETTER := {1};", \u0003, arg));
			stringBuilder.AppendLine(string.Format("{0}.PSETTER := {1};", \u0003, arg2));
			stringBuilder.AppendLine("{implicit off}");
			stringBuilder.AppendLine(string.Format("{0}.PINSTANCE := {1};", \u0003, arg3));
			return stringBuilder.ToString();
		}

		// Token: 0x04000806 RID: 2054
		[CompilerGenerated]
		private readonly IScope5 \u0001;

		// Token: 0x04000807 RID: 2055
		[CompilerGenerated]
		private readonly ComplexPropertyInfoGenerator \u0001;
	}
}
