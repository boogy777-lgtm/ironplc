using System;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u0016;
using \u0019;
using \u001C;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u000F
{
	// Token: 0x020003DD RID: 989
	internal sealed class \u001A
	{
		// Token: 0x170008F7 RID: 2295
		// (get) Token: 0x0600374C RID: 14156 RVA: 0x000E3424 File Offset: 0x000E1624
		private _ICompileContext CompileContext { get; }

		// Token: 0x170008F8 RID: 2296
		// (get) Token: 0x0600374D RID: 14157 RVA: 0x000E342C File Offset: 0x000E162C
		private bool OnlineChange { get; }

		// Token: 0x170008F9 RID: 2297
		// (get) Token: 0x0600374E RID: 14158 RVA: 0x000E3434 File Offset: 0x000E1634
		private bool BootProject { get; }

		// Token: 0x170008FA RID: 2298
		// (get) Token: 0x0600374F RID: 14159 RVA: 0x000E343C File Offset: 0x000E163C
		private IMessageCategory MessageCategory { get; }

		// Token: 0x170008FB RID: 2299
		// (get) Token: 0x06003750 RID: 14160 RVA: 0x000E3444 File Offset: 0x000E1644
		// (set) Token: 0x06003751 RID: 14161 RVA: 0x000E344C File Offset: 0x000E164C
		public bool AllocationError { get; set; }

		// Token: 0x170008FC RID: 2300
		// (get) Token: 0x06003752 RID: 14162 RVA: 0x000E3458 File Offset: 0x000E1658
		private \u0081.\u0017 TableGenerator { get; }

		// Token: 0x170008FD RID: 2301
		// (get) Token: 0x06003753 RID: 14163 RVA: 0x000E3460 File Offset: 0x000E1660
		private \u001C.\u0015 POUGenerator { get; }

		// Token: 0x06003754 RID: 14164 RVA: 0x000E3468 File Offset: 0x000E1668
		public \u001A(_ICompileContext \u001C\u0004, bool \u008A\u0004, bool \u008E\u0004, Codegeneration \u000E\u0002)
		{
			this.CompileContext = \u001C\u0004;
			this.OnlineChange = \u008A\u0004;
			this.BootProject = \u008E\u0004;
			this.MessageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			this.TableGenerator = new \u0081.\u0017(this.CompileContext, \u000E\u0002);
			this.POUGenerator = new \u001C.\u0015(this.CompileContext, \u000E\u0002);
		}

		// Token: 0x06003755 RID: 14165 RVA: 0x000E34CC File Offset: 0x000E16CC
		public void \u0001()
		{
			global::\u0016.\u0016 u;
			\u001A.\u0001(this.CompileContext, this.OnlineChange, this.BootProject, out u);
		}

		// Token: 0x06003756 RID: 14166 RVA: 0x000E34F4 File Offset: 0x000E16F4
		public void \u0001(IProgressCallback \u0002)
		{
			ushort maxValue = ushort.MaxValue;
			int u = -1;
			global::\u0016.\u0016 u3;
			global::\u0016.\u0016 u2 = \u001A.\u0001(this.CompileContext, this.OnlineChange, this.BootProject, out u3);
			int u4 = 0;
			int u5;
			_ICompiledPOU icompiledPOU = this.TableGenerator.\u0001(u2, out u5);
			_ICompiledPOU icompiledPOU2 = null;
			if (u3 != null)
			{
				icompiledPOU2 = this.TableGenerator.\u0001(u3, out u4);
			}
			_ISignature isignature = this.TableGenerator.\u0001(u5, u4);
			this.CompileContext.AddSignature(isignature, null, null, true);
			if (!MemoryCompiler.\u0003(this.CompileContext.DataManager, ref maxValue, ref u, this.CompileContext.DataManager._MemorySettings.PackMode, icompiledPOU.CompiledCode.CodeSize, this.CompileContext.DataManager._MemorySettings.DataSegmentSize, DataSegmentFlags.Code))
			{
				string u6 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u6, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(this.MessageCategory, message);
				this.AllocationError = true;
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u);
			}
			if (this.BootProject)
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
			}
			this.CompileContext.AddCompiledPOU(icompiledPOU, isignature, null);
			if (u3 != null && this.BootProject && !this.AllocationError)
			{
				_ISignature sign = this.TableGenerator.\u0001(0, 0);
				this.CompileContext.AddSignature(sign, null, null, true);
				if (!MemoryCompiler.\u0003(this.CompileContext.DataManager, ref maxValue, ref u, this.CompileContext.DataManager._MemorySettings.PackMode, icompiledPOU2.CompiledCode.CodeSize, this.CompileContext.DataManager._MemorySettings.DataSegmentSize, DataSegmentFlags.Code))
				{
					string u6 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
					{
						icompiledPOU2.Name,
						icompiledPOU2.CompiledCode.CodeSize
					});
					_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u6, Severity.Error, MessageId.Err_OutOfCodeMemory);
					APEnvironmentFacade.Instance.AddMessage(this.MessageCategory, message);
					this.AllocationError = true;
				}
				else
				{
					icompiledPOU2.CompiledCode.Location = global::\u0019.\u0003.\u0001(maxValue, u);
				}
				if (this.BootProject)
				{
					icompiledPOU2.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
				}
				this.CompileContext.AddCompiledPOU(icompiledPOU2, sign, null);
			}
			if (!this.AllocationError)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.Progress.NotifyTaskProgress(\u0002, "relocation pou");
				this.\u0001(u3, u2, icompiledPOU, icompiledPOU2, isignature);
			}
		}

		// Token: 0x06003757 RID: 14167 RVA: 0x000E3790 File Offset: 0x000E1990
		private void \u0001(global::\u0016.\u0016 \u0002, global::\u0016.\u0016 \u0003, _ICompiledPOU \u0004, _ICompiledPOU \u0005, _ISignature \u0006)
		{
			ushort u = 0;
			int u2 = 0;
			_ISignature isignature = this.POUGenerator.\u0001();
			this.CompileContext.AddSignature(isignature, null, null, true);
			_ICompiledPOU icompiledPOU = this.POUGenerator.\u0001(\u0003, \u0002, isignature, \u0006, \u0004, \u0005);
			if (!MemoryCompiler.\u0003(this.CompileContext.DataManager, ref u, ref u2, this.CompileContext.DataManager.PackMode, icompiledPOU.CompiledCode.CodeSize, this.CompileContext.DataManager._MemorySettings.CodeSegmentSize, DataSegmentFlags.Code))
			{
				string u3 = global::\u0003.\u0006.\u0001(MessageId.Err_OutOfCodeMemory, new object[]
				{
					icompiledPOU.Name,
					icompiledPOU.CompiledCode.CodeSize
				});
				_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u3, Severity.Error, MessageId.Err_OutOfCodeMemory);
				APEnvironmentFacade.Instance.AddMessage(this.MessageCategory, message);
			}
			else
			{
				icompiledPOU.CompiledCode.Location = global::\u0019.\u0003.\u0001(u, u2);
			}
			icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile | CompiledPOUFlags.ToRemoveAfterDownload, true);
			if (this.BootProject)
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.BootProjectRelevant, true);
			}
		}

		// Token: 0x04000AD4 RID: 2772
		[CompilerGenerated]
		private readonly _ICompileContext \u0001;

		// Token: 0x04000AD5 RID: 2773
		[CompilerGenerated]
		private readonly bool \u0001;

		// Token: 0x04000AD6 RID: 2774
		[CompilerGenerated]
		private readonly bool \u0002;

		// Token: 0x04000AD7 RID: 2775
		[CompilerGenerated]
		private readonly IMessageCategory \u0001;

		// Token: 0x04000AD8 RID: 2776
		[CompilerGenerated]
		private bool \u0003;

		// Token: 0x04000AD9 RID: 2777
		[CompilerGenerated]
		private readonly \u0081.\u0017 \u0001;

		// Token: 0x04000ADA RID: 2778
		[CompilerGenerated]
		private readonly \u001C.\u0015 \u0001;
	}
}
