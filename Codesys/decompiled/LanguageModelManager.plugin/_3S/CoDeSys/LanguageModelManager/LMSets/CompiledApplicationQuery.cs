using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.LMSets
{
	// Token: 0x020001BB RID: 443
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	internal class CompiledApplicationQuery : ILMCompiledApplicationQuery5, ILMCompiledApplicationQuery4, ILMCompiledApplicationQuery3, ILMCompiledApplicationQuery2, ILMCompiledApplicationQuery
	{
		// Token: 0x06001FA3 RID: 8099 RVA: 0x00057509 File Offset: 0x00056509
		internal CompiledApplicationQuery(_ICompileContext comcon)
		{
			this._comcon = comcon;
		}

		// Token: 0x17000847 RID: 2119
		// (get) Token: 0x06001FA4 RID: 8100 RVA: 0x00057518 File Offset: 0x00056518
		public ICodegenerator Codegenerator
		{
			get
			{
				return this._comcon.Codegenerator;
			}
		}

		// Token: 0x17000848 RID: 2120
		// (get) Token: 0x06001FA5 RID: 8101 RVA: 0x00057525 File Offset: 0x00056525
		public int PointerSize
		{
			get
			{
				return this._comcon.PointerSize;
			}
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x00057532 File Offset: 0x00056532
		public IEnumerable<string> FindChangedObjects()
		{
			return this._comcon.FindChangedObjects();
		}

		// Token: 0x06001FA7 RID: 8103 RVA: 0x0005753F File Offset: 0x0005653F
		public IEnumerable<IChangedLMObject> FindChangedObjectsDetailed()
		{
			return this._comcon.FindChangedObjectsDetailed();
		}

		// Token: 0x06001FA8 RID: 8104 RVA: 0x0005754C File Offset: 0x0005654C
		public IDirectVariable FindDirectVariable(IAbsoluteAddressInfo addressInfo)
		{
			return this._comcon.FindDirectVariable(addressInfo);
		}

		// Token: 0x17000849 RID: 2121
		// (get) Token: 0x06001FA9 RID: 8105 RVA: 0x0005755A File Offset: 0x0005655A
		public IApplicationContent ApplicationContent
		{
			get
			{
				return this._comcon.GetApplicationContent();
			}
		}

		// Token: 0x1700084A RID: 2122
		// (get) Token: 0x06001FAA RID: 8106 RVA: 0x00057567 File Offset: 0x00056567
		public IDataManager2 DataManager
		{
			get
			{
				return this._comcon.GetDataManager();
			}
		}

		// Token: 0x06001FAB RID: 8107 RVA: 0x00057574 File Offset: 0x00056574
		public string GetDefaultInitializationCode(IVariable var, ISignature sign, IScope scope, string stInstancePath)
		{
			return CompilerProxy.GetDefaultInitializationCode(var, sign, scope, stInstancePath);
		}

		// Token: 0x06001FAC RID: 8108 RVA: 0x00057580 File Offset: 0x00056580
		public byte[] GetInitializationBlob(IScope5 scope, bool isMotorolaByteOrder, IVariable var, out IRelocationList2 relocations)
		{
			return CompilerProxy.GetInitializationBlob(scope, isMotorolaByteOrder, var, out relocations);
		}

		// Token: 0x06001FAD RID: 8109 RVA: 0x0005758C File Offset: 0x0005658C
		public IMemoryManager GetMemoryManager(ushort usArea)
		{
			return this._comcon.GetMemoryManager(usArea);
		}

		// Token: 0x06001FAE RID: 8110 RVA: 0x0005759A File Offset: 0x0005659A
		public byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly)
		{
			return this._comcon.GetTaskIds(var, signDecl, bWriteOnly);
		}

		// Token: 0x06001FAF RID: 8111 RVA: 0x000575AA File Offset: 0x000565AA
		public IEnumerable<ITaskInfo> GetTasksReferencingSignature(ISignature sign)
		{
			return this._comcon.GetTasksReferencingSignature(sign);
		}

		// Token: 0x06001FB0 RID: 8112 RVA: 0x000575B8 File Offset: 0x000565B8
		public IEnumerable<IInstancePathInfo> InstancePaths(ISignature sign, bool bWithDerivedFunctionBlocks)
		{
			return this._comcon.InstancePaths(sign, bWithDerivedFunctionBlocks);
		}

		// Token: 0x06001FB1 RID: 8113 RVA: 0x000575C7 File Offset: 0x000565C7
		public string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures)
		{
			return this._comcon.InstancePaths(sign, out varInstances, out declaringSignatures);
		}

		// Token: 0x06001FB2 RID: 8114 RVA: 0x000575D7 File Offset: 0x000565D7
		public string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses)
		{
			return this._comcon.InstancePaths(sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
		}

		// Token: 0x06001FB3 RID: 8115 RVA: 0x000575ED File Offset: 0x000565ED
		public IDataLocation LocateAddress(out bool bError, IDirectVariable dirvar)
		{
			return this._comcon.LocateAddress(out bError, dirvar);
		}

		// Token: 0x06001FB4 RID: 8116 RVA: 0x000575FC File Offset: 0x000565FC
		public string[] SubElements(string stSignatureName, string stAccessPath, out bool bValid)
		{
			return this._comcon.SubElements(stSignatureName, stAccessPath, out bValid);
		}

		// Token: 0x06001FB5 RID: 8117 RVA: 0x0005760C File Offset: 0x0005660C
		public string[] SubElements(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, out bool bValid)
		{
			return this._comcon.SubElements(stSignatureName, nProjectHandle, guidObject, stAccessPath, out bValid);
		}

		// Token: 0x06001FB6 RID: 8118 RVA: 0x00057620 File Offset: 0x00056620
		public string[] SubElementsWithRange(IType type, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid)
		{
			return this._comcon.SubElementsWithRange(type, stAccessPath, nStartIndex, nEndIndex, out bValid);
		}

		// Token: 0x06001FB7 RID: 8119 RVA: 0x00057634 File Offset: 0x00056634
		public string[] SubElementsWithRange(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid)
		{
			return this._comcon.SubElementsWithRange(stSignatureName, nProjectHandle, guidObject, stAccessPath, nStartIndex, nEndIndex, out bValid);
		}

		// Token: 0x06001FB8 RID: 8120 RVA: 0x0005764C File Offset: 0x0005664C
		public IEnumerable<string> SubElementsWithRange(IType type, string stAccessPath, int nStartIndex, int nEndIndex, GUIHidingFlags eFlagsToConsider, out bool bValid)
		{
			return from sse in this.GetSubElementsHierarchy(type, stAccessPath, nStartIndex, nEndIndex, eFlagsToConsider, out bValid)
			select sse.MemberName;
		}

		// Token: 0x06001FB9 RID: 8121 RVA: 0x00057681 File Offset: 0x00056681
		public IEnumerable<ICodePosition> GetAccessPositionsOfVariable(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId)
		{
			return this._comcon.GetReferencePositionsOfPOUEx(nSignatureIdWithReferences, nSignatureIdWithVar, nVariableId);
		}

		// Token: 0x06001FBA RID: 8122 RVA: 0x00057694 File Offset: 0x00056694
		public IEnumerable<ISignatureMemberHierachyInfo> GetSubElementsHierarchy(IType type, string stAccessPath, int nStartIndex, int nEndIndex, GUIHidingFlags eFlagsToConsider, out bool bValid)
		{
			bValid = false;
			if (type == null)
			{
				return null;
			}
			IScope5 scope = CompilerProxy.CreateScope(this._comcon, Common.InvalidID);
			UserdefType userdefType = type as UserdefType;
			ISignatureMemberHierachyInfo[] stHelp;
			if (userdefType == null)
			{
				IHasEnumerableComponents hasEnumerableComponents = type as IHasEnumerableComponents;
				if (hasEnumerableComponents != null)
				{
					if (nStartIndex < 0)
					{
						nStartIndex = 0;
					}
					if (nEndIndex < 0 || nEndIndex < nStartIndex)
					{
						nEndIndex = 2147483646;
					}
					return from c in hasEnumerableComponents.GetComponents(scope, out bValid).Skip(nStartIndex).Take(nEndIndex - nStartIndex + 1).Distinct<string>()
					select new SignatureMemberHierachyInfo(stAccessPath + c, null, 0);
				}
				ISignatureMemberHierachyInfo[] array = (from c in ((_IType)type).GetComponents(scope, out bValid)
				select new SignatureMemberHierachyInfo(c, null, 0)).ToArray<SignatureMemberHierachyInfo>();
				stHelp = array;
			}
			else
			{
				stHelp = userdefType.GetComponents(scope, out bValid, eFlagsToConsider);
			}
			return CompiledApplicationQuery.CreateResultEnumerable(stAccessPath, nStartIndex, nEndIndex, stHelp);
		}

		// Token: 0x06001FBB RID: 8123 RVA: 0x00057788 File Offset: 0x00056788
		public IEnumerable<ISignatureMemberHierachyInfo> GetSubElementsHierarchy(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid)
		{
			bValid = false;
			if (!string.IsNullOrEmpty(stSignatureName))
			{
				if (stAccessPath == string.Empty || stAccessPath.StartsWith("."))
				{
					stAccessPath = stSignatureName + stAccessPath;
				}
				else
				{
					stAccessPath = stSignatureName + "." + stAccessPath;
				}
			}
			bool flag;
			_IExpression iexpression = CompilerProxy.CreateParser(stAccessPath, true).ParseSTOperand(out flag);
			if (iexpression == null)
			{
				return null;
			}
			bValid = !flag;
			if (!bValid)
			{
				return null;
			}
			IScope5 scope = CompilerProxy.CreateScope(this._comcon, Common.InvalidID);
			if (!string.IsNullOrEmpty(stSignatureName))
			{
				IList<ISignature> list = scope[stSignatureName];
				if (list != null && list.Count == 1 && list[0].POUType == Operator.VarGlobal)
				{
					scope.FindListBeforeVariable = true;
				}
			}
			CompilerProxy.TypifyExprement(iexpression, scope, this._comcon, null, false, false, null);
			if (iexpression.Type == null)
			{
				return null;
			}
			_IType itype = iexpression.Type.DeRefType as _IType;
			if (itype == null)
			{
				return null;
			}
			ISignatureMemberHierachyInfo[] array = (from stMember in itype.GetComponents(scope, out bValid)
			select new SignatureMemberHierachyInfo(stMember, null, 0)).ToArray<SignatureMemberHierachyInfo>();
			ISignatureMemberHierachyInfo[] stHelp = array;
			if (itype.Class == TypeClass.Userdef && guidObject != Guid.Empty)
			{
				UserdefType userdefType = itype as UserdefType;
				stHelp = userdefType.GetComponents(scope, out bValid, GUIHidingFlags.None);
				ISignature signature = userdefType.GetSignature(scope);
				if (signature != null && signature.ObjectGuid != guidObject)
				{
					_ISignature isignature = this._comcon[guidObject];
					if (isignature != null && ((signature.Name == isignature.Name && signature.POUType == Operator.Method) || CompiledApplicationQuery.IsBaseType(this._comcon, isignature, signature)))
					{
						stHelp = UserdefType.GetSignatureComponents(scope, isignature, GUIHidingFlags.None, 0);
					}
				}
			}
			return CompiledApplicationQuery.CreateResultEnumerable(stAccessPath, nStartIndex, nEndIndex, stHelp);
		}

		// Token: 0x06001FBC RID: 8124 RVA: 0x00057951 File Offset: 0x00056951
		private static bool IsBaseType(_ICompileContext comcon, ISignature signBase, ISignature sign)
		{
			while (sign.BaseSignatureId != -1)
			{
				if (sign.BaseSignatureId == signBase.Id)
				{
					return true;
				}
				sign = comcon[sign.BaseSignatureId];
			}
			return false;
		}

		// Token: 0x06001FBD RID: 8125 RVA: 0x0005797D File Offset: 0x0005697D
		public IEnumerable<uint> CreateChecksumListByVariableOffsetsForSignature(ISignature sign)
		{
			return CompilerProxy.CreateChecksumListByVariableOffsetsForSignature(sign, this._comcon.ApplicationGuid);
		}

		// Token: 0x06001FBE RID: 8126 RVA: 0x00057990 File Offset: 0x00056990
		public ISignature FindPrecompileSignature(ISignature compiledSignature)
		{
			ILMPreCompileSet precompileSetOfSignature = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPrecompileSetOfSignature(compiledSignature);
			ILMPreCompileSet preCompileSet = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.GetPreCompileSet(Guid.Empty);
			ISignature[] array;
			_IPreCompileContext ipreCompileContext;
			return CompilerProxy.GetPrecompileSignature(this._comcon, (_ISignature)compiledSignature, (_IPreCompileContext)precompileSetOfSignature, (_IPreCompileContext)preCompileSet, out array, out ipreCompileContext);
		}

		// Token: 0x06001FBF RID: 8127 RVA: 0x000579F0 File Offset: 0x000569F0
		private static IEnumerable<ISignatureMemberHierachyInfo> CreateResultEnumerable(string stAccessPath, int nStartIndex, int nEndIndex, ISignatureMemberHierachyInfo[] stHelp)
		{
			LList<ISignatureMemberHierachyInfo> llist = new LList<ISignatureMemberHierachyInfo>();
			LDictionary<string, ISignatureMemberHierachyInfo> ldictionary = new LDictionary<string, ISignatureMemberHierachyInfo>();
			int num = 0;
			int num2 = stHelp.Length - 1;
			if (nStartIndex >= num && nEndIndex <= num2 && nStartIndex <= nEndIndex)
			{
				num = nStartIndex;
				num2 = nEndIndex;
			}
			for (int i = num2; i >= num; i--)
			{
				if (stHelp[i].EmptyLevel)
				{
					llist.Add(stHelp[i]);
				}
				else if (!ldictionary.ContainsKey(stHelp[i].MemberName))
				{
					llist.Add(new SignatureMemberHierachyInfo(stAccessPath + stHelp[i].MemberName, stHelp[i].Signature, stHelp[i].Level));
					ldictionary.Add(stHelp[i].MemberName, stHelp[i]);
				}
			}
			llist.Reverse();
			return llist;
		}

		// Token: 0x06001FC0 RID: 8128 RVA: 0x00057A9F File Offset: 0x00056A9F
		public IEnumerable<IInstancePathInfoWithAttribute> InstancePathsForAttribute(string stAttributeName, Guid gdApplication)
		{
			return this.InstancePathsForAttribute(stAttributeName, gdApplication, EInstancePathsLookupFlag.None);
		}

		// Token: 0x06001FC1 RID: 8129 RVA: 0x00057AAC File Offset: 0x00056AAC
		public IEnumerable<IInstancePathInfoWithAttribute> InstancePathsForAttribute(string stAttributeName, Guid gdApplication, EInstancePathsLookupFlag eLookupFlags)
		{
			ILMCompiledApplicationSet compiledApplicationSet = APEnvironmentFacade.Instance.LMServiceProvider.CompileService.GetCompiledApplicationSet(gdApplication);
			if (compiledApplicationSet == null)
			{
				return Array.Empty<IInstancePathInfoWithAttribute>();
			}
			IEnumerable<ISignature> enumerable = from sign in compiledApplicationSet.AllSignaturesFlat
			where sign.HasAttribute(stAttributeName)
			select sign;
			bool bWithNamespace = EInstancePathsLookupFlag.WithNamespace == (eLookupFlags & EInstancePathsLookupFlag.WithNamespace);
			bool bWithStackVariables = EInstancePathsLookupFlag.WithStackVariables == (eLookupFlags & EInstancePathsLookupFlag.WithStackVariables);
			bool bWithDerivedClasses = EInstancePathsLookupFlag.WithDerivedFunctionBlocks == (eLookupFlags & EInstancePathsLookupFlag.WithDerivedFunctionBlocks);
			List<CompiledApplicationQuery.InstancePathInfo> list = new List<CompiledApplicationQuery.InstancePathInfo>();
			foreach (ISignature signature in enumerable)
			{
				string attributeValue = signature.GetAttributeValue(stAttributeName);
				IVariable[] array2;
				ISignature[] array3;
				string[] array = this.InstancePaths(signature, out array2, out array3, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
				for (int i = 0; i < array.Length; i++)
				{
					list.Add(new CompiledApplicationQuery.InstancePathInfo(array[i], array2[i], array3[i], attributeValue));
				}
			}
			return list;
		}

		// Token: 0x0400063F RID: 1599
		private readonly _ICompileContext _comcon;

		// Token: 0x020002C1 RID: 705
		[DebuggerDisplay("{InstancePath} DeclaringSignature: {DeclaringSignature.OrgName}, VarInstance: {VarInstance.Name}, AttributeValue: {AttributeValue}")]
		private sealed class InstancePathInfo : IInstancePathInfoWithAttribute, IInstancePathInfo
		{
			// Token: 0x06002C17 RID: 11287 RVA: 0x00074617 File Offset: 0x00073617
			internal InstancePathInfo(string stInstancePath, IVariable varInstance, ISignature declaringSignature, string stAttributeValue)
			{
				this.InstancePath = stInstancePath;
				this.VarInstance = varInstance;
				this.DeclaringSignature = declaringSignature;
				this.AttributeValue = stAttributeValue;
			}

			// Token: 0x17000C23 RID: 3107
			// (get) Token: 0x06002C18 RID: 11288 RVA: 0x0007463C File Offset: 0x0007363C
			// (set) Token: 0x06002C19 RID: 11289 RVA: 0x00074644 File Offset: 0x00073644
			public string InstancePath { get; private set; }

			// Token: 0x17000C24 RID: 3108
			// (get) Token: 0x06002C1A RID: 11290 RVA: 0x0007464D File Offset: 0x0007364D
			// (set) Token: 0x06002C1B RID: 11291 RVA: 0x00074655 File Offset: 0x00073655
			public IVariable VarInstance { get; private set; }

			// Token: 0x17000C25 RID: 3109
			// (get) Token: 0x06002C1C RID: 11292 RVA: 0x0007465E File Offset: 0x0007365E
			// (set) Token: 0x06002C1D RID: 11293 RVA: 0x00074666 File Offset: 0x00073666
			public ISignature DeclaringSignature { get; private set; }

			// Token: 0x17000C26 RID: 3110
			// (get) Token: 0x06002C1E RID: 11294 RVA: 0x0007466F File Offset: 0x0007366F
			// (set) Token: 0x06002C1F RID: 11295 RVA: 0x00074677 File Offset: 0x00073677
			public string AttributeValue { get; private set; }
		}
	}
}
