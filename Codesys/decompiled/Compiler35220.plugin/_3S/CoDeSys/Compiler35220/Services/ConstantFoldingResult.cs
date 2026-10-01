using System;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000A9 RID: 169
	public class ConstantFoldingResult
	{
		// Token: 0x06000D7F RID: 3455 RVA: 0x00023B3C File Offset: 0x00021D3C
		public ConstantFoldingResult(_ILiteralValue literalValue)
		{
			this._literalValue = literalValue;
			this._structureInitialization = null;
			this._arrayInitialization = null;
		}

		// Token: 0x06000D80 RID: 3456 RVA: 0x00023B5C File Offset: 0x00021D5C
		public ConstantFoldingResult(_IExpression expression)
		{
			this._literalValue = null;
			_IStructureInitialization istructureInitialization = expression as _IStructureInitialization;
			if (istructureInitialization != null)
			{
				this._structureInitialization = istructureInitialization;
			}
			else
			{
				this._structureInitialization = null;
			}
			_IArrayInitialization iarrayInitialization = expression as _IArrayInitialization;
			if (iarrayInitialization != null)
			{
				this._arrayInitialization = iarrayInitialization;
				return;
			}
			this._arrayInitialization = null;
		}

		// Token: 0x04000247 RID: 583
		public readonly _ILiteralValue _literalValue;

		// Token: 0x04000248 RID: 584
		public readonly _IStructureInitialization _structureInitialization;

		// Token: 0x04000249 RID: 585
		public readonly _IArrayInitialization _arrayInitialization;
	}
}
