using System;
using System.Collections.Concurrent;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u001E
{
	// Token: 0x020000DE RID: 222
	internal sealed class \u0005
	{
		// Token: 0x06000FB8 RID: 4024 RVA: 0x0002B7DC File Offset: 0x000299DC
		public \u0005(ConcurrentQueue<_ICompiledPOU> \u0008\u0002, Codegeneration \u000E\u0002, \u0004 \u000F\u0002)
		{
			this.\u0001 = \u0008\u0002;
			this.\u0001 = \u000E\u0002;
			this.\u0001 = \u000F\u0002;
		}

		// Token: 0x17000479 RID: 1145
		// (get) Token: 0x06000FB9 RID: 4025 RVA: 0x0002B804 File Offset: 0x00029A04
		// (set) Token: 0x06000FBA RID: 4026 RVA: 0x0002B848 File Offset: 0x00029A48
		public bool Cancel
		{
			get
			{
				object u = this.\u0001;
				bool u2;
				lock (u)
				{
					u2 = this.\u0002;
				}
				return u2;
			}
			set
			{
				object u = this.\u0001;
				lock (u)
				{
					this.\u0002 = value;
				}
			}
		}

		// Token: 0x06000FBB RID: 4027 RVA: 0x0002B88C File Offset: 0x00029A8C
		public void \u0001()
		{
			_ICompiledPOU icompiledPOU = null;
			try
			{
				while (this.\u0001.TryDequeue(out icompiledPOU) && !this.Cancel)
				{
					if (!icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode) && icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile) && !icompiledPOU.GetFlag(CompiledPOUFlags.NoCompile) && !icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob))
					{
						if (icompiledPOU.GetFlagInternal(InternalCompiledPOUFlags.IsImplicitInitFunction))
						{
							_ISignature u = this.\u0001._ICompileContext.GetSignatureById(icompiledPOU.SignatureId) as _ISignature;
							this.\u0001.\u0003(icompiledPOU, u);
						}
						else
						{
							this.\u0001.\u0002(icompiledPOU);
						}
						string fullName = icompiledPOU.GetFullName(this.\u0001._ICompileContext);
						this.\u0001.\u0001(fullName);
						this.\u0001++;
					}
				}
				this.\u0001 = true;
			}
			catch (Exception innerException)
			{
				string message = "Internal error in POU: " + icompiledPOU.Name;
				this.\u0001.\u0001(new Exception(message, innerException));
			}
		}

		// Token: 0x040002AF RID: 687
		private readonly ConcurrentQueue<_ICompiledPOU> \u0001;

		// Token: 0x040002B0 RID: 688
		private readonly Codegeneration \u0001;

		// Token: 0x040002B1 RID: 689
		private readonly \u0004 \u0001;

		// Token: 0x040002B2 RID: 690
		private readonly object \u0001 = new object();

		// Token: 0x040002B3 RID: 691
		public int \u0001;

		// Token: 0x040002B4 RID: 692
		public bool \u0001;

		// Token: 0x040002B5 RID: 693
		private bool \u0002;
	}
}
