using System;
using System.Collections.Generic;
using System.Linq;
using \u0013;
using \u0015;
using \u0017;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0080;

namespace \u0081
{
	// Token: 0x02000148 RID: 328
	internal sealed class \u0007 : ICommonScope2, ICommonScope, _IPrecompileScope, IPrecompileScope6, IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope, IPrecompileScope7, IPrecompileScopeWithAliasService, global::\u0017.\u0006, global::\u0015.\u0002
	{
		// Token: 0x060016B6 RID: 5814 RVA: 0x00045340 File Offset: 0x00043540
		private \u0007(LList<global::\u0015.\u0002> \u0011\u0003)
		{
			this.\u0001 = \u0011\u0003;
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x00045350 File Offset: 0x00043550
		public static \u0081.\u0007 \u0001(ISignature \u0002, int \u0003, Guid \u0004)
		{
			LList<global::\u0015.\u0002> llist = new LList<global::\u0015.\u0002>();
			if (\u0004 != Guid.Empty)
			{
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(\u0004);
				if (ipreCompileContext != null)
				{
					llist.Add(new CheckerScope(\u0003, ipreCompileContext._GetLibraryTable(\u0004), \u0002 as _ISignature, ipreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool));
				}
			}
			llist.Add(new CheckerScope(\u0003, APEnvironmentFacade.Instance.LanguageModelMgr.Pool._GetLibraryTable(\u0004), \u0002 as _ISignature, APEnvironmentFacade.Instance.LanguageModelMgr.Pool, APEnvironmentFacade.Instance.LanguageModelMgr.Pool));
			foreach (_IPreCompileContext ipreCompileContext2 in APEnvironmentFacade.Instance.LanguageModelMgr._PrecompileContexts)
			{
				if (ipreCompileContext2.ApplicationGuid != \u0004)
				{
					llist.Add(new CheckerScope(\u0003, ipreCompileContext2._GetLibraryTable(ipreCompileContext2.ApplicationGuid), null, ipreCompileContext2, APEnvironmentFacade.Instance.LanguageModelMgr.Pool));
				}
			}
			return new \u0081.\u0007(llist);
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x00045478 File Offset: 0x00043678
		public static \u0081.\u0007 \u0001(ISignature \u0002, int \u0003)
		{
			new LList<global::\u0015.\u0002>();
			Guid activeApplicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
			return \u0081.\u0007.\u0001(\u0002, \u0003, activeApplicationGuid);
		}

		// Token: 0x060016B9 RID: 5817 RVA: 0x000454A0 File Offset: 0x000436A0
		internal static IPrecompileScope2 \u0001(Guid \u0002, _IPreCompileContext \u0003, _ISignature \u0004)
		{
			if (\u0003.ApplicationGuid == Guid.Empty && string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				return \u0081.\u0007.\u0001(\u0004, 4);
			}
			_ILibraryTable libraryTable = \u0003._GetLibraryTable(\u0002);
			if (!string.IsNullOrEmpty((\u0004 != null) ? \u0004.LibraryPath : null))
			{
				_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContextOfSignature(\u0004) as _IPreCompileContext;
				if (ipreCompileContext == null)
				{
					ipreCompileContext = \u0003;
				}
				return new CheckerScope(\u0003.PointerSize, libraryTable, \u0004, ipreCompileContext, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			}
			return new CheckerScope(\u0003.PointerSize, libraryTable, \u0004, \u0003, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
		}

		// Token: 0x17000547 RID: 1351
		// (get) Token: 0x060016BA RID: 5818 RVA: 0x00045548 File Offset: 0x00043748
		// (set) Token: 0x060016BB RID: 5819 RVA: 0x0004555C File Offset: 0x0004375C
		public bool IgnoreImplicitEnumMembers
		{
			get
			{
				return this.\u0001[0].IgnoreImplicitEnumMembers;
			}
			set
			{
				this.\u0001[0].IgnoreImplicitEnumMembers = value;
			}
		}

		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x060016BC RID: 5820 RVA: 0x00045570 File Offset: 0x00043770
		// (set) Token: 0x060016BD RID: 5821 RVA: 0x00045584 File Offset: 0x00043784
		public bool IgnoreActions
		{
			get
			{
				return this.\u0001[0].IgnoreActions;
			}
			set
			{
				this.\u0001[0].IgnoreActions = value;
			}
		}

		// Token: 0x060016BE RID: 5822 RVA: 0x00045598 File Offset: 0x00043798
		public void \u0001()
		{
			this.\u0001[0].SetPointerSize();
		}

		// Token: 0x060016BF RID: 5823 RVA: 0x000455AC File Offset: 0x000437AC
		public string \u0001()
		{
			return this.\u0001[0].\u0001();
		}

		// Token: 0x060016C0 RID: 5824 RVA: 0x000455C0 File Offset: 0x000437C0
		public global::\u0015.\u0002 \u0001()
		{
			return this.\u0001[0].\u0001();
		}

		// Token: 0x060016C1 RID: 5825 RVA: 0x000455D4 File Offset: 0x000437D4
		private Tuple<global::\u0015.\u0002, ISignature> \u0001(IExpression \u0002)
		{
			global::\u0015.\u0002 item = null;
			ISignature signature = null;
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature[] array = u.FindSignature(\u0002);
				if (array != null && array.Length != 0)
				{
					if (signature == null)
					{
						signature = array[0];
						item = u;
					}
					else if (array[0].GetFlag(SignatureFlag.SuperGlobal))
					{
						signature = array[0];
						item = u;
						break;
					}
				}
			}
			return new Tuple<global::\u0015.\u0002, ISignature>(item, signature);
		}

		// Token: 0x060016C2 RID: 5826 RVA: 0x00045660 File Offset: 0x00043860
		public global::\u0015.\u0002 \u0001(IExpression \u0002)
		{
			Tuple<global::\u0015.\u0002, ISignature> tuple = this.\u0001(\u0002);
			global::\u0015.\u0002 item = tuple.Item1;
			_ISignature isignature = (_ISignature)tuple.Item2;
			if (isignature != null && isignature.GetFlag(SignatureFlag.Alias))
			{
				IVariable variable = null;
				if (isignature.AllVariables.Count<_IVariable>() == 1)
				{
					variable = isignature.AllVariables[0];
				}
				if (variable != null && variable.Type.Class == TypeClass.Userdef)
				{
					tuple = this.\u0001(variable.Type as IUserdefType);
					if (tuple == null)
					{
						return null;
					}
					item = tuple.Item1;
					isignature = (_ISignature)tuple.Item2;
				}
			}
			if (isignature != null)
			{
				return item.\u0001(isignature);
			}
			return null;
		}

		// Token: 0x060016C3 RID: 5827 RVA: 0x000456FC File Offset: 0x000438FC
		public global::\u0015.\u0002 \u0001(_ISignature \u0002)
		{
			if (\u0002 != null)
			{
				foreach (global::\u0015.\u0002 u in this.\u0001)
				{
					ISignature[] array = u.\u0001(\u0002.Name);
					if (array != null && array.Length != 0)
					{
						return u.\u0001(\u0002);
					}
				}
			}
			return this.\u0001[0].\u0001(\u0002);
		}

		// Token: 0x060016C4 RID: 5828 RVA: 0x00045778 File Offset: 0x00043978
		public global::\u0015.\u0002 \u0001(_IPreCompileContext \u0002)
		{
			return this.\u0001[0].\u0001(\u0002);
		}

		// Token: 0x060016C5 RID: 5829 RVA: 0x0004578C File Offset: 0x0004398C
		public global::\u0015.\u0002 \u0002(_ISignature \u0002)
		{
			return this.\u0001[0].\u0002(\u0002);
		}

		// Token: 0x060016C6 RID: 5830 RVA: 0x000457A0 File Offset: 0x000439A0
		public ISignature[] \u0001(string \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature[] array = u.\u0001(\u0002);
				if (array != null && array.Length != 0)
				{
					return array;
				}
			}
			return null;
		}

		// Token: 0x060016C7 RID: 5831 RVA: 0x000457FC File Offset: 0x000439FC
		public bool \u0001(string \u0002, out IVariable[] \u0003, out ISignature[] \u0004, out global::\u0015.\u0002 \u0005)
		{
			\u0003 = null;
			\u0004 = null;
			\u0005 = null;
			using (IEnumerator<global::\u0015.\u0002> enumerator = this.\u0001.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.\u0001(\u0002, out \u0003, out \u0004, out \u0005))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x17000549 RID: 1353
		public ISignature this[Guid \u0002]
		{
			get
			{
				foreach (global::\u0015.\u0002 u in this.\u0001)
				{
					ISignature signature = u[\u0002];
					if (signature != null)
					{
						return signature;
					}
				}
				return null;
			}
		}

		// Token: 0x1700054A RID: 1354
		public ISignature[] this[string \u0002]
		{
			get
			{
				foreach (global::\u0015.\u0002 u in this.\u0001)
				{
					ISignature[] array = u[\u0002];
					if (array != null)
					{
						return array;
					}
				}
				return null;
			}
		}

		// Token: 0x060016CA RID: 5834 RVA: 0x00045910 File Offset: 0x00043B10
		public global::\u0015.\u0002 \u0002(IExpression \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				global::\u0015.\u0002 u2 = u.\u0002(\u0002);
				if (u2 != null)
				{
					return u2;
				}
			}
			return null;
		}

		// Token: 0x060016CB RID: 5835 RVA: 0x00045968 File Offset: 0x00043B68
		public IPrecompileScope \u0001(IExpression \u0002)
		{
			return this.\u0002(\u0002);
		}

		// Token: 0x060016CC RID: 5836 RVA: 0x00045974 File Offset: 0x00043B74
		public IIdentifierInfo[] \u0001(string \u0002)
		{
			LList<IIdentifierInfo> llist = new LList<IIdentifierInfo>();
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				IIdentifierInfo[] identifierInfo = u.GetIdentifierInfo(\u0002);
				if (identifierInfo != null)
				{
					llist.AddRange(identifierInfo);
				}
			}
			return llist.ToArray();
		}

		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x060016CD RID: 5837 RVA: 0x000459D8 File Offset: 0x00043BD8
		// (set) Token: 0x060016CE RID: 5838 RVA: 0x000459E0 File Offset: 0x00043BE0
		public Guid ApplicationGuid
		{
			get
			{
				return Guid.Empty;
			}
			set
			{
			}
		}

		// Token: 0x060016CF RID: 5839 RVA: 0x000459E4 File Offset: 0x00043BE4
		public ISignature2 \u0001(IExpression \u0002, out string \u0003)
		{
			\u0003 = null;
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature2 signature = u.FindSignatureGlobal(\u0002, out \u0003);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016D0 RID: 5840 RVA: 0x00045A40 File Offset: 0x00043C40
		public string \u0002(string \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				string @namespace = u.GetNamespace(\u0002);
				if (!string.IsNullOrEmpty(@namespace))
				{
					return @namespace;
				}
			}
			return null;
		}

		// Token: 0x060016D1 RID: 5841 RVA: 0x00045A9C File Offset: 0x00043C9C
		public ISignature \u0001(IExpression \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignatureGlobal(\u0002);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016D2 RID: 5842 RVA: 0x00045AF4 File Offset: 0x00043CF4
		public bool \u0001(string \u0002, out IVariable \u0003, out ISignature \u0004, out IPrecompileScope \u0005)
		{
			\u0003 = null;
			\u0004 = null;
			\u0005 = null;
			using (IEnumerator<global::\u0015.\u0002> enumerator = this.\u0001.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.FindDeclaration(\u0002, out \u0003, out \u0004, out \u0005))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060016D3 RID: 5843 RVA: 0x00045B58 File Offset: 0x00043D58
		public string \u0001(string \u0002)
		{
			return null;
		}

		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x060016D4 RID: 5844 RVA: 0x00045B5C File Offset: 0x00043D5C
		public ISignature LocalSignature
		{
			get
			{
				return this.\u0001[0].LocalSignature;
			}
		}

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x060016D5 RID: 5845 RVA: 0x00045B70 File Offset: 0x00043D70
		public ISignature MostLocalSignature
		{
			get
			{
				return this.\u0001[0].MostLocalSignature;
			}
		}

		// Token: 0x060016D6 RID: 5846 RVA: 0x00045B84 File Offset: 0x00043D84
		public ISignature \u0001(string \u0002)
		{
			return this.\u0001[0].FindSignatureLocal(\u0002);
		}

		// Token: 0x060016D7 RID: 5847 RVA: 0x00045B98 File Offset: 0x00043D98
		public ISignature \u0001(IQualifiedNameExpression \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignatureGlobal(\u0002);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016D8 RID: 5848 RVA: 0x00045BF0 File Offset: 0x00043DF0
		public ISignature \u0002(string \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignatureGlobal(\u0002);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016D9 RID: 5849 RVA: 0x00045C48 File Offset: 0x00043E48
		public IPrecompileScope \u0001()
		{
			return this.\u0001();
		}

		// Token: 0x060016DA RID: 5850 RVA: 0x00045C50 File Offset: 0x00043E50
		public IPrecompileScope \u0001(string \u0002)
		{
			ISignature[] array = this.\u0001(\u0002);
			if (array.Length == 1)
			{
				return this.\u0001(array[0]);
			}
			return null;
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x00045C78 File Offset: 0x00043E78
		public IPrecompileScope \u0001(ISignature \u0002)
		{
			return this.\u0001(\u0002 as _ISignature);
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x00045C88 File Offset: 0x00043E88
		public IIdentifierInfo[] \u0001(bool \u0002, bool \u0003)
		{
			LList<IIdentifierInfo> llist = new LList<IIdentifierInfo>();
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				llist.AddRange(u.GetAllDeclarations(\u0002, \u0003));
			}
			return llist.ToArray();
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x00045CE8 File Offset: 0x00043EE8
		public IIdentifierInfo[] \u0001()
		{
			return this.\u0001(true, false);
		}

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x060016DE RID: 5854 RVA: 0x00045CF4 File Offset: 0x00043EF4
		public int PointerSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x00045CF8 File Offset: 0x00043EF8
		public int \u0001(IType \u0002)
		{
			return this.\u0001[0].GetSize(\u0002);
		}

		// Token: 0x060016E0 RID: 5856 RVA: 0x00045D0C File Offset: 0x00043F0C
		public int \u0001(IType \u0002, IRecursionGuard \u0003)
		{
			ICommonScope2 commonScope = this.\u0001[0] as ICommonScope2;
			if (commonScope != null)
			{
				return commonScope.GetSize(\u0002, \u0003);
			}
			return commonScope.GetSize(\u0002);
		}

		// Token: 0x060016E1 RID: 5857 RVA: 0x00045D40 File Offset: 0x00043F40
		public bool \u0001(_IExpression \u0002)
		{
			return global::\u0013.\u0004.\u0001((_IExprement3)\u0002, true);
		}

		// Token: 0x060016E2 RID: 5858 RVA: 0x00045D50 File Offset: 0x00043F50
		public bool \u0002(_IExpression \u0002)
		{
			return global::\u0013.\u0004.\u0001((_IExprement3)\u0002, false);
		}

		// Token: 0x060016E3 RID: 5859 RVA: 0x00045D60 File Offset: 0x00043F60
		public _IVariable \u0001(_IExpression \u0002)
		{
			return (_IVariable)\u0002.GetVariable(this);
		}

		// Token: 0x060016E4 RID: 5860 RVA: 0x00045D70 File Offset: 0x00043F70
		public _ISignature \u0001(_IExpression \u0002)
		{
			return (_ISignature)APEnvironmentFacade.Instance.LanguageModelMgr.GetSignatureForPrecompileID(\u0002.PrecompileSignatureId);
		}

		// Token: 0x060016E5 RID: 5861 RVA: 0x00045D8C File Offset: 0x00043F8C
		public _IExpression \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (this.\u0001.Count > 0)
			{
				_IPrecompileScope3 iprecompileScope = this.\u0001[0] as _IPrecompileScope3;
				if (iprecompileScope != null)
				{
					return CheckerScope.\u0001(iprecompileScope, \u0002, \u0003);
				}
			}
			return \u0003._Initial;
		}

		// Token: 0x060016E6 RID: 5862 RVA: 0x00045DCC File Offset: 0x00043FCC
		public global::\u0017.\u0006 \u0001(_IExpression \u0002, _IUserdefType \u0003)
		{
			Tuple<global::\u0015.\u0002, ISignature> tuple = this.\u0001(\u0003);
			if (tuple != null)
			{
				return (global::\u0017.\u0006)tuple.Item1.\u0001((_ISignature)tuple.Item2);
			}
			return this;
		}

		// Token: 0x060016E7 RID: 5863 RVA: 0x00045E04 File Offset: 0x00044004
		public global::\u0017.\u0006 \u0001(_ISignature \u0002)
		{
			return (global::\u0017.\u0006)this.\u0001(\u0002);
		}

		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x060016E8 RID: 5864 RVA: 0x00045E14 File Offset: 0x00044014
		public bool ContainsCopyCode
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x00045E18 File Offset: 0x00044018
		public void \u0001(_IExpression \u0002, out _IVariable \u0003, out _ISignature \u0004, out global::\u0017.\u0006 \u0005)
		{
			IVariable variable;
			ISignature signature;
			IPrecompileScope precompileScope;
			this.\u0001(\u0002.ToString(), out variable, out signature, out precompileScope);
			\u0005 = (global::\u0017.\u0006)precompileScope;
			\u0003 = (_IVariable)variable;
			\u0004 = (_ISignature)signature;
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x00045E54 File Offset: 0x00044054
		public ISignature \u0001(IUserdefType \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignature(\u0002);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x00045EAC File Offset: 0x000440AC
		public Tuple<global::\u0015.\u0002, ISignature> \u0001(IUserdefType \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignature(\u0002);
				if (signature != null)
				{
					return new Tuple<global::\u0015.\u0002, ISignature>(u, signature);
				}
			}
			return null;
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x00045F0C File Offset: 0x0004410C
		public ISignature[] \u0001(IExpression \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature[] array = u.FindSignature(\u0002);
				if (array != null)
				{
					return array;
				}
			}
			return null;
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x00045F64 File Offset: 0x00044164
		public ISignature \u0001(IEnumType \u0002)
		{
			foreach (global::\u0015.\u0002 u in this.\u0001)
			{
				ISignature signature = u.FindSignature(\u0002);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x00045FBC File Offset: 0x000441BC
		public ILiteralValue \u0001(IExpression \u0002, bool \u0003)
		{
			bool flag;
			return (\u0002 as _IExpression).LiteralWithRecursionCheck(this, new LDictionary<IVariable, IVariable>(), \u0003, out flag);
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x00045FE0 File Offset: 0x000441E0
		public bool \u0001(ISignature \u0002, ISignature \u0003, ICommonScope \u0004)
		{
			_ISignature isignature = \u0002 as _ISignature;
			_ISignature isignature2 = \u0003 as _ISignature;
			return isignature != null && isignature2 != null && (isignature.ObjectGuid == isignature2.ObjectGuid || (isignature.BaseExpression != null || isignature2.BaseExpression != null));
		}

		// Token: 0x060016F0 RID: 5872 RVA: 0x0004602C File Offset: 0x0004422C
		public bool \u0001(IUserdefType \u0002, IUserdefType \u0003)
		{
			string a = (\u0002 as _IUserdefType).NameExpression.ToString();
			string b = (\u0003 as _IUserdefType).NameExpression.ToString();
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x060016F1 RID: 5873 RVA: 0x00046064 File Offset: 0x00044264
		public bool IsPrecompileScope
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060016F2 RID: 5874 RVA: 0x00046068 File Offset: 0x00044268
		public ISignature \u0001(ISignature \u0002, out IPrecompileScope7 \u0003)
		{
			global::\u0015.\u0002 u;
			_ISignature isignature = this.\u0001(\u0002 as _ISignature, out u);
			if (isignature != null)
			{
				\u0003 = u;
				return isignature;
			}
			\u0003 = this;
			return \u0002;
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x00046090 File Offset: 0x00044290
		public ICompiledType \u0001(ICompiledType \u0002, out IPrecompileScope7 \u0003)
		{
			global::\u0015.\u0002 u;
			ICompiledType result = this.\u0001(\u0002 as _IType, out u);
			\u0003 = u;
			return result;
		}

		// Token: 0x040003FC RID: 1020
		private readonly LList<global::\u0015.\u0002> \u0001;
	}
}
