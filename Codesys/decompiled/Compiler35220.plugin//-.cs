using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0002;
using \u0017;
using \u0019;
using \u001E;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0080
{
	// Token: 0x020000AA RID: 170
	internal sealed class \u0001 : DummyBaseClass, IExprementVisitor<ConstantFoldingResult>
	{
		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06000D81 RID: 3457 RVA: 0x00023BAC File Offset: 0x00021DAC
		// (set) Token: 0x06000D82 RID: 3458 RVA: 0x00023BB4 File Offset: 0x00021DB4
		private global::\u0017.\u0006 Scope { get; set; }

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06000D83 RID: 3459 RVA: 0x00023BC0 File Offset: 0x00021DC0
		private global::\u0002.\u0002 ConstantFolderService { get; }

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x00023BC8 File Offset: 0x00021DC8
		private IRecursionGuard RecursionGuard { get; }

		// Token: 0x06000D85 RID: 3461 RVA: 0x00023BD0 File Offset: 0x00021DD0
		private \u0001(ICommonScope \u009B\u0002, global::\u0002.\u0002 \u001D\u0008, IRecursionGuard \u001A\u0008)
		{
			this.Scope = (global::\u0017.\u0006)\u009B\u0002;
			this.ConstantFolderService = \u001D\u0008;
			this.RecursionGuard = \u001A\u0008;
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x00023BF4 File Offset: 0x00021DF4
		internal static _ILiteralValue \u0001(_IExpression2 \u0002, global::\u0002.\u0002 \u0003, ICommonScope \u0004, IRecursionGuard \u0005)
		{
			\u0080.\u0001 u = new \u0080.\u0001(\u0004, \u0003, \u0005);
			_IExprement3 iexprement = \u0002 as _IExprement3;
			if (iexprement == null)
			{
				return null;
			}
			if (!u.Scope.\u0001(\u0002))
			{
				return null;
			}
			ConstantFoldingResult constantFoldingResult = iexprement.Accept<ConstantFoldingResult>(u);
			if (constantFoldingResult == null)
			{
				return null;
			}
			return constantFoldingResult._literalValue;
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x00023C3C File Offset: 0x00021E3C
		private static ConstantFoldingResult \u0001(_IExpression2 \u0002, global::\u0002.\u0002 \u0003, ICommonScope \u0004, IRecursionGuard \u0005)
		{
			\u0080.\u0001 u = new \u0080.\u0001(\u0004, \u0003, \u0005);
			_IExprement3 iexprement = \u0002 as _IExprement3;
			if (iexprement == null)
			{
				return null;
			}
			if (!u.Scope.\u0001(\u0002))
			{
				return null;
			}
			return iexprement.Accept<ConstantFoldingResult>(u);
		}

		// Token: 0x06000D88 RID: 3464 RVA: 0x00023C78 File Offset: 0x00021E78
		private ConstantFoldingResult \u0001(_IExpression \u0002, global::\u0017.\u0006 \u0003 = null)
		{
			global::\u0017.\u0006 u = null;
			if (\u0003 != null)
			{
				u = this.Scope;
				this.Scope = \u0003;
			}
			ConstantFoldingResult result = ((_IExprement3)\u0002).Accept<ConstantFoldingResult>(this);
			if (u != null)
			{
				this.Scope = u;
			}
			return result;
		}

		// Token: 0x06000D89 RID: 3465 RVA: 0x00023CB0 File Offset: 0x00021EB0
		private bool \u0001(_IIndexAccessExpression \u0002, ILiteralValue \u0003, out _ILiteralValue \u0004)
		{
			\u0004 = null;
			if (\u0003 == null || \u0002._Accesses.Count != 1)
			{
				return false;
			}
			ConstantFoldingResult constantFoldingResult = this.\u0001((_IExpression2)\u0002.GetAccess(0), null);
			if (constantFoldingResult == null)
			{
				\u0004 = null;
				return true;
			}
			int index;
			if (!constantFoldingResult._literalValue.GetInt(out index))
			{
				\u0004 = null;
				return true;
			}
			string text;
			if (!\u0003.GetString(out text))
			{
				\u0004 = null;
				return true;
			}
			\u0004 = global::\u0019.\u0003.\u0001((long)((ulong)text[index]));
			return true;
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x00023D24 File Offset: 0x00021F24
		private IRecursionGuard \u0001(IVariable \u0002)
		{
			IRecursionGuard recursionGuard = this.RecursionGuard.Duplicate();
			if (\u0002 != null && this.RecursionGuard.Has(\u0002))
			{
				throw new RecursiveConstantException(\u0081.\u0002.Err_RecursiveConstantInitialisation);
			}
			if (\u0002 != null)
			{
				recursionGuard.Add(\u0002);
			}
			return recursionGuard;
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x00023D64 File Offset: 0x00021F64
		private ConstantFoldingResult \u0001(_IExpression \u0002, IRecursionGuard \u0003)
		{
			_IArrayInitialization iarrayInitialization = \u0002 as _IArrayInitialization;
			if (iarrayInitialization != null)
			{
				return new ConstantFoldingResult(iarrayInitialization);
			}
			_IStructureInitialization istructureInitialization = \u0002 as _IStructureInitialization;
			if (istructureInitialization == null)
			{
				return \u0080.\u0001.\u0001((_IExpression2)\u0002, this.ConstantFolderService, this.Scope, \u0003);
			}
			return new ConstantFoldingResult(istructureInitialization);
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x00023DB0 File Offset: 0x00021FB0
		private _IExpression \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003 == null)
			{
				return null;
			}
			_IExpression iexpression = this.Scope.\u0001(\u0002, \u0003);
			if (iexpression == null)
			{
				return this.\u0001(\u0003, this.Scope);
			}
			return iexpression;
		}

		// Token: 0x06000D8D RID: 3469 RVA: 0x00023DE4 File Offset: 0x00021FE4
		private _IExpression \u0001(IVariable \u0002, ICommonScope \u0003)
		{
			if (\u0002.Type is IEnumType)
			{
				ILMPreCompileService5 ilmpreCompileService = (ILMPreCompileService5)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService;
				ILiteralValue literalValue = (ilmpreCompileService != null) ? ilmpreCompileService.GetEnumInitValue(\u0002, this.Scope) : null;
				if (literalValue == null)
				{
					return null;
				}
				return ConstantFoldingHelper.\u0001(literalValue);
			}
			else
			{
				_ISignature isignature = this.\u0001((_IVariable2)\u0002, \u0003);
				if (isignature == null)
				{
					return null;
				}
				IList<_IVariable> allVariables = isignature.AllVariables;
				if (!allVariables.Any<_IVariable>())
				{
					return null;
				}
				return allVariables[0]._Initial;
			}
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x00023E64 File Offset: 0x00022064
		private _ISignature \u0001(_IVariable2 \u0002, ICommonScope \u0003)
		{
			_IAliasType ialiasType = \u0002.CompiledTypeInternal as _IAliasType;
			if (ialiasType != null)
			{
				return (_ISignature)\u0003.FindSignature(ialiasType);
			}
			_IUserdefType iuserdefType = \u0002.CompiledTypeInternal as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISignature isignature = (_ISignature)\u0003.FindSignature(iuserdefType);
				if (isignature != null && isignature.GetFlag(SignatureFlag.Alias))
				{
					return isignature;
				}
			}
			return null;
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x00023EBC File Offset: 0x000220BC
		private bool \u0001(_IExpression \u0002, out _IVariable \u0003, out IArrayType \u0004)
		{
			\u0003 = this.Scope.\u0001(\u0002);
			if (\u0003 != null)
			{
				IArrayType arrayType = \u0003.Type as IArrayType;
				if (arrayType != null)
				{
					\u0004 = arrayType;
					return true;
				}
			}
			\u0004 = null;
			return false;
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x00023EF4 File Offset: 0x000220F4
		private ConstantFoldingResult \u0001(_IIndexAccessExpression \u0002, _IVariable \u0003, _IArrayInitialization \u0004, IArrayType \u0005)
		{
			int num = 0;
			int num2 = 1;
			if (\u0005.Dimensions.Length != \u0002._Accesses.Count)
			{
				return null;
			}
			IRecursionGuard recursionGuard = this.\u0001(\u0003);
			bool flag;
			IList<_IExpression> list = ConstantFoldingHelper.\u0001(this.ConstantFolderService, \u0004, this.Scope, recursionGuard, out flag);
			if (list == null || !flag)
			{
				return null;
			}
			for (int i = \u0005.Dimensions.Length - 1; i >= 0; i--)
			{
				int num3;
				if (!this.\u0001(\u0002, i, out num3))
				{
					return null;
				}
				IArrayDimension u = \u0005.Dimensions[i];
				int num4;
				int num5;
				if (!this.\u0001(u, recursionGuard, out num4, out num5))
				{
					return null;
				}
				num += (num3 - num4) * num2;
				num2 *= num5;
			}
			if (list.Count <= num || num < 0)
			{
				return null;
			}
			_IExpression2 u2 = (_IExpression2)list[num];
			return this.\u0001(u2, recursionGuard);
		}

		// Token: 0x06000D91 RID: 3473 RVA: 0x00023FC0 File Offset: 0x000221C0
		private bool \u0001(IArrayDimension \u0002, IRecursionGuard \u0003, out int \u0004, out int \u0005)
		{
			\u0004 = -1;
			\u0005 = -1;
			if (!this.ConstantFolderService.\u0001((_IExpression2)\u0002.LowerBorder, this.Scope, \u0003, out \u0004))
			{
				return false;
			}
			int num;
			if (!this.ConstantFolderService.\u0001((_IExpression2)\u0002.UpperBorder, this.Scope, \u0003, out num))
			{
				return false;
			}
			\u0005 = num - \u0004 + 1;
			return \u0005 >= 0;
		}

		// Token: 0x06000D92 RID: 3474 RVA: 0x0002402C File Offset: 0x0002222C
		private bool \u0001(_IIndexAccessExpression \u0002, int \u0003, out int \u0004)
		{
			\u0004 = -1;
			ConstantFoldingResult constantFoldingResult = this.\u0001((_IExpression2)\u0002.GetAccess(\u0003), null);
			if (constantFoldingResult == null)
			{
				return false;
			}
			ILiteralValue literalValue = constantFoldingResult._literalValue;
			return literalValue != null && literalValue.GetInt(out \u0004);
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x00024070 File Offset: 0x00022270
		private static bool \u0001(_ILiteralValue \u0002, _ILiteralValue \u0003, out _ILiteralValue \u0004)
		{
			\u0004 = null;
			if (\u0002 == null)
			{
				return false;
			}
			if (\u0003 == null)
			{
				\u0004 = \u0002;
				return true;
			}
			int num;
			if (!\u0002.GetInt(out num))
			{
				\u0004 = \u0002;
				return true;
			}
			int num2;
			if (!\u0003.GetInt(out num2))
			{
				\u0004 = \u0002;
				return true;
			}
			bool u = (num2 >> num & 1) != 0;
			\u0004 = global::\u0019.\u0003.\u0001(u);
			return true;
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x000240C0 File Offset: 0x000222C0
		private ConstantFoldingResult \u0001(_IStructureInitialization \u0002, bool \u0003, global::\u0017.\u0006 \u0004, _IExpression \u0005, bool \u0006, _IExpression \u0007)
		{
			ConstantFoldingResult constantFoldingResult = null;
			if (\u0003)
			{
				if (this.Scope.\u0001(\u0005) && !\u0006 && !this.Scope.\u0001(\u0007))
				{
					IVariable variable = this.\u0001(\u0002, \u0005, \u0004, \u0007, ref constantFoldingResult);
					if (constantFoldingResult == null && variable != null && variable.Initial != null)
					{
						_IExpression iexpression = variable.Initial as _IExpression;
						if (iexpression != null)
						{
							IRecursionGuard u = this.\u0001(variable);
							return this.\u0001(iexpression, u);
						}
					}
				}
				return constantFoldingResult;
			}
			return null;
		}

		// Token: 0x06000D95 RID: 3477 RVA: 0x00024134 File Offset: 0x00022334
		private IVariable \u0001(_IStructureInitialization \u0002, _IExpression \u0003, global::\u0017.\u0006 \u0004, _IExpression \u0005, ref ConstantFoldingResult \u0006)
		{
			IVariable variable = this.Scope.\u0001(\u0003);
			if (variable == null)
			{
				return null;
			}
			IVariable variable2 = \u0004.\u0001(\u0005);
			if (variable.Initial != null && variable2 != null && \u0002 != null)
			{
				foreach (_IAssignmentExpression iassignmentExpression in \u0002._CompoInits)
				{
					IVariable variable3 = \u0004.\u0001(iassignmentExpression._LValue);
					if (variable3 != null && string.Equals(iassignmentExpression._LValue.ToString(), variable2.Name, StringComparison.OrdinalIgnoreCase))
					{
						IRecursionGuard u = this.\u0001(variable3);
						\u0006 = this.\u0001(iassignmentExpression._RValue, u);
						break;
					}
				}
			}
			return variable2;
		}

		// Token: 0x06000D96 RID: 3478 RVA: 0x000241EC File Offset: 0x000223EC
		public ConstantFoldingResult \u0001(_IOperatorExpression \u0002)
		{
			bool flag;
			_ILiteralValue literalValue = (_ILiteralValue)this.ConstantFolderService.\u0001(\u0002, this.Scope, this.RecursionGuard, true, out flag);
			if (flag)
			{
				throw new RecursiveConstantException(\u0081.\u0002.Err_RecursiveConstantInitialisation);
			}
			return new ConstantFoldingResult(literalValue);
		}

		// Token: 0x06000D97 RID: 3479 RVA: 0x0002422C File Offset: 0x0002242C
		public ConstantFoldingResult \u0001(_IConversionExpression \u0002)
		{
			ConstantFoldingResult constantFoldingResult = this.\u0001(\u0002._Exp, null);
			if (constantFoldingResult == null)
			{
				return null;
			}
			return new ConstantFoldingResult(\u001E.\u0004.\u0001(\u0002, constantFoldingResult._literalValue));
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x00024260 File Offset: 0x00022460
		public ConstantFoldingResult \u0001(_ICurrentTaskExpression \u0002)
		{
			return this.\u0001(\u0002._Base, null);
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x00024270 File Offset: 0x00022470
		public ConstantFoldingResult \u0001(_ILiteralExpression \u0002)
		{
			return new ConstantFoldingResult((_ILiteralValue)\u0002.LiteralValue);
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x00024284 File Offset: 0x00022484
		public ConstantFoldingResult \u0001(_IVariableExpression \u0002)
		{
			_IVariable ivariable = this.Scope.\u0001(\u0002);
			_ISignature u = this.Scope.\u0001(\u0002);
			IRecursionGuard u2 = this.\u0001(ivariable);
			if (ivariable != null && ivariable.HasAttribute("system_variable") && ivariable.Name == "__CONTAINS_COPY_CODE")
			{
				return new ConstantFoldingResult(global::\u0019.\u0003.\u0001(this.Scope.ContainsCopyCode));
			}
			_IExpression iexpression = this.\u0001(u, ivariable);
			if (iexpression == null)
			{
				return null;
			}
			return this.\u0001(iexpression, u2);
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x00024304 File Offset: 0x00022504
		public ConstantFoldingResult \u0001(_IIndexAccessExpression \u0002)
		{
			_IExpression var = \u0002._Var;
			ConstantFoldingResult constantFoldingResult = this.\u0001(\u0002._Var, null);
			if (constantFoldingResult == null)
			{
				return null;
			}
			_ILiteralValue literalValue = constantFoldingResult._literalValue;
			_IVariable u;
			IArrayType u2;
			if (constantFoldingResult._arrayInitialization != null && this.\u0001(var, out u, out u2))
			{
				return this.\u0001(\u0002, u, constantFoldingResult._arrayInitialization, u2);
			}
			_ILiteralValue literalValue2;
			if (this.\u0001(\u0002, literalValue, out literalValue2))
			{
				return new ConstantFoldingResult(literalValue2);
			}
			return null;
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x00024370 File Offset: 0x00022570
		public ConstantFoldingResult \u0001(_ICompoAccessExpression \u0002)
		{
			_IExpression left = \u0002._Left;
			_IExpression right = \u0002._Right;
			ConstantFoldingResult constantFoldingResult = this.\u0001(left, null);
			_IUserdefType iuserdefType = left.Type as _IUserdefType;
			global::\u0017.\u0006 u = this.Scope;
			if (iuserdefType != null)
			{
				u = this.Scope.\u0001(left, iuserdefType);
			}
			else
			{
				_IVariable ivariable;
				_ISignature isignature;
				global::\u0017.\u0006 u2;
				this.Scope.\u0001(left, out ivariable, out isignature, out u2);
				if (ivariable != null)
				{
					_IUserdefType iuserdefType2 = ivariable.Type as _IUserdefType;
					if (iuserdefType2 != null)
					{
						iuserdefType = iuserdefType2;
						u = this.Scope.\u0001(left, iuserdefType);
						goto IL_9C;
					}
				}
				if (ivariable == null && isignature != null)
				{
					u = this.Scope.\u0001(isignature);
				}
				else if (u2 != null)
				{
					u = u2;
				}
			}
			IL_9C:
			bool u3 = iuserdefType != null;
			bool u4 = left.Type != null && left.Type.Class == TypeClass.Reference;
			if (((constantFoldingResult != null) ? constantFoldingResult._structureInitialization : null) != null)
			{
				return this.\u0001(constantFoldingResult._structureInitialization, u3, u, left, u4, right);
			}
			ConstantFoldingResult constantFoldingResult2 = this.\u0001(right, u);
			_ILiteralValue literalValue;
			if (((constantFoldingResult2 != null) ? constantFoldingResult2._literalValue : null) != null && ((constantFoldingResult != null) ? constantFoldingResult._literalValue : null) != null && \u0080.\u0001.\u0001(constantFoldingResult2._literalValue, constantFoldingResult._literalValue, out literalValue))
			{
				return new ConstantFoldingResult(literalValue);
			}
			return constantFoldingResult2;
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x000244A8 File Offset: 0x000226A8
		public ConstantFoldingResult \u0001(_ICopyScopeExpression \u0002)
		{
			return this.\u0001(\u0002._Base, null);
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x000244B8 File Offset: 0x000226B8
		public ConstantFoldingResult \u0001(_IGlobalScopeExpression \u0002)
		{
			return this.\u0001(\u0002._Base, null);
		}

		// Token: 0x06000D9F RID: 3487 RVA: 0x000244C8 File Offset: 0x000226C8
		public ConstantFoldingResult \u0001(_ISystemScopeExpression \u0002)
		{
			return this.\u0001(\u0002._Base, null);
		}

		// Token: 0x06000DA0 RID: 3488 RVA: 0x000244D8 File Offset: 0x000226D8
		public ConstantFoldingResult \u0001(_IPoolScopeExpression \u0002)
		{
			return this.\u0001(\u0002._Base, null);
		}

		// Token: 0x06000DA1 RID: 3489 RVA: 0x000244E8 File Offset: 0x000226E8
		public ConstantFoldingResult \u0001(_INamespaceAccessExpression \u0002)
		{
			return this.\u0001(\u0002._Access, null);
		}

		// Token: 0x06000DA2 RID: 3490 RVA: 0x000244F8 File Offset: 0x000226F8
		public ConstantFoldingResult \u0001(_IPartialAccessExpression \u0002)
		{
			ConstantFoldingResult constantFoldingResult = this.\u0001(\u0002._Left, null);
			if (constantFoldingResult._literalValue == null)
			{
				return null;
			}
			int partOffset = \u0002.PartOffset;
			DirectVariableSize partSize = \u0002.PartSize;
			return new ConstantFoldingResult(PartialAccessConstantFolder.CreateLiteralValue(constantFoldingResult._literalValue, partOffset, partSize));
		}

		// Token: 0x0400024A RID: 586
		[CompilerGenerated]
		private global::\u0017.\u0006 \u0001;

		// Token: 0x0400024B RID: 587
		[CompilerGenerated]
		private readonly global::\u0002.\u0002 \u0001;

		// Token: 0x0400024C RID: 588
		[CompilerGenerated]
		private readonly IRecursionGuard \u0001;
	}
}
