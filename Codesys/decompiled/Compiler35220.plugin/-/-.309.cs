using System;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0019
{
	// Token: 0x02000344 RID: 836
	internal sealed class \u0012
	{
		// Token: 0x1700083B RID: 2107
		// (get) Token: 0x06003294 RID: 12948 RVA: 0x000C2960 File Offset: 0x000C0B60
		private global::\u000E.\u0017 Context { get; }

		// Token: 0x1700083C RID: 2108
		// (get) Token: 0x06003295 RID: 12949 RVA: 0x000C2968 File Offset: 0x000C0B68
		private IScope Scope
		{
			get
			{
				return this.Context.Scope;
			}
		}

		// Token: 0x1700083D RID: 2109
		// (get) Token: 0x06003296 RID: 12950 RVA: 0x000C2978 File Offset: 0x000C0B78
		private _ICompileContext Comcon
		{
			get
			{
				return this.Context.Comcon;
			}
		}

		// Token: 0x1700083E RID: 2110
		// (get) Token: 0x06003297 RID: 12951 RVA: 0x000C2988 File Offset: 0x000C0B88
		private IErrorVisitor Errorvisitor
		{
			get
			{
				return this.Context.Errorvisitor;
			}
		}

		// Token: 0x1700083F RID: 2111
		// (get) Token: 0x06003298 RID: 12952 RVA: 0x000C2998 File Offset: 0x000C0B98
		private ISourcePosition SourcePos
		{
			get
			{
				return this.Context.SourcePos;
			}
		}

		// Token: 0x17000840 RID: 2112
		// (get) Token: 0x06003299 RID: 12953 RVA: 0x000C29A8 File Offset: 0x000C0BA8
		private _ISignature SignDecl
		{
			get
			{
				return this.Context.SignDecl;
			}
		}

		// Token: 0x17000841 RID: 2113
		// (get) Token: 0x0600329A RID: 12954 RVA: 0x000C29B8 File Offset: 0x000C0BB8
		private GenericTypeChecker GenericTypeChecker { get; }

		// Token: 0x17000842 RID: 2114
		// (get) Token: 0x0600329B RID: 12955 RVA: 0x000C29C0 File Offset: 0x000C0BC0
		// (set) Token: 0x0600329C RID: 12956 RVA: 0x000C29C8 File Offset: 0x000C0BC8
		internal _ISignature UserdefSignature { get; set; }

		// Token: 0x17000843 RID: 2115
		// (get) Token: 0x0600329D RID: 12957 RVA: 0x000C29D4 File Offset: 0x000C0BD4
		// (set) Token: 0x0600329E RID: 12958 RVA: 0x000C29DC File Offset: 0x000C0BDC
		private bool ReferencingCrossReference { get; set; }

		// Token: 0x17000844 RID: 2116
		// (get) Token: 0x0600329F RID: 12959 RVA: 0x000C29E8 File Offset: 0x000C0BE8
		// (set) Token: 0x060032A0 RID: 12960 RVA: 0x000C29F0 File Offset: 0x000C0BF0
		internal _IType GeneratedType { get; private set; }

		// Token: 0x060032A1 RID: 12961 RVA: 0x000C29FC File Offset: 0x000C0BFC
		internal \u0012(global::\u000E.\u0017 \u0083\u0005, bool \u0004\u0008, GenericTypeChecker \u0005\u0008)
		{
			this.Context = \u0083\u0005;
			this.ReferencingCrossReference = \u0004\u0008;
			this.GenericTypeChecker = \u0005\u0008;
		}

		// Token: 0x060032A2 RID: 12962 RVA: 0x000C2A1C File Offset: 0x000C0C1C
		public void \u0001(_IUserdefType \u0002)
		{
			this.UserdefSignature = this.\u0001(\u0002.NameExpression as _IExpression);
			if (this.UserdefSignature == null)
			{
				this.Errorvisitor.MessageList.Add(global::\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_UnknownType, new object[]
				{
					\u0002.NameExpression.ToString()
				}), Severity.Error, MessageId.Err_UnknownType));
			}
			else
			{
				\u0002.SignatureId = this.UserdefSignature.Id;
				this.\u0003(this.UserdefSignature);
				this.\u0003(\u0002, this.UserdefSignature);
				if (this.UserdefSignature.GetFlag(SignatureFlag.Alias) && this.UserdefSignature.All.Length == 1)
				{
					this.\u0002(\u0002, this.UserdefSignature);
				}
				else if (this.UserdefSignature.GetFlag(SignatureFlag.Enum))
				{
					this.\u0002(this.UserdefSignature);
				}
				else if (this.UserdefSignature.POUType == Operator.Interface)
				{
					this.\u0001(\u0002, this.UserdefSignature);
				}
			}
			this.GenericTypeChecker.\u0001(\u0002, this.UserdefSignature, this.Context.VarWithType);
			if (this.GeneratedType == null)
			{
				this.GeneratedType = \u0002;
			}
		}

		// Token: 0x060032A3 RID: 12963 RVA: 0x000C2B44 File Offset: 0x000C0D44
		private void \u0001(_IUserdefType \u0002, _ISignature \u0003)
		{
			string stName = CompilerServicesInternal.\u0001(\u0003, this.Comcon);
			_ICompileContext icompileContext = this.Comcon;
			ISignature signature = null;
			while (icompileContext != null && signature == null)
			{
				signature = icompileContext[stName];
				icompileContext = icompileContext.ParentContext;
			}
			Debug.\u0001(signature != null);
			_IUserdefType iuserdefType = global::\u0019.\u0003.\u0001(((_IExpression)\u0002.NameExpression).Duplicate() as _IExpression);
			if (signature != null)
			{
				iuserdefType.SignatureId = signature.Id;
			}
			this.GeneratedType = iuserdefType;
		}

		// Token: 0x060032A4 RID: 12964 RVA: 0x000C2BB8 File Offset: 0x000C0DB8
		private void \u0002(_ISignature \u0002)
		{
			if (\u0002.AllVariables.Count > 0)
			{
				this.GeneratedType = (\u0002.AllVariables.First<_IVariable>().CompiledType as _IType);
				return;
			}
			this.GeneratedType = global::\u0019.\u0003.\u0001(\u0002.Name, \u0002.Id);
		}

		// Token: 0x060032A5 RID: 12965 RVA: 0x000C2C08 File Offset: 0x000C0E08
		private void \u0002(_IUserdefType \u0002, _ISignature \u0003)
		{
			_IExpression iexpression = \u0003.All[0].Initial as _IExpression;
			_IAliasType ialiasType = global::\u0019.\u0003.\u0001(\u0003.All[0].CompiledType as _IType);
			ialiasType.IsCompiled = true;
			if (iexpression != null)
			{
				ialiasType._DefaultValue = iexpression;
			}
			ialiasType.ScopeId = \u0002.ScopeId;
			ialiasType.SignatureId = \u0002.SignatureId;
			ialiasType.NameExpression = \u0002.NameExpression;
			this.GeneratedType = ialiasType;
			bool flag = ialiasType.EffectiveType.Class == TypeClass.Bit;
			bool flag2 = this.SignDecl != null && (this.SignDecl.POUType == Operator.FunctionBlock || this.SignDecl.GetFlag(SignatureFlag.Structure) || this.SignDecl.GetFlag(SignatureFlag.Alias));
			if (flag && !flag2)
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Err_BitsForStructureOnly, Array.Empty<object>());
				this.Errorvisitor.MessageList.Add(global::\u0019.\u0003.\u0001(this.SourcePos, u, Severity.Error, MessageId.Err_BitsForStructureOnly));
			}
		}

		// Token: 0x060032A6 RID: 12966 RVA: 0x000C2CFC File Offset: 0x000C0EFC
		private void \u0003(_ISignature \u0002)
		{
			if (this.SignDecl != null)
			{
				if (this.ReferencingCrossReference)
				{
					\u0002.AddReferencer(this.SignDecl.Id);
					return;
				}
				\u0002.AddDeclarer(this.SignDecl.Id);
			}
		}

		// Token: 0x060032A7 RID: 12967 RVA: 0x000C2D34 File Offset: 0x000C0F34
		private void \u0003(_IUserdefType \u0002, _ISignature \u0003)
		{
			if ((\u0003.POUType == Operator.Program && this.SignDecl != null && this.SignDecl.ParentSignatureId != \u0003.Id) || \u0003.POUType == Operator.Method || \u0003.POUType == Operator.Action || \u0003.POUType == Operator.Function || (\u0003.POUType == Operator.VarGlobal && !\u0003.GetFlag(SignatureFlag.Enum) && this.SignDecl != null && this.SignDecl.ParentSignatureId != \u0003.Id))
			{
				this.Errorvisitor.MessageList.Add(global::\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_NoInstanceObject, new object[]
				{
					\u0002.NameExpression.ToString(),
					Scanner.GetTextOfOperator(\u0003.POUType)
				}), Severity.Error, MessageId.Err_NoInstanceObject));
			}
		}

		// Token: 0x060032A8 RID: 12968 RVA: 0x000C2E00 File Offset: 0x000C1000
		private _ISignature \u0001(_IExpression \u0002)
		{
			_ISignature result = null;
			if (\u0002 != null)
			{
				\u0081.\u0016.\u0001(this.Scope.GlobalScope, \u0002);
				IScope2 scope = this.Scope as IScope2;
				ISignature[] array = (scope != null) ? scope.FindSignature(\u0002) : null;
				if (array != null && array.Length != 0)
				{
					result = (array[0] as _ISignature);
					if (array.Length > 1)
					{
						this.Errorvisitor.MessageList.Add(global::\u0019.\u0003.\u0001(this.SourcePos, global::\u0003.\u0006.\u0001(MessageId.Err_Ambiguity, new object[]
						{
							\u0002.ToString()
						}), Severity.Error, MessageId.Err_Ambiguity));
						foreach (ISignature signature in array)
						{
							_ISourcePosition isourcePosition = global::\u0019.\u0003.\u0001();
							isourcePosition.SetObjectIdentification(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), signature.ObjectGuid);
							string u = global::\u0003.\u0006.\u0001(MessageId.Inf_RelatedPosition, Array.Empty<object>());
							this.Errorvisitor.MessageList.Add(global::\u0019.\u0003.\u0001(isourcePosition, u, Severity.Information, MessageId.Inf_RelatedPosition));
						}
					}
				}
			}
			return result;
		}

		// Token: 0x04000982 RID: 2434
		[CompilerGenerated]
		private readonly global::\u000E.\u0017 \u0001;

		// Token: 0x04000983 RID: 2435
		[CompilerGenerated]
		private readonly GenericTypeChecker \u0001;

		// Token: 0x04000984 RID: 2436
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x04000985 RID: 2437
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000986 RID: 2438
		[CompilerGenerated]
		private _IType \u0001;
	}
}
