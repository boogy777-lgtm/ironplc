using System;
using \u0001;
using \u0015;
using \u0018;
using \u001F;
using _3S.CoDeSys.Compiler35220.ImplicitCode;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x020003C4 RID: 964
	internal static class \u0016
	{
		// Token: 0x060036D5 RID: 14037 RVA: 0x000DF3B8 File Offset: 0x000DD5B8
		internal static void \u0001(_ISignature \u0002, _ISignature \u0003, _ICompileContext \u0004, _ICompileContext \u0005)
		{
			global::\u0001.\u0014.\u0001(\u0002, \u0003, \u0004);
			FbInitSignatureGenerator.\u0001(\u0002, \u0003);
			FBImplicitPropertyVariableGenerator.\u0001(\u0002, \u0003);
			if (\u0002.POUType != Operator.FunctionBlock && !\u0002.GetFlag(SignatureFlag.Structure))
			{
				return;
			}
			FbInitSignatureGenerator.\u0001(\u0002, \u0003, \u0004, \u0005);
			\u0018.\u0012.\u0001(\u0002, \u0003, \u0004, \u0005);
			if (\u0002.POUType != Operator.FunctionBlock || \u0002.GetFlag(SignatureFlag.ImplicitInterfaceUnion))
			{
				return;
			}
			global::\u0001.\u0015.\u0001(\u0002, \u0003, \u0004, \u0005);
			global::\u0015.\u0008.\u0001(\u0002, \u0003, \u0004, \u0005);
			global::\u0014.\u0015.\u0001(\u0002, \u0003, \u0004, \u0005);
			\u001F.\u0015.\u0001(\u0002, \u0003, \u0004, \u0005);
		}
	}
}
