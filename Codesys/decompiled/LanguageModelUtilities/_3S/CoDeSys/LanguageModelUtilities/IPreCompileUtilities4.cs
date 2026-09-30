using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LibManObject;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedInterface]
	public interface IPreCompileUtilities4 : IPreCompileUtilities3, IPreCompileUtilities2, IPreCompileUtilities
	{
		bool GetItemPathFromProjectRec(Stack<ILibManItem> itemPath, IProject proToLookIn, Guid gdApp, IProject proToLookFor);

		IEvaluationContext3 CreateContext3(int nProj, int nAttrProj, Guid gdScope, IGetLibInformation libInfo);

		IEvaluationContext3 GetContextFromSignature(int nProjAttracting, ISignature sig, IGetLibInformation libInfo);

		bool GetItemPathFromProjectRec(Stack<ILibManItem> itemPath, IProject proToLookIn, Guid gdApp, IProject proToLookFor, IGetLibInformation libInfo);

		bool GetProjectPathByLibDisplayName(int nProj, string stDisplayName, Stack<int> itemPath, IGetLibInformation libInfo);

		IProject GetProjectByLibNamespacePath(int nProj, string stPath, IGetLibInformation libInfo);

		IProject GetProjectFromLibManItem(ILibManItem item, IGetLibInformation libInfo);

		IGetLibInformation CreateBufferLibInfo();

		IGetLibInformation CreateLibInfo();
	}
}
