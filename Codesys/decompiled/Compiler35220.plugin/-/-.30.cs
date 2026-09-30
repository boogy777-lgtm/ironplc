using System;
using System.IO;
using \u0019;
using \u001B;
using \u001E;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0008
{
	// Token: 0x02000082 RID: 130
	internal sealed class \u0003 : \u0019.\u0002
	{
		// Token: 0x06000B38 RID: 2872 RVA: 0x00018A60 File Offset: 0x00016C60
		internal \u0003(BinaryReader \u009E\u0002, ILMSerializableTypeFactory2 \u0002\u0003, \u001E.\u0003 \u0005\u0003) : base(\u009E\u0002, \u0002\u0003, \u0005\u0003)
		{
		}

		// Token: 0x06000B39 RID: 2873 RVA: 0x00018A6C File Offset: 0x00016C6C
		public override void \u0001(_IUserdefType \u0002)
		{
			\u0002.NameExpression = base.ExpressionDeserializer.\u0001<IExpression>(base.Reader);
		}

		// Token: 0x06000B3A RID: 2874 RVA: 0x00018A88 File Offset: 0x00016C88
		public override void \u0001(IGenericUserdefType \u0002)
		{
			\u0002.NameExpression = base.ExpressionDeserializer.\u0001<IExpression>(base.Reader);
			foreach (_IExpression exp in \u001B.\u0001.\u0002<_IExpression>(base.Reader, new Func<BinaryReader, _IExpression>(base.ExpressionDeserializer.\u0001<_IExpression>)))
			{
				\u0002.AddGenericConstantInitialization(exp);
			}
		}

		// Token: 0x06000B3B RID: 2875 RVA: 0x00018B04 File Offset: 0x00016D04
		public override void \u0001(_IAliasType \u0002)
		{
			\u0002.NameExpression = base.ExpressionDeserializer.\u0001<IExpression>(base.Reader);
			((IAliasTypeSerializable)\u0002).SetEffectiveType(base.\u0002());
			\u0002.IsCompiled = base.Reader.ReadBoolean();
			\u0002._DefaultValue = base.ExpressionDeserializer.\u0001<_IExpression>(base.Reader);
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x00018B64 File Offset: 0x00016D64
		public override void \u0001(_IEnumType \u0002)
		{
			((IEnumTypeSerializable)\u0002).Name = base.Reader.ReadString();
			\u0002._Base = base.\u0002();
			\u0002._DefaultValue = base.ExpressionDeserializer.\u0001<_IVariableExpression>(base.Reader);
			\u0002.AfterDeserialize();
		}
	}
}
