using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using CODESYS.WhiteParseTrees.Nodes.Tokens;

namespace CODESYS.WhiteParseTrees.Nodes.Factories
{
	public static class OperatorTokenFactory
	{
		[System.Runtime.CompilerServices.NullableContext(1)]
		public static IWhiteOperatorToken CreateOperatorTokenByOperator(string stText, Operator @operator)
		{
			switch (@operator)
			{
			case Operator.__Reloc:
				return new __RelocToken(stText, @operator);
			case Operator.__Copy:
				return new __CopyToken(stText, @operator);
			case Operator.__Lazy:
				return new __LazyToken(stText, @operator);
			case Operator.Any:
				return new AnySimpleTypeToken(stText, @operator);
			case Operator.AnyBit:
				return new AnyBitSimpleTypeToken(stText, @operator);
			case Operator.AnyDate:
				return new AnyDateSimpleTypeToken(stText, @operator);
			case Operator.AnyInt:
				return new AnyIntSimpleTypeToken(stText, @operator);
			case Operator.AnyNum:
				return new AnyNumSimpleTypeToken(stText, @operator);
			case Operator.AnyReal:
				return new AnyRealSimpleTypeToken(stText, @operator);
			case Operator.Bit:
				return new BitSimpleTypeToken(stText, @operator);
			case Operator.Bool:
				return new BoolSimpleTypeToken(stText, @operator);
			case Operator.Byte:
				return new ByteSimpleTypeToken(stText, @operator);
			case Operator.Word:
				return new WordSimpleTypeToken(stText, @operator);
			case Operator.DWord:
				return new IdWordSimpleTypeToken(stText, @operator);
			case Operator.LWord:
				return new IlWordSimpleTypeToken(stText, @operator);
			case Operator.SInt:
				return new IsIntSimpleTypeToken(stText, @operator);
			case Operator.Int:
				return new IntSimpleTypeToken(stText, @operator);
			case Operator.DInt:
				return new IdIntSimpleTypeToken(stText, @operator);
			case Operator.LInt:
				return new IlIntSimpleTypeToken(stText, @operator);
			case Operator.USInt:
				return new UsIntSimpleTypeToken(stText, @operator);
			case Operator.UInt:
				return new IuIntSimpleTypeToken(stText, @operator);
			case Operator.UDInt:
				return new UdIntSimpleTypeToken(stText, @operator);
			case Operator.ULInt:
				return new UlIntSimpleTypeToken(stText, @operator);
			case Operator.Real:
				return new RealSimpleTypeToken(stText, @operator);
			case Operator.LReal:
				return new IlRealSimpleTypeToken(stText, @operator);
			case Operator.String:
				return new StringSimpleTypeToken(stText, @operator);
			case Operator.WString:
				return new WStringSimpleTypeToken(stText, @operator);
			case Operator.Time:
				return new TimeSimpleTypeToken(stText, @operator);
			case Operator.LTime:
				return new IlTimeSimpleTypeToken(stText, @operator);
			case Operator.Date:
				return new DateSimpleTypeToken(stText, @operator);
			case Operator.DateAndTime:
				return new DateAndTimeSimpleTypeToken(stText, @operator);
			case Operator.TimeOfDay:
				return new TimeOfDaySimpleTypeToken(stText, @operator);
			case Operator.Adr:
				return new AdrToken(stText, @operator);
			case Operator.BitAdr:
				return new BitAdrToken(stText, @operator);
			case Operator.IndexOf:
				return new IndexOfToken(stText, @operator);
			case Operator.SizeOf:
				return new SizeOfToken(stText, @operator);
			case Operator.Ini:
				return new IniToken(stText, @operator);
			case Operator.Abs:
				return new AbsToken(stText, @operator);
			case Operator.Limit:
				return new LimitToken(stText, @operator);
			case Operator.Min:
				return new MinToken(stText, @operator);
			case Operator.Max:
				return new MaxToken(stText, @operator);
			case Operator.Trunc:
				return new TruncToken(stText, @operator);
			case Operator.Mux:
				return new MuxToken(stText, @operator);
			case Operator.Sel:
				return new SelToken(stText, @operator);
			case Operator.Rol:
				return new RolToken(stText, @operator);
			case Operator.Ror:
				return new RorToken(stText, @operator);
			case Operator.Shl:
				return new ShlToken(stText, @operator);
			case Operator.Shr:
				return new ShrToken(stText, @operator);
			case Operator.Exp:
				return new ExpToken(stText, @operator);
			case Operator.Expt:
				return new ExptToken(stText, @operator);
			case Operator.Sqrt:
				return new SqrtToken(stText, @operator);
			case Operator.Ln:
				return new LnToken(stText, @operator);
			case Operator.Log:
				return new LogToken(stText, @operator);
			case Operator.Sin:
				return new SinToken(stText, @operator);
			case Operator.Cos:
				return new CosToken(stText, @operator);
			case Operator.Tan:
				return new TanToken(stText, @operator);
			case Operator.ASin:
				return new ASinToken(stText, @operator);
			case Operator.ACos:
				return new ACosToken(stText, @operator);
			case Operator.ATan:
				return new ATanToken(stText, @operator);
			case Operator.Action:
				return new ActionToken(stText, @operator);
			case Operator.Array:
				return new ArrayToken(stText, @operator);
			case Operator.Params:
				return new ParamsToken(stText, @operator);
			case Operator.At:
				return new AtToken(stText, @operator);
			case Operator.By:
				return new ByToken(stText, @operator);
			case Operator.Case:
				return new CaseToken(stText, @operator);
			case Operator.Constant:
				return new ConstantToken(stText, @operator);
			case Operator.Do:
				return new DoToken(stText, @operator);
			case Operator.Else:
				return new ElseToken(stText, @operator);
			case Operator.Elsif:
				return new ElsIfToken(stText, @operator);
			case Operator.EndAction:
				return new EndActionToken(stText, @operator);
			case Operator.EndCase:
				return new EndCaseToken(stText, @operator);
			case Operator.EndFor:
				return new EndForToken(stText, @operator);
			case Operator.EndFunction:
				return new EndFunctionToken(stText, @operator);
			case Operator.EndFunctionBlock:
				return new EndFunctionBlockToken(stText, @operator);
			case Operator.EndIf:
				return new EndIfToken(stText, @operator);
			case Operator.EndProgram:
				return new EndProgramToken(stText, @operator);
			case Operator.EndRepeat:
				return new EndRepeatToken(stText, @operator);
			case Operator.EndStruct:
				return new EndStructToken(stText, @operator);
			case Operator.EndUnion:
				return new EndUnionToken(stText, @operator);
			case Operator.EndType:
				return new EndTypeToken(stText, @operator);
			case Operator.EndVar:
				return new EndVarToken(stText, @operator);
			case Operator.EndWhile:
				return new EndWhileToken(stText, @operator);
			case Operator.Exit:
				return new ExitToken(stText, @operator);
			case Operator.Continue:
				return new ContinueToken(stText, @operator);
			case Operator.For:
				return new ForToken(stText, @operator);
			case Operator.From:
				return new FromToken(stText, @operator);
			case Operator.Function:
				return new FunctionToken(stText, @operator);
			case Operator.FunctionBlock:
				return new FunctionBlockToken(stText, @operator);
			case Operator.If:
				return new IfToken(stText, @operator);
			case Operator.Of:
				return new OfToken(stText, @operator);
			case Operator.Persistent:
				return new PersistentToken(stText, @operator);
			case Operator.Pointer:
				return new PointerToken(stText, @operator);
			case Operator.Program:
				return new ProgramToken(stText, @operator);
			case Operator.ReadOnly:
				return new ReadOnlyToken(stText, @operator);
			case Operator.ReadWrite:
				return new ReadWriteToken(stText, @operator);
			case Operator.Repeat:
				return new RepeatToken(stText, @operator);
			case Operator.Retain:
				return new RetainToken(stText, @operator);
			case Operator.Return:
				return new ReturnToken(stText, @operator);
			case Operator.Struct:
				return new StructToken(stText, @operator);
			case Operator.Union:
				return new UnionToken(stText, @operator);
			case Operator.Then:
				return new ThenToken(stText, @operator);
			case Operator.To:
				return new ToToken(stText, @operator);
			case Operator.Type:
				return new TypeToken(stText, @operator);
			case Operator.Until:
				return new UntilToken(stText, @operator);
			case Operator.Var:
				return new VarToken(stText, @operator);
			case Operator.VarAccess:
				return new VarAccessToken(stText, @operator);
			case Operator.VarConfig:
				return new VarConfigToken(stText, @operator);
			case Operator.VarExternal:
				return new VarExternalToken(stText, @operator);
			case Operator.VarGlobal:
				return new VarGlobalToken(stText, @operator);
			case Operator.VarInput:
				return new VarInputToken(stText, @operator);
			case Operator.VarInOut:
				return new VarInOutToken(stText, @operator);
			case Operator.VarOutput:
				return new VarOutputToken(stText, @operator);
			case Operator.VarTemp:
				return new VarTempToken(stText, @operator);
			case Operator.VarStat:
				return new VarStatToken(stText, @operator);
			case Operator.While:
				return new WhileToken(stText, @operator);
			case Operator.Extends:
				return new ExtendsToken(stText, @operator);
			case Operator.Implements:
				return new ImplementsToken(stText, @operator);
			case Operator.Method:
				return new MethodToken(stText, @operator);
			case Operator.Interface:
				return new InterfaceToken(stText, @operator);
			case Operator.This:
				return new ThisToken(stText, @operator);
			case Operator.Super:
				return new SuperToken(stText, @operator);
			case Operator.Add:
				return new AddToken(stText, @operator);
			case Operator.Sub:
				return new SubToken(stText, @operator);
			case Operator.Mul:
				return new MulToken(stText, @operator);
			case Operator.Div:
				return new DivToken(stText, @operator);
			case Operator.Mod:
				return new ModToken(stText, @operator);
			case Operator.And:
				return new AndToken(stText, @operator);
			case Operator.AndN:
				return new AndNToken(stText, @operator);
			case Operator.Or:
				return new OrToken(stText, @operator);
			case Operator.OrN:
				return new OrNToken(stText, @operator);
			case Operator.Xor:
				return new XorToken(stText, @operator);
			case Operator.XorN:
				return new XorNToken(stText, @operator);
			case Operator.Not:
				return new NotToken(stText, @operator);
			case Operator.Eq:
				return new EqToken(stText, @operator);
			case Operator.Ne:
				return new NeToken(stText, @operator);
			case Operator.Ge:
				return new GeToken(stText, @operator);
			case Operator.Gt:
				return new GtToken(stText, @operator);
			case Operator.Le:
				return new LeToken(stText, @operator);
			case Operator.Lt:
				return new LtToken(stText, @operator);
			case Operator.Jmp:
				return new JumpToken(stText, @operator);
			case Operator.Ret:
				return new ReturnToken(stText, @operator);
			case Operator.Move:
				return new MoveToken(stText, @operator);
			case Operator.TestAndSet:
				return new TestAndSetToken(stText, @operator);
			case Operator.Plus:
				return new PlusToken(stText, @operator);
			case Operator.Minus:
				return new MinusToken(stText, @operator);
			case Operator.Times:
				return new TimesToken(stText, @operator);
			case Operator.Power:
				return new PowerToken(stText, @operator);
			case Operator.Divide:
				return new DivideToken(stText, @operator);
			case Operator.Period:
				return new PeriodToken(stText, @operator);
			case Operator.Colon:
				return new ColonToken(stText, @operator);
			case Operator.Assign:
				return new AssignToken(stText, @operator);
			case Operator.SetAssign:
				return new SetAssignToken(stText, @operator);
			case Operator.ResetAssign:
				return new ResetAssignToken(stText, @operator);
			case Operator.LeftParenthesis:
				return new LeftParenthesisToken(stText, @operator);
			case Operator.RightParenthesis:
				return new RightParenthesisToken(stText, @operator);
			case Operator.LeftBracket:
				return new LeftBracketToken(stText, @operator);
			case Operator.RightBracket:
				return new RightBracketToken(stText, @operator);
			case Operator.Comma:
				return new CommaToken(stText, @operator);
			case Operator.Semicolon:
				return new SemicolonToken(stText, @operator);
			case Operator.Range:
				return new RangeToken(stText, @operator);
			case Operator.AssignOut:
				return new AssignOutToken(stText, @operator);
			case Operator.Less:
				return new LessToken(stText, @operator);
			case Operator.Greater:
				return new GreaterToken(stText, @operator);
			case Operator.LessEqual:
				return new LessEqualToken(stText, @operator);
			case Operator.GreaterEqual:
				return new GreaterEqualToken(stText, @operator);
			case Operator.Equal:
				return new EqualToken(stText, @operator);
			case Operator.NotEqual:
				return new NotEqualToken(stText, @operator);
			case Operator.Ampersand:
				return new AmpersandToken(stText, @operator);
			case Operator.VerticalLine:
				return new VerticalLineToken(stText, @operator);
			case Operator.DeRef:
				return new DeRefToken(stText, @operator);
			case Operator.Conversion:
				return new ConvOperatorToken(stText, @operator);
			case Operator.RefAssign:
				return new RefAssignToken(stText, @operator);
			case Operator.Reference:
				return new ReferenceToken(stText, @operator);
			case Operator.Property:
				return new PropertyToken(stText, @operator);
			case Operator.TruncInt:
				return new TruncIntToken(stText, @operator);
			case Operator.__VarInfo:
				return new __VarInfoToken(stText, @operator);
			case Operator.__SystemScope:
				return new __SystemScopeToken(stText, @operator);
			case Operator.__TypeOf:
				return new __TypeOfToken(stText, @operator);
			case Operator.__MaxOffset:
				return new __MaxOffsetToken(stText, @operator);
			case Operator.__IsValidRef:
				return new __IsValidRefToken(stText, @operator);
			case Operator.__QueryInterface:
				return new __QueryInterfaceToken(stText, @operator);
			case Operator.__QueryPointer:
				return new __QueryPointerToken(stText, @operator);
			case Operator.__New:
				return new __NewToken(stText, @operator);
			case Operator.__Delete:
				return new __DeleteToken(stText, @operator);
			case Operator.__Cast:
				return new __CastToken(stText, @operator);
			case Operator.__AdrInst:
				return new __AdrInstToken(stText, @operator);
			case Operator.__RefAdr:
				return new __RefAdrToken(stText, @operator);
			case Operator.SafeBool:
				return new SafeBoolToken(stText, @operator);
			case Operator.SafeByte:
				return new SafeByteToken(stText, @operator);
			case Operator.SafeUSInt:
				return new SafeUSIntToken(stText, @operator);
			case Operator.SafeSInt:
				return new SafeSIntToken(stText, @operator);
			case Operator.SafeWord:
				return new SafeWordToken(stText, @operator);
			case Operator.SafeUInt:
				return new SafeUIntToken(stText, @operator);
			case Operator.SafeInt:
				return new SafeIntToken(stText, @operator);
			case Operator.SafeDWord:
				return new SafeDWordToken(stText, @operator);
			case Operator.SafeUDInt:
				return new SafeUDIntToken(stText, @operator);
			case Operator.SafeTime:
				return new SafeTimeToken(stText, @operator);
			case Operator.SafeDInt:
				return new SafeDIntToken(stText, @operator);
			case Operator.SafeLWord:
				return new SafeLWordToken(stText, @operator);
			case Operator.SafeULInt:
				return new SafeULIntToken(stText, @operator);
			case Operator.SafeLInt:
				return new SafeLIntToken(stText, @operator);
			case Operator.__BitOffset:
				return new __BitOffsetToken(stText, @operator);
			case Operator.__FCall:
				return new __FCallToken(stText, @operator);
			case Operator.__PropertyInfo:
				return new __PropertyInfoToken(stText, @operator);
			case Operator.Class:
				return new ClassToken(stText, @operator);
			case Operator.Abstract:
				return new AbstractToken(stText, @operator);
			case Operator.Override:
				return new OverrideToken(stText, @operator);
			case Operator.Overload:
				return new OverloadToken(stText, @operator);
			case Operator.Public:
				return new PublicToken(stText, @operator);
			case Operator.Private:
				return new PrivateToken(stText, @operator);
			case Operator.Protected:
				return new ProtectedToken(stText, @operator);
			case Operator.Internal:
				return new InternalToken(stText, @operator);
			case Operator.Final:
				return new FinalToken(stText, @operator);
			case Operator.__XWord:
				return new __XWordToken(stText, @operator);
			case Operator.__UXInt:
				return new __UXIntToken(stText, @operator);
			case Operator.__MemorySet:
				return new __MemorySetToken(stText, @operator);
			case Operator.And_Then:
				return new And_ThenToken(stText, @operator);
			case Operator.Or_Else:
				return new Or_ElseToken(stText, @operator);
			case Operator.__GetLTick:
				return new __GetLTickToken(stText, @operator);
			case Operator.__XInt:
				return new __XIntToken(stText, @operator);
			case Operator.__Try:
				return new TryToken(stText, @operator);
			case Operator.__EndTry:
				return new EndTryToken(stText, @operator);
			case Operator.__Catch:
				return new CatchToken(stText, @operator);
			case Operator.__Finally:
				return new FinallyToken(stText, @operator);
			case Operator.__Throw:
				return new __ThrowToken(stText, @operator);
			case Operator.__XString:
				return new __XStringToken(stText, @operator);
			case Operator.VarInst:
				return new VarInstToken(stText, @operator);
			case Operator.__CheckLicense:
				return new __CheckLicenseToken(stText, @operator);
			case Operator.__CallInitFunction:
				return new __CallInitFunctionToken(stText, @operator);
			case Operator.LowerBound:
				return new LowerBoundToken(stText, @operator);
			case Operator.UpperBound:
				return new UpperBoundToken(stText, @operator);
			case Operator.AnyString:
				return new AnyStringToken(stText, @operator);
			case Operator.__PoolScope:
				return new __PoolScopeToken(stText, @operator);
			case Operator.__CheckLicenseBit:
				return new __CheckLicenseBitToken(stText, @operator);
			case Operator.__XAdd:
				return new __XAddToken(stText, @operator);
			case Operator.__MemoryBarrier:
				return new __MemoryBarrierToken(stText, @operator);
			case Operator.__CurrentTask:
				return new __CurrentTaskToken(stText, @operator);
			case Operator.__CompareAndSwap:
				return new __CompareAndSwapToken(stText, @operator);
			case Operator.__Vector:
				return new __VectorToken(stText, @operator);
			case Operator.__vcAdd:
				return new __vcAddToken(stText, @operator);
			case Operator.__vcSub:
				return new __vcSubToken(stText, @operator);
			case Operator.__vcMul:
				return new __vcMulToken(stText, @operator);
			case Operator.__vcDiv:
				return new __vcDivToken(stText, @operator);
			case Operator.__vcDot:
				return new __vcDotToken(stText, @operator);
			case Operator.__vcSqrt:
				return new __vcSqrtToken(stText, @operator);
			case Operator.__vcMin:
				return new __vcMinToken(stText, @operator);
			case Operator.__vcMax:
				return new __vcMaxToken(stText, @operator);
			case Operator.__vcSetReal:
				return new __vcSetRealToken(stText, @operator);
			case Operator.__vcSetLReal:
				return new __vcSetLRealToken(stText, @operator);
			case Operator.__vcLoadReal:
				return new __vcLoadRealToken(stText, @operator);
			case Operator.__vcLoadLReal:
				return new __vcLoadLRealToken(stText, @operator);
			case Operator.__vcStore:
				return new __vcStoreToken(stText, @operator);
			case Operator.Hash:
				return new HashToken(stText, @operator);
			case Operator.SafeReal:
				return new SafeRealToken(stText, @operator);
			case Operator.SafeLReal:
				return new SafeLRealToken(stText, @operator);
			case Operator.LDate:
				return new IlDateSimpleTypeToken(stText, @operator);
			case Operator.LDateAndTime:
				return new IlDateAndTimeSimpleTypeToken(stText, @operator);
			case Operator.LTimeOfDay:
				return new IlTimeOfDaySimpleTypeToken(stText, @operator);
			case Operator.XSizeOf:
				return new XSizeOfToken(stText, @operator);
			case Operator.__PouName:
				return new __PouNameToken(stText, @operator);
			case Operator.__Position:
				return new __PositionToken(stText, @operator);
			case Operator.VarGeneric:
				return new VarGenericToken(stText, @operator);
			case Operator.JmpC:
				return new JmpCToken(stText, @operator);
			case Operator.JmpCN:
				return new JmpCNToken(stText, @operator);
			case Operator.RetC:
				return new RetCToken(stText, @operator);
			case Operator.RetCN:
				return new RetCNToken(stText, @operator);
			case Operator.__LateCompiledExpr:
				return new __LateCompiledExprToken(stText, @operator);
			case Operator.Cal:
				return new CalToken(stText, @operator);
			case Operator.CalC:
				return new CalCToken(stText, @operator);
			case Operator.CalCN:
				return new CalCNToken(stText, @operator);
			case Operator.Ld:
				return new LdToken(stText, @operator);
			case Operator.LdN:
				return new LdNToken(stText, @operator);
			case Operator.St:
				return new StToken(stText, @operator);
			case Operator.StN:
				return new StNToken(stText, @operator);
			case Operator.R:
				return new RToken(stText, @operator);
			case Operator.S:
				return new SToken(stText, @operator);
			case Operator.FupAssign:
				return new FupAssignToken(stText, @operator);
			case Operator.__LocalOffset:
				return new __LocalOffsetToken(stText, @operator);
			case Operator.__CRC:
				return new __CRCToken(stText, @operator);
			case Operator.__Init:
				return new __InitToken(stText, @operator);
			case Operator.__Wait:
				return new __WaitToken(stText, @operator);
			case Operator.EndMethod:
				return new EndMethodToken(stText, @operator);
			case Operator.EndProperty:
				return new EndPropertyToken(stText, @operator);
			case Operator.EndInterface:
				return new EndInterfaceToken(stText, @operator);
			case Operator.PropertySet:
				return new PropertySetToken(stText, @operator);
			case Operator.PropertyGet:
				return new PropertyGetToken(stText, @operator);
			case Operator.Namespace:
				return new NamespaceToken(stText, @operator);
			case Operator.EndNamespace:
				return new EndNamespaceToken(stText, @operator);
			case Operator.Transition:
				return new TransitionToken(stText, @operator);
			case Operator.EndTransition:
				return new EndTransitionToken(stText, @operator);
			default:
				throw new ArgumentException($"unexpected token {@operator} found");
			}
		}
	}
}
