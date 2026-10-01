using System;
using System.Collections.Generic;
using System.Linq;
using \u0002;
using \u0003;
using \u0006;
using \u0007;
using \u0011;
using \u0012;
using \u0014;
using \u0016;
using \u0019;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Phase1_Typification;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0081;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase3_Location
{
	// Token: 0x020002F0 RID: 752
	internal sealed class Locator
	{
		// Token: 0x06002E13 RID: 11795 RVA: 0x000A93A8 File Offset: 0x000A75A8
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			IList<_ISignature> allSignatureList = \u0002.AllSignatureList;
			foreach (_ISignature isignature in allSignatureList)
			{
				if (isignature.POUType != Operator.VarGlobal || isignature.HasAttribute("subsequent"))
				{
					_ISignature u = null;
					if (\u0003 != null)
					{
						u = \u0003[isignature.Id];
					}
					Locator.\u0001(\u0002.DataManager, \u0002, \u0003, isignature, u);
				}
			}
			foreach (_ISignature isignature2 in allSignatureList)
			{
				foreach (_ISignature isignature3 in isignature2.GetSubSignatures())
				{
					_ISignature u2 = null;
					if (\u0003 != null)
					{
						u2 = \u0003[isignature3.Id];
					}
					Locator.\u0001(\u0002.DataManager, \u0002, \u0003, isignature3, u2);
				}
			}
		}

		// Token: 0x06002E14 RID: 11796 RVA: 0x000A94A0 File Offset: 0x000A76A0
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, out bool \u0005)
		{
			IList<_ISignature> list = Locator.\u0001(\u0002.AllFlat);
			\u0005 = false;
			foreach (_ISignature isignature in list)
			{
				_ISignature u = null;
				if (\u0003 != null)
				{
					u = \u0003[isignature.Id];
				}
				bool flag = !global::\u0014.\u0013.\u0007(isignature, \u0002.CreateIScope(isignature.Id) as _IScope, \u0002);
				if (!flag)
				{
					Locator.\u0001(isignature, u, \u0002, \u0003, out flag);
				}
				\u0005 = (\u0005 || flag);
			}
			foreach (_ISignature u2 in list)
			{
				bool flag2;
				Locator.\u0001(\u0002, u2, out flag2);
				\u0005 = (\u0005 || flag2);
			}
		}

		// Token: 0x06002E15 RID: 11797 RVA: 0x000A9578 File Offset: 0x000A7778
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			ushort u = 0;
			int u2 = 0;
			if (\u0004 != null && \u0004.FPDataLocation != null)
			{
				\u0003.FPDataLocation = \u0004.FPDataLocation;
				MemoryCompiler.\u0002(\u0002.DataManager, \u0003.FPDataLocation.Area, \u0003.FPDataLocation.Offset, \u0002.PointerSize, DataSegmentFlags.None);
				return;
			}
			DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Data;
			if (\u0002.DSFCallback != null)
			{
				dataSegmentFlags = \u0002.DSFCallback.GetDataSegmentFlagForFunctionPointer(\u0003, \u0002, dataSegmentFlags);
			}
			if (!MemoryCompiler.\u0003(\u0002.DataManager, ref u, ref u2, \u0002.PointerSize, \u0002.PointerSize, CompilerServicesInternal.NoSegmentation, dataSegmentFlags))
			{
				\u0003.AddMessage(Severity.Error, MessageId.Err_OutOfMemoryFunctionPointer, new object[]
				{
					\u0003.OrgName,
					\u0002.PointerSize
				});
			}
			\u0003.FPDataLocation = global::\u0019.\u0003.\u0001(u, u2);
		}

		// Token: 0x06002E16 RID: 11798 RVA: 0x000A963C File Offset: 0x000A783C
		internal static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			foreach (_ISignature isignature in \u0002.POUSignatures)
			{
				_ISignature u = null;
				if (\u0003 != null)
				{
					u = \u0003[isignature.Id];
				}
				if (isignature.POUType != Operator.Interface && (isignature.POUType == Operator.Function || isignature.POUType == Operator.Program))
				{
					Locator.\u0001(\u0002, isignature, u);
				}
			}
			foreach (_ISignature isignature2 in \u0002.AllSignatures)
			{
				if (isignature2.POUType != Operator.Interface)
				{
					foreach (_ISignature isignature3 in isignature2.SubSignatures)
					{
						_ISignature u2 = null;
						if (\u0003 != null)
						{
							u2 = \u0003[isignature3.Id];
						}
						if (isignature3.POUType == Operator.Method)
						{
							Locator.\u0001(\u0002, isignature3, u2);
						}
					}
				}
			}
			\u0084.\u0015.\u0001(\u0002, \u0003);
		}

		// Token: 0x06002E17 RID: 11799 RVA: 0x000A9748 File Offset: 0x000A7948
		internal static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003, bool \u0004, out bool \u0005)
		{
			Locator.\u0001(\u0002, \u0003, \u0004, out \u0005);
			Locator.\u0002(\u0002, \u0003);
		}

		// Token: 0x06002E18 RID: 11800 RVA: 0x000A975C File Offset: 0x000A795C
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			bool flag;
			Locator.\u0001(\u0002, \u0003, \u0004, \u0005, out flag);
			Locator.\u0001(\u0004.DataManager, \u0004, \u0005, \u0002, \u0003);
		}

		// Token: 0x06002E19 RID: 11801 RVA: 0x000A9784 File Offset: 0x000A7984
		internal static void \u0001(_ICompileContext \u0002, _ISignature \u0003, out bool \u0004)
		{
			\u0004 = false;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002, \u0003.Id);
			foreach (IVariable variable in \u0003.AllExternals)
			{
				_IVariable ivariable = (_IVariable)variable;
				_IExpression iexpression = global::\u0019.\u0003.Builder.ParseExpression(ivariable.VersionedName) as _IExpression;
				Debug.\u0001(iexpression != null);
				ExpressionTypifierWithSpecialTasks ivisit = new ExpressionTypifierWithSpecialTasks(scope, \u0002, true, \u0003, ivariable);
				iexpression.Accept(ivisit);
				_IVariable ivariable2 = iexpression.GetVariable(scope) as _IVariable;
				_ISignature isignature = iexpression.GetSignatureEx(scope) as _ISignature;
				Debug.\u0001(ivariable2 != null);
				ivariable.DataLocation = ivariable2.DataLocation;
				if (!global::\u0006.\u0011.\u0001(ivariable2.CompiledType, ivariable.CompiledType, scope as ICommonScope))
				{
					string u = global::\u0003.\u0006.\u0001(MessageId.Err_WrongTypeForExternal, new object[]
					{
						ivariable.OrgName
					});
					\u0003.AddError(global::\u0019.\u0003.\u0001(ivariable.SourcePosition, u, Severity.Error, MessageId.Err_WrongTypeForExternal));
					_ISourcePosition sourcepos = global::\u0019.\u0003.\u0001(APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath), isignature.ObjectGuid, ivariable2.SourcePosition.Position, ivariable2.SourcePosition.PositionOffset, (short)ivariable2.OrgName.Length);
					\u0003.AddMessageString(sourcepos, Severity.Information, \u0081.\u0002.Inf_RelatedPosition, Array.Empty<object>());
					\u0004 = true;
				}
				else
				{
					ivariable2.AddCrossReference(\u0003.Id, null);
				}
			}
		}

		// Token: 0x06002E1A RID: 11802 RVA: 0x000A9918 File Offset: 0x000A7B18
		internal static IDataLocation \u0001(_ICompileContext \u0002, out bool \u0003, IDirectVariable \u0004)
		{
			IMessage message;
			return Locator.\u0001(\u0002, out message, out \u0003, global::\u0019.\u0003.\u0001(), \u0004, null);
		}

		// Token: 0x06002E1B RID: 11803 RVA: 0x000A9938 File Offset: 0x000A7B38
		internal static IDataLocation \u0001(_ICompileContext \u0002, out IMessage \u0003, out bool \u0004, ISourcePosition \u0005, IDirectVariable \u0006, IVariable2 \u0007)
		{
			_IDirectLocationInfo idirectLocationInfo;
			if (\u0002.DirvarLocationTable.TryGetLocationInfo(\u0006, \u0007, out idirectLocationInfo))
			{
				\u0003 = idirectLocationInfo.Message;
				\u0004 = idirectLocationInfo.Error;
				return idirectLocationInfo.DatLoc;
			}
			IAddressCalculator addressCalculator = null;
			IAddressCalculator addressCalculator2 = new AddressCalculator();
			string stringValue = global::\u0016.\u0004.AddressCalculatorGuid.GetStringValue(\u0002.GetTargetSettings());
			Guid guid = new Guid(stringValue);
			if (guid != Guid.Empty)
			{
				addressCalculator = APEnvironmentFacade.Instance.TryCreateAddressCalculator(guid);
			}
			if (addressCalculator == null)
			{
				addressCalculator = new AddressCalculator();
			}
			bool flag;
			IDataLocation dataLocation = addressCalculator.CalculateAddress(out \u0003, out flag, out \u0004, \u0005, \u0002, \u0006, \u0007);
			if (!flag)
			{
				dataLocation = addressCalculator2.CalculateAddress(out \u0003, out flag, out \u0004, \u0005, \u0002, \u0006, \u0007);
			}
			bool flag2 = true;
			if (dataLocation == null)
			{
				dataLocation = global::\u0019.\u0003.\u0001(CompilerServicesInternal.InvalidRefId, CompilerServicesInternal.InvalidSignatureOffset);
				if (!\u0004)
				{
					flag2 = false;
				}
			}
			if (flag2)
			{
				\u0002.DirvarLocationTable.AddLocationInfo(\u0006, \u0007, dataLocation, \u0003, \u0004);
			}
			return dataLocation;
		}

		// Token: 0x06002E1C RID: 11804 RVA: 0x000A9A18 File Offset: 0x000A7C18
		private static IVariable[] \u0001(_ISignature \u0002)
		{
			IVariable[] allInputs = \u0002.AllInputs;
			IVariable[] array = new IVariable[allInputs.Length];
			IVariable variable = \u0002[IdentifierConstants.InstancePointer];
			if (variable == null)
			{
				return allInputs;
			}
			array[0] = variable;
			int num = 0;
			int i = 1;
			while (i < array.Length)
			{
				if (allInputs[num] == variable)
				{
					num++;
				}
				array[i] = allInputs[num];
				i++;
				num++;
			}
			return array;
		}

		// Token: 0x06002E1D RID: 11805 RVA: 0x000A9A74 File Offset: 0x000A7C74
		internal static int \u0001(IVariable \u0002, _ISignature \u0003, IScope \u0004, out bool \u0005)
		{
			_IType itype = \u0002.CompiledType as _IType;
			ErrorVisitor errorVisitor = new ErrorVisitor();
			global::\u0002.\u000E.\u0001(itype, \u0004, errorVisitor);
			foreach (_ICompilerMessage message in errorVisitor.MessageList)
			{
				\u0003.AddMessage(message);
			}
			return itype.SizeChecked(\u0004, out \u0005);
		}

		// Token: 0x06002E1E RID: 11806 RVA: 0x000A9AE4 File Offset: 0x000A7CE4
		internal static bool \u0001(IDataManager3 \u0002, _ICompileContext \u0003, _ICompileContext \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			bool result = true;
			_IDataManager idataManager = \u0002 as _IDataManager;
			int stackAlignment = idataManager.StackAlignment;
			int u = Locator.\u0001(\u0005, idataManager);
			int u2 = Locator.\u0001(\u0003, \u0005, idataManager);
			VarFlag u000E;
			DataLocationFlag u000F;
			if (\u0005.POUType == Operator.Type)
			{
				u000E = VarFlag.RelativeInstance;
				u000F = DataLocationFlag.RelativeInstance;
			}
			else
			{
				u000E = VarFlag.RelativeStack;
				u000F = DataLocationFlag.RelativeStack;
			}
			IScope5 u3 = global::\u0007.\u0005.\u0001(\u0003, Helper.InvalidId, false);
			bool u4 = false;
			Operator poutype = \u0005.POUType;
			if (poutype <= Operator.Program)
			{
				if (poutype != Operator.Function)
				{
					if (poutype != Operator.FunctionBlock)
					{
						if (poutype != Operator.Program)
						{
							return result;
						}
						if (\u0005.HasAttribute("subsequent"))
						{
							u4 = true;
							if (\u0005.Temps.Length != 0)
							{
								string u5 = global::\u0003.\u0006.\u0001(MessageId.Err_NoVarTempInSubsequentPrograms, Array.Empty<object>());
								\u0005.AddError(global::\u0019.\u0003.\u0001(\u0005.Temps[0].SourcePosition, u5, Severity.Error, MessageId.Err_NoVarTempInSubsequentPrograms));
							}
						}
					}
					else
					{
						bool flag = false;
						if (\u0006 != null && \u0005.GetSizeOfMemoryReserve() == \u0006.GetSizeOfMemoryReserve())
						{
							flag = Locator.\u0001(\u0002 as _IDataManager, \u0003, \u0004, \u0005, \u0006);
						}
						if (!flag)
						{
							if (!Locator.\u0001(\u0002, \u0003, \u0005, \u0006))
							{
								result = false;
							}
							\u0005.SetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy, true);
							return result;
						}
						return result;
					}
				}
			}
			else if (poutype != Operator.Type)
			{
				if (poutype != Operator.VarGlobal)
				{
					if (poutype != Operator.Method)
					{
						return result;
					}
				}
				else
				{
					if (!\u0005.HasAttribute("subsequent"))
					{
						return result;
					}
					u4 = true;
				}
			}
			else
			{
				\u0005.SetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy, true);
			}
			Locator.\u0001(\u0003, \u0005, \u0006, u3, u, u2, u4, u000E, u000F, stackAlignment);
			return result;
		}

		// Token: 0x06002E1F RID: 11807 RVA: 0x000A9C70 File Offset: 0x000A7E70
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, IScope5 \u0005, int \u0006, int \u0007, bool \u0008, VarFlag \u000E, DataLocationFlag \u000F, int \u0010)
		{
			if (\u0003.POUType == Operator.Type && !\u0003.GetFlag(SignatureFlag.Structure))
			{
				return;
			}
			if (\u0003.GetFlag(SignatureFlag.Union))
			{
				Locator.\u0001(\u0003, \u0005, \u0007);
				return;
			}
			_ISignature isignature = \u0005[\u0003.BaseSignatureId] as _ISignature;
			if (\u0003.BaseSignatureId != Helper.InvalidId)
			{
				Debug.\u0001(isignature != null);
			}
			Locator.\u0001 u = new Locator.\u0001
			{
				Offset = 0,
				\u0002 = -1,
				\u0001 = 0U,
				\u0003 = 1
			};
			if (isignature != null)
			{
				u.Offset = isignature.Size;
				int num = Locator.\u0001(isignature, \u0006, \u0005);
				if (num > u.\u0003)
				{
					u.\u0003 = num;
				}
			}
			if (u.\u0003 > \u0007)
			{
				u.\u0003 = \u0007;
			}
			\u0003.Size = 0;
			IVariable[] array = null;
			int iIndexInput = -1;
			int num2 = -1;
			bool u2 = Locator.\u0001(\u0002, \u0003, ref array, ref num2, ref iIndexInput);
			bool u3 = false;
			bool flag = false;
			int num3 = Locator.\u0001(array);
			for (int i = 0; i < array.Length; i++)
			{
				_IVariable ivariable = array[i] as _IVariable;
				if (!Locator.\u0001(\u0003, ivariable, \u0008))
				{
					IVariable variable = null;
					if (\u0004 != null)
					{
						variable = \u0004[ivariable.Id];
					}
					_IVariable ivariable2 = variable as _IVariable;
					if (ivariable2 != null)
					{
						ivariable2.SetFlag(VarFlag.OnlChangeNoExit, true);
					}
					u.\u0001();
					int num4 = -1;
					bool flag2;
					int num5 = Locator.\u0001(ivariable, \u0003, \u0005, out flag2);
					if (!flag2)
					{
						flag = true;
					}
					if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE) && ivariable.GetFlag(VarFlag.Input) && (\u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method))
					{
						ICodegenerator5 codegenerator = \u0002.Codegenerator as ICodegenerator5;
						if (codegenerator != null)
						{
							int externalInputSize = codegenerator.GetExternalInputSize(iIndexInput, ivariable);
							if (externalInputSize >= 0)
							{
								num5 = externalInputSize;
							}
						}
						ICPPCompatibleCodegenerator icppcompatibleCodegenerator = \u0002.Codegenerator as ICPPCompatibleCodegenerator;
						if (icppcompatibleCodegenerator != null)
						{
							num4 = icppcompatibleCodegenerator.GetExternalInputGranularity(ivariable.CompiledType);
						}
					}
					if (ivariable.Address == null || !Locator.\u0001(\u0002, \u0003, ivariable))
					{
						if (ivariable.CompiledType.Class == TypeClass.Bit)
						{
							Locator.\u0001(\u0003, ivariable, u, ref u3);
						}
						else
						{
							u.\u0003();
							u.\u0001(Locator.\u0001(ivariable, \u0006, \u0005), \u0007);
							if (num4 > 0)
							{
								u.\u0006 = num4;
							}
							u.\u0002();
							if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
							{
								u.\u0005 = int.Parse(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET));
								u3 = true;
							}
							ivariable.SetFlag(\u000E, true);
							ivariable.DataLocation = global::\u0019.\u0003.\u0001(u.\u0005, \u000F);
							u.\u0005 += num5;
							if (i != num2)
							{
								if (i != num3)
								{
									goto IL_29D;
								}
							}
							while (u.\u0005 % \u0010 != 0)
							{
								u.\u0005++;
							}
							IL_29D:
							if (i == num2)
							{
								\u0003.CalleeSize = u.\u0005;
								u.\u0005 = 0;
							}
							u.Offset = u.\u0005;
						}
					}
				}
			}
			foreach (_IVariable ivariable3 in array)
			{
				if (ivariable3.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
				{
					Locator.\u0001(\u0002, \u0003, ivariable3, \u0005 as _IScope);
				}
			}
			u.\u0001(u3);
			int num6 = u.Offset;
			u.\u0003 = Locator.\u0001(\u0002, \u0003, u.\u0003, num6, \u0010);
			while (num6 % u.\u0003 != 0)
			{
				num6++;
			}
			\u0003.Size = num6;
			Locator.\u0001(\u0003, array, num2, u2, \u000F);
			if (!flag)
			{
				\u0003.SetFlag(SignatureFlag.Located, true);
			}
		}

		// Token: 0x06002E20 RID: 11808 RVA: 0x000A9FF0 File Offset: 0x000A81F0
		private static int \u0001(IVariable[] \u0002)
		{
			int result = -1;
			int num = 0;
			while (num < \u0002.Length && \u0002[num].HasAttribute("added-after-compile"))
			{
				result = num;
				num++;
			}
			return result;
		}

		// Token: 0x06002E21 RID: 11809 RVA: 0x000AA020 File Offset: 0x000A8220
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, ref IVariable[] \u0004, ref int \u0005, ref int \u0006)
		{
			bool result = false;
			if (\u0003.POUType == Operator.Program)
			{
				result = Helper.\u0001(CodegeneratorProperties.PositiveStackGrow, \u0002.Codegenerator);
				if (\u0003.HasAttribute("subsequent"))
				{
					\u0004 = \u0003.All;
				}
				else
				{
					\u0004 = \u0003.Temps;
					\u0005 = \u0004.Length - 1;
				}
			}
			else if (\u0003.POUType == Operator.Function || \u0003.POUType == Operator.Method)
			{
				result = Helper.\u0001(CodegeneratorProperties.PositiveStackGrow, \u0002.Codegenerator);
				\u0004 = Locator.\u0001(\u0003, out \u0006, out \u0005);
			}
			else
			{
				\u0004 = \u0003.All;
			}
			return result;
		}

		// Token: 0x06002E22 RID: 11810 RVA: 0x000AA0A8 File Offset: 0x000A82A8
		internal static bool \u0001(_IDataManager \u0002, _ICompileContext \u0003, _ICompileContext \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			if (\u0006 == null)
			{
				return false;
			}
			if (\u0005.HasAttribute("subsequent"))
			{
				return false;
			}
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, Helper.InvalidId, false);
			if (\u0005.BaseSignatureId != Helper.InvalidId)
			{
				_ISignature isignature = scope[\u0005.BaseSignatureId] as _ISignature;
				if (isignature == null || isignature.GetFlagInternal(SignatureFlagInternal.ForceOnlineChangeCopy))
				{
					return false;
				}
			}
			bool flag = true;
			Locator.\u0001 u = new Locator.\u0001
			{
				Offset = \u0006.HighestUsedOffset,
				\u0002 = -1,
				\u0001 = 0U,
				\u0003 = -1
			};
			bool flag2 = false;
			bool bSetTrue = false;
			int u2 = Locator.\u0001(\u0005, \u0002);
			int u3 = Locator.\u0001(\u0003, \u0005, \u0002);
			foreach (_IVariable ivariable in \u0005.AllVariables)
			{
				if (!Locator.\u0001(\u0005, ivariable, false))
				{
					_IVariable ivariable2 = \u0006[ivariable.Id] as _IVariable;
					bool flag3 = ivariable2 == null;
					if (!flag3 && global::\u0016.\u0011.\u0001(ivariable, ivariable2, \u0003, \u0004))
					{
						flag = false;
						break;
					}
					if (flag3)
					{
						if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
						{
							flag = false;
							break;
						}
						u.\u0001();
						bSetTrue = true;
						bool flag4;
						int num = Locator.\u0001(ivariable, \u0005, scope, out flag4);
						if (!flag4)
						{
							flag = false;
						}
						if (ivariable.Address != null && Locator.\u0001(\u0003, \u0005, ivariable))
						{
							continue;
						}
						if (ivariable.CompiledType.Class == TypeClass.Bit)
						{
							Locator.\u0001(\u0005, ivariable, u, ref flag2);
							continue;
						}
						u.\u0003();
						u.\u0001(Locator.\u0001(ivariable, u2, scope), u3);
						u.\u0002();
						if (u.\u0005 + num <= \u0006.Size)
						{
							ivariable.DataLocation = global::\u0019.\u0003.\u0001(u.\u0005, DataLocationFlag.RelativeInstance);
							u.\u0005 += num;
							u.Offset = u.\u0005;
							if (!ivariable.GetFlag(VarFlag.Temp))
							{
								ivariable.SetFlag(VarFlag.OnlChangeInit, true);
							}
						}
						else
						{
							flag = false;
						}
					}
					else
					{
						ivariable.DataLocation = ivariable2.DataLocation;
						if (ivariable.Address != null && !ivariable.Address.Incomplete && ivariable2.GetFlag(VarFlag.Absolut))
						{
							ivariable.SetFlag(VarFlag.Absolut, true);
							ivariable2.SetFlag(VarFlag.OnlChangeNoExit, true);
							continue;
						}
						ivariable2.SetFlag(VarFlag.OnlChangeNoExit, true);
					}
					if (ivariable.GetFlag(VarFlag.Constant) && ivariable.GetFlag(VarFlag.OnlChangeInit))
					{
						bSetTrue = true;
					}
					ivariable.SetFlag(VarFlag.RelativeInstance, true);
					if (!flag)
					{
						break;
					}
				}
			}
			if (flag)
			{
				foreach (_IVariable ivariable3 in \u0005.AllVariables)
				{
					if (ivariable3.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
					{
						Locator.\u0001(\u0003, \u0005, ivariable3, scope as _IScope);
					}
				}
				\u0005.SetFlag(SignatureFlag.Located, true);
				\u0005.SetFlagInternal(SignatureFlagInternal.OnlineChangePartialInit, bSetTrue);
				\u0005.Size = \u0006.Size;
				\u0005.HighestUsedOffset = u.Offset;
			}
			else
			{
				\u0005.SetFlag(SignatureFlag.Located, false);
				foreach (_IVariable ivariable4 in \u0005.AllVariables)
				{
					ivariable4.SetFlag(VarFlag.OnlChangeInit | VarFlag.OnlChangeNoExit, false);
				}
			}
			return flag;
		}

		// Token: 0x06002E23 RID: 11811 RVA: 0x000AA464 File Offset: 0x000A8664
		internal static bool \u0001(IDataManager3 \u0002, _ICompileContext \u0003, _ISignature \u0004, _ISignature \u0005)
		{
			_IDataManager idataManager = \u0002 as _IDataManager;
			int num = Locator.\u0001(\u0004, idataManager);
			int num2 = Locator.\u0001(\u0003, \u0004, idataManager);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0003, Helper.InvalidId, false);
			_ISignature isignature = scope[\u0004.BaseSignatureId] as _ISignature;
			if (\u0004.BaseSignatureId != Helper.InvalidId)
			{
				Debug.\u0001(isignature != null);
			}
			Locator.\u0001 u = new Locator.\u0001
			{
				Offset = \u0003.PointerSize,
				\u0002 = -1,
				\u0001 = 0U,
				\u0003 = -1
			};
			if (isignature != null)
			{
				u.Offset = isignature.Size;
			}
			u.\u0003 = Locator.\u0001(\u0003, \u0004, isignature, num, scope, u.\u0003, num2);
			\u0004.Size = 0;
			IList<_IVariable> allVariables = \u0004.AllVariables;
			bool u2 = false;
			bool flag = false;
			for (int i = 0; i < allVariables.Count; i++)
			{
				_IVariable ivariable = allVariables[i];
				if (!Locator.\u0001(\u0004, ivariable, false))
				{
					IVariable variable = null;
					if (\u0005 != null)
					{
						variable = \u0005[ivariable.Id];
					}
					_IVariable ivariable2 = variable as _IVariable;
					if (ivariable2 != null)
					{
						ivariable2.SetFlag(VarFlag.OnlChangeNoExit, true);
					}
					u.\u0001();
					bool flag2;
					int num3 = Locator.\u0001(ivariable, \u0004, scope, out flag2);
					if (!flag2)
					{
						\u0004.AddMessage(ivariable._SourcePosition, Severity.Error, MessageId.Err_VarTooBig, new object[]
						{
							ivariable.VersionedName,
							2147483647U,
							int.MaxValue
						});
						flag = true;
					}
					if (ivariable.Address == null || !Locator.\u0001(\u0003, \u0004, ivariable))
					{
						if (ivariable.CompiledType.Class == TypeClass.Bit)
						{
							Locator.\u0001(\u0004, ivariable, u, ref u2);
						}
						else
						{
							u.\u0003();
							u.\u0001(Locator.\u0001(ivariable, num, scope), num2);
							u.\u0002();
							if (ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
							{
								u.\u0005 = int.Parse(ivariable.GetAttributeValue(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET));
								u2 = true;
							}
							ivariable.SetFlag(VarFlag.RelativeInstance, true);
							ivariable.DataLocation = global::\u0019.\u0003.\u0001(u.\u0005, DataLocationFlag.RelativeInstance);
							u.\u0005 += num3;
							u.Offset = u.\u0005;
						}
					}
				}
			}
			foreach (_IVariable ivariable3 in allVariables)
			{
				if (ivariable3.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
				{
					Locator.\u0001(\u0003, \u0004, ivariable3, scope as _IScope);
				}
			}
			u.\u0001(u2);
			int num4 = u.Offset;
			if (global::\u0012.\u0014.\u0001(\u0004))
			{
				num4 = Locator.\u0001(\u0004, num4);
			}
			u.\u0003 = Locator.\u0001(\u0003, \u0004, u.\u0003, num4, idataManager.StackAlignment);
			while (num4 % u.\u0003 != 0)
			{
				num4++;
			}
			\u0004.Size = num4;
			\u0004.HighestUsedOffset = u.Offset;
			if (!flag)
			{
				\u0004.SetFlag(SignatureFlag.Located, true);
			}
			return !flag;
		}

		// Token: 0x06002E24 RID: 11812 RVA: 0x000AA77C File Offset: 0x000A897C
		private static void \u0001(_ISignature \u0002, _IVariable \u0003, Locator.\u0001 \u0004, ref bool \u0005)
		{
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET))
			{
				\u0004.\u0005 = int.Parse(\u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_RELATIVE_OFFSET));
				\u0004.\u0002 = -1;
				\u0004.\u0001 = 0U;
				\u0005 = true;
			}
			\u0004.\u0004();
			int u = \u0004.\u0002;
			uint u2 = \u0004.\u0001;
			\u0004.\u0001 = u2 + 1U;
			IDataLocation2 dataLocation = global::\u0019.\u0003.\u0001(u, (byte)u2);
			\u0003.DataLocation = dataLocation;
			\u0004.Offset = \u0004.\u0005;
			if (\u0002.POUType == Operator.FunctionBlock || \u0002.POUType == Operator.Type)
			{
				\u0003.SetFlag(VarFlag.RelativeInstance, true);
				return;
			}
			\u0003.SetFlag(VarFlag.RelativeStack, true);
		}

		// Token: 0x06002E25 RID: 11813 RVA: 0x000AA824 File Offset: 0x000A8A24
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, _IVariable \u0004)
		{
			IMessage cm;
			bool flag;
			IDataLocation dataLocation = Locator.\u0001(\u0002, out cm, out flag, \u0004._SourcePosition, \u0004.Address, \u0004);
			\u0004.DataLocation = dataLocation;
			if (!\u0004.Address.Incomplete && dataLocation != null)
			{
				if (flag)
				{
					\u0003.AddError(cm);
				}
				\u0004.SetFlag(VarFlag.Absolut, true);
				return true;
			}
			return false;
		}

		// Token: 0x06002E26 RID: 11814 RVA: 0x000AA880 File Offset: 0x000A8A80
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003, bool \u0004)
		{
			if (\u0003.GetFlag(VarFlag.External))
			{
				return true;
			}
			if (\u0003.GetFlag(VarFlag.Absolut) && !\u0004 && \u0003.Address == null)
			{
				return true;
			}
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
			{
				return true;
			}
			if (\u0003.GetFlag(VarFlag.ReplacedConstant) || (\u0003.IsProperty && !\u0003.IsPropertyMonitor))
			{
				\u0003.DataLocation = global::\u0019.\u0003.\u0001(CompilerServicesInternal.InvalidRefId, CompilerServicesInternal.InvalidSignatureOffset);
				if (\u0002.POUType == Operator.FunctionBlock || \u0002.POUType == Operator.Type)
				{
					\u0003.SetFlag(VarFlag.RelativeInstance, true);
				}
				else
				{
					\u0003.SetFlag(VarFlag.RelativeStack, true);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06002E27 RID: 11815 RVA: 0x000AA928 File Offset: 0x000A8B28
		private static int \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004, int \u0005, IScope5 \u0006, int \u0007, int \u0008)
		{
			if (\u0004 != null)
			{
				int num = Locator.\u0001(\u0004, \u0005, \u0006);
				if (num > \u0007)
				{
					\u0007 = num;
				}
			}
			else
			{
				\u0007 = \u0002.PointerSize;
			}
			if (\u0003.HasMemoryReserve)
			{
				\u0007 = 8;
			}
			if (\u0007 > \u0008)
			{
				\u0007 = \u0008;
			}
			return \u0007;
		}

		// Token: 0x06002E28 RID: 11816 RVA: 0x000AA96C File Offset: 0x000A8B6C
		private static int \u0001(_ISignature \u0002, int \u0003)
		{
			int num = 0;
			if (\u0002.GetAttributeIntValue(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS, ref num))
			{
				\u0003 += num;
			}
			return \u0003;
		}

		// Token: 0x06002E29 RID: 11817 RVA: 0x000AA990 File Offset: 0x000A8B90
		private static int \u0001(_ICompileContext \u0002, _ISignature \u0003, int \u0004, int \u0005, int \u0006)
		{
			if (\u0003.POUType == Operator.Method || \u0003.POUType == Operator.Function)
			{
				\u0004 = \u0006;
			}
			int minGranularity = \u0002.DataManager._MemorySettings.MinGranularity;
			if (\u0005 >= minGranularity && \u0004 < minGranularity && minGranularity > 0)
			{
				\u0004 = minGranularity;
			}
			return \u0004;
		}

		// Token: 0x06002E2A RID: 11818 RVA: 0x000AA9D8 File Offset: 0x000A8BD8
		private static void \u0001(_ISignature \u0002, IVariable[] \u0003, int \u0004, bool \u0005, DataLocationFlag \u0006)
		{
			if (!\u0005)
			{
				for (int i = \u0004; i >= 0; i--)
				{
					_IVariable ivariable = \u0003[i] as _IVariable;
					if (ivariable.DataLocation.IsRelativ && !ivariable.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
					{
						ivariable.DataLocation = global::\u0019.\u0003.\u0001(ivariable.DataLocation.Offset - \u0002.CalleeSize, \u0006);
					}
				}
				return;
			}
			for (int j = \u0003.Length - 1; j > \u0004; j--)
			{
				_IVariable ivariable2 = \u0003[j] as _IVariable;
				if (!ivariable2.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
				{
					ivariable2.DataLocation = global::\u0019.\u0003.\u0001(ivariable2.DataLocation.Offset - \u0002.Size);
				}
			}
		}

		// Token: 0x06002E2B RID: 11819 RVA: 0x000AAA7C File Offset: 0x000A8C7C
		private static int \u0001(_ICompileContext \u0002, _ISignature \u0003, _IDataManager \u0004)
		{
			int num = \u0004.PackMode;
			int packMode = \u0003.PackMode;
			if (packMode != -1)
			{
				num = packMode;
			}
			if (num != 8 && \u0003.POUType == Operator.Method && (\u0003.Name.StartsWith("__GET") || \u0003.Name.StartsWith("__SET")))
			{
				ISignature signature = global::\u0007.\u0005.\u0001(\u0002)[\u0003.ParentSignatureId];
				if (signature != null)
				{
					string stName = \u0003.Name.Substring(5);
					IVariable variable = signature[stName];
					if (variable != null && variable.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_CALL)
					{
						num = 8;
					}
				}
			}
			return num;
		}

		// Token: 0x06002E2C RID: 11820 RVA: 0x000AAB1C File Offset: 0x000A8D1C
		private static int \u0001(_ISignature \u0002, _IDataManager \u0003)
		{
			int num = \u0003.MinSize;
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE))
			{
				string attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_MINIMAL_INPUT_SIZE);
				try
				{
					num = int.Parse(attributeValue);
				}
				catch
				{
					num = 0;
				}
				if (num != 1 && num != 2 && num != 4 && num != 8)
				{
					num = 0;
				}
			}
			return num;
		}

		// Token: 0x06002E2D RID: 11821 RVA: 0x000AAB7C File Offset: 0x000A8D7C
		private static bool \u0001(_ICompileContext \u0002, _ISignature \u0003, _IVariable \u0004, _IScope \u0005)
		{
			VarFlag vf = VarFlag.None;
			IDataLocation dataLocation = null;
			string attributeValue = \u0004.GetAttributeValue(CompileAttributes.ATTRIBUTE_USELOCATION);
			if (attributeValue != string.Empty && attributeValue.ToUpperInvariant() != \u0004.VersionedName.ToUpperInvariant())
			{
				if (\u0004.HasFlag(VarFlag.AllocateInInstance))
				{
					_ISignature isignature = \u0002.GetSignatureById(\u0003.ParentSignatureId) as _ISignature;
					dataLocation = Helper.\u0001(attributeValue, isignature.AllVariables).DataLocation;
					vf = VarFlag.RelativeInstance;
				}
				else if (attributeValue.Contains("."))
				{
					ICompoAccessExpression compoAccessExpression = new global::\u0011.\u0006(attributeValue).\u0002() as ICompoAccessExpression;
					IVariable variable = Helper.\u0001(compoAccessExpression.Left.ToString(), \u0003.AllVariables);
					if (variable != null && variable.Type.Class == TypeClass.Userdef)
					{
						_ISignature isignature2 = (variable.Type as _IUserdefType).GetSignature(\u0005) as _ISignature;
						if (isignature2 != null)
						{
							IVariable variable2 = Helper.\u0001(compoAccessExpression.Right.ToString(), isignature2.AllVariables);
							if (variable2 != null)
							{
								dataLocation = global::\u0019.\u0003.\u0001(variable.DataLocation.Offset + variable2.DataLocation.Offset);
								if (variable.HasFlag(VarFlag.RelativeInstance))
								{
									vf = VarFlag.RelativeInstance;
								}
								else
								{
									vf = VarFlag.RelativeStack;
								}
							}
						}
					}
				}
				else
				{
					dataLocation = Helper.\u0001(attributeValue, \u0003.AllVariables).DataLocation;
					if (\u0003.POUType == Operator.FunctionBlock || \u0003.POUType == Operator.Type)
					{
						vf = VarFlag.RelativeInstance;
					}
					else
					{
						vf = VarFlag.RelativeStack;
					}
				}
				if (dataLocation != null)
				{
					\u0004.DataLocation = dataLocation;
					\u0004.SetFlag(vf, true);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002E2E RID: 11822 RVA: 0x000AAD24 File Offset: 0x000A8F24
		private static IVariable[] \u0001(_ISignature \u0002, out int \u0003, out int \u0004)
		{
			\u0003 = -1;
			\u0004 = -1;
			IVariable[] locals = \u0002.Locals;
			IVariable[] array = Locator.\u0001(\u0002);
			IVariable[] outputs = \u0002.Outputs;
			IVariable[] instanceLocals = \u0002.InstanceLocals;
			IVariable[] array2 = new IVariable[locals.Length + array.Length + outputs.Length + instanceLocals.Length];
			locals.CopyTo(array2, 0);
			int num = locals.Length;
			int num2 = num - 1;
			while (num2 >= 0 && (locals[num2].GetFlag(VarFlag.ReplacedConstant) || (locals[num2] as _IVariable).IsProperty || locals[num2].Address != null || locals[num2].CompiledType.Class == TypeClass.Bit))
			{
				num2--;
			}
			if (\u0002.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE))
			{
				outputs.CopyTo(array2, num);
				num += outputs.Length;
				\u0004 = num - 1;
				array.CopyTo(array2, num);
				\u0003 = num;
				num += array.Length;
			}
			else
			{
				\u0004 = num2;
				array.CopyTo(array2, num);
				num += array.Length;
				outputs.CopyTo(array2, num);
				num += outputs.Length;
			}
			instanceLocals.CopyTo(array2, num);
			num += instanceLocals.Length;
			return array2;
		}

		// Token: 0x06002E2F RID: 11823 RVA: 0x000AAE3C File Offset: 0x000A903C
		private static void \u0001(_ISignature \u0002, IScope5 \u0003, int \u0004)
		{
			bool flag = false;
			int num = 0;
			IEnumerable<IVariable> allVariables = \u0002.AllVariables;
			int num2 = 0;
			_ISignature isignature = \u0003[\u0002.BaseSignatureId] as _ISignature;
			if (\u0002.BaseSignatureId != Helper.InvalidId)
			{
				Debug.\u0001(isignature != null);
				if (isignature != null)
				{
					num2 = isignature.Size;
					num = Locator.\u0001(Locator.\u0001(isignature, num, \u0003), \u0004);
				}
			}
			foreach (IVariable variable in allVariables)
			{
				_IVariable ivariable = (_IVariable)variable;
				int num3 = Locator.\u0001(Locator.\u0001(ivariable, 0, \u0003), \u0004);
				if (num3 <= 0)
				{
					num3 = 8;
				}
				num = Math.Max(num3, num);
				bool flag2;
				num2 = Math.Max(num2, Locator.\u0001(ivariable, \u0002, \u0003, out flag2));
				if (!flag2)
				{
					flag = true;
				}
				ivariable.DataLocation = global::\u0019.\u0003.\u0001(0);
				ivariable.SetFlag(VarFlag.RelativeInstance, true);
			}
			if (0 < num)
			{
				while (num2 % num != 0)
				{
					num2++;
				}
			}
			if (!flag)
			{
				\u0002.SetFlag(SignatureFlag.Located, true);
			}
			\u0002.Size = num2;
		}

		// Token: 0x06002E30 RID: 11824 RVA: 0x000AAF50 File Offset: 0x000A9150
		private static int \u0001(int \u0002, int \u0003)
		{
			if (\u0002 <= \u0003)
			{
				return \u0002;
			}
			return \u0003;
		}

		// Token: 0x06002E31 RID: 11825 RVA: 0x000AAF5C File Offset: 0x000A915C
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			return Locator.\u0001(\u0002, \u0003, \u0004, \u0005, false);
		}

		// Token: 0x06002E32 RID: 11826 RVA: 0x000AAF68 File Offset: 0x000A9168
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ICompileContext \u0005, bool \u0006)
		{
			bool result = global::\u0016.\u0011.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006);
			if (\u0002.GetFlag(VarFlag.OnlChangeCopy))
			{
				\u0004.ContainsCopyCode = true;
			}
			return result;
		}

		// Token: 0x06002E33 RID: 11827 RVA: 0x000AAF8C File Offset: 0x000A918C
		private static void \u0001(_ISignature \u0002, _IVariable \u0003, _IVariable \u0004, _ICompileContext \u0005, _ICompileContext \u0006)
		{
			bool flag = \u0003.CompiledType.Size(global::\u0007.\u0005.\u0001(\u0005)) <= \u0004.CompiledType.Size(global::\u0007.\u0005.\u0001(\u0006));
			if (!flag)
			{
				string text = string.Format("Memory Corrupted! Type change not detected in {0} {1}.{2}", \u0002.LibraryPath, \u0002.OrgName, \u0003.OrgName);
				Debug.\u0001(flag, text);
				\u0002.AddMessageString(\u0003._SourcePosition, Severity.FatalError, text, Array.Empty<object>());
			}
		}

		// Token: 0x06002E34 RID: 11828 RVA: 0x000AAFFC File Offset: 0x000A91FC
		private static bool \u0001(_IType \u0002)
		{
			if (\u0002.Class == TypeClass.__Vector)
			{
				return true;
			}
			_IArrayType iarrayType = \u0002 as _IArrayType;
			return iarrayType != null && Locator.\u0001(iarrayType._Base);
		}

		// Token: 0x06002E35 RID: 11829 RVA: 0x000AB02C File Offset: 0x000A922C
		internal static void \u0001(_ISignature \u0002, _IVariable \u0003, _ISignature \u0004, _ICompileContext \u0005, _ICompileContext \u0006, out bool \u0007)
		{
			_IDataManager dataManager = \u0005.DataManager;
			int packMode = dataManager.PackMode;
			int minSize = dataManager.MinSize;
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0005, Helper.InvalidId, false);
			\u0007 = false;
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_USELOCATION))
			{
				string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_USELOCATION);
				if (attributeValue != string.Empty && attributeValue.ToUpperInvariant() != \u0003.VersionedName.ToUpperInvariant())
				{
					IVariable variable = Helper.\u0001(attributeValue, \u0002.AllVariables);
					if (variable != null)
					{
						\u0003.DataLocation = variable.DataLocation;
						return;
					}
				}
			}
			if (\u0003.GetFlag(VarFlag.External))
			{
				return;
			}
			if (\u0003.GetFlag(VarFlag.Enum) && \u0003.GetFlag(VarFlag.ReplacedConstant))
			{
				\u0003.DataLocation = global::\u0019.\u0003.\u0001(CompilerServicesInternal.InvalidRefId, CompilerServicesInternal.InvalidSignatureOffset);
				return;
			}
			if (!\u0003.GetFlag(VarFlag.Absolut))
			{
				return;
			}
			if (\u0003.GetFlag(VarFlag.ReplacedConstant) || (\u0003.IsProperty && !\u0003.IsPropertyMonitor))
			{
				\u0003.DataLocation = global::\u0019.\u0003.\u0001(CompilerServicesInternal.InvalidRefId, CompilerServicesInternal.InvalidSignatureOffset);
				return;
			}
			_IVariable ivariable = null;
			if (\u0004 != null)
			{
				ivariable = (\u0004[\u0003.Id] as _IVariable);
			}
			ushort num = 0;
			int num2 = 0;
			if (\u0003.Address != null)
			{
				IMessage message;
				bool flag;
				\u0003.DataLocation = Locator.\u0001(\u0005, out message, out flag, \u0003._SourcePosition, \u0003.Address, \u0003);
				if (!\u0003.Address.Incomplete && \u0003.DataLocation != null)
				{
					if (message != null)
					{
						\u0002.AddError(message);
						\u0007 = (message.Severity == Severity.Error || message.Severity == Severity.FatalError);
					}
					\u0003.SetFlag(VarFlag.Absolut, true);
					if (Locator.\u0001(\u0003, ivariable, \u0005, \u0006) && ivariable != null)
					{
						\u0003.SetFlag(VarFlag.LocationChanged, true);
					}
					Locator.\u0001(\u0002, \u0003, minSize, scope);
					return;
				}
			}
			DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Data;
			bool flag2 = \u0003.Initial == null;
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST) && global::\u0016.\u0004.ConstantsInOwnSegment.GetBoolValue(\u0005.GetTargetSettings()))
			{
				dataSegmentFlags = DataSegmentFlags.Constant;
			}
			else if (\u0003.GetFlag(VarFlag.Constant) && TypeTable.IsBlock(\u0003.Type.Class) && !Locator.\u0001(\u0003._Type) && !flag2 && global::\u0016.\u0004.ConstantsInOwnSegment.GetBoolValue(\u0005.GetTargetSettings()))
			{
				dataSegmentFlags = DataSegmentFlags.Constant;
				\u0003.AddAttribute(CompileAttributes.ATTRIBUTE_BLOBINITCONST, null);
			}
			if (!Locator.\u0001(\u0003, ivariable, \u0005, \u0006))
			{
				\u0003.DataLocation = ivariable.DataLocation;
				if (!\u0005.InFastOnlineChange)
				{
					Locator.\u0001(\u0002, \u0003, ivariable, \u0005, \u0006);
					bool flag3 = MemoryCompiler.\u0002(dataManager, \u0003.DataLocation.Area, \u0003.DataLocation.Offset, \u0003._Type.Size(scope), DataSegmentFlags.None);
					Debug.\u0001(flag3, "Memory Corrupted! Clean application necessary!");
					if (!flag3)
					{
						\u0002.AddMessageString(\u0003._SourcePosition, Severity.FatalError, "Memory Corrupted! Clean application necessary!", Array.Empty<object>());
						\u0007 = true;
					}
				}
				return;
			}
			if (ivariable != null)
			{
				\u0003.SetFlag(VarFlag.LocationChanged, true);
			}
			int num3 = Locator.\u0001(\u0003, minSize, scope);
			if (num3 > packMode)
			{
				num3 = packMode;
			}
			int num4 = 0;
			try
			{
				num4 = \u0003._Type.Size(scope);
			}
			catch (OverflowException)
			{
				\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_VarTooBig, new object[]
				{
					\u0003.VersionedName,
					num4,
					dataManager.DataSegmentSize
				});
			}
			if (num4 < 0)
			{
				\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_VarTooBig, new object[]
				{
					\u0003.VersionedName,
					(uint)num4,
					int.MaxValue
				});
				\u0007 = true;
			}
			else if (dataManager.DataSegmentSize != CompilerServicesInternal.NoSegmentation && num4 > dataManager.DataSegmentSize)
			{
				\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_VarTooBig, new object[]
				{
					\u0003.VersionedName,
					num4,
					dataManager.DataSegmentSize
				});
				\u0007 = true;
			}
			else
			{
				DataSegmentFlags dataSegmentFlags2 = DataSegmentFlags.None;
				uint num5 = Locator.\u0001(\u0002, \u0003);
				if (num5 != 4294967295U)
				{
					dataSegmentFlags2 = (DataSegmentFlags)num5;
				}
				Locator.\u0001(\u0002, \u0003, \u0005, scope, ref \u0007, ref dataSegmentFlags2);
				if (dataSegmentFlags2 == DataSegmentFlags.None)
				{
					dataSegmentFlags2 = dataSegmentFlags;
				}
				if (dataSegmentFlags2 != DataSegmentFlags.Data && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_OFFSET))
				{
					bool flag4;
					num2 = \u0084.\u0004.\u0001(\u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_OFFSET), out flag4);
					if (flag4)
					{
						num = MemoryCompiler.\u0001(dataManager, dataSegmentFlags2);
						if (num != CompilerServicesInternal.InvalidRefId && MemoryCompiler.\u0002(dataManager, num, num2, \u0003._Type.Size(scope), DataSegmentFlags.None))
						{
							\u0003.DataLocation = global::\u0019.\u0003.\u0001(num, num2);
							return;
						}
					}
				}
				if (\u0005.DSFCallback != null)
				{
					dataSegmentFlags2 = \u0005.DSFCallback.GetDataSegmentFlagForVariable(\u0003, \u0002, \u0005, dataSegmentFlags2);
				}
				if (!MemoryCompiler.\u0003(dataManager, ref num, ref num2, num3, num4, dataManager.DataSegmentSize, dataSegmentFlags2))
				{
					string empty = string.Empty;
					\u0007 = true;
					_IDataSegment preferredDataSegment = MemoryCompiler.GetPreferredDataSegment(dataManager, dataSegmentFlags2);
					if (preferredDataSegment != null)
					{
						if (dataSegmentFlags2 == DataSegmentFlags.Retain)
						{
							if (dataManager.IsRetainSupported())
							{
								\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfRetainMemoryWithWholeSize, new object[]
								{
									\u0003.OrgName,
									num4,
									preferredDataSegment.MaxContiguosMemory
								});
							}
							else
							{
								\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_RetainsNotSupported, Array.Empty<object>());
							}
						}
						else if ((dataSegmentFlags2 & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent)
						{
							if (\u0003.Name.Contains("__"))
							{
								if (\u0002.Messages.Select(new Func<IMessage, bool>(Locator.<>c.<>9.\u0001)).Count<bool>() == 0)
								{
									\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfPersistentMemoryImplicit, new object[]
									{
										empty
									});
								}
							}
							else
							{
								\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfPersistentMemoryExplicit, new object[]
								{
									\u0003.OrgName,
									num4,
									preferredDataSegment.MaxContiguosMemory,
									empty
								});
							}
						}
						else
						{
							\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfMemoryWithWholeSize, new object[]
							{
								\u0003.OrgName,
								num4,
								preferredDataSegment.MaxContiguosMemory
							});
						}
					}
					else if (dataSegmentFlags2 == DataSegmentFlags.Retain)
					{
						if (dataManager.IsRetainSupported())
						{
							\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfRetainMemory, new object[]
							{
								\u0003.OrgName,
								num4
							});
						}
						else
						{
							\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_RetainsNotSupported, Array.Empty<object>());
						}
					}
					else if ((dataSegmentFlags2 & DataSegmentFlags.Persistent) == DataSegmentFlags.Persistent)
					{
						if (\u0003.Name.Contains("__"))
						{
							if (\u0002.Messages.Select(new Func<IMessage, bool>(Locator.<>c.<>9.\u0002)).Count<bool>() == 0)
							{
								\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfPersistentMemoryImplicit, new object[]
								{
									empty
								});
							}
						}
						else
						{
							\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfPersistentMemoryExplicit, new object[]
							{
								\u0003.OrgName,
								num4,
								0,
								empty
							});
						}
					}
					else
					{
						\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_OutOfMemory, new object[]
						{
							\u0003.OrgName,
							num4
						});
					}
				}
			}
			\u0003.DataLocation = global::\u0019.\u0003.\u0001(num, num2);
		}

		// Token: 0x06002E36 RID: 11830 RVA: 0x000AB798 File Offset: 0x000A9998
		private static void \u0001(_ISignature \u0002, _IVariable \u0003, int \u0004, IScope5 \u0005)
		{
			int num = Locator.\u0001(\u0003, \u0004, \u0005);
			if (\u0003.DataLocation.Offset % num != 0)
			{
				\u0002.\u0001(\u0003.SourcePosition, Messages.\u0001(\u0003, MessageId.Wrn_GranularityMismatchForDirectVariable), MessageId.Wrn_GranularityMismatchForDirectVariable, new object[]
				{
					\u0003.OrgName,
					num,
					\u0003.Address
				});
			}
		}

		// Token: 0x06002E37 RID: 11831 RVA: 0x000AB7FC File Offset: 0x000A99FC
		private static void \u0001(_ISignature \u0002, _IVariable \u0003, _ICompileContext \u0004, IScope5 \u0005, ref bool \u0006, ref DataSegmentFlags \u0007)
		{
			bool flag = \u0003.GetFlag(VarFlag.Retain);
			if (\u0004.RetainInCycle)
			{
				if (flag)
				{
					flag = \u0002.HasAttribute("do_retain");
				}
				if (\u0003.GetFlag(VarFlag.Persistent) && \u0003.GetFlag(VarFlag.Retain))
				{
					flag = true;
				}
			}
			else
			{
				if (!flag && \u0003.Type is _IUserdefType)
				{
					ISignature signature = (\u0003.Type as _IUserdefType).GetSignature(\u0005);
					if (\u0002 != null && signature.GetFlag(SignatureFlag.ContainsRetain))
					{
						flag = true;
					}
				}
				if (!flag && \u0003.Type is _IArrayType)
				{
					ICompiledType compiledType = \u0084.\u0004.\u0001(\u0003.Type as _IArrayType);
					if (compiledType is _IUserdefType)
					{
						ISignature signature2 = (compiledType as _IUserdefType).GetSignature(\u0005);
						if (\u0002 != null && signature2.GetFlag(SignatureFlag.ContainsRetain))
						{
							flag = true;
						}
					}
				}
			}
			if (flag)
			{
				\u0007 |= DataSegmentFlags.Retain;
			}
			if (\u0003.GetFlag(VarFlag.Persistent))
			{
				\u0007 |= DataSegmentFlags.Persistent;
			}
			if (flag && global::\u0016.\u0004.NoBytesInRetain.GetBoolValue(\u0004.GetTargetSettings()) && Locator.\u0001(\u0003.CompiledType, 8, \u0005) == 1)
			{
				\u0002.AddMessage(\u0003._SourcePosition, Severity.Error, MessageId.Err_NoBytesInRetain, Array.Empty<object>());
				\u0006 = true;
			}
		}

		// Token: 0x06002E38 RID: 11832 RVA: 0x000AB940 File Offset: 0x000A9B40
		private static uint \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			uint result = uint.MaxValue;
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_LOCATION) || \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_LOCATION))
			{
				string attributeValue = \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_LOCATION);
				if (attributeValue == null)
				{
					attributeValue = \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_LOCATION);
				}
				bool flag;
				result = \u0084.\u0004.\u0001(attributeValue, out flag);
				if (!flag)
				{
					result = uint.MaxValue;
				}
			}
			return result;
		}

		// Token: 0x06002E39 RID: 11833 RVA: 0x000AB998 File Offset: 0x000A9B98
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005, out bool \u0006)
		{
			\u0006 = false;
			IScope5 u = global::\u0007.\u0005.\u0001(\u0004, Helper.InvalidId, false);
			_IDataManager dataManager = \u0004.DataManager;
			Operator poutype = \u0002.POUType;
			if (poutype <= Operator.Program)
			{
				if (poutype == Operator.Function)
				{
					goto IL_190;
				}
				if (poutype != Operator.FunctionBlock)
				{
					if (poutype != Operator.Program)
					{
						goto IL_1FF;
					}
				}
				else
				{
					_IVirtualFunctionTable ivirtualFunctionTable = \u0002.VirtualFunctionTable as _IVirtualFunctionTable;
					_IVirtualFunctionTable ivirtualFunctionTable2 = null;
					if (\u0003 != null)
					{
						ivirtualFunctionTable2 = (\u0003.VirtualFunctionTable as _IVirtualFunctionTable);
					}
					if (ivirtualFunctionTable2 != null && ivirtualFunctionTable2.IsEqual(ivirtualFunctionTable))
					{
						ivirtualFunctionTable.DataLocation = ivirtualFunctionTable2.DataLocation;
						MemoryCompiler.\u0002(dataManager, ivirtualFunctionTable.DataLocation.Area, ivirtualFunctionTable.DataLocation.Offset, ivirtualFunctionTable.Size, DataSegmentFlags.None);
					}
					else
					{
						ushort u2 = 0;
						int u3 = 0;
						if (ivirtualFunctionTable.Size > 0)
						{
							DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Data;
							if (\u0004.DSFCallback != null)
							{
								dataSegmentFlags = \u0004.DSFCallback.GetDataSegmentFlagForVirtualFunctionTable(\u0002, \u0004, dataSegmentFlags);
							}
							if (!MemoryCompiler.\u0003(dataManager, ref u2, ref u3, \u0004.PointerSize, ivirtualFunctionTable.Size, dataManager.DataSegmentSize, dataSegmentFlags))
							{
								\u0002.AddMessage(Severity.Error, MessageId.Err_OutOfMemory, new object[]
								{
									\u0002.Name,
									ivirtualFunctionTable.Size
								});
							}
						}
						ivirtualFunctionTable.DataLocation = global::\u0019.\u0003.\u0001(u2, u3);
					}
				}
			}
			else if (poutype <= Operator.VarGlobal)
			{
				if (poutype != Operator.Type)
				{
					if (poutype != Operator.VarGlobal)
					{
						goto IL_1FF;
					}
				}
				else if (!\u0002.HasAttribute("singleton"))
				{
					goto IL_1FF;
				}
			}
			else
			{
				if (poutype == Operator.Method)
				{
					goto IL_190;
				}
				if (poutype != Operator.Interface)
				{
					goto IL_1FF;
				}
				goto IL_1FF;
			}
			if (\u0002.HasAttribute("subsequent") && (\u0002.POUType == Operator.Program || \u0002.POUType == Operator.VarGlobal))
			{
				Locator.\u0001(\u0002, \u0003, \u0004, \u0005, u, dataManager);
				goto IL_1FF;
			}
			IL_190:
			bool flag = false;
			if (\u0003 != null && (\u0003.POUType != \u0002.POUType || \u0002.GetFlag(SignatureFlag.Persistent)))
			{
				\u0003 = null;
			}
			foreach (IVariable variable in \u0002.AllVariables)
			{
				_IVariable u4 = (_IVariable)variable;
				Locator.\u0001(\u0002, u4, \u0003, \u0004, \u0005, out flag);
				\u0006 = (\u0006 || flag);
			}
			IL_1FF:
			\u0002.SetFlag(SignatureFlag.Located, !\u0006);
		}

		// Token: 0x06002E3A RID: 11834 RVA: 0x000ABBC8 File Offset: 0x000A9DC8
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005, IScope5 \u0006, _IDataManager \u0007)
		{
			bool flag = \u0003 == null || \u0003.ChecksumNoInit != \u0002.ChecksumNoInit || !\u0003.HasAttribute("subsequent");
			bool flag2 = \u0002.GetFlag(SignatureFlag.ContainsRetain);
			if (\u0004.RetainInCycle)
			{
				flag2 = false;
			}
			DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Data;
			if (flag2)
			{
				dataSegmentFlags = DataSegmentFlags.Retain;
			}
			ushort u = 0;
			int num = 0;
			if (!flag)
			{
				foreach (_IVariable ivariable in \u0002.AllVariables)
				{
					_IVariable ivariable2 = null;
					if (\u0003 != null)
					{
						ivariable2 = (\u0003[ivariable.Id] as _IVariable);
					}
					if (ivariable2 == null)
					{
						flag = true;
						break;
					}
					if (!global::\u0006.\u0011.\u0001(ivariable.CompiledType, ivariable2.CompiledType, global::\u0007.\u0005.\u0001(\u0004) as ICommonScope, global::\u0007.\u0005.\u0001(\u0005) as ICommonScope))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				if (\u0004.DSFCallback != null)
				{
					dataSegmentFlags = \u0004.DSFCallback.GetDataSegmentFlagForGVLSubsequent(\u0002, \u0004, dataSegmentFlags);
				}
				int u2 = Locator.\u0001(\u0002, 0, \u0006);
				if (!MemoryCompiler.\u0003(\u0007, ref u, ref num, u2, \u0002.Size, \u0007.DataSegmentSize, dataSegmentFlags))
				{
					\u0002.AddMessage(Severity.Error, MessageId.Err_OutOfMemory, new object[]
					{
						\u0002.OrgName,
						\u0002.Size
					});
				}
			}
			else
			{
				_IVariable ivariable3 = \u0003.AllVariables.FirstOrDefault(new Func<_IVariable, bool>(Locator.<>c.<>9.\u0001));
				IDataLocation dataLocation = (ivariable3 != null) ? ivariable3.DataLocation : null;
				if (dataLocation != null)
				{
					bool flag3 = MemoryCompiler.\u0002(\u0007, dataLocation.Area, dataLocation.Offset, \u0002.Size, DataSegmentFlags.None);
					Debug.\u0001(flag3, "Memory Corrupted! Clean application necessary!");
					if (!flag3)
					{
						_IExpression nameExpression = \u0002._NameExpression;
						\u0002.AddMessageString(((nameExpression != null) ? nameExpression.Position : null) as _ISourcePosition, Severity.FatalError, "Memory Corrupted! Clean application necessary!", Array.Empty<object>());
					}
				}
			}
			foreach (_IVariable ivariable4 in \u0002.AllVariables)
			{
				if (!ivariable4.IsProperty)
				{
					if (ivariable4.Address != null)
					{
						\u0002.AddMessage(ivariable4._SourcePosition, Severity.Error, MessageId.Err_NoDirectAddressInSubsequentVarDecl, new object[]
						{
							ivariable4.OrgName
						});
					}
					else
					{
						_IVariable ivariable5 = null;
						if (\u0003 != null)
						{
							ivariable5 = (\u0003[ivariable4.Id] as _IVariable);
						}
						ivariable4.SetFlag(VarFlag.Absolut, true);
						ivariable4.SetFlag(VarFlag.RelativeInstance, false);
						ivariable4.SetFlag(VarFlag.RelativeStack, false);
						if (Locator.\u0001(ivariable4, ivariable5, \u0004, \u0005, flag) || flag)
						{
							ivariable4.SetFlag(VarFlag.LocationChanged, true);
							ivariable4.SetFlag((VarFlag)((ulong)int.MinValue), true);
							IDataLocation dataLocation2 = ivariable4.DataLocation;
							if (dataLocation2.IsRelativ)
							{
								ivariable4.DataLocation = global::\u0019.\u0003.\u0001(u, num + dataLocation2.Offset);
							}
						}
						else
						{
							ivariable4.DataLocation = ivariable5.DataLocation;
							if (!ivariable4.HasFlag(VarFlag.ReplacedConstant))
							{
								Locator.\u0001(\u0002, ivariable4, ivariable5, \u0004, \u0005);
							}
						}
					}
				}
			}
		}

		// Token: 0x06002E3B RID: 11835 RVA: 0x000ABF0C File Offset: 0x000AA10C
		private static IList<_ISignature> \u0001(IList<_ISignature> \u0002)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			LSortedList<int, LList<_ISignature>> lsortedList = new LSortedList<int, LList<_ISignature>>();
			foreach (_ISignature isignature in \u0002)
			{
				int num = 50000;
				if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_LOCATE_DATA_SLOT))
				{
					string attributeValue = isignature.GetAttributeValue(CompileAttributes.ATTRIBUTE_LOCATE_DATA_SLOT);
					try
					{
						num = int.Parse(attributeValue);
					}
					catch
					{
					}
				}
				LList<_ISignature> llist2 = null;
				if (!lsortedList.TryGetValue(num, ref llist2))
				{
					llist2 = new LList<_ISignature>();
					lsortedList[num] = llist2;
				}
				llist2.Add(isignature);
			}
			using (IEnumerator<LList<_ISignature>> enumerator2 = lsortedList.Values.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					foreach (IGrouping<bool, _ISignature> grouping in enumerator2.Current.GroupBy(new Func<_ISignature, bool>(Locator.<>c.<>9.\u0001)).OrderBy(new Func<IGrouping<bool, _ISignature>, bool>(Locator.<>c.<>9.\u0001)))
					{
						llist.AddRange(grouping);
					}
				}
			}
			return llist;
		}

		// Token: 0x06002E3C RID: 11836 RVA: 0x000AC084 File Offset: 0x000AA284
		internal static int \u0001(ICompiledType \u0002, int \u0003, IScope5 \u0004)
		{
			TypeClass @class = \u0002.Class;
			if (@class == TypeClass.String)
			{
				return 1;
			}
			if (@class == TypeClass.WString)
			{
				return 2;
			}
			switch (@class)
			{
			case TypeClass.Subrange:
				return Locator.\u0002((\u0002 as _ISubrangeType)._Base, \u0003, \u0004);
			case TypeClass.Array:
				return Locator.\u0002((\u0002.DeRefType as _IArrayType).BaseType, \u0003, \u0004);
			case TypeClass.Userdef:
			{
				_IUserdefType iuserdefType = \u0002.DeRefType as _IUserdefType;
				_ISignature isignature = \u0004[iuserdefType.SignatureId] as _ISignature;
				Debug.\u0001(isignature != null, "Internal Error");
				if (isignature == null)
				{
					return 1;
				}
				int num = 8;
				foreach (IVariable variable in isignature.AllVariables)
				{
					int num2 = Locator.\u0001(((_IVariable)variable).CompiledType, \u0003, \u0004);
					if (num2 < num)
					{
						num = num2;
					}
				}
				if (isignature.POUType == Operator.FunctionBlock && \u0004.PointerSize < num)
				{
					num = \u0004.PointerSize;
				}
				return num;
			}
			}
			int num3 = \u0002.Size(\u0004);
			if (num3 > \u0003)
			{
				return \u0003;
			}
			return num3;
		}

		// Token: 0x06002E3D RID: 11837 RVA: 0x000AC1AC File Offset: 0x000AA3AC
		internal static int \u0001(_ISignature \u0002, int \u0003, IScope5 \u0004)
		{
			ICodegenerator3 codegenerator = \u0004.ApplicationContext.Codegenerator as ICodegenerator3;
			int num = 1;
			IEnumerable<IVariable> allVariables = \u0002.AllVariables;
			foreach (IVariable variable in allVariables)
			{
				int num2 = Locator.\u0001((_IVariable)variable, \u0003, \u0004);
				if (num2 > num)
				{
					num = num2;
				}
			}
			if (\u0002.BaseSignatureId != -1)
			{
				int num3 = Locator.\u0001(\u0004[\u0002.BaseSignatureId] as _ISignature, \u0003, \u0004);
				if (num3 > num)
				{
					num = num3;
				}
			}
			if (\u0002.POUType == Operator.FunctionBlock)
			{
				if (\u0004.PointerSize > num)
				{
					num = \u0004.PointerSize;
				}
			}
			else if (codegenerator != null && codegenerator.GetProperty(CodegeneratorProperties.MinimalGranularity2) && 2 > num && allVariables.Count<IVariable>() > 1)
			{
				num = 2;
			}
			return num;
		}

		// Token: 0x06002E3E RID: 11838 RVA: 0x000AC280 File Offset: 0x000AA480
		private static int \u0001(IVariable \u0002, int \u0003, IScope5 \u0004)
		{
			if ((\u0002 as _IVariable).IsProperty && \u0002.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) != CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
			{
				return 1;
			}
			int num = Locator.\u0002(\u0002.CompiledType, \u0003, \u0004);
			int val;
			if (\u0002.HasAttribute("granularity") && int.TryParse(\u0002.GetAttributeValue("granularity"), out val))
			{
				num = Math.Max(num, val);
			}
			return num;
		}

		// Token: 0x06002E3F RID: 11839 RVA: 0x000AC2EC File Offset: 0x000AA4EC
		private static int \u0002(int \u0002, int \u0003)
		{
			while (\u0002 != 0 && \u0003 != 0)
			{
				if (\u0002 > \u0003)
				{
					\u0002 %= \u0003;
				}
				else
				{
					\u0003 %= \u0002;
				}
			}
			if (\u0002 != 0)
			{
				return \u0002;
			}
			return \u0003;
		}

		// Token: 0x06002E40 RID: 11840 RVA: 0x000AC30C File Offset: 0x000AA50C
		private static long \u0001(int \u0002, int \u0003)
		{
			return (long)(\u0002 / Locator.\u0002(\u0002, \u0003) * \u0003);
		}

		// Token: 0x06002E41 RID: 11841 RVA: 0x000AC31C File Offset: 0x000AA51C
		internal static int \u0002(ICompiledType \u0002, int \u0003, IScope5 \u0004)
		{
			TypeClass @class = \u0002.Class;
			if (@class <= TypeClass.WString)
			{
				if (@class == TypeClass.String)
				{
					return 1;
				}
				if (@class == TypeClass.WString)
				{
					return 2;
				}
			}
			else
			{
				switch (@class)
				{
				case TypeClass.Subrange:
					return Locator.\u0002((\u0002 as _ISubrangeType)._Base, \u0003, \u0004);
				case TypeClass.Enum:
					return Locator.\u0002(((_IEnumType)\u0002)._Base, \u0003, \u0004);
				case TypeClass.Array:
					return Locator.\u0002((\u0002.DeRefType as _IArrayType).BaseType, \u0003, \u0004);
				case TypeClass.Params:
					break;
				case TypeClass.Userdef:
				{
					_IUserdefType iuserdefType = \u0002.DeRefType as _IUserdefType;
					_ISignature isignature = \u0004[iuserdefType.SignatureId] as _ISignature;
					if (isignature == null || isignature.HasErrors)
					{
						return 8;
					}
					int num = Locator.\u0001(isignature, \u0003, \u0004);
					int packMode = isignature.PackMode;
					if (packMode != -1 && num > packMode)
					{
						num = packMode;
					}
					return num;
				}
				default:
					if (@class == TypeClass.__Vector)
					{
						int u = Locator.\u0002((\u0002 as _IVectorType)._Base, \u0003, \u0004);
						int vectorAlignment = ((_ICompileContext)((_IScope)\u0004).ApplicationContext).VectorAlignment;
						return (int)Locator.\u0001(u, vectorAlignment);
					}
					break;
				}
			}
			int num2 = \u0002.Size(\u0004);
			if (num2 < \u0003)
			{
				return \u0003;
			}
			return num2;
		}

		// Token: 0x06002E42 RID: 11842 RVA: 0x000AC444 File Offset: 0x000AA644
		internal static IEnumerable<_IArea> \u0001(_ICompileContext \u0002, _IMemorySettings \u0003, IList<_IArea> \u0004)
		{
			if (\u0003.MaxSizeForOnlineChange != 2147483647 || global::\u0016.\u0004.SingleOnlineChangeArea.GetBoolValue(\u0002.GetTargetSettings()))
			{
				return new _IArea[]
				{
					Locator.\u0001(\u0002, \u0003, \u0004)
				};
			}
			return Locator.\u0001(\u0003);
		}

		// Token: 0x06002E43 RID: 11843 RVA: 0x000AC480 File Offset: 0x000AA680
		private static IEnumerable<_IArea> \u0001(_IMemorySettings \u0002)
		{
			List<_IArea> list = new List<_IArea>();
			foreach (_IArea iarea in \u0002.Areas.OfType<_IArea>())
			{
				if (!iarea.GetAreaFlag(AreaFlags.OnlineChange))
				{
					if (iarea.GetDataSegmentFlag(DataSegmentFlags.Data) || iarea.GetDataSegmentFlag(DataSegmentFlags.Code) || iarea.GetDataSegmentFlag(DataSegmentFlags.Constant))
					{
						DataSegmentFlags dataSegmentFlags = DataSegmentFlags.Input | DataSegmentFlags.Output | DataSegmentFlags.Memory | DataSegmentFlags.Area | DataSegmentFlags.NonSafety | DataSegmentFlags.Special;
						_IArea iarea2 = global::\u0019.\u0003.\u0001(iarea.DataSegmentFlags & ~dataSegmentFlags, AreaFlags.DynamicSize | AreaFlags.OnlineChange);
						iarea2.MinimalAreaSize = iarea.MinimalAreaSize;
						iarea2.MaximalAreaSize = iarea.MaximalAreaSize;
						iarea2.AllocationPlusInPercent = iarea.AllocationPlusInPercent;
						iarea2.Size = iarea.MaximalAreaSize;
						list.Add(iarea2);
					}
					_IArea item = list.First(new Func<_IArea, bool>(Locator.<>c.<>9.\u0001));
					list.Remove(item);
					list.Add(item);
				}
			}
			return list;
		}

		// Token: 0x06002E44 RID: 11844 RVA: 0x000AC58C File Offset: 0x000AA78C
		private static _IArea \u0001(_ICompileContext \u0002, _IMemorySettings \u0003, IEnumerable<_IArea> \u0004)
		{
			_IArea iarea = null;
			if (\u0002.DataManager.HasConstantSegment)
			{
				iarea = global::\u0019.\u0003.\u0001(DataSegmentFlags.Data | DataSegmentFlags.Constant | DataSegmentFlags.Code, AreaFlags.DynamicSize | AreaFlags.OnlineChange);
			}
			else
			{
				iarea = global::\u0019.\u0003.\u0001(DataSegmentFlags.Data | DataSegmentFlags.Code, AreaFlags.DynamicSize | AreaFlags.OnlineChange);
			}
			int num = \u0003.MaxSizeForOnlineChange;
			if (\u0003.MaxSizeForOnlineChange != 2147483647)
			{
				foreach (_IArea iarea2 in \u0004)
				{
					if (iarea2.GetDataSegmentFlag(DataSegmentFlags.Data | DataSegmentFlags.Code))
					{
						num -= iarea2.Size;
					}
				}
				iarea.Size = num;
			}
			else
			{
				_IArea iarea3 = \u0003.Areas.OfType<_IArea>().First(new Func<_IArea, bool>(Locator.<>c.<>9.\u0002));
				iarea.MinimalAreaSize = iarea3.MinimalAreaSize;
				iarea.MaximalAreaSize = iarea3.MaximalAreaSize;
				iarea.AllocationPlusInPercent = iarea3.AllocationPlusInPercent;
				iarea.Size = iarea3.MaximalAreaSize;
			}
			return iarea;
		}

		// Token: 0x020002F1 RID: 753
		internal sealed class \u0001
		{
			// Token: 0x06002E46 RID: 11846 RVA: 0x000AC690 File Offset: 0x000AA890
			internal void \u0001()
			{
				this.\u0005 = this.Offset;
				this.\u0006 = -1;
			}

			// Token: 0x170007CB RID: 1995
			// (get) Token: 0x06002E47 RID: 11847 RVA: 0x000AC6A8 File Offset: 0x000AA8A8
			// (set) Token: 0x06002E48 RID: 11848 RVA: 0x000AC6B0 File Offset: 0x000AA8B0
			internal int Offset
			{
				get
				{
					return this.\u0001;
				}
				set
				{
					this.\u0001 = value;
					if (this.\u0001 > this.\u0004)
					{
						this.\u0004 = this.\u0001;
					}
				}
			}

			// Token: 0x06002E49 RID: 11849 RVA: 0x000AC6D4 File Offset: 0x000AA8D4
			internal void \u0001(int \u0002, int \u0003)
			{
				this.\u0006 = \u0002;
				if (this.\u0006 <= 0)
				{
					this.\u0006 = 8;
				}
				if (this.\u0006 > \u0003)
				{
					this.\u0006 = \u0003;
				}
				if (this.\u0006 > this.\u0003)
				{
					this.\u0003 = this.\u0006;
				}
			}

			// Token: 0x06002E4A RID: 11850 RVA: 0x000AC724 File Offset: 0x000AA924
			internal void \u0002()
			{
				if (this.\u0005 % this.\u0006 != 0)
				{
					this.\u0005 = (this.\u0005 / this.\u0006 + 1) * this.\u0006;
				}
			}

			// Token: 0x06002E4B RID: 11851 RVA: 0x000AC754 File Offset: 0x000AA954
			internal void \u0003()
			{
				this.\u0001 = 0U;
				this.\u0002 = -1;
			}

			// Token: 0x06002E4C RID: 11852 RVA: 0x000AC764 File Offset: 0x000AA964
			internal void \u0004()
			{
				if (this.\u0002 == -1)
				{
					this.\u0002 = this.\u0005;
					this.\u0005++;
				}
				if (this.\u0001 == 8U)
				{
					this.\u0001 = 0U;
					this.\u0002++;
					this.\u0005++;
				}
			}

			// Token: 0x06002E4D RID: 11853 RVA: 0x000AC7C0 File Offset: 0x000AA9C0
			internal void \u0001(bool \u0002)
			{
				if (\u0002 && this.\u0004 > this.\u0001)
				{
					this.\u0001 = this.\u0004;
				}
			}

			// Token: 0x040008C3 RID: 2243
			internal int \u0001;

			// Token: 0x040008C4 RID: 2244
			internal int \u0002;

			// Token: 0x040008C5 RID: 2245
			internal uint \u0001;

			// Token: 0x040008C6 RID: 2246
			internal int \u0003;

			// Token: 0x040008C7 RID: 2247
			internal int \u0004;

			// Token: 0x040008C8 RID: 2248
			internal int \u0005;

			// Token: 0x040008C9 RID: 2249
			internal int \u0006;
		}
	}
}
