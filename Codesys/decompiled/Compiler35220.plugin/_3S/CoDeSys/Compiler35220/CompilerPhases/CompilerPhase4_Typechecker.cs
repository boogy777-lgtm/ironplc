using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using \u0002;
using \u0006;
using \u0007;
using \u000E;
using \u0015;
using \u0018;
using \u001A;
using _3S.CoDeSys.Compiler35220.Compile.Phase4_TypeCheck;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Phase4_TypeCheck;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.CompilerPhases
{
	// Token: 0x020003FC RID: 1020
	internal sealed class CompilerPhase4_Typechecker : ILMCompiledParseTreeService
	{
		// Token: 0x06003874 RID: 14452 RVA: 0x000E8098 File Offset: 0x000E6298
		private CompilerPhase4_Typechecker(global::\u000E.\u001B compileInformation, IDictionary<int, CompactedTypifiedParseTreeInformation> parseTreeInformationTable)
		{
			this.CompileInformation = compileInformation;
			this.ParseTreeInformationTable = parseTreeInformationTable;
			this.\u0001(this);
		}

		// Token: 0x06003875 RID: 14453 RVA: 0x000E80B8 File Offset: 0x000E62B8
		private void \u0001(ILMCompiledParseTreeService \u0002)
		{
			ILMCompiledApplicationSetForInstrumentation ilmcompiledApplicationSetForInstrumentation = this.ComconNew as ILMCompiledApplicationSetForInstrumentation;
			if (ilmcompiledApplicationSetForInstrumentation != null)
			{
				ilmcompiledApplicationSetForInstrumentation.CompiledParseTreeService = \u0002;
				foreach (ICompiledPOUWithParseTreeProvider compiledPOUWithParseTreeProvider in this.ComconNew.CompiledPOUList.OfType<ICompiledPOUWithParseTreeProvider>())
				{
					((global::\u0002.\u0003)compiledPOUWithParseTreeProvider.ParseTreeProvider).CompiledParseTreeService = \u0002;
				}
			}
		}

		// Token: 0x06003876 RID: 14454 RVA: 0x000E8130 File Offset: 0x000E6330
		private CompilerPhase4_Typechecker(global::\u000E.\u001B ci)
		{
			this.CompileInformation = ci;
		}

		// Token: 0x17000960 RID: 2400
		// (get) Token: 0x06003877 RID: 14455 RVA: 0x000E8140 File Offset: 0x000E6340
		// (set) Token: 0x06003878 RID: 14456 RVA: 0x000E8148 File Offset: 0x000E6348
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x17000961 RID: 2401
		// (get) Token: 0x06003879 RID: 14457 RVA: 0x000E8154 File Offset: 0x000E6354
		// (set) Token: 0x0600387A RID: 14458 RVA: 0x000E815C File Offset: 0x000E635C
		public bool ErrorsOccured { get; private set; }

		// Token: 0x17000962 RID: 2402
		// (get) Token: 0x0600387B RID: 14459 RVA: 0x000E8168 File Offset: 0x000E6368
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x17000963 RID: 2403
		// (get) Token: 0x0600387C RID: 14460 RVA: 0x000E8178 File Offset: 0x000E6378
		// (set) Token: 0x0600387D RID: 14461 RVA: 0x000E8180 File Offset: 0x000E6380
		private IDictionary<int, CompactedTypifiedParseTreeInformation> ParseTreeInformationTable { get; set; }

		// Token: 0x17000964 RID: 2404
		// (get) Token: 0x0600387E RID: 14462 RVA: 0x000E818C File Offset: 0x000E638C
		// (set) Token: 0x0600387F RID: 14463 RVA: 0x000E8194 File Offset: 0x000E6394
		private bool UpToDate { get; set; }

		// Token: 0x06003880 RID: 14464 RVA: 0x000E81A0 File Offset: 0x000E63A0
		public static CompilerPhase4_Typechecker \u0001(global::\u000E.\u001B \u0002, IDictionary<int, CompactedTypifiedParseTreeInformation> \u0003)
		{
			return new CompilerPhase4_Typechecker(\u0002, \u0003);
		}

		// Token: 0x06003881 RID: 14465 RVA: 0x000E81AC File Offset: 0x000E63AC
		public static CompilerPhase4_Typechecker \u0001(global::\u000E.\u001B \u0002)
		{
			return new CompilerPhase4_Typechecker(\u0002)
			{
				UpToDate = true,
				ErrorsOccured = false
			};
		}

		// Token: 0x06003882 RID: 14466 RVA: 0x000E81C4 File Offset: 0x000E63C4
		public ISequenceStatement3 \u0001(ICompiledPOU \u0002)
		{
			IScope scope = this.ComconNew.CreateGlobalIScope();
			_ISignature isignature = scope[\u0002.SignatureId] as _ISignature;
			if (isignature == null)
			{
				return \u0002.ParseTree as ISequenceStatement3;
			}
			if (isignature.POUType == Operator.Method)
			{
				_ISignature isignature2 = scope[isignature.ParentSignatureId] as _ISignature;
				if (isignature2 == null || isignature2.POUType == Operator.Interface)
				{
					return \u0002.ParseTree as ISequenceStatement3;
				}
			}
			this.\u0001(\u0002 as _ICompiledPOU);
			return \u0002.ParseTree as ISequenceStatement3;
		}

		// Token: 0x06003883 RID: 14467 RVA: 0x000E824C File Offset: 0x000E644C
		public void \u0001(ICompiledPOU \u0002, ISequenceStatement3 \u0003)
		{
			this.\u0001(\u0002 as _ICompiledPOU, \u0003);
		}

		// Token: 0x06003884 RID: 14468 RVA: 0x000E825C File Offset: 0x000E645C
		private void \u0003(bool \u0002)
		{
			int processorCount = Environment.ProcessorCount;
			CompilerPhase4_Typechecker.\u0001[] array = new CompilerPhase4_Typechecker.\u0001[processorCount];
			Thread[] array2 = new Thread[processorCount];
			ConcurrentQueue<_ICompiledPOU> u0010_u = new ConcurrentQueue<_ICompiledPOU>(this.ComconNew.GetAllCompiledPOUsEx().OrderBy(new Func<ICompiledPOU4, ICompiledPOU4>(CompilerPhase4_Typechecker.<>c.<>9.\u0001), new \u0084.\u0005()).Cast<_ICompiledPOU>());
			ConcurrentDictionary<int, CompactedTypifiedParseTreeInformation> u000F_u = new ConcurrentDictionary<int, CompactedTypifiedParseTreeInformation>(this.ParseTreeInformationTable);
			for (int i = 0; i < processorCount; i++)
			{
				array[i] = new CompilerPhase4_Typechecker.\u0001(this.CompileInformation, u000F_u, u0010_u)
				{
					ErrorsOccured = \u0002
				};
				array2[i] = new Thread(new ThreadStart(array[i].\u0001));
			}
			for (int j = 0; j < processorCount; j++)
			{
				array2[j].Start();
			}
			int[] array3 = new int[processorCount];
			for (int k = 0; k < processorCount; k++)
			{
				array3[0] = 0;
			}
			int num = 0;
			do
			{
				if (array2[num].Join(50))
				{
					num++;
				}
			}
			while (num < processorCount);
			for (int l = 0; l < processorCount; l++)
			{
				this.ErrorsOccured = (this.ErrorsOccured || array[l].ErrorsOccured);
			}
		}

		// Token: 0x06003885 RID: 14469 RVA: 0x000E8388 File Offset: 0x000E6588
		internal void \u0004(bool \u0002)
		{
			if (!this.UpToDate)
			{
				this.\u0003(\u0002);
			}
			this.\u0001();
		}

		// Token: 0x06003886 RID: 14470 RVA: 0x000E83A0 File Offset: 0x000E65A0
		public void \u0001()
		{
			this.\u0001(null);
		}

		// Token: 0x06003887 RID: 14471 RVA: 0x000E83AC File Offset: 0x000E65AC
		public void \u0001(_ICompiledPOU \u0002)
		{
			if (this.UpToDate)
			{
				return;
			}
			CompilerPhase4_Typechecker.\u0001 u = new CompilerPhase4_Typechecker.\u0001(this.CompileInformation, null, null);
			CompactedTypifiedParseTreeInformation u2;
			this.ParseTreeInformationTable.TryGetValue(\u0002.SignatureId, out u2);
			u.\u0001(\u0002, u2);
			if (u.ErrorsOccured)
			{
				this.ErrorsOccured = true;
			}
		}

		// Token: 0x06003888 RID: 14472 RVA: 0x000E83FC File Offset: 0x000E65FC
		private void \u0001(_ICompiledPOU \u0002, ISequenceStatement3 \u0003)
		{
			CompactedTypifiedParseTreeInformation compactedTypifiedParseTreeInformation;
			this.ParseTreeInformationTable.TryGetValue(\u0002.SignatureId, out compactedTypifiedParseTreeInformation);
			if (compactedTypifiedParseTreeInformation == null)
			{
				return;
			}
			new CompilerPhase4_Typechecker.\u0001(this.CompileInformation, null, null).\u0001(\u0002, \u0003 as _IStatement, compactedTypifiedParseTreeInformation);
		}

		// Token: 0x06003889 RID: 14473 RVA: 0x000E843C File Offset: 0x000E663C
		private static bool \u0001(IMessage \u0002)
		{
			_ICompilerMessage icompilerMessage = \u0002 as _ICompilerMessage;
			return icompilerMessage != null && (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError) && icompilerMessage.ShowCompile;
		}

		// Token: 0x0600388A RID: 14474 RVA: 0x000E8470 File Offset: 0x000E6670
		public static bool \u0001(_ICompiledPOU \u0002, _IStatement \u0003, _ICompileContext \u0004)
		{
			return CompilerPhase4_Typechecker.\u0001(\u0002, \u0003, \u0004, new ErrorVisitor());
		}

		// Token: 0x0600388B RID: 14475 RVA: 0x000E8480 File Offset: 0x000E6680
		private static bool \u0001(_ICompiledPOU \u0002, _IStatement \u0003, _ICompileContext \u0004, ErrorVisitor \u0005)
		{
			ISignature signature = \u0004[\u0002.SignatureId];
			\u0005.\u0001();
			\u0003.Accept(\u0005);
			\u0002.SetMessages(Array.Empty<_ICompilerMessage>());
			bool flag = signature.GetFlag(SignatureFlag.External);
			return \u0005.MessageList.Count > 0 && !flag && \u0005.Messages.Any(new Func<IMessage, bool>(CompilerPhase4_Typechecker.\u0001));
		}

		// Token: 0x04000B40 RID: 2880
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;

		// Token: 0x04000B41 RID: 2881
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000B42 RID: 2882
		[CompilerGenerated]
		private IDictionary<int, CompactedTypifiedParseTreeInformation> \u0001;

		// Token: 0x04000B43 RID: 2883
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x020003FD RID: 1021
		private sealed class \u0001
		{
			// Token: 0x0600388C RID: 14476 RVA: 0x000E84E4 File Offset: 0x000E66E4
			public \u0001(global::\u000E.\u001B \u008F\u0004, ConcurrentDictionary<int, CompactedTypifiedParseTreeInformation> \u000F\u0005, ConcurrentQueue<_ICompiledPOU> \u0010\u0005)
			{
				this.CompileInformation = \u008F\u0004;
				this.ErrorVisitor = new ErrorVisitor();
				this.ParseTreeInformationTable = \u000F\u0005;
				this.CompiledPOUs = \u0010\u0005;
			}

			// Token: 0x17000965 RID: 2405
			// (get) Token: 0x0600388D RID: 14477 RVA: 0x000E850C File Offset: 0x000E670C
			private global::\u000E.\u001B CompileInformation { get; }

			// Token: 0x17000966 RID: 2406
			// (get) Token: 0x0600388E RID: 14478 RVA: 0x000E8514 File Offset: 0x000E6714
			// (set) Token: 0x0600388F RID: 14479 RVA: 0x000E851C File Offset: 0x000E671C
			public bool ErrorsOccured { get; set; }

			// Token: 0x17000967 RID: 2407
			// (get) Token: 0x06003890 RID: 14480 RVA: 0x000E8528 File Offset: 0x000E6728
			private _ICompileContext ComconNew
			{
				get
				{
					return this.CompileInformation.ComconNew;
				}
			}

			// Token: 0x17000968 RID: 2408
			// (get) Token: 0x06003891 RID: 14481 RVA: 0x000E8538 File Offset: 0x000E6738
			private _ICompileContext ComconOld
			{
				get
				{
					return this.CompileInformation.ComconOld;
				}
			}

			// Token: 0x17000969 RID: 2409
			// (get) Token: 0x06003892 RID: 14482 RVA: 0x000E8548 File Offset: 0x000E6748
			private IDictionary<int, CompactedTypifiedParseTreeInformation> ParseTreeInformationTable { get; }

			// Token: 0x1700096A RID: 2410
			// (get) Token: 0x06003893 RID: 14483 RVA: 0x000E8550 File Offset: 0x000E6750
			private ConcurrentQueue<_ICompiledPOU> CompiledPOUs { get; }

			// Token: 0x1700096B RID: 2411
			// (get) Token: 0x06003894 RID: 14484 RVA: 0x000E8558 File Offset: 0x000E6758
			private ErrorVisitor ErrorVisitor { get; }

			// Token: 0x06003895 RID: 14485 RVA: 0x000E8560 File Offset: 0x000E6760
			public void \u0001()
			{
				_ICompiledPOU icompiledPOU = null;
				try
				{
					while (this.CompiledPOUs.TryDequeue(out icompiledPOU))
					{
						CompactedTypifiedParseTreeInformation u;
						this.ParseTreeInformationTable.TryGetValue(icompiledPOU.SignatureId, out u);
						this.\u0001(icompiledPOU, u);
					}
				}
				catch (Exception ex)
				{
					"Internal error in POU " + icompiledPOU.Name + ": " + ex.Message;
				}
			}

			// Token: 0x06003896 RID: 14486 RVA: 0x000E85D0 File Offset: 0x000E67D0
			public void \u0001(_ICompiledPOU \u0002, CompactedTypifiedParseTreeInformation \u0003)
			{
				if (\u0002.GetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone))
				{
					return;
				}
				\u0002.SetFlagInternal(InternalCompiledPOUFlags.TypeCheckDone, true);
				this.\u0002(\u0002, \u0003);
				this.\u0001(\u0002);
			}

			// Token: 0x06003897 RID: 14487 RVA: 0x000E85FC File Offset: 0x000E67FC
			private void \u0001(_ICompiledPOU \u0002)
			{
				if (this.ErrorsOccured || \u0002.GetFlag(CompiledPOUFlags.ContainsNoParseTree))
				{
					return;
				}
				_ISignature isignature = this.ComconNew.GetSignatureById(\u0002.SignatureId) as _ISignature;
				_ISignature u = null;
				if (this.ComconOld != null)
				{
					u = (this.ComconOld.GetSignatureById(\u0002.SignatureId) as _ISignature);
				}
				global::\u0006.\u000F.\u0002(isignature, this.ComconNew, this.ComconOld);
				global::\u0006.\u000F.\u0003(isignature, this.ComconNew, this.ComconOld);
				if (!\u0002.GetFlagInternal(InternalCompiledPOUFlags.ImplicitInitialisationCodeAdded))
				{
					global::\u0007.\u0013.\u0001(this.ComconNew, \u0002, isignature, u);
				}
				if (\u0002.GetFlagInternal(InternalCompiledPOUFlags.ContainsTryCatch))
				{
					bool flag;
					\u0084.\u0016.\u0001(\u0002, out flag);
					if (flag)
					{
						this.ErrorsOccured = true;
					}
				}
			}

			// Token: 0x06003898 RID: 14488 RVA: 0x000E86B4 File Offset: 0x000E68B4
			private void \u0002(_ICompiledPOU \u0002, CompactedTypifiedParseTreeInformation \u0003)
			{
				if (\u0002.GetFlagInternal(InternalCompiledPOUFlags.ToCheck) && \u0003 != null)
				{
					\u0002.DuplicateParseTreeForCompilation();
					_IStatement parseTree = \u0002.GetParseTree();
					this.\u0001(\u0002, parseTree, \u0003);
					this.ErrorsOccured |= CompilerPhase4_Typechecker.\u0001(\u0002, parseTree, this.ComconNew, this.ErrorVisitor);
					global::\u001A.\u000F.\u0001(parseTree, \u0002);
				}
			}

			// Token: 0x06003899 RID: 14489 RVA: 0x000E870C File Offset: 0x000E690C
			public void \u0001(_ICompiledPOU \u0002, _IStatement \u0003, CompactedTypifiedParseTreeInformation \u0004)
			{
				_IScope iscope = global::\u0007.\u0005.\u0001(this.ComconNew, \u0002.SignatureId) as _IScope;
				\u0080.\u000E.\u0001(this.ComconNew, \u0003, \u0002.SignatureId, iscope);
				if (!\u0004.IsEmpty())
				{
					TypifiedParseTreeInformationSetter.SetInformationInParseTree(\u0003, \u0004);
				}
				global::\u0015.\u0006.\u0001(iscope, \u0002, this.ComconNew, \u0003);
				ConstantFolder.ReplaceFoldedConstants(\u0003, this.ComconNew, iscope, \u0002);
				global::\u0018.\u000E.\u0001(\u0003, this.ComconNew);
				TypeCheckerVisitor ivisit = new TypeCheckerVisitor(iscope, this.ComconNew, false, \u0002, true);
				\u0003.Accept(ivisit);
			}

			// Token: 0x04000B44 RID: 2884
			[CompilerGenerated]
			private readonly global::\u000E.\u001B \u0001;

			// Token: 0x04000B45 RID: 2885
			[CompilerGenerated]
			private bool \u0001;

			// Token: 0x04000B46 RID: 2886
			[CompilerGenerated]
			private readonly IDictionary<int, CompactedTypifiedParseTreeInformation> \u0001;

			// Token: 0x04000B47 RID: 2887
			[CompilerGenerated]
			private readonly ConcurrentQueue<_ICompiledPOU> \u0001;

			// Token: 0x04000B48 RID: 2888
			[CompilerGenerated]
			private readonly ErrorVisitor \u0001;
		}
	}
}
