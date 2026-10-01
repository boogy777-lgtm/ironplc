using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using \u0006;
using \u0016;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace _3S.CoDeSys.Compiler35220.Phase2_AfterTypification
{
	// Token: 0x020002F8 RID: 760
	internal static class ObjectsToCompileDetector
	{
		// Token: 0x06002EA3 RID: 11939 RVA: 0x000AEBE4 File Offset: 0x000ACDE4
		internal static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			bool flag = false;
			if (\u0003 != null)
			{
				_IPreCompileContext precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(\u0002.ApplicationGuid);
				flag = \u0003.DefineChanged(precomp);
			}
			if (\u0003 == null || flag)
			{
				foreach (_ICompiledPOU icompiledPOU in \u0002.CompiledPOUList)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
				}
				return;
			}
			foreach (_ICompiledPOU icompiledPOU2 in \u0002.CompiledPOUList)
			{
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, false);
				_ICompiledPOU icompiledPOU3 = \u0003._GetCompiledPOUById(icompiledPOU2.SignatureId);
				if (icompiledPOU3 == null || icompiledPOU2.GetFlag(CompiledPOUFlags.ToGenerate))
				{
					icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
					if (icompiledPOU3 == null)
					{
						icompiledPOU2.SetFlag(CompiledPOUFlags.New, true);
					}
				}
				else
				{
					if (icompiledPOU3.Checksum == 0U)
					{
						if (icompiledPOU3.TimeStamp != icompiledPOU2.TimeStamp)
						{
							icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
						}
					}
					else if (icompiledPOU3.Checksum != icompiledPOU2.Checksum)
					{
						icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
					}
					if (!icompiledPOU3.GetFlag(CompiledPOUFlags.TopLevel) && icompiledPOU2.GetFlag(CompiledPOUFlags.TopLevel))
					{
						icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
					}
				}
			}
			IList<_ISignature> allFlat = \u0002.AllFlat;
			global::\u0016.\u0012 u = new global::\u0016.\u0012();
			foreach (_ISignature isignature in allFlat)
			{
				isignature.SetFlag(SignatureFlag.Temp, false);
			}
			foreach (_ISignature isignature2 in allFlat)
			{
				try
				{
					_ISignature isignature3 = \u0003[isignature2.Id];
					if (isignature2.POUType != Operator.Program && (isignature3 == null || isignature2.ChecksumNoInit != isignature3.ChecksumNoInit || isignature3.Size != isignature2.Size))
					{
						isignature2.SetFlag(SignatureFlag.Temp, true);
					}
					ObjectsToCompileDetector.\u0001(\u0002, isignature2, isignature3);
					ObjectsToCompileDetector.\u0002(\u0002, isignature2, isignature3);
					if (isignature2.GetFlag(SignatureFlag.FunctionTableChanged))
					{
						isignature2.SetFlag(SignatureFlag.Temp, true);
						foreach (object obj in isignature2._SubSignatures)
						{
							((_ISignature)obj).SetFlag(SignatureFlag.Temp, true);
						}
					}
					ObjectsToCompileDetector.\u0001(\u0002, \u0003, u, isignature2, isignature3);
					if (isignature2.GetFlag(SignatureFlag.Temp))
					{
						foreach (int num in isignature2.CallerIds)
						{
							u.Add(num);
						}
					}
				}
				catch (Exception ex)
				{
					Debug.\u0001(ex.Message);
				}
			}
			ObjectsToCompileDetector.\u0001(\u0002, \u0003, u);
			foreach (_ISignature isignature4 in allFlat)
			{
				isignature4.SetFlag(SignatureFlag.Temp, false);
			}
			foreach (int nId in u)
			{
				_ICompiledPOU icompiledPOU4 = \u0002._GetCompiledPOUById(nId);
				if (icompiledPOU4 != null)
				{
					icompiledPOU4.SetFlag(CompiledPOUFlags.ToCompile, true);
				}
			}
			ObjectsToCompileDetector.\u0002(\u0002, \u0003);
		}

		// Token: 0x06002EA4 RID: 11940 RVA: 0x000AF000 File Offset: 0x000AD200
		private static void \u0002(_ICompileContext \u0002, _ICompileContext \u0003)
		{
			foreach (_ICompiledPOU icompiledPOU in \u0002.CompiledPOUList.Where(new Func<_ICompiledPOU, bool>(ObjectsToCompileDetector.<>c.<>9.\u0001)))
			{
				_ICompiledPOU icompiledPOU2 = \u0003._GetCompiledPOUById(icompiledPOU.SignatureId);
				if (icompiledPOU2 != null)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ContainsVirtualFunctionCalls, icompiledPOU2.GetFlag(CompiledPOUFlags.ContainsVirtualFunctionCalls));
					icompiledPOU.SetMessages(icompiledPOU2.Messages);
				}
			}
		}

		// Token: 0x06002EA5 RID: 11941 RVA: 0x000AF09C File Offset: 0x000AD29C
		private static void \u0001(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			if (\u0004 != null && \u0003.Checksum != \u0004.Checksum)
			{
				_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0003.Id);
				if (icompiledPOU != null)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
				}
				if (\u0003.POUType == Operator.FunctionBlock || \u0003.POUType == Operator.Type)
				{
					ISignature subSignature = \u0003.GetSubSignature(IdentifierConstants.InitMethodName);
					if (subSignature != null)
					{
						_ICompiledPOU icompiledPOU2 = \u0002._GetCompiledPOUById(subSignature.Id);
						if (icompiledPOU2 != null)
						{
							icompiledPOU2.SetFlag(CompiledPOUFlags.ToCompile, true);
						}
					}
				}
				if (ObjectsToCompileDetector.\u0001(\u0004, \u0003))
				{
					ObjectsToCompileDetector.\u0001(\u0002, \u0003, new CompilerServicesInternal.CallersCompiledPOUProcessor(ObjectsToCompileDetector.<>c.<>9.\u0001));
				}
			}
		}

		// Token: 0x06002EA6 RID: 11942 RVA: 0x000AF144 File Offset: 0x000AD344
		private static void \u0002(_ICompileContext \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			if (\u0004 != null && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_NO_CHECK) != \u0004.HasAttribute(CompileAttributes.ATTRIBUTE_NO_CHECK))
			{
				_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(\u0003.Id);
				if (icompiledPOU == null)
				{
					return;
				}
				icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, true);
			}
		}

		// Token: 0x06002EA7 RID: 11943 RVA: 0x000AF17C File Offset: 0x000AD37C
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, global::\u0016.\u0012 \u0004)
		{
			foreach (_ISignature isignature in \u0003.AllFlat)
			{
				if (!isignature.GetFlag(SignatureFlag.NoCompareWithNew))
				{
					_ISignature isignature2 = \u0002[isignature.Id];
					if (isignature2 == null)
					{
						foreach (int u in isignature.CallerIds)
						{
							\u0004.\u0001(u);
						}
					}
					ObjectsToCompileDetector.\u0001(\u0004, isignature2, isignature.AllVariables);
					if (isignature2 != null && !ObjectsToCompileDetector.\u0001(isignature2, isignature))
					{
						foreach (int u2 in isignature2.ReferencerIds)
						{
							\u0004.\u0001(u2);
						}
					}
				}
			}
		}

		// Token: 0x06002EA8 RID: 11944 RVA: 0x000AF250 File Offset: 0x000AD450
		private static void \u0001(global::\u0016.\u0012 \u0002, _ISignature \u0003, IEnumerable<IVariable> \u0004)
		{
			foreach (_IVariable ivariable in \u0004.OfType<_IVariable>())
			{
				_IVariable ivariable2 = null;
				if (\u0003 != null)
				{
					ivariable2 = (\u0003[ivariable.Id] as _IVariable);
				}
				if (ivariable2 == null && !ivariable.GetFlag(VarFlag.Implicit))
				{
					foreach (ICrossReference crossReference in ivariable.CrossReferences)
					{
						\u0002.\u0001(crossReference.CodeId);
					}
				}
			}
		}

		// Token: 0x06002EA9 RID: 11945 RVA: 0x000AF2EC File Offset: 0x000AD4EC
		private static void \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ICompileContext \u0005, _ISignature \u0006, global::\u0016.\u0012 \u0007)
		{
			ObjectsToCompileDetector.\u0001 u = new ObjectsToCompileDetector.\u0001();
			u.\u0001 = \u0007;
			if (\u0003.GetFlag(VarFlag.Input | VarFlag.RelativeStack) && \u0002 != null && \u0002.Initial != null && \u0003.Initial != null && !Helper.\u0001(\u0005, \u0004, \u0006, \u0002, \u0003))
			{
				ObjectsToCompileDetector.\u0001(\u0005, \u0006, new CompilerServicesInternal.CallersCompiledPOUProcessor(u.\u0001));
			}
		}

		// Token: 0x06002EAA RID: 11946 RVA: 0x000AF350 File Offset: 0x000AD550
		private static void \u0001(_IVariable \u0002, _IVariable \u0003, _ICompileContext \u0004, _ISignature \u0005, global::\u0016.\u0012 \u0006)
		{
			ObjectsToCompileDetector.\u0002 u = new ObjectsToCompileDetector.\u0002();
			u.\u0001 = \u0006;
			if (\u0003.GetFlag(VarFlag.Inout) && (\u0002 == null || !\u0002.GetFlag(VarFlag.Inout)))
			{
				ObjectsToCompileDetector.\u0001(\u0004, \u0005, new CompilerServicesInternal.CallersCompiledPOUProcessor(u.\u0001));
			}
		}

		// Token: 0x06002EAB RID: 11947 RVA: 0x000AF3A0 File Offset: 0x000AD5A0
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, global::\u0016.\u0012 \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			foreach (_IVariable u in \u0005.AllVariables)
			{
				ObjectsToCompileDetector.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006, u);
			}
		}

		// Token: 0x06002EAC RID: 11948 RVA: 0x000AF3F4 File Offset: 0x000AD5F4
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, global::\u0016.\u0012 \u0004, _ISignature \u0005, _ISignature \u0006, _IVariable \u0007)
		{
			_IVariable ivariable = null;
			if (\u0006 != null)
			{
				ivariable = (\u0006[\u0007.Id] as _IVariable);
			}
			bool flag = ObjectsToCompileDetector.\u0001(\u0002, \u0003, \u0005, \u0007, ivariable);
			if (!flag && ivariable != null && ivariable.DataLocation == null && \u0007.DataLocation == null)
			{
				return;
			}
			ObjectsToCompileDetector.\u0001(ivariable, \u0007, \u0003, \u0002, \u0005, \u0004);
			ObjectsToCompileDetector.\u0001(ivariable, \u0007, \u0002, \u0005, \u0004);
			IScope scope = \u0003.CreateGlobalIScope();
			IScope scope2 = \u0002.CreateGlobalIScope();
			bool flag2 = ivariable != null && global::\u0006.\u0011.\u0001(\u0007._Type, ivariable._Type, scope2 as ICommonScope, scope as ICommonScope);
			bool flag3 = flag || ivariable == null || ivariable.VersionedName != \u0007.VersionedName || (ivariable.DataLocation == null && \u0007.DataLocation != null) || (ivariable.DataLocation != null && \u0007.DataLocation == null) || !ivariable.DataLocation.IsEqual(\u0007.DataLocation) || !flag2;
			flag3 = ObjectsToCompileDetector.\u0001(\u0007, ivariable, scope, scope2, flag3);
			flag3 = ObjectsToCompileDetector.\u0001(\u0007, ivariable, flag3);
			if (flag3)
			{
				ObjectsToCompileDetector.\u0001(\u0002, \u0003, \u0004, \u0007, ivariable);
			}
			_IUserdefType iuserdefType = ObjectsToCompileDetector.\u0001(\u0007.CompiledType);
			if (iuserdefType == null)
			{
				return;
			}
			_ISignature isignature = \u0002[iuserdefType.SignatureId];
			if (isignature == null)
			{
				return;
			}
			Debug.\u0001(isignature != null);
			if (!isignature.GetFlag(SignatureFlag.Temp))
			{
				return;
			}
			if (\u0005.POUType != Operator.Program)
			{
				\u0005.SetFlag(SignatureFlag.Temp, true);
			}
			foreach (ICrossReference crossReference in \u0007.CrossReferences)
			{
				\u0004.\u0001(crossReference.CodeId);
			}
		}

		// Token: 0x06002EAD RID: 11949 RVA: 0x000AF598 File Offset: 0x000AD798
		private static bool \u0001(_ICompileContext \u0002, _ICompileContext \u0003, _ISignature \u0004, _IVariable \u0005, _IVariable \u0006)
		{
			return \u0006 != null && (\u0006.GetFlag(VarFlag.Constant) || \u0006.GetFlag(VarFlag.Enum)) && (\u0005.GetFlag(VarFlag.Constant) || \u0005.GetFlag(VarFlag.Enum)) && !Helper.\u0001(\u0002, \u0003, \u0004, \u0006, \u0005);
		}

		// Token: 0x06002EAE RID: 11950 RVA: 0x000AF5F0 File Offset: 0x000AD7F0
		private static _IUserdefType \u0001(ICompiledType \u0002)
		{
			_IUserdefType result = null;
			if (\u0002.Class == TypeClass.Userdef)
			{
				result = (\u0002 as _IUserdefType);
			}
			else if (\u0002.Class == TypeClass.Array)
			{
				\u0002 = \u0084.\u0004.\u0001(\u0002 as _IArrayType);
				if (\u0002.Class == TypeClass.Userdef)
				{
					result = (\u0002 as _IUserdefType);
				}
			}
			return result;
		}

		// Token: 0x06002EAF RID: 11951 RVA: 0x000AF63C File Offset: 0x000AD83C
		private static void \u0001(_ICompileContext \u0002, _ICompileContext \u0003, global::\u0016.\u0012 \u0004, _IVariable \u0005, _IVariable \u0006)
		{
			foreach (ICrossReference u in \u0005.CrossReferences)
			{
				ObjectsToCompileDetector.\u0001(\u0002, \u0004, u);
			}
			if (\u0006 != null)
			{
				foreach (ICrossReference u2 in \u0006.CrossReferences)
				{
					ObjectsToCompileDetector.\u0001(\u0003, \u0004, u2);
				}
			}
		}

		// Token: 0x06002EB0 RID: 11952 RVA: 0x000AF690 File Offset: 0x000AD890
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, IScope \u0004, IScope \u0005, bool \u0006)
		{
			if (!\u0006)
			{
				\u0006 = (\u0003 != null && \u0003.Type.Class == TypeClass.Reference && \u0002.Type.Class == TypeClass.Reference && \u0003.CompiledType.DeRefType.Size(\u0004) != \u0002.CompiledType.DeRefType.Size(\u0005));
				\u0006 = (\u0006 || (\u0003 != null && \u0003.Type.Class == TypeClass.Pointer && \u0002.Type.Class == TypeClass.Pointer && (\u0003.CompiledType as _IPointerType).BaseType.Size(\u0004) != (\u0002.CompiledType as _IPointerType).BaseType.Size(\u0005)));
			}
			return \u0006;
		}

		// Token: 0x06002EB1 RID: 11953 RVA: 0x000AF754 File Offset: 0x000AD954
		private static IEnumerable<string> \u0001(_IVariable \u0002)
		{
			if (\u0002 != null)
			{
				return \u0002.Attributes.Except(new string[]
				{
					"''NORMAL__COMMENT",
					"''DOCU__COMMENT",
					"message_guid",
					"inferredtype",
					"conditionalshow",
					"hide",
					"property",
					"get",
					"set"
				});
			}
			return new string[0];
		}

		// Token: 0x06002EB2 RID: 11954 RVA: 0x000AF7C8 File Offset: 0x000AD9C8
		private static IEnumerable<string> \u0001(_ISignature \u0002)
		{
			if (\u0002 != null)
			{
				return \u0002.Attributes.Except(new string[]
				{
					"''NORMAL__COMMENT",
					"''DOCU__COMMENT",
					"message_guid",
					"generate_implicit_init_function",
					"contains_no_copy",
					"''crc"
				});
			}
			return new string[0];
		}

		// Token: 0x06002EB3 RID: 11955 RVA: 0x000AF824 File Offset: 0x000ADA24
		internal static bool \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			IEnumerable<string> enumerable = ObjectsToCompileDetector.\u0001(\u0002);
			IEnumerable<string> enumerable2 = ObjectsToCompileDetector.\u0001(\u0003);
			int num = enumerable.Count<string>();
			int num2 = enumerable2.Count<string>();
			return (num == 0 && num2 == 0) || (num == num2 && ObjectsToCompileDetector.\u0001(\u0002 as IHasAttributes, \u0003 as IHasAttributes, enumerable, enumerable2));
		}

		// Token: 0x06002EB4 RID: 11956 RVA: 0x000AF870 File Offset: 0x000ADA70
		private static bool \u0001(IHasAttributes \u0002, IHasAttributes \u0003, IEnumerable<string> \u0004, IEnumerable<string> \u0005)
		{
			if (\u0002 == null)
			{
				throw new ArgumentNullException("objNew");
			}
			bool flag = true;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string text in \u0004)
			{
				string attributeValue = \u0002.GetAttributeValue(text);
				dictionary[text] = attributeValue;
			}
			foreach (string text2 in \u0005)
			{
				string attributeValue2 = \u0003.GetAttributeValue(text2);
				string text3;
				if (dictionary.TryGetValue(text2, out text3))
				{
					if (attributeValue2 != null && text3 != null)
					{
						flag = attributeValue2.Equals(text3);
					}
				}
				else
				{
					flag = false;
				}
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}

		// Token: 0x06002EB5 RID: 11957 RVA: 0x000AF93C File Offset: 0x000ADB3C
		private static bool \u0001(_IVariable \u0002, _IVariable \u0003, bool \u0004)
		{
			IEnumerable<string> enumerable = null;
			IEnumerable<string> enumerable2 = null;
			int num = 0;
			int num2 = 0;
			if (!\u0004)
			{
				enumerable = ObjectsToCompileDetector.\u0001(\u0002);
				enumerable2 = ObjectsToCompileDetector.\u0001(\u0003);
				num = enumerable.Count<string>();
				num2 = enumerable2.Count<string>();
				\u0004 = (num != num2);
			}
			if (num == 0 && num2 == 0)
			{
				return \u0004;
			}
			if (!\u0004)
			{
				\u0004 = !ObjectsToCompileDetector.\u0001(\u0002 as IHasAttributes, \u0003 as IHasAttributes, enumerable, enumerable2);
			}
			return \u0004;
		}

		// Token: 0x06002EB6 RID: 11958 RVA: 0x000AF9A0 File Offset: 0x000ADBA0
		private static void \u0001(_ICompileContext \u0002, global::\u0016.\u0012 \u0003, ICrossReference \u0004)
		{
			\u0003.\u0001(\u0004.CodeId);
			ISignature signatureById = \u0002.GetSignatureById(\u0004.CodeId);
			if (signatureById == null || signatureById.POUType != Operator.Type)
			{
				return;
			}
			foreach (int u in signatureById.DeclarerIds)
			{
				\u0003.\u0001(u);
			}
		}

		// Token: 0x06002EB7 RID: 11959 RVA: 0x000AF9F4 File Offset: 0x000ADBF4
		private static bool \u0001(ISignature \u0002, ISignature \u0003)
		{
			ISignatureWithOptionalInputs signatureWithOptionalInputs = \u0003 as ISignatureWithOptionalInputs;
			ISignatureWithOptionalInputs signatureWithOptionalInputs2 = \u0002 as ISignatureWithOptionalInputs;
			return signatureWithOptionalInputs != null && signatureWithOptionalInputs2 != null && signatureWithOptionalInputs2.ChecksumOptionalInputs != signatureWithOptionalInputs.ChecksumOptionalInputs;
		}

		// Token: 0x06002EB8 RID: 11960 RVA: 0x000AFA28 File Offset: 0x000ADC28
		private static void \u0001(_ICompileContext \u0002, ISignature \u0003, CompilerServicesInternal.CallersCompiledPOUProcessor \u0004)
		{
			foreach (int nId in \u0003.CallerIds)
			{
				_ICompiledPOU icompiledPOU = \u0002._GetCompiledPOUById(nId);
				if (icompiledPOU == null && Operator.FunctionBlock == \u0003.POUType)
				{
					ISignature subSignature = \u0003.GetSubSignature(IdentifierConstants.MainSignatureName);
					if (subSignature != null)
					{
						icompiledPOU = \u0002._GetCompiledPOUById(subSignature.Id);
					}
				}
				if (icompiledPOU != null)
				{
					\u0004(icompiledPOU);
				}
			}
		}

		// Token: 0x020002FA RID: 762
		[CompilerGenerated]
		private sealed class \u0001
		{
			// Token: 0x06002EBE RID: 11966 RVA: 0x000AFAC4 File Offset: 0x000ADCC4
			internal void \u0001(_ICompiledPOU \u0002)
			{
				this.\u0001.\u0001(\u0002.SignatureId);
			}

			// Token: 0x040008E6 RID: 2278
			public global::\u0016.\u0012 \u0001;
		}

		// Token: 0x020002FB RID: 763
		[CompilerGenerated]
		private sealed class \u0002
		{
			// Token: 0x06002EC0 RID: 11968 RVA: 0x000AFAE0 File Offset: 0x000ADCE0
			internal void \u0001(_ICompiledPOU \u0002)
			{
				this.\u0001.\u0001(\u0002.SignatureId);
			}

			// Token: 0x040008E7 RID: 2279
			public global::\u0016.\u0012 \u0001;
		}
	}
}
