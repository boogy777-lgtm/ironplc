"""Join AST interface rows with concrete red/green node classes and producers.

Purpose (RE-1): close the remaining gaps of the AST map.

  1. For every interface row in `tables\\ast_nodes.csv` find the concrete
     red class (`concrete_impl`) and its green twin (`green_impl`) inside
     `decompiled\\LanguageModelManager.plugin\\...\\LanguageModelManager\\`.
     Matching is by *implemented interface*, not by class name, so that
     `_IElseIf -> ElseIf` and `_IArrayInitialization -> ArrayInitialisation`
     are resolved correctly.
  2. Recover `red_factory_method` from the real factory code:
       - `RedTreeBuilder.visit(_IXxx)` -> `ITreeFactory.CreateYyy` (nodes),
       - `LanguageModelBuilder.CreateYyy` (types/services).
  3. Fill `producer` with the parser method (`file:line`) from
     `tables\\ast_builder_map.csv`; nodes that the parser never builds get
     `нет (декларация/типизация)`.
  4. Add the AST node interfaces/classes that are still missing from
     `ast_nodes.csv` (they are present in `red_tree_nodes.csv`).
  5. Regenerate `tables\\red_tree_nodes.csv`.

Only files inside the allowed zone are written:
  tables\\ast_nodes.csv, tables\\red_tree_nodes.csv.

Usage:  python tools\\build_concrete_map.py
"""
import csv, re, os, io, collections

ROOT = r"C:\Codesys"
LMM = os.path.join(ROOT, "decompiled", "LanguageModelManager.plugin", "_3S", "CoDeSys", "LanguageModelManager")
REDTREE = os.path.join(ROOT, "decompiled", "Compiler35220.plugin", "_3S", "CoDeSys", "Compiler35220",
                       "TreeConversion", "RedTreeBuilder.cs")

CLASS_RE = re.compile(r'\b(?P<mods>(?:abstract\s+|sealed\s+|static\s+|public\s+|internal\s+)*)class\s+(\w+)\s*(?:<[^>]*>)?\s*:\s*([^\r\n{]+)')
IFACE_TOK = re.compile(r'(?<![\w.])(_?I[A-Z]\w*)')

# Abstract node interfaces: they do not identify a unique concrete class.
ROOT_NODES = {
    "IExprement", "IExpression", "IStatement", "IType",
    "ICompiledType", "ICompiledType2", "ICompiledType3", "ICompiledType4", "ICompiledType5",
}
# Highlighted interfaces used for dispatching in ast_builder_map hints that must
# not become the producer/factory of a concrete node.
HINT_BLACKLIST = {"IExpression", "IType", "IStatement", "IExprement"}

SPECIAL = {
    "IType": "IECType", "IType2": "IECType", "ISafetyType": "IECType",
    "ICompiledType": "IECType", "ICompiledType2": "IECType", "ICompiledType3": "IECType",
    "ICompiledType4": "IECType", "ICompiledType5": "IECType",
    "ISubRoutineStatement": "SubRoutineStatement", "ISubroutineStatement": "SubRoutineStatement",
    "ICompiledPOU": "CompiledPOU",
}
ALIASES = {
    "ArrayInitialization": "ArrayInitialisation",
    "MultipleIndexInitialization": "MultipleIndexInitialisation",
    "StructureInitialization": "StructureInitialisation",
    "Initialization": "Initialisation",
}
NODE_BASES = {"Statement", "PositionStatement", "SequenceStatement", "Expression",
              "PositionExpression", "Exprement", "IECType"}

# Curated parser producers recovered from the Parser35220.plugin analysis
# (interface -> "file:line method").  Kept here so regenerating the base
# `ast_nodes.csv` from the binaries does not lose them.
PRODUCER_OVERRIDE = {
    "_IEmbeddedLanguageStatement": r"\Statements\ImplementationBlockParser.cs:55",
    "IHasConstantTypeExpression": r"\Pragmas\HasConstantValueOrTypePragmaParser.cs:167",
    "IPartialAccessExpression": r"\Expressions\OperandParser.cs:189",
    "IAddressExpression": r"\Expressions\OperandParser.cs:454",
    "IArrayInitialization": r"\Expressions\ArrayInitializationParser.cs:111",
    "IAssignmentExpression": r"\Expressions\ExpressionParser.cs:124",
    "IBaseExpression": r"\Expressions\ThisAndBaseExpressionParser.cs:62",
    "IBreakPointStatement": r"\Pragmas\PragmaStatementParser.cs:239",
    "ICallExpression": r"\Expressions\FunctionCallParser.cs:84",
    "ICase": r"\Statements\CaseStatementParser.cs:195",
    "ICaseLabelStatement": r"\Statements\CaseStatementParser.cs:256",
    "ICaseRangeExpression": r"\Statements\CaseStatementParser.cs:400",
    "ICaseStatement": r"\Statements\CaseStatementParser.cs:155",
    "ICastExpression": r"\Expressions\ImplicitCastOperatorParser.cs:97",
    "ICommentStatement": r"\Statements\StatementParser.cs:254",
    "ICompilerVersionExpression": r"\Pragmas\PragmaOperandParser.cs:156",
    "ICompoAccessExpression": r"\Expressions\OperandParser.cs:160",
    "IContinueStatement": r"\Statements\StatementParser.cs:549",
    "IConversionExpression": r"\Expressions\ConversionExpressionParser.cs:113",
    "ICurrentTaskExpression": r"\Expressions\CurrentTaskExpressionParser.cs:62",
    "IDefinedExpression": r"\Pragmas\DefinedPragmaOperandParser.cs:91",
    "IDefineReference": r"\Pragmas\DefinedPragmaOperandParser.cs:137",
    "IDefineStatement": r"\Pragmas\PragmaStatementParser.cs:251",
    "IDeRefAccessExpression": r"\Expressions\OperandParser.cs:268",
    "IElseIf": r"\Statements\IfStatementParser.cs:196",
    "IEmptyStatement": r"\Pragmas\ErrorPragmaParser.cs:82",
    "IEnumDeclarationListStatement": r"\Declaration\EnumListParser.cs:111",
    "IErrorExpression": r"\Expressions\MinMaxOperatorParser.cs:186",
    "IErrorStatement": r"\InternalParser.cs:205",
    "IExitStatement": r"\Statements\StatementParser.cs:547",
    "IExpressionStatement": r"\Statements\ConditionalCallParser.cs:134",
    "IForStatement": r"\Statements\ForStatementParser.cs:118",
    "IGlobalScopeExpression": r"\Expressions\ScopeExpressionParser.cs:151",
    "IHasAttributeExpression": r"\Pragmas\HasAttributePragmaParser.cs:84",
    "IHasConstantValueExpression": r"\Pragmas\HasConstantValueOrTypePragmaParser.cs:106",
    "IHasTypeExpression": r"\Pragmas\HasTypePragmaParser.cs:105",
    "IHasValueExpression": r"\Pragmas\HasValuePragmaParser.cs:78",
    "IIfStatement": r"\Statements\IfStatementParser.cs:110",
    "IIndexAccessExpression": r"\Expressions\OperandParser.cs:91",
    "IIsEnumTypeExpression": r"\Pragmas\HasTypePragmaParser.cs:183",
    "IJumpStatement": r"\Statements\JumpStatementParser.cs:98",
    "ILabelStatement": r"\Statements\StatementParser.cs:929",
    "ILiteralExpression": r"\Expressions\NewExpressionParser.cs:134",
    "ILMDataType": "LanguageModelOfRawST.cs:34 CreateLanguageModelOfPOUSyntax",
    "IMultipleIndexInitialization": r"\Expressions\ArrayInitializationParser.cs:164",
    "INamespaceAccessExpression": r"\Expressions\OperandParser.cs:134",
    "INewExpression": r"\Expressions\NewExpressionParser.cs:103",
    "INullExpression": r"\Expressions\OperandParser.cs:145",
    "IOperatorExpression": r"\Expressions\MinMaxOperatorParser.cs:161",
    "IPoolScopeExpression": r"\Declaration\TypeParser.cs:353",
    "IPOUDeclarationStatement": r"\Declaration\POUDeclarationParser.cs:25",
    "IPouReference": r"\Pragmas\ItemReferenceParser.cs:170",
    "IPragmaAssertion": r"\Pragmas\PragmaStatementParser.cs:312",
    "IPragmaIfStatement": r"\Pragmas\PragmaIfStatementParser.cs:17",
    "IPragmaOperatorExpression": r"\Pragmas\PragmaOperandParser.cs:163",
    "IPragmaStatement": r"\Pragmas\PragmaStatementParser.cs:101",
    "IRepeatStatement": r"\Statements\RepeatStatementParser.cs:150",
    "IReturnStatement": r"\Statements\ReturnStatementParser.cs:74",
    "IRuntimeVersionExpression": r"\Pragmas\PragmaOperandParser.cs:174",
    "ISequenceStatement": r"\InternalParser.cs:179",
    "IStructureInitialization": r"\Expressions\StructureInitializationParser.cs:106",
    "ISystemScopeExpression": r"\Declaration\TypeParser.cs:348",
    "IThisExpression": r"\Expressions\ThisAndBaseExpressionParser.cs:70",
    "ITypeDeclarationStatement": r"\Declaration\TypeDeclarationParser.cs:17",
    "ITypeExpression": r"\Declaration\POUDeclarationParser.cs:259",
    "ITypeReference": r"\Pragmas\ItemReferenceParser.cs:187",
    "IVariableDeclarationListStatement": r"\Declaration\VariableListParser.cs:91",
    "IVariableDeclarationStatement": r"\Declaration\VariableDeclarationParser.cs:18",
    "IVariableExpression": r"\Declaration\POUDeclarationParser.cs:190",
    "IVariableReference": r"\Pragmas\ItemReferenceParser.cs:204",
    "IWarningDisableRestorePragmaStatement": r"\Pragmas\ErrorPragmaParser.cs:156",
    "IWhileStatement": r"\Statements\StatementParser.cs:859",
    "IArrayType": r"\Declaration\TypeParser.cs:614",
    "IDirectVariable": r"\Declaration\VariableDeclarationParser.cs:240",
    "IPointerType": r"\Declaration\TypeParser.cs:681",
    "IReferenceType": r"\Declaration\TypeParser.cs:656",
    "IStringType": r"\Declaration\TypeParser.cs:544",
    "ISubrangeType": r"\Declaration\TypeParser.cs:140",
    "IUserdefType": r"\Declaration\TypeParser.cs:357",
    "IVectorType": r"\Declaration\TypeParser.cs:476",
    "IWStringType": r"\Declaration\TypeParser.cs:514",
}

# Red factory methods that are chosen by runtime type in RedTreeBuilder
# (literal family) and therefore are not visible in a single visit() body.
SPECIAL_FACTORY = {
    "IFloatLiteralExpression": "CreateFloatLiteralExpression",
    "IIntegerLiteralExpression": "CreateIntegerLiteralExpression",
    "IStringLiteralExpression": "CreateStringLiteralExpression",
    "ILiteralExpression": "CreateLiteralExpression",
    "IQualifiedNameExpression": "",
    "IDirectAddressBitType": "",
}


def read_csv(path):
    with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
        return list(csv.reader(f))


def write_csv(path, header, rows):
    with io.open(path, "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f)
        w.writerow(header)
        w.writerows(rows)


def norm(iface):
    return iface[1:] if iface.startswith("_") else iface


def derived_class(iface):
    name = norm(iface)
    if not name.startswith("I") or len(name) < 2 or not name[1].isupper():
        return None
    base = re.sub(r'\d+$', '', name[1:])
    return ALIASES.get(base, base)


def scan_classes():
    red, green = {}, {}
    for dp, dn, fn in os.walk(LMM):
        low = dp.replace("/", "\\").lower()
        is_green = "\\greentrees" in low
        is_builder = "\\redtrees" in low
        for name in fn:
            if not name.endswith(".cs"):
                continue
            p = os.path.join(dp, name)
            try:
                txt = io.open(p, "r", encoding="utf-8-sig", errors="replace").read()
            except Exception:
                continue
            rel = os.path.relpath(p, LMM)
            for m in CLASS_RE.finditer(txt):
                abstract = "abstract" in (m.group("mods") or "")
                cls, bases = m.group(2), m.group(3)
                ifaces, base_cls = set(), None
                for tok in re.split(r'[,\s]+', bases):
                    if not tok:
                        continue
                    if IFACE_TOK.fullmatch(tok):
                        ifaces.add(norm(tok))
                    elif re.fullmatch(r'\w+', tok) and base_cls is None:
                        base_cls = tok
                rec = {"file": rel, "ifaces": ifaces, "base": base_cls, "abstract": abstract}
                (green if is_green else red if not is_builder else {})[cls] = rec
    return red, green


ROOTS = {"Statement", "Expression", "Exprement", "IECType"}
EXCLUDE_IFACES = {"IArchivable", "ICloneable", "IComparable"}


def is_node_class(cls, rec):
    if cls in ROOTS or cls == "Case":
        return True
    if rec.get("abstract"):
        return False
    base = rec["base"] or ""
    if (base in NODE_BASES or base.endswith("Statement") or base.endswith("Expression")
            or base.endswith("Type")):
        return True
    return any(i.endswith(("Statement", "Expression", "Type")) and i not in EXCLUDE_IFACES
               for i in rec["ifaces"])


def kind_of(cls, rec, prim):
    base = rec["base"] or ""
    if cls == "Case" or prim.endswith("Statement") or base.endswith("Statement"):
        return "statement"
    if prim.endswith("Type") or base.endswith("Type"):
        return "type"
    return "expression"


def primary_iface(cls, rec):
    """Most specific AST interface of a node class (its canonical name)."""
    if cls == "IECType":
        return "IType"
    ifaces = [i for i in rec["ifaces"] if i not in EXCLUDE_IFACES]
    for i in sorted(ifaces):
        if derived_class(i) == cls:
            return i
    for suffix in ("Statement", "Expression", "Type"):
        for i in sorted(ifaces):
            if i.endswith(suffix) and not re.search(r'\d$', i) and i not in ROOT_NODES:
                return i
    return sorted(ifaces)[0] if ifaces else ""


def scan_redtreebuilder_factories():
    out = {}
    try:
        txt = io.open(REDTREE, "r", encoding="utf-8-sig", errors="replace").read()
    except Exception:
        return out
    for m in re.finditer(r'public\s+(?:virtual\s+)?void\s+visit\s*\(\s*(_I\w+)\s+\w+\s*\)\s*\{(.*?)\n\t\t\}',
                         txt, re.S):
        iface, body = norm(m.group(1)), m.group(2)
        fm = re.search(r'(Create\w+)\s*\(', body)
        if fm and iface not in out:
            out[iface] = fm.group(1)
    return out


def scan_builder_factories():
    """CreateYyy -> {from_iface, file:line} from LanguageModelBuilder.cs."""
    out = {}
    p = os.path.join(LMM, "LanguageModelBuilder.cs")
    try:
        txt = io.open(p, "r", encoding="utf-8-sig", errors="replace").read()
    except Exception:
        return out
    lines = txt.split("\n")
    for n, line in enumerate(lines, 1):
        m = re.search(r'public\s+(_?I\w+)\s+(Create\w+)\s*\(', line)
        if m:
            out.setdefault(m.group(2), {"from": norm(m.group(1)), "where": "LanguageModelBuilder.cs:%d" % n})
    return out


def build_producers():
    prod = {}
    for row in read_csv(os.path.join(ROOT, "tables", "ast_builder_map.csv"))[1:]:
        if len(row) < 5:
            continue
        parser_method, builder_iface, call_method, hint = row[1], row[2], row[3], row[4]
        for tok in IFACE_TOK.findall(hint):
            if norm(tok) in HINT_BLACKLIST:
                continue
            e = prod.setdefault(norm(tok), {"producer": "", "builder": "", "factory": ""})
            if not e["producer"]:
                e["producer"] = parser_method
            if not e["builder"]:
                e["builder"] = builder_iface
            fm = re.search(r'(Create\w+)', call_method or "")
            if fm and not e["factory"]:
                e["factory"] = fm.group(1)
    return prod


def pick_class(iface, cands):
    if not cands:
        return None
    names = set(cands)
    if iface in SPECIAL and SPECIAL[iface] in names:
        return SPECIAL[iface]
    d = derived_class(iface)
    if d and d in names:
        return d
    if len(cands) == 1:
        return next(iter(names))
    return None


def norm_producer(pm):
    pm = pm.replace("/", "\\")
    if "\\" in pm and not pm.startswith("\\"):
        pm = "\\" + pm
    return pm


def main():
    red, green = scan_classes()
    iface2cls = collections.defaultdict(dict)
    for cls, rec in red.items():
        for i in rec["ifaces"]:
            iface2cls[i][cls] = rec

    rtb_factory = scan_redtreebuilder_factories()
    builder_factory = scan_builder_factories()
    producers = build_producers()

    node_classes = {c: r for c, r in red.items() if "\\" not in r["file"].replace("/", "\\") and is_node_class(c, r)}

    # factory method keyed by normalized interface (from RedTreeBuilder).
    def factory_for(iface, cls):
        if iface in rtb_factory:
            return rtb_factory[iface]
        for cand in ("Create" + cls, "Create" + re.sub(r'\d+$', '', cls)):
            if cand in builder_factory:
                return cand
        low = {k.lower(): k for k in builder_factory}
        return low.get(("Create" + cls).lower(), "")

    # ---- load ast_nodes, add the missing node rows ----------------------
    nodes = read_csv(os.path.join(ROOT, "tables", "ast_nodes.csv"))
    head, nrows = nodes[0], nodes[1:]
    for extra in ("green_impl", "note"):
        if extra not in head:
            head.append(extra)
    idx = {h: i for i, h in enumerate(head)}
    for r in nrows:
        while len(r) < len(head):
            r.append("")

    represented = set(norm(r[idx["full_name"]].split(".")[-1]) for r in nrows)
    added = []
    for cls, rec in sorted(node_classes.items()):
        prim = primary_iface(cls, rec)
        if not prim or prim in represented:
            continue
        base_if = "_" + prim if ("_" + prim) in [("_" + x) for x in rec["ifaces"]] else prim
        full = "_3S.CoDeSys.Core.LanguageModel." + base_if
        kind = kind_of(cls, rec, prim)
        row = [""] * len(head)
        row[idx["kind"]] = kind
        row[idx["full_name"]] = full
        nrows.append(row)
        represented.add(prim)
        added.append((kind, prim, cls))

    # ---- fill every row -------------------------------------------------
    stats = collections.Counter()
    without_concrete = []
    for r in nrows:
        full = r[idx["full_name"]]
        iface = norm(full.split(".")[-1])
        kind = r[idx["kind"]]
        cands = iface2cls.get(iface, {})
        cls = pick_class(iface, cands)

        r[idx["concrete_impl"]] = ("_3S.CoDeSys.LanguageModelManager." + cls) if cls else ""
        g = cls + "_Green" if cls else ""
        r[idx["green_impl"]] = ("_3S.CoDeSys.LanguageModelManager.GreenTrees." + g) if (g and g in green) else ""

        # producer: curated override wins, then ast_builder_map (exact then prefix).
        if not r[idx["red_factory_method"]] and SPECIAL_FACTORY.get(iface):
            r[idx["red_factory_method"]] = SPECIAL_FACTORY[iface]
        e = producers.get(iface)
        if not e:
            for k, v in producers.items():
                if iface.startswith(k) and len(k) >= 7:
                    e = v
                    break
        if not e and cls:
            d = derived_class(iface)
            for k, v in producers.items():
                if d and derived_class(k) == d:
                    e = v
                    break
        if e:
            if not r[idx["builder_interface"]] and e.get("builder"):
                r[idx["builder_interface"]] = e["builder"]
            if not r[idx["red_factory_method"]] and e.get("factory"):
                r[idx["red_factory_method"]] = e["factory"]
        prod = PRODUCER_OVERRIDE.get(iface, "") or (e.get("producer", "") if e else "")
        if prod:
            r[idx["producer"]] = norm_producer(prod)

        # red factory method: explicit parser builder call, then RedTreeBuilder,
        # then LanguageModelBuilder, then literal specials.
        if not r[idx["red_factory_method"]] and iface in SPECIAL_FACTORY:
            r[idx["red_factory_method"]] = SPECIAL_FACTORY[iface]
        if not r[idx["red_factory_method"]] and cls:
            r[idx["red_factory_method"]] = factory_for(iface, cls)

        # reason: only for AST nodes the parser does not build
        if not r[idx["producer"]] and kind in ("statement", "expression", "type"):
            r[idx["producer"]] = "нет (декларация/типизация)"

        if not r[idx["note"]]:
            if not cls:
                if len(cands) > 3:
                    r[idx["note"]] = "маркерный интерфейс: реализуют %d классов" % len(cands)
                elif cands:
                    r[idx["note"]] = "неоднозначно: " + ", ".join(sorted(cands))
                elif iface in ROOT_NODES:
                    r[idx["note"]] = "абстрактный корневой интерфейс"
                elif kind in ("statement", "expression", "type"):
                    r[idx["note"]] = "нет red-класса в LMM"
                elif kind == "type":
                    r[idx["note"]] = "нет класса в LMM (тип-система Core)"
                else:
                    r[idx["note"]] = "сервисный интерфейс (реализация вне LMM / Core)"
            elif not r[idx["red_factory_method"]]:
                r[idx["note"]] = "нет factory (NotImplementedException в RedTreeBuilder)"
            elif not r[idx["producer"]]:
                r[idx["note"]] = "абстрактный/сервисный интерфейс (без producer)"

        stats["rows"] += 1
        stats["concrete"] += bool(r[idx["concrete_impl"]])
        stats["green"] += bool(r[idx["green_impl"]])
        stats["factory"] += bool(r[idx["red_factory_method"]])
        stats["producer"] += bool(r[idx["producer"]])
        if not cls:
            without_concrete.append((full.split(".")[-1], kind, r[idx["note"]]))

    write_csv(os.path.join(ROOT, "tables", "ast_nodes.csv"), head, nrows)

    # ---- regenerate red_tree_nodes.csv ---------------------------------
    rows_out = []
    for cls, rec in sorted(node_classes.items()):
        prim = primary_iface(cls, rec)
        if not prim:
            continue
        kind = kind_of(cls, rec, prim)
        g = cls + "_Green"
        rows_out.append([kind, "_3S.CoDeSys.Core.LanguageModel._" + prim, cls,
                         ("_3S.CoDeSys.LanguageModelManager.GreenTrees." + g) if g in green else "",
                         SPECIAL_FACTORY.get(prim) or factory_for(prim, cls), cls + ".cs",
                         "|".join(sorted(rec["ifaces"]))])
    write_csv(os.path.join(ROOT, "tables", "red_tree_nodes.csv"),
              ["kind", "primary_interface", "concrete_class", "green_class",
               "red_factory_method", "file", "interfaces"], rows_out)

    print("red classes:", len(red), " green classes:", len(green), " node classes:", len(node_classes))
    print("RedTreeBuilder visit->Create:", len(rtb_factory), " LanguageModelBuilder Create*:", len(builder_factory))
    print("new ast_nodes rows added:", len(added))
    for a in added:
        print("   + %-9s %-38s -> %s" % a)
    print("ast_nodes rows:", stats["rows"])
    print("concrete_impl:", stats["concrete"], " green_impl:", stats["green"],
          " red_factory_method:", stats["factory"], " producer:", stats["producer"])
    print("rows without concrete_impl:", len(without_concrete))
    print("red_tree_nodes rows:", len(rows_out))
    print("\n-- rows without concrete_impl (reason) --")
    for last, kind, note in sorted(set(without_concrete)):
        print("  %-46s %-11s %s" % (last, kind, note))


if __name__ == "__main__":
    main()
