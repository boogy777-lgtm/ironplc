using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities3 : IPreCompileUtilities2, IPreCompileUtilities
	{
		InitialValueResult DetermineInitialValue(IEvaluationContext ctxBase, string stAccessPath, out IExpression expInit, out IEvaluationContext ctxExpression);

		ISignature2 FindTypeSignature2(IEvaluationContext context, string stTypeName);

		IPreCompileContext4 GetPreCompileContextForProject(int nProj);

		bool IsPrimitiveType(IType t);

		ICompiledType2 GetCompiledType(string stTypeName);

		IEvaluationContext GetContextFromSignature(int nProjAttracting, ISignature sig);

		IVariable FindVar(ISignature2 sig, string stName, out ISignature2 foundSig);

		IProject GetAttractingProject(int nContextProj, Guid gdContextApp, int nAttractedProj);

		bool GetProjectPathByLibDisplayName(int nProj, string stDisplayName, Stack<int> itemPath);

		IProject GetProjectByLibNamespacePath(int nProj, string stPath);

		IProject GetProjectFromLibManItem(ILibManItem item);

		IManagedLibrary GetManagedLib(ILibManItem lmi);

		IEnumerable<ILibManItem> GetAllLibManItemsInProject(int nProj);
	}
}
