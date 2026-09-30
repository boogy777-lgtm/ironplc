using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u0004;
using \u000E;
using \u0011;
using \u0012;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Messaging.MessageDecorator;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u001B
{
	// Token: 0x0200034A RID: 842
	internal sealed class \u000F
	{
		// Token: 0x17000851 RID: 2129
		// (get) Token: 0x060032D0 RID: 13008 RVA: 0x000C3E6C File Offset: 0x000C206C
		private _ISignature Signature { get; }

		// Token: 0x17000852 RID: 2130
		// (get) Token: 0x060032D1 RID: 13009 RVA: 0x000C3E74 File Offset: 0x000C2074
		private _ICompileContext Comcon { get; }

		// Token: 0x17000853 RID: 2131
		// (get) Token: 0x060032D2 RID: 13010 RVA: 0x000C3E7C File Offset: 0x000C207C
		private _IVariable Variable { get; }

		// Token: 0x17000854 RID: 2132
		// (get) Token: 0x060032D3 RID: 13011 RVA: 0x000C3E84 File Offset: 0x000C2084
		private _ISignature VariableTypeSignature { get; }

		// Token: 0x17000855 RID: 2133
		// (get) Token: 0x060032D4 RID: 13012 RVA: 0x000C3E8C File Offset: 0x000C208C
		private _ISignature EffectiveTypeSignature { get; }

		// Token: 0x060032D5 RID: 13013 RVA: 0x000C3E94 File Offset: 0x000C2094
		internal \u000F(_ISignature \u001C\u0002, _ICompileContext \u0001\u0002, _IVariable \u001A\u0002, _ISignature \u0086\u0005, _ISignature \u0087\u0005)
		{
			this.Signature = \u001C\u0002;
			this.Comcon = \u0001\u0002;
			this.Variable = \u001A\u0002;
			this.VariableTypeSignature = \u0086\u0005;
			this.EffectiveTypeSignature = \u0087\u0005;
		}

		// Token: 0x060032D6 RID: 13014 RVA: 0x000C3EC4 File Offset: 0x000C20C4
		private static void \u0001(object \u0002, MessageId \u0003, params object[] \u0004)
		{
			Tuple<_ISignature, _IVariable> tuple = \u0002 as Tuple<_ISignature, _IVariable>;
			if (tuple != null)
			{
				tuple.Item1.\u0001(tuple.Item2.SourcePosition, Severity.Error, \u0003, \u0004);
			}
		}

		// Token: 0x060032D7 RID: 13015 RVA: 0x000C3EF4 File Offset: 0x000C20F4
		private void \u0001(object \u0002, _ICompilerMessage \u0003)
		{
			Tuple<_ISignature, _IVariable> tuple = \u0002 as Tuple<_ISignature, _IVariable>;
			if (tuple != null)
			{
				tuple.Item1.AddMessage(\u0003);
			}
		}

		// Token: 0x060032D8 RID: 13016 RVA: 0x000C3F18 File Offset: 0x000C2118
		internal void \u0001(IScope5 \u0002, TypeCheckerVisitor \u0003, ErrorVisitor \u0004)
		{
			if (!this.Variable.HasFlag(VarFlag.Alias) && this.EffectiveTypeSignature != null)
			{
				_ISignature isignature = (_ISignature)this.EffectiveTypeSignature.GetSubSignature(IdentifierConstants.InitMethodName);
				if (isignature != null && this.Variable.InputAssignments != null && isignature.GetFlagInternal(SignatureFlagInternal.Overloaded))
				{
					IList<_ISignature> list = \u001F.\u0010.\u0001((ICommonScope)\u0002, this.Comcon, this.Variable.InputAssignments, isignature, (_ISignature4)this.EffectiveTypeSignature);
					\u001F.\u0010.\u0001(new Tuple<_ISignature, _IVariable>(this.Signature, this.Variable), IdentifierConstants.InitMethodName, list, new global::\u000E.\u0016(global::\u001B.\u000F.\u0001), new global::\u0012.\u0013(this.\u0001));
					isignature = list.FirstOrDefault<_ISignature>();
				}
				if (isignature == null)
				{
					if (this.Variable.InputAssignments != null && this.Variable.InputAssignments.Length != 0)
					{
						this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoMatchingInitMethodFound, new object[]
						{
							this.VariableTypeSignature.OrgName
						});
						return;
					}
				}
				else
				{
					this.\u0001(\u0002, isignature, \u0003, \u0004);
				}
			}
		}

		// Token: 0x060032D9 RID: 13017 RVA: 0x000C403C File Offset: 0x000C223C
		internal void \u0001()
		{
			if (this.Variable.HasFlag(VarFlag.Retain) && !global::\u001B.\u000F.\u0001(this.VariableTypeSignature, this.Comcon))
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_InstanceNotAllowedInRetain, new object[]
				{
					this.VariableTypeSignature.OrgName
				});
			}
			if (this.Variable.HasFlag(VarFlag.Retain) && global::\u001B.\u000F.\u0002(this.VariableTypeSignature, this.Comcon))
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Warning, MessageId.Wrn_ExitForRetainInstances, Array.Empty<object>());
			}
		}

		// Token: 0x060032DA RID: 13018 RVA: 0x000C40E8 File Offset: 0x000C22E8
		private static string \u0001(_ISignature \u0002, IScope \u0003)
		{
			string libraryPath = \u0002.LibraryPath;
			if (!string.IsNullOrEmpty(libraryPath))
			{
				return libraryPath;
			}
			ISignature signature = \u0003[\u0002.ParentSignatureId];
			if (signature != null)
			{
				return signature.LibraryPath;
			}
			return string.Empty;
		}

		// Token: 0x060032DB RID: 13019 RVA: 0x000C4124 File Offset: 0x000C2324
		internal void \u0001(IScope \u0002)
		{
			if (this.VariableTypeSignature.GetFlag(SignatureFlag.Internal) && !string.Equals(global::\u001B.\u000F.\u0001(this.Signature, \u0002), this.VariableTypeSignature.LibraryPath, StringComparison.OrdinalIgnoreCase))
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_AccessToInternalObject, new object[]
				{
					this.VariableTypeSignature.OrgName,
					this.VariableTypeSignature.LibraryPath
				});
			}
		}

		// Token: 0x060032DC RID: 13020 RVA: 0x000C41A4 File Offset: 0x000C23A4
		internal void \u0001(ErrorVisitor \u0002)
		{
			string attributeValue = this.VariableTypeSignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBSOLETE);
			if (!string.IsNullOrEmpty(attributeValue))
			{
				string u = global::\u0003.\u0006.\u0001(MessageId.Wrn_Obsolete, new object[]
				{
					this.VariableTypeSignature.OrgName,
					attributeValue
				});
				_ICompilerMessage icompilerMessage = global::\u0019.\u0003.\u0001(this.Variable._SourcePosition, u, Severity.Warning, MessageId.Wrn_Obsolete);
				icompilerMessage = \u0002.\u0001(icompilerMessage);
				this.Signature.AddMessage(icompilerMessage);
			}
		}

		// Token: 0x060032DD RID: 13021 RVA: 0x000C421C File Offset: 0x000C241C
		private static bool \u0001(_ISignature \u0002, _ICompileContext \u0003)
		{
			LHashSet<string> lhashSet = new LHashSet<string>();
			LStack<_ISignature> lstack = new LStack<_ISignature>();
			lstack.Push(\u0002);
			while (lstack.Count > 0)
			{
				_ISignature isignature = lstack.Pop();
				string searchName = isignature.GetSearchName(\u0003);
				if (lhashSet.Add(searchName))
				{
					if (isignature.HasAttribute("no_instance_in_retain"))
					{
						return false;
					}
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.Type.Class == TypeClass.Userdef)
						{
							_IUserdefType iuserdefType = (_IUserdefType)ivariable.Type;
							_ISignature isignature2 = \u0003[iuserdefType.SignatureId];
							if (isignature2 != null)
							{
								lstack.Push(isignature2);
							}
						}
					}
				}
			}
			return true;
		}

		// Token: 0x060032DE RID: 13022 RVA: 0x000C42F0 File Offset: 0x000C24F0
		private static bool \u0002(_ISignature \u0002, _ICompileContext \u0003)
		{
			LHashSet<string> lhashSet = new LHashSet<string>();
			LStack<_ISignature> lstack = new LStack<_ISignature>();
			lstack.Push(\u0002);
			while (lstack.Count > 0)
			{
				_ISignature isignature = lstack.Pop();
				string searchName = isignature.GetSearchName(\u0003);
				if (lhashSet.Add(searchName))
				{
					if (isignature.GetSubSignature("FB_EXIT") != null)
					{
						return true;
					}
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.Type.Class == TypeClass.Userdef)
						{
							_IUserdefType iuserdefType = (_IUserdefType)ivariable.Type;
							_ISignature isignature2 = \u0003[iuserdefType.SignatureId];
							if (isignature2 != null)
							{
								lstack.Push(isignature2);
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060032DF RID: 13023 RVA: 0x000C43C4 File Offset: 0x000C25C4
		private static int \u0001(_IArrayType \u0002, IScope5 \u0003)
		{
			int numOfElements = \u0002.GetNumOfElements(\u0003);
			_IArrayType iarrayType = \u0002.BaseType as _IArrayType;
			if (iarrayType != null)
			{
				return numOfElements * global::\u001B.\u000F.\u0001(iarrayType, \u0003);
			}
			return numOfElements;
		}

		// Token: 0x060032E0 RID: 13024 RVA: 0x000C43F4 File Offset: 0x000C25F4
		private void \u0001(IScope5 \u0002, ISignature \u0003, TypeCheckerVisitor \u0004, ErrorVisitor \u0005, _IVariable[] \u0006)
		{
			int num = global::\u001B.\u000F.\u0001(this.Variable.Type as _IArrayType, \u0002);
			int num2 = (\u0006.Length - 3) * num;
			int num3 = \u0006.Length - 3;
			if (this.Variable.InputAssignments == null)
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_NumOfInitializersDoNotMatch, new object[]
				{
					0,
					num
				});
				return;
			}
			if (num3 == this.Variable.InputAssignments.Length)
			{
				this.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
				return;
			}
			if (num2 == this.Variable.InputAssignments.Length)
			{
				for (int i = 0; i < num2; i += \u0006.Length - 3)
				{
					IList<IAssignmentExpression> list = new LList<IAssignmentExpression>();
					for (int j = i; j < i + (\u0006.Length - 3); j++)
					{
						list.Add(this.Variable.InputAssignments[j]);
					}
					global::\u001B.\u000F.\u0001(this.Variable, this.Signature, this.EffectiveTypeSignature, list, \u0006);
				}
				this.\u0001(\u0003, \u0002, \u0006, \u0004, \u0005);
				return;
			}
			if (3 == \u0006.Length)
			{
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoMatchingInitMethodFound, new object[]
				{
					this.EffectiveTypeSignature.OrgName
				});
				return;
			}
			this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_NumOfInitializersDoNotMatch, new object[]
			{
				this.Variable.InputAssignments.Length / (\u0006.Length - 3),
				num
			});
		}

		// Token: 0x060032E1 RID: 13025 RVA: 0x000C4588 File Offset: 0x000C2788
		private void \u0001(IScope5 \u0002, ISignature \u0003, TypeCheckerVisitor \u0004, ErrorVisitor \u0005, IReadOnlyList<_IVariable> \u0006)
		{
			if (this.Variable.InputAssignments == null || this.Variable.InputAssignments.Length != \u0006.Count - 3)
			{
				C138MessageDecorator u = new C138MessageDecorator(\u0003, this.Variable);
				this.Signature.\u0001(this.Variable.SourcePosition, Severity.Error, MessageId.Err_NoMatchingInitMethodFound, u, new object[]
				{
					this.EffectiveTypeSignature.OrgName
				});
				return;
			}
			global::\u001B.\u000F.\u0001(this.Variable, this.Signature, this.EffectiveTypeSignature, this.Variable.InputAssignments, \u0006);
			this.\u0001(\u0003, \u0002, \u0006, \u0004, \u0005);
		}

		// Token: 0x060032E2 RID: 13026 RVA: 0x000C462C File Offset: 0x000C282C
		private void \u0001(IScope5 \u0002, ISignature \u0003, TypeCheckerVisitor \u0004, ErrorVisitor \u0005)
		{
			_IVariable[] array = Helper.\u0001((_ISignature)\u0003).ToArray<_IVariable>();
			if (!this.Variable.GetFlag(VarFlag.NoInit) && (array.Length > 3 || this.Variable.InputAssignments != null))
			{
				if (this.Variable.Type is _IArrayType)
				{
					this.\u0001(\u0002, \u0003, \u0004, \u0005, array);
					return;
				}
				this.\u0001(\u0002, \u0003, \u0004, \u0005, array);
			}
		}

		// Token: 0x060032E3 RID: 13027 RVA: 0x000C469C File Offset: 0x000C289C
		private void \u0001(ISignature \u0002, IScope5 \u0003, IReadOnlyList<_IVariable> \u0004, TypeCheckerVisitor \u0005, ErrorVisitor \u0006)
		{
			IScope5 scope = global::\u0004.\u0012.\u0001(\u0003, this.Comcon, \u0002, false);
			TypeCheckerVisitor ivisit = new TypeCheckerVisitor(scope, this.Comcon);
			int num = \u0004.Count - 3;
			for (int i = 0; i < this.Variable.InputAssignments.Length; i++)
			{
				_IAssignmentExpression iassignmentExpression = (_IAssignmentExpression)this.Variable.InputAssignments[i];
				_IExpression lvalue = iassignmentExpression._LValue;
				_IExpression rvalue = iassignmentExpression._RValue;
				_IExpression iexpression = ((rvalue != null) ? rvalue.Duplicate() : null) as _IExpression;
				lvalue.Accept(ivisit);
				_IVariable u;
				if (lvalue is _INullExpression)
				{
					int index = i % num + 2;
					u = \u0004[index];
				}
				else
				{
					u = (_IVariable)lvalue.GetVariable(scope);
				}
				if (iexpression != null)
				{
					iexpression.Accept(\u0005);
				}
				_IExpression iexpression2 = lvalue;
				if (iexpression2 is _INullExpression)
				{
					iexpression2 = iexpression;
				}
				\u0005.\u0001(iexpression2, ref iexpression, \u0002, u, lvalue);
				if (lvalue.Type is _IReferenceType)
				{
					\u0005.\u0001(iexpression, \u0002, iexpression, u);
				}
				if (this.Variable.HasFlag(VarFlag.AllocateInInstance))
				{
					foreach (IAssignmentExpression assignmentExpression in this.Variable.InputAssignments)
					{
						global::\u0004.\u0011.\u0001(this.Variable, \u0003, (_IExpression)assignmentExpression, this.Signature, \u0005);
					}
				}
				iassignmentExpression.Accept(\u0006);
				iexpression.Accept(\u0006);
			}
		}

		// Token: 0x060032E4 RID: 13028 RVA: 0x000C480C File Offset: 0x000C2A0C
		private static void \u0001(IVariable \u0002, _ISignature \u0003, ISignature \u0004, IList<IAssignmentExpression> \u0005, IReadOnlyList<_IVariable> \u0006)
		{
			bool flag = false;
			bool flag2 = false;
			for (int i = 2; i < \u0006.Count - 1; i++)
			{
				bool flag3 = false;
				string name = \u0006[i].Name;
				foreach (IAssignmentExpression assignmentExpression in \u0005)
				{
					if (assignmentExpression.LValue is _INullExpression)
					{
						flag3 = true;
						flag = true;
						break;
					}
					if (assignmentExpression.LValue.ToString().Equals(name, StringComparison.CurrentCultureIgnoreCase))
					{
						flag3 = true;
						flag2 = true;
						break;
					}
				}
				if (!flag3)
				{
					\u0003.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_NoMatchingInitMethodFound, new object[]
					{
						\u0004.OrgName
					});
				}
			}
			if (flag && flag2)
			{
				\u0003.\u0001(\u0002.SourcePosition, Severity.Error, MessageId.Err_FunctionCallMixedStyle, Array.Empty<object>());
			}
		}

		// Token: 0x04000994 RID: 2452
		[CompilerGenerated]
		private readonly _ISignature \u0001;

		// Token: 0x04000995 RID: 2453
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000996 RID: 2454
		[CompilerGenerated]
		private readonly _IVariable \u0001;

		// Token: 0x04000997 RID: 2455
		[CompilerGenerated]
		private readonly _ISignature \u0002;

		// Token: 0x04000998 RID: 2456
		[CompilerGenerated]
		private readonly _ISignature \u0003;

		// Token: 0x04000999 RID: 2457
		private const int \u0001 = 3;

		// Token: 0x0400099A RID: 2458
		private const int \u0002 = 2;

		// Token: 0x0400099B RID: 2459
		private const int \u0003 = 1;
	}
}
