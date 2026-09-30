using System;
using \u0007;
using \u0014;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.PreCompile;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0006
{
	// Token: 0x020003C9 RID: 969
	internal static class \u0012
	{
		// Token: 0x060036E2 RID: 14050 RVA: 0x000DFC54 File Offset: 0x000DDE54
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			_IDataManager idataManager = MemoryCompiler.\u0001(\u0002.DataManager);
			MemoryCompiler.\u0001(idataManager);
			LList<_IDataSegment> llist = new LList<_IDataSegment>();
			for (int i = 0; i < idataManager.Count; i++)
			{
				_IDataSegment idataSegment = idataManager[i];
				_IArea iarea = idataManager.GetAreaByIndex((int)idataSegment.Area);
				if (iarea != null && global::\u0006.\u0012.\u0001(idataSegment, idataManager))
				{
					iarea = null;
				}
				else if (iarea == null && \u0002.ParentContext != null && \u0002.ParentContext.DataManager != null)
				{
					_IDataManager idataManager2 = MemoryCompiler.\u0001(\u0002.ParentContext.DataManager);
					if (!global::\u0006.\u0012.\u0001(idataSegment, idataManager2))
					{
						iarea = idataManager2.GetAreaByIndex((int)idataSegment.Area);
						if ((iarea.AreaFlags & (AreaFlags)2147483648U) == AreaFlags.None)
						{
							throw new InvalidOperationException("Invalid memory configuration. A data segment has been mapped to an area not marked with MappedSegmentAccessOnly");
						}
					}
				}
				if (iarea != null)
				{
					llist.Add(idataSegment);
				}
			}
			LStringBuilder lstringBuilder = new LStringBuilder();
			lstringBuilder.AppendLine("{attribute 'hide'}");
			lstringBuilder.AppendLine("{attribute 'qualified_only'}");
			lstringBuilder.AppendLine("VAR_GLOBAL");
			lstringBuilder.AppendLine("{attribute 'init_on_onlchange'}");
			lstringBuilder.AppendLine(string.Format("\tarrdataSegments : ARRAY [0..{0}] OF __SYSTEM.__SegmentInfo := [", llist.Count - 1));
			bool flag = true;
			foreach (_IDataSegment idataSegment2 in llist)
			{
				if (!flag)
				{
					lstringBuilder.AppendLine(",");
				}
				flag = false;
				DataSegmentFlags dataSegmentFlags = idataSegment2.Flags & ~DataSegmentFlags.Area;
				lstringBuilder.AppendFormat("\t\t(dsKindOf := {0},", new object[]
				{
					(ushort)dataSegmentFlags
				});
				lstringBuilder.AppendFormat("uiArea\t := {0},", new object[]
				{
					idataSegment2.Area
				});
				lstringBuilder.AppendFormat("udOffset\t := {0},", new object[]
				{
					(uint)idataSegment2.Address
				});
				lstringBuilder.AppendFormat("udSize\t := {0},", new object[]
				{
					(uint)idataSegment2.Size
				});
				lstringBuilder.AppendFormat("udHighestUsedAddress := {0})", new object[]
				{
					(uint)idataSegment2.SizeAllocated
				});
			}
			lstringBuilder.AppendLine("];");
			lstringBuilder.AppendLine();
			lstringBuilder.AppendLine("{attribute 'init_on_onlchange'}");
			lstringBuilder.AppendLine(string.Format("\tdataSegments : __SYSTEM.__DataSegments := (nNumOfSegments := {0}, pDataSegments := ADR(arrdataSegments));", llist.Count));
			lstringBuilder.AppendLine("END_VAR");
			_ISignature isignature = ParserHelper.\u0001("__DataSegmentInfoVariables", lstringBuilder.ToString(), true);
			_ISignature isignature2 = null;
			if (\u0003 != null)
			{
				isignature2 = \u0003[isignature.Name];
			}
			isignature = isignature.CreateCompiledSignature(isignature2, \u0002.HasByteSupport());
			\u0002.AddSignature(isignature, isignature2, \u0003, true);
			IScope5 u = global::\u0007.\u0005.\u0001(\u0002, isignature.Id);
			global::\u0014.\u0013.\u0002(isignature, u, \u0002);
			Locator.\u0001(isignature, isignature2, \u0002, \u0003);
		}

		// Token: 0x060036E3 RID: 14051 RVA: 0x000DFF34 File Offset: 0x000DE134
		private static bool \u0001(_IDataSegment \u0002, _IDataManager \u0003)
		{
			_IArea areaByIndex = \u0003.GetAreaByIndex((int)\u0002.Area);
			return (areaByIndex.DataSegmentFlags == \u0002.Flags && (\u0002.Flags == DataSegmentFlags.Memory || \u0002.Flags == DataSegmentFlags.Input || \u0002.Flags == DataSegmentFlags.Output) && areaByIndex.Size == \u0002.Size && \u0002.Address == 0) || (areaByIndex.Dynamic && ((\u0002.SizeAllocated == 0 && \u0002.Flags == (DataSegmentFlags.Retain | DataSegmentFlags.Area) && \u0003._MemorySettings.RetainDynamic) || (\u0002.SizeAllocated == 0 && (\u0002.Flags == (DataSegmentFlags.Area | DataSegmentFlags.Persistent) || \u0002.Flags == (DataSegmentFlags.Retain | DataSegmentFlags.Area | DataSegmentFlags.Persistent)) && \u0003._MemorySettings.PersistentDynamic)));
		}
	}
}
