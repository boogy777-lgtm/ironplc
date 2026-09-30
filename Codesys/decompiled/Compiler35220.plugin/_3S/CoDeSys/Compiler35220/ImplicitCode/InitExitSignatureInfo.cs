using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u000E;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0083;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003DA RID: 986
	internal sealed class InitExitSignatureInfo
	{
		// Token: 0x0600373E RID: 14142 RVA: 0x000E3094 File Offset: 0x000E1294
		internal InitExitSignatureInfo(_ICompileContext comcon)
		{
			InitExitSignatureInfo.\u0001 u = new InitExitSignatureInfo.\u0001();
			Dictionary<string, InitExitSignatureInfo.\u0001> dictionary = new Dictionary<string, InitExitSignatureInfo.\u0001>(StringComparer.OrdinalIgnoreCase);
			foreach (_ISignature isignature in comcon.AllFlat)
			{
				if (!isignature.GetFlag(SignatureFlag.NoInit))
				{
					if (isignature.GetFlagInternal(SignatureFlagInternal.CallWithinGlobalInitExit))
					{
						u.\u0002(isignature);
					}
					else
					{
						if (isignature.POUType != Operator.Program && isignature.POUType != Operator.VarGlobal)
						{
							if (!isignature.AllVariables.Any(new Func<_IVariable, bool>(InitExitSignatureInfo.<>c.<>9.\u0001)))
							{
								continue;
							}
						}
						string key;
						if (InitExitSignatureInfo.\u0001(isignature, out key))
						{
							InitExitSignatureInfo.\u0001 u2;
							if (!dictionary.TryGetValue(key, out u2))
							{
								u2 = new InitExitSignatureInfo.\u0001();
								dictionary.Add(key, u2);
							}
							u2.\u0001(isignature);
						}
						else
						{
							u.\u0001(isignature);
						}
					}
				}
			}
			this.NormalSignatures = u.\u0001();
			this.\u0001 = new Dictionary<string, LList<_ISignature>>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, InitExitSignatureInfo.\u0001> keyValuePair in dictionary)
			{
				this.\u0001.Add(keyValuePair.Key, this.\u0001(keyValuePair.Value.\u0001()));
			}
		}

		// Token: 0x0600373F RID: 14143 RVA: 0x000E320C File Offset: 0x000E140C
		private static bool \u0001(_ISignature \u0002, out string \u0003)
		{
			Debug.\u0001(\u0002 != null, "sign != null");
			if (\u0002.GetFlagInternal(SignatureFlagInternal.ExplicitInitExitHandling))
			{
				\u0003 = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_EXPLICIT_INIT_EXIT_HANDLING);
				return \u0003 != string.Empty;
			}
			\u0003 = string.Empty;
			return false;
		}

		// Token: 0x170008F4 RID: 2292
		// (get) Token: 0x06003740 RID: 14144 RVA: 0x000E324C File Offset: 0x000E144C
		internal LList<\u0019> NormalSignatures { get; }

		// Token: 0x170008F5 RID: 2293
		// (get) Token: 0x06003741 RID: 14145 RVA: 0x000E3254 File Offset: 0x000E1454
		internal LList<_ISignature> NormalSignaturesWithoutSlotCalls
		{
			get
			{
				return this.\u0001(this.NormalSignatures);
			}
		}

		// Token: 0x170008F6 RID: 2294
		// (get) Token: 0x06003742 RID: 14146 RVA: 0x000E3264 File Offset: 0x000E1464
		internal IDictionary<string, LList<_ISignature>> ExplicitSignatures
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x06003743 RID: 14147 RVA: 0x000E326C File Offset: 0x000E146C
		internal LList<_ISignature> \u0001(IEnumerable<\u0019> \u0002)
		{
			Debug.\u0001(\u0002 != null, "orgList != null");
			LList<_ISignature> llist = new LList<_ISignature>();
			foreach (\u0019 u in \u0002)
			{
				\u0083.\u0010 u2 = u as \u0083.\u0010;
				if (u2 != null)
				{
					llist.Add(u2._ISignature);
				}
			}
			return llist;
		}

		// Token: 0x04000ACF RID: 2767
		private readonly Dictionary<string, LList<_ISignature>> \u0001;

		// Token: 0x04000AD0 RID: 2768
		[CompilerGenerated]
		private readonly LList<\u0019> \u0001;

		// Token: 0x020003DB RID: 987
		private sealed class \u0001
		{
			// Token: 0x06003744 RID: 14148 RVA: 0x000E32D8 File Offset: 0x000E14D8
			internal void \u0001(_ISignature \u0002)
			{
				int u = Helper.\u0001(\u0002);
				this.\u0001(new \u0083.\u0010(\u0002), u);
			}

			// Token: 0x06003745 RID: 14149 RVA: 0x000E32FC File Offset: 0x000E14FC
			internal LList<\u0019> \u0001()
			{
				LList<\u0019> llist = new LList<\u0019>();
				foreach (LList<\u0019> llist2 in this.\u0001.Values)
				{
					llist.AddRange(llist2);
				}
				return llist;
			}

			// Token: 0x06003746 RID: 14150 RVA: 0x000E3358 File Offset: 0x000E1558
			internal void \u0002(_ISignature \u0002)
			{
				Debug.\u0001(\u0002 != null, "sign != null");
				int u = 50000;
				\u0002.GetAttributeIntValue(CompileAttributes.ATTRIBUTE_CALL_WITHIN_GLOBAL_INIT_EXIT_SLOT, ref u);
				this.\u0001(new \u0083.\u0011(\u0002), u);
			}

			// Token: 0x06003747 RID: 14151 RVA: 0x000E3394 File Offset: 0x000E1594
			private void \u0001(\u0019 \u0002, int \u0003)
			{
				Debug.\u0001(\u0002 != null, "item != null");
				LList<\u0019> llist;
				if (!this.\u0001.TryGetValue(\u0003, ref llist))
				{
					llist = new LList<\u0019>();
					this.\u0001[\u0003] = llist;
				}
				llist.Add(\u0002);
			}

			// Token: 0x04000AD1 RID: 2769
			private readonly LSortedList<int, LList<\u0019>> \u0001 = new LSortedList<int, LList<\u0019>>();
		}
	}
}
