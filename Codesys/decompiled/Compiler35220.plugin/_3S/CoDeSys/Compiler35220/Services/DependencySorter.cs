using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000B9 RID: 185
	internal sealed class DependencySorter
	{
		// Token: 0x06000E54 RID: 3668 RVA: 0x00026D78 File Offset: 0x00024F78
		internal bool \u0001(_ICompileContext \u0002)
		{
			if (!this.\u0002(\u0002))
			{
				return false;
			}
			DependencySorter.\u0001(\u0002);
			return true;
		}

		// Token: 0x06000E55 RID: 3669 RVA: 0x00026D8C File Offset: 0x00024F8C
		private bool \u0002(_ICompileContext \u0002)
		{
			IList<_ISignature> pousignaturesEx = \u0002.POUSignaturesEx;
			LList<_ISignature> llist = new LList<_ISignature>(pousignaturesEx.Count);
			LDictionary<int, _ISignature> u = new LDictionary<int, _ISignature>(pousignaturesEx.Count);
			LStack<_ISignature> u2 = new LStack<_ISignature>();
			foreach (_ISignature u3 in pousignaturesEx)
			{
				if (!this.\u0001(\u0002, u3, llist, u, u2))
				{
					return false;
				}
			}
			\u0002.SetPOUSignaturesEx(llist);
			return true;
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x00026E10 File Offset: 0x00025010
		private static void \u0001(_ICompileContext \u0002)
		{
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					if (!ivariable.IsProperty)
					{
						if (ivariable.GetFlag(VarFlag.Retain))
						{
							isignature.SetFlag(SignatureFlag.ContainsRetain, true);
						}
						if (ivariable.GetFlag(VarFlag.LocalPersistent))
						{
							isignature.SetFlag(SignatureFlag.ContainsPersistent, true);
						}
					}
				}
			}
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00026ED8 File Offset: 0x000250D8
		private static void \u0001(_IScope \u0002, ICompiledType \u0003, LList<_ISignature> \u0004, bool \u0005, out ICompiledType \u0006)
		{
			TypeClass @class;
			for (;;)
			{
				@class = \u0003.Class;
				switch (@class)
				{
				case TypeClass.String:
					goto IL_82;
				case TypeClass.WString:
					goto IL_99;
				case TypeClass.Time:
				case TypeClass.Date:
				case TypeClass.DateAndTime:
				case TypeClass.TimeOfDay:
				case TypeClass.Enum:
					goto IL_100;
				case TypeClass.Pointer:
				{
					if (!\u0005)
					{
						goto IL_100;
					}
					_IPointerType2 ipointerType = \u0003 as _IPointerType2;
					if (ipointerType != null)
					{
						\u0003 = ipointerType.OriginalBase;
						continue;
					}
					goto IL_100;
				}
				case TypeClass.Reference:
				{
					if (!\u0005)
					{
						goto IL_100;
					}
					_IReferenceType ireferenceType = \u0003 as _IReferenceType;
					if (ireferenceType != null)
					{
						\u0003 = ireferenceType.OriginalBase;
						continue;
					}
					goto IL_100;
				}
				case TypeClass.Subrange:
					goto IL_59;
				case TypeClass.Array:
				{
					_IArrayType u = \u0003 as _IArrayType;
					\u0003 = DependencySorter.\u0001(\u0002, \u0004, u);
					continue;
				}
				}
				break;
			}
			if (@class != TypeClass.__Vector)
			{
				goto IL_100;
			}
			_IVectorType ivectorType = \u0003 as _IVectorType;
			DependencySorter.\u0001.\u0001(ivectorType._Dimension, \u0004, \u0002);
			\u0003 = ivectorType._Base;
			goto IL_100;
			IL_59:
			_ISubrangeType isubrangeType = \u0003 as _ISubrangeType;
			DependencySorter.\u0001.\u0001(isubrangeType._LowerBorder, \u0004, \u0002);
			DependencySorter.\u0001.\u0001(isubrangeType._UpperBorder, \u0004, \u0002);
			\u0003 = isubrangeType._Base;
			goto IL_100;
			IL_82:
			DependencySorter.\u0001.\u0001((\u0003 as _IStringType).Length, \u0004, \u0002);
			\u0003 = null;
			goto IL_100;
			IL_99:
			DependencySorter.\u0001.\u0001((\u0003 as _IWStringType).Length, \u0004, \u0002);
			\u0003 = null;
			IL_100:
			\u0006 = \u0003;
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x00026FEC File Offset: 0x000251EC
		private static ICompiledType \u0001(_IScope \u0002, LList<_ISignature> \u0003, _IArrayType \u0004)
		{
			foreach (_IArrayDimension iarrayDimension in \u0004._Dimensions)
			{
				DependencySorter.\u0001.\u0001(iarrayDimension._LowerBorder, \u0003, \u0002);
				DependencySorter.\u0001.\u0001(iarrayDimension._UpperBorder, \u0003, \u0002);
			}
			return \u0084.\u0004.\u0001(\u0004);
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00027050 File Offset: 0x00025250
		private bool \u0001(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006)
		{
			if (!CompilerServicesInternal.\u0001(\u0003, \u0006, false))
			{
				return false;
			}
			if (\u0005.ContainsKey(\u0003.Id))
			{
				return true;
			}
			\u0005[\u0003.Id] = \u0003;
			\u0006.Push(\u0003);
			if (!this.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006))
			{
				return false;
			}
			\u0006.Pop();
			\u0004.Add(\u0003);
			return true;
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x000270B4 File Offset: 0x000252B4
		private bool \u0002(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006)
		{
			if (!this.\u0003(\u0002, \u0003, \u0004, \u0005, \u0006))
			{
				return false;
			}
			if (!this.\u0004(\u0002, \u0003, \u0004, \u0005, \u0006))
			{
				return false;
			}
			LList<_ISignature> llist = new LList<_ISignature>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			_IScope iscope = \u0002.CreateIScope(\u0003.Id) as _IScope;
			foreach (_IVariable ivariable in \u0003.AllVariables.Where(new Func<_IVariable, bool>(DependencySorter.<>c.<>9.\u0001)))
			{
				if (ivariable.GetFlag(VarFlag.Retain))
				{
					\u0003.SetFlag(SignatureFlag.ContainsRetain, true);
				}
				if (ivariable.GetFlag(VarFlag.LocalPersistent))
				{
					\u0003.SetFlag(SignatureFlag.ContainsPersistent, true);
				}
				if (ivariable._Initial != null)
				{
					DependencySorter.\u0001.\u0001(ivariable._Initial, llist2, iscope);
				}
				ICompiledType type = ivariable._Type;
				DependencySorter.\u0001(iscope, type, llist, \u0003.GetFlag(SignatureFlag.Alias), out type);
				if (!this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, ivariable, type))
				{
					return false;
				}
			}
			return this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, llist, llist2);
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x00027208 File Offset: 0x00025408
		private bool \u0001(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006, LList<_ISignature> \u0007, LList<_ISignature> \u0008)
		{
			if (\u0007.Count > 0 || \u0008.Count > 0)
			{
				this.\u0001.Add(\u0003);
			}
			foreach (_ISignature isignature in \u0007)
			{
				if ((Operator.VarGlobal != isignature.POUType || !isignature.GetFlag(SignatureFlag.Enum)) && !this.\u0001(\u0002, isignature, \u0004, \u0005, \u0006))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00027294 File Offset: 0x00025494
		private bool \u0001(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006, _IVariable \u0007, ICompiledType \u0008)
		{
			if (\u0008 == null || \u0008.Class != TypeClass.Userdef)
			{
				return true;
			}
			_IUserdefType iuserdefType = \u0008 as _IUserdefType;
			_ISignature isignature = \u0002[iuserdefType.SignatureId];
			if (isignature != null)
			{
				if (!this.\u0001(\u0002, isignature, \u0004, \u0005, \u0006))
				{
					return false;
				}
				if (\u0007._Type.Class == TypeClass.Userdef && isignature.GetFlag(SignatureFlag.ContainsRetain))
				{
					\u0003.SetFlag(SignatureFlag.ContainsRetain, true);
				}
			}
			return true;
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x00027310 File Offset: 0x00025510
		private bool \u0003(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006)
		{
			_ISignature isignature = \u0002[\u0003.BaseSignatureId];
			if (isignature != null)
			{
				if (!this.\u0001(\u0002, isignature, \u0004, \u0005, \u0006))
				{
					return false;
				}
				if (isignature.GetFlag(SignatureFlag.ContainsRetain))
				{
					\u0003.SetFlag(SignatureFlag.ContainsRetain, true);
				}
			}
			return true;
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x00027360 File Offset: 0x00025560
		private bool \u0004(_ICompileContext \u0002, _ISignature \u0003, LList<_ISignature> \u0004, LDictionary<int, _ISignature> \u0005, LStack<_ISignature> \u0006)
		{
			foreach (int nId in \u0003.InterfaceIds)
			{
				_ISignature isignature = \u0002[nId];
				if (isignature != null && !this.\u0001(\u0002, isignature, \u0004, \u0005, \u0006))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400026B RID: 619
		public readonly LList<_ISignature> \u0001 = new LList<_ISignature>();

		// Token: 0x020000BA RID: 186
		private sealed class \u0001 : EmptyVisitor351900
		{
			// Token: 0x06000E60 RID: 3680 RVA: 0x000273B8 File Offset: 0x000255B8
			private \u0001(IList<_ISignature> \u009C\u0004)
			{
				this.\u0001 = \u009C\u0004;
			}

			// Token: 0x06000E61 RID: 3681 RVA: 0x000273D4 File Offset: 0x000255D4
			public static void \u0001(_IExpression \u0002, IList<_ISignature> \u0003, _IScope \u0004)
			{
				if (\u0002 == null)
				{
					return;
				}
				new DependencySorter.\u0001(\u0003).\u0001(\u0002, \u0004);
			}

			// Token: 0x06000E62 RID: 3682 RVA: 0x000273E8 File Offset: 0x000255E8
			private void \u0001(_IExpression \u0002, _IScope \u0003)
			{
				_IScope u = this.\u0001;
				this.\u0001 = \u0003;
				StandardTraverser ivisit = new StandardTraverser(this);
				\u0002.Accept(ivisit);
				this.\u0001 = u;
			}

			// Token: 0x06000E63 RID: 3683 RVA: 0x00027418 File Offset: 0x00025618
			public override void visit(_IVariableExpression variable, AccessFlag access)
			{
				ISignature signatureEx = variable.GetSignatureEx(this.\u0001);
				IVariable variable2 = variable.GetVariable(this.\u0001);
				if (signatureEx != null && variable2 != null && (variable2.GetFlag(VarFlag.Constant) || variable2.GetFlag(VarFlag.ReplacedConstant)) && variable2.Initial != null && this.\u0001.Add(variable2))
				{
					_IScope u = this.\u0001.CreateLocalScope(signatureEx) as _IScope;
					this.\u0001(variable2.Initial as _IExpression, u);
					this.\u0001.Remove(variable2);
				}
			}

			// Token: 0x06000E64 RID: 3684 RVA: 0x000274A4 File Offset: 0x000256A4
			public override void visit(_IOperatorExpression op)
			{
				base.visit(op);
				if (Helper.\u0001(op) && op._OperandsList.Count == 1)
				{
					ISignature signature = op._OperandsList[0].GetSignature(this.\u0001);
					if (signature != null)
					{
						this.\u0001.Add(signature as _ISignature);
					}
				}
			}

			// Token: 0x0400026C RID: 620
			private readonly LHashSet<IVariable> \u0001 = new LHashSet<IVariable>();

			// Token: 0x0400026D RID: 621
			private readonly IList<_ISignature> \u0001;

			// Token: 0x0400026E RID: 622
			private _IScope \u0001;
		}
	}
}
