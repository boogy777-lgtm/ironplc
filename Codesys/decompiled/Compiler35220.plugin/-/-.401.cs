using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220.CompilerPhases;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0082;

namespace \u0018
{
	// Token: 0x02000401 RID: 1025
	internal sealed class \u0013
	{
		// Token: 0x1700097A RID: 2426
		// (get) Token: 0x060038CF RID: 14543 RVA: 0x000EA5D4 File Offset: 0x000E87D4
		private CompilerPhase4_Typechecker TypeChecker { get; }

		// Token: 0x060038D0 RID: 14544 RVA: 0x000EA5DC File Offset: 0x000E87DC
		public \u0013(ConcurrentQueue<_ICompiledPOU> \u0008\u0002, CompilerPhase4_Typechecker \u0090\u0004, Codegeneration \u000E\u0002, \u0082.\u0004 \u000F\u0002)
		{
			this.\u0001 = \u0008\u0002;
			this.\u0001 = \u000E\u0002;
			this.\u0001 = \u000F\u0002;
			this.TypeChecker = \u0090\u0004;
		}

		// Token: 0x1700097B RID: 2427
		// (get) Token: 0x060038D1 RID: 14545 RVA: 0x000EA60C File Offset: 0x000E880C
		// (set) Token: 0x060038D2 RID: 14546 RVA: 0x000EA650 File Offset: 0x000E8850
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

		// Token: 0x060038D3 RID: 14547 RVA: 0x000EA694 File Offset: 0x000E8894
		public void \u0001()
		{
			_ICompiledPOU icompiledPOU = null;
			try
			{
				this.\u0001(out icompiledPOU);
			}
			catch (Exception innerException)
			{
				string message;
				if (icompiledPOU != null)
				{
					message = "Internal error in POU: " + icompiledPOU.Name;
				}
				message = "Internal error in Code";
				this.\u0001.\u0001(new Exception(message, innerException));
			}
		}

		// Token: 0x060038D4 RID: 14548 RVA: 0x000EA6EC File Offset: 0x000E88EC
		private void \u0001(out _ICompiledPOU \u0002)
		{
			\u0002 = null;
			_ICompiledPOU icompiledPOU;
			while (this.\u0001.TryDequeue(out icompiledPOU))
			{
				\u0002 = icompiledPOU;
				if (this.Cancel)
				{
					break;
				}
				if (!icompiledPOU.GetFlag(CompiledPOUFlags.ContainsNoCode))
				{
					if (!icompiledPOU.GetFlag(CompiledPOUFlags.ToCompile))
					{
						this.\u0003(icompiledPOU);
					}
					else if (!icompiledPOU.GetFlag(CompiledPOUFlags.NoCompile) && !icompiledPOU.GetFlag(CompiledPOUFlags.Blob) && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob))
					{
						this.\u0001(icompiledPOU);
					}
				}
			}
			this.\u0001 = true;
		}

		// Token: 0x060038D5 RID: 14549 RVA: 0x000EA770 File Offset: 0x000E8970
		private void \u0001(_ICompiledPOU \u0002)
		{
			this.TypeChecker.\u0001(\u0002);
			if (!this.TypeChecker.ErrorsOccured)
			{
				if (\u0002.GetFlagInternal(InternalCompiledPOUFlags.IsImplicitInitFunction))
				{
					_ISignature u = this.\u0001._ICompileContext.GetSignatureById(\u0002.SignatureId) as _ISignature;
					this.\u0001.\u0003(\u0002, u);
				}
				else
				{
					this.\u0001.\u0002(\u0002);
				}
				this.\u0002(\u0002);
				this.\u0003(\u0002);
			}
			string fullName = \u0002.GetFullName(this.\u0001._ICompileContext);
			this.\u0001.\u0001(fullName);
			this.\u0001++;
		}

		// Token: 0x060038D6 RID: 14550 RVA: 0x000EA814 File Offset: 0x000E8A14
		internal void \u0002(_ICompiledPOU \u0002)
		{
			ErrorVisitor errorVisitor = new ErrorVisitor();
			\u0002.Accept(errorVisitor);
			\u0002.SetMessages(errorVisitor._Messages);
		}

		// Token: 0x060038D7 RID: 14551 RVA: 0x000EA83C File Offset: 0x000E8A3C
		internal void \u0003(_ICompiledPOU \u0002)
		{
			if (!this.\u0001.KeepCompileInformation)
			{
				\u0002.SetParseTree(\u0019.\u0003.\u0001());
			}
			\u0002.SetFlag(CompiledPOUFlags.ContainsNoParseTree, true);
		}

		// Token: 0x04000B4F RID: 2895
		private readonly ConcurrentQueue<_ICompiledPOU> \u0001;

		// Token: 0x04000B50 RID: 2896
		private readonly Codegeneration \u0001;

		// Token: 0x04000B51 RID: 2897
		private readonly \u0082.\u0004 \u0001;

		// Token: 0x04000B52 RID: 2898
		private readonly object \u0001 = new object();

		// Token: 0x04000B53 RID: 2899
		public int \u0001;

		// Token: 0x04000B54 RID: 2900
		public bool \u0001;

		// Token: 0x04000B55 RID: 2901
		private bool \u0002;

		// Token: 0x04000B56 RID: 2902
		[CompilerGenerated]
		private readonly CompilerPhase4_Typechecker \u0001;
	}
}
