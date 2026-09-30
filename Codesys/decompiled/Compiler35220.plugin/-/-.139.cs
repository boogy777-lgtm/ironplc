using System;
using System.Collections;
using System.Runtime.CompilerServices;
using \u0013;
using \u0015;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace \u0003
{
	// Token: 0x0200018C RID: 396
	internal sealed class \u0007 : \u0013.\u0001, IExprementVisitor352000, IExprementVisitor351900, IExprementVisitor351800, IExprementVisitor351500, IExprementVisitor351400, IExprementVisitor351300, IExprementVisitor3590, IExprementVisitor
	{
		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x06001BC2 RID: 7106 RVA: 0x0005B4C0 File Offset: 0x000596C0
		private Hashtable DefineTable { get; }

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x06001BC3 RID: 7107 RVA: 0x0005B4C8 File Offset: 0x000596C8
		private \u0015.\u0002 Scope { get; }

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x06001BC4 RID: 7108 RVA: 0x0005B4D0 File Offset: 0x000596D0
		private _IPreCompileContext PreCompileContext { get; }

		// Token: 0x06001BC5 RID: 7109 RVA: 0x0005B4D8 File Offset: 0x000596D8
		private \u0007(\u0080.\u000F \u001B\u0004, Hashtable \u007F\u0005, _IPreCompileContext \u0080\u0005, \u0015.\u0002 \u009B\u0002) : base(\u001B\u0004)
		{
			this.DefineTable = \u007F\u0005;
			this.Scope = \u009B\u0002;
			this.PreCompileContext = \u0080\u0005;
		}

		// Token: 0x06001BC6 RID: 7110 RVA: 0x0005B4F8 File Offset: 0x000596F8
		public static void \u0001(_IExprement \u0002, Hashtable \u0003, _IPreCompileContext \u0004, \u0015.\u0002 \u0005)
		{
			global::\u0003.\u0007 ivisit = new global::\u0003.\u0007(new \u0080.\u000F(), \u0003, \u0004, \u0005);
			\u0002.Accept(ivisit);
		}

		// Token: 0x06001BC7 RID: 7111 RVA: 0x0005B51C File Offset: 0x0005971C
		public override void visit(_IVariableReference varref)
		{
			if (varref.InstancePath != null)
			{
				IVariable variable = varref.GetVariable(this.Scope);
				varref.Value = (variable != null);
			}
		}

		// Token: 0x06001BC8 RID: 7112 RVA: 0x0005B548 File Offset: 0x00059748
		public override void visit(_ITypeReference typeref)
		{
			ISignature[] array = this.Scope.FindSignature(typeref.InstancePath);
			ISignature signature = null;
			if (array != null && array.Length != 0)
			{
				signature = array[0];
			}
			if (signature != null && signature.POUType == Operator.Type)
			{
				typeref.Value = true;
				return;
			}
			if (signature != null && signature.GetFlag(SignatureFlag.Enum))
			{
				typeref.Value = true;
			}
		}

		// Token: 0x06001BC9 RID: 7113 RVA: 0x0005B5A0 File Offset: 0x000597A0
		public override void visit(_IPouReference pouref)
		{
			pouref.Value = false;
			_ISignature isignature = this.\u0001(pouref.InstancePath);
			if (isignature != null && (isignature.POUType == Operator.Program || isignature.POUType == Operator.Function || isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Method || isignature.POUType == Operator.Action || Operator.Interface == isignature.POUType))
			{
				pouref.Value = true;
			}
		}

		// Token: 0x06001BCA RID: 7114 RVA: 0x0005B608 File Offset: 0x00059808
		public override void visit(_ITaskReference taskref)
		{
			taskref.Value = false;
			for (int i = 0; i < this.PreCompileContext.TaskList.Count; i++)
			{
				if (string.Equals(this.PreCompileContext.TaskList[i].TaskName, taskref.TaskName, StringComparison.OrdinalIgnoreCase))
				{
					taskref.Value = true;
					return;
				}
			}
		}

		// Token: 0x06001BCB RID: 7115 RVA: 0x0005B664 File Offset: 0x00059864
		public override void visit(_IResourceReference resref)
		{
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(APEnvironmentFacade.Instance.ActiveApplicationGuid);
			string deviceName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceName(deviceOfApplication);
			resref.Value = string.Equals(deviceName, resref.ResourceName, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001BCC RID: 7116 RVA: 0x0005B6BC File Offset: 0x000598BC
		public override void visit(_IDefineReference defref)
		{
			defref.Value = this.DefineTable.Contains(defref.Define);
		}

		// Token: 0x06001BCD RID: 7117 RVA: 0x0005B6D8 File Offset: 0x000598D8
		public override void visit(_IDefinedExpression defexp)
		{
			defexp.ItemReference.Accept(this);
			_IPragmaExpression ipragmaExpression = defexp.ItemReference as _IPragmaExpression;
			if (ipragmaExpression != null)
			{
				defexp.Value = ipragmaExpression.Value;
			}
		}

		// Token: 0x06001BCE RID: 7118 RVA: 0x0005B70C File Offset: 0x0005990C
		public override void visit(_IHasAttributeExpression hasattribute)
		{
			_IItemReference iitemReference = hasattribute.ItemReference as _IItemReference;
			hasattribute.Value = false;
			if (iitemReference != null)
			{
				iitemReference.Accept(this);
				if (iitemReference.Value && iitemReference.HasAttribute(hasattribute.Attribute, this.Scope))
				{
					hasattribute.Value = true;
				}
			}
		}

		// Token: 0x06001BCF RID: 7119 RVA: 0x0005B75C File Offset: 0x0005995C
		public override void visit(_IHasValueExpression hasvalue)
		{
			hasvalue.Value = this.\u0001(hasvalue.Define, hasvalue.DefineValue);
		}

		// Token: 0x06001BD0 RID: 7120 RVA: 0x0005B778 File Offset: 0x00059978
		public override void visit(_IHasConstantValueExpression hasvalue)
		{
			_IHasConstantValueExpression2 ihasConstantValueExpression = hasvalue as _IHasConstantValueExpression2;
			ILiteralValue literalValue = this.\u0001(ihasConstantValueExpression);
			ILiteralValue literalValue2 = this.\u0002(ihasConstantValueExpression);
			bool value = false;
			if (literalValue != null && literalValue2 != null)
			{
				if (literalValue2.KindOf == literalValue.KindOf)
				{
					value = PragmaEvaluationHelper.MatchesWhereKindOfMatch(ihasConstantValueExpression, literalValue, literalValue2);
				}
				else
				{
					value = PragmaEvaluationHelper.MatchesWhereKindOfDontMatch(ihasConstantValueExpression, literalValue, literalValue2);
				}
			}
			if (ihasConstantValueExpression.Constant != null && literalValue == null)
			{
				ihasConstantValueExpression.ValueStillUndecided = true;
			}
			ihasConstantValueExpression.Value = value;
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x0005B7E0 File Offset: 0x000599E0
		private ILiteralValue \u0001(_IHasConstantValueExpression2 \u0002)
		{
			ILiteralValue result = null;
			if (\u0002.Constant != null)
			{
				result = ((_IExpression)\u0002.Constant).Literal(this.Scope);
			}
			return result;
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x0005B810 File Offset: 0x00059A10
		public override void visit(_ICompilerVersionExpression compversion)
		{
			this.\u0001(compversion, compversion.OpComparison, APEnvironmentFacade.Instance.CompilerVersionToUseInternal(), compversion.VersionToTest);
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x0005B830 File Offset: 0x00059A30
		protected void \u0001(_IPragmaExpression \u0002, Operator \u0003, Version \u0004, Version \u0005)
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

		// Token: 0x06001BD4 RID: 7124 RVA: 0x0005B8E0 File Offset: 0x00059AE0
		public void \u0001(_IProjectDefinedExpression \u0002)
		{
			if (\u0002.DefineReference != null)
			{
				\u0002.Value = ProjectDefines.IsInProjectDefined(\u0002.DefineReference.Define);
			}
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x0005B900 File Offset: 0x00059B00
		private ILiteralValue \u0002(_IHasConstantValueExpression2 \u0002)
		{
			ILiteralValue literalValue = null;
			if (\u0002.ConstantValue != null)
			{
				literalValue = \u0002.ConstantValue.LiteralValue;
			}
			else if (\u0002._ConstantValue != null)
			{
				bool flag;
				literalValue = \u0002._ConstantValue.LiteralWithRecursionCheck(this.Scope, new LDictionary<IVariable, IVariable>(), true, out flag);
				if (literalValue == null)
				{
					IVariable variable = \u0002._ConstantValue.GetVariable(this.Scope);
					if (variable != null && variable.Initial != null && variable.Initial is ILiteralExpression)
					{
						literalValue = (variable.Initial as ILiteralExpression).LiteralValue;
					}
				}
			}
			return literalValue;
		}

		// Token: 0x06001BD6 RID: 7126 RVA: 0x0005B988 File Offset: 0x00059B88
		public override void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			_IVariable ivariable = hasConstantTypeExpression._Constant.GetVariable(this.Scope) as _IVariable;
			hasConstantTypeExpression.Value = (ivariable != null && ivariable.GetFlag(VarFlag.ReplacedConstant) == hasConstantTypeExpression._ConstantTypeReplaced);
		}

		// Token: 0x06001BD7 RID: 7127 RVA: 0x0005B9CC File Offset: 0x00059BCC
		public override void visit(_IHasTypeExpression hastype)
		{
			if (hastype.Variable != null)
			{
				hastype.Variable.Accept(this);
			}
			if ((hastype.Variable as _IVariableReference).Value)
			{
				hastype.Value = false;
				IVariable variable = hastype.Variable.GetVariable(this.Scope);
				if (hastype.ReferencedType != null)
				{
					TypeClass? typeClass = (variable != null) ? new TypeClass?(variable.Type.Class) : null;
					TypeClass @class = hastype.ReferencedType.Class;
					if (typeClass.GetValueOrDefault() == @class & typeClass != null)
					{
						if (variable.Type.Class == TypeClass.Userdef)
						{
							_IUserdefType iuserdefType = variable.Type as _IUserdefType;
							_IUserdefType iuserdefType2 = hastype.ReferencedType as _IUserdefType;
							_ISignature isignature = this.\u0001(iuserdefType2.NameExpression);
							if (isignature != null && iuserdefType.SignatureId >= 0 && iuserdefType.SignatureId == isignature.PrecompileId)
							{
								hastype.Value = true;
								return;
							}
						}
						else
						{
							hastype.Value = true;
						}
					}
				}
			}
		}

		// Token: 0x06001BD8 RID: 7128 RVA: 0x0005BACC File Offset: 0x00059CCC
		public override void visit(_IPragmaOperatorExpression popexp)
		{
			bool flag = false;
			bool flag2 = true;
			bool flag3 = false;
			foreach (_IExpression iexpression in popexp.Operands)
			{
				iexpression.Accept(this);
				if (!flag3)
				{
					_IPragmaExpression ipragmaExpression = iexpression as _IPragmaExpression;
					if (ipragmaExpression == null)
					{
						flag3 = true;
						flag = false;
					}
					else
					{
						if (popexp.Code == PragmaOperator.Not)
						{
							flag = !ipragmaExpression.Value;
							break;
						}
						if (flag2)
						{
							flag = ipragmaExpression.Value;
							flag2 = false;
						}
						else
						{
							flag = global::\u0003.\u0007.\u0001(popexp.Code, flag, ipragmaExpression);
						}
					}
				}
			}
			popexp.Value = flag;
		}

		// Token: 0x06001BD9 RID: 7129 RVA: 0x0005BB74 File Offset: 0x00059D74
		private static bool \u0001(PragmaOperator \u0002, bool \u0003, _IPragmaExpression \u0004)
		{
			if (\u0002 == PragmaOperator.And)
			{
				\u0003 = (\u0004.Value && \u0003);
			}
			else if (\u0002 == PragmaOperator.Or)
			{
				\u0003 = (\u0004.Value || \u0003);
			}
			else
			{
				Debug.\u0001(false);
			}
			return \u0003;
		}

		// Token: 0x06001BDA RID: 7130 RVA: 0x0005BBA0 File Offset: 0x00059DA0
		public void \u0001(_IRuntimeVersionExpression \u0002)
		{
		}

		// Token: 0x06001BDB RID: 7131 RVA: 0x0005BBA4 File Offset: 0x00059DA4
		public void \u0001(_INamespaceAccessExpression \u0002)
		{
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x0005BBA8 File Offset: 0x00059DA8
		public void \u0001(_ICurrentTaskExpression \u0002)
		{
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x0005BBAC File Offset: 0x00059DAC
		public void \u0001(_IPoolScopeExpression \u0002)
		{
		}

		// Token: 0x06001BDE RID: 7134 RVA: 0x0005BBB0 File Offset: 0x00059DB0
		public void \u0001(_IPartialAccessExpression \u0002)
		{
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x0005BBB4 File Offset: 0x00059DB4
		private _ISignature \u0001(IExpression \u0002)
		{
			ISignature[] array = this.Scope.FindSignature(\u0002);
			ISignature signature = null;
			if (array != null && array.Length != 0)
			{
				signature = array[0];
			}
			if (signature == null && \u0002 is ICompoAccessExpression)
			{
				_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)\u0002;
				ISignature[] array2 = this.Scope.FindSignature(icompoAccessExpression.Left);
				if (array2 != null && array2.Length != 0)
				{
					ISignature signature2 = array2[0];
					signature = this.Scope.\u0001(signature2 as _ISignature).FindSignatureLocal(icompoAccessExpression.Right.ToString());
				}
			}
			return signature as _ISignature;
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x0005BC34 File Offset: 0x00059E34
		private bool \u0001(string \u0002, string \u0003)
		{
			return this.DefineTable.ContainsKey(\u0002) && (string)this.DefineTable[\u0002] == \u0003;
		}

		// Token: 0x040004C2 RID: 1218
		[CompilerGenerated]
		private new readonly Hashtable \u0001;

		// Token: 0x040004C3 RID: 1219
		[CompilerGenerated]
		private new readonly \u0015.\u0002 \u0001;

		// Token: 0x040004C4 RID: 1220
		[CompilerGenerated]
		private new readonly _IPreCompileContext \u0001;
	}
}
