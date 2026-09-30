using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using \u0019;
using \u001B;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Compiler35220.Serialization;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u000E
{
	// Token: 0x02000096 RID: 150
	internal abstract class \u0002
	{
		// Token: 0x17000424 RID: 1060
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x0001F194 File Offset: 0x0001D394
		// (set) Token: 0x06000C70 RID: 3184 RVA: 0x0001F19C File Offset: 0x0001D39C
		protected BinaryReader Reader { get; set; }

		// Token: 0x17000425 RID: 1061
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x0001F1A8 File Offset: 0x0001D3A8
		// (set) Token: 0x06000C72 RID: 3186 RVA: 0x0001F1B0 File Offset: 0x0001D3B0
		protected ITreeFactory Factory { get; set; }

		// Token: 0x06000C73 RID: 3187 RVA: 0x0001F1BC File Offset: 0x0001D3BC
		protected \u0002(BinaryReader \u009E\u0002, ITreeFactory \u0003\u0003)
		{
			this.Reader = \u009E\u0002;
			this.Factory = \u0003\u0003;
			this.\u0001 = new \u001B.\u0001();
			this.\u0001 = new Func<_IExprement>[Enum.GetNames(typeof(ExprementTag)).Length];
			this.\u0001[0] = new Func<_IExprement>(this.\u0002);
			this.\u0001[1] = new Func<_IExprement>(this.\u0090);
			this.\u0001[2] = new Func<_IExprement>(this.\u008B);
			this.\u0001[3] = new Func<_IExprement>(this.\u0004);
			this.\u0001[4] = new Func<_IExprement>(this.\u0005);
			this.\u0001[5] = new Func<_IExprement>(this.\u008C);
			this.\u0001[6] = new Func<_IExprement>(this.\u008D);
			this.\u0001[8] = new Func<_IExprement>(this.\u008E);
			this.\u0001[9] = new Func<_IExprement>(this.\u0006);
			this.\u0001[10] = new Func<_IExprement>(this.\u0007);
			this.\u0001[11] = new Func<_IExprement>(this.\u0008);
			this.\u0001[12] = new Func<_IExprement>(this.\u0003);
			this.\u0001[13] = new Func<_IExprement>(this.\u0019);
			this.\u0001[15] = new Func<_IExprement>(this.\u000E);
			this.\u0001[16] = new Func<_IExprement>(this.\u0017\u0002);
			this.\u0001[17] = new Func<_IExprement>(this.\u000F);
			this.\u0001[18] = new Func<_IExprement>(this.\u0010);
			this.\u0001[19] = new Func<_IExprement>(this.\u0011);
			this.\u0001[20] = new Func<_IExprement>(this.\u0012);
			this.\u0001[21] = new Func<_IExprement>(this.\u0013);
			this.\u0001[22] = new Func<_IExprement>(this.\u0014);
			this.\u0001[80] = new Func<_IExprement>(this.\u0015);
			this.\u0001[81] = new Func<_IExprement>(this.\u0016);
			this.\u0001[23] = new Func<_IExprement>(this.\u0017);
			this.\u0001[24] = new Func<_IExprement>(this.\u0018);
			this.\u0001[25] = new Func<_IExprement>(this.\u008F);
			this.\u0001[26] = new Func<_IExprement>(this.\u0013\u0002);
			this.\u0001[27] = new Func<_IExprement>(this.\u001A);
			this.\u0001[28] = new Func<_IExprement>(this.\u001B);
			this.\u0001[29] = new Func<_IExprement>(this.\u001C);
			this.\u0001[30] = new Func<_IExprement>(this.\u0011\u0002);
			this.\u0001[31] = new Func<_IExprement>(this.\u0010\u0002);
			this.\u0001[32] = new Func<_IExprement>(this.\u001E);
			this.\u0001[33] = new Func<_IExprement>(this.\u001F);
			this.\u0001[35] = new Func<_IExprement>(this.\u007F);
			this.\u0001[36] = new Func<_IExprement>(this.\u0080);
			this.\u0001[37] = new Func<_IExprement>(this.\u0081);
			this.\u0001[38] = new Func<_IExprement>(this.\u0082);
			this.\u0001[39] = new Func<_IExprement>(this.\u0012\u0002);
			this.\u0001[40] = new Func<_IExprement>(this.\u0083);
			this.\u0001[41] = new Func<_IExprement>(this.\u0084);
			this.\u0001[42] = new Func<_IExprement>(this.\u0086);
			this.\u0001[43] = new Func<_IExprement>(this.\u0087);
			this.\u0001[44] = new Func<_IExprement>(this.\u0088);
			this.\u0001[45] = new Func<_IExprement>(this.\u0089);
			this.\u0001[46] = new Func<_IExprement>(this.\u008A);
			this.\u0001[47] = new Func<_IExprement>(this.\u0016\u0002);
			this.\u0001[48] = new Func<_IExprement>(this.\u0015\u0002);
			this.\u0001[49] = new Func<_IExprement>(this.\u0091);
			this.\u0001[50] = new Func<_IExprement>(this.\u0092);
			this.\u0001[51] = new Func<_IExprement>(this.\u0007\u0002);
			this.\u0001[52] = new Func<_IExprement>(this.\u0008\u0002);
			this.\u0001[53] = new Func<_IExprement>(this.\u009F);
			this.\u0001[54] = new Func<_IExprement>(this.\u000F\u0002);
			this.\u0001[56] = new Func<_IExprement>(this.\u0002\u0002);
			this.\u0001[57] = new Func<_IExprement>(this.\u0094);
			this.\u0001[58] = new Func<_IExprement>(this.\u0095);
			this.\u0001[59] = new Func<_IExprement>(this.\u0093);
			this.\u0001[60] = new Func<_IExprement>(this.\u0096);
			this.\u0001[61] = new Func<_IExprement>(this.\u0097);
			this.\u0001[62] = new Func<_IExprement>(this.\u0098);
			this.\u0001[63] = new Func<_IExprement>(this.\u0099);
			this.\u0001[64] = new Func<_IExprement>(this.\u009A);
			this.\u0001[65] = new Func<_IExprement>(this.\u009B);
			this.\u0001[66] = new Func<_IExprement>(this.\u009C);
			this.\u0001[67] = new Func<_IExprement>(this.\u009D);
			this.\u0001[68] = new Func<_IExprement>(this.\u009E);
			this.\u0001[69] = new Func<_IExprement>(this.\u0001\u0002);
			this.\u0001[70] = new Func<_IExprement>(this.\u0003\u0002);
			this.\u0001[71] = new Func<_IExprement>(this.\u0004\u0002);
			this.\u0001[72] = new Func<_IExprement>(this.\u0005\u0002);
			this.\u0001[73] = new Func<_IExprement>(this.\u0006\u0002);
			this.\u0001[74] = new Func<_IExprement>(this.\u0014\u0002);
			this.\u0001[75] = new Func<_IExprement>(this.\u000E\u0002);
			this.\u0001[76] = new Func<_IExprement>(this.\u0018\u0002);
			this.\u0001[77] = new Func<_IExprement>(this.\u0019\u0002);
			this.\u0001[79] = new Func<_IExprement>(this.\u001D);
		}

		// Token: 0x06000C74 RID: 3188 RVA: 0x0001F874 File Offset: 0x0001DA74
		protected _IExpression \u0001()
		{
			return (_IExpression)this.\u0001();
		}

		// Token: 0x06000C75 RID: 3189 RVA: 0x0001F884 File Offset: 0x0001DA84
		protected _IStatement \u0001()
		{
			return (_IStatement)this.\u0001();
		}

		// Token: 0x06000C76 RID: 3190 RVA: 0x0001F894 File Offset: 0x0001DA94
		protected _IExprement \u0001()
		{
			ExprementTag exprementTag = (ExprementTag)this.Reader.ReadUInt32();
			Func<_IExprement> func;
			if (exprementTag < (ExprementTag)this.\u0001.Length)
			{
				func = this.\u0001[(int)exprementTag];
			}
			else
			{
				func = null;
			}
			if (func != null)
			{
				return func();
			}
			return this.\u0001(exprementTag);
		}

		// Token: 0x06000C77 RID: 3191 RVA: 0x0001F8D8 File Offset: 0x0001DAD8
		public ICompactedParseTreeInformation \u0001(BinaryReader \u0002)
		{
			ICompactedParseTreeInformation compactedParseTreeInformation = this.\u0001.CreateCompactedParseTreeInformation();
			global::\u000E.\u0002.\u0001(\u0002, compactedParseTreeInformation);
			global::\u000E.\u0002.\u0001(\u0002, this.\u0001, compactedParseTreeInformation.MessageTable);
			return compactedParseTreeInformation;
		}

		// Token: 0x06000C78 RID: 3192 RVA: 0x0001F90C File Offset: 0x0001DB0C
		protected static void \u0001(BinaryReader \u0002, _ILanguageModelBuilder2 \u0003, IDictionary<int, IList<_ICompilerMessage>> \u0004)
		{
			int num = \u0002.ReadInt32();
			if (num > 0)
			{
				\u001B.\u0001 u = new \u001B.\u0001();
				for (int i = 0; i < num; i++)
				{
					int key = \u0002.ReadInt32();
					int num2 = \u0002.ReadInt32();
					_ICompilerMessage[] array = new _ICompilerMessage[num2];
					for (int j = 0; j < num2; j++)
					{
						array[j] = u.\u0001(\u0002, \u0003);
					}
					\u0004.Add(key, array);
				}
			}
		}

		// Token: 0x06000C79 RID: 3193 RVA: 0x0001F978 File Offset: 0x0001DB78
		private static void \u0001(BinaryReader \u0002, ICompactedParseTreeInformation \u0003)
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

		// Token: 0x06000C7A RID: 3194 RVA: 0x0001F9E4 File Offset: 0x0001DBE4
		public virtual _IExprement \u0001(ExprementTag \u0002)
		{
			throw new FormatException(string.Format("Unknown tag {0}.", \u0002));
		}

		// Token: 0x06000C7B RID: 3195 RVA: 0x0001F9FC File Offset: 0x0001DBFC
		public _IExprement \u0002()
		{
			return null;
		}

		// Token: 0x06000C7C RID: 3196 RVA: 0x0001FA00 File Offset: 0x0001DC00
		public _IExprement \u0003()
		{
			int num = this.Reader.ReadInt32();
			_IStatement[] array = new _IStatement[num];
			for (int i = 0; i < num; i++)
			{
				int num2 = this.Reader.ReadInt32();
				_IStatement2 istatement = (_IStatement2)this.\u0001();
				istatement.Flags = (StatementFlag)num2;
				array[i] = istatement;
			}
			return this.Factory.CreateSequenceStatement(array);
		}

		// Token: 0x06000C7D RID: 3197 RVA: 0x0001FA60 File Offset: 0x0001DC60
		public _IExprement \u0004()
		{
			_IExpression expCondition = this.\u0001();
			_IStatement stateControlled = this.\u0001();
			return this.Factory.CreateWhileStatement(expCondition, stateControlled);
		}

		// Token: 0x06000C7E RID: 3198 RVA: 0x0001FA88 File Offset: 0x0001DC88
		public _IExprement \u0005()
		{
			_IExpression expCondition = this.\u0001();
			_IStatement stateControlled = this.\u0001();
			return this.Factory.CreateRepeatStatement(expCondition, stateControlled);
		}

		// Token: 0x06000C7F RID: 3199 RVA: 0x0001FAB0 File Offset: 0x0001DCB0
		public _IExprement \u0006()
		{
			_IExpression counterstart = this.\u0001();
			_IExpression upper = this.\u0001();
			_IExpression by = this.\u0001();
			_IExpression counter = this.\u0001();
			_IExpression condition = this.\u0001();
			_IStatement controlled = this.\u0001();
			return this.Factory.CreateForStatement(counterstart, counter, by, upper, condition, controlled);
		}

		// Token: 0x06000C80 RID: 3200 RVA: 0x0001FAFC File Offset: 0x0001DCFC
		public _IExprement \u0007()
		{
			return this.Factory.CreateExitStatement();
		}

		// Token: 0x06000C81 RID: 3201 RVA: 0x0001FB0C File Offset: 0x0001DD0C
		public _IExprement \u0008()
		{
			return this.Factory.CreateContinueStatement();
		}

		// Token: 0x06000C82 RID: 3202 RVA: 0x0001FB1C File Offset: 0x0001DD1C
		public _IExprement \u000E()
		{
			_IExpression condition = this.\u0001();
			_IStatement stateThen = this.\u0001();
			int num = this.Reader.ReadInt32();
			_IElseIf[] array = new _IElseIf[num];
			for (int i = 0; i < num; i++)
			{
				_IExpression expCondition = this.\u0001();
				_IStatement controlled = this.\u0001();
				_IElseIf ielseIf = this.Factory.CreateElseIf(expCondition, controlled);
				array[i] = ielseIf;
			}
			_IStatement stateElse = this.\u0001();
			return this.Factory.CreateIfStatement(condition, stateThen, stateElse, array);
		}

		// Token: 0x06000C83 RID: 3203 RVA: 0x0001FB9C File Offset: 0x0001DD9C
		public _IExprement \u000F()
		{
			_IExpression expCondition = this.\u0001();
			return this.Factory.CreateReturnStatement(expCondition);
		}

		// Token: 0x06000C84 RID: 3204 RVA: 0x0001FBBC File Offset: 0x0001DDBC
		public _IExprement \u0010()
		{
			string stLabel = this.Reader.ReadString();
			_IExpression expCondition = this.\u0001();
			return this.Factory.CreateJumpStatement(expCondition, stLabel);
		}

		// Token: 0x06000C85 RID: 3205 RVA: 0x0001FBEC File Offset: 0x0001DDEC
		public _IExprement \u0011()
		{
			string stLabel = this.Reader.ReadString();
			return this.Factory.CreateLabelStatement(stLabel);
		}

		// Token: 0x06000C86 RID: 3206 RVA: 0x0001FC14 File Offset: 0x0001DE14
		public _IExprement \u0012()
		{
			string stComment = this.Reader.ReadString();
			bool bDocComment = this.Reader.ReadBoolean();
			return this.Factory.CreateCommentStatement(stComment, bDocComment);
		}

		// Token: 0x06000C87 RID: 3207 RVA: 0x0001FC48 File Offset: 0x0001DE48
		public _IExprement \u0013()
		{
			string stPragma = this.Reader.ReadString();
			return this.Factory.CreatePragmaStatement(stPragma);
		}

		// Token: 0x06000C88 RID: 3208 RVA: 0x0001FC70 File Offset: 0x0001DE70
		public _IExprement \u0014()
		{
			string g = this.Reader.ReadString();
			string stText = this.Reader.ReadString();
			return this.Factory.CreateMessageGuidPragmaStatement(new Guid(g), stText);
		}

		// Token: 0x06000C89 RID: 3209 RVA: 0x0001FCA8 File Offset: 0x0001DEA8
		private _IExprement \u0015()
		{
			bool bOn = this.Reader.ReadBoolean();
			string stText = this.Reader.ReadString();
			return ((ITreeFactory7)this.Factory).CreateImplicitCodeSectionPragma(bOn, stText);
		}

		// Token: 0x06000C8A RID: 3210 RVA: 0x0001FCE0 File Offset: 0x0001DEE0
		private _IExprement \u0016()
		{
			int nId = this.Reader.ReadInt32();
			string stText = this.Reader.ReadString();
			return ((ITreeFactory7)this.Factory).CreateLocalSignatureIdPragma(nId, stText);
		}

		// Token: 0x06000C8B RID: 3211 RVA: 0x0001FD18 File Offset: 0x0001DF18
		public _IExprement \u0017()
		{
			bool bRestore = this.Reader.ReadBoolean();
			string stId = this.Reader.ReadString();
			string stText = this.Reader.ReadString();
			return this.Factory.CreateWarningDisableRestorePragmaStatement(bRestore, stId, stText);
		}

		// Token: 0x06000C8C RID: 3212 RVA: 0x0001FD58 File Offset: 0x0001DF58
		public _IExprement \u0018()
		{
			_IExpression exp = this.\u0001();
			return this.Factory.CreateExpressionStatement(exp);
		}

		// Token: 0x06000C8D RID: 3213 RVA: 0x0001FD78 File Offset: 0x0001DF78
		public virtual _IExprement \u0019()
		{
			Operator kindof = (Operator)this.Reader.ReadUInt32();
			_IExpression expLValue = this.\u0001();
			_IExpression expRValue = this.\u0001();
			return this.Factory.CreateAssignmentExpression(expLValue, expRValue, kindof);
		}

		// Token: 0x06000C8E RID: 3214 RVA: 0x0001FDB0 File Offset: 0x0001DFB0
		public _IExprement \u001A()
		{
			_IExpression expCallee = this.\u0001();
			_IExpression expCondition = this.\u0001();
			string text = this.Reader.ReadString();
			_IType typeExpected = null;
			if (!string.IsNullOrEmpty(text))
			{
				typeExpected = global::\u0019.\u0001.\u0001(text).ParseType();
			}
			int num = this.Reader.ReadInt32();
			_IExpression[] array = new _IExpression[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001();
			}
			int num2 = this.Reader.ReadInt32();
			_IExpression[] array2 = new _IExpression[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = this.\u0001();
			}
			int num3 = this.Reader.ReadInt32();
			_IExpression[] array3 = new _IExpression[num3];
			for (int k = 0; k < num3; k++)
			{
				array3[k] = this.\u0001();
			}
			int num4 = this.Reader.ReadInt32();
			_IExpression[] array4 = new _IExpression[num4];
			for (int l = 0; l < num4; l++)
			{
				array4[l] = this.\u0001();
			}
			int num5 = this.Reader.ReadInt32();
			_IExpression[] array5 = new _IExpression[num5];
			for (int m = 0; m < num5; m++)
			{
				array5[m] = this.\u0001();
			}
			return this.Factory.CreateCallExpression(expCallee, expCondition, typeExpected, array, array2, array4, array3, array5);
		}

		// Token: 0x06000C8F RID: 3215 RVA: 0x0001FF00 File Offset: 0x0001E100
		public virtual _IExprement \u001B()
		{
			Operator oc = (Operator)this.Reader.ReadUInt32();
			int num = this.Reader.ReadInt32();
			_IExpression[] array = new _IExpression[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001();
			}
			return this.Factory.CreateOperatorExpression(oc, array);
		}

		// Token: 0x06000C90 RID: 3216 RVA: 0x0001FF50 File Offset: 0x0001E150
		public _IExprement \u001C()
		{
			TypeClass from = (TypeClass)this.Reader.ReadUInt32();
			TypeClass to = (TypeClass)this.Reader.ReadUInt32();
			_IExpression exp = this.\u0001();
			return this.Factory.CreateConversionExpression(from, to, exp);
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x0001FF8C File Offset: 0x0001E18C
		public _IExprement \u001D()
		{
			TypeClass from = (TypeClass)this.Reader.ReadUInt32();
			TypeClass to = (TypeClass)this.Reader.ReadUInt32();
			_IExpression expression = this.\u0001();
			return ((ITreeFactory6)this.Factory).CreateImplicitConversionExpression(from, to, expression);
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0001FFCC File Offset: 0x0001E1CC
		public _IExprement \u001E()
		{
			return this.Factory.CreateThisExpression();
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x0001FFDC File Offset: 0x0001E1DC
		public _IExprement \u001F()
		{
			return this.Factory.CreateBaseExpression();
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x0001FFEC File Offset: 0x0001E1EC
		public virtual _IExprement \u007F()
		{
			TypeClass constantType = (TypeClass)this.Reader.ReadUInt32();
			long lValue = this.Reader.ReadInt64();
			bool bNegative = this.Reader.ReadBoolean();
			return this.Factory.CreateIntegerLiteralExpression(lValue, constantType, bNegative);
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x0002002C File Offset: 0x0001E22C
		public virtual _IExprement \u0080()
		{
			TypeClass constantType = (TypeClass)this.Reader.ReadUInt32();
			long lValue = this.Reader.ReadInt64();
			int nbase = this.Reader.ReadInt32();
			bool bNegative = this.Reader.ReadBoolean();
			return this.Factory.CreateBasedIntegerLiteralExpression(lValue, constantType, nbase, bNegative);
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x00020078 File Offset: 0x0001E278
		public virtual _IExprement \u0081()
		{
			TypeClass constantType = (TypeClass)this.Reader.ReadUInt32();
			string stValue = this.Reader.ReadString();
			StringEncoding stringEncoding = (StringEncoding)this.Reader.ReadUInt32();
			return ((ITreeFactory3)this.Factory).CreateStringLiteralExpression(stValue, constantType, stringEncoding);
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x000200BC File Offset: 0x0001E2BC
		public virtual _IExprement \u0082()
		{
			TypeClass constantType = (TypeClass)this.Reader.ReadUInt32();
			double dValue = this.Reader.ReadDouble();
			return this.Factory.CreateFloatLiteralExpression(dValue, constantType);
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x000200F0 File Offset: 0x0001E2F0
		public _IExprement \u0083()
		{
			IExpression expression = global::\u0019.\u0001.\u0001(this.Reader.ReadString()).ParseExpression();
			Debug.\u0001(expression is IAddressExpression);
			return this.Factory.CreateAddressExpression(((IAddressExpression)expression).DirectAddress);
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x00020138 File Offset: 0x0001E338
		public virtual _IExprement \u0084()
		{
			string stName = this.Reader.ReadString();
			return this.Factory.CreateVariableExpression(stName);
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x00020160 File Offset: 0x0001E360
		public virtual _IExprement \u0086()
		{
			_IExpression expBase = this.\u0001();
			int num = this.Reader.ReadInt32();
			_IExpression[] array = new _IExpression[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001();
			}
			return this.Factory.CreateIndexAccessExpression(expBase, array);
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x000201AC File Offset: 0x0001E3AC
		public virtual _IExprement \u0087()
		{
			_IExpression expLeft = this.\u0001();
			_IExpression expRight = this.\u0001();
			return this.Factory.CreateCompoAccessExpression(expLeft, expRight);
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x000201D4 File Offset: 0x0001E3D4
		public virtual _IExprement \u0088()
		{
			return this.Factory.CreateDeRefAccessExpression(this.\u0001());
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x000201E8 File Offset: 0x0001E3E8
		public _IExprement \u0089()
		{
			return this.Factory.CreateGlobalScopeExpression(this.\u0001());
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x000201FC File Offset: 0x0001E3FC
		public _IExprement \u008A()
		{
			return this.Factory.CreateSystemScopeExpression(this.\u0001());
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x00020210 File Offset: 0x0001E410
		public _IExprement \u008B()
		{
			return this.Factory.CreateEmptyStatement();
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x00020220 File Offset: 0x0001E420
		public _IExprement \u008C()
		{
			_IExpression expHigh = this.\u0001();
			_IExpression expLow = this.\u0001();
			return this.Factory.CreateCaseRangeExpression(expLow, expHigh);
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x00020248 File Offset: 0x0001E448
		public _IExprement \u008D()
		{
			int num = this.Reader.ReadInt32();
			_IExpression[] array = new _IExpression[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = this.\u0001();
			}
			return this.Factory.CreateCaseLabelStatement(array);
		}

		// Token: 0x06000CA2 RID: 3234 RVA: 0x0002028C File Offset: 0x0001E48C
		public _IExprement \u008E()
		{
			_IExpression expswitch = this.\u0001();
			int num = this.Reader.ReadInt32();
			_ICase[] array = new _ICase[num];
			for (int i = 0; i < num; i++)
			{
				_ICaseLabelStatement caselabel = (_ICaseLabelStatement)this.\u0001();
				_IStatement controlled = this.\u0001();
				_ICase icase = this.Factory.CreateCase(caselabel, controlled);
				array[i] = icase;
			}
			_IStatement elsecase = this.\u0001();
			return this.Factory.CreateCaseStatement(expswitch, array, elsecase);
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x00020304 File Offset: 0x0001E504
		public _IExprement \u008F()
		{
			return this.Factory.CreateErrorExpression();
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x00020314 File Offset: 0x0001E514
		public _IExprement \u0090()
		{
			return this.Factory.CreateErrorStatement();
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x00020324 File Offset: 0x0001E524
		public _IExprement \u0091()
		{
			return this.Factory.CreateNullExpression();
		}

		// Token: 0x06000CA6 RID: 3238 RVA: 0x00020334 File Offset: 0x0001E534
		public _IExprement \u0092()
		{
			return this.Factory.CreateNullStatement();
		}

		// Token: 0x06000CA7 RID: 3239 RVA: 0x00020344 File Offset: 0x0001E544
		public _IExprement \u0093()
		{
			_IExpression expNumber = this.\u0001();
			_IExpression expValue = this.\u0001();
			return this.Factory.CreateMultipleIndexInitialisation(expValue, expNumber);
		}

		// Token: 0x06000CA8 RID: 3240 RVA: 0x0002036C File Offset: 0x0001E56C
		public _IExprement \u0094()
		{
			int num = this.Reader.ReadInt32();
			LList<_IExpression> llist = new LList<_IExpression>();
			for (int i = 0; i < num; i++)
			{
				llist.Add(this.\u0001());
			}
			return this.Factory.CreateArrayInitialisation(llist);
		}

		// Token: 0x06000CA9 RID: 3241 RVA: 0x000203B0 File Offset: 0x0001E5B0
		public _IExprement \u0095()
		{
			int num = this.Reader.ReadInt32();
			LList<_IAssignmentExpression> llist = new LList<_IAssignmentExpression>();
			for (int i = 0; i < num; i++)
			{
				llist.Add((_IAssignmentExpression)this.\u0001());
			}
			return this.Factory.CreateStructureInitialisation(llist);
		}

		// Token: 0x06000CAA RID: 3242 RVA: 0x000203F8 File Offset: 0x0001E5F8
		public _IExprement \u0096()
		{
			return this.Factory.CreateDefineReference(this.Reader.ReadString());
		}

		// Token: 0x06000CAB RID: 3243 RVA: 0x00020410 File Offset: 0x0001E610
		public _IExprement \u0097()
		{
			return this.Factory.CreateVariableReference(this.\u0001());
		}

		// Token: 0x06000CAC RID: 3244 RVA: 0x00020424 File Offset: 0x0001E624
		public _IExprement \u0098()
		{
			return this.Factory.CreateTypeReference(this.\u0001());
		}

		// Token: 0x06000CAD RID: 3245 RVA: 0x00020438 File Offset: 0x0001E638
		public _IExprement \u0099()
		{
			return this.Factory.CreatePouReference(this.\u0001());
		}

		// Token: 0x06000CAE RID: 3246 RVA: 0x0002044C File Offset: 0x0001E64C
		public _IExprement \u009A()
		{
			return this.Factory.CreateTaskReference(this.Reader.ReadString());
		}

		// Token: 0x06000CAF RID: 3247 RVA: 0x00020464 File Offset: 0x0001E664
		public _IExprement \u009B()
		{
			return this.Factory.CreateResourceReference(this.Reader.ReadString());
		}

		// Token: 0x06000CB0 RID: 3248 RVA: 0x0002047C File Offset: 0x0001E67C
		public _IExprement \u009C()
		{
			return this.Factory.CreateDefinedExpression(this.\u0001() as _IItemReference);
		}

		// Token: 0x06000CB1 RID: 3249 RVA: 0x00020494 File Offset: 0x0001E694
		public _IExprement \u009D()
		{
			_IItemReference itref = this.\u0001() as _IItemReference;
			_IItemReference itrefFrom = this.\u0001() as _IItemReference;
			return this.Factory.CreateXRefExpression(itref, itrefFrom);
		}

		// Token: 0x06000CB2 RID: 3250 RVA: 0x000204C8 File Offset: 0x0001E6C8
		public _IExprement \u009E()
		{
			string version = this.Reader.ReadString();
			return this.Factory.CreateCompilerVersionExpression(new Version(version), (Operator)this.Reader.ReadUInt32());
		}

		// Token: 0x06000CB3 RID: 3251 RVA: 0x00020500 File Offset: 0x0001E700
		public _IExprement \u009F()
		{
			PragmaOperator op = (PragmaOperator)this.Reader.ReadUInt32();
			int num = this.Reader.ReadInt32();
			_IExpression[] array = null;
			if (num > 0)
			{
				array = new _IExpression[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = this.\u0001();
				}
			}
			return this.Factory.CreatePragmaOperatorExpression(op, array);
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x00020554 File Offset: 0x0001E754
		public _IExprement \u0001\u0002()
		{
			_IExpression expCond = this.\u0001();
			_IStatement ifthen = this.\u0001();
			_IPragmaElseIf[] array = null;
			int num = this.Reader.ReadInt32();
			if (num > 0)
			{
				array = new _IPragmaElseIf[num];
				for (int i = 0; i < num; i++)
				{
					_IPragmaExpression expCondition = this.\u0001() as _IPragmaExpression;
					_IStatement stControlled = this.\u0001();
					_IPragmaElseIf ipragmaElseIf = this.Factory.CreatePragmaElseIf(expCondition, stControlled);
					array[i] = ipragmaElseIf;
				}
			}
			_IStatement ifelse = this.\u0001();
			return this.Factory.CreatePragmaIfStatement(expCond, ifthen, ifelse, array);
		}

		// Token: 0x06000CB5 RID: 3253 RVA: 0x000205E0 File Offset: 0x0001E7E0
		public _IExprement \u0002\u0002()
		{
			return this.Factory.CreateDefineStatement(this.Reader.ReadBoolean(), this.Reader.ReadString(), this.Reader.ReadString());
		}

		// Token: 0x06000CB6 RID: 3254 RVA: 0x00020610 File Offset: 0x0001E810
		public _IExprement \u0003\u0002()
		{
			_IType type = global::\u0019.\u0001.\u0001(this.Reader.ReadString()).ParseType();
			_IVariableReference varref = this.\u0001() as _IVariableReference;
			return this.Factory.CreateHasCompatibleTypeExpression(varref, type);
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0002064C File Offset: 0x0001E84C
		public _IExprement \u0004\u0002()
		{
			_IType type = global::\u0019.\u0001.\u0001(this.Reader.ReadString()).ParseType();
			_IVariableReference varref = this.\u0001() as _IVariableReference;
			return this.Factory.CreateHasTypeExpression(varref, type);
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x00020688 File Offset: 0x0001E888
		public _IExprement \u0005\u0002()
		{
			_IParser iparser = global::\u0019.\u0001.\u0001(this.Reader.ReadString());
			return this.Factory.CreateIsEnumTypeExpression(iparser.ParseType());
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x000206B8 File Offset: 0x0001E8B8
		public _IExprement \u0006\u0002()
		{
			string stAttribute = this.Reader.ReadString();
			_IItemReference itref = this.\u0001() as _IItemReference;
			return this.Factory.CreateHasAttributeExpression(itref, stAttribute);
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x000206EC File Offset: 0x0001E8EC
		public _IExprement \u0007\u0002()
		{
			return this.Factory.CreateHasValueExpression(this.Reader.ReadString(), this.Reader.ReadString());
		}

		// Token: 0x06000CBB RID: 3259 RVA: 0x00020710 File Offset: 0x0001E910
		public _IExprement \u0008\u0002()
		{
			Operator opComparison = (Operator)this.Reader.ReadUInt32();
			_IExpression constant = this.\u0001();
			_IExpression value = this.\u0001();
			return this.Factory.CreateHasConstantValueExpression(constant, value, opComparison);
		}

		// Token: 0x06000CBC RID: 3260 RVA: 0x00020748 File Offset: 0x0001E948
		public _IExprement \u000E\u0002()
		{
			_IExpression constant = this.\u0001();
			bool bConstantTypeReplaced = this.Reader.ReadBoolean();
			return ((ITreeFactory3)this.Factory).CreateHasConstantTypeExpression(constant, bConstantTypeReplaced);
		}

		// Token: 0x06000CBD RID: 3261 RVA: 0x0002077C File Offset: 0x0001E97C
		public _IExprement \u000F\u0002()
		{
			string errorOutput = this.Reader.ReadString();
			_IExpression condition = this.\u0001();
			return this.Factory.CreatePragmaAssertion(condition, errorOutput);
		}

		// Token: 0x06000CBE RID: 3262 RVA: 0x000207AC File Offset: 0x0001E9AC
		public _IExprement \u0010\u0002()
		{
			string text = this.Reader.ReadString();
			_IParser iparser = global::\u0019.\u0001.\u0001(text);
			_IType type = null;
			if (!string.IsNullOrEmpty(text))
			{
				type = iparser.ParseType();
			}
			_IExpression expBase = this.\u0001();
			_IExpression expWithType = this.\u0001();
			return this.Factory.CreateCastExpression(expWithType, expBase, type);
		}

		// Token: 0x06000CBF RID: 3263 RVA: 0x000207F8 File Offset: 0x0001E9F8
		public _IExprement \u0011\u0002()
		{
			_IType typeIn = global::\u0019.\u0001.\u0001(this.Reader.ReadString()).ParseType();
			_IExpression expCount = this.\u0001();
			int num = this.Reader.ReadInt32();
			IAssignmentExpression[] array = null;
			if (num != 0)
			{
				array = new IAssignmentExpression[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = (IAssignmentExpression)this.\u0001();
				}
			}
			return this.Factory.CreateNewExpression(typeIn, expCount, array);
		}

		// Token: 0x06000CC0 RID: 3264 RVA: 0x00020868 File Offset: 0x0001EA68
		public _IExprement \u0012\u0002()
		{
			_IParser iparser = global::\u0019.\u0001.\u0001(this.Reader.ReadString());
			return this.Factory.CreateTypeExpression(iparser.ParseType());
		}

		// Token: 0x06000CC1 RID: 3265 RVA: 0x00020898 File Offset: 0x0001EA98
		public _IExprement \u0013\u0002()
		{
			_IExpression expAccess = this.\u0001();
			_IExpression expNamespace = this.\u0001();
			return this.Factory.CreateNamespaceAccessExpression(expNamespace, expAccess);
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x000208C0 File Offset: 0x0001EAC0
		public _IExprement \u0014\u0002()
		{
			string version = this.Reader.ReadString();
			Operator test = (Operator)this.Reader.ReadUInt32();
			return this.Factory.CreateRuntimeVersionExpression(new Version(version), test);
		}

		// Token: 0x06000CC3 RID: 3267 RVA: 0x000208F8 File Offset: 0x0001EAF8
		public _IExprement \u0015\u0002()
		{
			return this.Factory.CreateCurrentTaskExpression(this.\u0001());
		}

		// Token: 0x06000CC4 RID: 3268 RVA: 0x0002090C File Offset: 0x0001EB0C
		public _IExprement \u0016\u0002()
		{
			return this.Factory.CreatePoolScopeExpression(this.\u0001());
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x00020920 File Offset: 0x0001EB20
		public _IExprement \u0017\u0002()
		{
			_ISequenceStatement seqTry = (_ISequenceStatement)this.\u0001();
			_ISequenceStatement seqCatch = (_ISequenceStatement)this.\u0001();
			_IExpression expException = this.\u0001();
			_ISequenceStatement seqFinally = (_ISequenceStatement)this.\u0001();
			return this.Factory.CreateTryCatchStatement(seqTry, seqCatch, seqFinally, expException);
		}

		// Token: 0x06000CC6 RID: 3270 RVA: 0x00020968 File Offset: 0x0001EB68
		public _IExprement \u0018\u0002()
		{
			_IExpression left = this.\u0001();
			DirectVariableSize partSize = (DirectVariableSize)this.Reader.ReadInt32();
			int partOffset = this.Reader.ReadInt32();
			return ((ITreeFactory4)this.Factory).CreatePartialAccessExpression(left, partSize, partOffset);
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x000209A8 File Offset: 0x0001EBA8
		public _IExprement \u0019\u0002()
		{
			_IDefineReference defineReference = (_IDefineReference)this.\u0001();
			return ((ITreeFactory5)this.Factory).CreateProjectDefinedExpression(defineReference);
		}

		// Token: 0x06000CC8 RID: 3272 RVA: 0x000209D4 File Offset: 0x0001EBD4
		public _IExprement \u001A\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CC9 RID: 3273 RVA: 0x000209DC File Offset: 0x0001EBDC
		public _IExprement \u001B\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCA RID: 3274 RVA: 0x000209E4 File Offset: 0x0001EBE4
		public _IExprement \u001C\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCB RID: 3275 RVA: 0x000209EC File Offset: 0x0001EBEC
		public _IExprement \u001D\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCC RID: 3276 RVA: 0x000209F4 File Offset: 0x0001EBF4
		public _IExprement \u001E\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCD RID: 3277 RVA: 0x000209FC File Offset: 0x0001EBFC
		public _IExprement \u001F\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCE RID: 3278 RVA: 0x00020A04 File Offset: 0x0001EC04
		public _IExprement \u007F\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CCF RID: 3279 RVA: 0x00020A0C File Offset: 0x0001EC0C
		public _IExprement \u0080\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CD0 RID: 3280 RVA: 0x00020A14 File Offset: 0x0001EC14
		public _IExprement \u0081\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06000CD1 RID: 3281 RVA: 0x00020A1C File Offset: 0x0001EC1C
		public _IExprement \u0082\u0002()
		{
			throw new NotImplementedException();
		}

		// Token: 0x04000226 RID: 550
		[CompilerGenerated]
		private BinaryReader \u0001;

		// Token: 0x04000227 RID: 551
		[CompilerGenerated]
		private ITreeFactory \u0001;

		// Token: 0x04000228 RID: 552
		protected \u001B.\u0001 \u0001;

		// Token: 0x04000229 RID: 553
		protected readonly _ILanguageModelBuilder3 \u0001 = (_ILanguageModelBuilder3)APEnvironmentFacade.Instance.LanguageModelMgr.CreateLanguageModelBuilder();

		// Token: 0x0400022A RID: 554
		private readonly Func<_IExprement>[] \u0001;
	}
}
