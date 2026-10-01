using System;
using System.Collections.Generic;
using \u0001;
using \u0008;
using \u0011;
using \u0012;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Scopes;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelUtilities;
using _3S.CoDeSys.Utilities;

namespace \u0017
{
	// Token: 0x02000177 RID: 375
	internal static class \u0008
	{
		// Token: 0x06001960 RID: 6496 RVA: 0x0004EEEC File Offset: 0x0004D0EC
		internal static LList<IVariable> \u0001(_ISignature \u0002)
		{
			LList<IVariable> llist = new LList<IVariable>();
			foreach (IVariable variable in \u0002.AllLazy)
			{
				if (variable.Type.Class == TypeClass.Lazy)
				{
					llist.Add(variable);
				}
			}
			return llist;
		}

		// Token: 0x06001961 RID: 6497 RVA: 0x0004EF50 File Offset: 0x0004D150
		internal static IEnumerable<IDeclarationInfo> \u0001(_IPreCompileContext \u0002, string \u0003, string \u0004, string \u0005)
		{
			if (\u0003 == null)
			{
				throw new ArgumentNullException("stCode");
			}
			if (\u0004 == null)
			{
				throw new ArgumentNullException("stPOUName");
			}
			ISignature signature = \u0002[\u0004.ToUpperInvariant()];
			_ISignature isignature = signature as _ISignature;
			if (signature == null)
			{
				throw new ArgumentException("Object not found in resource", "lIdPOU");
			}
			if (!string.IsNullOrEmpty(\u0005))
			{
				ISignature[] subSignatures = \u0002.GetSubSignatures(signature.ObjectGuid);
				if (subSignatures != null)
				{
					foreach (ISignature signature2 in subSignatures)
					{
						if (signature2.Name == \u0005.ToUpperInvariant() && signature2.POUType != Operator.Action)
						{
							signature = signature2;
							break;
						}
					}
				}
			}
			_IExpression[] array2 = new global::\u0011.\u0006(\u0003, true).\u0001();
			_IStatement istatement = new global::\u0011.\u0006(\u0003).\u0001();
			CheckerScope checkerScope = new CheckerScope(\u0002.ApplicationGuid, signature as _ISignature, \u0002, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
			LList<IVariable> llist = \u0017.\u0008.\u0001(signature as _ISignature);
			if (isignature != signature)
			{
				llist.AddRange(\u0017.\u0008.\u0001(isignature));
			}
			UnknownIdentVisitor unknownIdentVisitor = new UnknownIdentVisitor(checkerScope, true, null, llist);
			foreach (_IExpression iexprement in array2)
			{
				unknownIdentVisitor.\u0001(checkerScope);
				iexprement.Accept(unknownIdentVisitor);
			}
			unknownIdentVisitor.\u0001(checkerScope);
			istatement.Accept(unknownIdentVisitor);
			SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(signature as _ISignature, APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signature.LibraryPath), \u0002, true);
			simpleTypeInferrer.Lazies = unknownIdentVisitor.DeclarationInfo;
			_IExpression[] array3 = array2;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].Accept(simpleTypeInferrer);
			}
			istatement.Accept(simpleTypeInferrer);
			return simpleTypeInferrer.ResolvedInfo;
		}

		// Token: 0x06001962 RID: 6498 RVA: 0x0004F108 File Offset: 0x0004D308
		internal static IIdentifierInfo[] \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004, bool \u0005)
		{
			_ISignature isignature = \u0002[\u0003];
			CheckerScope scope = new CheckerScope(isignature, \u0002, null);
			bool flag;
			_IExpression iexpression = new global::\u0011.\u0006(\u0004, true).\u0002(out flag);
			if (iexpression == null || flag)
			{
				return null;
			}
			IVariable variable;
			IType type;
			ISignature u;
			try
			{
				if (\u0005)
				{
					int nProjectHandle;
					if (isignature == null)
					{
						nProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
					}
					else
					{
						nProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath);
					}
					SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(isignature, nProjectHandle, \u0002, true);
					iexpression.Accept(simpleTypeInferrer);
					variable = simpleTypeInferrer.DerivedVariable;
					type = simpleTypeInferrer.DerivedType;
					u = simpleTypeInferrer.DerivedSignature;
				}
				else
				{
					UnknownIdentVisitor unknownIdentVisitor = new UnknownIdentVisitor(scope, null);
					iexpression.Accept(unknownIdentVisitor);
					variable = unknownIdentVisitor.DerivedVariable;
					type = unknownIdentVisitor.DerivedType;
					u = unknownIdentVisitor.DerivedSignature;
				}
			}
			catch
			{
				return null;
			}
			string u0018_u = string.Empty;
			IdentifierInfoFlag identifierInfoFlag = IdentifierInfoFlag.None;
			if (variable != null)
			{
				identifierInfoFlag = IdentifierInfoFlag.Variable;
				u0018_u = variable.Comment;
				if (variable.GetFlag(VarFlag.Global))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Global;
				}
				if (variable.GetFlag(VarFlag.External))
				{
					identifierInfoFlag |= IdentifierInfoFlag.External;
				}
				if (variable.GetFlag(VarFlag.Inout))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Inout;
				}
				if (variable.GetFlag(VarFlag.Input))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Input;
				}
				if (variable.GetFlag(VarFlag.Local))
				{
					identifierInfoFlag |= IdentifierInfoFlag.Local;
				}
			}
			IType type2 = type;
			if (type2 == null && variable != null)
			{
				type2 = variable.Type;
			}
			if (type2 is _ILazyType && variable != null && variable.HasAttribute("inferredtype"))
			{
				type2 = global::\u0011.\u0006.\u0001(variable.GetAttributeValue("inferredtype"));
			}
			return new global::\u0008.\u0006[]
			{
				new global::\u0008.\u0006(\u0004, u0018_u, identifierInfoFlag, type2)
				{
					Variable = variable,
					Signature = u
				}
			};
		}

		// Token: 0x06001963 RID: 6499 RVA: 0x0004F2E0 File Offset: 0x0004D4E0
		internal static IExpressionInfo \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004, bool \u0005)
		{
			_ISignature isignature = \u0002[\u0003];
			global::\u0011.\u0006 u = new global::\u0011.\u0006(\u0004, \u0005);
			bool flag;
			_IExpression iexpression = u.\u0001(out flag);
			IToken token;
			if (iexpression == null || flag || u.UsedScanner.GetNext(out token) != TokenType.End)
			{
				return null;
			}
			int nProjectHandle;
			if (isignature == null)
			{
				nProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
			}
			else
			{
				nProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath);
			}
			SimpleTypeInferrer simpleTypeInferrer = new SimpleTypeInferrer(isignature, nProjectHandle, \u0002, true);
			iexpression.Accept(simpleTypeInferrer);
			return new global::\u0012.\u0006(iexpression, simpleTypeInferrer.DerivedType);
		}

		// Token: 0x06001964 RID: 6500 RVA: 0x0004F370 File Offset: 0x0004D570
		internal static IIdentifierInfo[] \u0001(_IPreCompileContext \u0002, Guid \u0003, string \u0004, FindSubelementsFlags \u0005, out bool \u0006)
		{
			return (APEnvironmentFacade.Instance.LanguageModelUtilities.PreCompileUtils as IPreCompileUtilities7).FindSubelements(\u0003, \u0002, \u0004, \u0005, out \u0006);
		}

		// Token: 0x06001965 RID: 6501 RVA: 0x0004F394 File Offset: 0x0004D594
		public static IExprement \u0001(ISignature \u0002, ICompiledPOU \u0003, ISourcePosition \u0004, WhatToFind \u0005)
		{
			global::\u0001.\u0003 u = new global::\u0001.\u0003(\u0005, \u0004);
			if (\u0002 != null)
			{
				u.\u0001((_ISignature)\u0002);
				if (u.FoundExprement != null)
				{
					return u.FoundExprement;
				}
			}
			if (\u0003 != null)
			{
				u.\u0001((_ICompiledPOU)\u0003);
				if (u.FoundExprement != null)
				{
					return u.FoundExprement;
				}
			}
			return null;
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x0004F3E8 File Offset: 0x0004D5E8
		public static IExprement \u0001(ISourcePosition \u0002, WhatToFind \u0003, out IPreCompileContext \u0004)
		{
			\u0004 = null;
			if (!APEnvironmentFacade.Instance.ExistProject(\u0002.ProjectHandle))
			{
				return null;
			}
			foreach (IExplicitExpressionAtSourcePositionProvider explicitExpressionAtSourcePositionProvider in APEnvironmentFacade.Instance.GetAllExplicitExpressionAtSourcePositionProviders())
			{
				IExprement expressionAtSourcePosition = explicitExpressionAtSourcePositionProvider.GetExpressionAtSourcePosition(\u0002, \u0003, out \u0004);
				if (expressionAtSourcePosition != null)
				{
					return expressionAtSourcePosition;
				}
			}
			LList<Guid> llist = new LList<Guid>();
			llist.Add(\u0002.ObjectGuid);
			LList<Guid> llist2 = llist;
			ILanguageModelProviderWithDependencies languageModelProviderWithDependencies = APEnvironmentFacade.Instance.GetLanguageModelProviderWithDependencies(\u0002);
			if (languageModelProviderWithDependencies != null)
			{
				llist2.AddRange(languageModelProviderWithDependencies.ObjectsToUpdate);
			}
			bool flag = APEnvironmentFacade.Instance.IsPrimaryProject(\u0002.ProjectHandle);
			foreach (Guid guid in llist2)
			{
				if (flag)
				{
					using (IEnumerator<KeyValuePair<ISignature, IPreCompileContext>> enumerator3 = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignatures(guid).GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							KeyValuePair<ISignature, IPreCompileContext> keyValuePair = enumerator3.Current;
							ISignature key = keyValuePair.Key;
							\u0004 = keyValuePair.Value;
							if (string.IsNullOrWhiteSpace(\u0004.LibraryPath))
							{
								ICompiledPOU compiledPOU = \u0004.GetCompiledPOU(guid);
								IExprement exprement = \u0017.\u0008.\u0001(key, compiledPOU, \u0002, \u0003);
								if (exprement != null)
								{
									return exprement;
								}
							}
						}
						continue;
					}
				}
				\u0004 = APEnvironmentFacade.Instance.GetLibraryContext(\u0002.ProjectHandle);
				if (\u0004 != null)
				{
					ISignature signature = \u0004.GetSignature(guid);
					ICompiledPOU compiledPOU2 = \u0004.GetCompiledPOU(guid);
					IExprement exprement2 = \u0017.\u0008.\u0001(signature, compiledPOU2, \u0002, \u0003);
					if (exprement2 != null)
					{
						return exprement2;
					}
				}
			}
			\u0004 = null;
			return null;
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x0004F5AC File Offset: 0x0004D7AC
		public static IIdentifierInfo[] \u0001(string \u0002, ISourcePosition \u0003, WhatToFind \u0004)
		{
			_IPreCompileContext ipreCompileContext;
			if (APEnvironmentFacade.Instance.IsPrimaryProject(\u0003.ProjectHandle))
			{
				IPreCompileContext preCompileContext;
				APEnvironmentFacade.Instance.LanguageModelMgr.FindSignature(\u0003.ObjectGuid, out preCompileContext);
				ipreCompileContext = (preCompileContext as _IPreCompileContext);
			}
			else
			{
				ipreCompileContext = APEnvironmentFacade.Instance.GetLibraryContext(\u0003.ProjectHandle);
			}
			if (ipreCompileContext == null)
			{
				return null;
			}
			ISignature signature = ipreCompileContext.GetSignature(\u0003.ObjectGuid);
			ICompiledPOU compiledPOU = ipreCompileContext.GetCompiledPOU(\u0003.ObjectGuid);
			if (signature == null && compiledPOU == null)
			{
				return null;
			}
			global::\u0001.\u0003 u = new global::\u0001.\u0003(\u0004, \u0003);
			if (signature != null)
			{
				IExprement exprement = null;
				Guid guidSignature = Guid.Empty;
				u.\u0001((_ISignature)signature);
				if (u.FoundTypeExprement != null)
				{
					guidSignature = Guid.Empty;
					exprement = u.FoundTypeExprement;
				}
				else if (u.FoundExprement != null)
				{
					guidSignature = \u0003.ObjectGuid;
					exprement = u.FoundExprement;
				}
				if (exprement != null)
				{
					string stAccessPath = (exprement.ToString().IndexOf(\u0002, StringComparison.OrdinalIgnoreCase) < 0) ? \u0002 : exprement.ToString();
					return ipreCompileContext.GetIdentifierInfoFast(guidSignature, stAccessPath);
				}
			}
			if (compiledPOU != null)
			{
				u.\u0001((_ICompiledPOU)compiledPOU);
				string stAccessPath2 = \u0002;
				if (u.FoundExprement != null)
				{
					IExprement exprement2 = u.FoundExprement;
					stAccessPath2 = ((exprement2.ToString().IndexOf(\u0002, StringComparison.OrdinalIgnoreCase) < 0) ? \u0002 : exprement2.ToString());
				}
				return ipreCompileContext.GetIdentifierInfoFast(\u0003.ObjectGuid, stAccessPath2);
			}
			return null;
		}
	}
}
