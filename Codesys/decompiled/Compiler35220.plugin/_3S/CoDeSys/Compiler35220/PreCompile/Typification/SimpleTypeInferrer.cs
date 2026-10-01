using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0001;
using \u0004;
using \u0005;
using \u0006;
using \u0015;
using \u0018;
using \u0019;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.PreCompile.Typification
{
	// Token: 0x020001B2 RID: 434
	internal sealed class SimpleTypeInferrer : global::\u0001.\u0007
	{
		// Token: 0x06001FDE RID: 8158 RVA: 0x0006BEC4 File Offset: 0x0006A0C4
		public SimpleTypeInferrer(_ISignature signature, int nProjectHandle, _IPreCompileContext precomLocal, bool bKeepAlias) : base(signature, nProjectHandle, precomLocal, false)
		{
			this.KeepAliases = bKeepAlias;
			global::\u0015.\u0002 u = CheckerScope.\u0001(base.PointerSize, this.\u0001._GetLibraryTable(), signature, this.\u0001, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			base.\u0001(u, AccessFlag.Unknown);
			base.TopOfStack.\u0001 = IgnoreCheckerMessageFlags.Information;
		}

		// Token: 0x06001FDF RID: 8159 RVA: 0x0006BF44 File Offset: 0x0006A144
		public SimpleTypeInferrer(CheckerScope scope) : base(scope.LocalSignature as _ISignature, scope.\u0001(), scope.LocalContext, false)
		{
			base.\u0001(scope, AccessFlag.Unknown);
			base.TopOfStack.\u0001 = IgnoreCheckerMessageFlags.Information;
		}

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x06001FE0 RID: 8160 RVA: 0x0006BFA4 File Offset: 0x0006A1A4
		// (set) Token: 0x06001FE1 RID: 8161 RVA: 0x0006BFAC File Offset: 0x0006A1AC
		internal \u0018.\u0001[] Lazies { get; set; }

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x06001FE2 RID: 8162 RVA: 0x0006BFB8 File Offset: 0x0006A1B8
		public IEnumerable<\u0018.\u0001> ResolvedInfo
		{
			get
			{
				foreach (\u0018.\u0001 u in this.\u0001)
				{
					if (!this.\u0001(u.DerivedType))
					{
						u.DerivedType = this.\u0001(u.DerivedType);
					}
				}
				return this.\u0001;
			}
		}

		// Token: 0x06001FE3 RID: 8163 RVA: 0x0006C024 File Offset: 0x0006A224
		public new IType \u0001(IType \u0002)
		{
			if (\u0002 == null)
			{
				return TypeTable.Int;
			}
			if (\u0002 is _IAnyBitButBoolIsPreferred)
			{
				return TypeTable.Bool;
			}
			switch (\u0002.Class)
			{
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.Lazy:
				return TypeTable.Int;
			case TypeClass.AnyBit:
				return TypeTable.Word;
			case TypeClass.AnyDate:
				return TypeTable.Date;
			case TypeClass.AnyReal:
				return TypeTable.Real;
			case TypeClass.AnyString:
				return TypeTable.String;
			}
			return \u0002;
		}

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x06001FE4 RID: 8164 RVA: 0x0006C0BC File Offset: 0x0006A2BC
		protected override bool KeepAliases { get; } = 1;

		// Token: 0x06001FE5 RID: 8165 RVA: 0x0006C0C4 File Offset: 0x0006A2C4
		public override bool \u0001(_IExprement \u0002, _IExpression \u0003, _IExpression \u0004, ICompiledType \u0005, ICompiledType \u0006, global::\u0015.\u0002 \u0007, global::\u0015.\u0002 \u0008, bool \u000E, out bool \u000F)
		{
			\u000F = false;
			if (\u0005 == null && \u0006 != null && \u0003 is IVariableExpression)
			{
				this.\u0001(new \u0018.\u0001(\u0003.ToString(), \u0003.Position, this.\u0001, \u0006, AccessFlag.Read, VarFlag.Local, this.\u0001.LibraryPath, null, Guid.Empty));
			}
			else if (\u0006 == null && \u0005 != null)
			{
				Guid guid = this.\u0001;
				string text = null;
				VarFlag u0080_u = VarFlag.None;
				if (\u0004 is IVariableExpression)
				{
					text = \u0004.ToString();
					u0080_u = VarFlag.Local;
				}
				else if (\u0004 is IGlobalScopeExpression)
				{
					text = ((IGlobalScopeExpression)\u0004).Base.ToString();
					u0080_u = VarFlag.Global;
				}
				else
				{
					ICompoAccessExpression compoAccessExpression = \u0004 as ICompoAccessExpression;
					if (compoAccessExpression != null)
					{
						guid = this.\u0001(guid, out text, out u0080_u, compoAccessExpression);
					}
				}
				if (text != null)
				{
					_IType u = global::\u0005.\u0004.\u0001(base.Scope, \u0007, \u0005 as _IType);
					this.\u0001(new \u0018.\u0001(text, \u0004.Position, guid, u, AccessFlag.Read, u0080_u, this.\u0001.LibraryPath, null, Guid.Empty));
				}
			}
			return true;
		}

		// Token: 0x06001FE6 RID: 8166 RVA: 0x0006C1C8 File Offset: 0x0006A3C8
		private new Guid \u0001(Guid \u0002, out string \u0003, out VarFlag \u0004, ICompoAccessExpression \u0005)
		{
			\u0003 = \u0005.Right.ToString();
			IVariable[] array;
			ISignature[] array2;
			global::\u0015.\u0002 u;
			base.Scope.\u0001(\u0005.Left.ToString(), out array, out array2, out u);
			if (array != null && 1 == array.Length)
			{
				IUserdefType2 userdefType = array[0].Type as IUserdefType2;
				if (userdefType != null && Helper.InvalidId != userdefType.SignatureId)
				{
					ISignature6 signatureForPrecompileID = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetSignatureForPrecompileID(userdefType.SignatureId);
					\u0002 = signatureForPrecompileID.ObjectGuid;
					Operator poutype = signatureForPrecompileID.POUType;
					if (poutype == Operator.FunctionBlock)
					{
						\u0004 = VarFlag.Input;
						return \u0002;
					}
					if (poutype == Operator.Type)
					{
						\u0004 = VarFlag.Structure;
						return \u0002;
					}
					\u0004 = VarFlag.Local;
					return \u0002;
				}
			}
			\u0004 = VarFlag.Local;
			return \u0002;
		}

		// Token: 0x06001FE7 RID: 8167 RVA: 0x0006C278 File Offset: 0x0006A478
		private new _IType \u0001(_IType \u0002, IVariable \u0003)
		{
			IVarLenArrayTypeInfo2 varLenArrayTypeInfo = APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.TypeInfo as IVarLenArrayTypeInfo2;
			string stType;
			if (varLenArrayTypeInfo != null && varLenArrayTypeInfo.IsVarLenArray(\u0003, out stType))
			{
				IMessage message;
				return (_IType)APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder().CreateComplexType(stType, out message);
			}
			return \u0002;
		}

		// Token: 0x06001FE8 RID: 8168 RVA: 0x0006C2D0 File Offset: 0x0006A4D0
		public override void \u0001(_ICallExpression \u0002, _ISignature \u0003, int \u0004, _IVariable \u0005, _IExpression \u0006, _IType \u0007, IVariable \u0008, global::\u0015.\u0002 \u000E, global::\u0015.\u0002 \u000F)
		{
			if (\u0005 == null)
			{
				return;
			}
			if (\u0004 >= \u0002.ParamExpressions.Count<_IExpression>())
			{
				return;
			}
			if (\u0007 != null || \u0005.Type == null)
			{
				return;
			}
			IExpression expression = \u0002.ParamExpressions[\u0004];
			if (expression is IVariableExpression)
			{
				_IType itype = global::\u0005.\u0004.\u0001(base.Scope, base.Scope.\u0001(\u0003), \u0005._Type);
				itype = this.\u0001(itype, \u0005);
				IUserdefType2 userdefType = \u0005._Type as IUserdefType2;
				if (userdefType != null)
				{
					_ISystemScopeExpression isystemScopeExpression = userdefType.NameExpression as _ISystemScopeExpression;
					if (isystemScopeExpression != null)
					{
						itype = global::\u0019.\u0003.\u0001(isystemScopeExpression);
					}
				}
				this.\u0001(new \u0018.\u0001(expression.ToString(), expression.Position, this.\u0001, itype, AccessFlag.Read, VarFlag.Local, this.\u0001.LibraryPath, null, Guid.Empty));
				return;
			}
			_IOperatorExpression ioperatorExpression = expression as _IOperatorExpression;
			if (ioperatorExpression != null && Operator.Adr == ioperatorExpression.Code && \u0005.Type.Class == TypeClass.Pointer && ioperatorExpression._OperandsList != null && ioperatorExpression._OperandsList.Count == 1 && ioperatorExpression._OperandsList[0] is _IVariableExpression)
			{
				_IPointerType ipointerType = \u0005.Type as _IPointerType;
				if (((ipointerType != null) ? ipointerType._Base : null) != null)
				{
					string u0082_u = null;
					if (!string.IsNullOrEmpty(\u0003.LibraryPath))
					{
						u0082_u = base.Scope.\u0001(\u0003.LibraryPath);
					}
					this.\u0001(new \u0018.\u0001(ioperatorExpression._OperandsList[0].ToString(), ioperatorExpression._OperandsList[0].Position, this.\u0001, ipointerType._Base, AccessFlag.Read, VarFlag.Local, this.\u0001.LibraryPath, u0082_u, Guid.Empty));
				}
			}
		}

		// Token: 0x06001FE9 RID: 8169 RVA: 0x0006C490 File Offset: 0x0006A690
		public override void \u0001(_IVariableExpression \u0002)
		{
			base.\u0001(\u0002);
			if (base.TopOfStack.typeResolved != null && base.TopOfStack.typeResolved.Class == TypeClass.Lazy && this.Lazies != null)
			{
				foreach (\u0018.\u0001 declarationInfo in this.Lazies)
				{
					if (declarationInfo.Name == \u0002.Name)
					{
						this.\u0001(base.Scope, declarationInfo.DerivedType as _IType);
						return;
					}
				}
			}
			Guid u001F_u;
			VarFlag u0080_u;
			Guid u0083_u;
			this.\u0001(out u001F_u, out u0080_u, out u0083_u);
			if (base.TopOfStack.typeResolved == null && base.TopOfStack.\u0001 == null && base.TopOfStack.\u0004 == null && base.TopOfStack.\u0001 == null)
			{
				IUserdefType2 userdefType = base.TopOfStack.\u0001 as IUserdefType2;
				_IType u;
				if (userdefType != null)
				{
					_ISystemScopeExpression isystemScopeExpression = userdefType.NameExpression as _ISystemScopeExpression;
					if (isystemScopeExpression != null)
					{
						u = global::\u0019.\u0003.\u0001(isystemScopeExpression);
						goto IL_11E;
					}
				}
				u = global::\u0005.\u0004.\u0001(base.Scope, base.TopOfStack.\u0001, base.TopOfStack.\u0001);
				IL_11E:
				if (!string.IsNullOrEmpty(base.TopOfStack.\u0001))
				{
					IMessage message;
					u = (_IType)APEnvironmentFacade.Instance.LMServiceProvider.CreatorService.CreateLanguageModelBuilder().CreateComplexType(base.TopOfStack.\u0001, out message);
				}
				this.\u0001(new \u0018.\u0001(\u0002.ToString(), \u0002.Position, u001F_u, u, AccessFlag.Read, u0080_u, this.\u0001.LibraryPath, null, u0083_u));
			}
		}

		// Token: 0x06001FEA RID: 8170 RVA: 0x0006C624 File Offset: 0x0006A824
		public override void \u0001(_IOperatorExpression \u0002)
		{
			IList<_IExpression> operandsList = \u0002._OperandsList;
			LList<_IVariable> u = new LList<_IVariable>();
			LList<_ISignature> u2 = new LList<_ISignature>();
			_IType itype = base.TopOfStack.\u0001;
			if (itype == null)
			{
				\u0002.AcceptOperatorVisitor(this.\u0001);
				itype = this.\u0001.PreferredType;
			}
			SimpleTypeChecker simpleTypeChecker = new global::\u0001.\u0007(this.\u0001, this.\u0001, this.\u0001, false);
			\u0002.Accept(simpleTypeChecker);
			IList<_IType> u3 = simpleTypeChecker.OperatorInfo.\u0001;
			_IType itype2 = u3.FirstOrDefault(new Func<_IType, bool>(SimpleTypeInferrer.<>c.<>9.\u0001));
			_IType itype3 = this.\u0001(\u0002, itype2 ?? itype);
			for (int i = 0; i < operandsList.Count; i++)
			{
				if (u3[i] == null)
				{
					_IExprement iexprement = operandsList[i];
					if (itype3 == null || this.\u0001(itype3, itype2))
					{
						itype3 = this.\u0001(\u0002, itype2);
					}
					base.\u0001(AccessFlag.Read);
					base.TopOfStack.\u0001 = itype3;
					iexprement.Accept(this);
					itype3 = base.TopOfStack.\u0001;
					base.\u0002();
				}
			}
			if (itype == null)
			{
				itype = itype3;
			}
			this.\u0001(\u0002, u3, itype, operandsList);
			_IType itype4 = base.\u0001(\u0002, u3);
			this.\u0001(null, itype4);
			if (itype4 != null)
			{
				base.TopOfStack.\u0001 = itype4;
			}
			base.TopOfStack.\u0001 = new global::\u0004.\u0006
			{
				\u0001 = operandsList,
				\u0001 = u2,
				\u0001 = u3,
				\u0001 = u,
				\u0001 = itype4
			};
			if (u3.All(new Func<_IType, bool>(SimpleTypeInferrer.<>c.<>9.\u0002)))
			{
				\u0002.AcceptOperatorVisitor(this);
			}
			base.TopOfStack.\u0001 = null;
			base.TopOfStack.\u0001 = null;
			base.TopOfStack.\u0004 = null;
		}

		// Token: 0x06001FEB RID: 8171 RVA: 0x0006C804 File Offset: 0x0006AA04
		private new void \u0001(_IOperatorExpression \u0002, IList<_IType> \u0003, _IType \u0004, IList<_IExpression> \u0005)
		{
			Operator code = \u0002.Code;
			if (code - Operator.Add <= 1 || code - Operator.Plus <= 1)
			{
				if (\u0003[0] == null && \u0003[1] != null)
				{
					bool flag = \u0003[1].Class == TypeClass.TimeOfDay || \u0003[1].Class == TypeClass.DateAndTime || \u0003[1].Class == TypeClass.Date;
					bool flag2 = \u0003[1].Class == TypeClass.LTimeOfDay || \u0003[1].Class == TypeClass.LDateAndTime || \u0003[1].Class == TypeClass.LDate;
					if (flag || flag2)
					{
						base.\u0001(AccessFlag.Read);
						if (flag2)
						{
							if (\u0004.Class == TypeClass.LTime)
							{
								base.TopOfStack.\u0001 = TypeTable.Get(\u0003[1].Class);
							}
							else
							{
								base.TopOfStack.\u0001 = TypeTable.Get(TypeClass.LTime);
							}
						}
						else if (\u0004.Class == TypeClass.Time)
						{
							base.TopOfStack.\u0001 = TypeTable.Get(\u0003[1].Class);
						}
						else
						{
							base.TopOfStack.\u0001 = TypeTable.Get(TypeClass.Time);
						}
						\u0005[0].Accept(this);
						base.\u0002();
						return;
					}
				}
				else if (\u0003[1] == null && \u0003[0] != null)
				{
					bool flag3 = \u0003[0].Class == TypeClass.TimeOfDay || \u0003[0].Class == TypeClass.DateAndTime || \u0003[0].Class == TypeClass.Date;
					bool flag4 = \u0003[0].Class == TypeClass.LTimeOfDay || \u0003[0].Class == TypeClass.LDateAndTime || \u0003[0].Class == TypeClass.LDate;
					if (flag3 || flag4)
					{
						base.\u0001(AccessFlag.Read);
						if (flag4)
						{
							if (\u0004.Class == TypeClass.LTime)
							{
								base.TopOfStack.\u0001 = TypeTable.Get(\u0003[0].Class);
							}
							else
							{
								base.TopOfStack.\u0001 = TypeTable.Get(TypeClass.LTime);
							}
						}
						else if (\u0004.Class == TypeClass.Time)
						{
							base.TopOfStack.\u0001 = TypeTable.Get(\u0003[0].Class);
						}
						else
						{
							base.TopOfStack.\u0001 = TypeTable.Get(TypeClass.Time);
						}
						\u0005[1].Accept(this);
						base.\u0002();
					}
				}
			}
		}

		// Token: 0x06001FEC RID: 8172 RVA: 0x0006CA64 File Offset: 0x0006AC64
		private new _IType \u0001(_IOperatorExpression \u0002, _IType \u0003)
		{
			if (\u0002.Code == Operator.Adr && \u0003 != null && \u0003.Class == TypeClass.Pointer)
			{
				\u0003 = (\u0003.BaseType as _IType);
			}
			return \u0003;
		}

		// Token: 0x06001FED RID: 8173 RVA: 0x0006CA8C File Offset: 0x0006AC8C
		public new bool \u0001(IType \u0002)
		{
			return \u0002 != null && TypeTable.IsConcreteType(\u0002.Class);
		}

		// Token: 0x06001FEE RID: 8174 RVA: 0x0006CAA0 File Offset: 0x0006ACA0
		public new bool \u0001(IType \u0002, IType \u0003)
		{
			return \u0003 != null && (\u0002 == null || TypeTable.IsConcreterType(\u0002.Class, \u0003.Class));
		}

		// Token: 0x06001FEF RID: 8175 RVA: 0x0006CAC0 File Offset: 0x0006ACC0
		private new void \u0001(\u0018.\u0001 \u0002)
		{
			bool flag = true;
			for (int i = 0; i < this.\u0001.Count; i++)
			{
				\u0018.\u0001 u = this.\u0001[i];
				if (u.Name == \u0002.Name && u.ObjectGuid == \u0002.ObjectGuid)
				{
					IType u2 = u.DerivedType;
					if (!this.\u0001(u.DerivedType) && this.\u0001(u.DerivedType, \u0002.DerivedType))
					{
						u2 = \u0002.DerivedType;
					}
					else if (!this.\u0001(\u0002.DerivedType))
					{
						u2 = u.DerivedType;
					}
					else if (TypeTable.IsInteger(u.DerivedType.Class) && TypeTable.IsInteger(\u0002.DerivedType.Class))
					{
						int size = TypeTable.GetSize(u.DerivedType.Class, null);
						if (TypeTable.GetSize(\u0002.DerivedType.Class, null) > size)
						{
							u2 = \u0002.DerivedType;
						}
					}
					else
					{
						u2 = \u0002.DerivedType;
					}
					if (u.Access == AccessFlag.Unknown)
					{
						u.Access = \u0002.Access;
					}
					u.DerivedType = u2;
					if (u.VarFlag == VarFlag.Local)
					{
						u.VarFlag = \u0002.VarFlag;
					}
					if (string.IsNullOrEmpty(u.Namespace))
					{
						u.Namespace = \u0002.Namespace;
					}
					flag = false;
				}
			}
			if (flag)
			{
				this.\u0001.Add(\u0002);
			}
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x0006CC28 File Offset: 0x0006AE28
		private new void \u0001(out Guid \u0002, out VarFlag \u0003, out Guid \u0004)
		{
			\u0004 = Guid.Empty;
			\u0003 = base.TopOfStack.\u0001;
			\u0002 = this.\u0001;
			global::\u0015.\u0002 u = base.Scope;
			ISignature signature = (u != null) ? u.MostLocalSignature : null;
			if (signature == null)
			{
				return;
			}
			Operator poutype = signature.POUType;
			if (poutype != Operator.FunctionBlock)
			{
				if (poutype == Operator.Type)
				{
					\u0002 = signature.ObjectGuid;
					\u0003 = VarFlag.Structure;
					return;
				}
				if (poutype == Operator.VarGlobal)
				{
					\u0004 = signature.ObjectGuid;
					\u0003 = VarFlag.Global;
					return;
				}
			}
			else if (this.\u0001 != signature.ObjectGuid)
			{
				\u0002 = signature.ObjectGuid;
				\u0003 = VarFlag.Input;
			}
		}

		// Token: 0x04000536 RID: 1334
		private new readonly LList<\u0018.\u0001> \u0001 = new LList<\u0018.\u0001>();

		// Token: 0x04000537 RID: 1335
		private new readonly global::\u0006.\u0005 \u0001 = new global::\u0006.\u0005();

		// Token: 0x04000538 RID: 1336
		[CompilerGenerated]
		private new \u0018.\u0001[] \u0001;

		// Token: 0x04000539 RID: 1337
		[CompilerGenerated]
		private new readonly bool \u0001;
	}
}
