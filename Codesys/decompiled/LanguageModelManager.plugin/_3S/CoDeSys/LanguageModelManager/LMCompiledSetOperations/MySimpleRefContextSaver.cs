using System;
using System.IO;
using System.Threading;
using CODESYS.ProjectFormat;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;

namespace _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations
{
	// Token: 0x020001B6 RID: 438
	internal class MySimpleRefContextSaver
	{
		// Token: 0x06001F7D RID: 8061 RVA: 0x00056B21 File Offset: 0x00055B21
		public MySimpleRefContextSaver(CompileContext comconToSave, string stPath, bool bBootDuplicate, bool bBootInfoOnly, ISideCarEntry sideCarEntry)
		{
			this._comconToSave = comconToSave;
			this._stPath = stPath;
			this._bBootDuplicate = bBootDuplicate;
			this._bBootInfoOnly = bBootInfoOnly;
			this._sideCarEntry = sideCarEntry;
		}

		// Token: 0x06001F7E RID: 8062 RVA: 0x00056B4E File Offset: 0x00055B4E
		private string DetailedErrorMessage(Exception ex)
		{
			if (ex is PathTooLongException)
			{
				return Strings.DownloadInfoSaveException_PathToLong;
			}
			if (ex is OutOfMemoryException)
			{
				return Strings.DownloadInfoSaveException_OutOfMemory;
			}
			return Strings.DownloadInfoSaveException_Generic;
		}

		// Token: 0x06001F7F RID: 8063 RVA: 0x00056B74 File Offset: 0x00055B74
		private void ErrorMessagePrompt(Exception ex)
		{
			string arg = this.DetailedErrorMessage(ex);
			string stMessage = string.Format(Strings.DownloadInfoSaveException, this._stPath, arg);
			APEnvironmentFacade.Instance.ErrorReportingWithDetails(stMessage, delegate(object sender, EventArgs e)
			{
				APEnvironmentFacade.Instance.Information(ex.ToString(), "ExceptionInContextSavingThread_Details", Array.Empty<object>());
			}, null, "ExceptionInContextSavingThread", Array.Empty<object>());
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x00056BD0 File Offset: 0x00055BD0
		internal void SaveContext()
		{
			bool flag = false;
			PerformanceTimer performanceTimer = new PerformanceTimer("Compileinfo");
			try
			{
				performanceTimer.StartTiming();
				using (ITransactionalStream transactionalStream = this._sideCarEntry.WriteTo())
				{
					LMCompiledSetPersistence.StoreToStream(transactionalStream.WritableStream, this._comconToSave);
					transactionalStream.Commit();
				}
				performanceTimer.StopTiming();
				if (this._bBootDuplicate)
				{
					this.CreateBootDuplicate();
				}
			}
			catch (PathTooLongException ex)
			{
				APEnvironmentFacade.Instance.InvokeInPrimaryThread(new MySimpleRefContextSaver.VoidErrorMessagePrompt(this.ErrorMessagePrompt), new object[]
				{
					ex
				}, true);
				flag = false;
			}
			catch (ThreadAbortException)
			{
				flag = true;
				Thread.ResetAbort();
			}
			catch (Exception ex2)
			{
				APEnvironmentFacade.Instance.InvokeInPrimaryThread(new MySimpleRefContextSaver.VoidErrorMessagePrompt(this.ErrorMessagePrompt), new object[]
				{
					ex2
				}, true);
				flag = true;
			}
			finally
			{
				try
				{
					if (flag)
					{
						this._sideCarEntry.Delete();
					}
					if (!flag)
					{
						string environmentVariable = Environment.GetEnvironmentVariable("AP_DEBUG_LMM_DUMP_TIMES_COMPILEINFO");
						if (!string.IsNullOrEmpty(environmentVariable))
						{
							APEnvironmentFacade.Instance.FileSystem.AppendAllText(environmentVariable, performanceTimer.OutputMeasurementTMStyle());
						}
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x17000842 RID: 2114
		// (get) Token: 0x06001F81 RID: 8065 RVA: 0x00056D24 File Offset: 0x00055D24
		// (set) Token: 0x06001F82 RID: 8066 RVA: 0x00056D2C File Offset: 0x00055D2C
		internal Thread Thread { get; set; }

		// Token: 0x06001F83 RID: 8067 RVA: 0x00056D38 File Offset: 0x00055D38
		internal void CreateBootDuplicate()
		{
			string path = Path.ChangeExtension(this._stPath, ".bootinfo");
			string path2 = Path.ChangeExtension(this._stPath, ".bootinfo_guids");
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			try
			{
				if (this._sideCarEntry.Exists)
				{
					this._sideCarEntry.CopyTo(Path.GetFileName(path));
					flag2 = true;
					using (ITransactionalStream transactionalStream = this._sideCarEntry.CreateAnother(Path.GetFileName(path2)).WriteTo())
					{
						byte[] array = this._comconToSave.CodeId.ToByteArray();
						byte[] array2 = this._comconToSave.DataId.ToByteArray();
						transactionalStream.WritableStream.Write(array, 0, array.Length);
						transactionalStream.WritableStream.Write(array2, 0, array2.Length);
						transactionalStream.Commit();
					}
					flag3 = true;
				}
			}
			catch (ThreadAbortException)
			{
				flag = true;
				if (this._bBootInfoOnly)
				{
					Thread.ResetAbort();
				}
			}
			catch (IOException ex)
			{
				APEnvironmentFacade.Instance.Error(ex.Message, string.Empty, Array.Empty<object>());
				flag = true;
				if (!this._bBootInfoOnly)
				{
					throw;
				}
			}
			catch (Exception)
			{
				flag = true;
				if (!this._bBootInfoOnly)
				{
					throw;
				}
			}
			finally
			{
				if (flag)
				{
					if (flag2)
					{
						this._sideCarEntry.CreateAnother(Path.GetFileName(path)).Delete();
					}
					if (flag3)
					{
						this._sideCarEntry.CreateAnother(Path.GetFileName(path2)).Delete();
					}
				}
			}
		}

		// Token: 0x04000629 RID: 1577
		private readonly CompileContext _comconToSave;

		// Token: 0x0400062A RID: 1578
		private readonly string _stPath;

		// Token: 0x0400062B RID: 1579
		private readonly bool _bBootDuplicate;

		// Token: 0x0400062C RID: 1580
		private readonly bool _bBootInfoOnly;

		// Token: 0x0400062D RID: 1581
		private readonly ISideCarEntry _sideCarEntry;

		// Token: 0x020002BE RID: 702
		// (Invoke) Token: 0x06002C10 RID: 11280
		private delegate void VoidErrorMessagePrompt(Exception ex);
	}
}
