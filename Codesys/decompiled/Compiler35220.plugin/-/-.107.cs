using System;
using System.Collections.Generic;
using System.Linq;
using \u0007;
using \u0012;
using \u0014;
using \u001A;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;

namespace \u000E
{
	// Token: 0x0200013F RID: 319
	internal static class \u0007
	{
		// Token: 0x06001600 RID: 5632 RVA: 0x00040CF8 File Offset: 0x0003EEF8
		internal static _ISignature \u0001(\u0081.\u0008 \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			string searchName = \u0003.GetSearchName(\u0005);
			bool flag = false;
			ISignature[] array = null;
			if (\u0004.ApplicationGuid == \u0005.ApplicationGuid && \u0004.ApplicationGuid != Guid.Empty)
			{
				ISignature signature = \u0005[searchName];
				if (signature != null)
				{
					array = new ISignature[]
					{
						signature
					};
				}
			}
			else
			{
				array = \u0002.\u0001(searchName);
			}
			_ISignature isignature = \u0003;
			if (array != null && array.Length != 0)
			{
				if (array[0].GetFlag(SignatureFlag.Typified) || array[0].GetFlag(SignatureFlag.Temp))
				{
					return array[0] as _ISignature;
				}
				flag = true;
				isignature = (array[0] as _ISignature);
			}
			if (!flag)
			{
				isignature = global::\u000E.\u0007.\u0002(\u0002, \u0003, \u0004, \u0005, \u0006);
			}
			global::\u000E.\u0007.\u0002(\u0002, isignature, \u0004, \u0005);
			return isignature;
		}

		// Token: 0x06001601 RID: 5633 RVA: 0x00040DB0 File Offset: 0x0003EFB0
		internal static _ISignature \u0002(\u0081.\u0008 \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			_ISignature isignature = global::\u0012.\u000E.\u0001(\u0003, \u0004, \u0005, \u0006);
			global::\u000E.\u0007.\u0001(\u0002, \u0004, isignature, \u0005, \u0006);
			return isignature;
		}

		// Token: 0x06001602 RID: 5634 RVA: 0x00040DD4 File Offset: 0x0003EFD4
		internal static void \u0001(\u0081.\u0008 \u0002, _ISignature \u0003, _ICompiledPOU \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			if (!\u0005.IsDefined("no_duplication_thread") && \u0002.TypifierLateParseTreeLoader != null)
			{
				_ICompiledPOU icompiledPOU = \u0004.CreateCompiledPOU();
				ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider = icompiledPOU as ICompiledPOUWithParseTreeProvider;
				compiledPOUWithParseTreeProvider.ParseTreeProvider = \u001A.\u0012.\u0001(compiledPOUWithParseTreeProvider, \u0002.TypifierLateParseTreeLoader);
				\u0002.TypifierLateParseTreeLoader.\u0001(compiledPOUWithParseTreeProvider);
				\u0005.AddCompiledPOU(icompiledPOU, \u0003, \u0006);
				return;
			}
			_ICompiledPOU cpou = \u0004.CreateCompiledPOU();
			\u0005.AddCompiledPOU(cpou, \u0003, \u0006);
		}

		// Token: 0x06001603 RID: 5635 RVA: 0x00040E40 File Offset: 0x0003F040
		internal static IVariable[] \u0001(\u0081.\u0008 \u0002, IReadOnlyList<_ISignature> \u0003, string \u0004, _ICompileContext \u0005, _ICompileContext \u0006, _IPreCompileContext \u0007, out ISignature[] \u0008)
		{
			IVariable[] array = new IVariable[\u0003.Count];
			\u0008 = new ISignature[\u0003.Count];
			for (int i = 0; i < \u0003.Count; i++)
			{
				_ISignature u = \u0003[i];
				_ISignature isignature = global::\u000E.\u0007.\u0003(\u0002, u, \u0007, \u0005, \u0006);
				IVariable variable = isignature[\u0004];
				array[i] = variable;
				\u0008[i] = isignature;
			}
			return array;
		}

		// Token: 0x06001604 RID: 5636 RVA: 0x00040EA4 File Offset: 0x0003F0A4
		internal static void \u0001(\u0081.\u0008 \u0002, global::\u0007.\u0006 \u0003, _ICompileContext \u0004, _ICompileContext \u0005, out IVariable[] \u0006, out ISignature[] \u0007)
		{
			LList<_IVariable> llist = new LList<_IVariable>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			if (\u0003.\u0001())
			{
				IVariable[] array = new _IVariable[1];
				\u0006 = array;
				ISignature[] array2 = new _ISignature[1];
				\u0007 = array2;
				_ISignature isignature = global::\u000E.\u0007.\u0003(\u0002, \u0003.SimpleSign, \u0003.SimplePreCompileContext, \u0004, \u0005);
				_IVariable ivariable = isignature[\u0003.SimpleVar.Name] as _IVariable;
				\u0003.SimpleSign = isignature;
				\u0003.SimpleVar = ivariable;
				\u0006[0] = ivariable;
				\u0007[0] = isignature;
			}
			else
			{
				IVariable[] array = new _IVariable[\u0003.\u0001()];
				\u0006 = array;
				ISignature[] array2 = new _ISignature[\u0003.\u0002()];
				\u0007 = array2;
				_IPreCompileContext[] array3 = \u0003.\u0001();
				_ISignature[] array4 = \u0003.\u0001();
				_IVariable[] array5 = \u0003.\u0001();
				for (int i = 0; i < array4.Length; i++)
				{
					_ISignature isignature2 = global::\u000E.\u0007.\u0003(\u0002, array4[i], array3[i], \u0004, \u0005);
					_IVariable ivariable2 = isignature2[array5[i].Name] as _IVariable;
					\u0006[i] = ivariable2;
					\u0007[i] = isignature2;
					llist.Add(ivariable2);
					llist2.Add(isignature2);
				}
				\u0003.\u0001(\u0007 as _ISignature[]);
				\u0003.\u0001(\u0006 as _IVariable[]);
			}
			\u0003.\u0001 = true;
		}

		// Token: 0x06001605 RID: 5637 RVA: 0x00040FE8 File Offset: 0x0003F1E8
		private static _ISignature \u0003(\u0081.\u0008 \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			string searchName = \u0003.GetSearchName(\u0005);
			_ICompileContext icompileContext;
			_ISignature isignature = \u0002.\u0001(searchName, out icompileContext) as _ISignature;
			if (isignature != null && (!(\u0004.ApplicationGuid != Guid.Empty) || !(\u0004.ApplicationGuid != icompileContext.ApplicationGuid)))
			{
				return global::\u000E.\u0007.\u0001(\u0002, \u0004, \u0005, isignature);
			}
			isignature = \u0005.AddCompiledSignature(\u0003, searchName, \u0004, \u0006, false);
			foreach (_ISignature isignature2 in isignature.SubSignatures.OfType<_ISignature>())
			{
				_ICompiledPOU pou = \u0004.GetPOU(isignature2.ObjectGuid);
				if (pou != null)
				{
					global::\u000E.\u0007.\u0001(\u0002, isignature2, pou, \u0005, \u0006);
				}
			}
			if (\u0004.LibraryPath != null)
			{
				\u0002.\u0001(\u0004);
			}
			global::\u000E.\u0007.\u0001(\u0002, isignature, \u0004, \u0005);
			return isignature;
		}

		// Token: 0x06001606 RID: 5638 RVA: 0x000410C8 File Offset: 0x0003F2C8
		private static _ISignature \u0001(\u0081.\u0008 \u0002, _IPreCompileContext \u0003, _ICompileContext \u0004, _ISignature \u0005)
		{
			if (\u0003.LibraryPath != null)
			{
				\u0002.\u0001(\u0003);
			}
			if (!\u0005.GetFlag(SignatureFlag.Typified) && !\u0005.GetFlag(SignatureFlag.Temp))
			{
				Tuple<_ISignature, _IPreCompileContext> tuple = new Tuple<_ISignature, _IPreCompileContext>(\u0005, \u0003);
				if (global::\u000E.\u0007.\u0001.Add(tuple))
				{
					global::\u000E.\u0007.\u0001(\u0002, \u0005, \u0003, \u0004);
					global::\u000E.\u0007.\u0001.Remove(tuple);
				}
			}
			return \u0005;
		}

		// Token: 0x06001607 RID: 5639 RVA: 0x0004112C File Offset: 0x0003F32C
		private static void \u0001(\u0081.\u0008 \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005)
		{
			global::\u0007.\u0005 u = \u0002.\u0001(\u0003, \u0004);
			\u0003.SetFlag(SignatureFlag.Temp, true);
			global::\u0014.\u0013.\u0003(\u0003, u, \u0005);
			foreach (_ISignature isignature in \u0003.SubSignatures.OfType<_ISignature>())
			{
				u.MethodSignature = isignature;
				isignature.SetFlag(SignatureFlag.Temp, true);
				global::\u0014.\u0013.\u0003(isignature, u, \u0005);
				isignature.SetFlag(SignatureFlag.Temp, false);
			}
			\u0003.SetFlag(SignatureFlag.Temp, false);
		}

		// Token: 0x06001608 RID: 5640 RVA: 0x000411D0 File Offset: 0x0003F3D0
		internal static void \u0002(\u0081.\u0008 \u0002, _ISignature \u0003, _IPreCompileContext \u0004, _ICompileContext \u0005)
		{
			\u0003.SetFlag(SignatureFlag.Temp, true);
			global::\u0007.\u0005 u = \u0002.\u0001(\u0003, \u0004);
			global::\u0014.\u0013.\u0003(\u0003, u, \u0005);
			foreach (_ISignature isignature in \u0003.SubSignatures.OfType<_ISignature>())
			{
				u.MethodSignature = isignature;
				isignature.SetFlag(SignatureFlag.Temp, true);
				global::\u0014.\u0013.\u0003(isignature, u, \u0005);
				isignature.SetFlag(SignatureFlag.Temp, false);
			}
			\u0003.SetFlag(SignatureFlag.Temp, false);
		}

		// Token: 0x06001609 RID: 5641 RVA: 0x00041274 File Offset: 0x0003F474
		private static void \u0001(\u0081.\u0008 \u0002, _IPreCompileContext \u0003, _ISignature \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			_ICompiledPOU pou = \u0003.GetPOU(\u0004.ObjectGuid);
			if (pou != null)
			{
				global::\u000E.\u0007.\u0001(\u0002, \u0004, pou, \u0005, \u0006);
			}
			foreach (_ISignature isignature in \u0004.SubSignatures.OfType<_ISignature>())
			{
				pou = \u0003.GetPOU(isignature.ObjectGuid);
				if (pou != null)
				{
					global::\u000E.\u0007.\u0001(\u0002, isignature, pou, \u0005, \u0006);
				}
			}
		}

		// Token: 0x040003DD RID: 989
		private static readonly LHashSet<Tuple<_ISignature, _IPreCompileContext>> \u0001 = new LHashSet<Tuple<_ISignature, _IPreCompileContext>>();
	}
}
