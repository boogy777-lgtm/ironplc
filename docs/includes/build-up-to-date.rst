A build that finds nothing changed since the last successful build does not
compile. It writes the line that starts the build, then the line ``The
application is up to date``, and leaves the container as it is. A change of a
source file, of an activated compatibility library, of a setting or of the
compiler makes the build compile, and so does a container that is missing or is
not the one the last build wrote.
