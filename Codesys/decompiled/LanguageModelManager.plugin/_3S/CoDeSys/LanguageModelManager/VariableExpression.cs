using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000079 RID: 121
	[TypeGuid("{6708bd14-cddc-4e28-b69d-d3183b7c423e}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class VariableExpression : Expression, _IVariableExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, IVariableExpression3, IVariableExpression2, IVariableExpression, IQualifiedNameExpression, IPositionExprement
	{
		// Token: 0x060007B6 RID: 1974 RVA: 0x00013142 File Offset: 0x00012142
		public VariableExpression()
		{
		}

		// Token: 0x060007B7 RID: 1975 RVA: 0x00013155 File Offset: 0x00012155
		public VariableExpression(string stName)
		{
			this.m_stName = stName;
		}

		// Token: 0x060007B8 RID: 1976 RVA: 0x0001316F File Offset: 0x0001216F
		internal VariableExpression(string stName, IToken token) : base(token)
		{
			this.m_stName = stName;
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x0001318C File Offset: 0x0001218C
		internal VariableExpression(_IVariable var, _ISignature sign)
		{
			this.m_stName = var.Name;
			this.VariableId = var.Id;
			this.SignatureId = sign.Id;
			this._CompiledType = var._Type;
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x000131DA File Offset: 0x000121DA
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			this.m_stName = string.Intern(this.m_stName);
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x000131F4 File Offset: 0x000121F4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			_IVariable ivariable = this.GetVariable(scope) as _IVariable;
			if (ivariable == null)
			{
				return true;
			}
			bool flag = ivariable.Type.Class == TypeClass.Reference;
			if (ivariable.IsProperty && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35730 || !flag))
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
			{
				if (ivariable.HasFlag(VarFlag.ReplacedConstant))
				{
					return false;
				}
				if (bWriteToConstants)
				{
					return true;
				}
				if (ivariable.HasFlag(VarFlag.Constant))
				{
					return bVarInoutConstant;
				}
			}
			else if (ivariable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant) && !TypeTable.IsBlock(ivariable.Type.Class))
			{
				return false;
			}
			return true;
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x000101C6 File Offset: 0x0000F1C6
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.IsLValue(scope, bWriteToConstants, false);
		}

		// Token: 0x060007BD RID: 1981 RVA: 0x00013296 File Offset: 0x00012296
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout)
		{
			return this.IsLValue(scope, bWriteToConstants, bPassToVarInout, false);
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x000132A4 File Offset: 0x000122A4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			_IVariable ivariable = this.GetVariable(scope) as _IVariable;
			if (ivariable == null)
			{
				return true;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900 && ivariable.Address != null && ivariable.Address.Location == DirectVariableLocation.Input && ivariable.Address.Incomplete)
			{
				return bWriteToConstants;
			}
			if (ivariable.Address != null && !bPassToVarInout && ivariable.Address.Location == DirectVariableLocation.Input && !ivariable.Address.Incomplete)
			{
				return false;
			}
			if (ivariable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
			{
				return !ivariable.HasFlag(VarFlag.ReplacedConstant) && bWriteToConstants;
			}
			if (ivariable.IsProperty)
			{
				if (bPassToVarInout)
				{
					return false;
				}
				if (ivariable.HasAttribute(CompileAttributes.DEVICE_PARAMETER) && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_READ_ONLY))
				{
					return true;
				}
				if (ivariable.HasAttribute(CompileAttributes.SET_ACCESS))
				{
					return true;
				}
				ISignature signature = scope[this.SignatureId];
				if (signature != null)
				{
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
					{
						IScope5 scope2 = (scope as IScope5).CreateLocalScope(signature);
						if (scope2.FindSignatureLocal(IdentifierConstants.CreateSetterName(ivariable.VersionedName)) == null)
						{
							return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100 && ivariable.Type != null && ivariable.Type.Class == TypeClass.Reference && !bRefAssign && scope2.FindSignatureLocal(IdentifierConstants.CreateGetterName(ivariable.VersionedName)) != null;
						}
					}
					else if (signature.GetSubSignature(IdentifierConstants.CreateSetterName(ivariable.VersionedName)) == null)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x060007BF RID: 1983 RVA: 0x00013418 File Offset: 0x00012418
		// (set) Token: 0x060007C0 RID: 1984 RVA: 0x00013420 File Offset: 0x00012420
		public string Name
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x060007C1 RID: 1985 RVA: 0x0000E8C4 File Offset: 0x0000D8C4
		public string Namespace
		{
			get
			{
				return string.Empty;
			}
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00013429 File Offset: 0x00012429
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x00013432 File Offset: 0x00012432
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x0001343B File Offset: 0x0001243B
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00013444 File Offset: 0x00012444
		public override IVariable GetVariable(IScope scope)
		{
			ISignature signature = scope[this.SignatureId];
			if (signature == null)
			{
				return null;
			}
			return signature[this.VariableId];
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x00013470 File Offset: 0x00012470
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			IVariable result = null;
			ISignature signature = null;
			IPrecompileScope precompileScope = null;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100 || this.PrecompileSignatureId == Common.InvalidID || this.PrecompileVariableId == Common.InvalidID)
			{
				scope.FindDeclaration(this.Name, out result, out signature, out precompileScope);
				return result;
			}
			signature = APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.PrecompileSignatureId);
			if (signature == null)
			{
				return null;
			}
			return signature[this.PrecompileVariableId];
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000134EC File Offset: 0x000124EC
		public ISignature GetSignature(IPrecompileScope scope)
		{
			IVariable variable = null;
			ISignature result = null;
			IPrecompileScope precompileScope = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100 && this.PrecompileSignatureId != Common.InvalidID)
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(this.PrecompileSignatureId);
			}
			scope.FindDeclaration(this.Name, out variable, out result, out precompileScope);
			return result;
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x060007C8 RID: 1992 RVA: 0x00013547 File Offset: 0x00012547
		// (set) Token: 0x060007C9 RID: 1993 RVA: 0x0001357A File Offset: 0x0001257A
		public override int VariableId
		{
			get
			{
				if (this.GetFlag(VarExprFlag.PrecompileTypified))
				{
					return Common.InvalidID;
				}
				if (this._CompiledType == null || this.m_varcompiledInfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_varcompiledInfo.IVariableId;
			}
			set
			{
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.IVariableId = value;
			}
		}

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x0001359B File Offset: 0x0001259B
		public override bool IsPOUReference
		{
			get
			{
				return this.VariableId == Expression.InvalidId && this.SignatureId != Expression.InvalidId;
			}
		}

		// Token: 0x060007CB RID: 1995 RVA: 0x000135BC File Offset: 0x000125BC
		public override ISignature GetSignature(IScope scope)
		{
			if (this.VariableId != Expression.InvalidId)
			{
				return null;
			}
			return scope[this.SignatureId];
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x060007CC RID: 1996 RVA: 0x000135D9 File Offset: 0x000125D9
		// (set) Token: 0x060007CD RID: 1997 RVA: 0x0001360C File Offset: 0x0001260C
		public override int SignatureId
		{
			get
			{
				if (this.GetFlag(VarExprFlag.PrecompileTypified))
				{
					return Common.InvalidID;
				}
				if (this._CompiledType == null || this.m_varcompiledInfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_varcompiledInfo.ISignatureId;
			}
			set
			{
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.ISignatureId = value;
			}
		}

		// Token: 0x060007CE RID: 1998 RVA: 0x0001362D File Offset: 0x0001262D
		public IScope GetScope(IScope scope)
		{
			return scope.GetScopeById(this.ScopeId);
		}

		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x060007CF RID: 1999 RVA: 0x0001363B File Offset: 0x0001263B
		// (set) Token: 0x060007D0 RID: 2000 RVA: 0x00013656 File Offset: 0x00012656
		public int ScopeId
		{
			get
			{
				if (this.m_varcompiledInfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_varcompiledInfo.IScopeId;
			}
			set
			{
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.IScopeId = value;
			}
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00013678 File Offset: 0x00012678
		public override _IExprement Duplicate()
		{
			VariableExpression variableExpression = new VariableExpression();
			variableExpression.m_stName = this.m_stName;
			if (this.m_varcompiledInfo != null)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
				{
					variableExpression.m_varcompiledInfo = new VarExpCompiledInfo();
					variableExpression.m_varcompiledInfo.IScopeId = this.m_varcompiledInfo.IScopeId;
					variableExpression.m_varcompiledInfo.ISignatureId = this.m_varcompiledInfo.ISignatureId;
					variableExpression.m_varcompiledInfo.IVariableId = this.m_varcompiledInfo.IVariableId;
				}
				else
				{
					variableExpression.VariableId = this.VariableId;
					variableExpression.SignatureId = this.SignatureId;
					variableExpression.ScopeId = this.ScopeId;
				}
				variableExpression.m_varcompiledInfo.Varexprflag = this.m_varcompiledInfo.Varexprflag;
			}
			this.DuplicateCommon(variableExpression);
			return variableExpression;
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x00013744 File Offset: 0x00012744
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			IVariable variable = this.GetVariable(scope);
			if (variable == null)
			{
				return false;
			}
			if (variable.Type is IReferenceType)
			{
				return false;
			}
			if (bAllocatedOK)
			{
				return variable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant);
			}
			return variable.HasFlag(VarFlag.ReplacedConstant);
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00013784 File Offset: 0x00012784
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			IVariable variable = this.GetVariable(scope);
			if (variable == null || !variable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
			{
				return base.LiteralUnchecked(scope);
			}
			if (variable.Initial == null)
			{
				return base.LiteralUnchecked(scope);
			}
			return (variable.Initial as Expression).LiteralUnchecked(scope);
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x000137EC File Offset: 0x000127EC
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x060007D5 RID: 2005 RVA: 0x00013808 File Offset: 0x00012808
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00013824 File Offset: 0x00012824
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900;
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), greaterEqualV, out flag);
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00013850 File Offset: 0x00012850
		public override IDataLocation DataLocation(IScope scope)
		{
			_IVariable ivariable = this.GetVariable(scope) as _IVariable;
			if (ivariable == null || ivariable.DataLocation == null || ivariable.GetFlag(VarFlag.ReplacedConstant) || ivariable.IsProperty)
			{
				return base.DataLocation(scope);
			}
			return ivariable.DataLocation;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x00013898 File Offset: 0x00012898
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			IVariable variable = this.GetVariable(scope);
			if (recursionGuard != null && variable != null && recursionGuard.Has(variable))
			{
				bRecursionError = true;
				return null;
			}
			if (variable != null && variable.HasAttribute("system_variable") && variable.Name == "__CONTAINS_COPY_CODE" && scope is IScope5 && (scope as IScope5).ApplicationContext != null)
			{
				CompileContext compileContext = (scope as IScope5).ApplicationContext as CompileContext;
				bRecursionError = false;
				return new LiteralValue(compileContext.ContainsCopyCode);
			}
			bRecursionError = false;
			if (recursionGuard != null && variable != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				recursionGuard.Add(variable);
			}
			if (variable == null || !variable.HasFlag(VarFlag.ReplacedConstant | VarFlag.Constant))
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			if (variable.Initial == null)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			return (variable.Initial as Expression).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
		}

		// Token: 0x060007D9 RID: 2009 RVA: 0x00013994 File Offset: 0x00012994
		[SuppressMessage("Major Bug", "S2259:Null pointers should not be dereferenced", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			IVariable variable = this.GetVariable(scope);
			ISignature signature = this.GetSignature(scope);
			if (recursionGuard != null && variable != null && recursionGuard.Has(variable))
			{
				bRecursionError = true;
				return null;
			}
			bRecursionError = false;
			if (recursionGuard != null && variable != null)
			{
				recursionGuard = recursionGuard.Duplicate();
				recursionGuard.Add(variable);
			}
			VarFlag vfFlag;
			if (bAllocatedOK)
			{
				vfFlag = (VarFlag.ReplacedConstant | VarFlag.Constant);
			}
			else
			{
				vfFlag = VarFlag.ReplacedConstant;
			}
			if (variable == null || !variable.HasFlag(vfFlag))
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			IExpression initialExpression = VariableExpression.GetInitialExpression(scope, signature, variable);
			if (initialExpression != null)
			{
				return ((Expression)initialExpression).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			ILiteralValue literalValue = base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			if (literalValue != null || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
			{
				return literalValue;
			}
			if (variable.Type.Class != TypeClass.Enum)
			{
				return base.LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			ILMPreCompileService5 ilmpreCompileService = (ILMPreCompileService5)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService;
			if (ilmpreCompileService == null)
			{
				return null;
			}
			return ilmpreCompileService.GetEnumInitValue(variable, scope as ICommonScope);
		}

		// Token: 0x060007DA RID: 2010 RVA: 0x00013AA4 File Offset: 0x00012AA4
		private static IExpression GetInitialExpression(IPrecompileScope scope, ISignature sign, IVariable variable)
		{
			IExpression expression = null;
			Guid guid = ((IPrecompileScope6)scope).ApplicationGuid;
			bool flag = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351760 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800;
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351840;
			bool flag2 = flag || greaterEqualV;
			PropertyInfo property = scope.GetType().GetProperty("RootApplicationGuid");
			if (property != null && flag2)
			{
				guid = (Guid)property.GetValue(scope);
			}
			if (guid != Guid.Empty && !string.IsNullOrEmpty(sign.LibraryPath) && sign.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST) && ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351740 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351810))
			{
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guid) as _IPreCompileContext).ParameterTable(sign.LibraryPath);
				if (caseInsensitiveDictionary != null && caseInsensitiveDictionary.ContainsKey(variable.Name))
				{
					expression = caseInsensitiveDictionary[variable.Name];
				}
			}
			if (expression == null)
			{
				expression = variable.Initial;
			}
			return expression;
		}

		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x060007DB RID: 2011 RVA: 0x00013BD2 File Offset: 0x00012BD2
		public override IExprInfo Info
		{
			get
			{
				return this.VarInfo;
			}
		}

		// Token: 0x170001BA RID: 442
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x00013BDA File Offset: 0x00012BDA
		// (set) Token: 0x060007DD RID: 2013 RVA: 0x00013BF1 File Offset: 0x00012BF1
		public IVariableExprInfo VarInfo
		{
			get
			{
				if (this.m_varcompiledInfo == null)
				{
					return null;
				}
				return this.m_varcompiledInfo.m_expinfo;
			}
			set
			{
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.m_expinfo = value;
			}
		}

		// Token: 0x170001BB RID: 443
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x00013C12 File Offset: 0x00012C12
		// (set) Token: 0x060007DF RID: 2015 RVA: 0x00013C1B File Offset: 0x00012C1B
		public bool NoVirtual
		{
			get
			{
				return this.GetFlag(VarExprFlag.NoVirtual);
			}
			set
			{
				this.SetFlag(VarExprFlag.NoVirtual, value);
			}
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x00013C25 File Offset: 0x00012C25
		public bool GetFlag(VarExprFlag vfFlag)
		{
			return this.m_varcompiledInfo != null && (this.m_varcompiledInfo.Varexprflag & vfFlag) == vfFlag;
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x00013C44 File Offset: 0x00012C44
		public void SetFlag(VarExprFlag vfFlag, bool bSetTrue)
		{
			if (this.m_varcompiledInfo == null)
			{
				this.m_varcompiledInfo = new VarExpCompiledInfo();
			}
			if (bSetTrue)
			{
				this.m_varcompiledInfo.Varexprflag |= vfFlag;
				return;
			}
			this.m_varcompiledInfo.Varexprflag &= ~vfFlag;
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x00013C90 File Offset: 0x00012C90
		public override string ToString()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				return this.m_stName;
			}
			return base.ToString();
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x060007E3 RID: 2019 RVA: 0x00013CB0 File Offset: 0x00012CB0
		// (set) Token: 0x060007E4 RID: 2020 RVA: 0x00013CC7 File Offset: 0x00012CC7
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				if (this.m_varcompiledInfo == null)
				{
					return null;
				}
				return this.m_varcompiledInfo.m_ctype;
			}
			set
			{
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.m_ctype = value;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x060007E5 RID: 2021 RVA: 0x00013CE8 File Offset: 0x00012CE8
		// (set) Token: 0x060007E6 RID: 2022 RVA: 0x00013CF0 File Offset: 0x00012CF0
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_position;
			}
			set
			{
				this.m_position = value;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x060007E7 RID: 2023 RVA: 0x00013CF9 File Offset: 0x00012CF9
		// (set) Token: 0x060007E8 RID: 2024 RVA: 0x00003AE9 File Offset: 0x00002AE9
		[DefaultSerialization("Length")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "Setter cannot be changed for compatibility reasons")]
		public override short LengthIntern
		{
			get
			{
				return (short)this.Name.Length;
			}
			set
			{
			}
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x060007E9 RID: 2025 RVA: 0x00013D07 File Offset: 0x00012D07
		// (set) Token: 0x060007EA RID: 2026 RVA: 0x00013D0F File Offset: 0x00012D0F
		[DefaultSerialization("VariableId")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private int VariableIdToSave
		{
			get
			{
				return this.VariableId;
			}
			set
			{
				this.VariableId = value;
			}
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x060007EB RID: 2027 RVA: 0x00013D18 File Offset: 0x00012D18
		// (set) Token: 0x060007EC RID: 2028 RVA: 0x00013D20 File Offset: 0x00012D20
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private int SignatureIdToSave
		{
			get
			{
				return this.SignatureId;
			}
			set
			{
				this.SignatureId = value;
			}
		}

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x060007ED RID: 2029 RVA: 0x00013D29 File Offset: 0x00012D29
		// (set) Token: 0x060007EE RID: 2030 RVA: 0x00013D31 File Offset: 0x00012D31
		[DefaultSerialization("ScopeId")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private int ScopeIdToSave
		{
			get
			{
				return this.ScopeId;
			}
			set
			{
				this.ScopeId = value;
			}
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060007EF RID: 2031 RVA: 0x00013D3C File Offset: 0x00012D3C
		// (set) Token: 0x060007F0 RID: 2032 RVA: 0x00013D88 File Offset: 0x00012D88
		public override int PrecompileVariableId
		{
			get
			{
				if (this.GetFlag(VarExprFlag.PrecompileTypified) && this.m_varcompiledInfo != null)
				{
					return this.m_varcompiledInfo.IVariableId;
				}
				if (this._CompiledType != null || this.m_varcompiledInfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_varcompiledInfo.IVariableId;
			}
			set
			{
				if (!this.GetFlag(VarExprFlag.PrecompileTypified) && this._CompiledType != null)
				{
					throw new InvalidOperationException("Cannot set precompile variable ID for a compiled variable expression");
				}
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.IVariableId = value;
			}
		}

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060007F1 RID: 2033 RVA: 0x00013DC8 File Offset: 0x00012DC8
		// (set) Token: 0x060007F2 RID: 2034 RVA: 0x00013E14 File Offset: 0x00012E14
		public override int PrecompileSignatureId
		{
			get
			{
				if (this.GetFlag(VarExprFlag.PrecompileTypified) && this.m_varcompiledInfo != null)
				{
					return this.m_varcompiledInfo.ISignatureId;
				}
				if (this._CompiledType != null || this.m_varcompiledInfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_varcompiledInfo.ISignatureId;
			}
			set
			{
				if (!this.GetFlag(VarExprFlag.PrecompileTypified) && this._CompiledType != null)
				{
					throw new InvalidOperationException("Cannot set precompile signature ID for a compiled variable expression");
				}
				if (this.m_varcompiledInfo == null)
				{
					this.m_varcompiledInfo = new VarExpCompiledInfo();
				}
				this.m_varcompiledInfo.ISignatureId = value;
			}
		}

		// Token: 0x04000109 RID: 265
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x0400010A RID: 266
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[DefaultDuplication(DuplicationMethod.Shallow)]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x0400010B RID: 267
		private VarExpCompiledInfo m_varcompiledInfo;
	}
}
