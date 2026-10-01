using System;
using System.Collections.Generic;
using System.IO;
using \u000E;
using \u0019;
using \u001B;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Compiler35220.TreeConversion;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001E
{
	// Token: 0x02000098 RID: 152
	internal sealed class \u0003 : global::\u000E.\u0002
	{
		// Token: 0x06000CD9 RID: 3289 RVA: 0x00020B08 File Offset: 0x0001ED08
		internal \u0003(BinaryReader \u009E\u0002, ITreeFactory \u0003\u0003) : base(\u009E\u0002, \u0003\u0003)
		{
			base.Reader = \u009E\u0002;
			base.Factory = \u0003\u0003;
			this.\u0001 = new \u0019.\u0002(\u009E\u0002, (ILMSerializableTypeFactory2)this.\u0001, this);
		}

		// Token: 0x06000CDA RID: 3290 RVA: 0x00020B38 File Offset: 0x0001ED38
		public new \u0001 \u0001<\u0001>(BinaryReader \u0002)
		{
			_IExprement iexprement = base.\u0001();
			if (iexprement == null)
			{
				return default(\u0001);
			}
			ICompactedParseTreeInformation info = base.\u0001(\u0002);
			PrecompileParseTreeInformationSetter.SetInformationInParseTree(iexprement, info);
			return (\u0001)((object)iexprement);
		}

		// Token: 0x06000CDB RID: 3291 RVA: 0x00020B70 File Offset: 0x0001ED70
		public \u0001 \u0002<\u0001>(BinaryReader \u0002)
		{
			_IExprement iexprement = base.\u0001();
			if (iexprement == null)
			{
				return default(\u0001);
			}
			ICompactedCompiledParseTreeInformation compactedCompiledParseTreeInformation = this.\u0001.CreateCompactedCompiledParseTreeInformation();
			this.\u0001(\u0002, this.\u0001, compactedCompiledParseTreeInformation);
			CompiledParseTreeInformationSetter.SetInformationInParseTree(iexprement, compactedCompiledParseTreeInformation);
			return (\u0001)((object)iexprement);
		}

		// Token: 0x06000CDC RID: 3292 RVA: 0x00020BBC File Offset: 0x0001EDBC
		public new ICompactedCompiledParseTreeInformation \u0001(BinaryReader \u0002, _ILanguageModelBuilder3 \u0003, ICompactedCompiledParseTreeInformation \u0004)
		{
			global::\u001E.\u0003.\u0001(\u0002, \u0004);
			global::\u000E.\u0002.\u0001(\u0002, \u0003, \u0004.MessageTable);
			this.\u0001(\u0002, \u0004);
			return \u0004;
		}

		// Token: 0x06000CDD RID: 3293 RVA: 0x00020BDC File Offset: 0x0001EDDC
		private new static void \u0001(BinaryReader \u0002, ICompactedCompiledParseTreeInformation \u0003)
		{
			int num = \u0002.ReadInt32();
			if (num <= 0)
			{
				return;
			}
			\u0003.SourcePosTable = new List<long>(num);
			\u0003.LengthTable = new List<short>(num);
			for (int i = 0; i < num; i++)
			{
				\u0003.SourcePosTable.Add(\u0002.ReadInt64());
			}
			for (int j = 0; j < num; j++)
			{
				\u0003.LengthTable.Add(\u0002.ReadInt16());
			}
		}

		// Token: 0x06000CDE RID: 3294 RVA: 0x00020C48 File Offset: 0x0001EE48
		protected new void \u0001(BinaryReader \u0002, ICompactedCompiledParseTreeInformation \u0003)
		{
			int num = \u0002.ReadInt32();
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					int key = \u0002.ReadInt32();
					int iSignatureId = \u0002.ReadInt32();
					int iVariableId = \u0002.ReadInt32();
					_IType compiledType = null;
					if (!global::\u001B.\u0001.\u0001(\u0002))
					{
						compiledType = this.\u0001.\u0001();
					}
					\u0003.TypeInfoTable.Add(key, new CompiledExpressionTypeInfo(iSignatureId, iVariableId, compiledType));
				}
			}
		}

		// Token: 0x0400022C RID: 556
		private new readonly \u0019.\u0002 \u0001;
	}
}
