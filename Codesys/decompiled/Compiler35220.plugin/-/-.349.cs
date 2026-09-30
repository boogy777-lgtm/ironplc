using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0007;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0081;

namespace \u0005
{
	// Token: 0x02000390 RID: 912
	internal sealed class \u0007 : IMemoryStatisticsOutputProvider
	{
		// Token: 0x170008B8 RID: 2232
		// (get) Token: 0x06003517 RID: 13591 RVA: 0x000D1034 File Offset: 0x000CF234
		internal static IMemoryStatisticsOutputProvider Singleton
		{
			get
			{
				if (APEnvironmentFacade.Instance.InjectionCompleted && APEnvironmentFacade.Instance.MemoryStatisticsOutputProviderOrNull != null)
				{
					return APEnvironmentFacade.Instance.MemoryStatisticsOutputProviderOrNull;
				}
				if (global::\u0005.\u0007.\u0001 == null)
				{
					global::\u0005.\u0007.\u0001 = new global::\u0005.\u0007();
				}
				return global::\u0005.\u0007.\u0001;
			}
		}

		// Token: 0x06003518 RID: 13592 RVA: 0x000D1070 File Offset: 0x000CF270
		public void \u0001(ILMCompiledApplicationSet \u0002)
		{
			IMessageStorage messageStorage = APEnvironmentFacade.Instance.MessageStorage;
			IMessageCategory messageCategory = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			if (APEnvironmentFacade.Instance.DebugFlagSet)
			{
				_IDataManager dataManager = ((_ICompileContext)\u0002).DataManager;
				int num = global::\u0005.\u0007.\u0001((_ICompileContext)\u0002);
				int num2 = global::\u0005.\u0007.\u0002((_ICompileContext)\u0002);
				if (num > 0)
				{
					string u = string.Format(\u0081.\u0001.CodeSizeOutput, num);
					_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u, Severity.Information, MessageId.None);
					messageStorage.AddMessage(messageCategory, message);
				}
				if (num2 > 0)
				{
					string u2 = string.Format(\u0081.\u0001.DataSizeOutput, num2);
					_ICompilerMessage message2 = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u2, Severity.Information, MessageId.None);
					messageStorage.AddMessage(messageCategory, message2);
				}
				int num3 = 0;
				for (int i = 0; i < dataManager.Count; i++)
				{
					_IDataSegment idataSegment = dataManager[(ushort)i];
					if (idataSegment.GetFlag(DataSegmentFlags.Data) || idataSegment.GetFlag(DataSegmentFlags.Code))
					{
						num3 += idataSegment.SizeAllocated;
					}
				}
				if (num3 != 0)
				{
					string u3 = string.Format(\u0081.\u0001.TotalMemorySizeCodeAndData, num3);
					_ICompilerMessage message3 = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u3, Severity.Information, MessageId.None);
					messageStorage.AddMessage(messageCategory, message3);
				}
			}
			global::\u0005.\u0007.\u0001((ICompileContext)\u0002, messageStorage, messageCategory);
		}

		// Token: 0x06003519 RID: 13593 RVA: 0x000D11B0 File Offset: 0x000CF3B0
		private static bool \u0001(_IDataManager \u0002)
		{
			for (int i = 0; i < (int)\u0002.AreaCount; i++)
			{
				_IArea areaByIndex = \u0002.GetAreaByIndex(i);
				if (areaByIndex != null && areaByIndex.AvailableSize > 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600351A RID: 13594 RVA: 0x000D11E8 File Offset: 0x000CF3E8
		private static _IMemoryManager[] \u0001(_ICompileContext \u0002, _IDataManager \u0003)
		{
			return MemoryCompiler.\u0001(\u0003, \u0002);
		}

		// Token: 0x0600351B RID: 13595 RVA: 0x000D11F4 File Offset: 0x000CF3F4
		private static string \u0001(_IDataSegment \u0002, _IDataManager \u0003)
		{
			string text = string.Empty;
			DataSegmentFlags dataSegmentFlags = DataSegmentFlags.None;
			if (\u0002.GetFlag(DataSegmentFlags.Input))
			{
				dataSegmentFlags = DataSegmentFlags.Input;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Output))
			{
				dataSegmentFlags = DataSegmentFlags.Output;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Memory))
			{
				dataSegmentFlags = DataSegmentFlags.Memory;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Code))
			{
				dataSegmentFlags = DataSegmentFlags.Code;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Retain))
			{
				dataSegmentFlags = DataSegmentFlags.Retain;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Persistent))
			{
				dataSegmentFlags = DataSegmentFlags.Persistent;
			}
			if (\u0002.GetFlag(DataSegmentFlags.NonSafety))
			{
				dataSegmentFlags = DataSegmentFlags.NonSafety;
			}
			bool flag = false;
			if (\u0002.GetFlag(DataSegmentFlags.Data))
			{
				text = text + " " + \u0081.\u0001.Data_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Input))
			{
				if (dataSegmentFlags == DataSegmentFlags.Input && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Input_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Output))
			{
				if (dataSegmentFlags == DataSegmentFlags.Output && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Output_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Memory))
			{
				if (dataSegmentFlags == DataSegmentFlags.Memory && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Memory_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Code))
			{
				if (dataSegmentFlags == DataSegmentFlags.Code && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Code_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Retain) && (!\u0002.GetFlag(DataSegmentFlags.Persistent) || \u0003._MemorySettings.OneSRAM))
			{
				if (dataSegmentFlags == DataSegmentFlags.Retain && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Retain_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.Persistent))
			{
				if (dataSegmentFlags == DataSegmentFlags.Persistent && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.Persistent_Text;
				flag = true;
			}
			if (\u0002.GetFlag(DataSegmentFlags.NonSafety))
			{
				if (dataSegmentFlags == DataSegmentFlags.NonSafety && flag)
				{
					text = text + " " + \u0081.\u0001.And_Text;
				}
				else if (text != string.Empty)
				{
					text += ",";
				}
				text = text + " " + \u0081.\u0001.NonSafety_Text;
			}
			return text;
		}

		// Token: 0x0600351C RID: 13596 RVA: 0x000D14EC File Offset: 0x000CF6EC
		private static bool \u0001(IDeviceIdentification \u0002)
		{
			if (\u0002 == null)
			{
				return false;
			}
			if (string.IsNullOrEmpty(\u0002.Id))
			{
				return false;
			}
			string[] array = \u0002.Id.Split(new char[]
			{
				' '
			});
			uint num;
			return 2 == array.Length && uint.TryParse(array[0], NumberStyles.HexNumber, null, out num) && num == 0U;
		}

		// Token: 0x0600351D RID: 13597 RVA: 0x000D1544 File Offset: 0x000CF744
		private static void \u0001(ICompileContext \u0002, IMessageStorage \u0003, IMessageCategory \u0004)
		{
			_ICompileContext icompileContext = \u0002 as _ICompileContext;
			_IDataManager idataManager = MemoryCompiler.\u0001(icompileContext.DataManager);
			_IArea[] u = MemoryCompiler.\u0001(idataManager);
			bool flag = global::\u0005.\u0007.\u0001(idataManager);
			_IMemoryManager[] u2 = null;
			if (flag)
			{
				u2 = global::\u0005.\u0007.\u0001(icompileContext, idataManager);
			}
			IList<_IDataSegment> areaSegments = idataManager.AreaSegments;
			if (areaSegments != null && areaSegments.Count > 0)
			{
				for (int i = 0; i < areaSegments.Count; i++)
				{
					_IDataSegment u3 = areaSegments[i];
					global::\u0005.\u0007.\u0001(\u0003, \u0004, idataManager, u, u2, i, u3);
				}
			}
		}

		// Token: 0x0600351E RID: 13598 RVA: 0x000D15C4 File Offset: 0x000CF7C4
		private static void \u0001(IMessageStorage \u0002, IMessageCategory \u0003, _IDataManager \u0004, _IArea[] \u0005, _IMemoryManager[] \u0006, int \u0007, _IDataSegment \u0008)
		{
			global::\u0005.\u0007.\u0001 u = new global::\u0005.\u0007.\u0001();
			if (2147483647 == \u0008.Size || \u0008.Size == 0)
			{
				return;
			}
			string text = global::\u0005.\u0007.\u0001(\u0008, \u0004);
			_IMemoryManager imemoryManager = null;
			if (\u0006 != null)
			{
				imemoryManager = \u0006[(int)\u0008.Area];
			}
			u.\u0001 = \u0004.GetAreaByIndex((int)\u0008.Area);
			if (\u0005 != null && !\u0005.Any(new Func<_IArea, bool>(u.\u0001)))
			{
				return;
			}
			int availableSize = u.\u0001.AvailableSize;
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			if (availableSize > 0 && imemoryManager != null)
			{
				int sizeWithoutGaps = imemoryManager.SizeWithoutGaps;
				if (sizeWithoutGaps < availableSize)
				{
					string u2 = string.Format(\u0081.\u0001.Area_Output_Checked, text, sizeWithoutGaps, availableSize);
					\u0002.AddMessage(\u0003, \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u2, Severity.Information, MessageId.None));
					return;
				}
			}
			else
			{
				global::\u0005.\u0007.\u0001(\u0002, \u0003, \u0004, \u0007, \u0008, text);
			}
		}

		// Token: 0x0600351F RID: 13599 RVA: 0x000D169C File Offset: 0x000CF89C
		private static void \u0001(IMessageStorage \u0002, IMessageCategory \u0003, _IDataManager \u0004, int \u0005, _IDataSegment \u0006, string \u0007)
		{
			if (\u0006.GetFlag(DataSegmentFlags.Retain))
			{
				foreach (_IDataSegment idataSegment in \u0004.AllDataSegmentsByFlag(DataSegmentFlags.Retain))
				{
					if (idataSegment.Area == \u0006.Area && !idataSegment.GetFlag(DataSegmentFlags.Area))
					{
						\u0006 = idataSegment;
						break;
					}
				}
			}
			long num = (long)\u0006.MaxContiguosMemory;
			long num2 = (long)\u0006.Size;
			long num3 = (long)\u0006.SizeAllocated;
			long num4 = 0L;
			if (num2 != 0L)
			{
				num4 = num * 100L / num2;
			}
			string u;
			if (\u0006.GetFlag(DataSegmentFlags.Persistent) && \u0004._MemorySettings.OneSRAM)
			{
				u = string.Format(\u0081.\u0001.Area_Output_Info_Simple_2, new object[]
				{
					\u0005,
					\u0007,
					num2,
					num,
					num4
				});
			}
			else
			{
				u = string.Format(\u0081.\u0001.Area_Output_Info, new object[]
				{
					\u0005,
					\u0007,
					num2,
					num3,
					num,
					num4
				});
			}
			_ICompilerMessage message = \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u, Severity.Information, MessageId.None);
			\u0002.AddMessage(\u0003, message);
		}

		// Token: 0x06003520 RID: 13600 RVA: 0x000D17DC File Offset: 0x000CF9DC
		internal static bool \u0001(_ICompileContext \u0002, IMessageStorage \u0003, IMessageCategory \u0004)
		{
			_IDataManager idataManager = MemoryCompiler.\u0001(\u0002.DataManager);
			_IArea[] array = MemoryCompiler.\u0001(idataManager);
			if (!global::\u0005.\u0007.\u0001(idataManager))
			{
				return true;
			}
			_IMemoryManager[] array2 = global::\u0005.\u0007.\u0001(\u0002, idataManager);
			bool result = true;
			IList<_IDataSegment> areaSegments = idataManager.AreaSegments;
			for (int i = 0; i < areaSegments.Count; i++)
			{
				_IDataSegment idataSegment = areaSegments[i];
				if (idataSegment.Size != 2147483647 && idataSegment.Size != 0)
				{
					global::\u0005.\u0007.\u0002 u = new global::\u0005.\u0007.\u0002();
					string text = global::\u0005.\u0007.\u0001(idataSegment, idataManager);
					_IMemoryManager imemoryManager = null;
					if (array2 != null)
					{
						imemoryManager = array2[(int)idataSegment.Area];
					}
					u.\u0001 = idataManager.GetAreaByIndex((int)idataSegment.Area);
					if (array == null || array.Where(new Func<_IArea, bool>(u.\u0001)).Count<_IArea>() != 0)
					{
						int availableSize = u.\u0001.AvailableSize;
						if (text != string.Empty && availableSize > 0 && imemoryManager != null)
						{
							int sizeWithoutGaps = imemoryManager.SizeWithoutGaps;
							if (sizeWithoutGaps >= availableSize)
							{
								string u2 = string.Format(\u0081.\u0001.Area_Output_Checked_Exceeded, text, sizeWithoutGaps, availableSize);
								\u0003.AddMessage(\u0004, \u0019.\u0003.\u0001(\u0019.\u0003.\u0001(), u2, Severity.Error, MessageId.None));
								result = false;
							}
						}
					}
				}
			}
			return result;
		}

		// Token: 0x06003521 RID: 13601 RVA: 0x000D1918 File Offset: 0x000CFB18
		private static int \u0001(_ICompileContext \u0002)
		{
			int num = 0;
			foreach (_ICompiledPOU icompiledPOU in \u0002.CompiledPOUList)
			{
				if (icompiledPOU.CompiledCode != null && !icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob) && !icompiledPOU.GetFlag(CompiledPOUFlags.Blob))
				{
					num += icompiledPOU.CompiledCode.CodeSize;
				}
			}
			return num;
		}

		// Token: 0x06003522 RID: 13602 RVA: 0x000D1994 File Offset: 0x000CFB94
		private static int \u0002(_ICompileContext \u0002)
		{
			int num = 0;
			IScope scope = global::\u0007.\u0005.\u0001(\u0002);
			foreach (_ISignature isignature in \u0002.AllSignatureList)
			{
				if ((isignature.POUType == Operator.VarGlobal || isignature.POUType == Operator.VarConfig || isignature.POUType == Operator.VarAccess || isignature.POUType == Operator.Type || isignature.POUType == Operator.FunctionBlock || isignature.POUType == Operator.Interface || isignature.POUType == Operator.Function || isignature.POUType == Operator.Method || isignature.POUType == Operator.Program || isignature.POUType == Operator.None) && ((isignature.POUType != Operator.Function && isignature.POUType != Operator.Method && isignature.POUType != Operator.Program) || \u0002.GetCompiledPOUById(isignature.Id) == null) && (isignature.POUType == Operator.Program || isignature.POUType == Operator.VarGlobal || isignature.Statics.Length != 0))
				{
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.DataLocation != null && ivariable._Type != null && !ivariable.DataLocation.IsRelativ && ivariable.GetFlag(VarFlag.Absolut) && !ivariable.GetFlag(VarFlag.ReplacedConstant) && !ivariable.GetFlag(VarFlag.Implicit))
						{
							int num2 = ivariable._Type.Size(scope);
							num += num2;
						}
					}
				}
			}
			foreach (_ICompiledPOU icompiledPOU in \u0002.CompiledPOUList)
			{
				_ISignature isignature2 = \u0002[icompiledPOU.SignatureId];
				if (isignature2 != null && (isignature2.POUType == Operator.Program || isignature2.POUType == Operator.VarGlobal || isignature2.Statics.Length != 0))
				{
					foreach (_IVariable ivariable2 in isignature2.AllVariables)
					{
						if (ivariable2.DataLocation != null && ivariable2._Type != null && !ivariable2.DataLocation.IsRelativ && ivariable2.GetFlag(VarFlag.Absolut) && !ivariable2.GetFlag(VarFlag.ReplacedConstant) && !ivariable2.GetFlag(VarFlag.Implicit))
						{
							int num3 = ivariable2._Type.Size(scope);
							num += num3;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x04000A51 RID: 2641
		private static IMemoryStatisticsOutputProvider \u0001;

		// Token: 0x02000391 RID: 913
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06003525 RID: 13605 RVA: 0x000D1C94 File Offset: 0x000CFE94
			internal bool \u0001(_IArea \u0002)
			{
				return \u0002.Index == this.\u0001.Index;
			}

			// Token: 0x04000A52 RID: 2642
			public _IArea \u0001;
		}

		// Token: 0x02000392 RID: 914
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06003527 RID: 13607 RVA: 0x000D1CB4 File Offset: 0x000CFEB4
			internal bool \u0001(_IArea \u0002)
			{
				return \u0002.Index == this.\u0001.Index;
			}

			// Token: 0x04000A53 RID: 2643
			public _IArea \u0001;
		}
	}
}
