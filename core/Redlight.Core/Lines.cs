// Lines.cs — Lux's reactive line sets, one set per stage-event class.
// The sizing law these tables embody (6 lines per class, 3 classes) is
// the week's port-back deliverable: see docs/PORT-BACK.md. Order is
// contract: the JS realization carries the identical tables in the
// identical order, and conformance vectors lock the selection stream.
namespace Redlight.Core;

/// <summary>Stage-event classes the voice system answers.</summary>
public static class EventClass
{
    public const int Applause = 0;
    public const int Compliment = 2;
    public const int Count = 3;
    public const int Flash = 1;
}

/// <summary>The line tables. Class-major: Lines[class][line].</summary>
public static class Lines
{
    public const int LinesPerClass = 6;

    public static readonly string[] ClassNames = { "APPLAUSE", "FLASH", "COMPLIMENT" };

    public static readonly string[][] Table =
    {
        new[]
        {
            "Yes - hear that? All for me.",
            "Louder. I bloom under noise.",
            "That sound keeps me warm.",
            "Clap again - watch me glow.",
            "Your hands say what eyes mean.",
            "More. I am not shy tonight.",
        },
        new[]
        {
            "Caught. Keep the picture.",
            "The flash loves me back.",
            "Freeze me like that - yes.",
            "One more. I dare the lens.",
            "Bright. I do not blink.",
            "Print that one in red.",
        },
        new[]
        {
            "Say it slower. I keep praise.",
            "Kind words raise my floor.",
            "Tell me again - I collect.",
            "Praise looks good on me.",
            "Your voice, my favourite light.",
            "Mm. Known, and glad of it.",
        },
    };

    /// <summary>Body-writing archive stubs, chosen deterministically per seed.</summary>
    public static readonly string[] ArchiveStubs =
    {
        "ink: 'watch me' - wrist (stub)",
        "ink: 'known' - collarbone (stub)",
        "ink: 'again' - hip (stub)",
        "ink: 'yours, the lens' - ankle (stub)",
    };
}
