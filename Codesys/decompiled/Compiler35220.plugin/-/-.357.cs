using System;
using System.Collections.Generic;
using System.Linq;
using \u0003;
using \u0007;
using \u0011;
using \u001D;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0010
{
	// Token: 0x020003A4 RID: 932
	internal static class \u0011
	{
		// Token: 0x060035F6 RID: 13814 RVA: 0x000D7B78 File Offset: 0x000D5D78
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISequenceStatement \u0005)
		{
			foreach (_ISignature isignature in \u0002.AllFlat)
			{
				if (isignature.POUType == Operator.FunctionBlock)
				{
					_ISignature isignature2 = ((\u0003 != null) ? \u0003.GetSignatureById(isignature.Id) : null) as _ISignature;
					if (isignature2 != null)
					{
						global::\u0010.\u0011.\u0001(\u0002, isignature, isignature2, \u0004, \u0005);
					}
				}
			}
		}

		// Token: 0x060035F7 RID: 13815 RVA: 0x000D7BF0 File Offset: 0x000D5DF0
		private static bool \u0001(_IVariable \u0002)
		{
			if (\u0002.InputAssignments == null)
			{
				return true;
			}
			IAssignmentExpression[] inputAssignments = \u0002.InputAssignments;
			for (int i = 0; i < inputAssignments.Length; i++)
			{
				if (inputAssignments[i].LValue is INullExpression)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060035F8 RID: 13816 RVA: 0x000D7C30 File Offset: 0x000D5E30
		private static string \u0001(string \u0002, IEnumerable<IAssignmentExpression> \u0003, string \u0004, global::\u0003.\u0017 \u0005)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("{0}.FB_Init(bInitRetains:=FALSE,bInCopyCode:= FALSE", new object[]
			{
				\u0002
			});
			if (\u0003 != null)
			{
				foreach (IAssignmentExpression u in \u0003)
				{
					lstringBuilder.Append(",");
					lstringBuilder.Append(\u0005.\u0001(\u0004, u));
				}
			}
			lstringBuilder.Append(");");
			return lstringBuilder.ToString();
		}

		// Token: 0x060035F9 RID: 13817 RVA: 0x000D7CC0 File Offset: 0x000D5EC0
		private static string \u0002(string \u0002, IEnumerable<IAssignmentExpression> \u0003, string \u0004, global::\u0003.\u0017 \u0005)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendFormat("{0}.FB_Init(FALSE, FALSE", new object[]
			{
				\u0002
			});
			if (\u0003 != null)
			{
				foreach (IAssignmentExpression assignmentExpression in \u0003)
				{
					lstringBuilder.Append(",");
					lstringBuilder.Append(\u0005.\u0001(\u0004, assignmentExpression.RValue));
				}
			}
			lstringBuilder.Append(");");
			return lstringBuilder.ToString();
		}

		// Token: 0x060035FA RID: 13818 RVA: 0x000D7D54 File Offset: 0x000D5F54
		private static bool \u0001(ICompiledType \u0002, IScope \u0003)
		{
			_IUserdefType iuserdefType = \u0002 as _IUserdefType;
			return iuserdefType != null && iuserdefType.GetSignature(\u0003).GetSubSignature("__VFINIT") != null;
		}

		// Token: 0x060035FB RID: 13819 RVA: 0x000D7D84 File Offset: 0x000D5F84
		private static string \u0001(string \u0002, _IVariable \u0003, IScope \u0004, global::\u0003.\u0017 \u0005)
		{
			string text = string.Format("{0}.{1}", \u0002, \u0003.OrgName);
			string str = string.Empty;
			if (global::\u0010.\u0011.\u0001(\u0003.CompiledType, \u0004))
			{
				str = text + ".__VFINIT();";
			}
			string str2;
			if (global::\u0010.\u0011.\u0001(\u0003))
			{
				str2 = global::\u0010.\u0011.\u0001(text, \u0003.InputAssignments, \u0002, \u0005);
			}
			else
			{
				str2 = global::\u0010.\u0011.\u0002(text, \u0003.InputAssignments, \u0002, \u0005);
			}
			return str + str2;
		}

		// Token: 0x060035FC RID: 13820 RVA: 0x000D7DF4 File Offset: 0x000D5FF4
		private static string \u0001(int \u0002, _ISignature \u0003, IScope5 \u0004, string[] \u0005, _IVariable \u0006, _IArrayType \u0007)
		{
			Helper.\u0001(\u0002, \u0003, null);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in \u0005)
			{
				LStringBuilder lstringBuilder2 = new LStringBuilder();
				lstringBuilder2.AppendFormat("{0}.{1}", new object[]
				{
					text,
					\u0006.Name
				});
				Helper.\u0001(\u0007, lstringBuilder2, 0);
				string str = string.Empty;
				if (global::\u0010.\u0011.\u0001(\u0084.\u0004.\u0001(\u0007), \u0004))
				{
					str = string.Format("{0}.__VFINIT();", lstringBuilder2);
				}
				string str2 = string.Format("{0}.FB_Init(bInitRetains:=FALSE,bInCopyCode:= FALSE);", lstringBuilder2);
				string u = str + str2;
				lstringBuilder2.Clear();
				Helper.\u0001(\u0007, u, lstringBuilder2, \u0004, 0);
				lstringBuilder.Append(lstringBuilder2);
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x060035FD RID: 13821 RVA: 0x000D7EBC File Offset: 0x000D60BC
		private static string \u0001(IScope5 \u0002, string[] \u0003, _IVariable \u0004, _IArrayType \u0005, global::\u0003.\u0017 \u0006)
		{
			bool flag = global::\u0010.\u0011.\u0001(\u0004);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in \u0003)
			{
				IList<_IExpression> list = Helper.\u0001(global::\u0011.\u0006.\u0001(text + "." + \u0004.OrgName) as _IExpression, \u0002, \u0005);
				int num = \u0004.InputAssignments.Length / list.Count<_IExpression>();
				Debug.\u0001(num * list.Count<_IExpression>() == \u0004.InputAssignments.Length);
				LList<IAssignmentExpression> llist = new LList<IAssignmentExpression>();
				llist.AddRange(\u0004.InputAssignments);
				foreach (_IExpression iexpression in list)
				{
					LList<IAssignmentExpression> llist2 = new LList<IAssignmentExpression>();
					llist2.AddRange(llist.TakeRange(0, num));
					llist.RemoveRange(0, num);
					string text2 = string.Empty;
					if (global::\u0010.\u0011.\u0001(\u0084.\u0004.\u0001(\u0005), \u0002))
					{
						text2 = string.Format("{0}.__VFINIT();", iexpression);
					}
					lstringBuilder.AppendLine(text2);
					if (flag)
					{
						lstringBuilder.Append(global::\u0010.\u0011.\u0001(iexpression.ToString(), llist2, text, \u0006));
					}
					else
					{
						lstringBuilder.Append(global::\u0010.\u0011.\u0002(iexpression.ToString(), llist2, text, \u0006));
					}
				}
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x060035FE RID: 13822 RVA: 0x000D8024 File Offset: 0x000D6224
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, _ISignature \u0005, _ISequenceStatement \u0006)
		{
			bool flag = false;
			LStringBuilder lstringBuilder = new LStringBuilder();
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			scope.MethodSignature = \u0005;
			global::\u0003.\u0017 u = new global::\u0003.\u0017(\u0002, \u0003);
			IList<_IVariable> allVariables = \u0003.AllVariables;
			for (int i = allVariables.Count - 1; i >= 0; i--)
			{
				_IVariable ivariable = allVariables[i];
				_IVariable u2 = \u0004[ivariable.OrgName] as _IVariable;
				if (global::\u0010.\u0011.\u0001(ivariable, u2))
				{
					LList<string> llist = InstancePathService.\u0001(\u0002, \u0003, Array.Empty<int>(), null, null, true);
					if (ivariable.CompiledType.Class == TypeClass.Userdef)
					{
						foreach (string u3 in llist)
						{
							lstringBuilder.AppendFormat(global::\u0010.\u0011.\u0001(u3, ivariable, scope, u), Array.Empty<object>());
						}
						flag = (flag || llist.Any<string>());
					}
					else if (ivariable.CompiledType.Class == TypeClass.Array)
					{
						global::\u0010.\u0011.\u0001(\u0005, ref flag, lstringBuilder, scope, u, ivariable, llist);
					}
				}
			}
			global::\u0010.\u0011.\u0001(\u0002, \u0005, \u0006, flag, lstringBuilder);
		}

		// Token: 0x060035FF RID: 13823 RVA: 0x000D8158 File Offset: 0x000D6358
		private static void \u0001(_ISignature \u0002, ref bool \u0003, LStringBuilder \u0004, IScope5 \u0005, global::\u0003.\u0017 \u0006, _IVariable \u0007, LList<string> \u0008)
		{
			_IArrayType iarrayType = \u0007.CompiledType as _IArrayType;
			int num = Helper.\u0001(iarrayType);
			bool flag;
			int numOfElements = iarrayType.GetNumOfElements(\u0005, out flag);
			if (num == 0)
			{
				return;
			}
			if (flag && numOfElements == 0)
			{
				return;
			}
			if (\u0007.InputAssignments == null || \u0007.InputAssignments.Length == 0)
			{
				\u0004.Append(global::\u0010.\u0011.\u0001(num, \u0002, \u0005, \u0008.ToArray(), \u0007, iarrayType));
			}
			else
			{
				\u0004.Append(global::\u0010.\u0011.\u0001(\u0005, \u0008.ToArray(), \u0007, iarrayType, \u0006));
			}
			\u0003 = (\u0003 || \u0008.Any<string>());
		}

		// Token: 0x06003600 RID: 13824 RVA: 0x000D81E8 File Offset: 0x000D63E8
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003)
		{
			if (\u001D.\u000F.\u0001(\u0002))
			{
				return false;
			}
			if (\u0002.HasFlag(VarFlag.Absolut))
			{
				return false;
			}
			if (\u0002.GetFlag(VarFlag.NoInit))
			{
				return false;
			}
			if (\u0003 != null)
			{
				return false;
			}
			ICompiledType compiledType = \u0002.CompiledType;
			if (\u0002.CompiledType.Class == TypeClass.Array)
			{
				compiledType = \u0084.\u0004.\u0001(compiledType);
			}
			return compiledType.Class == TypeClass.Userdef;
		}

		// Token: 0x06003601 RID: 13825 RVA: 0x000D8250 File Offset: 0x000D6450
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISequenceStatement \u0004, bool \u0005, LStringBuilder \u0006)
		{
			if (\u0005)
			{
				\u0006.Insert(0, "{implicit on}");
				\u0006.AppendLine("{implicit off}");
				_IStatement istatement = new global::\u0011.\u0006(\u0006.ToString(), true).\u0001();
				IScope5 scope = \u0002.CreateGlobalIScope() as IScope5;
				scope.MethodSignature = \u0003;
				istatement.Accept(new ExpressionTypifierWithSpecialTasks(scope, \u0002, false, null)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
				});
				istatement.Accept(new TypeCheckerVisitor(scope, \u0002, true)
				{
					TreatReferenceAsPointer = true,
					InterfaceAsInterface = true
				});
				ErrorVisitor ivisit = new ErrorVisitor();
				istatement.Accept(ivisit);
				\u0004.Add(istatement);
			}
		}
	}
}
