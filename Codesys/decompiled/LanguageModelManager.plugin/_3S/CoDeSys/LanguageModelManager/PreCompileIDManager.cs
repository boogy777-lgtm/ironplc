using System;
using System.Diagnostics;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DB RID: 219
	internal static class PreCompileIDManager
	{
		// Token: 0x06000F81 RID: 3969 RVA: 0x0002A79D File Offset: 0x0002979D
		internal static void ResetPrecompileSignatures()
		{
			PreCompileIDManager._precompileIDMan = new MyIdMan();
			PreCompileIDManager._precompileSignatures = new LDictionary<int, WeakReference>();
		}

		// Token: 0x06000F82 RID: 3970 RVA: 0x0002A7B3 File Offset: 0x000297B3
		internal static int CreatePrecompileSignatureId()
		{
			int next = PreCompileIDManager._precompileIDMan.GetNext();
			Debug.Assert(next < 1073741824, "An overflow occured in the precompile signature IDs. Precompile IDs are no longer unique.");
			return next;
		}

		// Token: 0x06000F83 RID: 3971 RVA: 0x0002A7D4 File Offset: 0x000297D4
		internal static void RegisterPrecompileSignature(_ISignature signature)
		{
			Debug.Assert(signature.PrecompileId != Common.InvalidID);
			object precompileIDLock = PreCompileIDManager._precompileIDLock;
			lock (precompileIDLock)
			{
				PreCompileIDManager._precompileSignatures[signature.PrecompileId] = new WeakReference(signature, false);
			}
		}

		// Token: 0x06000F84 RID: 3972 RVA: 0x0002A83C File Offset: 0x0002983C
		internal static void DeregisterPrecompileSignatureId(int precompileId)
		{
			Debug.Assert(precompileId != Common.InvalidID);
			object precompileIDLock = PreCompileIDManager._precompileIDLock;
			lock (precompileIDLock)
			{
				PreCompileIDManager._precompileSignatures.Remove(precompileId);
			}
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x0002A894 File Offset: 0x00029894
		internal static ISignature6 GetSignatureForPrecompileID(int precompileId)
		{
			_ISignature result = null;
			object precompileIDLock = PreCompileIDManager._precompileIDLock;
			lock (precompileIDLock)
			{
				if (PreCompileIDManager._precompileSignatures.ContainsKey(precompileId))
				{
					result = (PreCompileIDManager._precompileSignatures[precompileId].Target as _ISignature);
				}
			}
			return result;
		}

		// Token: 0x04000390 RID: 912
		private static MyIdMan _precompileIDMan = new MyIdMan();

		// Token: 0x04000391 RID: 913
		private static object _precompileIDLock = new object();

		// Token: 0x04000392 RID: 914
		private static LDictionary<int, WeakReference> _precompileSignatures = new LDictionary<int, WeakReference>();
	}
}
