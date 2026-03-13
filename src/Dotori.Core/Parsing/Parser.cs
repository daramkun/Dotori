namespace Dotori.Core.Parsing;

public sealed class ParseException(string message, SourceLocation location)
    : Exception($"{location}: {message}")
{
    public SourceLocation Location { get; } = location;
}

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private readonly string _filePath;
    private int _pos;

    public Parser(List<Token> tokens, string filePath)
    {
        _tokens = tokens;
        _filePath = filePath;
    }

    // ─── Helpers ───────────────────────────────────────────────────────────

    private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];
    private Token Peek(int offset = 1) => (_pos + offset) < _tokens.Count ? _tokens[_pos + offset] : _tokens[^1];

    private Token Consume()
    {
        var t = Current;
        _pos++;
        return t;
    }

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind != kind)
            throw new ParseException(
                $"Expected {kind} but got {Current.Kind} '{Current.Text}'",
                Current.Location);
        return Consume();
    }

    private bool TryConsume(TokenKind kind)
    {
        if (Current.Kind == kind) { Consume(); return true; }
        return false;
    }

    // ─── Entry ─────────────────────────────────────────────────────────────

    public DotoriFile ParseFile()
    {
        ProjectDecl? project = null;
        PackageDecl? package = null;

        while (Current.Kind != TokenKind.Eof)
        {
            if (Current.Kind == TokenKind.Ident && Current.Text == "project")
            {
                if (project != null)
                    throw new ParseException("Duplicate 'project' declaration", Current.Location);
                project = ParseProject();
            }
            else if (Current.Kind == TokenKind.Ident && Current.Text == "package")
            {
                if (package != null)
                    throw new ParseException("Duplicate 'package' declaration", Current.Location);
                package = ParsePackage();
            }
            else
            {
                throw new ParseException(
                    $"Expected 'project' or 'package', got '{Current.Text}'",
                    Current.Location);
            }
        }

        return new DotoriFile
        {
            FilePath = _filePath,
            Project = project,
            Package = package,
        };
    }

    // ─── Project ───────────────────────────────────────────────────────────

    private ProjectDecl ParseProject()
    {
        var loc = Current.Location;
        Expect(TokenKind.Ident); // "project"
        var name = Expect(TokenKind.Ident).Text;
        Expect(TokenKind.LBrace);

        var decl = new ProjectDecl { Location = loc, Name = name };

        while (Current.Kind != TokenKind.RBrace && Current.Kind != TokenKind.Eof)
        {
            decl.Items.Add(ParseProjectItem());
        }

        Expect(TokenKind.RBrace);
        return decl;
    }

    private ProjectItem ParseProjectItem()
    {
        // Condition block: [windows] { ... }
        if (Current.Kind == TokenKind.LBracket)
            return ParseConditionBlock();

        if (Current.Kind != TokenKind.Ident)
            throw new ParseException($"Expected identifier, got '{Current.Text}'", Current.Location);

        var loc = Current.Location;
        return Current.Text switch
        {
            "type"               => ParseSimpleProp(loc),
            "std"                => ParseSimpleProp(loc),
            "description"        => ParseSimpleProp(loc),
            "optimize"           => ParseSimpleProp(loc),
            "debug-info"         => ParseSimpleProp(loc),
            "runtime-link"       => ParseSimpleProp(loc),
            "libc"               => ParseSimpleProp(loc),
            "stdlib"             => ParseSimpleProp(loc),
            "lto"                => ParseSimpleProp(loc),
            "warnings"           => ParseSimpleProp(loc),
            "warnings-as-errors" => ParseSimpleProp(loc),
            "android-api-level"  => ParseSimpleProp(loc),
            "macos-min"          => ParseSimpleProp(loc),
            "ios-min"            => ParseSimpleProp(loc),
            "tvos-min"           => ParseSimpleProp(loc),
            "watchos-min"        => ParseSimpleProp(loc),
            "emscripten-flags"   => ParseEmscriptenFlags(loc),
            "sources"            => ParseSourcesBlock(loc, isModules: false),
            "modules"            => ParseSourcesBlock(loc, isModules: true),
            "headers"            => ParseHeadersBlock(loc),
            "defines"            => ParseDefinesBlock(loc),
            "links"              => ParseLinksBlock(loc),
            "frameworks"         => ParseFrameworksBlock(loc),
            "compile-flags"      => ParseCompileFlagsBlock(loc),
            "link-flags"         => ParseLinkFlagsBlock(loc),
            "dependencies"       => ParseDependenciesBlock(loc),
            "pch"                => ParsePchBlock(loc),
            "unity-build"        => ParseUnityBuildBlock(loc),
            "output"             => ParseOutputBlock(loc),
            "pre-build"          => ParseBuildScriptBlock(loc, isPost: false),
            "post-build"         => ParseBuildScriptBlock(loc, isPost: true),
            _                    => throw new ParseException($"Unknown project item '{Current.Text}'", loc),
        };
    }

    private ProjectItem ParseSimpleProp(SourceLocation loc)
    {
        var key = Consume().Text;
        Expect(TokenKind.Equals);

        return key switch
        {
            "type" => new ProjectTypeProp(ParseProjectType()) { Location = loc },
            "std" => new StdProp(ParseCxxStd()) { Location = loc },
            "description" => new DescriptionProp(Expect(TokenKind.String).Text) { Location = loc },
            "optimize" => new OptimizeProp(ParseOptimizeLevel()) { Location = loc },
            "debug-info" => new DebugInfoProp(ParseDebugInfoLevel()) { Location = loc },
            "runtime-link" => new RuntimeLinkProp(ParseRuntimeLink()) { Location = loc },
            "libc" => new LibcProp(ParseLibcType()) { Location = loc },
            "stdlib" => new StdlibProp(ParseStdlibType()) { Location = loc },
            "lto" => new LtoProp(ParseBool()) { Location = loc },
            "warnings" => new WarningsProp(ParseWarningLevel()) { Location = loc },
            "warnings-as-errors" => new WarningsAsErrorsProp(ParseBool()) { Location = loc },
            "android-api-level" => new AndroidApiLevelProp(int.Parse(Expect(TokenKind.Integer).Text)) { Location = loc },
            "macos-min" => new MacosMinProp(Expect(TokenKind.String).Text) { Location = loc },
            "ios-min" => new IosMinProp(Expect(TokenKind.String).Text) { Location = loc },
            "tvos-min" => new TvosMinProp(Expect(TokenKind.String).Text) { Location = loc },
            "watchos-min" => new WatchosMinProp(Expect(TokenKind.String).Text) { Location = loc },
            _ => throw new ParseException($"Unknown property '{key}'", loc),
        };
    }

    private EmscriptenFlagsProp ParseEmscriptenFlags(SourceLocation loc)
    {
        Consume(); // "emscripten-flags"
        Expect(TokenKind.LBrace);
        var flags = new List<string>();
        while (Current.Kind == TokenKind.String)
            flags.Add(Consume().Text);
        Expect(TokenKind.RBrace);
        return new EmscriptenFlagsProp(flags) { Location = loc };
    }

    private SourcesBlock ParseSourcesBlock(SourceLocation loc, bool isModules)
    {
        Consume(); // "sources" or "modules"
        Expect(TokenKind.LBrace);
        var block = new SourcesBlock(isModules) { Location = loc };
        while (Current.Kind == TokenKind.Ident)
        {
            if (Current.Text == "include" || Current.Text == "exclude")
            {
                var isInclude = Consume().Text == "include";
                var glob = Expect(TokenKind.String).Text;
                block.Items.Add(new SourceItem(isInclude, glob));
            }
            else if (isModules && Current.Text == "export-map")
            {
                Consume(); // "export-map"
                Expect(TokenKind.Equals);
                block.ExportMap = ParseBool();
            }
            else
            {
                break;
            }
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private HeadersBlock ParseHeadersBlock(SourceLocation loc)
    {
        Consume(); // "headers"
        Expect(TokenKind.LBrace);
        var block = new HeadersBlock { Location = loc };
        while (Current.Kind == TokenKind.Ident &&
               (Current.Text == "public" || Current.Text == "private"))
        {
            var isPublic = Consume().Text == "public";
            var path = Expect(TokenKind.String).Text;
            block.Items.Add(new HeaderItem(isPublic, path));
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private List<string> ParseStringList()
    {
        Expect(TokenKind.LBrace);
        var list = new List<string>();
        while (Current.Kind == TokenKind.String || Current.Kind == TokenKind.Ident)
            list.Add(Consume().Text);
        Expect(TokenKind.RBrace);
        return list;
    }

    private DefinesBlock ParseDefinesBlock(SourceLocation loc)
    {
        Consume(); // "defines"
        var block = new DefinesBlock { Location = loc };
        block.Values.AddRange(ParseStringList());
        return block;
    }

    private LinksBlock ParseLinksBlock(SourceLocation loc)
    {
        Consume(); // "links"
        var block = new LinksBlock { Location = loc };
        block.Values.AddRange(ParseStringList());
        return block;
    }

    private FrameworksBlock ParseFrameworksBlock(SourceLocation loc)
    {
        Consume(); // "frameworks"
        var block = new FrameworksBlock { Location = loc };
        block.Values.AddRange(ParseStringList());
        return block;
    }

    private CompileFlagsBlock ParseCompileFlagsBlock(SourceLocation loc)
    {
        Consume(); // "compile-flags"
        var block = new CompileFlagsBlock { Location = loc };
        block.Values.AddRange(ParseStringList());
        return block;
    }

    private LinkFlagsBlock ParseLinkFlagsBlock(SourceLocation loc)
    {
        Consume(); // "link-flags"
        var block = new LinkFlagsBlock { Location = loc };
        block.Values.AddRange(ParseStringList());
        return block;
    }

    private DependenciesBlock ParseDependenciesBlock(SourceLocation loc)
    {
        Consume(); // "dependencies"
        Expect(TokenKind.LBrace);
        var block = new DependenciesBlock { Location = loc };
        while (Current.Kind == TokenKind.Ident)
        {
            var name = Consume().Text;
            Expect(TokenKind.Equals);
            var value = ParseDepValue();
            block.Items.Add(new DependencyItem(name, value));
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private DependencyValue ParseDepValue()
    {
        if (Current.Kind == TokenKind.String)
            return new VersionDependency(Consume().Text);

        Expect(TokenKind.LBrace);
        var dep = new ComplexDependency();
        while (Current.Kind != TokenKind.RBrace && Current.Kind != TokenKind.Eof)
        {
            var key = Expect(TokenKind.Ident).Text;
            Expect(TokenKind.Equals);
            switch (key)
            {
                case "git":     dep.Git     = Expect(TokenKind.String).Text; break;
                case "tag":     dep.Tag     = Expect(TokenKind.String).Text; break;
                case "commit":  dep.Commit  = Expect(TokenKind.String).Text; break;
                case "path":    dep.Path    = Expect(TokenKind.String).Text; break;
                case "version": dep.Version = Expect(TokenKind.String).Text; break;
                default: throw new ParseException($"Unknown dependency option '{key}'", Current.Location);
            }
            TryConsume(TokenKind.Comma);
        }
        Expect(TokenKind.RBrace);
        return dep;
    }

    private PchBlock ParsePchBlock(SourceLocation loc)
    {
        Consume(); // "pch"
        Expect(TokenKind.LBrace);
        var block = new PchBlock { Location = loc };
        while (Current.Kind == TokenKind.Ident)
        {
            var key = Consume().Text;
            Expect(TokenKind.Equals);
            switch (key)
            {
                case "header":  block.Header  = Expect(TokenKind.String).Text; break;
                case "source":  block.Source  = Expect(TokenKind.String).Text; break;
                case "modules": block.Modules = ParseBool(); break;
                default: throw new ParseException($"Unknown pch option '{key}'", Current.Location);
            }
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private UnityBuildBlock ParseUnityBuildBlock(SourceLocation loc)
    {
        Consume(); // "unity-build"
        Expect(TokenKind.LBrace);
        var block = new UnityBuildBlock { Location = loc };
        while (Current.Kind == TokenKind.Ident)
        {
            var key = Consume().Text;
            switch (key)
            {
                case "enabled":
                    Expect(TokenKind.Equals);
                    block.Enabled = ParseBool();
                    break;
                case "batch-size":
                    Expect(TokenKind.Equals);
                    block.BatchSize = int.Parse(Expect(TokenKind.Integer).Text);
                    break;
                case "exclude":
                    Expect(TokenKind.LBrace);
                    while (Current.Kind == TokenKind.String)
                        block.Exclude.Add(Consume().Text);
                    Expect(TokenKind.RBrace);
                    break;
                default: throw new ParseException($"Unknown unity-build option '{key}'", Current.Location);
            }
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private OutputBlock ParseOutputBlock(SourceLocation loc)
    {
        Consume(); // "output"
        Expect(TokenKind.LBrace);
        var block = new OutputBlock { Location = loc };
        while (Current.Kind == TokenKind.Ident)
        {
            var key = Consume().Text;
            Expect(TokenKind.Equals);
            switch (key)
            {
                case "binaries":  block.Binaries  = Expect(TokenKind.String).Text; break;
                case "libraries": block.Libraries = Expect(TokenKind.String).Text; break;
                case "symbols":   block.Symbols   = Expect(TokenKind.String).Text; break;
                default: throw new ParseException($"Unknown output option '{key}'", Current.Location);
            }
        }
        Expect(TokenKind.RBrace);
        return block;
    }

    private ProjectItem ParseBuildScriptBlock(SourceLocation loc, bool isPost)
    {
        Consume(); // "pre-build" or "post-build"
        Expect(TokenKind.LBrace);
        if (isPost)
        {
            var block = new PostBuildBlock { Location = loc };
            while (Current.Kind == TokenKind.String)
                block.Commands.Add(Consume().Text);
            Expect(TokenKind.RBrace);
            return block;
        }
        else
        {
            var block = new PreBuildBlock { Location = loc };
            while (Current.Kind == TokenKind.String)
                block.Commands.Add(Consume().Text);
            Expect(TokenKind.RBrace);
            return block;
        }
    }

    private ConditionBlock ParseConditionBlock()
    {
        var loc = Current.Location;
        Expect(TokenKind.LBracket);

        // condition = ident { "." ident }
        var atoms = new List<string>();
        atoms.Add(Expect(TokenKind.Ident).Text);
        while (Current.Kind == TokenKind.Dot)
        {
            Consume(); // "."
            atoms.Add(Expect(TokenKind.Ident).Text);
        }
        Expect(TokenKind.RBracket);

        Expect(TokenKind.LBrace);
        var block = new ConditionBlock(new ConditionExpr(atoms)) { Location = loc };
        while (Current.Kind != TokenKind.RBrace && Current.Kind != TokenKind.Eof)
            block.Items.Add(ParseProjectItem());
        Expect(TokenKind.RBrace);
        return block;
    }

    // ─── Package ───────────────────────────────────────────────────────────

    private PackageDecl ParsePackage()
    {
        var loc = Current.Location;
        Expect(TokenKind.Ident); // "package"
        Expect(TokenKind.LBrace);

        var decl = new PackageDecl { Location = loc };

        while (Current.Kind != TokenKind.RBrace && Current.Kind != TokenKind.Eof)
        {
            if (Current.Kind != TokenKind.Ident)
                throw new ParseException($"Expected identifier in package block", Current.Location);

            var key = Consume().Text;
            switch (key)
            {
                case "name":
                    Expect(TokenKind.Equals);
                    decl.Name = Expect(TokenKind.String).Text;
                    break;
                case "version":
                    Expect(TokenKind.Equals);
                    decl.Version = Expect(TokenKind.String).Text;
                    break;
                case "description":
                    Expect(TokenKind.Equals);
                    decl.Description = Expect(TokenKind.String).Text;
                    break;
                case "license":
                    Expect(TokenKind.Equals);
                    decl.License = Expect(TokenKind.String).Text;
                    break;
                case "homepage":
                    Expect(TokenKind.Equals);
                    decl.Homepage = Expect(TokenKind.String).Text;
                    break;
                case "authors":
                    Expect(TokenKind.LBrace);
                    while (Current.Kind == TokenKind.String)
                        decl.Authors.Add(Consume().Text);
                    Expect(TokenKind.RBrace);
                    break;
                case "exports":
                    Expect(TokenKind.LBrace);
                    while (Current.Kind == TokenKind.Ident)
                    {
                        var exportName = Consume().Text;
                        Expect(TokenKind.Equals);
                        decl.Exports[exportName] = Expect(TokenKind.String).Text;
                    }
                    Expect(TokenKind.RBrace);
                    break;
                default:
                    throw new ParseException($"Unknown package field '{key}'", Current.Location);
            }
        }

        Expect(TokenKind.RBrace);
        return decl;
    }

    // ─── Enum parsers ──────────────────────────────────────────────────────

    private ProjectType ParseProjectType()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "executable"     => ProjectType.Executable,
            "static-library" => ProjectType.StaticLibrary,
            "shared-library" => ProjectType.SharedLibrary,
            "header-only"    => ProjectType.HeaderOnly,
            _ => throw new ParseException($"Unknown project type '{t.Text}'", t.Location),
        };
    }

    private CxxStd ParseCxxStd()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "c++17" => CxxStd.Cxx17,
            "c++20" => CxxStd.Cxx20,
            "c++23" => CxxStd.Cxx23,
            _ => throw new ParseException($"Unknown C++ standard '{t.Text}'", t.Location),
        };
    }

    private OptimizeLevel ParseOptimizeLevel()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "none"  => OptimizeLevel.None,
            "size"  => OptimizeLevel.Size,
            "speed" => OptimizeLevel.Speed,
            "full"  => OptimizeLevel.Full,
            _ => throw new ParseException($"Unknown optimize level '{t.Text}'", t.Location),
        };
    }

    private DebugInfoLevel ParseDebugInfoLevel()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "none"    => DebugInfoLevel.None,
            "minimal" => DebugInfoLevel.Minimal,
            "full"    => DebugInfoLevel.Full,
            _ => throw new ParseException($"Unknown debug-info level '{t.Text}'", t.Location),
        };
    }

    private RuntimeLink ParseRuntimeLink()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "static"  => RuntimeLink.Static,
            "dynamic" => RuntimeLink.Dynamic,
            _ => throw new ParseException($"Unknown runtime-link '{t.Text}'", t.Location),
        };
    }

    private LibcType ParseLibcType()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "glibc" => LibcType.Glibc,
            "musl"  => LibcType.Musl,
            _ => throw new ParseException($"Unknown libc type '{t.Text}'", t.Location),
        };
    }

    private StdlibType ParseStdlibType()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "libc++"    => StdlibType.LibCxx,
            "libstdc++" => StdlibType.LibStdCxx,
            _ => throw new ParseException($"Unknown stdlib type '{t.Text}'", t.Location),
        };
    }

    private WarningLevel ParseWarningLevel()
    {
        var t = Expect(TokenKind.Ident);
        return t.Text switch
        {
            "none"    => WarningLevel.None,
            "default" => WarningLevel.Default,
            "all"     => WarningLevel.All,
            "extra"   => WarningLevel.Extra,
            _ => throw new ParseException($"Unknown warning level '{t.Text}'", t.Location),
        };
    }

    private bool ParseBool()
    {
        if (Current.Kind == TokenKind.BoolTrue) { Consume(); return true; }
        if (Current.Kind == TokenKind.BoolFalse) { Consume(); return false; }
        throw new ParseException($"Expected true or false, got '{Current.Text}'", Current.Location);
    }
}
