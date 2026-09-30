using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0003;
using \u000E;
using \u0010;
using \u0016;
using \u0019;
using \u001F;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.OnlineChange;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0002
{
	// Token: 0x02000402 RID: 1026
	internal sealed class \u0014
	{
		// Token: 0x1700097C RID: 2428
		// (get) Token: 0x060038D8 RID: 14552 RVA: 0x000EA864 File Offset: 0x000E8A64
		// (set) Token: 0x060038D9 RID: 14553 RVA: 0x000EA86C File Offset: 0x000E8A6C
		private global::\u000E.\u001B CompileInformation { get; set; }

		// Token: 0x1700097D RID: 2429
		// (get) Token: 0x060038DA RID: 14554 RVA: 0x000EA878 File Offset: 0x000E8A78
		private bool OnlineChange
		{
			get
			{
				return this.CompileInformation.OnlineChange;
			}
		}

		// Token: 0x1700097E RID: 2430
		// (get) Token: 0x060038DB RID: 14555 RVA: 0x000EA888 File Offset: 0x000E8A88
		private _ICompileContext ComconNew
		{
			get
			{
				return this.CompileInformation.ComconNew;
			}
		}

		// Token: 0x1700097F RID: 2431
		// (get) Token: 0x060038DC RID: 14556 RVA: 0x000EA898 File Offset: 0x000E8A98
		private _ICompileContext ComconOld
		{
			get
			{
				return this.CompileInformation.ComconOld;
			}
		}

		// Token: 0x17000980 RID: 2432
		// (get) Token: 0x060038DD RID: 14557 RVA: 0x000EA8A8 File Offset: 0x000E8AA8
		private static IMessageCategory MessageCategory
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;
			}
		}

		// Token: 0x17000981 RID: 2433
		// (get) Token: 0x060038DE RID: 14558 RVA: 0x000EA8BC File Offset: 0x000E8ABC
		private IOnlineChangeDetails OnlineChangeDetails
		{
			get
			{
				return this.CompileInformation.OnlineChangeDetails;
			}
		}

		// Token: 0x060038DF RID: 14559 RVA: 0x000EA8CC File Offset: 0x000E8ACC
		internal \u0014(global::\u000E.\u001B \u008F\u0004)
		{
			this.CompileInformation = \u008F\u0004;
		}

		// Token: 0x060038E0 RID: 14560 RVA: 0x000EA8DC File Offset: 0x000E8ADC
		internal void \u0001(ICodegenerator \u0002, ref bool \u0003)
		{
			\u0003 = (Messages.\u0001(this.ComconNew, APEnvironmentFacade.Instance.MessageStorage, global::\u0002.\u0014.MessageCategory) & \u0003);
			if (\u0003 && this.OnlineChange && this.OnlineChangeDetails != null)
			{
				this.\u0003();
			}
			this.ComconNew.ResetExprementHashTables();
			\u0002.EndGeneration();
			this.\u0001();
			this.\u0004();
			if (!this.OnlineChange)
			{
				this.ComconNew.LastCodeId = this.ComconNew.CodeId;
				this.ComconNew.LastDataId = this.ComconNew.DataId;
			}
			this.\u0002();
		}

		// Token: 0x060038E1 RID: 14561 RVA: 0x000EA97C File Offset: 0x000E8B7C
		private void \u0001()
		{
			OnlineChangeDetails onlineChangeDetails = this.OnlineChangeDetails as OnlineChangeDetails;
			if (onlineChangeDetails != null)
			{
				onlineChangeDetails.ResetOnlineChangeFlags();
				return;
			}
			foreach (_ISignature isignature in this.ComconNew.GetAllSignaturesFlatInvariant())
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					ivariable.SetFlag(VarFlag.LocationChanged | VarFlag.OnlChangeCopy | VarFlag.OnlChangeInit | VarFlag.OnlChangeVFInit | VarFlag.OnlChangeExit | VarFlag.OnlChangeReInit, false);
				}
			}
		}

		// Token: 0x060038E2 RID: 14562 RVA: 0x000EAA20 File Offset: 0x000E8C20
		private void \u0002()
		{
			if (global::\u0016.\u0004.CompactDownload.GetBoolValue(this.ComconNew.GetTargetSettings()) && this.ComconNew.DSFCallback != null && this.ComconNew.DSFCallback is IMemoryAllocationCallbackEmbedded)
			{
				int num = ((IMemoryAllocationCallbackEmbedded)this.ComconNew.DSFCallback).GlobalCodeReserve(this.ComconNew);
				ushort num2 = 0;
				int num3 = 0;
				if (num > 0)
				{
					bool flag = MemoryCompiler.\u0003(this.ComconNew.DataManager, ref num2, ref num3, 8, num, this.ComconNew.DataManager.CodeSegmentSize, DataSegmentFlags.Code);
					ushort num4 = 255;
					foreach (IArea area in this.ComconNew.DataManager.Areas)
					{
						if (area.Flags.HasFlag(DataSegmentFlags.Code))
						{
							num4 = (ushort)area.Index;
							break;
						}
					}
					if (!flag || num4 != num2)
					{
						string u = global::\u0003.\u0006.\u0001(MessageId.Err_NotEnoughMemoryForCompactDownload, new object[]
						{
							num
						});
						_ICompilerMessage message = global::\u0019.\u0003.\u0001(null, u, Severity.Error, MessageId.Err_NotEnoughMemoryForCompactDownload);
						APEnvironmentFacade.Instance.AddMessage(global::\u0002.\u0014.MessageCategory, message);
					}
				}
			}
		}

		// Token: 0x060038E3 RID: 14563 RVA: 0x000EAB5C File Offset: 0x000E8D5C
		internal void \u0003()
		{
			global::\u0010.\u0010.\u0001(this.ComconNew, this.OnlineChangeDetails);
		}

		// Token: 0x060038E4 RID: 14564 RVA: 0x000EAB70 File Offset: 0x000E8D70
		private static bool \u0001()
		{
			IOEMCustomization oemcustomization = APEnvironmentFacade.Instance.OEMCustomization;
			return oemcustomization != null && oemcustomization.HasValue("LanguageModelManager", "DisableChecksumComputation") && oemcustomization.GetBoolValue("LanguageModelManager", "DisableChecksumComputation");
		}

		// Token: 0x060038E5 RID: 14565 RVA: 0x000EABB0 File Offset: 0x000E8DB0
		internal void \u0004()
		{
			uint num;
			uint num2;
			if (global::\u0002.\u0014.\u0001())
			{
				Random random = new Random();
				num = (uint)random.Next(0, int.MaxValue);
				num2 = (uint)random.Next(0, int.MaxValue);
			}
			else
			{
				num = this.\u0001();
				num2 = this.\u0002();
			}
			this.ComconNew.CheckSumCode = num2;
			this.ComconNew.CheckSumData = num;
			if (this.ComconOld != null)
			{
				this.ComconNew.CheckSumCodeLast = this.ComconOld.CheckSumCode;
				this.ComconNew.CheckSumDataLast = this.ComconOld.CheckSumData;
			}
			else
			{
				this.ComconNew.CheckSumCodeLast = num2;
				this.ComconNew.CheckSumDataLast = num;
			}
			byte[] array = new byte[16];
			IntegerUnion integerUnion = new IntegerUnion
			{
				m_int0 = (int)this.ComconNew.CheckSumCode
			};
			array[0] = integerUnion.m_byte0;
			array[1] = integerUnion.m_byte1;
			array[2] = integerUnion.m_byte2;
			array[3] = integerUnion.m_byte3;
			this.ComconNew.CodeId = new Guid(array);
			integerUnion.m_int0 = (int)this.ComconNew.CheckSumData;
			array[0] = integerUnion.m_byte0;
			array[1] = integerUnion.m_byte1;
			array[2] = integerUnion.m_byte2;
			array[3] = integerUnion.m_byte3;
			this.ComconNew.DataId = new Guid(array);
		}

		// Token: 0x060038E6 RID: 14566 RVA: 0x000EACF8 File Offset: 0x000E8EF8
		private uint \u0001()
		{
			MyChecksumStream myChecksumStream = new MyChecksumStream(false);
			BinaryWriter binaryWriter = new BinaryWriter(myChecksumStream);
			LList<ISignature> llist = new LList<ISignature>();
			IList<ISignature4> allSignaturesFlatEx = this.ComconNew.GetAllSignaturesFlatEx();
			\u001F.\u0003 u = new \u001F.\u0003();
			LDictionary<int, bool> ldictionary = new LDictionary<int, bool>();
			for (int i = 0; i < (int)this.ComconNew.DataManager.AreaCount; i++)
			{
				_IArea area = this.ComconNew.DataManager.GetArea(i);
				ldictionary[area.Index] = (area.GetDataSegmentFlag(DataSegmentFlags.Retain) || area.GetDataSegmentFlag(DataSegmentFlags.Persistent));
			}
			LDictionary<int, LList<Tuple<ISignature, _IVariable>>> ldictionary2 = new LDictionary<int, LList<Tuple<ISignature, _IVariable>>>();
			foreach (_ISignature isignature in allSignaturesFlatEx.OfType<_ISignature>())
			{
				llist.Add(isignature);
				if (isignature.GetFlag(SignatureFlag.ContainsPersistent) || isignature.GetFlag(SignatureFlag.ContainsRetain))
				{
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.DataLocation != null && ivariable.DataLocation.Area != 65535 && ldictionary.ContainsKey((int)ivariable.DataLocation.Area) && ldictionary[(int)ivariable.DataLocation.Area])
						{
							LList<Tuple<ISignature, _IVariable>> llist2;
							if (ldictionary2.ContainsKey((int)ivariable.DataLocation.Area))
							{
								llist2 = ldictionary2[(int)ivariable.DataLocation.Area];
							}
							else
							{
								llist2 = new LList<Tuple<ISignature, _IVariable>>();
								ldictionary2.Add((int)ivariable.DataLocation.Area, llist2);
							}
							llist2.Add(new Tuple<ISignature, _IVariable>(isignature, ivariable));
						}
					}
				}
			}
			llist.Sort(u);
			\u0084.\u0003 u2 = new \u0084.\u0003();
			foreach (LList<Tuple<ISignature, _IVariable>> llist3 in ldictionary2.Values)
			{
				llist3.Sort(u2);
			}
			using (IEnumerator<_ISignature> enumerator = llist.OfType<_ISignature>().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					uint value;
					if (global::\u0002.\u0014.\u0001(enumerator.Current, out value))
					{
						binaryWriter.Write(value);
					}
				}
			}
			IScope2 u3 = this.ComconNew.CreateGlobalIScope() as IScope2;
			foreach (int num in ldictionary2.Keys)
			{
				LList<Tuple<ISignature, _IVariable>> llist4 = ldictionary2[num];
				MyChecksumStream myChecksumStream2 = new MyChecksumStream(false);
				BinaryWriter binaryWriter2 = new BinaryWriter(myChecksumStream2);
				binaryWriter2.Write(APEnvironmentFacade.Instance.LanguageModelMgr.GetApplicationNameByGuid(this.ComconNew.ApplicationGuid));
				foreach (Tuple<ISignature, _IVariable> tuple in llist4)
				{
					int offset = tuple.Item2.DataLocation.Offset;
					binaryWriter2.Write(tuple.Item1.Name);
					if (tuple.Item2.GetFlag(VarFlag.Implicit))
					{
						binaryWriter2.Write("IMPLICIT");
					}
					else
					{
						binaryWriter2.Write(tuple.Item2.Name);
					}
					binaryWriter2.Write(offset);
					Debug.\u0001(global::\u0003.\u0002.\u0001(binaryWriter2, tuple.Item2.CompiledType, this.ComconNew, u3));
				}
				binaryWriter2.Flush();
				myChecksumStream2.Close();
				this.ComconNew.DataManager.GetAreaByIndex(num).Checksum = myChecksumStream2.Checksum;
			}
			binaryWriter.Flush();
			myChecksumStream.Close();
			return myChecksumStream.Checksum;
		}

		// Token: 0x060038E7 RID: 14567 RVA: 0x000EB164 File Offset: 0x000E9364
		private static bool \u0001(_ISignature \u0002, out uint \u0003)
		{
			\u0003 = 0U;
			if (!\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				return false;
			}
			string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			try
			{
				\u0003 = uint.Parse(attributeValue);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x060038E8 RID: 14568 RVA: 0x000EB1B4 File Offset: 0x000E93B4
		private uint \u0002()
		{
			ICRCSum icrcsum = APEnvironmentFacade.Instance.LanguageModelMgr.CreateCheckSumComputer();
			IList<ICompiledPOU4> allCompiledPOUsEx = this.ComconNew.GetAllCompiledPOUsEx();
			LDictionary<IDataLocation, _ICompiledPOU> ldictionary = new LDictionary<IDataLocation, _ICompiledPOU>(allCompiledPOUsEx.Count);
			foreach (_ICompiledPOU icompiledPOU in allCompiledPOUsEx.OfType<_ICompiledPOU>())
			{
				if (icompiledPOU.CompiledCode != null && icompiledPOU.CompiledCode.Location != null && !icompiledPOU.GetFlag(CompiledPOUFlags.ToRemoveAfterDownload) && !icompiledPOU.GetFlag(CompiledPOUFlags.IgnoreForChecksum))
				{
					ldictionary[icompiledPOU.CompiledCode.Location] = icompiledPOU;
				}
			}
			LList<IDataLocation> llist = Enumerable.ToLList<IDataLocation>(ldictionary.Keys);
			llist.Sort();
			foreach (IDataLocation dataLocation in llist)
			{
				_ICompiledPOU icompiledPOU2 = ldictionary[dataLocation];
				IntegerUnion integerUnion = default(IntegerUnion);
				integerUnion.m_short0 = (short)icompiledPOU2.CompiledCode.Location.Area;
				integerUnion.m_uint1 = (uint)icompiledPOU2.CompiledCode.Location.Offset;
				byte[] array = new byte[12];
				BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(array));
				binaryWriter.Write(integerUnion.m_ulong);
				binaryWriter.Write(Helper.\u0001(icompiledPOU2));
				binaryWriter.Flush();
				icrcsum.CRC32Update(array, array.Length);
			}
			return icrcsum.CRC32Finish(Array.Empty<byte>(), 0);
		}

		// Token: 0x04000B57 RID: 2903
		[CompilerGenerated]
		private global::\u000E.\u001B \u0001;
	}
}
