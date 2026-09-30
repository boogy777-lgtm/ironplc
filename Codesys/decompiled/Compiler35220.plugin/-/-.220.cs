using System;
using System.Runtime.CompilerServices;
using \u000E;
using \u0014;
using \u0019;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u000F
{
	// Token: 0x02000262 RID: 610
	internal sealed class \u0012
	{
		// Token: 0x17000712 RID: 1810
		// (get) Token: 0x0600276A RID: 10090 RVA: 0x00087F80 File Offset: 0x00086180
		private global::\u000E.\u0011 Context { get; }

		// Token: 0x17000713 RID: 1811
		// (get) Token: 0x0600276B RID: 10091 RVA: 0x00087F88 File Offset: 0x00086188
		private IScope5 _Scope
		{
			get
			{
				return this.Context._Scope;
			}
		}

		// Token: 0x17000714 RID: 1812
		// (get) Token: 0x0600276C RID: 10092 RVA: 0x00087FA4 File Offset: 0x000861A4
		private _ICompiledPOU CompiledPOU { get; }

		// Token: 0x17000715 RID: 1813
		// (get) Token: 0x0600276D RID: 10093 RVA: 0x00087FAC File Offset: 0x000861AC
		private AssignmentInfo Assign { get; }

		// Token: 0x17000716 RID: 1814
		// (get) Token: 0x0600276E RID: 10094 RVA: 0x00087FB4 File Offset: 0x000861B4
		private _IVariable VarLeft { get; }

		// Token: 0x17000717 RID: 1815
		// (get) Token: 0x0600276F RID: 10095 RVA: 0x00087FBC File Offset: 0x000861BC
		private bool CallAssign
		{
			get
			{
				return this.CallScope != null;
			}
		}

		// Token: 0x17000718 RID: 1816
		// (get) Token: 0x06002770 RID: 10096 RVA: 0x00087FC8 File Offset: 0x000861C8
		private IScope5 CallScope { get; }

		// Token: 0x06002771 RID: 10097 RVA: 0x00087FD0 File Offset: 0x000861D0
		internal \u0012(global::\u000E.\u0011 \u0083\u0005, _ICompiledPOU \u0014\u0006, AssignmentInfo \u0015\u0006, _IVariable \u0016\u0006)
		{
			this.Context = \u0083\u0005;
			this.CompiledPOU = \u0014\u0006;
			this.Assign = \u0015\u0006;
			this.VarLeft = \u0016\u0006;
			this.CallScope = null;
		}

		// Token: 0x06002772 RID: 10098 RVA: 0x00087FFC File Offset: 0x000861FC
		internal \u0012(global::\u000E.\u0011 \u0083\u0005, _ICompiledPOU \u0014\u0006, AssignmentInfo \u0015\u0006, _IVariable \u0016\u0006, IScope5 \u0017\u0006)
		{
			this.Context = \u0083\u0005;
			this.CompiledPOU = \u0014\u0006;
			this.Assign = \u0015\u0006;
			this.VarLeft = \u0016\u0006;
			this.CallScope = \u0017\u0006;
		}

		// Token: 0x06002773 RID: 10099 RVA: 0x0008802C File Offset: 0x0008622C
		private bool \u0001(AssignmentInfo \u0002, _IVariable \u0003)
		{
			bool flag = \u0003 != null && \u0003.Type.Class == TypeClass.Reference && \u0002.KindOf == Operator.Assign;
			return \u0003 != null && \u0003.IsProperty && !flag;
		}

		// Token: 0x06002774 RID: 10100 RVA: 0x00088070 File Offset: 0x00086270
		private \u0001 \u0001<\u0001>(\u0001 \u0002, IScope5 \u0003 = null) where \u0001 : _IExprement
		{
			if (\u0003 == null)
			{
				return this.Context.Generator.\u0001<\u0001>(\u0002, this._Scope, this.CompiledPOU);
			}
			return this.Context.Generator.\u0001<\u0001>(\u0002, \u0003, this.CompiledPOU);
		}

		// Token: 0x06002775 RID: 10101 RVA: 0x000880BC File Offset: 0x000862BC
		private bool \u0001(_IUserdefType \u0002, out ISignature \u0003)
		{
			\u0003 = \u0002.GetSignature(this._Scope);
			return \u0003.GetFlag(SignatureFlag.ImplicitInterfaceUnion);
		}

		// Token: 0x06002776 RID: 10102 RVA: 0x000880DC File Offset: 0x000862DC
		private bool \u0001(_IExpression \u0002)
		{
			ILiteralValue literalValue = \u0002.Literal(this._Scope);
			int num;
			return literalValue != null && literalValue.GetInt(out num) && num == 0;
		}

		// Token: 0x06002777 RID: 10103 RVA: 0x0008810C File Offset: 0x0008630C
		internal AssignmentInfo \u0002()
		{
			bool flag = (this.CallAssign || this.Assign.KindOf == Operator.RefAssign) && this.VarLeft != null && this.VarLeft.Type.Class == TypeClass.Reference;
			_IUserdefType iuserdefType = this.Assign.LValue.Type.DeRefType as _IUserdefType;
			ISignature signature;
			if (iuserdefType == null || this.\u0001(this.Assign, this.VarLeft) || flag || !this.\u0001(iuserdefType, out signature))
			{
				return this.Assign;
			}
			_IUserdefType iuserdefType2 = this.Assign.RValue.Type.DeRefType as _IUserdefType;
			ISignature u = signature;
			_IUserdefType iuserdefType3 = signature["__Interface"].CompiledType.BaseType as _IUserdefType;
			if (iuserdefType3 != null)
			{
				signature = iuserdefType3.GetSignature(this._Scope);
			}
			_IExpression iexpression = null;
			string u2 = null;
			if (signature.HasAttribute(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID))
			{
				u2 = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID);
			}
			if (iuserdefType2 != null)
			{
				iexpression = this.\u0001(iuserdefType2, signature, u2, u);
			}
			else if (this.\u0001(this.Assign.RValue))
			{
				iexpression = \u0019.\u0003.\u0001(0L);
			}
			if (iexpression != null)
			{
				return this.\u0001(iexpression);
			}
			return this.Assign;
		}

		// Token: 0x06002778 RID: 10104 RVA: 0x00088264 File Offset: 0x00086464
		private AssignmentInfo \u0001(_IExpression \u0002)
		{
			AssignmentInfo result;
			if (this.CallAssign)
			{
				_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(this.Assign.LValue, Token.Empty);
				icompoAccessExpression._Left.Type = (this.Assign.LValue.Type as _IType);
				icompoAccessExpression._Right = \u0019.\u0003.\u0001("__Interface");
				IVariable variable = ((_IUserdefType)this.Assign.LValue.Type.DeRefType).GetSignature((this.Assign.KindOf == Operator.AssignOut) ? this._Scope : this.CallScope)["__Interface"];
				icompoAccessExpression._Right.Type = (variable.Type as _IType);
				icompoAccessExpression.Type = (variable.Type as _IType);
				_IExpression iexpression = (this.Assign.KindOf == Operator.AssignOut) ? this.\u0001<_ICompoAccessExpression>(icompoAccessExpression, null) : this.\u0001<_ICompoAccessExpression>(icompoAccessExpression, this.CallScope);
				_IExpression rvalue = (this.Assign.KindOf == Operator.AssignOut) ? this.\u0001<_IExpression>(\u0002, this.CallScope) : this.\u0001<_IExpression>(\u0002, null);
				\u0019.\u0003.\u0001(iexpression)._RValue = rvalue;
				result = new AssignmentInfo(iexpression, rvalue, Operator.Assign);
			}
			else
			{
				_ICompoAccessExpression icompoAccessExpression2 = \u0019.\u0003.\u0001(this.Assign.LValue, Token.Empty);
				icompoAccessExpression2._Right = \u0019.\u0003.\u0001("__Interface");
				_IAssignmentExpression iassignmentExpression = \u0019.\u0003.\u0001(icompoAccessExpression2);
				iassignmentExpression._RValue = \u0002;
				result = new AssignmentInfo(this.\u0001<_IAssignmentExpression>(iassignmentExpression, null));
			}
			this.Context.Generator.CopyPositionAndMessages(this.Assign.PositionExpression, result.PositionExpression);
			return result;
		}

		// Token: 0x06002779 RID: 10105 RVA: 0x00088434 File Offset: 0x00086634
		private _IExpression \u0001(_IUserdefType \u0002, ISignature \u0003, string \u0004, ISignature \u0005)
		{
			_IExpression result = null;
			_ISignature isignature = (_ISignature)\u0002.GetSignature(this._Scope);
			if (!isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				_IVariable ivariable = this.\u0001(isignature, \u0003.Id, \u0004) as _IVariable;
				if (ivariable != null)
				{
					ivariable.AddCrossReference(this.CompiledPOU.SignatureId, null);
					if (ivariable.DataLocation.Offset != 0)
					{
						_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(this.Assign.RValue, Token.Empty);
						icompoAccessExpression._Right = \u0019.\u0003.\u0001(ivariable.VersionedName);
						_IOperatorExpression ioperatorExpression = \u0019.\u0003.\u0001(Operator.Adr, Token.Empty);
						ioperatorExpression.AddOperand(icompoAccessExpression);
						result = ioperatorExpression;
					}
				}
			}
			else if (isignature.Id != \u0005.Id && !this.\u0001(isignature, \u0004))
			{
				result = this.\u0001(\u0003 as _ISignature, isignature);
			}
			return result;
		}

		// Token: 0x0600277A RID: 10106 RVA: 0x00088508 File Offset: 0x00086708
		private int \u0001(_ISignature \u0002, int \u0003, string \u0004)
		{
			if (\u0002.Id == \u0003)
			{
				return \u0002.Id;
			}
			foreach (int nId in \u0002.InterfaceIds)
			{
				_ISignature isignature = (_ISignature)this._Scope[nId];
				if (\u0004 != null && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID))
				{
					string attributeValue = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID);
					if (\u0004 == attributeValue)
					{
						return isignature.Id;
					}
				}
			}
			if (\u0002.BaseSignatureId != -1)
			{
				_ISignature u = this._Scope[\u0002.BaseSignatureId] as _ISignature;
				int num = this.\u0001(u, \u0003, \u0004);
				if (num != -1)
				{
					return num;
				}
			}
			return -1;
		}

		// Token: 0x0600277B RID: 10107 RVA: 0x000885B4 File Offset: 0x000867B4
		private IVariable \u0001(_ISignature \u0002, int \u0003, string \u0004)
		{
			IVariable variable = \u0002[IdentifierConstants.GetInterfacePointerName(\u0003)];
			if (variable != null)
			{
				return variable;
			}
			if (\u0002.BaseSignatureId != -1)
			{
				_ISignature u = this._Scope[\u0002.BaseSignatureId] as _ISignature;
				variable = this.\u0001(u, \u0003, \u0004);
				if (variable != null)
				{
					return variable;
				}
			}
			if (string.IsNullOrEmpty(\u0004))
			{
				return null;
			}
			int nId = this.\u0001(\u0002, \u0003, \u0004);
			return \u0002[IdentifierConstants.GetInterfacePointerName(nId)];
		}

		// Token: 0x0600277C RID: 10108 RVA: 0x00088624 File Offset: 0x00086824
		private bool \u0001(_ISignature \u0002, string \u0003)
		{
			if (string.IsNullOrWhiteSpace(\u0003))
			{
				return false;
			}
			if (!\u0002.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				return false;
			}
			if (\u0002.AllVariables.Count < 1 || \u0002.AllVariables[0].CompiledType == null)
			{
				return false;
			}
			_IUserdefType iuserdefType = \u0002.AllVariables[0].CompiledType.BaseType as _IUserdefType;
			if (iuserdefType == null)
			{
				return false;
			}
			ISignature signature = iuserdefType.GetSignature(this._Scope);
			return signature != null && signature.HasAttribute(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID) && signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMPATIBILITY_ID) == \u0003;
		}

		// Token: 0x0600277D RID: 10109 RVA: 0x000886C4 File Offset: 0x000868C4
		private _IExpression \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			int signatureId = ((_IUserdefType)\u0003.AllVariables[0].CompiledType.BaseType).SignatureId;
			string text = \u0003.Name.Replace("__UNION", "__STRUCT");
			text += string.Format("__{0}", signatureId);
			text = Helper.\u0001(text, \u0003, this.Context.Comcon);
			_IVariable ivariable = (_IVariable)global::\u0014.\u0005.\u0001(this.Context.Comcon, text)[IdentifierConstants.GetInterfacePointerName(\u0002.Id)];
			if (ivariable.DataLocation.Offset > 0)
			{
				int num = ivariable.DataLocation.Offset;
				if (this.\u0001(CodegeneratorProperties.WordAddressing))
				{
					num /= 2;
				}
				_ICallExpression icallExpression = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(\u0019.\u0003.\u0001("__CheckedInterfaceAssign"), Token.Empty), Token.Empty);
				_ILiteralExpression exp = \u0019.\u0003.\u0001((long)num);
				_ICompoAccessExpression icompoAccessExpression = \u0019.\u0003.\u0001(this.Assign.RValue, Token.Empty);
				icompoAccessExpression._Right = \u0019.\u0003.\u0001("__Interface");
				icallExpression.AddParam(icompoAccessExpression);
				icallExpression.AddParam(exp);
				return icallExpression;
			}
			return null;
		}

		// Token: 0x0600277E RID: 10110 RVA: 0x000887F4 File Offset: 0x000869F4
		private bool \u0001(CodegeneratorProperties \u0002)
		{
			ICodegenerator3 codegenerator = this.Context.CodeGen as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(\u0002);
		}

		// Token: 0x0400072C RID: 1836
		[CompilerGenerated]
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x0400072D RID: 1837
		[CompilerGenerated]
		private readonly _ICompiledPOU \u0001;

		// Token: 0x0400072E RID: 1838
		[CompilerGenerated]
		private readonly AssignmentInfo \u0001;

		// Token: 0x0400072F RID: 1839
		[CompilerGenerated]
		private readonly _IVariable \u0001;

		// Token: 0x04000730 RID: 1840
		[CompilerGenerated]
		private readonly IScope5 \u0001;
	}
}
