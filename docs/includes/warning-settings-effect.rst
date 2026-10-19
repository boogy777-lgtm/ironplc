A *disabled* warning is not shown and is not counted in the line that ends the
run. A warning that is *reported as an error* is shown and counted as an error,
with the same code, so it stops the build: ``compile`` writes no container and
the last line says ``No download possible``. A code in both lists is reported as
an error, because reporting as an error wins over disabling. An error is never
changed by either list. The :ref:`Precompile area <message-areas>`, which is the
:guilabel:`Problems` panel of the editor, does not show a disabled warning, and
shows a warning that is reported as an error as a warning, as the CODESYS
Messages view does while you edit; the build output shows it as an error.
