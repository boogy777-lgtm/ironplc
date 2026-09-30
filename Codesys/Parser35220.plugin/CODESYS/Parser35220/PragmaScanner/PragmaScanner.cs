using System;
using System.Collections;
using CODESYS.Parser;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.PragmaScanner
{
	// Token: 0x02000019 RID: 25
	internal class PragmaScanner : IPragmaScanner
	{
		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060001E6 RID: 486 RVA: 0x0000B483 File Offset: 0x00009683
		// (set) Token: 0x060001E7 RID: 487 RVA: 0x0000B48B File Offset: 0x0000968B
		public int PositionOffset { get; private set; }

		// Token: 0x060001E8 RID: 488 RVA: 0x0000B494 File Offset: 0x00009694
		internal PragmaScanner(IScanner9 scanner)
		{
			this.m_scanner = scanner;
		}

		// Token: 0x060001E9 RID: 489 RVA: 0x0000B4A3 File Offset: 0x000096A3
		internal PragmaScanner(IScanner9 scanner, IToken token) : this(scanner)
		{
			this.PositionOffset = (int)token.PositionOffset;
		}

		// Token: 0x060001EA RID: 490 RVA: 0x0000B4B8 File Offset: 0x000096B8
		public void Reset(string stPragma, IToken tokenPragma)
		{
			this.PositionOffset = (int)tokenPragma.PositionOffset;
			this.m_scanner.Initialize(stPragma);
			this.m_scanner.AllowMultipleUnderlines = true;
		}

		// Token: 0x060001EB RID: 491 RVA: 0x0000B4E0 File Offset: 0x000096E0
		static PragmaScanner()
		{
			PragmaScanner.s_htOperators_lockRequired["flow"] = 1;
			PragmaScanner.s_htOperators_lockRequired["noflow"] = 2;
			PragmaScanner.s_htOperators_lockRequired["bp"] = 3;
			PragmaScanner.s_htOperators_lockRequired["nobp"] = 4;
			PragmaScanner.s_htOperators_lockRequired["bp2"] = 57;
			PragmaScanner.s_htOperators_lockRequired["nobp2"] = 58;
			PragmaScanner.s_htOperators_lockRequired["p"] = 5;
			PragmaScanner.s_htOperators_lockRequired["bpdef"] = 6;
			PragmaScanner.s_htOperators_lockRequired["succ"] = 7;
			PragmaScanner.s_htOperators_lockRequired["error"] = 8;
			PragmaScanner.s_htOperators_lockRequired["fatalerror"] = 56;
			PragmaScanner.s_htOperators_lockRequired["warning"] = 9;
			PragmaScanner.s_htOperators_lockRequired["info"] = 10;
			PragmaScanner.s_htOperators_lockRequired["text"] = 11;
			PragmaScanner.s_htOperators_lockRequired["attribute"] = 12;
			PragmaScanner.s_htOperators_lockRequired["define"] = 13;
			PragmaScanner.s_htOperators_lockRequired["undefine"] = 14;
			PragmaScanner.s_htOperators_lockRequired["include"] = 15;
			PragmaScanner.s_htOperators_lockRequired["defined"] = 16;
			PragmaScanner.s_htOperators_lockRequired["project_defined"] = 61;
			PragmaScanner.s_htOperators_lockRequired["variable"] = 17;
			PragmaScanner.s_htOperators_lockRequired["type"] = 18;
			PragmaScanner.s_htOperators_lockRequired["task"] = 19;
			PragmaScanner.s_htOperators_lockRequired["xref"] = 20;
			PragmaScanner.s_htOperators_lockRequired["resource"] = 21;
			PragmaScanner.s_htOperators_lockRequired["implicit"] = 28;
			PragmaScanner.s_htOperators_lockRequired["allowpaths"] = 53;
			PragmaScanner.s_htOperators_lockRequired["messageguid"] = 31;
			PragmaScanner.s_htOperators_lockRequired["on"] = 29;
			PragmaScanner.s_htOperators_lockRequired["off"] = 30;
			PragmaScanner.s_htOperators_lockRequired["from"] = 22;
			PragmaScanner.s_htOperators_lockRequired["pou"] = 23;
			PragmaScanner.s_htOperators_lockRequired["hastype"] = 24;
			PragmaScanner.s_htOperators_lockRequired["hasattribute"] = 25;
			PragmaScanner.s_htOperators_lockRequired["assert"] = 33;
			PragmaScanner.s_htOperators_lockRequired["hasvalue"] = 26;
			PragmaScanner.s_htOperators_lockRequired["hasconstantvalue"] = 27;
			PragmaScanner.s_htOperators_lockRequired["hasconstanttype"] = 60;
			PragmaScanner.s_htOperators_lockRequired["OR"] = 43;
			PragmaScanner.s_htOperators_lockRequired["AND"] = 44;
			PragmaScanner.s_htOperators_lockRequired["NOT"] = 45;
			PragmaScanner.s_htOperators_lockRequired["show_compile"] = 34;
			PragmaScanner.s_htOperators_lockRequired["show_precompile"] = 35;
			PragmaScanner.s_htOperators_lockRequired["isenumtype"] = 52;
			PragmaScanner.s_htOperators_lockRequired["disable"] = 54;
			PragmaScanner.s_htOperators_lockRequired["restore"] = 55;
			PragmaScanner.s_htOperators_lockRequired["__INTERNAL_POINTEROP"] = 50;
			PragmaScanner.s_htOperators_lockRequired["COMPILERVERSION"] = 51;
			PragmaScanner.s_htOperators_lockRequired["RUNTIMEVERSION"] = 59;
		}

		// Token: 0x060001EC RID: 492 RVA: 0x0000B928 File Offset: 0x00009B28
		public PragmaTokenType GetNext(out IPragmaToken token)
		{
			IToken token2;
			this.m_scanner.GetNext(ref token2);
			PragmaToken pragmaToken = new PragmaToken(token2)
			{
				_type = 5
			};
			token = pragmaToken;
			TokenType type = token2.Type;
			if (type != 1)
			{
				switch (type)
				{
				case 14:
				{
					pragmaToken._type = 2;
					ulong lInteger;
					bool flag;
					Operator @operator;
					bool flag2;
					this.m_scanner.GetInteger(token2, ref lInteger, ref flag, ref @operator, ref flag2);
					pragmaToken._lInteger = (long)lInteger;
					goto IL_326;
				}
				case 15:
				{
					Operator operator2 = this.m_scanner.GetOperator(token2);
					if (operator2 <= 89)
					{
						if (operator2 <= 69)
						{
							if (operator2 == 68)
							{
								pragmaToken._type = 3;
								pragmaToken._pop = 38;
								goto IL_326;
							}
							if (operator2 != 69)
							{
								goto IL_326;
							}
							pragmaToken._type = 3;
							pragmaToken._pop = 37;
							goto IL_326;
						}
						else
						{
							if (operator2 == 75)
							{
								pragmaToken._type = 3;
								pragmaToken._pop = 39;
								goto IL_326;
							}
							if (operator2 == 86)
							{
								pragmaToken._type = 3;
								pragmaToken._pop = 22;
								goto IL_326;
							}
							if (operator2 != 89)
							{
								goto IL_326;
							}
							pragmaToken._type = 3;
							pragmaToken._pop = 36;
							goto IL_326;
						}
					}
					else if (operator2 <= 127)
					{
						if (operator2 == 103)
						{
							pragmaToken._type = 3;
							pragmaToken._pop = 18;
							goto IL_326;
						}
						if (operator2 != 127)
						{
							goto IL_326;
						}
						pragmaToken._type = 3;
						pragmaToken._pop = 44;
						goto IL_326;
					}
					else
					{
						if (operator2 == 129)
						{
							pragmaToken._type = 3;
							pragmaToken._pop = 43;
							goto IL_326;
						}
						if (operator2 == 133)
						{
							pragmaToken._type = 3;
							pragmaToken._pop = 45;
							goto IL_326;
						}
						switch (operator2)
						{
						case 162:
							pragmaToken._type = 3;
							pragmaToken._pop = 46;
							goto IL_326;
						case 163:
							pragmaToken._type = 3;
							pragmaToken._pop = 42;
							goto IL_326;
						case 164:
							pragmaToken._type = 3;
							pragmaToken._pop = 32;
							goto IL_326;
						case 165:
						case 166:
						case 169:
						case 170:
							goto IL_326;
						case 167:
							pragmaToken._type = 3;
							pragmaToken._pop = 40;
							goto IL_326;
						case 168:
							pragmaToken._type = 3;
							pragmaToken._pop = 41;
							goto IL_326;
						case 171:
							pragmaToken._type = 3;
							pragmaToken._pop = 47;
							goto IL_326;
						default:
							goto IL_326;
						}
					}
					break;
				}
				case 16:
					break;
				case 17:
					pragmaToken._type = 4;
					pragmaToken._stString = this.m_scanner.GetSingleByteString(token2);
					goto IL_326;
				default:
					if (type == 21)
					{
						pragmaToken._type = 6;
						goto IL_326;
					}
					break;
				}
				string tokenText = this.m_scanner.GetTokenText(token2);
				Hashtable obj = PragmaScanner.s_htOperators_lockRequired;
				lock (obj)
				{
					if (PragmaScanner.s_htOperators_lockRequired[tokenText] != null)
					{
						PragmaOperator pop = (PragmaOperator)PragmaScanner.s_htOperators_lockRequired[tokenText];
						pragmaToken._type = 3;
						pragmaToken._pop = pop;
					}
					else
					{
						pragmaToken._type = 1;
						pragmaToken._stIdent = this.m_scanner.GetTokenText(token2);
					}
				}
			}
			else
			{
				pragmaToken._type = 3;
				if (this.m_scanner.GetBoolean(token2))
				{
					pragmaToken._pop = 48;
				}
				else
				{
					pragmaToken._pop = 49;
				}
			}
			IL_326:
			return pragmaToken._type;
		}

		// Token: 0x060001ED RID: 493 RVA: 0x0000BC74 File Offset: 0x00009E74
		public void SetPosition(IPragmaToken pt)
		{
			this.m_scanner.SetPosition(pt.OrgToken);
		}

		// Token: 0x060001EE RID: 494 RVA: 0x0000BC87 File Offset: 0x00009E87
		public string GetTokenText(IPragmaToken pt)
		{
			return this.m_scanner.GetTokenText(pt.OrgToken);
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060001EF RID: 495 RVA: 0x0000BC9A File Offset: 0x00009E9A
		public IScanner9 OrgScanner
		{
			get
			{
				return this.m_scanner;
			}
		}

		// Token: 0x0400005C RID: 92
		private static readonly Hashtable s_htOperators_lockRequired = new Hashtable();

		// Token: 0x0400005D RID: 93
		private readonly IScanner9 m_scanner;
	}
}
