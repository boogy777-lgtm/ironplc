using System;
using System.Collections.Generic;
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
	// Token: 0x02000048 RID: 72
	[TypeGuid("{0ba7ff45-cf9b-419a-84c8-ae1727e3961f}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class CompoAccessExpression : Expression, _ICompoAccessExpression, _IExpression, _IExprement, IExprement3, IExprement2, IExprement, IExpression6, IExpression5, IExpression4, IExpression3, IExpression2, IExpression, ICompoAccessExpression, IQualifiedNameExpression, ILengthExprement
	{
		// Token: 0x060003DF RID: 991 RVA: 0x0000C3D6 File Offset: 0x0000B3D6
		public CompoAccessExpression()
		{
		}

		// Token: 0x060003E0 RID: 992 RVA: 0x0000C797 File Offset: 0x0000B797
		internal CompoAccessExpression(_IExpression expLeft, IToken token) : base(token)
		{
			this.m_exp = expLeft;
		}

		// Token: 0x060003E1 RID: 993 RVA: 0x0000C7A7 File Offset: 0x0000B7A7
		internal CompoAccessExpression(_IExpression expLeft)
		{
			this.m_exp = expLeft;
		}

		// Token: 0x060003E2 RID: 994 RVA: 0x0000C7B8 File Offset: 0x0000B7B8
		public override bool IsVarInOutInput(IScope scope, bool bWriteToConstants, bool bVarInoutConstant)
		{
			if (this._Right.IsLiteral)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352100 && base.Type != null && base.Type.Class == TypeClass.Reference)
			{
				return true;
			}
			if (this._Left.GetType() == typeof(ThisExpression) || this._Left.GetType() == typeof(BaseExpression))
			{
				return this._Right.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
			}
			return this._Left.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant) && this._Right.IsVarInOutInput(scope, bWriteToConstants, bVarInoutConstant);
		}

		// Token: 0x060003E3 RID: 995 RVA: 0x0000C865 File Offset: 0x0000B865
		public override bool IsLValue(IScope scope, bool bWriteToConstants)
		{
			return this.IsLValue(scope, bWriteToConstants, false, false);
		}

		// Token: 0x060003E4 RID: 996 RVA: 0x0000C874 File Offset: 0x0000B874
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override bool IsLValue(IScope scope, bool bWriteToConstants, bool bPassToVarInout, bool bRefAssign)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000 && this.Left is IDeRefAccessExpression && (this.Left as IDeRefAccessExpression).Base is IThisExpression)
			{
				return this._Right.IsLValue(scope, bWriteToConstants);
			}
			if (this._Right.Literal(scope) != null)
			{
				return (this._Left.GetVariable(scope) != null || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3200) && this._Left.IsLValue(scope, bWriteToConstants);
			}
			if (this._Left.GetType() == typeof(ThisExpression) || this._Left.GetType() == typeof(BaseExpression))
			{
				return this._Right.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34400)
			{
				_IVariable ivariable = this._Left.GetVariable(scope) as _IVariable;
				if (ivariable != null && ivariable.IsProperty && this._Left.Type != null)
				{
					bool flag = this._Left.Type != null && this._Left.Type.Class == TypeClass.Reference;
					bool flag2 = false;
					if (this._Left.Type.Class == TypeClass.Userdef)
					{
						ISignature signature = (this._Left.Type as UserdefType).GetSignature(scope);
						if (signature != null && signature.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
						{
							flag2 = true;
						}
					}
					if (flag || flag2)
					{
						return this._Right.IsLValue(scope, bWriteToConstants);
					}
				}
			}
			return this._Left.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign) && this._Right.IsLValue(scope, bWriteToConstants, bPassToVarInout, bRefAssign);
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060003E5 RID: 997 RVA: 0x0000CA28 File Offset: 0x0000BA28
		public IExpression Left
		{
			get
			{
				return this._Left;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060003E6 RID: 998 RVA: 0x0000CA30 File Offset: 0x0000BA30
		// (set) Token: 0x060003E7 RID: 999 RVA: 0x0000CA46 File Offset: 0x0000BA46
		public _IExpression _Left
		{
			get
			{
				if (this.m_exp == null)
				{
					return new NullExpression();
				}
				return this.m_exp;
			}
			set
			{
				this.m_exp = value;
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x060003E8 RID: 1000 RVA: 0x0000CA4F File Offset: 0x0000BA4F
		public IExpression Right
		{
			get
			{
				return this._Right;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x060003E9 RID: 1001 RVA: 0x0000CA57 File Offset: 0x0000BA57
		// (set) Token: 0x060003EA RID: 1002 RVA: 0x0000CA6D File Offset: 0x0000BA6D
		public _IExpression _Right
		{
			get
			{
				if (this.m_var == null)
				{
					return new NullExpression();
				}
				return this.m_var;
			}
			set
			{
				this.m_var = value;
			}
		}

		// Token: 0x060003EB RID: 1003 RVA: 0x0000CA76 File Offset: 0x0000BA76
		public override void Accept(IExprementVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0000CA7F File Offset: 0x0000BA7F
		public override T Accept<T>(IExprementVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		// Token: 0x060003ED RID: 1005 RVA: 0x0000CA88 File Offset: 0x0000BA88
		public override void AcceptVisitor(IExprVisitor visitor)
		{
			visitor.visit(this);
		}

		// Token: 0x060003EE RID: 1006 RVA: 0x0000CA94 File Offset: 0x0000BA94
		public override ISourcePosition GetPosition()
		{
			SourcePosition sourcePosition = this.m_exp.GetPosition() as SourcePosition;
			if (sourcePosition != null)
			{
				sourcePosition.Length = this.m_sLength;
			}
			return sourcePosition;
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x060003EF RID: 1007 RVA: 0x0000CAC2 File Offset: 0x0000BAC2
		// (set) Token: 0x060003F0 RID: 1008 RVA: 0x0000CACF File Offset: 0x0000BACF
		[Obfuscation(Feature = "rename")]
		public override IMinimalPosition PositionIntern
		{
			get
			{
				return this.m_exp.PositionIntern;
			}
			set
			{
				if (this.m_exp != null)
				{
					this.m_exp.PositionIntern = value;
				}
			}
		}

		// Token: 0x060003F1 RID: 1009 RVA: 0x0000CAE8 File Offset: 0x0000BAE8
		[Obfuscation(Feature = "rename")]
		public override void SetPositionIntern(IMinimalPosition minpos)
		{
			this.m_exp.PositionIntern = minpos;
			if (this.m_var != null && this.m_var.PositionIntern != null)
			{
				this.m_var.PositionIntern = MinimalPosition.CreateMinimalPosition(minpos.EditorPosition, this.m_var.PositionIntern.PositionOffset);
			}
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060003F2 RID: 1010 RVA: 0x0000CB3C File Offset: 0x0000BB3C
		// (set) Token: 0x060003F3 RID: 1011 RVA: 0x0000CB44 File Offset: 0x0000BB44
		[Obfuscation(Feature = "rename")]
		public override short LengthIntern
		{
			get
			{
				return this.m_sLength;
			}
			set
			{
				this.m_sLength = value;
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x0000CB50 File Offset: 0x0000BB50
		public override IVariable GetVariable(IScope scope)
		{
			IVariable variable = this._Right.GetVariable(scope);
			if (variable == null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000 && this._Right.Literal(scope) != null)
			{
				variable = this._Left.GetVariable(scope);
			}
			return variable;
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x060003F5 RID: 1013 RVA: 0x0000CB9A File Offset: 0x0000BB9A
		public override bool IsPOUReference
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33010)
				{
					return this._Right.IsPOUReference;
				}
				return base.IsPOUReference;
			}
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0000CBBF File Offset: 0x0000BBBF
		public override IVariable GetVariable(IPrecompileScope scope)
		{
			return this._Right.GetVariable(scope);
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0000CBCD File Offset: 0x0000BBCD
		public override ISignature GetSignature(IScope scope)
		{
			return this._Right.GetSignature(scope);
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0000CBDC File Offset: 0x0000BBDC
		public override ISignature GetSignatureEx(IScope scope)
		{
			ISignature signatureEx = this._Right.GetSignatureEx(scope);
			if (signatureEx == null && this._Right.Literal(scope) != null)
			{
				signatureEx = this._Left.GetSignatureEx(scope);
			}
			return signatureEx;
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x060003F9 RID: 1017 RVA: 0x0000CC15 File Offset: 0x0000BC15
		// (set) Token: 0x060003FA RID: 1018 RVA: 0x0000CC22 File Offset: 0x0000BC22
		public override int PrecompileVariableId
		{
			get
			{
				return this._Right.PrecompileVariableId;
			}
			set
			{
				this._Right.PrecompileVariableId = value;
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x060003FB RID: 1019 RVA: 0x0000CC30 File Offset: 0x0000BC30
		// (set) Token: 0x060003FC RID: 1020 RVA: 0x0000CC3D File Offset: 0x0000BC3D
		public override int PrecompileSignatureId
		{
			get
			{
				return this._Right.PrecompileSignatureId;
			}
			set
			{
				this._Right.PrecompileSignatureId = value;
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x060003FD RID: 1021 RVA: 0x0000CC4B File Offset: 0x0000BC4B
		// (set) Token: 0x060003FE RID: 1022 RVA: 0x0000CC87 File Offset: 0x0000BC87
		public override int SignatureId
		{
			get
			{
				if (this._Left.SignatureId == Common.InvalidID && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
				{
					return this._Right.SignatureId;
				}
				return this._Left.SignatureId;
			}
			set
			{
				throw new NotSupportedException("Signature Id cant be set in compo access");
			}
		}

		// Token: 0x060003FF RID: 1023 RVA: 0x0000CC94 File Offset: 0x0000BC94
		public override _IExprement Duplicate()
		{
			CompoAccessExpression compoAccessExpression = new CompoAccessExpression();
			if (this.m_exp != null)
			{
				compoAccessExpression.m_exp = (this.m_exp.Duplicate() as Expression);
			}
			this.DuplicateCommon(compoAccessExpression);
			if (this.m_var != null)
			{
				compoAccessExpression.m_var = (this.m_var.Duplicate() as Expression);
			}
			return compoAccessExpression;
		}

		// Token: 0x06000400 RID: 1024 RVA: 0x0000CCEC File Offset: 0x0000BCEC
		public override bool IsConstant(IScope scope, bool bAllocatedOK)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3200)
			{
				return this._Left.IsConstant(scope, bAllocatedOK) || this._Right.IsConstant(scope, bAllocatedOK);
			}
			if (this._Left.IsConstant(scope, bAllocatedOK))
			{
				return true;
			}
			if (this._Left.Type != null && this._Left.Type.Class == TypeClass.Userdef)
			{
				return this._Right.IsConstant(scope, bAllocatedOK);
			}
			return (this._Left.Type == null || !TypeTable.IsInteger(this._Left.Type.Class)) && this._Right.Literal(scope) == null && this._Right.IsConstant(scope, bAllocatedOK);
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0000CDB0 File Offset: 0x0000BDB0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralUnchecked(IScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			ILiteralValue literalValue = this._Left.LiteralUnchecked(scope);
			ILiteralValue literalValue2 = null;
			if (literalValue == null && this._Left.Type != null && this._Left.Type.Class == TypeClass.Userdef)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && this._Left.IsConstant(scope, true) && !this._Right.IsConstant(scope, true))
				{
					IVariable variable = this._Left.GetVariable(scope);
					IVariable variable2 = this._Right.GetVariable(scope);
					if (variable != null && variable.Initial != null)
					{
						bool flag2;
						StructureInitialisation structInit = CompoAccessExpression.GetStructInit(this._Left, scope, new RecursionGuard(), out flag2);
						if (structInit != null)
						{
							foreach (_IAssignmentExpression iassignmentExpression in structInit._CompoInits)
							{
								IVariable variable3 = iassignmentExpression._LValue.GetVariable(scope);
								if (variable3 != null && variable3.Name == variable2.Name)
								{
									literalValue2 = iassignmentExpression._RValue.LiteralUnchecked(scope);
									break;
								}
							}
						}
					}
					if (literalValue2 == null && variable2 != null && variable2.Initial != null)
					{
						literalValue2 = (variable2.Initial as Expression).LiteralUnchecked(scope);
					}
				}
				else
				{
					literalValue2 = this._Right.LiteralUnchecked(scope);
				}
				return literalValue2;
			}
			if (literalValue == null && this._Left.Type != null && this._Left.Type.Class == TypeClass.Userdef)
			{
				return this._Right.LiteralUnchecked(scope);
			}
			literalValue2 = this._Right.LiteralUnchecked(scope);
			if (literalValue2 == null)
			{
				return this._Right.LiteralUnchecked(scope);
			}
			if (literalValue == null)
			{
				return null;
			}
			int num;
			if (!literalValue2.GetInt(out num))
			{
				return null;
			}
			int num2;
			if (!literalValue.GetInt(out num2))
			{
				return null;
			}
			int num3 = num2 >> num & 1;
			bool b;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV32120)
			{
				b = (num3 != 0);
			}
			else
			{
				b = (num3 == 0);
			}
			return new LiteralValue(b);
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x0000CFE4 File Offset: 0x0000BFE4
		private static StructureInitialisation GetStructInit(_IExpression expr, IScope scope, IRecursionGuard recursionGuard, out bool bRecursion)
		{
			bRecursion = false;
			IVariable variable = expr.GetVariable(scope);
			if (variable == null || variable.Initial == null)
			{
				return null;
			}
			StructureInitialisation structureInitialisation = variable.Initial as StructureInitialisation;
			if (structureInitialisation != null)
			{
				return structureInitialisation;
			}
			ArrayInitialisation arrayInitialisation = variable.Initial as ArrayInitialisation;
			if (arrayInitialisation != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				IndexAccessExpression indexAccessExpression = expr as IndexAccessExpression;
				int num;
				if (indexAccessExpression != null && indexAccessExpression.GetConstantIndex(scope, recursionGuard, out bRecursion, out num))
				{
					bool flag;
					IList<_IExpression> flatList = arrayInitialisation.GetFlatList(scope, out flag);
					if (flatList != null && flag && num >= 0 && num < flatList.Count)
					{
						return flatList[num] as StructureInitialisation;
					}
				}
			}
			return null;
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x0000D088 File Offset: 0x0000C088
		public override ILiteralValue Literal(IScope scope, bool bAllocatedOK)
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400)
			{
				return base.Literal(scope, bAllocatedOK);
			}
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), bAllocatedOK, out flag);
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x0000D0C0 File Offset: 0x0000C0C0
		public override ILiteralValue Literal(IScope scope)
		{
			bool flag;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), false, out flag);
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x0000D0DC File Offset: 0x0000C0DC
		public override ILiteralValue Literal(IPrecompileScope scope)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				bool flag;
				return constantFolder.GetLiteralValue(this, scope, out flag);
			}
			bool greaterEqualV = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800;
			bool flag2;
			return base.LiteralWithRecursionCheck(scope, new LDictionary<IVariable, IVariable>(), greaterEqualV, out flag2);
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x0000D124 File Offset: 0x0000C124
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			bRecursionError = false;
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, out bRecursionError);
			}
			ILiteralValue literalValue = ((_IExpression2)this._Left).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			if (bRecursionError)
			{
				return null;
			}
			ILiteralValue literalValue2 = null;
			bool flag = literalValue == null && this._Left.Type != null && (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700 ? (this._Left.Type.BaseType.Class == TypeClass.Userdef) : (this._Left.Type.Class == TypeClass.Userdef));
			bool flag2 = this._Left.Type != null && this._Left.Type.Class == TypeClass.Reference;
			if (flag)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400 && this._Left.IsConstant(scope, true) && !flag2 && !this._Right.IsConstant(scope, bAllocatedOK))
				{
					IVariable variable = this._Left.GetVariable(scope);
					IVariable variable2 = this._Right.GetVariable(scope);
					if (variable != null && variable.Initial != null && variable2 != null)
					{
						StructureInitialisation structInit = CompoAccessExpression.GetStructInit(this._Left, scope, recursionGuard, out bRecursionError);
						if (structInit != null)
						{
							foreach (_IAssignmentExpression iassignmentExpression in structInit._CompoInits)
							{
								IVariable variable3 = iassignmentExpression._LValue.GetVariable(scope);
								if (variable3 != null && variable3.Name == variable2.Name)
								{
									literalValue2 = ((_IExpression2)iassignmentExpression._RValue).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
									break;
								}
							}
						}
					}
					if (literalValue2 == null && variable2 != null && variable2.Initial != null)
					{
						literalValue2 = (variable2.Initial as Expression).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
					}
				}
				else
				{
					literalValue2 = ((_IExpression2)this._Right).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
				}
				return literalValue2;
			}
			literalValue2 = ((_IExpression2)this._Right).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			if (bRecursionError)
			{
				return null;
			}
			if (literalValue2 == null)
			{
				return ((_IExpression2)this._Right).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			}
			if (literalValue == null)
			{
				return null;
			}
			int num;
			if (!literalValue2.GetInt(out num))
			{
				return null;
			}
			int num2;
			if (!literalValue.GetInt(out num2))
			{
				return null;
			}
			int num3 = num2 >> num & 1;
			bool b;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV32120)
			{
				b = (num3 != 0);
			}
			else
			{
				b = (num3 == 0);
			}
			return new LiteralValue(b);
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0000D3C4 File Offset: 0x0000C3C4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-86324")]
		public override ILiteralValue LiteralWithRecursionCheck(IPrecompileScope scope, IRecursionGuard recursionGuard, bool bAllocatedOK, out bool bRecursionError)
		{
			IConstantFolder3 constantFolder = VersionedCompilerFactory._ConstantFolder_OrNull as IConstantFolder3;
			if (constantFolder != null)
			{
				return constantFolder.GetLiteralValue(this, scope, recursionGuard, out bRecursionError);
			}
			ILiteralValue literalValue = ((_IExpression2)this._Left).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
			bRecursionError = false;
			if (literalValue == null && this._Left.Type != null && this._Left.Type.Class == TypeClass.Userdef)
			{
				ILiteralValue literalValue2 = ((_IExpression2)this._Right).LiteralWithRecursionCheck(scope, recursionGuard, bAllocatedOK, out bRecursionError);
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352100)
				{
					return literalValue2;
				}
				if (literalValue2 != null)
				{
					return literalValue2;
				}
			}
			IPrecompileScope scope2 = scope;
			IVariable variable;
			ISignature signature;
			IPrecompileScope precompileScope;
			scope.FindDeclaration(this._Left.ToString(), out variable, out signature, out precompileScope);
			if (variable == null && signature == null && precompileScope != null)
			{
				scope2 = precompileScope;
			}
			else if (variable == null && signature != null)
			{
				scope2 = scope.NewLocalScope(signature);
			}
			ILiteralValue literalValue3 = ((_IExpression2)this._Right).LiteralWithRecursionCheck(scope2, recursionGuard, bAllocatedOK, out bRecursionError);
			bool flag = literalValue3 != null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				flag = (flag && literalValue != null);
			}
			if (!flag)
			{
				return literalValue3;
			}
			if (literalValue == null)
			{
				return null;
			}
			int num;
			if (!literalValue3.GetInt(out num))
			{
				return null;
			}
			int num2;
			if (!literalValue.GetInt(out num2))
			{
				return null;
			}
			int num3 = num2 >> num & 1;
			bool b;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV32120)
			{
				b = (num3 != 0);
			}
			else
			{
				b = (num3 == 0);
			}
			return new LiteralValue(b);
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0000D530 File Offset: 0x0000C530
		public override IDataLocation DataLocation(IScope scope)
		{
			IDataLocation dataLocation = this._Left.DataLocation(scope);
			if (dataLocation == null && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				return base.DataLocation(scope);
			}
			IDataLocation dataLocation2 = this._Right.DataLocation(scope);
			if (dataLocation2 == null)
			{
				return base.DataLocation(scope);
			}
			if (!dataLocation2.IsRelativ && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				return dataLocation2;
			}
			IDataLocation result;
			if (dataLocation == null)
			{
				result = dataLocation2;
			}
			else if (dataLocation.IsRelativ)
			{
				if (dataLocation2.IsBitLocation)
				{
					result = LanguageModelBuilder.Singleton.CreateRelativeBitDataLocation(dataLocation.Offset + dataLocation2.Offset, dataLocation2.BitNr);
				}
				else
				{
					result = LanguageModelBuilder.Singleton.CreateRelativeDataLocation(dataLocation.Offset + dataLocation2.Offset, (dataLocation as _IRelativeDataLocation).Flags);
				}
			}
			else if (dataLocation2.IsBitLocation)
			{
				result = LanguageModelBuilder.Singleton.CreateBitDataLocation(dataLocation.Area, dataLocation.Offset + dataLocation2.Offset, dataLocation2.BitNr);
			}
			else
			{
				result = LanguageModelBuilder.Singleton.CreateDataLocation(dataLocation.Area, dataLocation.Offset + dataLocation2.Offset);
			}
			return result;
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000409 RID: 1033 RVA: 0x0000D645 File Offset: 0x0000C645
		// (set) Token: 0x0600040A RID: 1034 RVA: 0x0000D64D File Offset: 0x0000C64D
		public override IExprInfo Info
		{
			get
			{
				return this.m_expInfo;
			}
			set
			{
				this.m_expInfo = value;
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x0600040B RID: 1035 RVA: 0x0000D656 File Offset: 0x0000C656
		public string Name
		{
			get
			{
				return this._Right.ToString();
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x0600040C RID: 1036 RVA: 0x0000D663 File Offset: 0x0000C663
		public string Namespace
		{
			get
			{
				return this._Left.ToString();
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x0600040D RID: 1037 RVA: 0x0000D670 File Offset: 0x0000C670
		// (set) Token: 0x0600040E RID: 1038 RVA: 0x0000D678 File Offset: 0x0000C678
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.10")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		public override ICompiledType _CompiledType
		{
			get
			{
				return this.m_ctype;
			}
			set
			{
				this.m_ctype = value;
			}
		}

		// Token: 0x0400009B RID: 155
		[Obfuscation(Feature = "rename")]
		protected short m_sLength;

		// Token: 0x0400009C RID: 156
		[DefaultSerialization("compiledtype")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private ICompiledType m_ctype;

		// Token: 0x0400009D RID: 157
		[DefaultSerialization("expinfo")]
		[StorageVersion("3.3.0.0-3.5.10.255")]
		[StorageDefaultValue(null)]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private IExprInfo m_expInfo;

		// Token: 0x0400009E RID: 158
		[DefaultSerialization("Left")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IExpression m_exp;

		// Token: 0x0400009F RID: 159
		[DefaultSerialization("Right")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[DefaultDuplication(DuplicationMethod.Deep)]
		private _IExpression m_var;
	}
}
