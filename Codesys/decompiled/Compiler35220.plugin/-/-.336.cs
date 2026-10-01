using System;
using System.Collections.Generic;
using \u0004;
using \u0007;
using \u0011;
using \u0014;
using \u0018;
using \u001C;
using _3S.CoDeSys.Compiler35220.Phase3_Location;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0003
{
	// Token: 0x0200037D RID: 893
	internal sealed class \u0015 : \u001E
	{
		// Token: 0x06003469 RID: 13417 RVA: 0x000CE65C File Offset: 0x000CC85C
		public bool \u0001(global::\u0018.\u0010 \u0002)
		{
			foreach (global::\u0011.\u0014 u in \u0002.changedSignatures)
			{
				if (!this.\u0001(\u0002, u))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600346A RID: 13418 RVA: 0x000CE6B4 File Offset: 0x000CC8B4
		private bool \u0001(global::\u0018.\u0010 \u0002, global::\u0011.\u0014 \u0003)
		{
			_ISignature isignature = \u0003.CompiledSignature;
			if (\u0003.\u000E || \u0003.\u000F)
			{
				\u0002.\u0001 = true;
			}
			_ISignature isignature2 = this.\u0001(\u0002, isignature, \u0003);
			if (isignature2 == null)
			{
				return false;
			}
			if (\u0003.\u000E)
			{
				bool flag;
				global::\u0003.\u0015.\u0001(isignature2, isignature, \u0002.ComconNew, \u0002.ComconOld, out flag);
				if (flag)
				{
					return false;
				}
			}
			if (\u0003.\u000F)
			{
				global::\u0003.\u0015.\u0001(isignature2, isignature, \u0002.ComconNew, \u0002.ComconOld);
			}
			if (\u0002.\u0001(isignature2))
			{
				return false;
			}
			_ISignature isignature3 = \u0002.ComconOld[isignature.Id];
			if (isignature3 == null)
			{
				return false;
			}
			global::\u0003.\u0016 u = new global::\u0003.\u0016
			{
				focContext = \u0002,
				signChanges = \u0003,
				signCompiled = isignature2,
				signRef = isignature3,
				sign = isignature
			};
			using (IEnumerator<\u001C.\u0012> enumerator = global::\u0014.\u0014.Instance.ChangedSignatureProcessorSteps.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.\u0001(u))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600346B RID: 13419 RVA: 0x000CE7CC File Offset: 0x000CC9CC
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ISignature \u0004)
		{
			\u0002.CalleeSize = \u0003.CalleeSize;
			\u0002.Size = \u0003.Size;
			\u0002.HighestUsedOffset = \u0003.HighestUsedOffset;
			\u0002.SetFlag(SignatureFlag.Located, true);
			global::\u0003.\u0015.\u0002(\u0002, \u0003);
			global::\u0003.\u0015.\u0001(\u0002, \u0004);
			if (\u0003.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				\u0002.AddAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC, \u0003.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC));
			}
		}

		// Token: 0x0600346C RID: 13420 RVA: 0x000CE83C File Offset: 0x000CCA3C
		private _ISignature \u0001(global::\u0018.\u0010 \u0002, _ISignature \u0003, global::\u0011.\u0014 \u0004)
		{
			_ISignature isignature = \u0004.PrecompileSignature.CreateCompiledSignature(\u0003, \u0002.ComconNew, \u0002.ComconOld, \u0002.ComconNew.HasByteSupport());
			foreach (ISignature sign in \u0003.SubSignatures)
			{
				isignature.AddSubSignature(sign);
			}
			\u0002.ComconNew.RemoveSignature(\u0003);
			isignature.ParentSignatureId = \u0003.ParentSignatureId;
			if (\u0003.ParentSignatureId != Helper.InvalidId)
			{
				_ISignature u = \u0002.ComconNew[isignature.ParentSignatureId];
				_ISignature isignature2 = this.\u0001(\u0002, u);
				isignature2.RemoveSubSignature(\u0003);
				isignature2.AddSubSignature(isignature);
			}
			\u0002.ComconNew.AddSignature(isignature, \u0003, \u0002.ComconOld, false);
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0002.ComconNew, isignature.Id);
			scope.LocalSignature = isignature;
			\u0002.CompileInformation.InterfaceCompiler.\u0003(isignature, scope);
			if (\u0004.CheckInitialValues)
			{
				foreach (_IVariable u2 in isignature.AllVariables)
				{
					global::\u0004.\u0011.\u0001(u2, scope, \u0002.ComconNew, isignature);
				}
			}
			if (\u0002.\u0001(isignature))
			{
				return null;
			}
			LStack<ISignature> u3 = new LStack<ISignature>();
			uint num;
			if (!global::\u0003.\u0002.\u0001(isignature, u3, \u0002.ComconNew, out num))
			{
				return null;
			}
			isignature.AddAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC, num.ToString());
			return isignature;
		}

		// Token: 0x0600346D RID: 13421 RVA: 0x000CE9B4 File Offset: 0x000CCBB4
		private _ISignature \u0001(global::\u0018.\u0010 \u0002, _ISignature \u0003)
		{
			_ISignature isignature = \u0003.Duplicate(true);
			global::\u0003.\u0015.\u0001(isignature, \u0003, \u0003);
			\u0002.ComconNew.ReplaceSignature(\u0003, isignature);
			return isignature;
		}

		// Token: 0x0600346E RID: 13422 RVA: 0x000CE9E0 File Offset: 0x000CCBE0
		private static void \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (byte byTaskIndex in \u0003.TaskReferenceList)
			{
				\u0002.AddTaskReference(byTaskIndex);
			}
			foreach (int nId in \u0003.CallerIds)
			{
				\u0002.AddCaller(nId);
			}
			if (\u0003.POUType == Operator.FunctionBlock)
			{
				foreach (int nId2 in \u0003.DeclarerIds)
				{
					\u0002.AddDeclarer(nId2);
				}
				foreach (int nId3 in \u0003.ReferencerIds)
				{
					\u0002.AddReferencer(nId3);
				}
			}
		}

		// Token: 0x0600346F RID: 13423 RVA: 0x000CEA80 File Offset: 0x000CCC80
		private static void \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			_ISignatureSupportingFastOnlineChange isignatureSupportingFastOnlineChange = (_ISignatureSupportingFastOnlineChange)\u0002;
			isignatureSupportingFastOnlineChange.FPDataLocation = \u0003.FPDataLocation;
			if (\u0003.POUType == Operator.FunctionBlock || \u0003.POUType == Operator.Interface)
			{
				isignatureSupportingFastOnlineChange._VirtualFunctionTable = ((_ISignature2)\u0003)._VirtualFunctionTable;
			}
		}

		// Token: 0x06003470 RID: 13424 RVA: 0x000CEAC8 File Offset: 0x000CCCC8
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005, out bool \u0006)
		{
			\u0006 = false;
			_ISignature u = \u0005[\u0003.Id];
			foreach (_IVariable ivariable in global::\u0003.\u0015.\u0001(\u0003, \u0002))
			{
				if (!global::\u0003.\u0015.\u0001(\u0002, ivariable))
				{
					Locator.\u0001(\u0002, ivariable, u, \u0004, \u0005, out \u0006);
					ivariable.SetFlag(VarFlag.OnlChangeInit, true);
				}
			}
		}

		// Token: 0x06003471 RID: 13425 RVA: 0x000CEB44 File Offset: 0x000CCD44
		private static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			IScope5 scope = global::\u0007.\u0005.\u0001(\u0005, \u0003.Id);
			foreach (_IVariable ivariable in global::\u0003.\u0015.\u0002(\u0003, \u0002))
			{
				if (!ivariable.DataLocation.IsRelativ)
				{
					MemoryCompiler.\u0001(\u0004.DataManager, ivariable.DataLocation, ivariable.CompiledType.Size(scope), DataSegmentFlags.None);
				}
			}
		}

		// Token: 0x06003472 RID: 13426 RVA: 0x000CEBC4 File Offset: 0x000CCDC4
		internal static IEnumerable<_IVariable> \u0001(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (_IVariable ivariable in \u0003.AllVariables)
			{
				if (!(\u0002[ivariable.OrgName] is _IVariable))
				{
					yield return ivariable;
				}
			}
			IEnumerator<_IVariable> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003473 RID: 13427 RVA: 0x000CEBDC File Offset: 0x000CCDDC
		private static IEnumerable<_IVariable> \u0002(_ISignature \u0002, _ISignature \u0003)
		{
			foreach (_IVariable ivariable in \u0002.AllVariables)
			{
				if (!(\u0003[ivariable.OrgName] is _IVariable))
				{
					yield return ivariable;
				}
			}
			IEnumerator<_IVariable> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06003474 RID: 13428 RVA: 0x000CEBF4 File Offset: 0x000CCDF4
		private static bool \u0001(_ISignature \u0002, _IVariable \u0003)
		{
			if (\u0003.HasFlag(VarFlag.IsCompiled))
			{
				return \u0003.HasFlag(VarFlag.RelativeStack);
			}
			if (\u0002.POUType == Operator.Function || \u0002.POUType == Operator.Method)
			{
				return \u0003.HasFlag(VarFlag.Local | VarFlag.Input | VarFlag.Output);
			}
			return \u0003.HasFlag(VarFlag.Temp);
		}
	}
}
