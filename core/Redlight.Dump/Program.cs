// Program.cs — conformance vector dumper. Runs scripted scenarios
// against the C# core (the behaviour of record) and writes
// conformance/vectors.json. tools/conformance.mjs replays the same
// scenarios against the JS realization and must match exactly.
// Usage: Redlight.Dump <out-path>
using System;
using System.IO;
using System.Text;
using Redlight.Core;
using Core = Redlight.Core.Core;


static class Program
{
    // Scripted input formulas, mirrored verbatim in tools/conformance.mjs.
    // mode 0: demo (no external input). mode 1: gate/title confirms then a
    // pose/lens cadence. mode 2: demo at level 2, longer run.
    private static int ScriptBits(int mode, int step)
    {
        if (mode == 1)
        {
            if (step == 0 || step == 1) return InputBits.Confirm;
            int bits = 0;
            if (step % 37 == 0) bits |= InputBits.Tap;
            if (step % 91 == 0) bits |= InputBits.Confirm;
            return bits;
        }
        return 0;
    }

    private static uint Fnv1a(string text, uint hash)
    {
        foreach (byte b in Encoding.ASCII.GetBytes(text))
        {
            hash ^= b;
            hash *= 16777619u;
        }
        return hash;
    }

    private static string Scenario(string name, uint seed, int level, bool demo, int mode, int steps)
    {
        var core = new Core();
        if (core.Init(seed, level, demo) != 0) throw new InvalidOperationException("init");
        core.StartLevel(level);
        uint hash = 2166136261u;
        for (int i = 0; i < steps; i++)
        {
            core.Step(ScriptBits(mode, i));
            if (i % 100 == 0)
            {
                string snap = $"{i};{core.Screen};{core.AttentionMilli};{core.FloorMilli};" +
                              $"{core.EventsFired};{core.DistinctCount};{core.LastLineEncoded};" +
                              $"{core.LensHits};{core.KnownStage};{core.Elapsed}\n";
                hash = Fnv1a(snap, hash);
            }
        }
        hash = Fnv1a(core.Draw(), hash);
        return "    {\"name\": \"" + name + "\", \"seed\": " + seed + ", \"level\": " + level +
               ", \"demo\": " + (demo ? "true" : "false") + ", \"mode\": " + mode +
               ", \"steps\": " + steps + ", \"screen\": " + core.Screen +
               ", \"attention\": " + core.AttentionMilli + ", \"floor\": " + core.FloorMilli +
               ", \"events\": " + core.EventsFired + ", \"distinct\": " + core.DistinctCount +
               ", \"lensHits\": " + core.LensHits + ", \"stage\": " + core.KnownStage +
               ", \"checksum\": " + hash + "}";
    }

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("usage: Redlight.Dump <out-path>");
            return 2;
        }
        var sb = new StringBuilder();
        sb.Append("{\n  \"scenarios\": [\n");
        sb.Append(Scenario("demo-level-1", 42, 0, true, 0, 4000)).Append(",\n");
        sb.Append(Scenario("scripted-level-2", 7, 1, false, 1, 3000)).Append(",\n");
        sb.Append(Scenario("demo-level-3", 1234, 2, true, 2, 5000)).Append('\n');
        sb.Append("  ]\n}\n");
        File.WriteAllText(args[0], sb.ToString());
        Console.WriteLine("vectors written to " + args[0]);
        return 0;
    }
}
