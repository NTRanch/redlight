# Redlight — task recipes (wired at the v1.0 build, 2026-10-30).
# The C# core (core/Redlight.Core) is the behaviour of record; the
# shipped page runs its JS realization, locked by conformance vectors.
# dotnet 8.0.425 lives in the estate tools tree on the sandbox.

DOTNET := env_var_or_default("DOTNET", join(home_dir(), "workspace/tools/dotnet/dotnet"))

# Assemble the single-file jam entry: JS realization inlined in the shell
build:
    node tools/assemble.mjs

# Regenerate conformance vectors FROM the C# core (behaviour of record)
vectors:
    {{DOTNET}} run --project core/Redlight.Dump -c Release -- conformance/vectors.json

# Run every gate: C# suite, conformance, bridges, perf, scripted playtest
check:
    {{DOTNET}} run --project core/Redlight.Tests -c Release
    node tools/assemble.mjs
    node tools/conformance.mjs
    node tools/settings-check.mjs
    node tools/perf.mjs
    node tools/playtest.mjs

# Render the per-level preview videos from the shipped core (demo mode)
previews:
    node tools/capture.mjs 0 20264
    node tools/capture.mjs 1 20264
    node tools/capture.mjs 2 20264

# Build the planning book
book:
    mdbook build book
