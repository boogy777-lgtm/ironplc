using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal static class Helpers
	{
		private static TypeClass[] NoIntPrimitives = new TypeClass[13]
		{
			TypeClass.Bool,
			TypeClass.Date,
			TypeClass.DateAndTime,
			TypeClass.LReal,
			TypeClass.Real,
			TypeClass.Time,
			TypeClass.LTime,
			TypeClass.TimeOfDay,
			TypeClass.WString,
			TypeClass.String,
			TypeClass.LDate,
			TypeClass.LDateAndTime,
			TypeClass.LTimeOfDay
		};

		public static IExpression ParseExpression(string stExp)
		{
			if (stExp == null)
			{
				throw new ArgumentNullException("stExp");
			}
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner(stExp, bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			return ((IParser2)APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner)).ParseExpression();
		}

		public static IPreCompileContext GetPrecompileContext(IEvaluationContext context)
		{
			if (context is IEvaluationContext2 && context.ApplicationGuid == Guid.Empty)
			{
				return APEnvironmentFacade.Instance.GetPreCompileContextForProject((context as IEvaluationContext2).ProjectHandle);
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(context.ApplicationGuid);
		}

		public static bool StrEqCI(string st1, string st2)
		{
			return string.Compare(st1, st2, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		public static ISignature2 FindTypeSignature2(IEvaluationContext context, string stTypeName)
		{
			int num = -1;
			Guid applicationGuid = context.ApplicationGuid;
			Guid scopeIdentification = context.ScopeIdentification;
			num = ((!(context is IEvaluationContext2)) ? APEnvironmentFacade.Instance.PrimaryProjectHandle : ((IEvaluationContext2)context).ProjectHandle);
			return FindTypeSignature2(num, applicationGuid, scopeIdentification, stTypeName);
		}

		public static ISignature2 FindTypeSignature2FromObj(int nProj, Guid gdObj, string stTypeName)
		{
			Guid applicationGuid = APEnvironmentFacade.Instance.GetApplicationGuid(gdObj, nProj);
			return FindTypeSignature2(nProj, applicationGuid, gdObj, stTypeName);
		}

		public static ISignature2 FindTypeSignature2(int nProj, Guid gdApp, Guid gdObj, string stTypeName)
		{
			ISignature2 signature = FindSignatureByName(nProj, gdApp, gdObj, stTypeName);
			if (signature == null && Guid.Empty == gdApp)
			{
				signature = FindSignatureInPOUPool(nProj, stTypeName);
			}
			return signature;
		}

		private static ISignature2 FindSignatureByName(int nProj, Guid gdApp, Guid gdObj, string stTypeName)
		{
			if (Guid.Empty != gdApp)
			{
				IEnumerable<ISignature> enumerable = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(nProj, gdApp, gdObj, stTypeName);
				return (Fun.IsEmpty<ISignature>(enumerable) ? null : Fun.First<ISignature>(enumerable)) as ISignature2;
			}
			IEnumerable<ISignature> enumerable2 = APEnvironmentFacade.Instance.LanguageModelMgr.FindSignaturesByName(nProj, gdObj, stTypeName);
			return ((Fun.Count<ISignature>(enumerable2) == 1) ? Fun.First<ISignature>(enumerable2) : null) as ISignature2;
		}

		private static ISignature2 FindSignatureInPOUPool(int nProj, string stTypeName)
		{
			IExpression expression = ParseExpression(stTypeName);
			if (expression == null)
			{
				return null;
			}
			IPreCompileContext4 preCompileContextForProject = APEnvironmentFacade.Instance.GetPreCompileContextForProject(nProj);
			if (preCompileContextForProject == null)
			{
				return null;
			}
			return (ISignature2)((IPrecompileScope4)preCompileContextForProject.CreatePrecompileScope(Guid.Empty)).FindSignatureGlobal(expression);
		}

		public static ICompiledType2 GetCompiledType(string stType)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner("", bIncludeComments: false, bIncludeEndOfLines: false, bIncludePragmas: false, bIncludeWhitespaces: false);
			IParser3 obj = APEnvironmentFacade.Instance.LanguageModelMgr.CreateParser(scanner) as IParser3;
			scanner.Initialize(stType);
			return obj.ParseTypeDeclaration() as ICompiledType2;
		}

		public static bool IsSubrangeType(IType t)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (t is ICompiledType ct)
			{
				return IsSubrangeTypeC(ct);
			}
			return IsSubrangeType(t.ToString());
		}

		public static bool IsSubrangeType(string stType)
		{
			if (stType == null)
			{
				throw new ArgumentNullException("stType");
			}
			ICompiledType compiledType = GetCompiledType(stType);
			if (compiledType != null)
			{
				return IsSubrangeType(compiledType);
			}
			return false;
		}

		private static bool IsSubrangeTypeC(ICompiledType ct)
		{
			if (ct.Class == TypeClass.Subrange)
			{
				return IsPrimitiveTypeC(ct.BaseType);
			}
			return false;
		}

		public static bool IsPrimitiveType(IType t)
		{
			if (t == null)
			{
				throw new ArgumentNullException("t");
			}
			if (t is ICompiledType)
			{
				return IsPrimitiveTypeC((ICompiledType)t);
			}
			return IsPrimitiveType(t.ToString());
		}

		public static bool IsPrimitiveType(string stType)
		{
			if (stType == null)
			{
				throw new ArgumentNullException("stType");
			}
			ICompiledType compiledType = GetCompiledType(stType);
			if (compiledType != null)
			{
				return IsPrimitiveType(compiledType);
			}
			return false;
		}

		public static bool IsPrimitiveTypeC(ICompiledType ct)
		{
			if (ct.Class != TypeClass.Userdef)
			{
				if (!ct.IsInteger)
				{
					return Fun.Exists<TypeClass>((IEnumerable<TypeClass>)NoIntPrimitives, (Fun1<TypeClass, bool>)((TypeClass c) => c == ct.Class));
				}
				return true;
			}
			return false;
		}

		public static int GetProjectHandleFromSignature(ISignature sig)
		{
			return APEnvironmentFacade.Instance.GetProjectHandle(sig.LibraryPath);
		}

		public static IEvaluationContext4 GetContextFromSignature(int nProjAttracting, ISignature2 sigExpInit, ISignature2 sigExpInitLocal, IGetLibInformation libInfo)
		{
			int projectHandleFromSignature = GetProjectHandleFromSignature(sigExpInit);
			if (projectHandleFromSignature == APEnvironmentFacade.Instance.PrimaryProjectHandle)
			{
				Guid applicationGuid = APEnvironmentFacade.Instance.GetApplicationGuid(sigExpInit.ObjectGuid, projectHandleFromSignature);
				return new EvaluationContext4(projectHandleFromSignature, projectHandleFromSignature, applicationGuid, sigExpInit.ObjectGuid, sigExpInitLocal?.ObjectGuid ?? Guid.Empty, libInfo);
			}
			int nAttrProj = projectHandleFromSignature;
			if (projectHandleFromSignature == nProjAttracting)
			{
				nAttrProj = projectHandleFromSignature;
			}
			else
			{
				Stack<int> stack = new Stack<int>();
				string libraryPath = sigExpInit.LibraryPath;
				object libInfo2;
				if (!(libInfo is IGetLibInformation2))
				{
					IGetLibInformation2 getLibInformation = new GetLibInformation();
					libInfo2 = getLibInformation;
				}
				else
				{
					libInfo2 = (IGetLibInformation2)libInfo;
				}
				if (LibraryHelpers.FindLibByDisplayName(nProjAttracting, null, libraryPath, stack, (IGetLibInformation2)libInfo2) && stack.Count != 0 && stack.Peek() == projectHandleFromSignature)
				{
					if (stack.Count == 1)
					{
						nAttrProj = projectHandleFromSignature;
					}
					else
					{
						stack.Pop();
						nAttrProj = stack.Peek();
					}
				}
			}
			return new EvaluationContext4(projectHandleFromSignature, nAttrProj, sigExpInit.ObjectGuid, sigExpInitLocal?.ObjectGuid ?? Guid.Empty, libInfo);
		}

		public static int GetAttractingProject(IEvaluationContext context)
		{
			if (context is IEvaluationContext2)
			{
				return ((IEvaluationContext2)context).AttractingProjectHandle;
			}
			return APEnvironmentFacade.Instance.PrimaryProjectHandle;
		}
	}
}
