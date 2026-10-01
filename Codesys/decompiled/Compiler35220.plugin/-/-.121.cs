using System;
using System.Linq;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0005
{
	// Token: 0x02000169 RID: 361
	internal sealed class \u0002
	{
		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x060018A4 RID: 6308 RVA: 0x0004C6B4 File Offset: 0x0004A8B4
		internal object WorkQueueLocker
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x060018A5 RID: 6309 RVA: 0x0004C6BC File Offset: 0x0004A8BC
		public bool Empty
		{
			get
			{
				return this.\u0002() == null;
			}
		}

		// Token: 0x060018A6 RID: 6310 RVA: 0x0004C6DC File Offset: 0x0004A8DC
		internal void \u0001()
		{
			LHashSet<Guid> u = new LHashSet<Guid>(APEnvironmentFacade.Instance.GetOpenEditorSignatures());
			foreach (_IPreCompileContext ipreCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(true, true).OfType<_IPreCompileContext>())
			{
				if (ipreCompileContext.Dirty && this.\u0001(u, ipreCompileContext, true))
				{
					ipreCompileContext.Dirty = false;
				}
			}
		}

		// Token: 0x060018A7 RID: 6311 RVA: 0x0004C75C File Offset: 0x0004A95C
		internal void \u0002()
		{
			_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			foreach (_ISignature isignature in pool.AllFlat)
			{
				isignature.SetFlagInternal(SignatureFlagInternal.Checked, false);
			}
			pool.Dirty = true;
		}

		// Token: 0x060018A8 RID: 6312 RVA: 0x0004C7C0 File Offset: 0x0004A9C0
		private bool \u0001(LHashSet<Guid> \u0002, _IPreCompileContext \u0003, bool \u0004)
		{
			foreach (_ISignature isignature in \u0003.AllFlat)
			{
				if (!isignature.IsCompiledLibraryObject && !isignature.GetFlagInternal(SignatureFlagInternal.Checked))
				{
					\u0004 = false;
					LanguageModelResult languageModelResult = new LanguageModelResult(\u0003, isignature);
					if (!this.\u0001.Contains(languageModelResult))
					{
						if (\u0005.\u0002.\u0001(isignature))
						{
							\u0003.SetSignatureChecked(isignature);
						}
						else if (\u0002.Contains(isignature.ObjectGuid))
						{
							this.\u0002(languageModelResult);
						}
						else
						{
							this.\u0003(languageModelResult);
						}
					}
				}
			}
			return \u0004;
		}

		// Token: 0x060018A9 RID: 6313 RVA: 0x0004C868 File Offset: 0x0004AA68
		internal void \u0001(LanguageModelResult \u0002)
		{
			if (\u0002.\u0001 != null && \u0005.\u0002.\u0001(\u0002.\u0001))
			{
				return;
			}
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001.Insert(0, \u0002);
				if (this.\u0001.Count == 20)
				{
					this.\u0001.RemoveAt(19);
				}
			}
		}

		// Token: 0x060018AA RID: 6314 RVA: 0x0004C8E8 File Offset: 0x0004AAE8
		private void \u0002(LanguageModelResult \u0002)
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0002.Add(\u0002);
			}
		}

		// Token: 0x060018AB RID: 6315 RVA: 0x0004C930 File Offset: 0x0004AB30
		private void \u0003(LanguageModelResult \u0002)
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001.Add(\u0002);
			}
		}

		// Token: 0x060018AC RID: 6316 RVA: 0x0004C97C File Offset: 0x0004AB7C
		internal LanguageModelResult? \u0001()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001.Count > 0)
				{
					LanguageModelResult value = this.\u0001[0];
					this.\u0001.RemoveAt(0);
					return new LanguageModelResult?(value);
				}
				if (this.\u0002.Count > 0)
				{
					LanguageModelResult value2 = this.\u0002[0];
					this.\u0002.RemoveAt(0);
					return new LanguageModelResult?(value2);
				}
				if (this.\u0001.Count > 0)
				{
					LanguageModelResult languageModelResult = this.\u0001.First<LanguageModelResult>();
					this.\u0001.Remove(languageModelResult);
					return new LanguageModelResult?(languageModelResult);
				}
			}
			return null;
		}

		// Token: 0x060018AD RID: 6317 RVA: 0x0004CA64 File Offset: 0x0004AC64
		internal LanguageModelResult? \u0002()
		{
			object u = this.\u0001;
			lock (u)
			{
				if (this.\u0001.Count > 0)
				{
					return new LanguageModelResult?(this.\u0001[0]);
				}
				if (this.\u0002.Count > 0)
				{
					return new LanguageModelResult?(this.\u0002[0]);
				}
				if (this.\u0001.Count > 0)
				{
					return new LanguageModelResult?(this.\u0001.First<LanguageModelResult>());
				}
			}
			return null;
		}

		// Token: 0x060018AE RID: 6318 RVA: 0x0004CB1C File Offset: 0x0004AD1C
		internal void \u0003()
		{
			object u = this.\u0001;
			lock (u)
			{
				this.\u0001.Clear();
				this.\u0002.Clear();
				this.\u0001.Clear();
			}
			IMessageCategory precompileMessageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.PrecompileMessageCategory;
			APEnvironmentFacade.Instance.ClearMessages(precompileMessageCategory);
		}

		// Token: 0x060018AF RID: 6319 RVA: 0x0004CB98 File Offset: 0x0004AD98
		internal static bool \u0001(_ISignature \u0002)
		{
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_FORCE_PRECOMPILE_CHECKS))
			{
				return false;
			}
			if (\u0002.HasAttribute("ioconfig_pou"))
			{
				return true;
			}
			GUIHidingFlags guihidingFlags = GUIHidingFlags.AllCommon;
			if (\u0005.\u0002.\u0002(\u0002))
			{
				guihidingFlags &= ~GUIHidingFlags.EvaluateImplicitNames;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(\u0002, guihidingFlags) && !\u0005.\u0002.\u0001(\u0002);
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x0004CC00 File Offset: 0x0004AE00
		private static bool \u0002(_ISignature \u0002)
		{
			return \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY) || \u0002.Name.EndsWith("__VIS") || \u0002.Name.EndsWith("__PRT_OPCUA_TOUCHVARS");
		}

		// Token: 0x060018B1 RID: 6321 RVA: 0x0004CC3C File Offset: 0x0004AE3C
		private static bool \u0001(ISignature \u0002)
		{
			return \u0002.Name.StartsWith("CALLTASK__");
		}

		// Token: 0x04000458 RID: 1112
		private volatile LList<LanguageModelResult> \u0001 = new LList<LanguageModelResult>();

		// Token: 0x04000459 RID: 1113
		private volatile LList<LanguageModelResult> \u0002 = new LList<LanguageModelResult>();

		// Token: 0x0400045A RID: 1114
		private volatile LHashSet<LanguageModelResult> \u0001 = new LHashSet<LanguageModelResult>();

		// Token: 0x0400045B RID: 1115
		private readonly object \u0001 = new object();
	}
}
