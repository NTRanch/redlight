// Core.cs — Redlight deterministic fixed-step core (tiger-style-csharp).
// A recorded-rite sampler in Lux's studio: stage events fire on a seeded
// schedule, her reactive lines answer them (coverage is the test), and an
// attention economy runs the session — attention decays, praise raises
// the floor, the lens rewards a timed answer to a flash. The session
// closes in a written record; revelation runs Glimpsed -> Seen -> Known.
// The core emits a per-frame draw list (960x960, palette indices,
// r/c/l/p/t ops) and nothing else: no engine, no clock, no ambient RNG.
// The JS realization in web/game.mjs is the same core transliterated;
// conformance/vectors.json locks the two together exactly.
using System;
using System.Text;

namespace Redlight.Core;

/// <summary>Input bitset shared by shell, tests, and the JS realization.</summary>
public static class InputBits
{
    public const int Back = 4;
    public const int Confirm = 2;
    public const int Down = 32;
    public const int Left = 64;
    public const int Pause = 8;
    public const int Right = 128;
    public const int Tap = 1;
    public const int Up = 16;
}

/// <summary>Deterministic xorshift32. Seed 0 is remapped to a fixed constant.</summary>
public sealed class Rng
{
    private uint _state;

    public Rng(uint seed)
    {
        _state = seed == 0 ? 0x9E3779B9u : seed;
    }

    public uint Next()
    {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }

    public int Below(int bound)
    {
        if (bound <= 0) throw new ArgumentOutOfRangeException(nameof(bound));
        return (int)(Next() % (uint)bound);
    }
}

/// <summary>Per-level tuning. All attention values are milli (0..1000).</summary>
public readonly struct LevelDef
{
    public LevelDef(int durationSteps, int decayPerStep100, int eventBase, int eventVar, int targetMilli, string name)
    {
        DurationSteps = durationSteps;
        DecayPerStep100 = decayPerStep100;
        EventBase = eventBase;
        EventVar = eventVar;
        TargetMilli = targetMilli;
        Name = name;
    }

    public int DecayPerStep100 { get; }
    public int DurationSteps { get; }
    public int EventBase { get; }
    public int EventVar { get; }
    public string Name { get; }
    public int TargetMilli { get; }
}

/// <summary>The game core. One instance is one boot; sessions reset per level start.</summary>
public sealed class Core
{
    public const int AttentionMax = 1000;
    public const int FloorMax = 500;
    public const int LevelCount = 3;
    public const int ScreenGameOver = 7;
    public const int ScreenGate = 0;
    public const int ScreenHowTo = 2;
    public const int ScreenPause = 4;
    public const int ScreenPlay = 3;
    public const int ScreenSettings = 5;
    public const int ScreenTitle = 1;
    public const int ScreenVictory = 6;

    private const int DarkLimitSteps = 150;
    private const int FlashWindowSteps = 36;
    private const int PoseCooldownSteps = 30;
    private const int PoseGainMilli = 55;

    private static readonly LevelDef[] Levels =
    {
        new(2700, 30, 260, 160, 420, "SOLO RECORD"),
        new(3300, 38, 210, 140, 480, "LIVE SMALL"),
        new(3900, 46, 170, 120, 540, "LIVE FULL"),
    };

    private readonly bool[,] _heard = new bool[EventClass.Count, Lines.LinesPerClass];
    private readonly int[] _lastLine = { -1, -1, -1 };
    private readonly int[] _perClass = new int[EventClass.Count];

    private int _archiveBestStage;
    private int _archiveSessions;
    private int _attentionMilli = 420;
    private int _darkSteps;
    private int _decayAcc;
    private bool _demo;
    private int _demoTick;
    private int _distinctCount;
    private int _elapsed;
    private int _eventsFired;
    private int _flashWindow;
    private int _floorMilli;
    private int _knownStage;
    private string _lastHeardLine = "";
    private int _lensHits;
    private int _levelIdx;
    private int _menuCursor;
    private int _nextEventStep;
    private bool _paceQuick;
    private int _poseCooldown;
    private Rng _rng = new(1);
    private int _screen = ScreenGate;
    private uint _seed = 1;
    private int _settingsCursor;
    private bool _textLarge;

    public int ArchiveBestStage => _archiveBestStage;
    public int ArchiveSessions => _archiveSessions;
    public int AttentionMilli => _attentionMilli;
    public int DistinctCount => _distinctCount;
    public int Elapsed => _elapsed;
    public int EventsFired => _eventsFired;
    public int FloorMilli => _floorMilli;
    public int KnownStage => _knownStage;
    public int LensHits => _lensHits;
    public int LevelIdx => _levelIdx;
    public int Screen => _screen;

    /// <summary>Encoded last-line triple for checksums: (a+1)*49+(f+1)*7+(c+1).</summary>
    public int LastLineEncoded =>
        (_lastLine[0] + 1) * 49 + (_lastLine[1] + 1) * 7 + (_lastLine[2] + 1);

    public static LevelDef LevelDefAt(int idx) => Levels[idx];

    /// <summary>Boot the core. Returns 0 on success, 1 on a bad level.</summary>
    public int Init(uint seed, int level, bool demo)
    {
        if (level < 0 || level >= LevelCount) return 1;
        _seed = seed;
        _levelIdx = level;
        _demo = demo;
        _screen = ScreenGate;
        _menuCursor = 0;
        _settingsCursor = 0;
        _demoTick = 0;
        _rng = new Rng(seed);
        ResetSession();
        return 0;
    }

    /// <summary>Select and arm a level from the title. Returns 0 ok, 1 bad level.</summary>
    public int StartLevel(int level)
    {
        if (level < 0 || level >= LevelCount) return 1;
        _levelIdx = level;
        _rng = new Rng(_seed + (uint)(level * 7919));
        ResetSession();
        return 0;
    }

    /// <summary>Settings bridge in: pace bit 1, large-text bit 2.</summary>
    public void ApplySettings(bool paceQuick, bool textLarge)
    {
        _paceQuick = paceQuick;
        _textLarge = textLarge;
    }

    public int SettingsBits() => (_paceQuick ? 1 : 0) | (_textLarge ? 2 : 0);

    /// <summary>Written-record bridge in; values clamp to sane bounds.</summary>
    public void ApplyArchive(int sessions, int bestStage)
    {
        _archiveSessions = Math.Clamp(sessions, 0, 999);
        _archiveBestStage = Math.Clamp(bestStage, 0, 2);
    }

    /// <summary>Advance one fixed 16 ms step. Returns an event code (0 = none).</summary>
    public int Step(int inputBits)
    {
        int bits = inputBits & 0xFF;
        if (_demo) bits |= DemoBits();
        switch (_screen)
        {
            case ScreenGate:
                if ((bits & (InputBits.Confirm | InputBits.Tap)) != 0) _screen = ScreenTitle;
                return 0;
            case ScreenTitle:
                return StepTitle(bits);
            case ScreenHowTo:
                if ((bits & (InputBits.Back | InputBits.Confirm)) != 0) _screen = ScreenTitle;
                return 0;
            case ScreenSettings:
                return StepSettings(bits);
            case ScreenPause:
                if ((bits & (InputBits.Confirm | InputBits.Back | InputBits.Pause | InputBits.Tap)) != 0)
                {
                    _screen = ScreenPlay;
                }
                return 0;
            case ScreenVictory:
            case ScreenGameOver:
                if ((bits & (InputBits.Confirm | InputBits.Tap)) != 0) _screen = ScreenTitle;
                return 0;
            default:
                return StepPlay(bits);
        }
    }

    private int DemoBits()
    {
        _demoTick++;
        if (_screen == ScreenGate && _demoTick >= 20) { _demoTick = 0; return InputBits.Confirm; }
        if (_screen == ScreenTitle && _demoTick >= 20) { _demoTick = 0; return InputBits.Confirm; }
        if (_screen == ScreenPlay)
        {
            int bits = 0;
            if (_flashWindow > 0) bits |= InputBits.Confirm;
            if (_poseCooldown == 0 && _attentionMilli < 880) bits |= InputBits.Tap;
            return bits;
        }
        return 0;
    }

    private int StepTitle(int bits)
    {
        if ((bits & InputBits.Up) != 0) _menuCursor = (_menuCursor + 2) % 3;
        if ((bits & InputBits.Down) != 0) _menuCursor = (_menuCursor + 1) % 3;
        if (_menuCursor == 0)
        {
            if ((bits & InputBits.Left) != 0) _levelIdx = (_levelIdx + LevelCount - 1) % LevelCount;
            if ((bits & InputBits.Right) != 0) _levelIdx = (_levelIdx + 1) % LevelCount;
        }
        if ((bits & (InputBits.Confirm | InputBits.Tap)) == 0) return 0;
        if (_menuCursor == 1) { _screen = ScreenHowTo; return 8; }
        if (_menuCursor == 2) { _screen = ScreenSettings; return 8; }
        StartLevel(_levelIdx);
        _screen = ScreenPlay;
        return 8;
    }

    private int StepSettings(int bits)
    {
        if ((bits & InputBits.Up) != 0) _settingsCursor = (_settingsCursor + 2) % 3;
        if ((bits & InputBits.Down) != 0) _settingsCursor = (_settingsCursor + 1) % 3;
        if ((bits & InputBits.Back) != 0) { _screen = ScreenTitle; return 8; }
        if ((bits & (InputBits.Confirm | InputBits.Tap)) == 0) return 0;
        if (_settingsCursor == 0) _paceQuick = !_paceQuick;
        else if (_settingsCursor == 1) _textLarge = !_textLarge;
        else { _screen = ScreenTitle; return 8; }
        return 8;
    }

    private int StepPlay(int bits)
    {
        if ((bits & InputBits.Pause) != 0) { _screen = ScreenPause; return 0; }
        LevelDef def = Levels[_levelIdx];
        int duration = _paceQuick ? def.DurationSteps * 3 / 4 : def.DurationSteps;
        int eventCode = 0;

        if (_poseCooldown > 0) _poseCooldown--;
        if ((bits & InputBits.Tap) != 0 && _poseCooldown == 0)
        {
            _attentionMilli = Math.Min(AttentionMax, _attentionMilli + PoseGainMilli);
            _poseCooldown = PoseCooldownSteps;
            eventCode = 1;
        }

        if (_flashWindow > 0)
        {
            if ((bits & InputBits.Confirm) != 0)
            {
                _flashWindow = 0;
                _lensHits++;
                _attentionMilli = Math.Min(AttentionMax, _attentionMilli + 95);
                eventCode = 5;
            }
            else
            {
                _flashWindow--;
            }
        }

        _elapsed++;
        if (_elapsed >= _nextEventStep)
        {
            eventCode = FireEvent(def);
        }

        _decayAcc += def.DecayPerStep100;
        int decay = _decayAcc / 100;
        _decayAcc %= 100;
        if (decay > 0 && _attentionMilli > _floorMilli)
        {
            _attentionMilli = Math.Max(_floorMilli, _attentionMilli - decay);
        }

        if (_attentionMilli <= 0) _darkSteps++; else _darkSteps = 0;
        UpdateKnown(duration);

        if (_darkSteps >= DarkLimitSteps)
        {
            _screen = ScreenGameOver;
            return 7;
        }
        if (_elapsed >= duration)
        {
            if (_attentionMilli >= def.TargetMilli)
            {
                _screen = ScreenVictory;
                return 6;
            }
            _screen = ScreenGameOver;
            return 7;
        }
        return eventCode;
    }

    private int FireEvent(LevelDef def)
    {
        int cls = _rng.Below(EventClass.Count);
        int line = PickLine(cls);
        _eventsFired++;
        _perClass[cls]++;
        _lastHeardLine = Lines.Table[cls][line];
        _nextEventStep = _elapsed + def.EventBase + _rng.Below(def.EventVar);
        switch (cls)
        {
            case EventClass.Applause:
                _attentionMilli = Math.Min(AttentionMax, _attentionMilli + 70);
                return 2;
            case EventClass.Flash:
                _flashWindow = FlashWindowSteps;
                return 3;
            default:
                _floorMilli = Math.Min(FloorMax, _floorMilli + 50);
                _attentionMilli = Math.Min(AttentionMax, _attentionMilli + 35);
                return 4;
        }
    }

    private int PickLine(int cls)
    {
        int line = _rng.Below(Lines.LinesPerClass);
        if (line == _lastLine[cls])
        {
            line = (line + 1 + _rng.Below(Lines.LinesPerClass - 1)) % Lines.LinesPerClass;
        }
        _lastLine[cls] = line;
        if (!_heard[cls, line])
        {
            _heard[cls, line] = true;
            _distinctCount++;
        }
        return line;
    }

    private void UpdateKnown(int duration)
    {
        int known = _elapsed * 500 / Math.Max(1, duration) + _distinctCount * 28 + _lensHits * 22;
        if (known > 1000) known = 1000;
        _knownStage = known < 340 ? 0 : known < 680 ? 1 : 2;
    }

    private void ResetSession()
    {
        _attentionMilli = 420;
        _darkSteps = 0;
        _decayAcc = 0;
        _distinctCount = 0;
        _elapsed = 0;
        _eventsFired = 0;
        _flashWindow = 0;
        _floorMilli = 0;
        _knownStage = 0;
        _lastHeardLine = "";
        _lensHits = 0;
        _nextEventStep = 180 + _rng.Below(180);
        _poseCooldown = 0;
        for (int c = 0; c < EventClass.Count; c++)
        {
            _lastLine[c] = -1;
            _perClass[c] = 0;
            for (int l = 0; l < Lines.LinesPerClass; l++) _heard[c, l] = false;
        }
    }

    // ---- draw list ------------------------------------------------------------
    // Line protocol: "r x y w h c", "c x y r c", "l x1 y1 x2 y2 w c",
    // "p x1 y1 x2 y2 x3 y3 c", "t x y size c text". Integers only. This
    // text is part of the conformance surface: C# and JS must agree.

    private int TextBoost => _textLarge ? 2 : 0;

    public string Draw()
    {
        var sb = new StringBuilder(4096);
        void R(int x, int y, int w, int h, int c) => sb.Append("r ").Append(x).Append(' ').Append(y).Append(' ').Append(w).Append(' ').Append(h).Append(' ').Append(c).Append('\n');
        void C(int x, int y, int r, int c) => sb.Append("c ").Append(x).Append(' ').Append(y).Append(' ').Append(r).Append(' ').Append(c).Append('\n');
        void L(int x1, int y1, int x2, int y2, int w, int c) => sb.Append("l ").Append(x1).Append(' ').Append(y1).Append(' ').Append(x2).Append(' ').Append(y2).Append(' ').Append(w).Append(' ').Append(c).Append('\n');
        void T(int x, int y, int size, int c, string s) => sb.Append("t ").Append(x).Append(' ').Append(y).Append(' ').Append(size).Append(' ').Append(c).Append(' ').Append(s).Append('\n');

        R(0, 0, 960, 960, 0);
        R(24, 24, 912, 912, 9);
        R(32, 32, 896, 896, 0);
        switch (_screen)
        {
            case ScreenGate:
                T(336, 300, 40, 1, "REDLIGHT");
                T(252, 380, 20, 2, "A RECORDED RITE IN HER STUDIO");
                T(192, 440, 16, 3, "18+ ONLY. ADULTS ONLY BEYOND THIS GATE.");
                T(216, 476, 16, 3, "CONTAINS EXHIBITIONISM AND PRAISE PLAY.");
                T(264, 560, 18, 8, "PRESS ENTER OR TAP TO CONFIRM 18+");
                break;
            case ScreenTitle:
                T(336, 180, 40, 1, "REDLIGHT");
                T(300, 250, 18, 3, "LUX, ON THE RECORD");
                T(240, 340, 20, _menuCursor == 0 ? 1 : 2, (_menuCursor == 0 ? "> " : "  ") + "START - " + Levels[_levelIdx].Name);
                T(240, 396, 20, _menuCursor == 1 ? 1 : 2, (_menuCursor == 1 ? "> " : "  ") + "HOW TO PLAY");
                T(240, 452, 20, _menuCursor == 2 ? 1 : 2, (_menuCursor == 2 ? "> " : "  ") + "SETTINGS");
                T(240, 540, 16, 3, "LEFT/RIGHT PICKS THE AUDIENCE ON START.");
                T(240, 572, 16, 3, "RECORD: " + _archiveSessions + " SESSIONS, BEST " + StageName(_archiveBestStage));
                break;
            case ScreenHowTo:
                T(240, 160, 28, 1, "HOW TO PLAY");
                T(120, 240, 16 + TextBoost, 2, "THE LENS WATCHES. ATTENTION ALWAYS DECAYS.");
                T(120, 280, 16 + TextBoost, 2, "TAP OR SPACE: POSE. POSE RAISES ATTENTION.");
                T(120, 320, 16 + TextBoost, 2, "WHEN A FLASH FIRES, ANSWER WITH ENTER");
                T(120, 360, 16 + TextBoost, 2, "BEFORE ITS RING CLOSES: THE LENS PAYS.");
                T(120, 400, 16 + TextBoost, 2, "COMPLIMENTS RAISE THE FLOOR UNDER YOU.");
                T(120, 440, 16 + TextBoost, 2, "APPLAUSE FEEDS THE ROOM. HER LINES ANSWER");
                T(120, 480, 16 + TextBoost, 2, "EVERY EVENT. END SEEN, END ABOVE TARGET.");
                T(120, 560, 16, 3, "P PAUSES. BACKSPACE RETURNS.");
                T(120, 620, 18, 8, "ENTER OR BACKSPACE TO RETURN");
                break;
            case ScreenSettings:
                T(300, 180, 28, 1, "SETTINGS");
                T(240, 300, 20, _settingsCursor == 0 ? 1 : 2, (_settingsCursor == 0 ? "> " : "  ") + "PACE: " + (_paceQuick ? "QUICK" : "FULL"));
                T(240, 356, 20, _settingsCursor == 1 ? 1 : 2, (_settingsCursor == 1 ? "> " : "  ") + "TEXT: " + (_textLarge ? "LARGE" : "STD"));
                T(240, 412, 20, _settingsCursor == 2 ? 1 : 2, (_settingsCursor == 2 ? "> " : "  ") + "BACK");
                T(240, 500, 16, 3, "SETTINGS ARE NEVER GATED BY PROGRESS.");
                T(240, 532, 16, 3, "SAVES ON WEB ARE BEST-EFFORT (INDEXEDDB).");
                break;
            case ScreenPlay:
            case ScreenPause:
                DrawPlay(R, C, L, T);
                if (_screen == ScreenPause)
                {
                    R(280, 400, 400, 140, 9);
                    T(408, 440, 28, 1, "PAUSED");
                    T(340, 496, 16, 2, "ENTER RESUMES. THE LENS WAITS.");
                }
                break;
            case ScreenVictory:
                T(336, 170, 32, 1, "ON RECORD");
                T(240, 250, 18, 2, "THE WRITTEN RECORD - " + Levels[_levelIdx].Name);
                T(240, 310, 16 + TextBoost, 2, "ATTENTION " + _attentionMilli + " / TARGET " + Levels[_levelIdx].TargetMilli);
                T(240, 344, 16 + TextBoost, 2, "EVENTS " + _eventsFired + " - LINES HEARD " + _distinctCount + " OF 18");
                T(240, 378, 16 + TextBoost, 2, "APPLAUSE " + _perClass[0] + " FLASH " + _perClass[1] + " PRAISE " + _perClass[2]);
                T(240, 412, 16 + TextBoost, 2, "LENS ANSWERS " + _lensHits + " - REVELATION: " + StageName(_knownStage));
                T(240, 470, 16, 8, "BODY-WRITING ARCHIVE (STUB):");
                T(240, 504, 16, 5, Lines.ArchiveStubs[(int)(_seed % 4)]);
                T(240, 538, 16, 5, Lines.ArchiveStubs[(int)((_seed + 2) % 4)]);
                T(240, 620, 18, 8, "ENTER RETURNS TO TITLE");
                break;
            default:
                T(312, 220, 32, 5, "DARK ROOM");
                T(240, 300, 18, 2, "THE ATTENTION DIED. THE RECORD KEEPS:");
                T(240, 356, 16 + TextBoost, 2, "EVENTS " + _eventsFired + " - LINES HEARD " + _distinctCount + " OF 18");
                T(240, 390, 16 + TextBoost, 2, "REVELATION REACHED: " + StageName(_knownStage));
                T(240, 470, 18, 8, "ENTER RETURNS TO TITLE");
                break;
        }
        return sb.ToString();
    }

    private void DrawPlay(Action<int, int, int, int, int> r, Action<int, int, int, int> c,
        Action<int, int, int, int, int, int> l, Action<int, int, int, int, string> t)
    {
        LevelDef def = Levels[_levelIdx];
        int duration = _paceQuick ? def.DurationSteps * 3 / 4 : def.DurationSteps;
        // Stage: Lux as abstract light — a hot disc, a red record light.
        c(480, 420, 150, 4);
        c(480, 420, 118, 1);
        c(480, 396, 44, 2);
        c(462, 384, 9, 0);
        c(498, 384, 9, 0);
        l(380, 560, 580, 560, 6, 5);
        c(852, 96, 16, 1);
        t(760, 72, 16, 3, "REC");
        t(64, 64, 20, 1, "REDLIGHT - " + def.Name);
        // Attention bar with floor marker and target tick.
        r(64, 800, 832, 28, 9);
        r(64, 800, _attentionMilli * 832 / 1000, 28, 1);
        r(64 + _floorMilli * 832 / 1000, 792, 4, 44, 8);
        r(64 + def.TargetMilli * 832 / 1000, 792, 4, 44, 2);
        t(64, 844, 16, 2, "ATTENTION " + _attentionMilli + " FLOOR " + _floorMilli + " TARGET " + def.TargetMilli);
        // Session clock.
        r(64, 120, 832, 10, 9);
        r(64, 120, Math.Min(832, _elapsed * 832 / Math.Max(1, duration)), 10, 5);
        t(64, 142, 16, 3, "REVELATION: " + StageName(_knownStage) + " - LINES " + _distinctCount + "/18");
        if (_flashWindow > 0)
        {
            c(480, 420, 168, 8);
            t(336, 640, 18, 8, "FLASH! ANSWER WITH ENTER");
        }
        if (_lastHeardLine.Length > 0)
        {
            t(144, 700, 16 + TextBoost, 6, "LUX: " + _lastHeardLine);
        }
        if (_poseCooldown > 0)
        {
            t(64, 640, 16, 7, "POSE RECOVERING");
        }
    }

    private static string StageName(int stage) => stage switch
    {
        0 => "GLIMPSED",
        1 => "SEEN",
        _ => "KNOWN",
    };
}
