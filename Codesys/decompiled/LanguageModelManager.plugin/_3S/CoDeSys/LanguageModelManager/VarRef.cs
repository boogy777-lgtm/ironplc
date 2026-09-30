using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000106 RID: 262
	internal class VarRef : IVarRef4, IVarRef3, IVarRef2, IVarRef
	{
		// Token: 0x06001346 RID: 4934 RVA: 0x00035834 File Offset: 0x00034834
		public VarRef(_IExpression exp, Guid guidApplication, IScope5 scope)
		{
			this._WatchExpression = exp;
			this.ApplicationGuid = guidApplication;
			this.SignatureId = scope.MostLocalSignatureId;
			ISignature signature = (exp != null) ? exp.GetSignatureEx(scope) : null;
			this.DeclaringSignatureId = ((signature != null) ? signature.Id : this.SignatureId);
			IVariable variable = (exp != null) ? exp.GetVariable(scope) : null;
			if (variable != null && variable.Address != null && variable.Address.Size == DirectVariableSize.X && exp.Type.Class == TypeClass.Bool)
			{
				this._WatchExpression = LanguageModelBuilder.Singleton.CreateAddressExpression(variable.Address);
				this._WatchExpression.Type = TypeTable.Bit;
			}
			if (this._WatchExpression == null || this._WatchExpression.Type == null)
			{
				this.SetFlag(VarRefFlag.Invalid, true);
				return;
			}
			this.SetFlag(VarRefFlag.Constant, this._WatchExpression.IsConstant(scope, true));
			TypeClass @class = this._WatchExpression.Type.DeRefType.Class;
			if (@class <= TypeClass.Array)
			{
				if (@class != TypeClass.Pointer && @class != TypeClass.Array)
				{
					goto IL_157;
				}
			}
			else if (@class != TypeClass.Userdef)
			{
				if (@class != TypeClass.__Vector)
				{
					goto IL_157;
				}
			}
			else
			{
				ISignature signature2 = ((UserdefType)this._WatchExpression.Type.DeRefType).GetSignature(scope);
				if (signature2 == null)
				{
					goto IL_157;
				}
				if (!signature2.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					this.SetFlag(VarRefFlag.Extensible, true);
					goto IL_157;
				}
				this.SetFlag(VarRefFlag.Interface, true);
				this.SetFlag(VarRefFlag.Extensible, true);
				goto IL_157;
			}
			this.SetFlag(VarRefFlag.Extensible, true);
			IL_157:
			if (this._WatchExpression is _ICompoAccessExpression)
			{
				_ICompoAccessExpression icompoAccessExpression = this._WatchExpression as _ICompoAccessExpression;
				if (icompoAccessExpression._Right is _IVariableExpression)
				{
					_IVariableExpression ivariableExpression = icompoAccessExpression._Right as _IVariableExpression;
					_IVariable ivariable = ivariableExpression.GetVariable(scope) as _IVariable;
					if (ivariable != null && ivariable.IsProperty && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING_INSTEAD) && (!ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER) || ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_NOWATCH)))
					{
						this.SetFlag(VarRefFlag.Property, true);
						if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
						{
							string attributeValue = ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
							if (attributeValue == CompileAttributes.ATTRIBUTEVALUE_CALL)
							{
								this.SetFlag(VarRefFlag.PropertyByCall, true);
							}
							else if (attributeValue == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
							{
								this.SetFlag(VarRefFlag.PropertyByVariable, true);
							}
						}
					}
					else
					{
						string empty = string.Empty;
						ISignature signature3 = ivariableExpression.GetSignature(scope);
						if (signature3 != null && signature3.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING) && signature3.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
						{
							this.SetFlag(VarRefFlag.FunctionByCall, true);
						}
					}
				}
			}
			if (this._WatchExpression is _IVariableExpression)
			{
				_IVariable ivariable2 = (this._WatchExpression as _IVariableExpression).GetVariable(scope) as _IVariable;
				if (ivariable2 != null && ivariable2.IsProperty && !ivariable2.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING_INSTEAD) && (!ivariable2.HasAttribute(CompileAttributes.DEVICE_PARAMETER) || ivariable2.HasAttribute(CompileAttributes.ATTRIBUTE_NOWATCH)))
				{
					this.SetFlag(VarRefFlag.Property, true);
					if (ivariable2.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
					{
						string attributeValue2 = ivariable2.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
						if (attributeValue2 == CompileAttributes.ATTRIBUTEVALUE_CALL)
						{
							this.SetFlag(VarRefFlag.PropertyByCall, true);
						}
						else if (attributeValue2 == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
						{
							this.SetFlag(VarRefFlag.PropertyByVariable, true);
						}
					}
				}
			}
			if (this._WatchExpression is _ICallExpression)
			{
				_ICallExpression icallExpression = this._WatchExpression as _ICallExpression;
				if (icallExpression != null)
				{
					ISignature signature4 = null;
					_IVariableExpression ivariableExpression2 = icallExpression.Callee as _IVariableExpression;
					if (ivariableExpression2 != null && ivariableExpression2.SignatureId != -1 && APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication) != null)
					{
						signature4 = APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(guidApplication).GetSignatureById(ivariableExpression2.SignatureId);
					}
					if (signature4 == null)
					{
						string text = icallExpression.Callee.ToString();
						signature4 = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication).GetSignature(text);
					}
					if (signature4 != null && signature4.HasAttribute(CompileAttributes.ATTRIBUTE_MONITORING))
					{
						string text = signature4.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING);
						if (text == CompileAttributes.ATTRIBUTEVALUE_CALL)
						{
							this.SetFlag(VarRefFlag.FunctionByCall, true);
							return;
						}
					}
					else
					{
						this.SetFlag(VarRefFlag.Invalid, true);
					}
				}
			}
		}

		// Token: 0x06001347 RID: 4935 RVA: 0x00035C56 File Offset: 0x00034C56
		public VarRef(_IExpression exp, Guid guidApplication, IScope5 scope, _IExpression instancePathExpression) : this(exp, guidApplication, scope)
		{
			this.m_expInstPath = instancePathExpression;
		}

		// Token: 0x17000562 RID: 1378
		// (get) Token: 0x06001348 RID: 4936 RVA: 0x00035C69 File Offset: 0x00034C69
		// (set) Token: 0x06001349 RID: 4937 RVA: 0x00035C71 File Offset: 0x00034C71
		public _IExpression _OrgExpression { get; set; }

		// Token: 0x17000563 RID: 1379
		// (get) Token: 0x0600134A RID: 4938 RVA: 0x00035C7A File Offset: 0x00034C7A
		// (set) Token: 0x0600134B RID: 4939 RVA: 0x00035C82 File Offset: 0x00034C82
		public Guid ApplicationGuid { get; set; }

		// Token: 0x17000564 RID: 1380
		// (get) Token: 0x0600134C RID: 4940 RVA: 0x00035C8B File Offset: 0x00034C8B
		// (set) Token: 0x0600134D RID: 4941 RVA: 0x00035C93 File Offset: 0x00034C93
		public _IExpression _WatchExpression { get; set; }

		// Token: 0x17000565 RID: 1381
		// (get) Token: 0x0600134E RID: 4942 RVA: 0x00035C9C File Offset: 0x00034C9C
		public IExpression WatchExpression
		{
			get
			{
				return this._WatchExpression;
			}
		}

		// Token: 0x17000566 RID: 1382
		// (get) Token: 0x0600134F RID: 4943 RVA: 0x00035CA4 File Offset: 0x00034CA4
		// (set) Token: 0x06001350 RID: 4944 RVA: 0x00035CB6 File Offset: 0x00034CB6
		public IExpression InstancePathExpression
		{
			get
			{
				if (this.m_expInstPath != null)
				{
					return this.m_expInstPath;
				}
				return null;
			}
			set
			{
				this.m_expInstPath = (value as _IExpression);
			}
		}

		// Token: 0x17000567 RID: 1383
		// (get) Token: 0x06001351 RID: 4945 RVA: 0x00035CC4 File Offset: 0x00034CC4
		// (set) Token: 0x06001352 RID: 4946 RVA: 0x00035CCC File Offset: 0x00034CCC
		public IAddressInfo AddressInfo { get; set; }

		// Token: 0x17000568 RID: 1384
		// (get) Token: 0x06001353 RID: 4947 RVA: 0x00035CD5 File Offset: 0x00034CD5
		// (set) Token: 0x06001354 RID: 4948 RVA: 0x00035CDD File Offset: 0x00034CDD
		public int SignatureId { get; set; }

		// Token: 0x17000569 RID: 1385
		// (get) Token: 0x06001355 RID: 4949 RVA: 0x00035CE6 File Offset: 0x00034CE6
		internal int DeclaringSignatureId { get; }

		// Token: 0x06001356 RID: 4950 RVA: 0x00035CEE File Offset: 0x00034CEE
		public bool GetFlag(VarRefFlag vrflag)
		{
			return (this.m_flags & vrflag) == vrflag;
		}

		// Token: 0x06001357 RID: 4951 RVA: 0x00035CFB File Offset: 0x00034CFB
		public void SetFlag(VarRefFlag vrFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.m_flags |= vrFlag;
				return;
			}
			this.m_flags &= ~vrFlag;
		}

		// Token: 0x1700056A RID: 1386
		// (get) Token: 0x06001358 RID: 4952 RVA: 0x00035D1E File Offset: 0x00034D1E
		// (set) Token: 0x06001359 RID: 4953 RVA: 0x00035D26 File Offset: 0x00034D26
		public ISourcePosition Position { get; set; }

		// Token: 0x0600135A RID: 4954 RVA: 0x00035D30 File Offset: 0x00034D30
		public override bool Equals(object obj)
		{
			if (obj != null && base.GetType() != obj.GetType())
			{
				return false;
			}
			VarRef varRef = obj as VarRef;
			if (varRef == null)
			{
				return false;
			}
			if (this.ConstantValue != null)
			{
				return object.Equals(this.ConstantValue, varRef.ConstantValue);
			}
			if (varRef.AddressInfo == null)
			{
				return false;
			}
			if (!varRef.AddressInfo.Equals(this.AddressInfo))
			{
				return false;
			}
			if (varRef.WatchExpression == null)
			{
				return false;
			}
			if (varRef.WatchExpression.Type == null)
			{
				return false;
			}
			if (varRef.ApplicationGuid != this.ApplicationGuid)
			{
				return false;
			}
			if (this.AddressInfo is IAddressInfo4 && (this.AddressInfo as IAddressInfo4).ContainsStackRelativeAddress)
			{
				if (varRef.InstancePathExpression != this.InstancePathExpression)
				{
					return false;
				}
				if (varRef.InstancePathExpression != null && !varRef.InstancePathExpression.Equals(this.InstancePathExpression))
				{
					return false;
				}
			}
			return varRef.WatchExpression.Type.IsEqual(this.WatchExpression.Type) && this.DeclaringSignatureId == varRef.DeclaringSignatureId;
		}

		// Token: 0x0600135B RID: 4955 RVA: 0x00035E44 File Offset: 0x00034E44
		public override int GetHashCode()
		{
			int num;
			if (this.ConstantValue != null)
			{
				num = this.ConstantValue.GetHashCode();
			}
			else
			{
				num = (this.ApplicationGuid.GetHashCode() ^ this.DeclaringSignatureId.GetHashCode());
				if (this.AddressInfo != null)
				{
					num ^= this.AddressInfo.GetHashCode();
				}
			}
			if (this.WatchExpression != null && this.WatchExpression.Type != null)
			{
				num ^= this.WatchExpression.Type.ToString().GetHashCode();
			}
			if (this.AddressInfo is IAddressInfo4 && (this.AddressInfo as IAddressInfo4).ContainsStackRelativeAddress)
			{
				if (this.InstancePathExpression != null)
				{
					num ^= this.InstancePathExpression.ToString().GetHashCode();
				}
				else if (this.WatchExpression != null)
				{
					num ^= this.WatchExpression.ToString().GetHashCode();
				}
			}
			return num ^ base.GetType().GetHashCode();
		}

		// Token: 0x0600135C RID: 4956 RVA: 0x00035F34 File Offset: 0x00034F34
		public void SetConstantValue(ILiteralValue litval)
		{
			if (litval != null)
			{
				bool flag = false;
				switch (litval.KindOf)
				{
				case KindOfLiteral.SignedInteger:
					this.ConstantValue = litval.GetSignedLong(out flag);
					break;
				case KindOfLiteral.UnsignedInteger:
					this.ConstantValue = litval.GetUnsignedLong(out flag);
					break;
				case KindOfLiteral.Float:
					this.ConstantValue = litval.GetFloat(out flag);
					break;
				case KindOfLiteral.String:
					this.ConstantValue = litval.GetString(out flag);
					break;
				case KindOfLiteral.Bool:
					this.ConstantValue = litval.GetBoolV(out flag);
					break;
				}
				if (!flag)
				{
					this.ConstantValue = null;
				}
			}
		}

		// Token: 0x1700056B RID: 1387
		// (get) Token: 0x0600135D RID: 4957 RVA: 0x00035FD8 File Offset: 0x00034FD8
		// (set) Token: 0x0600135E RID: 4958 RVA: 0x00035FE0 File Offset: 0x00034FE0
		public object ConstantValue { get; set; }

		// Token: 0x1700056C RID: 1388
		// (get) Token: 0x0600135F RID: 4959 RVA: 0x00035FE9 File Offset: 0x00034FE9
		// (set) Token: 0x06001360 RID: 4960 RVA: 0x00035FF1 File Offset: 0x00034FF1
		public string DisplayExpression { get; set; }

		// Token: 0x0400046C RID: 1132
		protected _IExpression m_expInstPath;

		// Token: 0x0400046D RID: 1133
		protected VarRefFlag m_flags;
	}
}
