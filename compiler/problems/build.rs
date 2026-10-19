use std::{
    env,
    error::Error,
    fs::{self, File},
    io::Write,
    path::PathBuf,
    process,
};

struct ProblemDef {
    /// The code that users know this as. This should remain stable
    /// between releases to facilitate consistent documentation.
    code: String,
    /// The internal name that this error is known as. This makes for
    /// easy reading, but we don't promise that this remains consistent
    /// between releases.
    name: String,
    /// A message describing the type of error.
    message: String,
    /// The name of the `Severity` variant, taken from the `Severity` column.
    severity: &'static str,
    /// The CODESYS code that reports the same condition, from the `Codesys`
    /// column, when the decompiled checker confirms it.
    codesys: Option<String>,
}

fn create_problems() -> Result<(), Box<dyn Error>> {
    // Tell Cargo that if the error definitions change, to rerun this build script.
    println!("cargo:rerun-if-changed=resources/problem-codes.csv");
    println!("cargo:rerun-if-changed=resources/problem-texts.csv");
    println!("cargo:rerun-if-changed=resources/codeless-texts.csv");

    // Read the problem-codes.csv into bytes so that we can use it with
    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("problem-codes.csv");

    let src = fs::read_to_string(src_path).expect("Unable to read 'problem-codes.csv'");
    let src = src.as_bytes();

    // Read the file into the definition (we'll iterate over the structs more than once)
    let mut defs = vec![];
    let mut rdr = csv::Reader::from_reader(src);
    let header = rdr.headers()?.clone();
    ironplc_problems_gen::check_problem_codes_header(&header.iter().collect::<Vec<_>>())?;
    for result in rdr.records() {
        let record = result?;
        let code = record
            .get(0)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 0"))?;
        let name = record
            .get(1)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 1"))?;
        let message = record
            .get(2)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 2"))?;
        let severity = record
            .get(3)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 3"))?;
        let codesys = record
            .get(4)
            .ok_or_else(|| format!("Record {record:?} is not valid at column 4"))?;
        defs.push(ProblemDef {
            code: code.to_string(),
            name: name.to_string(),
            message: message.to_string(),
            severity: ironplc_problems_gen::severity_variant(code, severity)?,
            codesys: ironplc_problems_gen::codesys_code(code, codesys)?,
        });
    }

    // Create the output directory and file problems.rs that will have the definitions
    let mut out_path = PathBuf::from(env::var("OUT_DIR")?);
    fs::create_dir_all(out_path.clone())
        .map_err(|e| format!("Unable to create directory 'problems': {e}"))?;

    out_path.push("problems.rs");
    let mut out =
        File::create(out_path).map_err(|e| format!("Unable to create 'problems.rs': {e}"))?;

    // Create the severity enumeration
    out.write_all(ironplc_problems_gen::render_severity_enum().as_bytes())?;

    // Create the enumeration definition
    out.write_all(b"#[derive(Debug, Clone, Copy, PartialEq, Eq)]\npub enum Problem {\n")?;
    for def in &defs {
        if let Some(note) = ironplc_problems_gen::deprecation_note(&def.name) {
            out.write_all(format!("    #[deprecated(note = \"{note}\")]\n").as_bytes())?;
        }
        out.write_all(format!("    {},\n", def.name).as_bytes())?;
    }
    out.write_all(b"}\n\n")?;

    // Create the function to return information about each definition. The impl
    // allows `deprecated` because `code()`/`message()` legitimately match every
    // variant, including the compiler-located ones marked above.
    out.write_all(b"#[expect(deprecated, reason = \"the impl names every variant, including the deprecated compiler-located ones\")]\n")?;
    out.write_all(b"impl Problem {\n")?;

    // Define ALL and from_code(). Both name every variant, the deprecated ones
    // included, which the `expect` on the impl allows.
    out.write_all(b"    /// Every problem, in the order of `problem-codes.csv`.\n")?;
    out.write_all(format!("    pub const ALL: [Problem; {}] = [\n", defs.len()).as_bytes())?;
    for def in &defs {
        out.write_all(format!("        Problem::{},\n", def.name).as_bytes())?;
    }
    out.write_all(b"    ];\n\n")?;
    out.write_all(b"    /// The problem that has the code, written exactly as `problem-codes.csv` writes it\n")?;
    out.write_all(b"    /// (`P4083`), or `None`.\n")?;
    out.write_all(b"    pub fn from_code(code: &str) -> Option<Problem> {\n")?;
    out.write_all(b"        match code {\n")?;
    for def in &defs {
        out.write_all(
            format!(
                "            \"{}\" => Some(Problem::{}),\n",
                def.code, def.name
            )
            .as_bytes(),
        )?;
    }
    out.write_all(b"            _ => None,\n")?;
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n\n")?;

    // Define code()
    out.write_all(b"    /// Returns the code for the particular problem as a string.\n")?;
    out.write_all(b"    pub fn code(&self) -> &str {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        out.write_all(
            format!("            Problem::{} => \"{}\",\n", def.name, def.code).as_bytes(),
        )?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n\n")?;

    // Define message()
    out.write_all(b"    /// Returns the message for the particular problem as a string.\n")?;
    out.write_all(b"    /// The message is constant and does not depend on the particular instance of the problem.\n")?;
    out.write_all(b"    pub fn message(&self) -> &str {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        out.write_all(
            format!(
                "            Problem::{} => \"{}\",\n",
                def.name, def.message
            )
            .as_bytes(),
        )?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n\n")?;

    // Define severity(). The match names every problem, so the build fails on a
    // problem that has no severity.
    out.write_all(b"    /// Returns the severity of the particular problem, which every diagnostic of the problem has.\n")?;
    out.write_all(b"    pub fn severity(&self) -> Severity {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        out.write_all(
            format!(
                "            Problem::{} => Severity::{},\n",
                def.name, def.severity
            )
            .as_bytes(),
        )?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n\n")?;

    // Define codesys(). The match names every problem, so the build fails on a
    // problem the `Codesys` column does not cover.
    out.write_all(b"    /// Returns the CODESYS code that reports the same condition, such as `C0046`, or `None`\n")?;
    out.write_all(b"    /// when no CODESYS message is confirmed to be the same condition.\n")?;
    out.write_all(b"    pub fn codesys(&self) -> Option<&'static str> {\n")?;
    out.write_all(b"        match self {\n")?;
    for def in &defs {
        let arm = match &def.codesys {
            Some(codesys) => format!("Some(\"{codesys}\")"),
            None => "None".to_string(),
        };
        out.write_all(format!("            Problem::{} => {arm},\n", def.name).as_bytes())?;
    }
    out.write_all(b"        }\n")?;
    out.write_all(b"    }\n")?;

    out.write_all(b"}\n\n")?;

    // The warnings: the type a list of codes in a setting holds.
    let pairs: Vec<(&str, &str)> = defs
        .iter()
        .map(|def| (def.name.as_str(), def.severity))
        .collect();
    out.write_all(ironplc_problems_gen::render_warning_problems(&pairs).as_bytes())?;

    // Finalize the out file
    out.flush()?;

    create_texts(&defs)?;
    create_codeless_texts()
}

/// Writes the module `codeless`, one function for each row of
/// `codeless-texts.csv`: the messages that have no problem code.
fn create_codeless_texts() -> Result<(), Box<dyn Error>> {
    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("codeless-texts.csv");
    let src = fs::read_to_string(src_path)
        .map_err(|e| format!("Unable to read 'codeless-texts.csv': {e}"))?;

    let codeless = ironplc_problems_gen::generate_codeless_texts(&src)?;

    let mut out_path = PathBuf::from(env::var("OUT_DIR")?);
    out_path.push("codeless.rs");
    fs::write(out_path, codeless).map_err(|e| format!("Unable to create 'codeless.rs': {e}"))?;
    Ok(())
}

fn create_texts(defs: &[ProblemDef]) -> Result<(), Box<dyn Error>> {
    let mut src_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    src_path.push("resources");
    src_path.push("problem-texts.csv");
    let src = fs::read_to_string(src_path)
        .map_err(|e| format!("Unable to read 'problem-texts.csv': {e}"))?;

    let problems: Vec<(&str, &str)> = defs
        .iter()
        .map(|def| (def.code.as_str(), def.name.as_str()))
        .collect();
    let texts = ironplc_problems_gen::generate_texts(&src, &problems)?;

    let mut out_path = PathBuf::from(env::var("OUT_DIR")?);
    out_path.push("texts.rs");
    fs::write(out_path, texts).map_err(|e| format!("Unable to create 'texts.rs': {e}"))?;
    Ok(())
}

fn main() {
    // The severity column of `problem-codes.csv` is specified in
    // `diagnostic-severity.md`; this crate owns its `REQ-DS-problems-*` tests.
    ironplc_spec_requirements_gen::generate(&["diagnostic-severity.md"]);
    if let Err(err) = create_problems() {
        eprintln!("problem generating problems.rs: {err}");
        process::exit(1);
    }
}
