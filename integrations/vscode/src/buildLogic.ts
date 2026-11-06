/**
 * The client-side phases of a build on the engineering connection (ADR-0063,
 * Mechanism 3). A phase is the client's own position in the sequence of an
 * edit (compiling, uploading, verifying, running); each is one call, reported
 * through an injected `onPhase` so the device panel renders it. The stages
 * that map to the phases are the table `STAGES` of the online-edit dialog
 * (`onlineEditLogic`), and a Download reports them as well.
 */

/** The client-side phases of one build, in the order the panel renders them. */
export type BuildPhase = 'compiling' | 'uploading' | 'verifying' | 'running';
