import csv, re, os, io, sys

ROOT = r"C:\Codesys"
LMM = os.path.join(ROOT, "decompiled", "LanguageModelManager.plugin", "_3S", "CoDeSys", "LanguageModelManager")

def read_csv(path):
    with io.open(path, "r", encoding="utf-8-sig", newline="") as f:
        return list(csv.reader(f))

def write_csv(path, header, rows):
    with io.open(path, "w", encoding="utf-8", newline="") as f:
        w = csv.writer(f)
        w.writerow(header)
        w.writerows(rows)

# 1) inventory concrete red/green node classes
class_re = re.compile(r'class\s+(\w+)\s*:\s*([^\r\n{]+)')
red = {}   # class -> (kind, interfaces, relfile)
green = set()
for dp, dn, fn in os.walk(LMM):
    rel = os.path.relpath(dp, LMM)
    for name in fn:
        if not name.endswith(".cs"):
            continue
        p = os.path.join(dp, name)
        try:
            txt = io.open(p, "r", encoding="utf-8-sig", errors="replace").read()
        except Exception:
            continue
        for m in class_re.finditer(txt):
            cls, ifaces = m.group(1), m.group(2)
            if "\\GreenTrees" in dp or os.sep + "GreenTrees" in dp:
                green.add(cls)
                continue
            if "\\RedTrees" in dp or os.sep + "RedTrees" in dp:
                continue
            if re.search(r'\b_IStatement\b', ifaces):
                kind = "statement"
            elif re.search(r'\b_IExpression\b', ifaces):
                kind = "expression"
            elif re.search(r'\b_IType\b', ifaces) or re.search(r'\bIECType\b', ifaces):
                kind = "type"
            else:
                continue
            ifs = re.findall(r'(_?I[A-Z]\w+)', ifaces)
            red[cls] = (kind, ifs, os.path.join(rel, name) if rel != "." else name)

# 2) read maps
bmap = read_csv(os.path.join(ROOT, "tables", "ast_builder_map.csv"))
bhead, brows = bmap[0], bmap[1:]

def iface_tokens(hint):
    out = []
    for t in re.split(r'[ /+\u2192,]+', hint):
        t = t.strip()
        if re.match(r'^_?I[A-Z]\w*(Statement|Expression|Type|Reference)\b', t):
            out.append(t.lstrip("_"))
    return out

prod = {}   # iface-lastseg(stripped _) -> dict(producer,builder_interface,red_factory_method)
for row in brows:
    if len(row) < 5:
        continue
    construct, parser_method, builder_iface, call_method, hint = row[0], row[1], row[2], row[3], row[4]
    for it in iface_tokens(hint):
        entry = prod.setdefault(it, {"producer": "", "builder_interface": "", "red_factory_method": ""})
        if not entry["producer"]:
            entry["producer"] = parser_method
        if not entry["builder_interface"]:
            entry["builder_interface"] = builder_iface
        m = re.search(r'(Create\w+)', call_method or "")
        if m and not entry["red_factory_method"]:
            entry["red_factory_method"] = m.group(1)

# 3) update ast_nodes.csv
nodes = read_csv(os.path.join(ROOT, "tables", "ast_nodes.csv"))
head, nrows = nodes[0], nodes[1:]
idx = {h: i for i, h in enumerate(head)}
for extra in ("concrete_impl", "red_factory_method", "producer"):
    if extra not in idx:
        head.append(extra)
        idx[extra] = len(head) - 1
for r in nrows:
    while len(r) < len(head):
        r.append("")
    full = r[idx["full_name"]]
    last = full.split(".")[-1].lstrip("_")
    cls = last[1:] if (len(last) > 1 and last[0] == "I" and last[1].isupper()) else last
    cls = {"ArrayInitialization": "ArrayInitialisation",
           "MultipleIndexInitialization": "MultipleIndexInitialisation",
           "StructureInitialization": "StructureInitialisation",
           "StringType": "StringType"}.get(cls, cls)
    if cls in red:
        r[idx["concrete_impl"]] = "".join(["_3S.CoDeSys.LanguageModelManager.", cls])
    e = prod.get(last)
    if e:
        if "builder_interface" in idx and not r[idx["builder_interface"]]:
            r[idx["builder_interface"]] = e["builder_interface"]
        if not r[idx["red_factory_method"]]:
            r[idx["red_factory_method"]] = e["red_factory_method"]
        if not r[idx["producer"]]:
            r[idx["producer"]] = e["producer"]

write_csv(os.path.join(ROOT, "tables", "ast_nodes.csv"), head, nrows)

# 4) concrete node table
rf = read_csv(os.path.join(ROOT, "tables", "red_tree_factory_methods.csv"))
rh, rr = rf[0], rf[1:]
fmap = {}
for row in rr:
    if len(row) >= 3:
        fmap[row[2]] = row[1]  # node_kind -> factory_method
out = []
for cls, (kind, ifs, rel) in sorted(red.items(), key=lambda kv: (kv[1][0], kv[0])):
    g = cls + "_Green" if (cls + "_Green") in green else ""
    fact = ""
    # infer factory method by kind/name
    for cand in (ifs or []):
        pass
    out.append([kind, "_3S.CoDeSys.Core.LanguageModel." + (ifs[0] if ifs else ""), cls, g, "", rel])
write_csv(os.path.join(ROOT, "tables", "red_tree_nodes.csv"),
          ["kind", "primary_interface", "concrete_class", "green_class", "red_factory_method", "file"], out)

# 5) summary
filled_impl = sum(1 for r in nrows if r[idx["concrete_impl"]])
filled_prod = sum(1 for r in nrows if r[idx["producer"]])
filled_fact = sum(1 for r in nrows if r[idx["red_factory_method"]])
print("concrete red classes:", len(red), " green:", len(green))
print("ast_nodes rows:", len(nrows))
print("concrete_impl filled:", filled_impl, " red_factory_method filled:", filled_fact, " producer filled:", filled_prod)
print("red_tree_nodes.csv rows:", len(out))
