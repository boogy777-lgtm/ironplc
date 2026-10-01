using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Compiler35220.Messaging;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000331 RID: 817
	internal sealed class ObjectsToTypifyDetector
	{
		// Token: 0x17000811 RID: 2065
		// (get) Token: 0x0600313E RID: 12606 RVA: 0x000BD4D8 File Offset: 0x000BB6D8
		// (set) Token: 0x0600313F RID: 12607 RVA: 0x000BD4E0 File Offset: 0x000BB6E0
		public _ICompileContext Comcon { get; set; }

		// Token: 0x17000812 RID: 2066
		// (get) Token: 0x06003140 RID: 12608 RVA: 0x000BD4EC File Offset: 0x000BB6EC
		// (set) Token: 0x06003141 RID: 12609 RVA: 0x000BD4F4 File Offset: 0x000BB6F4
		public bool OnlineChange { get; set; }

		// Token: 0x17000813 RID: 2067
		// (get) Token: 0x06003142 RID: 12610 RVA: 0x000BD500 File Offset: 0x000BB700
		// (set) Token: 0x06003143 RID: 12611 RVA: 0x000BD508 File Offset: 0x000BB708
		public bool BootProject { get; set; }

		// Token: 0x17000814 RID: 2068
		// (get) Token: 0x06003144 RID: 12612 RVA: 0x000BD514 File Offset: 0x000BB714
		// (set) Token: 0x06003145 RID: 12613 RVA: 0x000BD51C File Offset: 0x000BB71C
		public _IPreCompileContext Precomp { get; set; }

		// Token: 0x17000815 RID: 2069
		// (get) Token: 0x06003146 RID: 12614 RVA: 0x000BD528 File Offset: 0x000BB728
		// (set) Token: 0x06003147 RID: 12615 RVA: 0x000BD530 File Offset: 0x000BB730
		public _IPreCompileContext PrecompPool { get; set; }

		// Token: 0x06003148 RID: 12616 RVA: 0x000BD53C File Offset: 0x000BB73C
		private ObjectsToTypifyDetector(_ICompileContext comcon, bool bOnlineChange, bool bBootProject, _IPreCompileContext precomp, _IPreCompileContext precompPool)
		{
			this.Comcon = comcon;
			this.OnlineChange = bOnlineChange;
			this.BootProject = bBootProject;
			this.Precomp = precomp;
			this.PrecompPool = precompPool;
		}

		// Token: 0x06003149 RID: 12617 RVA: 0x000BD56C File Offset: 0x000BB76C
		public static bool \u0001(_ICompileContext \u0002, bool \u0003, bool \u0004, _IPreCompileContext \u0005, _IPreCompileContext \u0006)
		{
			return new ObjectsToTypifyDetector(\u0002, \u0003, \u0004, \u0005, \u0006).\u0003();
		}

		// Token: 0x0600314A RID: 12618 RVA: 0x000BD580 File Offset: 0x000BB780
		private bool \u0003()
		{
			foreach (_ICompiledPOU icompiledPOU in this.Comcon.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
			{
				icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
			}
			LList<_ISignature> u = new LList<_ISignature>();
			LList<_ISignature> llist = new LList<_ISignature>();
			LList<_ISignature> llist2 = new LList<_ISignature>();
			if (this.Comcon.DefineChanged(this.Precomp) || !this.OnlineChange)
			{
				return true;
			}
			if (!this.Comcon.LibraryParamTablesEqual(this.Precomp))
			{
				return true;
			}
			if (!this.Comcon.LibraryListsEqual(this.Precomp, this.PrecompPool))
			{
				return true;
			}
			foreach (_ICompilerMessage icompilerMessage in Messages.\u0001(this.Comcon))
			{
				if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
				{
					return true;
				}
			}
			if (!this.Comcon.TaskList.IsEqual(this.Precomp.TaskList))
			{
				return true;
			}
			if (this.\u0004())
			{
				return true;
			}
			foreach (_ISignature u2 in this.Comcon.AllFlat)
			{
				if (this.\u0001(u, llist, llist2, u2))
				{
					return true;
				}
			}
			foreach (_ICompiledPOU icompiledPOU2 in this.Comcon.GetAllCompiledPOUsEx().OfType<_ICompiledPOU>())
			{
				icompiledPOU2.SetFlag(CompiledPOUFlags.ToTypify, false);
			}
			this.\u0002(llist);
			this.\u0003(llist2);
			this.\u0001(u);
			this.\u0004(u);
			return false;
		}

		// Token: 0x0600314B RID: 12619 RVA: 0x000BD77C File Offset: 0x000BB97C
		private bool \u0001(LList<_ISignature> \u0002, LList<_ISignature> \u0003, LList<_ISignature> \u0004, _ISignature \u0005)
		{
			if (\u0005.ObjectGuid == Guid.Empty || \u0005.GetFlag(SignatureFlag.Generated) || (\u0005.GetFlag(SignatureFlag.SuperGlobal) && !\u0005.HasAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL)))
			{
				return false;
			}
			ISignature[] u;
			_IPreCompileContext ipreCompileContext;
			_ISignature isignature = Helper.\u0001(this.Comcon, \u0005, this.Precomp, this.PrecompPool, out u, out ipreCompileContext);
			if ((isignature == null || (\u0005.Checksum != 0U && isignature.Checksum != \u0005.Checksum) || (\u0005.Checksum == 0U && isignature.TimeStamp != \u0005.TimeStamp)) && this.\u0001(\u0002, \u0003, \u0004, \u0005, isignature))
			{
				return true;
			}
			if (ObjectsToTypifyDetector.\u0001(\u0005, u))
			{
				return true;
			}
			this.\u0001(\u0003, \u0005);
			return false;
		}

		// Token: 0x0600314C RID: 12620 RVA: 0x000BD844 File Offset: 0x000BBA44
		private bool \u0001(LList<_ISignature> \u0002, LList<_ISignature> \u0003, LList<_ISignature> \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			if (\u0006 == null)
			{
				return true;
			}
			if (\u0005.GetFlag(SignatureFlag.InhibitOnlineChange))
			{
				return true;
			}
			if (\u0005.ChecksumNoInit == \u0006.ChecksumNoInit && ObjectsToTypifyDetector.\u0001(\u0002, \u0003, \u0004, \u0005, \u0006))
			{
				return true;
			}
			if (ObjectsToTypifyDetector.\u0001(\u0006, \u0005))
			{
				return true;
			}
			\u0002.Add(\u0005);
			if ((\u0005.GetFlagInternal(SignatureFlagInternal.ContainsInstanceVars) || \u0006.GetFlagInternal(SignatureFlagInternal.ContainsInstanceVars)) && \u0005.InstanceLocalsChanged(\u0006))
			{
				\u0002.Add(this.Comcon[\u0005.ParentSignatureId]);
			}
			return false;
		}

		// Token: 0x0600314D RID: 12621 RVA: 0x000BD8DC File Offset: 0x000BBADC
		private static bool \u0001(LList<_ISignature> \u0002, LList<_ISignature> \u0003, LList<_ISignature> \u0004, _ISignature \u0005, _ISignature \u0006)
		{
			if (\u0005.Constant.Length == 0 && !\u0005.GetFlag(SignatureFlag.Enum))
			{
				\u0003.Add(\u0005);
				if (ObjectsToTypifyDetector.\u0001(\u0005, \u0006))
				{
					\u0004.Add(\u0005);
				}
			}
			else
			{
				if (ObjectsToTypifyDetector.\u0001(\u0006, \u0005))
				{
					return true;
				}
				\u0002.Add(\u0005);
			}
			return false;
		}

		// Token: 0x0600314E RID: 12622 RVA: 0x000BD92C File Offset: 0x000BBB2C
		private void \u0001(LList<_ISignature> \u0002, _ISignature \u0003)
		{
			Guid guid;
			if (\u0003.HasAttribute("init_related_code") && Guid.TryParse(\u0003.GetAttributeValue("init_related_code"), out guid))
			{
				_ISignature isignature = this.Comcon.GetSignature(Guid.Parse(\u0003.GetAttributeValue("init_related_code"))) as _ISignature;
				if (isignature != null)
				{
					\u0002.Add(isignature);
				}
			}
		}

		// Token: 0x0600314F RID: 12623 RVA: 0x000BD988 File Offset: 0x000BBB88
		private static bool \u0001(_ISignature \u0002, ISignature \u0003)
		{
			bool flag = \u0002.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY) && \u0003.HasAttribute(CompileAttributes.ATTRIBUTE_QUALIFIED_ONLY);
			return \u0002.POUType == Operator.VarGlobal && !flag;
		}

		// Token: 0x06003150 RID: 12624 RVA: 0x000BD9C4 File Offset: 0x000BBBC4
		private bool \u0004()
		{
			if (Helper.\u0001(new _IPreCompileContext[]
			{
				this.Precomp,
				this.PrecompPool
			}) == ((_ICompileContext2)this.Comcon).PrecompileContextNamesChecksum)
			{
				return false;
			}
			foreach (ISignature signature in ((ILMPreCompileSet)this.Precomp).SignatureSet)
			{
				if (!signature.GetFlag(SignatureFlag.Generated) && !signature.GetFlag(SignatureFlag.SuperGlobal))
				{
					_ISignature isignature = this.Comcon[((_ISignature)signature).GetSearchName(this.Comcon)];
					if (isignature == null || isignature.Name != signature.Name)
					{
						isignature = this.Comcon[signature.Name];
					}
					if ((isignature == null || !isignature.GetFlag(SignatureFlag.Generated)) && isignature == null)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06003151 RID: 12625 RVA: 0x000BDAC8 File Offset: 0x000BBCC8
		private static bool \u0001(ISignature \u0002, ISignature \u0003)
		{
			ISignatureWithOptionalInputs signatureWithOptionalInputs = \u0003 as ISignatureWithOptionalInputs;
			ISignatureWithOptionalInputs signatureWithOptionalInputs2 = \u0002 as ISignatureWithOptionalInputs;
			return signatureWithOptionalInputs != null && signatureWithOptionalInputs2 != null && signatureWithOptionalInputs2.ChecksumOptionalInputs != signatureWithOptionalInputs.ChecksumOptionalInputs;
		}

		// Token: 0x06003152 RID: 12626 RVA: 0x000BDAFC File Offset: 0x000BBCFC
		private void \u0001(LList<_ISignature> \u0002)
		{
			foreach (ICompiledPOU4 compiledPOU in this.Comcon.GetAllCompiledPOUsEx())
			{
				_ICompiledPOU icompiledPOU = (_ICompiledPOU)compiledPOU;
				if (!(icompiledPOU.ObjectGuid == Guid.Empty))
				{
					_ICompiledPOU icompiledPOU2 = Helper.\u0001(this.Comcon, icompiledPOU, this.Precomp, this.PrecompPool);
					if (icompiledPOU2 == null || (icompiledPOU.Checksum != 0U && icompiledPOU2.Checksum != icompiledPOU.Checksum) || (icompiledPOU.Checksum == 0U && icompiledPOU2.TimeStamp != icompiledPOU.TimeStamp))
					{
						icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
						_ISignature isignature = this.Comcon.GetSignatureById(icompiledPOU.SignatureId) as _ISignature;
						if (isignature != null && isignature.POUType == Operator.Method)
						{
							isignature = (this.Comcon.GetSignatureById(isignature.ParentSignatureId) as _ISignature);
						}
						if (isignature != null && isignature.HasFlag((SignatureFlag)((ulong)-2147483648)))
						{
							\u0002.Add(isignature);
						}
					}
					else if (icompiledPOU.GetFlag(CompiledPOUFlags.ContainsDirVarAccess) && !this.BootProject)
					{
						icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
					}
					else
					{
						_ISignature isignature2 = this.Comcon[icompiledPOU.SignatureId];
						if (isignature2 != null && isignature2.GetFlag(SignatureFlag.Generated))
						{
							icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
						}
					}
				}
			}
		}

		// Token: 0x06003153 RID: 12627 RVA: 0x000BDC74 File Offset: 0x000BBE74
		private void \u0002(LList<_ISignature> \u0002)
		{
			foreach (ISignature signature in \u0002)
			{
				_ICompiledPOU icompiledPOU = this.Comcon._GetCompiledPOUById(signature.Id);
				if (icompiledPOU == null)
				{
					Guid guid;
					if (signature.POUType == Operator.FunctionBlock)
					{
						ISignature subSignature = signature.GetSubSignature(IdentifierConstants.MainSignatureName);
						if (subSignature != null)
						{
							icompiledPOU = this.Comcon._GetCompiledPOUById(subSignature.Id);
						}
						ISignature subSignature2 = signature.GetSubSignature(IdentifierConstants.InitMethodName);
						if (subSignature2 != null)
						{
							_ICompiledPOU icompiledPOU2 = this.Comcon._GetCompiledPOUById(subSignature2.Id);
							if (icompiledPOU2 != null)
							{
								icompiledPOU2.SetFlag(CompiledPOUFlags.ToTypify, true);
							}
						}
					}
					else if (signature.HasAttribute("init_related_code") && Guid.TryParse(signature.GetAttributeValue("init_related_code"), out guid))
					{
						icompiledPOU = (this.Comcon.GetCompiledPOU(Guid.Parse(signature.GetAttributeValue("init_related_code"))) as _ICompiledPOU);
					}
					if (icompiledPOU == null)
					{
						continue;
					}
				}
				icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
			}
		}

		// Token: 0x06003154 RID: 12628 RVA: 0x000BDD88 File Offset: 0x000BBF88
		private void \u0003(LList<_ISignature> \u0002)
		{
			foreach (ISignature u in \u0002)
			{
				ObjectsToTypifyDetector.\u0001(this.Comcon, u, new ObjectsToTypifyDetector.CallersCompiledPOUProcessor(ObjectsToTypifyDetector.<>c.<>9.\u0001));
			}
		}

		// Token: 0x06003155 RID: 12629 RVA: 0x000BDDF4 File Offset: 0x000BBFF4
		private static void \u0001(_ICompileContext \u0002, ISignature \u0003, ObjectsToTypifyDetector.CallersCompiledPOUProcessor \u0004)
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

		// Token: 0x06003156 RID: 12630 RVA: 0x000BDE58 File Offset: 0x000BC058
		private static bool \u0001(_ISignature \u0002, ISignature[] \u0003)
		{
			if (\u0003 == null)
			{
				return false;
			}
			HashSet<Guid> hashSet = new HashSet<Guid>();
			foreach (object obj in \u0002._SubSignatures)
			{
				ISignature signature = (ISignature)obj;
				if (signature.ObjectGuid != Guid.Empty && !(signature.ObjectGuid == Guid.Empty))
				{
					hashSet.Add(signature.ObjectGuid);
				}
			}
			foreach (ISignature signature2 in \u0003)
			{
				if (signature2.ObjectGuid == Guid.Empty || !hashSet.Contains(signature2.ObjectGuid))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003157 RID: 12631 RVA: 0x000BDF28 File Offset: 0x000BC128
		private void \u0004(LList<_ISignature> \u0002)
		{
			LDictionary<int, int> u = new LDictionary<int, int>();
			this.\u0001(\u0002, u);
		}

		// Token: 0x06003158 RID: 12632 RVA: 0x000BDF44 File Offset: 0x000BC144
		private static bool \u0001(ICompiledType \u0002, int \u0003)
		{
			switch (\u0002.Class)
			{
			case TypeClass.Pointer:
			case TypeClass.Reference:
			case TypeClass.Array:
				return ObjectsToTypifyDetector.\u0001(\u0002.BaseType, \u0003);
			case TypeClass.Userdef:
				return (\u0002 as _IUserdefType).SignatureId == \u0003;
			}
			return false;
		}

		// Token: 0x06003159 RID: 12633 RVA: 0x000BDF9C File Offset: 0x000BC19C
		private void \u0001(LList<_ISignature> \u0002, LDictionary<int, int> \u0003)
		{
			while (\u0002.Count != 0)
			{
				_ISignature isignature = \u0002[0];
				\u0002.RemoveAt(0);
				LList<int> llist = new LList<int>();
				while (\u0003.ContainsKey(isignature.Id))
				{
					if (\u0002.Count == 0)
					{
						return;
					}
					isignature = \u0002[0];
					\u0002.RemoveAt(0);
				}
				\u0003.Add(isignature.Id, isignature.Id);
				_ICompiledPOU icompiledPOU = this.Comcon.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
				if (icompiledPOU != null)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
				}
				foreach (int num in isignature.CallerIds)
				{
					_ICompiledPOU icompiledPOU2 = this.Comcon.GetCompiledPOUById(num) as _ICompiledPOU;
					if (icompiledPOU2 != null)
					{
						icompiledPOU2.SetFlag(CompiledPOUFlags.ToTypify, true);
					}
					llist.Add(num);
				}
				foreach (int num2 in isignature.ReferencerIds)
				{
					_ICompiledPOU icompiledPOU3 = this.Comcon.GetCompiledPOUById(num2) as _ICompiledPOU;
					if (icompiledPOU3 != null)
					{
						icompiledPOU3.SetFlag(CompiledPOUFlags.ToTypify, true);
					}
					llist.Add(num2);
				}
				this.\u0001(\u0002, isignature, llist);
				this.\u0001(isignature, llist);
				this.\u0001(\u0002, llist);
				this.\u0002(\u0002, isignature);
			}
		}

		// Token: 0x0600315A RID: 12634 RVA: 0x000BE0DC File Offset: 0x000BC2DC
		private void \u0002(LList<_ISignature> \u0002, _ISignature \u0003)
		{
			foreach (int nId in \u0003.DeclarerIds)
			{
				_ISignature isignature = this.Comcon[nId];
				if (isignature != null)
				{
					\u0002.Add(isignature);
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (ivariable.Type != null && ObjectsToTypifyDetector.\u0001(ivariable.CompiledType, \u0003.Id))
						{
							foreach (ICrossReference crossReference in ivariable.CrossReferences)
							{
								_ICompiledPOU icompiledPOU = this.Comcon.GetCompiledPOUById(crossReference.CodeId) as _ICompiledPOU;
								if (icompiledPOU != null)
								{
									icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0600315B RID: 12635 RVA: 0x000BE1CC File Offset: 0x000BC3CC
		private void \u0001(LList<_ISignature> \u0002, LList<int> \u0003)
		{
			foreach (int nId in \u0003)
			{
				_ISignature isignature = this.Comcon[nId];
				if (isignature != null)
				{
					if (isignature.Name == IdentifierConstants.MainSignatureName)
					{
						isignature = this.Comcon[isignature.ParentSignatureId];
					}
					if (isignature.GetFlag((SignatureFlag)((ulong)-2147483648)))
					{
						\u0002.Add(isignature);
					}
				}
			}
		}

		// Token: 0x0600315C RID: 12636 RVA: 0x000BE258 File Offset: 0x000BC458
		private void \u0001(_ISignature \u0002, LList<int> \u0003)
		{
			foreach (object obj in \u0002._SubSignatures)
			{
				_ISignature isignature = (_ISignature)obj;
				_ICompiledPOU icompiledPOU = this.Comcon.GetCompiledPOUById(isignature.Id) as _ICompiledPOU;
				if (icompiledPOU != null)
				{
					icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
				}
				\u0003.Add(isignature.Id);
			}
		}

		// Token: 0x0600315D RID: 12637 RVA: 0x000BE2E0 File Offset: 0x000BC4E0
		private void \u0001(LList<_ISignature> \u0002, _ISignature \u0003, LList<int> \u0004)
		{
			if (\u0003.POUType == Operator.Program || \u0003.POUType == Operator.FunctionBlock || \u0003.POUType == Operator.VarGlobal)
			{
				foreach (_IVariable ivariable in \u0003.AllVariables)
				{
					foreach (ICrossReference crossReference in ivariable.CrossReferences)
					{
						_ICompiledPOU icompiledPOU = this.Comcon.GetCompiledPOUById(crossReference.CodeId) as _ICompiledPOU;
						if (icompiledPOU != null)
						{
							icompiledPOU.SetFlag(CompiledPOUFlags.ToTypify, true);
						}
						\u0004.Add(crossReference.CodeId);
						this.\u0001(\u0002, ivariable, crossReference);
					}
				}
			}
		}

		// Token: 0x0600315E RID: 12638 RVA: 0x000BE3A0 File Offset: 0x000BC5A0
		private void \u0001(LList<_ISignature> \u0002, _IVariable \u0003, ICrossReference \u0004)
		{
			if (\u0003.GetFlag(VarFlag.Constant) || \u0003.GetFlag(VarFlag.ReplacedConstant))
			{
				_ISignature isignature = this.Comcon[\u0004.CodeId];
				if (isignature != null)
				{
					\u0002.Add(isignature);
				}
			}
		}

		// Token: 0x04000956 RID: 2390
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x04000957 RID: 2391
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000958 RID: 2392
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000959 RID: 2393
		[CompilerGenerated]
		private _IPreCompileContext \u0001;

		// Token: 0x0400095A RID: 2394
		[CompilerGenerated]
		private _IPreCompileContext \u0002;

		// Token: 0x02000332 RID: 818
		// (Invoke) Token: 0x06003160 RID: 12640
		public delegate void CallersCompiledPOUProcessor(_ICompiledPOU cpou);
	}
}
