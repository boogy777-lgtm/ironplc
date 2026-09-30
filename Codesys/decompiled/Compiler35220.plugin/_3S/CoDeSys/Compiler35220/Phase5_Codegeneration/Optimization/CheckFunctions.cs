using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration.Optimization
{
	// Token: 0x0200026A RID: 618
	public class CheckFunctions
	{
		// Token: 0x060027AB RID: 10155 RVA: 0x00089814 File Offset: 0x00087A14
		public CheckFunctions(_ICompileContext comcon)
		{
			foreach (ISignature u in comcon.AllSignatures)
			{
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_BOUNDS, ref this.m_stCheckBoundsFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_POINTER, ref this.m_stCheckPointerFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_RANGE_UNSIGNED, ref this.m_stCheckRangeUnsignedFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_RANGE_SIGNED, ref this.m_stCheckRangeSignedFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_LRANGE_UNSIGNED, ref this.m_stCheckLRangeUnsignedFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_LRANGE_SIGNED, ref this.m_stCheckLRangeSignedFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL64, ref this.m_stCheckDivReal64Fun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL32, ref this.m_stCheckDivRealFun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_DIV_INT64, ref this.m_stCheckDivInt64Fun);
				this.\u0001(u, CompileAttributes.ATTRIBUTE_CHECK_DIV_INT32, ref this.m_stCheckDivInt32Fun);
			}
			if (this.\u0001.Count != 0)
			{
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_BOUNDS, ref this.m_stCheckBoundsFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_POINTER, ref this.m_stCheckPointerFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_RANGE_UNSIGNED, ref this.m_stCheckRangeUnsignedFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_RANGE_SIGNED, ref this.m_stCheckRangeSignedFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_LRANGE_UNSIGNED, ref this.m_stCheckLRangeUnsignedFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_LRANGE_SIGNED, ref this.m_stCheckLRangeSignedFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL64, ref this.m_stCheckDivReal64Fun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_DIV_REAL32, ref this.m_stCheckDivRealFun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_DIV_INT64, ref this.m_stCheckDivInt64Fun);
				this.\u0001(CompileAttributes.ATTRIBUTE_CHECK_DIV_INT32, ref this.m_stCheckDivInt32Fun);
			}
		}

		// Token: 0x060027AC RID: 10156 RVA: 0x000899E8 File Offset: 0x00087BE8
		private void \u0001(string \u0002, ref string \u0003)
		{
			CheckFunctions.\u0004 u = new CheckFunctions.\u0004();
			u.\u0001 = \u0002;
			if (this.\u0001.Any(new Func<CheckFunctions.\u0003, bool>(u.\u0001)))
			{
				\u0003 = null;
			}
		}

		// Token: 0x060027AD RID: 10157 RVA: 0x00089A20 File Offset: 0x00087C20
		public bool CheckForCheckFunHide(IScope5 scope, string stCheckFunName)
		{
			_ISignature isignature = scope.FindSignatureLocal(stCheckFunName) as _ISignature;
			if (isignature != null)
			{
				Severity u0014_u = Messages.\u0001(isignature, MessageId.Wrn_ImplicitCheckFunctionShadowed);
				string stAttribute = this.\u0001[stCheckFunName];
				if (!isignature.HasAttribute(stAttribute))
				{
					this.\u0001.Add(new CheckFunctions.\u0001(scope.LocalSignature, isignature, null, stCheckFunName, u0014_u));
					return false;
				}
			}
			ISignature u0004_u;
			IVariable variable = scope.FindVariableLocal(stCheckFunName, out u0004_u);
			if (variable == null)
			{
				ISignature[] array2;
				IVariable[] array = scope.FindVariableGlobal(stCheckFunName, out array2);
				if (array != null && array.Length != 0)
				{
					variable = array[0];
					u0004_u = array2[0];
				}
			}
			if (variable != null)
			{
				Severity u0014_u2 = Messages.\u0001(variable, MessageId.Wrn_ImplicitCheckFunctionShadowed);
				this.\u0001.Add(new CheckFunctions.\u0001(scope.LocalSignature, u0004_u, variable, stCheckFunName, u0014_u2));
				return false;
			}
			return true;
		}

		// Token: 0x060027AE RID: 10158 RVA: 0x00089AD8 File Offset: 0x00087CD8
		public void ClearMessages()
		{
			this.\u0001.Clear();
		}

		// Token: 0x060027AF RID: 10159 RVA: 0x00089AE8 File Offset: 0x00087CE8
		public List<_ICompilerMessage> GetAllMessages()
		{
			List<_ICompilerMessage> list = new List<_ICompilerMessage>();
			foreach (CheckFunctions.\u0001 u in this.\u0001)
			{
				Severity u2 = u.\u0001;
				string u3 = string.Format(\u0018.\u0001(MessageId.Wrn_ImplicitCheckFunctionShadowed), u.\u0001);
				_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(u.\u0001.NameExpression.Position, u3, u2, MessageId.Wrn_ImplicitCheckFunctionShadowed);
				icompilerMessage.ObjectGuid = u.\u0001.ObjectGuid;
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(u.\u0001.LibraryPath);
				ISourcePosition u4;
				if (u.\u0001 != null)
				{
					u4 = u.\u0001.SourcePosition;
				}
				else
				{
					u4 = u.\u0002.NameExpression.Position;
				}
				Severity u5 = Severity.Information;
				if (u2 == Severity.SuppressedWarning)
				{
					u5 = Severity.SuppressedInformation;
				}
				_ICompilerMessage icompilerMessage2 = \u0019.\u0003.\u0001(u4, \u0018.\u0001(MessageId.Inf_RelatedPosition), u5, MessageId.Inf_RelatedPosition);
				icompilerMessage2.ObjectGuid = u.\u0002.ObjectGuid;
				icompilerMessage2.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(u.\u0002.LibraryPath);
				list.Add(icompilerMessage);
				list.Add(icompilerMessage2);
			}
			this.\u0001(list);
			return list;
		}

		// Token: 0x060027B0 RID: 10160 RVA: 0x00089C60 File Offset: 0x00087E60
		private void \u0001(List<_ICompilerMessage> \u0002)
		{
			foreach (CheckFunctions.\u0003 u in this.\u0001)
			{
				Severity severity = u.Severity;
				string u2 = string.Format(\u0018.\u0001(MessageId.Wrn_AmbiguousCheckfunctionInLibrary), CheckFunctions.\u0001(u.LocalSignature), CheckFunctions.\u0001(u.AmbiguousCheckFunctionSignature));
				_ICompilerMessage icompilerMessage = \u0019.\u0003.\u0001(u.LocalSignature.NameExpression.Position, u2, severity, MessageId.Wrn_AmbiguousCheckfunctionInLibrary);
				icompilerMessage.ObjectGuid = u.LocalSignature.ObjectGuid;
				icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(u.LocalSignature.LibraryPath);
				ISourcePosition position = u.AmbiguousCheckFunctionSignature.NameExpression.Position;
				Severity u3 = (Severity.SuppressedWarning == severity) ? Severity.SuppressedInformation : Severity.Information;
				_ICompilerMessage icompilerMessage2 = \u0019.\u0003.\u0001(position, \u0018.\u0001(MessageId.Inf_RelatedPosition), u3, MessageId.Inf_RelatedPosition);
				icompilerMessage2.ObjectGuid = u.AmbiguousCheckFunctionSignature.ObjectGuid;
				icompilerMessage2.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(u.AmbiguousCheckFunctionSignature.LibraryPath);
				\u0002.Add(icompilerMessage);
				\u0002.Add(icompilerMessage2);
			}
		}

		// Token: 0x060027B1 RID: 10161 RVA: 0x00089DC8 File Offset: 0x00087FC8
		private void \u0001(ISignature \u0002, string \u0003, ref string \u0004)
		{
			if (\u0002.HasAttribute(\u0003))
			{
				string u;
				\u0004 = CheckFunctions.\u0001(\u0002, out u);
				CheckFunctions.\u0002 value = new CheckFunctions.\u0002
				{
					Signature = \u0002,
					Namespace = u
				};
				CheckFunctions.\u0002 u2;
				if (this.\u0001.TryGetValue(\u0004, out u2) && !string.IsNullOrEmpty(value.Namespace) && !string.IsNullOrEmpty(u2.Namespace))
				{
					Severity severity = Messages.\u0001((_ISignature)value.Signature, MessageId.Wrn_AmbiguousCheckfunctionInLibrary);
					Severity severity2 = Messages.\u0001((_ISignature)u2.Signature, MessageId.Wrn_AmbiguousCheckfunctionInLibrary);
					Severity u0014_u = (Severity.SuppressedWarning == severity || Severity.SuppressedWarning == severity2) ? Severity.SuppressedWarning : Severity.Warning;
					this.\u0001.Add(new CheckFunctions.\u0003((_ISignature)value.Signature, (_ISignature)u2.Signature, \u0003, u0014_u));
				}
				this.\u0001[\u0002.Name] = \u0003;
				this.\u0001[\u0004] = value;
			}
		}

		// Token: 0x060027B2 RID: 10162 RVA: 0x00089EC4 File Offset: 0x000880C4
		public ISignature GetSignature(string stCheckFunction)
		{
			CheckFunctions.\u0002 u;
			if (this.\u0001.TryGetValue(stCheckFunction, out u))
			{
				return u.Signature;
			}
			return null;
		}

		// Token: 0x060027B3 RID: 10163 RVA: 0x00089EEC File Offset: 0x000880EC
		private static string \u0001(ISignature \u0002)
		{
			string text;
			string result = CheckFunctions.\u0001(\u0002, out text);
			if (!string.IsNullOrEmpty(text))
			{
				result = text + "#" + \u0002.Name;
			}
			return result;
		}

		// Token: 0x060027B4 RID: 10164 RVA: 0x00089F20 File Offset: 0x00088120
		private static string \u0001(ISignature \u0002, out string \u0003)
		{
			string name = \u0002.Name;
			\u0003 = string.Empty;
			if (!string.IsNullOrEmpty(\u0002.LibraryPath))
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(\u0002.LibraryPath);
				if (libraryContext != null)
				{
					\u0003 = libraryContext.Namespace;
				}
			}
			return name;
		}

		// Token: 0x060027B5 RID: 10165 RVA: 0x00089F70 File Offset: 0x00088170
		internal bool \u0001(_ICompileContext \u0002, _ICompiledPOU \u0003, bool \u0004, IScope5 \u0005)
		{
			bool flag = false;
			ISignature signature = CheckFunctions.\u0001(\u0003, \u0005, ref flag);
			if (\u0003 == null || (\u0004 && !flag))
			{
				return false;
			}
			if (signature == null)
			{
				return false;
			}
			if (signature.GetFlag(SignatureFlag.Action))
			{
				signature = \u0005[signature.ParentSignatureId];
			}
			return signature != null && this.\u0001(\u0002, signature as _ISignature) && !signature.GetFlag(SignatureFlag.Generated) && !signature.HasAttribute(CompileAttributes.ATTRIBUTE_NO_CHECK);
		}

		// Token: 0x060027B6 RID: 10166 RVA: 0x00089FEC File Offset: 0x000881EC
		private static ISignature \u0001(_ICompiledPOU \u0002, IScope5 \u0003, ref bool \u0004)
		{
			ISignature signature = null;
			if (\u0002 != null)
			{
				signature = \u0003[\u0002.SignatureId];
				if (signature != null && signature.Name == IdentifierConstants.MainSignatureName)
				{
					signature = \u0003[signature.ParentSignatureId];
				}
				if (signature != null && signature.HasAttribute(CompileAttributes.DO_CHECKS_FOR_IMPLICIT_CODE))
				{
					\u0004 = true;
				}
			}
			return signature;
		}

		// Token: 0x060027B7 RID: 10167 RVA: 0x0008A044 File Offset: 0x00088244
		private bool \u0001(_ICompileContext \u0002, _ISignature \u0003)
		{
			if (string.IsNullOrEmpty(\u0003.LibraryPath))
			{
				return true;
			}
			if (!\u0002.IsDefined(CompileAttributes.ATTRIBUTE_CHECKS_IN_LIBS))
			{
				return false;
			}
			bool isCompiledLibraryObject = \u0003.IsCompiledLibraryObject;
			bool result = !isCompiledLibraryObject;
			if (isCompiledLibraryObject)
			{
				_IPreCompileContext contextByLibraryPath = \u0002.GetContextByLibraryPath(\u0003.LibraryPath);
				if (contextByLibraryPath != null && contextByLibraryPath.IsDefined("AllowChecks"))
				{
					result = true;
				}
			}
			return result;
		}

		// Token: 0x060027B8 RID: 10168 RVA: 0x0008A0A4 File Offset: 0x000882A4
		public string GetCheckRangeFunction(bool bLInt, bool bSigned)
		{
			if (bLInt)
			{
				if (!bSigned)
				{
					return this.m_stCheckLRangeUnsignedFun;
				}
				return this.m_stCheckLRangeSignedFun;
			}
			else
			{
				if (!bSigned)
				{
					return this.m_stCheckRangeUnsignedFun;
				}
				return this.m_stCheckRangeSignedFun;
			}
		}

		// Token: 0x04000739 RID: 1849
		public readonly string m_stCheckBoundsFun;

		// Token: 0x0400073A RID: 1850
		public readonly string m_stCheckPointerFun;

		// Token: 0x0400073B RID: 1851
		public readonly string m_stCheckRangeUnsignedFun;

		// Token: 0x0400073C RID: 1852
		public readonly string m_stCheckRangeSignedFun;

		// Token: 0x0400073D RID: 1853
		public readonly string m_stCheckLRangeUnsignedFun;

		// Token: 0x0400073E RID: 1854
		public readonly string m_stCheckLRangeSignedFun;

		// Token: 0x0400073F RID: 1855
		public readonly string m_stCheckDivReal64Fun;

		// Token: 0x04000740 RID: 1856
		public readonly string m_stCheckDivRealFun;

		// Token: 0x04000741 RID: 1857
		public readonly string m_stCheckDivInt64Fun;

		// Token: 0x04000742 RID: 1858
		public readonly string m_stCheckDivInt32Fun;

		// Token: 0x04000743 RID: 1859
		private readonly Dictionary<string, string> \u0001 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x04000744 RID: 1860
		private readonly Dictionary<string, CheckFunctions.\u0002> \u0001 = new Dictionary<string, CheckFunctions.\u0002>();

		// Token: 0x04000745 RID: 1861
		private readonly HashSet<CheckFunctions.\u0001> \u0001 = new HashSet<CheckFunctions.\u0001>();

		// Token: 0x04000746 RID: 1862
		private readonly HashSet<CheckFunctions.\u0003> \u0001 = new HashSet<CheckFunctions.\u0003>();

		// Token: 0x0200026B RID: 619
		private struct \u0001
		{
			// Token: 0x060027B9 RID: 10169 RVA: 0x0008A0CC File Offset: 0x000882CC
			public \u0001(ISignature \u0018\u0003, ISignature \u0004\u0005, IVariable \u0005\u0005, string \u0006\u0005, Severity \u0014\u0008)
			{
				this.\u0001 = (_ISignature)\u0018\u0003;
				this.\u0002 = (_ISignature)\u0004\u0005;
				this.\u0001 = (_IVariable)\u0005\u0005;
				this.\u0001 = \u0006\u0005;
				this.\u0001 = \u0014\u0008;
			}

			// Token: 0x04000747 RID: 1863
			public readonly _ISignature \u0001;

			// Token: 0x04000748 RID: 1864
			public readonly _ISignature \u0002;

			// Token: 0x04000749 RID: 1865
			public readonly _IVariable \u0001;

			// Token: 0x0400074A RID: 1866
			public readonly string \u0001;

			// Token: 0x0400074B RID: 1867
			public readonly Severity \u0001;
		}

		// Token: 0x0200026C RID: 620
		internal struct \u0002
		{
			// Token: 0x17000721 RID: 1825
			// (get) Token: 0x060027BA RID: 10170 RVA: 0x0008A104 File Offset: 0x00088304
			// (set) Token: 0x060027BB RID: 10171 RVA: 0x0008A10C File Offset: 0x0008830C
			internal ISignature Signature { get; set; }

			// Token: 0x17000722 RID: 1826
			// (get) Token: 0x060027BC RID: 10172 RVA: 0x0008A118 File Offset: 0x00088318
			// (set) Token: 0x060027BD RID: 10173 RVA: 0x0008A120 File Offset: 0x00088320
			internal string Namespace { get; set; }

			// Token: 0x0400074C RID: 1868
			[CompilerGenerated]
			private ISignature \u0001;

			// Token: 0x0400074D RID: 1869
			[CompilerGenerated]
			private string \u0001;
		}

		// Token: 0x0200026D RID: 621
		internal struct \u0003
		{
			// Token: 0x17000723 RID: 1827
			// (get) Token: 0x060027BE RID: 10174 RVA: 0x0008A12C File Offset: 0x0008832C
			// (set) Token: 0x060027BF RID: 10175 RVA: 0x0008A134 File Offset: 0x00088334
			internal _ISignature LocalSignature { get; private set; }

			// Token: 0x17000724 RID: 1828
			// (get) Token: 0x060027C0 RID: 10176 RVA: 0x0008A140 File Offset: 0x00088340
			// (set) Token: 0x060027C1 RID: 10177 RVA: 0x0008A148 File Offset: 0x00088348
			internal _ISignature AmbiguousCheckFunctionSignature { get; private set; }

			// Token: 0x17000725 RID: 1829
			// (get) Token: 0x060027C2 RID: 10178 RVA: 0x0008A154 File Offset: 0x00088354
			// (set) Token: 0x060027C3 RID: 10179 RVA: 0x0008A15C File Offset: 0x0008835C
			internal string CheckFunction { get; set; }

			// Token: 0x17000726 RID: 1830
			// (get) Token: 0x060027C4 RID: 10180 RVA: 0x0008A168 File Offset: 0x00088368
			// (set) Token: 0x060027C5 RID: 10181 RVA: 0x0008A170 File Offset: 0x00088370
			internal Severity Severity { get; set; }

			// Token: 0x060027C6 RID: 10182 RVA: 0x0008A17C File Offset: 0x0008837C
			internal \u0003(_ISignature \u0083\u0008, _ISignature \u0084\u0008, string \u0086\u0008, Severity \u0014\u0008)
			{
				this.LocalSignature = \u0083\u0008;
				this.AmbiguousCheckFunctionSignature = \u0084\u0008;
				this.CheckFunction = \u0086\u0008;
				this.Severity = \u0014\u0008;
			}

			// Token: 0x0400074E RID: 1870
			[CompilerGenerated]
			private _ISignature \u0001;

			// Token: 0x0400074F RID: 1871
			[CompilerGenerated]
			private _ISignature \u0002;

			// Token: 0x04000750 RID: 1872
			[CompilerGenerated]
			private string \u0001;

			// Token: 0x04000751 RID: 1873
			[CompilerGenerated]
			private Severity \u0001;
		}

		// Token: 0x0200026E RID: 622
		[CompilerGenerated]
		private sealed class \u0004
		{
			// Token: 0x060027C8 RID: 10184 RVA: 0x0008A1A4 File Offset: 0x000883A4
			internal bool \u0001(CheckFunctions.\u0003 \u0002)
			{
				return \u0002.CheckFunction == this.\u0001;
			}

			// Token: 0x04000752 RID: 1874
			public string \u0001;
		}
	}
}
