using System;
using System.Linq;
using \u0002;
using \u0006;
using \u000E;
using \u0019;
using \u001C;
using \u001E;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Services.AttributeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000243 RID: 579
	internal sealed class OptionalInputsProvider : EmptyVisitor352000, IReplacer
	{
		// Token: 0x17000702 RID: 1794
		// (get) Token: 0x0600263F RID: 9791 RVA: 0x00085274 File Offset: 0x00083474
		private _ICompileContext _comcon
		{
			get
			{
				return this.\u0001.Comcon;
			}
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x00085290 File Offset: 0x00083490
		public OptionalInputsProvider(global::\u000E.\u0011 context)
		{
			this.\u0001 = context;
			this.\u0001 = new global::\u0002.\u0006(this);
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x000852AC File Offset: 0x000834AC
		public static void \u0001(global::\u000E.\u0011 \u0002, _IExpression \u0003)
		{
			OptionalInputsProvider optionalInputsProvider = new OptionalInputsProvider(\u0002)
			{
				\u0001 = \u0002.CompiledPOU
			};
			_ICallExpression icallExpression = \u0003 as _ICallExpression;
			if (icallExpression != null)
			{
				optionalInputsProvider.visit(icallExpression);
				return;
			}
			_IConversionExpression iconversionExpression = \u0003 as _IConversionExpression;
			if (iconversionExpression != null)
			{
				_ICallExpression icallExpression2 = iconversionExpression.Exp as _ICallExpression;
				if (icallExpression2 != null)
				{
					optionalInputsProvider.visit(icallExpression2);
				}
			}
		}

		// Token: 0x06002642 RID: 9794 RVA: 0x00085300 File Offset: 0x00083500
		public void ReplaceCode(_ICompiledPOU cpou)
		{
			this.visit(cpou);
		}

		// Token: 0x17000703 RID: 1795
		// (get) Token: 0x06002643 RID: 9795 RVA: 0x0008530C File Offset: 0x0008350C
		private IScope5 _Scope
		{
			get
			{
				return this.\u0001._Scope;
			}
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x00085328 File Offset: 0x00083528
		public override void visit(_ICompiledPOU cpou)
		{
			_ISignature isignature = this._Scope[cpou.SignatureId] as _ISignature;
			if (isignature == null || isignature.GetFlagInternal(SignatureFlagInternal.ContainsCallWithOmittedOptionalInput) || cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsCallWithOmittedOptionalInput))
			{
				this.\u0001.\u0001();
				this.\u0001 = cpou;
				((_IExprement)cpou.ParseTree).Accept(this.\u0001);
			}
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x00085394 File Offset: 0x00083594
		public override void visit(_ICallExpression call)
		{
			_IUserdefType iuserdefType = call.Callee.Type as _IUserdefType;
			_ISignature isignature = ((iuserdefType != null) ? iuserdefType.GetSignature(this._Scope) : null) as _ISignature;
			if (isignature == null || !isignature.GetFlagInternal(SignatureFlagInternal.OptionalInputs))
			{
				return;
			}
			ICaseInsensitiveDictionary<OptionalInputsProvider.\u0001> caseInsensitiveDictionary = this.\u0001(call, isignature);
			foreach (IVariable variable in isignature.AllInputs)
			{
				OptionalInputsProvider.\u0001 u;
				if (caseInsensitiveDictionary.TryGetValue(variable.Name, out u))
				{
					ICompiledType compiledType = variable.CompiledType;
					ISubrangeType2 subrangeType = compiledType as ISubrangeType2;
					if (subrangeType != null)
					{
						compiledType = subrangeType.BaseType;
					}
					_IExpression iexpression = this.\u0001(call, u, compiledType);
					_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001((_IVariable)variable, isignature);
					_IAssignmentExpression iassignmentExpression = global::\u0019.\u0003.\u0001(ivariableExpression, iexpression);
					if (TypeClass.Reference == variable.CompiledType.Class)
					{
						iassignmentExpression.KindOf = Operator.RefAssign;
					}
					global::\u0002.\u0006.\u0001(iassignmentExpression, this._Scope, this._comcon, this.\u0001);
					call.AddParam(iexpression, ivariableExpression);
				}
			}
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x000854A0 File Offset: 0x000836A0
		private ICaseInsensitiveDictionary<OptionalInputsProvider.\u0001> \u0001(_ICallExpression \u0002, ISignature \u0003)
		{
			CaseInsensitiveDictionary<OptionalInputsProvider.\u0001> caseInsensitiveDictionary = new CaseInsensitiveDictionary<OptionalInputsProvider.\u0001>();
			foreach (IVariable variable in \u0003.AllInputs)
			{
				if (!variable.GetFlag(VarFlag.Implicit))
				{
					OptionalInputsProvider.\u0001 u = OptionalInputsProvider.\u0001(\u0003, variable);
					if (u != null)
					{
						caseInsensitiveDictionary[variable.Name] = u;
					}
					else if (variable.Initial != null)
					{
						caseInsensitiveDictionary[variable.Name] = new OptionalInputsProvider.\u0001
						{
							\u0001 = OptionalInputsProvider.OptionalInputKind.Literal,
							\u0001 = variable.Initial
						};
					}
				}
			}
			foreach (_IAssignmentExpression iassignmentExpression in \u0002.InputAssigns.OfType<_IAssignmentExpression>())
			{
				_IVariableExpression ivariableExpression = iassignmentExpression.LValue as _IVariableExpression;
				if (ivariableExpression != null && ivariableExpression.Name != null)
				{
					caseInsensitiveDictionary.Remove(ivariableExpression.Name);
				}
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x06002647 RID: 9799 RVA: 0x00085590 File Offset: 0x00083790
		private static OptionalInputsProvider.\u0001 \u0001(ISignature \u0002, IVariable \u0003)
		{
			string attributeValue = \u0003.GetAttributeValue("implicit-parameter");
			OptionalInputsProvider.OptionalInputKind u;
			if (attributeValue != null && OptionalInputsProvider.\u0001.TryGetValue(attributeValue, ref u) && LMMAttributeProvider.CheckValidUsageOfImplicitParameter(attributeValue, \u0002, \u0003) == null)
			{
				return new OptionalInputsProvider.\u0001
				{
					\u0001 = u,
					\u0001 = \u0003.Initial,
					\u0001 = (\u0003.Type.Class == TypeClass.Pointer)
				};
			}
			return null;
		}

		// Token: 0x06002648 RID: 9800 RVA: 0x000855F4 File Offset: 0x000837F4
		private _ILiteralExpression \u0001(ILiteralValue \u0002, TypeClass \u0003)
		{
			KindOfLiteral kindOf = \u0002.KindOf;
			if (kindOf == KindOfLiteral.SignedInteger)
			{
				return global::\u0019.\u0003.\u0001(\u0002.SignedLong, \u0003);
			}
			if (kindOf == KindOfLiteral.UnsignedInteger)
			{
				return global::\u0019.\u0003.\u0001(\u0002.UnsignedLong, \u0003);
			}
			Debug.\u0001(false, "Unexpected KindOfLiteral value");
			return null;
		}

		// Token: 0x06002649 RID: 9801 RVA: 0x00085638 File Offset: 0x00083838
		private _IExpression \u0001(_ICallExpression \u0002, OptionalInputsProvider.\u0001 \u0003, ICompiledType \u0004)
		{
			if (\u0003.\u0001 == OptionalInputsProvider.OptionalInputKind.Literal)
			{
				ILiteralValue u = \u0003.\u0001.Literal(this._Scope);
				return this.\u0001(u, \u0004);
			}
			_IExpression iexpression;
			if (\u0003.\u0001 == OptionalInputsProvider.OptionalInputKind.PouName)
			{
				iexpression = this.\u0002(\u0003, \u0004);
			}
			else if (\u0003.\u0001 == OptionalInputsProvider.OptionalInputKind.Position)
			{
				iexpression = this.\u0002(\u0002, \u0003, \u0004);
			}
			else
			{
				if (\u0003.\u0001 != OptionalInputsProvider.OptionalInputKind.InstancePath)
				{
					throw new NotImplementedException(string.Format("Unknown optional input kind {0}", \u0003.\u0001));
				}
				iexpression = this.\u0001(\u0003, \u0004);
			}
			if (iexpression == null)
			{
				ILiteralValue u2 = \u0003.\u0001.Literal(this._Scope);
				return this.\u0001(u2, \u0004);
			}
			if (\u0003.\u0001)
			{
				_IOperatorExpression ioperatorExpression = global::\u0019.\u0003.\u0001(Operator.Adr, iexpression);
				ioperatorExpression.Type = \u0004;
				return ioperatorExpression;
			}
			return iexpression;
		}

		// Token: 0x0600264A RID: 9802 RVA: 0x000856F8 File Offset: 0x000838F8
		private _IExpression \u0001(OptionalInputsProvider.\u0001 \u0002, ICompiledType \u0003)
		{
			_ISignature isignature = (this._Scope.MethodSignature ?? this._Scope.LocalSignature) as _ISignature;
			isignature = (_ISignature)this.\u0001(isignature);
			string text = this.\u0001(isignature);
			if (text != null)
			{
				_ILiteralValue u = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetApplicationNameByGuid(this._comcon.ApplicationGuid, this._comcon.SimulationMode) + "." + text);
				return this.\u0001(u, \u0002.\u0001 ? \u0003.BaseType : \u0003);
			}
			_IVariable ivariable = OptionalInputsProvider.\u0001(isignature);
			_ISignature u2 = isignature;
			bool flag = false;
			if (ivariable == null && isignature != null)
			{
				flag = true;
				this.\u0001(isignature, out u2, out ivariable);
			}
			if (ivariable == null)
			{
				return null;
			}
			_IVariableExpression ivariableExpression = global::\u0019.\u0003.\u0001(ivariable, u2);
			_IExpression u3;
			if (flag)
			{
				u3 = global::\u0019.\u0003.\u0001(global::\u0019.\u0003.\u0001(\u001E.\u0011.\u0001(this._Scope)), ivariableExpression);
			}
			else
			{
				u3 = ivariableExpression;
			}
			return this.\u0001.Generator.\u0001<_IExpression>(u3, this._Scope, this.\u0001);
		}

		// Token: 0x0600264B RID: 9803 RVA: 0x0008580C File Offset: 0x00083A0C
		private void \u0001(_ISignature \u0002, out _ISignature \u0003, out _IVariable \u0004)
		{
			_ISignature isignature = this._Scope[\u0002.ParentSignatureId] as _ISignature;
			\u0004 = OptionalInputsProvider.\u0001(isignature);
			\u0003 = isignature;
			while (\u0004 == null && \u0003 != null && Helper.InvalidId != \u0003.BaseSignatureId)
			{
				_ISignature isignature2 = this._Scope[\u0003.BaseSignatureId] as _ISignature;
				\u0004 = OptionalInputsProvider.\u0001(isignature2);
				\u0003 = isignature2;
			}
		}

		// Token: 0x0600264C RID: 9804 RVA: 0x00085878 File Offset: 0x00083A78
		private static _IVariable \u0001(_ISignature \u0002)
		{
			if (\u0002 == null)
			{
				return null;
			}
			return \u0002.AllVariables.FirstOrDefault(new Func<_IVariable, bool>(OptionalInputsProvider.<>c.<>9.\u0001));
		}

		// Token: 0x0600264D RID: 9805 RVA: 0x000858AC File Offset: 0x00083AAC
		private string \u0001(_ISignature \u0002)
		{
			_ISignature isignature = (this._Scope[\u0002.ParentSignatureId] as _ISignature) ?? \u0002;
			string text;
			if (!\u001C.\u0003.\u0001(OptionalInputsProvider.\u0001, this._comcon, isignature, out text))
			{
				return null;
			}
			if (isignature == \u0002)
			{
				return text;
			}
			string text2 = global::\u0006.\u0001.\u0001(\u0002);
			if (!string.IsNullOrEmpty(text2))
			{
				return text + "." + text2;
			}
			return text;
		}

		// Token: 0x0600264E RID: 9806 RVA: 0x00085910 File Offset: 0x00083B10
		private _IExpression \u0002(_ICallExpression \u0002, OptionalInputsProvider.\u0001 \u0003, ICompiledType \u0004)
		{
			ISignature signature = this._Scope.MethodSignature ?? this._Scope.LocalSignature;
			int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath);
			signature = this.\u0001(signature);
			Guid objectGuid = signature.ObjectGuid;
			IPositionTextProvider positionTextProvider = APEnvironmentFacade.Instance.GetPositionTextProvider(projectHandle, objectGuid);
			string text = (positionTextProvider != null) ? positionTextProvider.GetPositionText(\u0002.Position.Position) : null;
			if (text == null)
			{
				return null;
			}
			_ILiteralValue u = global::\u0019.\u0003.\u0001(text);
			return this.\u0001(u, \u0003.\u0001 ? \u0004.BaseType : \u0004);
		}

		// Token: 0x0600264F RID: 9807 RVA: 0x000859AC File Offset: 0x00083BAC
		private _IExpression \u0002(OptionalInputsProvider.\u0001 \u0002, ICompiledType \u0003)
		{
			_ISignature isignature = (_ISignature)(this._Scope.MethodSignature ?? this._Scope.LocalSignature);
			string text = global::\u0006.\u0001.\u0001(this._Scope, isignature);
			text = this.\u0001(isignature, text);
			_ILiteralValue u = global::\u0019.\u0003.\u0001(text);
			return this.\u0001(u, \u0002.\u0001 ? \u0003.BaseType : \u0003);
		}

		// Token: 0x06002650 RID: 9808 RVA: 0x00085A10 File Offset: 0x00083C10
		private string \u0001(_ISignature \u0002, string \u0003)
		{
			if (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INIT_FUN))
			{
				return \u0003;
			}
			_ISignature isignature = this.\u0001(\u0002) as _ISignature;
			if (isignature == null)
			{
				return \u0003;
			}
			return global::\u0006.\u0001.\u0001(this._Scope, isignature);
		}

		// Token: 0x06002651 RID: 9809 RVA: 0x00085A4C File Offset: 0x00083C4C
		private ISignature \u0001(ISignature \u0002)
		{
			if (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_IMPLICIT_INIT_FUN))
			{
				return \u0002;
			}
			string[] array = \u0002.Name.Split(new string[]
			{
				"__"
			}, StringSplitOptions.RemoveEmptyEntries);
			int nId;
			if (4 != array.Length || !int.TryParse(array[1], out nId))
			{
				return \u0002;
			}
			_ISignature isignature = this._comcon.GetSignatureById(nId) as _ISignature;
			if (isignature == null)
			{
				return \u0002;
			}
			return isignature;
		}

		// Token: 0x06002652 RID: 9810 RVA: 0x00085AB0 File Offset: 0x00083CB0
		private _ILiteralExpression \u0001(ILiteralValue \u0002, ICompiledType \u0003)
		{
			TypeClass typeClass = \u0003.Class;
			if (typeClass == TypeClass.Enum)
			{
				typeClass = ((_IEnumType)\u0003)._Base.Class;
			}
			if (typeClass == TypeClass.Userdef && this.\u0001(\u0003))
			{
				typeClass = TypeClass.Reference;
			}
			_ILiteralExpression iliteralExpression;
			switch (typeClass)
			{
			case TypeClass.Bool:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.Bool);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				return iliteralExpression;
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
			case TypeClass.UXInt:
			case TypeClass.XWord:
				return this.\u0001(\u0002, typeClass);
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.XInt:
				return this.\u0001(\u0002, typeClass);
			case TypeClass.Real:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.Float);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				iliteralExpression.ConstantType = TypeClass.Real;
				return iliteralExpression;
			case TypeClass.LReal:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.Float);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				iliteralExpression.ConstantType = TypeClass.LReal;
				return iliteralExpression;
			case TypeClass.String:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.String);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				iliteralExpression.ConstantType = TypeClass.String;
				return iliteralExpression;
			case TypeClass.WString:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.String);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				iliteralExpression.ConstantType = TypeClass.WString;
				return iliteralExpression;
			case TypeClass.Time:
			case TypeClass.Date:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
				return this.\u0001(\u0002, typeClass);
			case TypeClass.Pointer:
				iliteralExpression = this.\u0001(\u0002, TypeClass.XWord);
				iliteralExpression.Type = \u0003;
				return iliteralExpression;
			case TypeClass.Reference:
			{
				TypeClass typeClass2;
				ICompiledType type;
				if (8 == this._comcon.PointerSize)
				{
					typeClass2 = TypeClass.LWord;
					type = global::\u0019.\u0003.\u0001();
				}
				else
				{
					typeClass2 = TypeClass.DWord;
					type = global::\u0019.\u0003.\u0001();
				}
				iliteralExpression = this.\u0001(\u0002, typeClass2);
				iliteralExpression.ConstantType = typeClass2;
				iliteralExpression.Type = type;
				return iliteralExpression;
			}
			case TypeClass.LTime:
			case TypeClass.LDate:
			case TypeClass.LDateAndTime:
			case TypeClass.LTimeOfDay:
				return this.\u0001(\u0002, typeClass);
			case TypeClass.XString:
				iliteralExpression = global::\u0019.\u0003.\u0001(\u0002.String);
				iliteralExpression.Type = global::\u0019.\u0003.\u0001();
				iliteralExpression.ConstantType = TypeClass.XString;
				return iliteralExpression;
			}
			iliteralExpression = null;
			return iliteralExpression;
		}

		// Token: 0x06002653 RID: 9811 RVA: 0x00085D08 File Offset: 0x00083F08
		private bool \u0001(ICompiledType \u0002)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature = this._Scope[iuserdefType.SignatureId] as _ISignature;
				if (isignature != null && isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002654 RID: 9812 RVA: 0x00085D4C File Offset: 0x00083F4C
		// Note: this type is marked as 'beforefieldinit'.
		static OptionalInputsProvider()
		{
			CaseInsensitiveDictionary<OptionalInputsProvider.OptionalInputKind> caseInsensitiveDictionary = new CaseInsensitiveDictionary<OptionalInputsProvider.OptionalInputKind>();
			caseInsensitiveDictionary["position"] = OptionalInputsProvider.OptionalInputKind.Position;
			caseInsensitiveDictionary["pouname"] = OptionalInputsProvider.OptionalInputKind.PouName;
			caseInsensitiveDictionary["instance-path"] = OptionalInputsProvider.OptionalInputKind.InstancePath;
			OptionalInputsProvider.\u0001 = caseInsensitiveDictionary;
			OptionalInputsProvider.\u0001 = new \u0080.\u0005.\u0001
			{
				\u0001 = true,
				\u0006 = false
			};
		}

		// Token: 0x040006F3 RID: 1779
		private _ICompiledPOU \u0001;

		// Token: 0x040006F4 RID: 1780
		private readonly global::\u0002.\u0006 \u0001;

		// Token: 0x040006F5 RID: 1781
		private readonly global::\u000E.\u0011 \u0001;

		// Token: 0x040006F6 RID: 1782
		private static readonly CaseInsensitiveDictionary<OptionalInputsProvider.OptionalInputKind> \u0001;

		// Token: 0x040006F7 RID: 1783
		private static readonly \u0080.\u0005.\u0001 \u0001;

		// Token: 0x02000244 RID: 580
		private enum OptionalInputKind
		{
			// Token: 0x040006F9 RID: 1785
			Literal,
			// Token: 0x040006FA RID: 1786
			PouName,
			// Token: 0x040006FB RID: 1787
			Position,
			// Token: 0x040006FC RID: 1788
			InstancePath
		}

		// Token: 0x02000245 RID: 581
		private sealed class \u0001
		{
			// Token: 0x040006FD RID: 1789
			public OptionalInputsProvider.OptionalInputKind \u0001;

			// Token: 0x040006FE RID: 1790
			public IExpression \u0001;

			// Token: 0x040006FF RID: 1791
			public bool \u0001;
		}
	}
}
