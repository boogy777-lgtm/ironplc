using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0083;

namespace \u0082
{
	// Token: 0x020002DE RID: 734
	internal sealed class \u0012 : EmptyVisitor352000, IExprementVisitorNoTraversion, IExprementVisitorNoTraversion352000, IExprementVisitorNoTraversion351900, IExprementVisitorNoTraversion351800, IExprementVisitorNoTraversion351500, IExprementVisitorNoTraversion351400, IExprementVisitorNoTraversion351300, IExprementVisitorNoTraversion3590
	{
		// Token: 0x17000797 RID: 1943
		// (get) Token: 0x06002C00 RID: 11264 RVA: 0x0009A728 File Offset: 0x00098928
		// (set) Token: 0x06002C01 RID: 11265 RVA: 0x0009A730 File Offset: 0x00098930
		private IScope5 Scope { get; set; }

		// Token: 0x17000798 RID: 1944
		// (get) Token: 0x06002C02 RID: 11266 RVA: 0x0009A73C File Offset: 0x0009893C
		// (set) Token: 0x06002C03 RID: 11267 RVA: 0x0009A744 File Offset: 0x00098944
		private _ICompileContext Comcon { get; set; }

		// Token: 0x17000799 RID: 1945
		// (get) Token: 0x06002C04 RID: 11268 RVA: 0x0009A750 File Offset: 0x00098950
		// (set) Token: 0x06002C05 RID: 11269 RVA: 0x0009A758 File Offset: 0x00098958
		private bool Early { get; set; }

		// Token: 0x1700079A RID: 1946
		// (get) Token: 0x06002C06 RID: 11270 RVA: 0x0009A764 File Offset: 0x00098964
		// (set) Token: 0x06002C07 RID: 11271 RVA: 0x0009A76C File Offset: 0x0009896C
		private bool ValueStillUndecided { get; set; }

		// Token: 0x06002C08 RID: 11272 RVA: 0x0009A778 File Offset: 0x00098978
		private \u0012(IScope5 \u009B\u0002, _ICompileContext \u0001\u0002, bool \u0098\u0007)
		{
			this.Scope = \u009B\u0002;
			this.Comcon = \u0001\u0002;
			this.Early = \u0098\u0007;
			this.\u0001 = new \u0083.\u0006(\u009B\u0002);
			this.\u0001 = new \u0014(\u009B\u0002);
		}

		// Token: 0x06002C09 RID: 11273 RVA: 0x0009A7B0 File Offset: 0x000989B0
		public static bool \u0001(_IPragmaExpression \u0002, IScope5 \u0003, _ICompileContext \u0004, bool \u0005)
		{
			\u0012 u = new \u0012(\u0003, \u0004, \u0005);
			StandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.ValueStillUndecided;
		}

		// Token: 0x06002C0A RID: 11274 RVA: 0x0009A7D8 File Offset: 0x000989D8
		public override void visit(_IDefineReference defref)
		{
			defref.Value = this.Scope.IsDefined(defref.Define);
		}

		// Token: 0x06002C0B RID: 11275 RVA: 0x0009A7F4 File Offset: 0x000989F4
		public override void visit(_IVariableReference varref)
		{
			if (varref.InstancePath.Type != null)
			{
				varref.Value = true;
			}
		}

		// Token: 0x06002C0C RID: 11276 RVA: 0x0009A80C File Offset: 0x00098A0C
		public override void visit(_ITypeReference typeref)
		{
			ISignature[] array = this.Scope.FindSignature(typeref.InstancePath);
			ISignature signature = null;
			if (array != null && array.Length != 0)
			{
				signature = array[0];
			}
			if (signature != null && (signature.POUType == Operator.Type || signature.GetFlag(SignatureFlag.Enum)))
			{
				typeref.Value = true;
			}
		}

		// Token: 0x06002C0D RID: 11277 RVA: 0x0009A858 File Offset: 0x00098A58
		public override void visit(_IPouReference pouref)
		{
			ISignature[] array = this.Scope.FindSignature(pouref.InstancePath);
			ISignature signature = null;
			if (array != null && array.Length != 0)
			{
				signature = array[0];
			}
			if (pouref.InstancePath is ICompoAccessExpression && signature == null)
			{
				_ICompoAccessExpression icompoAccessExpression = (_ICompoAccessExpression)pouref.InstancePath;
				ISignature[] array2 = this.Scope.FindSignature(icompoAccessExpression.Left);
				if (array2 != null && array2.Any<ISignature>())
				{
					ISignature sign = array2.First<ISignature>();
					_IScope iscope = this.Scope.CreateLocalScope(sign) as _IScope;
					if (iscope != null)
					{
						signature = iscope.FindSignatureLocal(icompoAccessExpression.Right.ToString());
					}
				}
			}
			if (signature != null && (signature.POUType == Operator.Program || signature.POUType == Operator.Function || signature.POUType == Operator.FunctionBlock || signature.POUType == Operator.Method || signature.POUType == Operator.Action || Operator.Interface == signature.POUType))
			{
				pouref.Value = true;
			}
		}

		// Token: 0x06002C0E RID: 11278 RVA: 0x0009A938 File Offset: 0x00098B38
		public override void visit(_ITaskReference taskref)
		{
			taskref.Value = false;
			for (int i = 0; i < this.Comcon.TaskList.Count; i++)
			{
				if (string.Equals(this.Comcon.TaskList[i].TaskName, taskref.TaskName, StringComparison.OrdinalIgnoreCase))
				{
					taskref.Value = true;
					return;
				}
			}
		}

		// Token: 0x06002C0F RID: 11279 RVA: 0x0009A994 File Offset: 0x00098B94
		public override void visit(_IResourceReference resref)
		{
			Guid deviceOfApplication = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.Comcon.ApplicationGuid);
			string deviceName = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceName(deviceOfApplication);
			resref.Value = string.Equals(deviceName, resref.ResourceName, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002C10 RID: 11280 RVA: 0x0009A9EC File Offset: 0x00098BEC
		public override void visit(_IDefinedExpression defexp)
		{
			_IPragmaExpression ipragmaExpression = defexp.ItemReference as _IPragmaExpression;
			if (ipragmaExpression != null)
			{
				defexp.Value = ipragmaExpression.Value;
			}
		}

		// Token: 0x06002C11 RID: 11281 RVA: 0x0009AA14 File Offset: 0x00098C14
		public override void visit(_IPragmaOperatorExpression popexp)
		{
			bool flag = false;
			bool flag2 = true;
			bool flag3 = false;
			foreach (_IExpression iexpression in popexp.Operands)
			{
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
						else if (popexp.Code == PragmaOperator.And)
						{
							flag = (ipragmaExpression.Value && flag);
						}
						else if (popexp.Code == PragmaOperator.Or)
						{
							flag = (ipragmaExpression.Value || flag);
						}
						else
						{
							Debug.\u0001(false);
						}
					}
				}
			}
			popexp.Value = flag;
		}

		// Token: 0x06002C12 RID: 11282 RVA: 0x0009AADC File Offset: 0x00098CDC
		public override void visit(_ICompilerVersionExpression compversion)
		{
			PragmaEvaluationHelper.visitVersionSupportingPragmaExpression(compversion, compversion.OpComparison, APEnvironmentFacade.Instance.CompilerVersionToUseInternal(), compversion.VersionToTest);
		}

		// Token: 0x06002C13 RID: 11283 RVA: 0x0009AAFC File Offset: 0x00098CFC
		public override void visit(_IProjectDefinedExpression projectDefinedExpression)
		{
			if (projectDefinedExpression.DefineReference != null)
			{
				projectDefinedExpression.Value = ProjectDefines.IsInProjectDefined(projectDefinedExpression.DefineReference.Define);
			}
		}

		// Token: 0x06002C14 RID: 11284 RVA: 0x0009AB1C File Offset: 0x00098D1C
		public override void visit(_IRuntimeVersionExpression runtimeversionexp)
		{
			Version currVersion = Helper.\u0001(this.Comcon.GetTargetSettings());
			PragmaEvaluationHelper.visitVersionSupportingPragmaExpression(runtimeversionexp, runtimeversionexp.OpComparison, currVersion, runtimeversionexp.VersionToTest);
		}

		// Token: 0x06002C15 RID: 11285 RVA: 0x0009AB50 File Offset: 0x00098D50
		public override void visit(_IXRefExpression xref)
		{
			_IPragmaExpression ipragmaExpression = null;
			if (xref.XRef != null)
			{
				xref.XRef.Accept(this);
				ipragmaExpression = (xref.XRef as _IPragmaExpression);
			}
			if (xref.XRefFrom != null)
			{
				xref.XRefFrom.Accept(this);
				ipragmaExpression = (xref.XRef as _IPragmaExpression);
			}
			if (ipragmaExpression != null)
			{
				xref.Value = ipragmaExpression.Value;
			}
		}

		// Token: 0x06002C16 RID: 11286 RVA: 0x0009ABB0 File Offset: 0x00098DB0
		public override void visit(_IHasTypeExpression hastype)
		{
			this.\u0001.\u0001(hastype);
			if (this.Early)
			{
				this.ValueStillUndecided = true;
				hastype.ValueStillUndecided = true;
			}
		}

		// Token: 0x06002C17 RID: 11287 RVA: 0x0009ABD4 File Offset: 0x00098DD4
		public override void visit(_IIsEnumTypeExpression isenumtype)
		{
			isenumtype.Value = false;
			isenumtype.ReferencedType = isenumtype.Type;
			if (isenumtype.ReferencedType != null)
			{
				isenumtype.Value = (isenumtype.ReferencedType.Class == TypeClass.Enum);
			}
		}

		// Token: 0x06002C18 RID: 11288 RVA: 0x0009AC08 File Offset: 0x00098E08
		public override void visit(_IHasAttributeExpression hasattribute)
		{
			_IItemReference iitemReference = hasattribute.ItemReference as _IItemReference;
			hasattribute.Value = false;
			if (iitemReference != null && iitemReference.Value && iitemReference.HasAttribute(hasattribute.Attribute, this.Scope))
			{
				hasattribute.Value = true;
			}
		}

		// Token: 0x06002C19 RID: 11289 RVA: 0x0009AC50 File Offset: 0x00098E50
		public override void visit(_IHasValueExpression hasvalue)
		{
			hasvalue.Value = this.Scope.DefineHasValue(hasvalue.Define, hasvalue.DefineValue);
		}

		// Token: 0x06002C1A RID: 11290 RVA: 0x0009AC70 File Offset: 0x00098E70
		public override void visit(_IHasConstantValueExpression hasvalue)
		{
			_IHasConstantValueExpression2 ihasConstantValueExpression;
			bool value;
			if (this.\u0001.\u0001(hasvalue, out ihasConstantValueExpression, out value) && this.Early)
			{
				this.ValueStillUndecided = true;
				hasvalue.Value = true;
			}
			ihasConstantValueExpression.Value = value;
		}

		// Token: 0x06002C1B RID: 11291 RVA: 0x0009ACAC File Offset: 0x00098EAC
		public override void visit(_IHasConstantTypeExpression hasConstantTypeExpression)
		{
			_IVariable ivariable = hasConstantTypeExpression._Constant.GetVariable(this.Scope) as _IVariable;
			hasConstantTypeExpression.Value = (ivariable != null && ivariable.GetFlag(VarFlag.ReplacedConstant) == hasConstantTypeExpression._ConstantTypeReplaced);
		}

		// Token: 0x0400085B RID: 2139
		private readonly \u0083.\u0006 \u0001;

		// Token: 0x0400085C RID: 2140
		private readonly \u0014 \u0001;

		// Token: 0x0400085D RID: 2141
		[CompilerGenerated]
		private IScope5 \u0001;

		// Token: 0x0400085E RID: 2142
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x0400085F RID: 2143
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000860 RID: 2144
		[CompilerGenerated]
		private bool \u0002;
	}
}
