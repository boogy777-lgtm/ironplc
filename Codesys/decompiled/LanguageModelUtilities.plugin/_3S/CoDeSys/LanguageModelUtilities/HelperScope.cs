using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class HelperScope : IPrecompileScope5, IPrecompileScope4, IPrecompileScope3, IPrecompileScope2, IPrecompileScope
	{
		private static Guid GUID_SYSTEM_APPLICATION = new Guid("{e0c003b2-1edd-477a-9148-e4b7c6a4e203}");

		private ISignature4 m_signlocal;

		private ISignature4 m_signmethod;

		private IPreCompileContext9 m_precomPool;

		private IPreCompileContext9 m_precomLocal;

		private bool m_bSearchLocalSignatures = true;

		private Guid m_guidApplication = Guid.Empty;

		private ILibraryTable m_libraryTable;

		private bool m_bLocalScope;

		private int m_nPointerSize = 4;

		public int PointerSize => m_nPointerSize;

		public bool LocalScope
		{
			get
			{
				return m_bLocalScope;
			}
			set
			{
				m_bLocalScope = value;
			}
		}

		public ISignature this[Guid guid] => m_precomLocal.GetSignature(guid);

		public virtual ISignature4[] this[string stName]
		{
			get
			{
				LList<ISignature4> val = new LList<ISignature4>();
				if (m_precomLocal != null)
				{
					if (m_precomLocal.GetSignature(stName) is ISignature4 signature)
					{
						val.Add(signature);
						return val.ToArray();
					}
					foreach (IPreCompileContext9 visibleLibrary in m_libraryTable.GetVisibleLibraries(m_precomLocal))
					{
						if (visibleLibrary.GetSignature(stName) is ISignature4 signature2)
						{
							val.Add(signature2);
						}
					}
				}
				if (((IEnumerable<ISignature4>)val).Count() > 0)
				{
					return val.ToArray();
				}
				if (m_precomPool != null)
				{
					if (m_precomPool.GetSignature(stName) is ISignature4 signature3)
					{
						val.Add(signature3);
						return val.ToArray();
					}
					foreach (IPreCompileContext9 visibleLibrary2 in m_libraryTable.GetVisibleLibraries(m_precomPool))
					{
						if (visibleLibrary2.GetSignature(stName) is ISignature4 signature4)
						{
							val.Add(signature4);
						}
					}
				}
				return val.ToArray();
			}
		}

		public ISignature LocalSignature
		{
			get
			{
				return m_signlocal;
			}
			set
			{
				m_signlocal = value as ISignature4;
			}
		}

		public int Id => -1;

		public Guid ApplicationGuid
		{
			get
			{
				return m_precomLocal.ApplicationGuid;
			}
			set
			{
			}
		}

		public static HelperScope _CreateGlobalScope(ISignature4 localSignature, IPreCompileContext9 localApplication)
		{
			return new HelperScope(localSignature, localApplication, _GetPoolContext(APEnvironmentFacade.Instance.LanguageModelMgr));
		}

		private static IPreCompileContext9 _GetPoolContext(ILanguageModelManager21 lmm)
		{
			IPreCompileContext[] array = lmm.AllPreCompileContexts(bWithDevices: false, bWithLibraries: true);
			foreach (IPreCompileContext preCompileContext in array)
			{
				if (string.IsNullOrEmpty(preCompileContext.LibraryPath) && preCompileContext.ApplicationGuid == Guid.Empty)
				{
					return preCompileContext as IPreCompileContext9;
				}
			}
			return null;
		}

		public HelperScope(ISignature4 signLocal, IPreCompileContext9 precomLocal, IPreCompileContext9 precomPool)
		{
			m_signlocal = signLocal;
			m_precomPool = precomPool;
			m_precomLocal = precomLocal;
			m_libraryTable = precomLocal.LibraryTable;
			if (m_signlocal != null && m_signlocal.ParentObjectGuid != Guid.Empty)
			{
				m_signmethod = m_signlocal;
				if (precomLocal != null)
				{
					m_signlocal = precomLocal.GetSignature(m_signlocal.ParentObjectGuid) as ISignature4;
				}
				else
				{
					m_signlocal = precomPool.GetSignature(m_signlocal.ParentObjectGuid) as ISignature4;
				}
			}
		}

		internal HelperScope(int nPointerSize, ILibraryTable libraryTable, ISignature4 signLocal, IPreCompileContext9 precomLocal, IPreCompileContext9 precomPool)
		{
			m_signlocal = signLocal;
			m_precomPool = precomPool;
			m_precomLocal = precomLocal;
			m_libraryTable = libraryTable;
			if (m_signlocal != null && m_signlocal.ParentObjectGuid != Guid.Empty)
			{
				m_signmethod = m_signlocal;
				if (precomLocal != null)
				{
					m_signlocal = precomLocal.GetSignature(m_signlocal.ParentObjectGuid) as ISignature4;
				}
				else
				{
					m_signlocal = precomPool.GetSignature(m_signlocal.ParentObjectGuid) as ISignature4;
				}
			}
			m_nPointerSize = nPointerSize;
		}

		private IPreCompileContext9 GetSystemContext()
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(GUID_SYSTEM_APPLICATION) as IPreCompileContext9;
		}

		private IPreCompileContext9 GetLibraryContext(string stLibraryPath)
		{
			IPreCompileContext[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: false, bWithLibraries: true);
			foreach (IPreCompileContext preCompileContext in array)
			{
				if (preCompileContext.LibraryPath == stLibraryPath)
				{
					return preCompileContext as IPreCompileContext9;
				}
			}
			return null;
		}

		private IPreCompileContext9 GetPoolContext()
		{
			IPreCompileContext[] array = APEnvironmentFacade.Instance.LanguageModelMgr.AllPreCompileContexts(bWithDevices: false, bWithLibraries: true);
			foreach (IPreCompileContext preCompileContext in array)
			{
				if (string.IsNullOrEmpty(preCompileContext.LibraryPath) && preCompileContext.ApplicationGuid == Guid.Empty)
				{
					return preCompileContext as IPreCompileContext9;
				}
			}
			return null;
		}

		public HelperScope CreateUserdefScope(ISignature4 newLocalSignature)
		{
			IPreCompileContext9 precomLocal = m_precomLocal;
			IPreCompileContext9 precomPool = m_precomPool;
			if (newLocalSignature != null && !string.IsNullOrEmpty(newLocalSignature.LibraryPath))
			{
				precomLocal = GetLibraryContext(newLocalSignature.LibraryPath);
				precomPool = null;
			}
			return new HelperScope(m_nPointerSize, m_libraryTable, newLocalSignature, precomLocal, precomPool);
		}

		public HelperScope CreateLibraryScope(IPreCompileContext9 precomLib)
		{
			return new HelperScope(m_nPointerSize, m_libraryTable, null, precomLib, null);
		}

		public HelperScope CreateGlobalScope()
		{
			return new HelperScope(m_nPointerSize, m_libraryTable, null, m_precomLocal, m_precomPool);
		}

		public ISignature FindSignature(IUserdefType udtype)
		{
			if (udtype == null)
			{
				return null;
			}
			ISignature[] array = FindSignature((udtype as IUserdefType2).NameExpression);
			ISignature[] array2 = array;
			if (array2 != null && array2.Count() == 1)
			{
				return array2[0];
			}
			return null;
		}

		public ISignature FindSignature(IEnumType etype)
		{
			if (etype == null)
			{
				return null;
			}
			ISignature[] array = FindSignature(etype.Name);
			ISignature[] array2 = array;
			if (array2.Count() == 1)
			{
				return array2[0];
			}
			return null;
		}

		public bool IsEqual(IUserdefType ud1, IUserdefType ud2)
		{
			string a = (ud1 as IUserdefType2).NameExpression.ToString();
			string b = (ud2 as IUserdefType2).NameExpression.ToString();
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		public ISignature4[] FindSignature(IExpression exp)
		{
			if (exp is IVariableExpression)
			{
				return FindSignature(exp.ToString());
			}
			if (exp is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = exp as ICompoAccessExpression;
				return FindScope(compoAccessExpression.Left)?.FindSignature(compoAccessExpression.Right);
			}
			if (exp is ISystemScopeExpression)
			{
				return new HelperScope(null, GetSystemContext(), null).FindSignature((exp as ISystemScopeExpression).Base);
			}
			return null;
		}

		public HelperScope FindScope(string stHelp)
		{
			IPreCompileContext9 libraryContextByNamespace = m_libraryTable.GetLibraryContextByNamespace(stHelp, m_precomLocal);
			if (libraryContextByNamespace == null)
			{
				return null;
			}
			return CreateLibraryScope(libraryContextByNamespace);
		}

		public HelperScope FindScope(IExpression exp)
		{
			if (exp is IVariableExpression)
			{
				IPreCompileContext9 libraryContextByNamespace = m_libraryTable.GetLibraryContextByNamespace(exp.ToString(), m_precomLocal);
				if (libraryContextByNamespace == null)
				{
					return null;
				}
				return CreateLibraryScope(libraryContextByNamespace);
			}
			if (exp is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = exp as ICompoAccessExpression;
				return FindScope(compoAccessExpression.Left)?.FindScope(compoAccessExpression.Right);
			}
			return null;
		}

		public virtual IVariable FindVariableLocal(string stName, out ISignature signWithVar)
		{
			signWithVar = null;
			IVariable variable = null;
			if (m_signmethod != null)
			{
				variable = m_signmethod[stName];
				if (variable != null)
				{
					signWithVar = m_signmethod;
					return variable;
				}
			}
			ISignature4 signature = m_signlocal;
			LDictionary<ISignature, ISignature> val = new LDictionary<ISignature, ISignature>();
			while (signature != null && !val.ContainsKey((ISignature)signature))
			{
				val.Add((ISignature)signature, (ISignature)signature);
				variable = signature[stName];
				if (variable != null)
				{
					signWithVar = signature;
					return variable;
				}
				if (signature.BaseExpression == null)
				{
					break;
				}
				ISignature[] array = FindSignature(signature.BaseExpression);
				ISignature[] array2 = array;
				if (array2 != null && array2.Count() == 1)
				{
					signature = array2[0] as ISignature4;
				}
			}
			return null;
		}

		public virtual IVariable[] FindVariableGlobal(string stName, out ISignature[] signsWithVar)
		{
			LList<ISignature> val = new LList<ISignature>();
			LList<IVariable> val2 = new LList<IVariable>();
			signsWithVar = null;
			if (m_precomLocal != null)
			{
				ISignature[] gVLSignatures = m_precomLocal.GVLSignatures;
				foreach (ISignature signature in gVLSignatures)
				{
					if (!signature.HasAttribute("qualified_only"))
					{
						IVariable variable = signature[stName];
						if (variable != null)
						{
							val.Add(signature);
							val2.Add(variable);
						}
					}
				}
			}
			if (val2.get_Count() == 0)
			{
				foreach (IPreCompileContext9 visibleLibrary in m_libraryTable.GetVisibleLibraries(m_precomLocal))
				{
					ISignature[] gVLSignatures = visibleLibrary.GVLSignatures;
					foreach (ISignature signature2 in gVLSignatures)
					{
						if (!signature2.HasAttribute("qualified_only"))
						{
							IVariable variable2 = signature2[stName];
							if (variable2 != null)
							{
								val.Add(signature2);
								val2.Add(variable2);
							}
						}
					}
				}
			}
			if (m_precomPool != null && val2.get_Count() == 0)
			{
				ISignature[] gVLSignatures = m_precomPool.GVLSignatures;
				foreach (ISignature signature3 in gVLSignatures)
				{
					if (!signature3.HasAttribute("qualified_only"))
					{
						IVariable variable3 = signature3[stName];
						if (variable3 != null)
						{
							val.Add(signature3);
							val2.Add(variable3);
						}
					}
				}
				if (val2.get_Count() == 0)
				{
					foreach (IPreCompileContext9 visibleLibrary2 in m_libraryTable.GetVisibleLibraries(m_precomPool))
					{
						gVLSignatures = visibleLibrary2.GVLSignatures;
						foreach (ISignature signature4 in gVLSignatures)
						{
							if (!signature4.HasAttribute("qualified_only"))
							{
								IVariable variable4 = signature4[stName];
								if (variable4 != null)
								{
									val.Add(signature4);
									val2.Add(variable4);
								}
							}
						}
					}
				}
			}
			if (val2.get_Count() > 0)
			{
				IVariable[] array = new IVariable[val2.get_Count()];
				val2.CopyTo(array);
				signsWithVar = new ISignature[val.get_Count()];
				val.CopyTo(signsWithVar);
				return array;
			}
			return null;
		}

		public IVariable[] FindVariable(string stName, out ISignature[] signsWithVar)
		{
			signsWithVar = null;
			ISignature signWithVar = null;
			IVariable variable = null;
			IVariable[] array = null;
			variable = FindVariableLocal(stName, out signWithVar);
			if (variable != null)
			{
				array = new IVariable[1] { variable };
				signsWithVar = new ISignature[1];
				signsWithVar[0] = signWithVar;
				return array;
			}
			if (!LocalScope)
			{
				array = FindVariableGlobal(stName, out signsWithVar);
				if (array != null)
				{
					return array;
				}
			}
			return null;
		}

		public IVariable[] FindVariable(string stName)
		{
			ISignature[] signsWithVar;
			return FindVariable(stName, out signsWithVar);
		}

		public ISignature4[] FindSignature(string stName)
		{
			return this[stName];
		}

		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use FindSignature in IScope2 instead")]
		public ISignature[] FindSignature(IQualifiedNameExpression qne)
		{
			throw new NotImplementedException();
		}

		public ISignature FindSignatureLocal(string stName)
		{
			ISignature4 signature = m_signlocal;
			IPreCompileContext preCompileContext = m_precomLocal;
			string text = stName.ToUpperInvariant();
			Stack<ISignature> stack = new Stack<ISignature>();
			while (signature != null && preCompileContext != null && !stack.Contains(signature))
			{
				stack.Push(signature);
				ISignature[] subSignatures = preCompileContext.GetSubSignatures(signature.ObjectGuid);
				if (subSignatures != null)
				{
					ISignature[] array = subSignatures;
					foreach (ISignature signature2 in array)
					{
						if (signature2.Name == text)
						{
							return signature2;
						}
					}
				}
				if (signature.BaseExpression == null)
				{
					break;
				}
				ISignature4[] array2 = FindSignature(signature.BaseExpression);
				if (array2 == null || array2.Length != 1)
				{
					return null;
				}
				signature = array2[0];
				preCompileContext = GetPrecompileContextOfSignature(signature);
			}
			return null;
		}

		private IPreCompileContext9 GetPrecompileContextOfSignature(ISignature4 sign)
		{
			if (!string.IsNullOrEmpty(sign.LibraryPath))
			{
				return GetLibraryContext(sign.LibraryPath);
			}
			if (m_precomLocal != null && m_precomLocal.GetSignature(sign.ObjectGuid) != null)
			{
				return m_precomLocal;
			}
			IPreCompileContext9 poolContext = GetPoolContext();
			if (poolContext.GetSignature(sign.ObjectGuid) != null)
			{
				return poolContext;
			}
			return null;
		}

		public bool FindDeclaration(string stIdent, out IVariable[] variable, out ISignature[] signature, out HelperScope scope)
		{
			IVariable variable2 = null;
			ISignature signWithVar = null;
			variable = null;
			signature = null;
			scope = null;
			variable2 = FindVariableLocal(stIdent, out signWithVar);
			if (variable2 != null)
			{
				variable = new IVariable[1];
				variable[0] = variable2;
				signature = new ISignature[1];
				signature[0] = signWithVar;
				return true;
			}
			signWithVar = FindSignatureLocal(stIdent);
			if (signWithVar != null)
			{
				signature = new ISignature[1];
				signature[0] = signWithVar;
				return true;
			}
			if (!LocalScope)
			{
				variable = FindVariableGlobal(stIdent, out signature);
				if (variable != null)
				{
					return true;
				}
				if (m_bSearchLocalSignatures)
				{
					ISignature[] array = (signature = this[stIdent]);
				}
			}
			if (signature != null && signature.Length != 0)
			{
				return true;
			}
			if (!LocalScope)
			{
				scope = FindScope(stIdent);
				if (scope != null)
				{
					return true;
				}
			}
			return false;
		}

		public IIdentifierInfo[] GetIdentifierInfo(string stAccessPath)
		{
			return new IIdentifierInfo[0];
		}

		public ISignature2 FindSignatureGlobal(IExpression expName, out string stNamespace)
		{
			stNamespace = null;
			if (!(FindSignatureGlobal(expName) is ISignature2 signature))
			{
				return null;
			}
			stNamespace = GetNamespace(signature.LibraryPath);
			return signature;
		}

		public string GetNamespace(string stLibraryId)
		{
			IList<IPreCompileContext9> visibleLibraries = m_libraryTable.GetVisibleLibraries(m_precomLocal);
			for (int i = 0; i < visibleLibraries.Count; i++)
			{
				if (visibleLibraries[i].LibraryPath.ToUpperInvariant() == stLibraryId.ToUpperInvariant())
				{
					return m_libraryTable.GetNamespaceOfLibrary(stLibraryId, m_precomLocal);
				}
			}
			return null;
		}

		public HelperScope CreateSystemScope()
		{
			return new HelperScope(m_nPointerSize, m_libraryTable, null, GetSystemContext(), m_precomPool);
		}

		public ISignature FindSignatureGlobal(IExpression exp)
		{
			if (exp is ICompoAccessExpression)
			{
				ICompoAccessExpression compoAccessExpression = exp as ICompoAccessExpression;
				return FindScope(compoAccessExpression.Left)?.FindSignatureGlobal(compoAccessExpression.Right);
			}
			if (exp is ISystemScopeExpression)
			{
				HelperScope helperScope = CreateSystemScope();
				ISystemScopeExpression systemScopeExpression = exp as ISystemScopeExpression;
				return helperScope.FindSignatureGlobal(systemScopeExpression.Base.ToString());
			}
			if (exp is IVariableExpression)
			{
				return FindSignatureGlobal(exp.ToString());
			}
			return null;
		}

		public bool FindDeclaration(string stIdent, out IVariable variable, out ISignature signature, out IPrecompileScope scope)
		{
			variable = null;
			signature = null;
			IVariable[] variable2;
			ISignature[] signature2;
			HelperScope scope2;
			bool result = FindDeclaration(stIdent, out variable2, out signature2, out scope2);
			if (variable2 != null && variable2.Length == 1)
			{
				variable = variable2[0];
			}
			if (signature2 != null && signature2.Length == 1)
			{
				signature = signature2[0];
			}
			scope = scope2;
			return result;
		}

		public ISignature FindSignatureGlobal(IQualifiedNameExpression qne)
		{
			return null;
		}

		public ISignature FindSignatureGlobal(string stName)
		{
			foreach (IPreCompileContext9 allPrecompileContext in GetAllPrecompileContexts())
			{
				ISignature signature = allPrecompileContext.GetSignature(stName);
				if (signature != null)
				{
					return signature;
				}
			}
			return null;
		}

		private IEnumerable<IPreCompileContext9> GetAllPrecompileContexts()
		{
			Guid guidLocal = ApplicationGuid;
			while (guidLocal != Guid.Empty)
			{
				IPreCompileContext9 comconApp = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidLocal) as IPreCompileContext9;
				yield return comconApp;
				IList<IPreCompileContext9> visibleLibraries = m_libraryTable.GetVisibleLibraries(comconApp);
				foreach (IPreCompileContext9 item in visibleLibraries)
				{
					yield return item;
				}
				guidLocal = APEnvironmentFacade.Instance.LanguageModelMgr.GetParentApplicationGuid(guidLocal);
			}
			if (m_precomPool == null)
			{
				yield break;
			}
			yield return m_precomPool;
			IList<IPreCompileContext9> visibleLibraries2 = m_libraryTable.GetVisibleLibraries(m_precomPool);
			foreach (IPreCompileContext9 item2 in visibleLibraries2)
			{
				yield return item2;
			}
		}

		public IPrecompileScope GlobalScope()
		{
			return CreateGlobalScope();
		}

		public IPrecompileScope NewLocalScope(string stName)
		{
			ISignature4[] array = FindSignature(stName);
			if (array.Length == 1)
			{
				return NewLocalScope(array[0]);
			}
			return null;
		}

		public IPrecompileScope NewLocalScope(ISignature sign)
		{
			return CreateUserdefScope(sign as ISignature4);
		}

		public IIdentifierInfo[] GetAllDeclarations()
		{
			return new IIdentifierInfo[0];
		}
	}
}
