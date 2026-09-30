using System;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0001
{
	// Token: 0x020001B4 RID: 436
	internal class \u0007 : SimpleTypeChecker
	{
		// Token: 0x06001FF5 RID: 8181 RVA: 0x0006CCF8 File Offset: 0x0006AEF8
		public \u0007(_ISignature \u0002\u0002, int \u0096\u0002, _IPreCompileContext \u0007\u0003, bool \u0096\u0003) : base(\u0002\u0002, \u0096\u0002, \u0007\u0003, \u0096\u0003)
		{
		}

		// Token: 0x06001FF6 RID: 8182 RVA: 0x0006CD08 File Offset: 0x0006AF08
		public override void visit(_ILiteralExpression litval)
		{
			base.visit(litval);
			if (base.TopOfStack.typeResolved.Class == TypeClass.BitConst)
			{
				this.\u0001(null, TypeTable.Int);
			}
			if (litval.ConstantType == TypeClass.AnyInt && base.TopOfStack.typeResolved.Class == TypeClass.SInt)
			{
				this.\u0001(null, TypeTable.Int);
			}
		}
	}
}
