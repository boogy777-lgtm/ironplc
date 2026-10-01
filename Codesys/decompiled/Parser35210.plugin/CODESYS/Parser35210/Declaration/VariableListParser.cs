using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Declaration
{
	internal readonly struct VariableListParser
	{
		private ParserContext Context { get; }

		private Operator OpCurrentVariableList => DeclarationParser.OpCurrentVariableList;

		private DeclarationParser DeclarationParser => Context.DeclarationParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private VariableListParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IVariableDeclarationListStatement ParseVariableList(ParserContext context, IToken token)
		{
			return new VariableListParser(context).ParseVariableList(token);
		}

		private void ParseReSyncIF()
		{
			Scanner.ParseReSyncIF();
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private TokenType Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			return Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private _IVariableDeclarationListStatement ParseVariableList(IToken tokenVar)
		{
			VarFlag vf = VarFlag.Temp;
			DeclarationParser.OpCurrentVariableList = Scanner.GetOperator(tokenVar);
			_IVariableDeclarationListStatement iVariableDeclarationListStatement = LMItemFactory.CreateVariableDeclarationListStatement(tokenVar);
			SetVariableFlagByOperator(ref vf, OpCurrentVariableList);
			ScanForAdditionalFlags(ref vf);
			iVariableDeclarationListStatement.Flags = vf;
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement();
			while (true)
			{
				if (Next(out var token, bWithPragma: true, bWithComment: true) == TokenType.Operator && (Scanner.GetOperator(token) == Operator.EndVar || Scanner.GetOperator(token) == Operator.EndStruct || Scanner.GetOperator(token) == Operator.EndUnion))
				{
					Scanner.SetPosition(token);
					break;
				}
				if (token.Type == TokenType.End)
				{
					break;
				}
				Scanner.SetPosition(token);
				bool inDeclaration = Context.StatementParser.InDeclaration;
				Context.StatementParser.InDeclaration = true;
				bool bError;
				_IStatement iStatement = Context.StatementParser.ParseSTStatement(out bError, bTopLevel: false);
				Context.StatementParser.InDeclaration = inDeclaration;
				if (bError)
				{
					ParseReSyncIF();
				}
				iSequenceStatement.Add(iStatement);
				iStatement.SetFlag(StatementFlag.Library, DeclarationParser.Library);
			}
			iVariableDeclarationListStatement.VariableDeclaration = iSequenceStatement;
			DeclarationParser.OpCurrentVariableList = Operator.None;
			return iVariableDeclarationListStatement;
		}

		private void ScanForAdditionalFlags(ref VarFlag vf)
		{
			IToken token;
			while (true)
			{
				Next(out token, bWithPragma: true, bWithComment: true);
				Scanner.SetPosition(token);
				if (Next(out var token2) != TokenType.Operator)
				{
					Scanner.SetPosition(token);
					return;
				}
				switch (Scanner.GetOperator(token2))
				{
				case Operator.Constant:
					vf |= VarFlag.Constant;
					vf |= VarFlag.ReplacedConstant;
					if ((vf & VarFlag.Inout) == VarFlag.Inout)
					{
						vf &= ~VarFlag.ReplacedConstant;
					}
					continue;
				case Operator.Retain:
					vf |= VarFlag.Retain;
					continue;
				case Operator.Persistent:
					vf |= VarFlag.Persistent;
					continue;
				case Operator.Internal:
					if ((vf & VarFlag.Global) == VarFlag.Global)
					{
						vf |= VarFlag.Internal;
						continue;
					}
					break;
				}
				break;
			}
			Scanner.SetPosition(token);
		}

		private static void SetVariableFlagByOperator(ref VarFlag vf, Operator op)
		{
			switch (op)
			{
			case Operator.VarConfig:
				vf = VarFlag.VarConfig;
				break;
			case Operator.VarGlobal:
				vf = VarFlag.Global | VarFlag.Absolut;
				break;
			case Operator.Struct:
				vf = VarFlag.Local | VarFlag.Structure;
				break;
			case Operator.Union:
				vf = VarFlag.Local | VarFlag.Structure | VarFlag.Union;
				break;
			case Operator.Var:
				vf = VarFlag.Local;
				break;
			case Operator.VarStat:
				vf = VarFlag.Absolut | VarFlag.Static;
				break;
			case Operator.VarInput:
				vf = VarFlag.Input;
				break;
			case Operator.VarOutput:
				vf = VarFlag.Output;
				break;
			case Operator.VarInOut:
				vf = VarFlag.Inout;
				break;
			case Operator.VarExternal:
				vf = VarFlag.External | VarFlag.Absolut;
				break;
			case Operator.VarTemp:
				vf = VarFlag.Temp;
				break;
			case Operator.VarInst:
				vf = VarFlag.AllocateInInstance;
				break;
			case Operator.VarGeneric:
				vf = VarFlag.Generic;
				break;
			}
		}
	}
}
