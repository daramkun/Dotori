#include "tree_sitter/parser.h"

#if defined(__GNUC__) || defined(__clang__)
#pragma GCC diagnostic ignored "-Wmissing-field-initializers"
#endif

#ifdef _MSC_VER
#pragma optimize("", off)
#elif defined(__clang__)
#pragma clang optimize off
#elif defined(__GNUC__)
#pragma GCC optimize ("O0")
#endif

#define LANGUAGE_VERSION 14
#define STATE_COUNT 212
#define LARGE_STATE_COUNT 4
#define SYMBOL_COUNT 141
#define ALIAS_COUNT 0
#define TOKEN_COUNT 88
#define EXTERNAL_TOKEN_COUNT 0
#define FIELD_COUNT 19
#define MAX_ALIAS_SEQUENCE_LENGTH 6
#define PRODUCTION_ID_COUNT 25

enum ts_symbol_identifiers {
  sym_comment = 1,
  sym_block_comment = 2,
  sym_identifier = 3,
  sym_integer = 4,
  anon_sym_true = 5,
  anon_sym_false = 6,
  anon_sym_DQUOTE = 7,
  sym_string_content = 8,
  sym_escape_sequence = 9,
  anon_sym_DOLLAR_LBRACE = 10,
  aux_sym_env_interpolation_token1 = 11,
  anon_sym_RBRACE = 12,
  anon_sym_project = 13,
  anon_sym_LBRACE = 14,
  anon_sym_RBRACE2 = 15,
  anon_sym_LBRACK = 16,
  anon_sym_RBRACK = 17,
  anon_sym_DOT = 18,
  anon_sym_EQ = 19,
  anon_sym_type = 20,
  anon_sym_std = 21,
  anon_sym_description = 22,
  anon_sym_optimize = 23,
  anon_sym_debug_DASHinfo = 24,
  anon_sym_runtime_DASHlink = 25,
  anon_sym_libc = 26,
  anon_sym_stdlib = 27,
  anon_sym_lto = 28,
  anon_sym_warnings = 29,
  anon_sym_warnings_DASHas_DASHerrors = 30,
  anon_sym_android_DASHapi_DASHlevel = 31,
  anon_sym_macos_DASHmin = 32,
  anon_sym_ios_DASHmin = 33,
  anon_sym_tvos_DASHmin = 34,
  anon_sym_watchos_DASHmin = 35,
  anon_sym_manifest = 36,
  anon_sym_sources = 37,
  anon_sym_modules = 38,
  anon_sym_include = 39,
  anon_sym_exclude = 40,
  anon_sym_export_DASHmap = 41,
  anon_sym_headers = 42,
  anon_sym_public = 43,
  anon_sym_private = 44,
  anon_sym_defines = 45,
  anon_sym_links = 46,
  anon_sym_frameworks = 47,
  anon_sym_framework_DASHpaths = 48,
  anon_sym_compile_DASHflags = 49,
  anon_sym_link_DASHflags = 50,
  anon_sym_resources = 51,
  anon_sym_pre_DASHbuild = 52,
  anon_sym_post_DASHbuild = 53,
  anon_sym_emscripten_DASHflags = 54,
  anon_sym_copy = 55,
  anon_sym_from = 56,
  anon_sym_to = 57,
  anon_sym_unity_DASHbuild = 58,
  anon_sym_enabled = 59,
  anon_sym_batch_DASHsize = 60,
  anon_sym_pch = 61,
  anon_sym_header = 62,
  anon_sym_source = 63,
  anon_sym_output = 64,
  anon_sym_binaries = 65,
  anon_sym_libraries = 66,
  anon_sym_symbols = 67,
  anon_sym_dependencies = 68,
  anon_sym_SLASH = 69,
  anon_sym_COMMA = 70,
  anon_sym_git = 71,
  anon_sym_tag = 72,
  anon_sym_commit = 73,
  anon_sym_path = 74,
  anon_sym_version = 75,
  anon_sym_option = 76,
  anon_sym_assembler = 77,
  anon_sym_tool = 78,
  anon_sym_format = 79,
  anon_sym_flags = 80,
  anon_sym_default = 81,
  anon_sym_package = 82,
  anon_sym_name = 83,
  anon_sym_license = 84,
  anon_sym_homepage = 85,
  anon_sym_authors = 86,
  anon_sym_exports = 87,
  sym_source_file = 88,
  sym_boolean = 89,
  sym_string = 90,
  sym_env_interpolation = 91,
  sym_project_decl = 92,
  sym_project_item = 93,
  sym_condition_block = 94,
  sym_condition_expr = 95,
  sym_property_stmt = 96,
  sym__prop_keyword = 97,
  sym__prop_value = 98,
  sym_sources_block = 99,
  sym_sources_item = 100,
  sym_headers_block = 101,
  sym_header_item = 102,
  sym_string_list_block = 103,
  sym__string_list_keyword = 104,
  sym_copy_block = 105,
  sym_copy_item = 106,
  sym_unity_build_block = 107,
  sym_pch_block = 108,
  sym_output_block = 109,
  sym_dependencies_block = 110,
  sym_dep_item = 111,
  sym_dep_name = 112,
  sym_dep_value = 113,
  sym_dep_object = 114,
  sym_dep_field = 115,
  sym_assembler_block = 116,
  sym_assembler_item = 117,
  sym_option_block = 118,
  sym_option_field = 119,
  sym_package_decl = 120,
  sym_package_item = 121,
  sym_authors_block = 122,
  sym_exports_block = 123,
  sym_export_entry = 124,
  aux_sym_string_repeat1 = 125,
  aux_sym_project_decl_repeat1 = 126,
  aux_sym_condition_expr_repeat1 = 127,
  aux_sym_sources_block_repeat1 = 128,
  aux_sym_headers_block_repeat1 = 129,
  aux_sym_string_list_block_repeat1 = 130,
  aux_sym_copy_block_repeat1 = 131,
  aux_sym_unity_build_block_repeat1 = 132,
  aux_sym_pch_block_repeat1 = 133,
  aux_sym_output_block_repeat1 = 134,
  aux_sym_dependencies_block_repeat1 = 135,
  aux_sym_dep_object_repeat1 = 136,
  aux_sym_assembler_block_repeat1 = 137,
  aux_sym_option_block_repeat1 = 138,
  aux_sym_package_decl_repeat1 = 139,
  aux_sym_exports_block_repeat1 = 140,
};

static const char * const ts_symbol_names[] = {
  [ts_builtin_sym_end] = "end",
  [sym_comment] = "comment",
  [sym_block_comment] = "block_comment",
  [sym_identifier] = "identifier",
  [sym_integer] = "integer",
  [anon_sym_true] = "true",
  [anon_sym_false] = "false",
  [anon_sym_DQUOTE] = "\"",
  [sym_string_content] = "string_content",
  [sym_escape_sequence] = "escape_sequence",
  [anon_sym_DOLLAR_LBRACE] = "${",
  [aux_sym_env_interpolation_token1] = "env_interpolation_token1",
  [anon_sym_RBRACE] = "}",
  [anon_sym_project] = "project",
  [anon_sym_LBRACE] = "{",
  [anon_sym_RBRACE2] = "}",
  [anon_sym_LBRACK] = "[",
  [anon_sym_RBRACK] = "]",
  [anon_sym_DOT] = ".",
  [anon_sym_EQ] = "=",
  [anon_sym_type] = "type",
  [anon_sym_std] = "std",
  [anon_sym_description] = "description",
  [anon_sym_optimize] = "optimize",
  [anon_sym_debug_DASHinfo] = "debug-info",
  [anon_sym_runtime_DASHlink] = "runtime-link",
  [anon_sym_libc] = "libc",
  [anon_sym_stdlib] = "stdlib",
  [anon_sym_lto] = "lto",
  [anon_sym_warnings] = "warnings",
  [anon_sym_warnings_DASHas_DASHerrors] = "warnings-as-errors",
  [anon_sym_android_DASHapi_DASHlevel] = "android-api-level",
  [anon_sym_macos_DASHmin] = "macos-min",
  [anon_sym_ios_DASHmin] = "ios-min",
  [anon_sym_tvos_DASHmin] = "tvos-min",
  [anon_sym_watchos_DASHmin] = "watchos-min",
  [anon_sym_manifest] = "manifest",
  [anon_sym_sources] = "sources",
  [anon_sym_modules] = "modules",
  [anon_sym_include] = "include",
  [anon_sym_exclude] = "exclude",
  [anon_sym_export_DASHmap] = "export-map",
  [anon_sym_headers] = "headers",
  [anon_sym_public] = "public",
  [anon_sym_private] = "private",
  [anon_sym_defines] = "defines",
  [anon_sym_links] = "links",
  [anon_sym_frameworks] = "frameworks",
  [anon_sym_framework_DASHpaths] = "framework-paths",
  [anon_sym_compile_DASHflags] = "compile-flags",
  [anon_sym_link_DASHflags] = "link-flags",
  [anon_sym_resources] = "resources",
  [anon_sym_pre_DASHbuild] = "pre-build",
  [anon_sym_post_DASHbuild] = "post-build",
  [anon_sym_emscripten_DASHflags] = "emscripten-flags",
  [anon_sym_copy] = "copy",
  [anon_sym_from] = "from",
  [anon_sym_to] = "to",
  [anon_sym_unity_DASHbuild] = "unity-build",
  [anon_sym_enabled] = "enabled",
  [anon_sym_batch_DASHsize] = "batch-size",
  [anon_sym_pch] = "pch",
  [anon_sym_header] = "header",
  [anon_sym_source] = "source",
  [anon_sym_output] = "output",
  [anon_sym_binaries] = "binaries",
  [anon_sym_libraries] = "libraries",
  [anon_sym_symbols] = "symbols",
  [anon_sym_dependencies] = "dependencies",
  [anon_sym_SLASH] = "/",
  [anon_sym_COMMA] = ",",
  [anon_sym_git] = "git",
  [anon_sym_tag] = "tag",
  [anon_sym_commit] = "commit",
  [anon_sym_path] = "path",
  [anon_sym_version] = "version",
  [anon_sym_option] = "option",
  [anon_sym_assembler] = "assembler",
  [anon_sym_tool] = "tool",
  [anon_sym_format] = "format",
  [anon_sym_flags] = "flags",
  [anon_sym_default] = "default",
  [anon_sym_package] = "package",
  [anon_sym_name] = "name",
  [anon_sym_license] = "license",
  [anon_sym_homepage] = "homepage",
  [anon_sym_authors] = "authors",
  [anon_sym_exports] = "exports",
  [sym_source_file] = "source_file",
  [sym_boolean] = "boolean",
  [sym_string] = "string",
  [sym_env_interpolation] = "env_interpolation",
  [sym_project_decl] = "project_decl",
  [sym_project_item] = "project_item",
  [sym_condition_block] = "condition_block",
  [sym_condition_expr] = "condition_expr",
  [sym_property_stmt] = "property_stmt",
  [sym__prop_keyword] = "_prop_keyword",
  [sym__prop_value] = "_prop_value",
  [sym_sources_block] = "sources_block",
  [sym_sources_item] = "sources_item",
  [sym_headers_block] = "headers_block",
  [sym_header_item] = "header_item",
  [sym_string_list_block] = "string_list_block",
  [sym__string_list_keyword] = "_string_list_keyword",
  [sym_copy_block] = "copy_block",
  [sym_copy_item] = "copy_item",
  [sym_unity_build_block] = "unity_build_block",
  [sym_pch_block] = "pch_block",
  [sym_output_block] = "output_block",
  [sym_dependencies_block] = "dependencies_block",
  [sym_dep_item] = "dep_item",
  [sym_dep_name] = "dep_name",
  [sym_dep_value] = "dep_value",
  [sym_dep_object] = "dep_object",
  [sym_dep_field] = "dep_field",
  [sym_assembler_block] = "assembler_block",
  [sym_assembler_item] = "assembler_item",
  [sym_option_block] = "option_block",
  [sym_option_field] = "option_field",
  [sym_package_decl] = "package_decl",
  [sym_package_item] = "package_item",
  [sym_authors_block] = "authors_block",
  [sym_exports_block] = "exports_block",
  [sym_export_entry] = "export_entry",
  [aux_sym_string_repeat1] = "string_repeat1",
  [aux_sym_project_decl_repeat1] = "project_decl_repeat1",
  [aux_sym_condition_expr_repeat1] = "condition_expr_repeat1",
  [aux_sym_sources_block_repeat1] = "sources_block_repeat1",
  [aux_sym_headers_block_repeat1] = "headers_block_repeat1",
  [aux_sym_string_list_block_repeat1] = "string_list_block_repeat1",
  [aux_sym_copy_block_repeat1] = "copy_block_repeat1",
  [aux_sym_unity_build_block_repeat1] = "unity_build_block_repeat1",
  [aux_sym_pch_block_repeat1] = "pch_block_repeat1",
  [aux_sym_output_block_repeat1] = "output_block_repeat1",
  [aux_sym_dependencies_block_repeat1] = "dependencies_block_repeat1",
  [aux_sym_dep_object_repeat1] = "dep_object_repeat1",
  [aux_sym_assembler_block_repeat1] = "assembler_block_repeat1",
  [aux_sym_option_block_repeat1] = "option_block_repeat1",
  [aux_sym_package_decl_repeat1] = "package_decl_repeat1",
  [aux_sym_exports_block_repeat1] = "exports_block_repeat1",
};

static const TSSymbol ts_symbol_map[] = {
  [ts_builtin_sym_end] = ts_builtin_sym_end,
  [sym_comment] = sym_comment,
  [sym_block_comment] = sym_block_comment,
  [sym_identifier] = sym_identifier,
  [sym_integer] = sym_integer,
  [anon_sym_true] = anon_sym_true,
  [anon_sym_false] = anon_sym_false,
  [anon_sym_DQUOTE] = anon_sym_DQUOTE,
  [sym_string_content] = sym_string_content,
  [sym_escape_sequence] = sym_escape_sequence,
  [anon_sym_DOLLAR_LBRACE] = anon_sym_DOLLAR_LBRACE,
  [aux_sym_env_interpolation_token1] = aux_sym_env_interpolation_token1,
  [anon_sym_RBRACE] = anon_sym_RBRACE,
  [anon_sym_project] = anon_sym_project,
  [anon_sym_LBRACE] = anon_sym_LBRACE,
  [anon_sym_RBRACE2] = anon_sym_RBRACE,
  [anon_sym_LBRACK] = anon_sym_LBRACK,
  [anon_sym_RBRACK] = anon_sym_RBRACK,
  [anon_sym_DOT] = anon_sym_DOT,
  [anon_sym_EQ] = anon_sym_EQ,
  [anon_sym_type] = anon_sym_type,
  [anon_sym_std] = anon_sym_std,
  [anon_sym_description] = anon_sym_description,
  [anon_sym_optimize] = anon_sym_optimize,
  [anon_sym_debug_DASHinfo] = anon_sym_debug_DASHinfo,
  [anon_sym_runtime_DASHlink] = anon_sym_runtime_DASHlink,
  [anon_sym_libc] = anon_sym_libc,
  [anon_sym_stdlib] = anon_sym_stdlib,
  [anon_sym_lto] = anon_sym_lto,
  [anon_sym_warnings] = anon_sym_warnings,
  [anon_sym_warnings_DASHas_DASHerrors] = anon_sym_warnings_DASHas_DASHerrors,
  [anon_sym_android_DASHapi_DASHlevel] = anon_sym_android_DASHapi_DASHlevel,
  [anon_sym_macos_DASHmin] = anon_sym_macos_DASHmin,
  [anon_sym_ios_DASHmin] = anon_sym_ios_DASHmin,
  [anon_sym_tvos_DASHmin] = anon_sym_tvos_DASHmin,
  [anon_sym_watchos_DASHmin] = anon_sym_watchos_DASHmin,
  [anon_sym_manifest] = anon_sym_manifest,
  [anon_sym_sources] = anon_sym_sources,
  [anon_sym_modules] = anon_sym_modules,
  [anon_sym_include] = anon_sym_include,
  [anon_sym_exclude] = anon_sym_exclude,
  [anon_sym_export_DASHmap] = anon_sym_export_DASHmap,
  [anon_sym_headers] = anon_sym_headers,
  [anon_sym_public] = anon_sym_public,
  [anon_sym_private] = anon_sym_private,
  [anon_sym_defines] = anon_sym_defines,
  [anon_sym_links] = anon_sym_links,
  [anon_sym_frameworks] = anon_sym_frameworks,
  [anon_sym_framework_DASHpaths] = anon_sym_framework_DASHpaths,
  [anon_sym_compile_DASHflags] = anon_sym_compile_DASHflags,
  [anon_sym_link_DASHflags] = anon_sym_link_DASHflags,
  [anon_sym_resources] = anon_sym_resources,
  [anon_sym_pre_DASHbuild] = anon_sym_pre_DASHbuild,
  [anon_sym_post_DASHbuild] = anon_sym_post_DASHbuild,
  [anon_sym_emscripten_DASHflags] = anon_sym_emscripten_DASHflags,
  [anon_sym_copy] = anon_sym_copy,
  [anon_sym_from] = anon_sym_from,
  [anon_sym_to] = anon_sym_to,
  [anon_sym_unity_DASHbuild] = anon_sym_unity_DASHbuild,
  [anon_sym_enabled] = anon_sym_enabled,
  [anon_sym_batch_DASHsize] = anon_sym_batch_DASHsize,
  [anon_sym_pch] = anon_sym_pch,
  [anon_sym_header] = anon_sym_header,
  [anon_sym_source] = anon_sym_source,
  [anon_sym_output] = anon_sym_output,
  [anon_sym_binaries] = anon_sym_binaries,
  [anon_sym_libraries] = anon_sym_libraries,
  [anon_sym_symbols] = anon_sym_symbols,
  [anon_sym_dependencies] = anon_sym_dependencies,
  [anon_sym_SLASH] = anon_sym_SLASH,
  [anon_sym_COMMA] = anon_sym_COMMA,
  [anon_sym_git] = anon_sym_git,
  [anon_sym_tag] = anon_sym_tag,
  [anon_sym_commit] = anon_sym_commit,
  [anon_sym_path] = anon_sym_path,
  [anon_sym_version] = anon_sym_version,
  [anon_sym_option] = anon_sym_option,
  [anon_sym_assembler] = anon_sym_assembler,
  [anon_sym_tool] = anon_sym_tool,
  [anon_sym_format] = anon_sym_format,
  [anon_sym_flags] = anon_sym_flags,
  [anon_sym_default] = anon_sym_default,
  [anon_sym_package] = anon_sym_package,
  [anon_sym_name] = anon_sym_name,
  [anon_sym_license] = anon_sym_license,
  [anon_sym_homepage] = anon_sym_homepage,
  [anon_sym_authors] = anon_sym_authors,
  [anon_sym_exports] = anon_sym_exports,
  [sym_source_file] = sym_source_file,
  [sym_boolean] = sym_boolean,
  [sym_string] = sym_string,
  [sym_env_interpolation] = sym_env_interpolation,
  [sym_project_decl] = sym_project_decl,
  [sym_project_item] = sym_project_item,
  [sym_condition_block] = sym_condition_block,
  [sym_condition_expr] = sym_condition_expr,
  [sym_property_stmt] = sym_property_stmt,
  [sym__prop_keyword] = sym__prop_keyword,
  [sym__prop_value] = sym__prop_value,
  [sym_sources_block] = sym_sources_block,
  [sym_sources_item] = sym_sources_item,
  [sym_headers_block] = sym_headers_block,
  [sym_header_item] = sym_header_item,
  [sym_string_list_block] = sym_string_list_block,
  [sym__string_list_keyword] = sym__string_list_keyword,
  [sym_copy_block] = sym_copy_block,
  [sym_copy_item] = sym_copy_item,
  [sym_unity_build_block] = sym_unity_build_block,
  [sym_pch_block] = sym_pch_block,
  [sym_output_block] = sym_output_block,
  [sym_dependencies_block] = sym_dependencies_block,
  [sym_dep_item] = sym_dep_item,
  [sym_dep_name] = sym_dep_name,
  [sym_dep_value] = sym_dep_value,
  [sym_dep_object] = sym_dep_object,
  [sym_dep_field] = sym_dep_field,
  [sym_assembler_block] = sym_assembler_block,
  [sym_assembler_item] = sym_assembler_item,
  [sym_option_block] = sym_option_block,
  [sym_option_field] = sym_option_field,
  [sym_package_decl] = sym_package_decl,
  [sym_package_item] = sym_package_item,
  [sym_authors_block] = sym_authors_block,
  [sym_exports_block] = sym_exports_block,
  [sym_export_entry] = sym_export_entry,
  [aux_sym_string_repeat1] = aux_sym_string_repeat1,
  [aux_sym_project_decl_repeat1] = aux_sym_project_decl_repeat1,
  [aux_sym_condition_expr_repeat1] = aux_sym_condition_expr_repeat1,
  [aux_sym_sources_block_repeat1] = aux_sym_sources_block_repeat1,
  [aux_sym_headers_block_repeat1] = aux_sym_headers_block_repeat1,
  [aux_sym_string_list_block_repeat1] = aux_sym_string_list_block_repeat1,
  [aux_sym_copy_block_repeat1] = aux_sym_copy_block_repeat1,
  [aux_sym_unity_build_block_repeat1] = aux_sym_unity_build_block_repeat1,
  [aux_sym_pch_block_repeat1] = aux_sym_pch_block_repeat1,
  [aux_sym_output_block_repeat1] = aux_sym_output_block_repeat1,
  [aux_sym_dependencies_block_repeat1] = aux_sym_dependencies_block_repeat1,
  [aux_sym_dep_object_repeat1] = aux_sym_dep_object_repeat1,
  [aux_sym_assembler_block_repeat1] = aux_sym_assembler_block_repeat1,
  [aux_sym_option_block_repeat1] = aux_sym_option_block_repeat1,
  [aux_sym_package_decl_repeat1] = aux_sym_package_decl_repeat1,
  [aux_sym_exports_block_repeat1] = aux_sym_exports_block_repeat1,
};

static const TSSymbolMetadata ts_symbol_metadata[] = {
  [ts_builtin_sym_end] = {
    .visible = false,
    .named = true,
  },
  [sym_comment] = {
    .visible = true,
    .named = true,
  },
  [sym_block_comment] = {
    .visible = true,
    .named = true,
  },
  [sym_identifier] = {
    .visible = true,
    .named = true,
  },
  [sym_integer] = {
    .visible = true,
    .named = true,
  },
  [anon_sym_true] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_false] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_DQUOTE] = {
    .visible = true,
    .named = false,
  },
  [sym_string_content] = {
    .visible = true,
    .named = true,
  },
  [sym_escape_sequence] = {
    .visible = true,
    .named = true,
  },
  [anon_sym_DOLLAR_LBRACE] = {
    .visible = true,
    .named = false,
  },
  [aux_sym_env_interpolation_token1] = {
    .visible = false,
    .named = false,
  },
  [anon_sym_RBRACE] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_project] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_LBRACE] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_RBRACE2] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_LBRACK] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_RBRACK] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_DOT] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_EQ] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_type] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_std] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_description] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_optimize] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_debug_DASHinfo] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_runtime_DASHlink] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_libc] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_stdlib] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_lto] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_warnings] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_warnings_DASHas_DASHerrors] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_android_DASHapi_DASHlevel] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_macos_DASHmin] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_ios_DASHmin] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_tvos_DASHmin] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_watchos_DASHmin] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_manifest] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_sources] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_modules] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_include] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_exclude] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_export_DASHmap] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_headers] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_public] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_private] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_defines] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_links] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_frameworks] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_framework_DASHpaths] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_compile_DASHflags] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_link_DASHflags] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_resources] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_pre_DASHbuild] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_post_DASHbuild] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_emscripten_DASHflags] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_copy] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_from] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_to] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_unity_DASHbuild] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_enabled] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_batch_DASHsize] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_pch] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_header] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_source] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_output] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_binaries] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_libraries] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_symbols] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_dependencies] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_SLASH] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_COMMA] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_git] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_tag] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_commit] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_path] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_version] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_option] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_assembler] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_tool] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_format] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_flags] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_default] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_package] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_name] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_license] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_homepage] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_authors] = {
    .visible = true,
    .named = false,
  },
  [anon_sym_exports] = {
    .visible = true,
    .named = false,
  },
  [sym_source_file] = {
    .visible = true,
    .named = true,
  },
  [sym_boolean] = {
    .visible = true,
    .named = true,
  },
  [sym_string] = {
    .visible = true,
    .named = true,
  },
  [sym_env_interpolation] = {
    .visible = true,
    .named = true,
  },
  [sym_project_decl] = {
    .visible = true,
    .named = true,
  },
  [sym_project_item] = {
    .visible = true,
    .named = true,
  },
  [sym_condition_block] = {
    .visible = true,
    .named = true,
  },
  [sym_condition_expr] = {
    .visible = true,
    .named = true,
  },
  [sym_property_stmt] = {
    .visible = true,
    .named = true,
  },
  [sym__prop_keyword] = {
    .visible = false,
    .named = true,
  },
  [sym__prop_value] = {
    .visible = false,
    .named = true,
  },
  [sym_sources_block] = {
    .visible = true,
    .named = true,
  },
  [sym_sources_item] = {
    .visible = true,
    .named = true,
  },
  [sym_headers_block] = {
    .visible = true,
    .named = true,
  },
  [sym_header_item] = {
    .visible = true,
    .named = true,
  },
  [sym_string_list_block] = {
    .visible = true,
    .named = true,
  },
  [sym__string_list_keyword] = {
    .visible = false,
    .named = true,
  },
  [sym_copy_block] = {
    .visible = true,
    .named = true,
  },
  [sym_copy_item] = {
    .visible = true,
    .named = true,
  },
  [sym_unity_build_block] = {
    .visible = true,
    .named = true,
  },
  [sym_pch_block] = {
    .visible = true,
    .named = true,
  },
  [sym_output_block] = {
    .visible = true,
    .named = true,
  },
  [sym_dependencies_block] = {
    .visible = true,
    .named = true,
  },
  [sym_dep_item] = {
    .visible = true,
    .named = true,
  },
  [sym_dep_name] = {
    .visible = true,
    .named = true,
  },
  [sym_dep_value] = {
    .visible = true,
    .named = true,
  },
  [sym_dep_object] = {
    .visible = true,
    .named = true,
  },
  [sym_dep_field] = {
    .visible = true,
    .named = true,
  },
  [sym_assembler_block] = {
    .visible = true,
    .named = true,
  },
  [sym_assembler_item] = {
    .visible = true,
    .named = true,
  },
  [sym_option_block] = {
    .visible = true,
    .named = true,
  },
  [sym_option_field] = {
    .visible = true,
    .named = true,
  },
  [sym_package_decl] = {
    .visible = true,
    .named = true,
  },
  [sym_package_item] = {
    .visible = true,
    .named = true,
  },
  [sym_authors_block] = {
    .visible = true,
    .named = true,
  },
  [sym_exports_block] = {
    .visible = true,
    .named = true,
  },
  [sym_export_entry] = {
    .visible = true,
    .named = true,
  },
  [aux_sym_string_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_project_decl_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_condition_expr_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_sources_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_headers_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_string_list_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_copy_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_unity_build_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_pch_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_output_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_dependencies_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_dep_object_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_assembler_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_option_block_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_package_decl_repeat1] = {
    .visible = false,
    .named = false,
  },
  [aux_sym_exports_block_repeat1] = {
    .visible = false,
    .named = false,
  },
};

enum ts_field_identifiers {
  field_batch_size = 1,
  field_binaries = 2,
  field_condition = 3,
  field_enabled = 4,
  field_from = 5,
  field_header = 6,
  field_key = 7,
  field_kind = 8,
  field_libraries = 9,
  field_modules = 10,
  field_name = 11,
  field_path = 12,
  field_pattern = 13,
  field_source = 14,
  field_symbols = 15,
  field_to = 16,
  field_value = 17,
  field_var_name = 18,
  field_visibility = 19,
};

static const char * const ts_field_names[] = {
  [0] = NULL,
  [field_batch_size] = "batch_size",
  [field_binaries] = "binaries",
  [field_condition] = "condition",
  [field_enabled] = "enabled",
  [field_from] = "from",
  [field_header] = "header",
  [field_key] = "key",
  [field_kind] = "kind",
  [field_libraries] = "libraries",
  [field_modules] = "modules",
  [field_name] = "name",
  [field_path] = "path",
  [field_pattern] = "pattern",
  [field_source] = "source",
  [field_symbols] = "symbols",
  [field_to] = "to",
  [field_value] = "value",
  [field_var_name] = "var_name",
  [field_visibility] = "visibility",
};

static const TSFieldMapSlice ts_field_map_slices[PRODUCTION_ID_COUNT] = {
  [1] = {.index = 0, .length = 1},
  [2] = {.index = 1, .length = 1},
  [3] = {.index = 2, .length = 1},
  [4] = {.index = 3, .length = 2},
  [5] = {.index = 5, .length = 1},
  [6] = {.index = 6, .length = 2},
  [7] = {.index = 8, .length = 2},
  [8] = {.index = 10, .length = 4},
  [9] = {.index = 14, .length = 3},
  [10] = {.index = 17, .length = 6},
  [11] = {.index = 23, .length = 3},
  [12] = {.index = 26, .length = 6},
  [13] = {.index = 32, .length = 1},
  [14] = {.index = 33, .length = 1},
  [15] = {.index = 34, .length = 1},
  [16] = {.index = 35, .length = 1},
  [17] = {.index = 36, .length = 1},
  [18] = {.index = 37, .length = 1},
  [19] = {.index = 38, .length = 1},
  [20] = {.index = 39, .length = 1},
  [21] = {.index = 40, .length = 1},
  [22] = {.index = 41, .length = 2},
  [23] = {.index = 43, .length = 1},
  [24] = {.index = 44, .length = 2},
};

static const TSFieldMapEntry ts_field_map_entries[] = {
  [0] =
    {field_name, 1},
  [1] =
    {field_value, 2},
  [2] =
    {field_kind, 0},
  [3] =
    {field_key, 0},
    {field_value, 2},
  [5] =
    {field_pattern, 1},
  [6] =
    {field_path, 1},
    {field_visibility, 0},
  [8] =
    {field_batch_size, 2, .inherited = true},
    {field_enabled, 2, .inherited = true},
  [10] =
    {field_batch_size, 0, .inherited = true},
    {field_batch_size, 1, .inherited = true},
    {field_enabled, 0, .inherited = true},
    {field_enabled, 1, .inherited = true},
  [14] =
    {field_header, 2, .inherited = true},
    {field_modules, 2, .inherited = true},
    {field_source, 2, .inherited = true},
  [17] =
    {field_header, 0, .inherited = true},
    {field_header, 1, .inherited = true},
    {field_modules, 0, .inherited = true},
    {field_modules, 1, .inherited = true},
    {field_source, 0, .inherited = true},
    {field_source, 1, .inherited = true},
  [23] =
    {field_binaries, 2, .inherited = true},
    {field_libraries, 2, .inherited = true},
    {field_symbols, 2, .inherited = true},
  [26] =
    {field_binaries, 0, .inherited = true},
    {field_binaries, 1, .inherited = true},
    {field_libraries, 0, .inherited = true},
    {field_libraries, 1, .inherited = true},
    {field_symbols, 0, .inherited = true},
    {field_symbols, 1, .inherited = true},
  [32] =
    {field_condition, 1},
  [33] =
    {field_enabled, 2},
  [34] =
    {field_batch_size, 2},
  [35] =
    {field_modules, 2},
  [36] =
    {field_header, 2},
  [37] =
    {field_source, 2},
  [38] =
    {field_binaries, 2},
  [39] =
    {field_libraries, 2},
  [40] =
    {field_symbols, 2},
  [41] =
    {field_name, 0},
    {field_value, 2},
  [43] =
    {field_var_name, 1},
  [44] =
    {field_from, 1},
    {field_to, 3},
};

static const TSSymbol ts_alias_sequences[PRODUCTION_ID_COUNT][MAX_ALIAS_SEQUENCE_LENGTH] = {
  [0] = {0},
};

static const uint16_t ts_non_terminal_alias_map[] = {
  0,
};

static const TSStateId ts_primary_state_ids[STATE_COUNT] = {
  [0] = 0,
  [1] = 1,
  [2] = 2,
  [3] = 3,
  [4] = 4,
  [5] = 5,
  [6] = 6,
  [7] = 7,
  [8] = 8,
  [9] = 9,
  [10] = 10,
  [11] = 11,
  [12] = 12,
  [13] = 13,
  [14] = 14,
  [15] = 15,
  [16] = 16,
  [17] = 17,
  [18] = 18,
  [19] = 19,
  [20] = 20,
  [21] = 21,
  [22] = 22,
  [23] = 23,
  [24] = 24,
  [25] = 25,
  [26] = 26,
  [27] = 27,
  [28] = 28,
  [29] = 29,
  [30] = 30,
  [31] = 31,
  [32] = 32,
  [33] = 33,
  [34] = 34,
  [35] = 35,
  [36] = 36,
  [37] = 37,
  [38] = 38,
  [39] = 39,
  [40] = 40,
  [41] = 41,
  [42] = 42,
  [43] = 43,
  [44] = 44,
  [45] = 45,
  [46] = 46,
  [47] = 47,
  [48] = 48,
  [49] = 49,
  [50] = 50,
  [51] = 51,
  [52] = 52,
  [53] = 53,
  [54] = 54,
  [55] = 55,
  [56] = 56,
  [57] = 57,
  [58] = 58,
  [59] = 59,
  [60] = 60,
  [61] = 61,
  [62] = 62,
  [63] = 63,
  [64] = 64,
  [65] = 65,
  [66] = 63,
  [67] = 67,
  [68] = 68,
  [69] = 69,
  [70] = 69,
  [71] = 71,
  [72] = 72,
  [73] = 73,
  [74] = 74,
  [75] = 75,
  [76] = 76,
  [77] = 77,
  [78] = 78,
  [79] = 79,
  [80] = 80,
  [81] = 81,
  [82] = 82,
  [83] = 83,
  [84] = 84,
  [85] = 85,
  [86] = 86,
  [87] = 87,
  [88] = 88,
  [89] = 89,
  [90] = 90,
  [91] = 91,
  [92] = 92,
  [93] = 93,
  [94] = 94,
  [95] = 95,
  [96] = 96,
  [97] = 97,
  [98] = 98,
  [99] = 99,
  [100] = 100,
  [101] = 101,
  [102] = 102,
  [103] = 103,
  [104] = 104,
  [105] = 105,
  [106] = 106,
  [107] = 107,
  [108] = 108,
  [109] = 109,
  [110] = 110,
  [111] = 111,
  [112] = 112,
  [113] = 113,
  [114] = 114,
  [115] = 115,
  [116] = 116,
  [117] = 117,
  [118] = 118,
  [119] = 119,
  [120] = 120,
  [121] = 121,
  [122] = 122,
  [123] = 123,
  [124] = 124,
  [125] = 125,
  [126] = 126,
  [127] = 127,
  [128] = 128,
  [129] = 129,
  [130] = 130,
  [131] = 131,
  [132] = 132,
  [133] = 133,
  [134] = 134,
  [135] = 135,
  [136] = 136,
  [137] = 137,
  [138] = 138,
  [139] = 139,
  [140] = 140,
  [141] = 141,
  [142] = 142,
  [143] = 143,
  [144] = 144,
  [145] = 145,
  [146] = 146,
  [147] = 147,
  [148] = 148,
  [149] = 149,
  [150] = 150,
  [151] = 151,
  [152] = 152,
  [153] = 153,
  [154] = 154,
  [155] = 155,
  [156] = 156,
  [157] = 157,
  [158] = 2,
  [159] = 3,
  [160] = 160,
  [161] = 161,
  [162] = 162,
  [163] = 163,
  [164] = 164,
  [165] = 165,
  [166] = 166,
  [167] = 167,
  [168] = 168,
  [169] = 169,
  [170] = 170,
  [171] = 171,
  [172] = 172,
  [173] = 173,
  [174] = 174,
  [175] = 175,
  [176] = 176,
  [177] = 177,
  [178] = 178,
  [179] = 179,
  [180] = 180,
  [181] = 181,
  [182] = 182,
  [183] = 183,
  [184] = 184,
  [185] = 185,
  [186] = 186,
  [187] = 187,
  [188] = 188,
  [189] = 189,
  [190] = 190,
  [191] = 191,
  [192] = 192,
  [193] = 193,
  [194] = 194,
  [195] = 195,
  [196] = 196,
  [197] = 197,
  [198] = 198,
  [199] = 199,
  [200] = 200,
  [201] = 201,
  [202] = 202,
  [203] = 203,
  [204] = 204,
  [205] = 205,
  [206] = 206,
  [207] = 207,
  [208] = 208,
  [209] = 209,
  [210] = 210,
  [211] = 211,
};

static bool ts_lex(TSLexer *lexer, TSStateId state) {
  START_LEXER();
  eof = lexer->eof(lexer);
  switch (state) {
    case 0:
      if (eof) ADVANCE(401);
      ADVANCE_MAP(
        '"', 418,
        '#', 402,
        '$', 398,
        '(', 9,
        ',', 498,
        '.', 443,
        '/', 497,
        '=', 444,
        '[', 441,
        '\\', 399,
        ']', 442,
        'a', 251,
        'b', 35,
        'c', 272,
        'd', 103,
        'e', 232,
        'f', 30,
        'g', 177,
        'h', 125,
        'i', 252,
        'l', 172,
        'm', 31,
        'n', 46,
        'o', 297,
        'p', 32,
        'r', 126,
        's', 273,
        't', 33,
        'u', 268,
        'v', 120,
        'w', 34,
        '{', 439,
        '}', 440,
      );
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(400);
      if (('0' <= lookahead && lookahead <= '9')) ADVANCE(413);
      END_STATE();
    case 1:
      if (lookahead == '"') ADVANCE(418);
      if (lookahead == '#') ADVANCE(402);
      if (lookahead == '(') ADVANCE(9);
      if (lookahead == '/') ADVANCE(29);
      if (lookahead == 'f') ADVANCE(405);
      if (lookahead == 't') ADVANCE(409);
      if (lookahead == '}') ADVANCE(437);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(2);
      if (('0' <= lookahead && lookahead <= '9')) ADVANCE(413);
      if (('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 2:
      if (lookahead == '"') ADVANCE(418);
      if (lookahead == '#') ADVANCE(402);
      if (lookahead == '(') ADVANCE(9);
      if (lookahead == '/') ADVANCE(29);
      if (lookahead == 'f') ADVANCE(405);
      if (lookahead == 't') ADVANCE(409);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(2);
      if (('0' <= lookahead && lookahead <= '9')) ADVANCE(413);
      if (('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 3:
      if (lookahead == '"') ADVANCE(418);
      if (lookahead == '#') ADVANCE(419);
      if (lookahead == '$') ADVANCE(398);
      if (lookahead == '(') ADVANCE(423);
      if (lookahead == '/') ADVANCE(425);
      if (lookahead == '\\') ADVANCE(399);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') ADVANCE(420);
      if (lookahead != 0) ADVANCE(426);
      END_STATE();
    case 4:
      ADVANCE_MAP(
        '#', 402,
        '(', 9,
        '/', 29,
        '[', 441,
        'a', 250,
        'c', 278,
        'd', 119,
        'e', 233,
        'f', 212,
        'h', 149,
        'i', 252,
        'l', 176,
        'm', 31,
        'o', 297,
        'p', 70,
        'r', 126,
        's', 291,
        't', 284,
        'u', 268,
        'w', 34,
        '}', 440,
      );
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(4);
      END_STATE();
    case 5:
      ADVANCE_MAP(
        '#', 402,
        '(', 9,
        '/', 29,
        'h', 151,
        'm', 277,
        's', 293,
        't', 276,
        '}', 440,
      );
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(5);
      END_STATE();
    case 6:
      if (lookahead == '#') ADVANCE(402);
      if (lookahead == '(') ADVANCE(9);
      if (lookahead == '/') ADVANCE(29);
      if (lookahead == '}') ADVANCE(440);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(6);
      if (('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 7:
      if (lookahead == ')') ADVANCE(403);
      if (lookahead == '*') ADVANCE(8);
      if (lookahead != 0) ADVANCE(10);
      END_STATE();
    case 8:
      if (lookahead == ')') ADVANCE(404);
      if (lookahead == '*') ADVANCE(7);
      if (lookahead != 0) ADVANCE(10);
      END_STATE();
    case 9:
      if (lookahead == '*') ADVANCE(10);
      END_STATE();
    case 10:
      if (lookahead == '*') ADVANCE(7);
      if (lookahead != 0) ADVANCE(10);
      END_STATE();
    case 11:
      if (lookahead == '-') ADVANCE(152);
      if (lookahead == 's') ADVANCE(471);
      END_STATE();
    case 12:
      if (lookahead == '-') ADVANCE(64);
      END_STATE();
    case 13:
      if (lookahead == '-') ADVANCE(244);
      END_STATE();
    case 14:
      if (lookahead == '-') ADVANCE(40);
      END_STATE();
    case 15:
      if (lookahead == '-') ADVANCE(227);
      END_STATE();
    case 16:
      if (lookahead == '-') ADVANCE(241);
      if (lookahead == 's') ADVANCE(515);
      END_STATE();
    case 17:
      if (lookahead == '-') ADVANCE(300);
      if (lookahead == 's') ADVANCE(472);
      END_STATE();
    case 18:
      if (lookahead == '-') ADVANCE(353);
      END_STATE();
    case 19:
      if (lookahead == '-') ADVANCE(191);
      END_STATE();
    case 20:
      if (lookahead == '-') ADVANCE(138);
      END_STATE();
    case 21:
      if (lookahead == '-') ADVANCE(223);
      END_STATE();
    case 22:
      if (lookahead == '-') ADVANCE(247);
      END_STATE();
    case 23:
      if (lookahead == '-') ADVANCE(248);
      END_STATE();
    case 24:
      if (lookahead == '-') ADVANCE(249);
      END_STATE();
    case 25:
      if (lookahead == '-') ADVANCE(68);
      END_STATE();
    case 26:
      if (lookahead == '-') ADVANCE(69);
      END_STATE();
    case 27:
      if (lookahead == '-') ADVANCE(155);
      END_STATE();
    case 28:
      if (lookahead == '-') ADVANCE(156);
      END_STATE();
    case 29:
      if (lookahead == '/') ADVANCE(402);
      END_STATE();
    case 30:
      if (lookahead == 'a') ADVANCE(224);
      if (lookahead == 'l') ADVANCE(36);
      if (lookahead == 'o') ADVANCE(308);
      if (lookahead == 'r') ADVANCE(52);
      END_STATE();
    case 31:
      if (lookahead == 'a') ADVANCE(88);
      if (lookahead == 'o') ADVANCE(95);
      END_STATE();
    case 32:
      if (lookahead == 'a') ADVANCE(76);
      if (lookahead == 'c') ADVANCE(166);
      if (lookahead == 'o') ADVANCE(347);
      if (lookahead == 'r') ADVANCE(122);
      if (lookahead == 'u') ADVANCE(60);
      END_STATE();
    case 33:
      if (lookahead == 'a') ADVANCE(157);
      if (lookahead == 'o') ADVANCE(483);
      if (lookahead == 'r') ADVANCE(381);
      if (lookahead == 'v') ADVANCE(290);
      if (lookahead == 'y') ADVANCE(299);
      END_STATE();
    case 34:
      if (lookahead == 'a') ADVANCE(309);
      END_STATE();
    case 35:
      if (lookahead == 'a') ADVANCE(365);
      if (lookahead == 'i') ADVANCE(263);
      END_STATE();
    case 36:
      if (lookahead == 'a') ADVANCE(159);
      END_STATE();
    case 37:
      if (lookahead == 'a') ADVANCE(99);
      END_STATE();
    case 38:
      if (lookahead == 'a') ADVANCE(164);
      END_STATE();
    case 39:
      if (lookahead == 'a') ADVANCE(315);
      END_STATE();
    case 40:
      if (lookahead == 'a') ADVANCE(301);
      END_STATE();
    case 41:
      if (lookahead == 'a') ADVANCE(295);
      END_STATE();
    case 42:
      if (lookahead == 'a') ADVANCE(360);
      END_STATE();
    case 43:
      if (lookahead == 'a') ADVANCE(373);
      END_STATE();
    case 44:
      if (lookahead == 'a') ADVANCE(350);
      END_STATE();
    case 45:
      if (lookahead == 'a') ADVANCE(371);
      END_STATE();
    case 46:
      if (lookahead == 'a') ADVANCE(237);
      END_STATE();
    case 47:
      if (lookahead == 'a') ADVANCE(63);
      END_STATE();
    case 48:
      if (lookahead == 'a') ADVANCE(102);
      END_STATE();
    case 49:
      if (lookahead == 'a') ADVANCE(380);
      if (lookahead == 'i') ADVANCE(267);
      END_STATE();
    case 50:
      if (lookahead == 'a') ADVANCE(165);
      END_STATE();
    case 51:
      if (lookahead == 'a') ADVANCE(240);
      END_STATE();
    case 52:
      if (lookahead == 'a') ADVANCE(240);
      if (lookahead == 'o') ADVANCE(231);
      END_STATE();
    case 53:
      if (lookahead == 'a') ADVANCE(161);
      END_STATE();
    case 54:
      if (lookahead == 'a') ADVANCE(162);
      END_STATE();
    case 55:
      if (lookahead == 'a') ADVANCE(163);
      END_STATE();
    case 56:
      if (lookahead == 'a') ADVANCE(101);
      END_STATE();
    case 57:
      if (lookahead == 'a') ADVANCE(322);
      END_STATE();
    case 58:
      if (lookahead == 'b') ADVANCE(72);
      if (lookahead == 'c') ADVANCE(131);
      if (lookahead == 'n') ADVANCE(206);
      END_STATE();
    case 59:
      if (lookahead == 'b') ADVANCE(452);
      END_STATE();
    case 60:
      if (lookahead == 'b') ADVANCE(213);
      END_STATE();
    case 61:
      if (lookahead == 'b') ADVANCE(377);
      if (lookahead == 'f') ADVANCE(49);
      if (lookahead == 'p') ADVANCE(129);
      if (lookahead == 's') ADVANCE(84);
      END_STATE();
    case 62:
      if (lookahead == 'b') ADVANCE(377);
      if (lookahead == 'f') ADVANCE(180);
      if (lookahead == 'p') ADVANCE(129);
      if (lookahead == 's') ADVANCE(84);
      END_STATE();
    case 63:
      if (lookahead == 'b') ADVANCE(219);
      END_STATE();
    case 64:
      if (lookahead == 'b') ADVANCE(383);
      END_STATE();
    case 65:
      if (lookahead == 'b') ADVANCE(281);
      END_STATE();
    case 66:
      if (lookahead == 'b') ADVANCE(226);
      END_STATE();
    case 67:
      if (lookahead == 'b') ADVANCE(71);
      if (lookahead == 'n') ADVANCE(206);
      END_STATE();
    case 68:
      if (lookahead == 'b') ADVANCE(386);
      END_STATE();
    case 69:
      if (lookahead == 'b') ADVANCE(387);
      END_STATE();
    case 70:
      if (lookahead == 'c') ADVANCE(166);
      if (lookahead == 'o') ADVANCE(347);
      if (lookahead == 'r') ADVANCE(121);
      END_STATE();
    case 71:
      if (lookahead == 'c') ADVANCE(451);
      END_STATE();
    case 72:
      if (lookahead == 'c') ADVANCE(451);
      if (lookahead == 'r') ADVANCE(57);
      END_STATE();
    case 73:
      if (lookahead == 'c') ADVANCE(468);
      END_STATE();
    case 74:
      if (lookahead == 'c') ADVANCE(214);
      END_STATE();
    case 75:
      if (lookahead == 'c') ADVANCE(214);
      if (lookahead == 'p') ADVANCE(280);
      END_STATE();
    case 76:
      if (lookahead == 'c') ADVANCE(209);
      if (lookahead == 't') ADVANCE(167);
      END_STATE();
    case 77:
      if (lookahead == 'c') ADVANCE(168);
      END_STATE();
    case 78:
      if (lookahead == 'c') ADVANCE(363);
      END_STATE();
    case 79:
      if (lookahead == 'c') ADVANCE(109);
      END_STATE();
    case 80:
      if (lookahead == 'c') ADVANCE(144);
      END_STATE();
    case 81:
      if (lookahead == 'c') ADVANCE(147);
      END_STATE();
    case 82:
      if (lookahead == 'c') ADVANCE(118);
      END_STATE();
    case 83:
      if (lookahead == 'c') ADVANCE(171);
      END_STATE();
    case 84:
      if (lookahead == 'c') ADVANCE(316);
      END_STATE();
    case 85:
      if (lookahead == 'c') ADVANCE(323);
      END_STATE();
    case 86:
      if (lookahead == 'c') ADVANCE(199);
      END_STATE();
    case 87:
      if (lookahead == 'c') ADVANCE(228);
      END_STATE();
    case 88:
      if (lookahead == 'c') ADVANCE(292);
      if (lookahead == 'n') ADVANCE(173);
      END_STATE();
    case 89:
      if (lookahead == 'd') ADVANCE(446);
      END_STATE();
    case 90:
      if (lookahead == 'd') ADVANCE(485);
      END_STATE();
    case 91:
      if (lookahead == 'd') ADVANCE(477);
      END_STATE();
    case 92:
      if (lookahead == 'd') ADVANCE(478);
      END_STATE();
    case 93:
      if (lookahead == 'd') ADVANCE(484);
      END_STATE();
    case 94:
      if (lookahead == 'd') ADVANCE(311);
      END_STATE();
    case 95:
      if (lookahead == 'd') ADVANCE(384);
      END_STATE();
    case 96:
      if (lookahead == 'd') ADVANCE(14);
      END_STATE();
    case 97:
      if (lookahead == 'd') ADVANCE(110);
      END_STATE();
    case 98:
      if (lookahead == 'd') ADVANCE(111);
      END_STATE();
    case 99:
      if (lookahead == 'd') ADVANCE(132);
      END_STATE();
    case 100:
      if (lookahead == 'd') ADVANCE(141);
      END_STATE();
    case 101:
      if (lookahead == 'd') ADVANCE(139);
      END_STATE();
    case 102:
      if (lookahead == 'd') ADVANCE(150);
      END_STATE();
    case 103:
      if (lookahead == 'e') ADVANCE(61);
      END_STATE();
    case 104:
      if (lookahead == 'e') ADVANCE(511);
      END_STATE();
    case 105:
      if (lookahead == 'e') ADVANCE(414);
      END_STATE();
    case 106:
      if (lookahead == 'e') ADVANCE(445);
      END_STATE();
    case 107:
      if (lookahead == 'e') ADVANCE(416);
      END_STATE();
    case 108:
      if (lookahead == 'e') ADVANCE(393);
      END_STATE();
    case 109:
      if (lookahead == 'e') ADVANCE(491);
      END_STATE();
    case 110:
      if (lookahead == 'e') ADVANCE(465);
      END_STATE();
    case 111:
      if (lookahead == 'e') ADVANCE(464);
      END_STATE();
    case 112:
      if (lookahead == 'e') ADVANCE(512);
      END_STATE();
    case 113:
      if (lookahead == 'e') ADVANCE(510);
      END_STATE();
    case 114:
      if (lookahead == 'e') ADVANCE(469);
      END_STATE();
    case 115:
      if (lookahead == 'e') ADVANCE(513);
      END_STATE();
    case 116:
      if (lookahead == 'e') ADVANCE(448);
      END_STATE();
    case 117:
      if (lookahead == 'e') ADVANCE(486);
      END_STATE();
    case 118:
      if (lookahead == 'e') ADVANCE(490);
      END_STATE();
    case 119:
      if (lookahead == 'e') ADVANCE(62);
      END_STATE();
    case 120:
      if (lookahead == 'e') ADVANCE(312);
      END_STATE();
    case 121:
      if (lookahead == 'e') ADVANCE(12);
      END_STATE();
    case 122:
      if (lookahead == 'e') ADVANCE(12);
      if (lookahead == 'i') ADVANCE(392);
      if (lookahead == 'o') ADVANCE(205);
      END_STATE();
    case 123:
      if (lookahead == 'e') ADVANCE(391);
      END_STATE();
    case 124:
      if (lookahead == 'e') ADVANCE(304);
      END_STATE();
    case 125:
      if (lookahead == 'e') ADVANCE(37);
      if (lookahead == 'o') ADVANCE(236);
      END_STATE();
    case 126:
      if (lookahead == 'e') ADVANCE(346);
      if (lookahead == 'u') ADVANCE(264);
      END_STATE();
    case 127:
      if (lookahead == 'e') ADVANCE(246);
      END_STATE();
    case 128:
      if (lookahead == 'e') ADVANCE(78);
      END_STATE();
    case 129:
      if (lookahead == 'e') ADVANCE(262);
      END_STATE();
    case 130:
      if (lookahead == 'e') ADVANCE(27);
      END_STATE();
    case 131:
      if (lookahead == 'e') ADVANCE(270);
      END_STATE();
    case 132:
      if (lookahead == 'e') ADVANCE(305);
      END_STATE();
    case 133:
      if (lookahead == 'e') ADVANCE(15);
      END_STATE();
    case 134:
      if (lookahead == 'e') ADVANCE(90);
      END_STATE();
    case 135:
      if (lookahead == 'e') ADVANCE(330);
      END_STATE();
    case 136:
      if (lookahead == 'e') ADVANCE(306);
      END_STATE();
    case 137:
      if (lookahead == 'e') ADVANCE(332);
      END_STATE();
    case 138:
      if (lookahead == 'e') ADVANCE(320);
      END_STATE();
    case 139:
      if (lookahead == 'e') ADVANCE(307);
      END_STATE();
    case 140:
      if (lookahead == 'e') ADVANCE(335);
      END_STATE();
    case 141:
      if (lookahead == 'e') ADVANCE(265);
      END_STATE();
    case 142:
      if (lookahead == 'e') ADVANCE(211);
      END_STATE();
    case 143:
      if (lookahead == 'e') ADVANCE(337);
      END_STATE();
    case 144:
      if (lookahead == 'e') ADVANCE(338);
      END_STATE();
    case 145:
      if (lookahead == 'e') ADVANCE(271);
      END_STATE();
    case 146:
      if (lookahead == 'e') ADVANCE(340);
      END_STATE();
    case 147:
      if (lookahead == 'e') ADVANCE(333);
      END_STATE();
    case 148:
      if (lookahead == 'e') ADVANCE(351);
      END_STATE();
    case 149:
      if (lookahead == 'e') ADVANCE(48);
      END_STATE();
    case 150:
      if (lookahead == 'e') ADVANCE(318);
      END_STATE();
    case 151:
      if (lookahead == 'e') ADVANCE(56);
      END_STATE();
    case 152:
      if (lookahead == 'f') ADVANCE(225);
      END_STATE();
    case 153:
      if (lookahead == 'f') ADVANCE(275);
      END_STATE();
    case 154:
      if (lookahead == 'f') ADVANCE(148);
      END_STATE();
    case 155:
      if (lookahead == 'f') ADVANCE(229);
      END_STATE();
    case 156:
      if (lookahead == 'f') ADVANCE(230);
      END_STATE();
    case 157:
      if (lookahead == 'g') ADVANCE(500);
      END_STATE();
    case 158:
      if (lookahead == 'g') ADVANCE(19);
      END_STATE();
    case 159:
      if (lookahead == 'g') ADVANCE(328);
      END_STATE();
    case 160:
      if (lookahead == 'g') ADVANCE(336);
      END_STATE();
    case 161:
      if (lookahead == 'g') ADVANCE(339);
      END_STATE();
    case 162:
      if (lookahead == 'g') ADVANCE(341);
      END_STATE();
    case 163:
      if (lookahead == 'g') ADVANCE(343);
      END_STATE();
    case 164:
      if (lookahead == 'g') ADVANCE(113);
      END_STATE();
    case 165:
      if (lookahead == 'g') ADVANCE(115);
      END_STATE();
    case 166:
      if (lookahead == 'h') ADVANCE(487);
      END_STATE();
    case 167:
      if (lookahead == 'h') ADVANCE(502);
      END_STATE();
    case 168:
      if (lookahead == 'h') ADVANCE(18);
      END_STATE();
    case 169:
      if (lookahead == 'h') ADVANCE(287);
      END_STATE();
    case 170:
      if (lookahead == 'h') ADVANCE(342);
      END_STATE();
    case 171:
      if (lookahead == 'h') ADVANCE(294);
      END_STATE();
    case 172:
      if (lookahead == 'i') ADVANCE(58);
      if (lookahead == 't') ADVANCE(274);
      END_STATE();
    case 173:
      if (lookahead == 'i') ADVANCE(154);
      END_STATE();
    case 174:
      if (lookahead == 'i') ADVANCE(242);
      END_STATE();
    case 175:
      if (lookahead == 'i') ADVANCE(396);
      END_STATE();
    case 176:
      if (lookahead == 'i') ADVANCE(67);
      if (lookahead == 't') ADVANCE(274);
      END_STATE();
    case 177:
      if (lookahead == 'i') ADVANCE(358);
      END_STATE();
    case 178:
      if (lookahead == 'i') ADVANCE(59);
      END_STATE();
    case 179:
      if (lookahead == 'i') ADVANCE(302);
      END_STATE();
    case 180:
      if (lookahead == 'i') ADVANCE(267);
      END_STATE();
    case 181:
      if (lookahead == 'i') ADVANCE(73);
      END_STATE();
    case 182:
      if (lookahead == 'i') ADVANCE(96);
      END_STATE();
    case 183:
      if (lookahead == 'i') ADVANCE(368);
      END_STATE();
    case 184:
      if (lookahead == 'i') ADVANCE(21);
      END_STATE();
    case 185:
      if (lookahead == 'i') ADVANCE(285);
      END_STATE();
    case 186:
      if (lookahead == 'i') ADVANCE(269);
      END_STATE();
    case 187:
      if (lookahead == 'i') ADVANCE(359);
      END_STATE();
    case 188:
      if (lookahead == 'i') ADVANCE(254);
      END_STATE();
    case 189:
      if (lookahead == 'i') ADVANCE(215);
      END_STATE();
    case 190:
      if (lookahead == 'i') ADVANCE(216);
      END_STATE();
    case 191:
      if (lookahead == 'i') ADVANCE(261);
      END_STATE();
    case 192:
      if (lookahead == 'i') ADVANCE(217);
      END_STATE();
    case 193:
      if (lookahead == 'i') ADVANCE(256);
      END_STATE();
    case 194:
      if (lookahead == 'i') ADVANCE(257);
      END_STATE();
    case 195:
      if (lookahead == 'i') ADVANCE(140);
      END_STATE();
    case 196:
      if (lookahead == 'i') ADVANCE(260);
      END_STATE();
    case 197:
      if (lookahead == 'i') ADVANCE(259);
      END_STATE();
    case 198:
      if (lookahead == 'i') ADVANCE(143);
      END_STATE();
    case 199:
      if (lookahead == 'i') ADVANCE(146);
      END_STATE();
    case 200:
      if (lookahead == 'i') ADVANCE(397);
      END_STATE();
    case 201:
      if (lookahead == 'i') ADVANCE(303);
      END_STATE();
    case 202:
      if (lookahead == 'i') ADVANCE(286);
      END_STATE();
    case 203:
      if (lookahead == 'i') ADVANCE(222);
      END_STATE();
    case 204:
      if (lookahead == 'i') ADVANCE(243);
      END_STATE();
    case 205:
      if (lookahead == 'j') ADVANCE(128);
      END_STATE();
    case 206:
      if (lookahead == 'k') ADVANCE(11);
      END_STATE();
    case 207:
      if (lookahead == 'k') ADVANCE(450);
      END_STATE();
    case 208:
      if (lookahead == 'k') ADVANCE(17);
      END_STATE();
    case 209:
      if (lookahead == 'k') ADVANCE(38);
      END_STATE();
    case 210:
      if (lookahead == 'l') ADVANCE(506);
      END_STATE();
    case 211:
      if (lookahead == 'l') ADVANCE(456);
      END_STATE();
    case 212:
      if (lookahead == 'l') ADVANCE(36);
      if (lookahead == 'o') ADVANCE(308);
      if (lookahead == 'r') ADVANCE(51);
      END_STATE();
    case 213:
      if (lookahead == 'l') ADVANCE(181);
      END_STATE();
    case 214:
      if (lookahead == 'l') ADVANCE(379);
      END_STATE();
    case 215:
      if (lookahead == 'l') ADVANCE(91);
      END_STATE();
    case 216:
      if (lookahead == 'l') ADVANCE(92);
      END_STATE();
    case 217:
      if (lookahead == 'l') ADVANCE(93);
      END_STATE();
    case 218:
      if (lookahead == 'l') ADVANCE(334);
      END_STATE();
    case 219:
      if (lookahead == 'l') ADVANCE(134);
      END_STATE();
    case 220:
      if (lookahead == 'l') ADVANCE(362);
      END_STATE();
    case 221:
      if (lookahead == 'l') ADVANCE(137);
      END_STATE();
    case 222:
      if (lookahead == 'l') ADVANCE(130);
      END_STATE();
    case 223:
      if (lookahead == 'l') ADVANCE(123);
      END_STATE();
    case 224:
      if (lookahead == 'l') ADVANCE(349);
      END_STATE();
    case 225:
      if (lookahead == 'l') ADVANCE(53);
      END_STATE();
    case 226:
      if (lookahead == 'l') ADVANCE(136);
      END_STATE();
    case 227:
      if (lookahead == 'l') ADVANCE(196);
      END_STATE();
    case 228:
      if (lookahead == 'l') ADVANCE(385);
      END_STATE();
    case 229:
      if (lookahead == 'l') ADVANCE(54);
      END_STATE();
    case 230:
      if (lookahead == 'l') ADVANCE(55);
      END_STATE();
    case 231:
      if (lookahead == 'm') ADVANCE(481);
      END_STATE();
    case 232:
      if (lookahead == 'm') ADVANCE(354);
      if (lookahead == 'n') ADVANCE(47);
      if (lookahead == 'x') ADVANCE(75);
      END_STATE();
    case 233:
      if (lookahead == 'm') ADVANCE(354);
      if (lookahead == 'x') ADVANCE(74);
      END_STATE();
    case 234:
      if (lookahead == 'm') ADVANCE(245);
      if (lookahead == 'p') ADVANCE(394);
      END_STATE();
    case 235:
      if (lookahead == 'm') ADVANCE(65);
      END_STATE();
    case 236:
      if (lookahead == 'm') ADVANCE(124);
      END_STATE();
    case 237:
      if (lookahead == 'm') ADVANCE(104);
      END_STATE();
    case 238:
      if (lookahead == 'm') ADVANCE(42);
      END_STATE();
    case 239:
      if (lookahead == 'm') ADVANCE(296);
      if (lookahead == 'p') ADVANCE(394);
      END_STATE();
    case 240:
      if (lookahead == 'm') ADVANCE(108);
      END_STATE();
    case 241:
      if (lookahead == 'm') ADVANCE(41);
      END_STATE();
    case 242:
      if (lookahead == 'm') ADVANCE(175);
      if (lookahead == 'o') ADVANCE(253);
      END_STATE();
    case 243:
      if (lookahead == 'm') ADVANCE(133);
      END_STATE();
    case 244:
      if (lookahead == 'm') ADVANCE(188);
      END_STATE();
    case 245:
      if (lookahead == 'm') ADVANCE(187);
      if (lookahead == 'p') ADVANCE(203);
      END_STATE();
    case 246:
      if (lookahead == 'm') ADVANCE(66);
      END_STATE();
    case 247:
      if (lookahead == 'm') ADVANCE(193);
      END_STATE();
    case 248:
      if (lookahead == 'm') ADVANCE(194);
      END_STATE();
    case 249:
      if (lookahead == 'm') ADVANCE(197);
      END_STATE();
    case 250:
      if (lookahead == 'n') ADVANCE(94);
      if (lookahead == 's') ADVANCE(345);
      END_STATE();
    case 251:
      if (lookahead == 'n') ADVANCE(94);
      if (lookahead == 's') ADVANCE(345);
      if (lookahead == 'u') ADVANCE(366);
      END_STATE();
    case 252:
      if (lookahead == 'n') ADVANCE(87);
      if (lookahead == 'o') ADVANCE(327);
      END_STATE();
    case 253:
      if (lookahead == 'n') ADVANCE(504);
      END_STATE();
    case 254:
      if (lookahead == 'n') ADVANCE(458);
      END_STATE();
    case 255:
      if (lookahead == 'n') ADVANCE(503);
      END_STATE();
    case 256:
      if (lookahead == 'n') ADVANCE(459);
      END_STATE();
    case 257:
      if (lookahead == 'n') ADVANCE(457);
      END_STATE();
    case 258:
      if (lookahead == 'n') ADVANCE(447);
      END_STATE();
    case 259:
      if (lookahead == 'n') ADVANCE(460);
      END_STATE();
    case 260:
      if (lookahead == 'n') ADVANCE(207);
      END_STATE();
    case 261:
      if (lookahead == 'n') ADVANCE(153);
      END_STATE();
    case 262:
      if (lookahead == 'n') ADVANCE(100);
      END_STATE();
    case 263:
      if (lookahead == 'n') ADVANCE(39);
      END_STATE();
    case 264:
      if (lookahead == 'n') ADVANCE(372);
      END_STATE();
    case 265:
      if (lookahead == 'n') ADVANCE(86);
      END_STATE();
    case 266:
      if (lookahead == 'n') ADVANCE(186);
      END_STATE();
    case 267:
      if (lookahead == 'n') ADVANCE(135);
      END_STATE();
    case 268:
      if (lookahead == 'n') ADVANCE(183);
      END_STATE();
    case 269:
      if (lookahead == 'n') ADVANCE(160);
      END_STATE();
    case 270:
      if (lookahead == 'n') ADVANCE(352);
      END_STATE();
    case 271:
      if (lookahead == 'n') ADVANCE(28);
      END_STATE();
    case 272:
      if (lookahead == 'o') ADVANCE(234);
      END_STATE();
    case 273:
      if (lookahead == 'o') ADVANCE(378);
      if (lookahead == 't') ADVANCE(89);
      if (lookahead == 'y') ADVANCE(235);
      END_STATE();
    case 274:
      if (lookahead == 'o') ADVANCE(453);
      END_STATE();
    case 275:
      if (lookahead == 'o') ADVANCE(449);
      END_STATE();
    case 276:
      if (lookahead == 'o') ADVANCE(482);
      END_STATE();
    case 277:
      if (lookahead == 'o') ADVANCE(95);
      END_STATE();
    case 278:
      if (lookahead == 'o') ADVANCE(239);
      END_STATE();
    case 279:
      if (lookahead == 'o') ADVANCE(210);
      END_STATE();
    case 280:
      if (lookahead == 'o') ADVANCE(314);
      END_STATE();
    case 281:
      if (lookahead == 'o') ADVANCE(218);
      END_STATE();
    case 282:
      if (lookahead == 'o') ADVANCE(319);
      END_STATE();
    case 283:
      if (lookahead == 'o') ADVANCE(182);
      END_STATE();
    case 284:
      if (lookahead == 'o') ADVANCE(279);
      if (lookahead == 'v') ADVANCE(290);
      if (lookahead == 'y') ADVANCE(299);
      END_STATE();
    case 285:
      if (lookahead == 'o') ADVANCE(255);
      END_STATE();
    case 286:
      if (lookahead == 'o') ADVANCE(258);
      END_STATE();
    case 287:
      if (lookahead == 'o') ADVANCE(313);
      END_STATE();
    case 288:
      if (lookahead == 'o') ADVANCE(317);
      END_STATE();
    case 289:
      if (lookahead == 'o') ADVANCE(388);
      END_STATE();
    case 290:
      if (lookahead == 'o') ADVANCE(355);
      END_STATE();
    case 291:
      if (lookahead == 'o') ADVANCE(389);
      if (lookahead == 't') ADVANCE(89);
      END_STATE();
    case 292:
      if (lookahead == 'o') ADVANCE(356);
      END_STATE();
    case 293:
      if (lookahead == 'o') ADVANCE(390);
      END_STATE();
    case 294:
      if (lookahead == 'o') ADVANCE(357);
      END_STATE();
    case 295:
      if (lookahead == 'p') ADVANCE(466);
      END_STATE();
    case 296:
      if (lookahead == 'p') ADVANCE(203);
      END_STATE();
    case 297:
      if (lookahead == 'p') ADVANCE(370);
      if (lookahead == 'u') ADVANCE(367);
      END_STATE();
    case 298:
      if (lookahead == 'p') ADVANCE(382);
      END_STATE();
    case 299:
      if (lookahead == 'p') ADVANCE(106);
      END_STATE();
    case 300:
      if (lookahead == 'p') ADVANCE(45);
      END_STATE();
    case 301:
      if (lookahead == 'p') ADVANCE(184);
      END_STATE();
    case 302:
      if (lookahead == 'p') ADVANCE(375);
      END_STATE();
    case 303:
      if (lookahead == 'p') ADVANCE(374);
      END_STATE();
    case 304:
      if (lookahead == 'p') ADVANCE(50);
      END_STATE();
    case 305:
      if (lookahead == 'r') ADVANCE(489);
      END_STATE();
    case 306:
      if (lookahead == 'r') ADVANCE(505);
      END_STATE();
    case 307:
      if (lookahead == 'r') ADVANCE(488);
      END_STATE();
    case 308:
      if (lookahead == 'r') ADVANCE(238);
      END_STATE();
    case 309:
      if (lookahead == 'r') ADVANCE(266);
      if (lookahead == 't') ADVANCE(83);
      END_STATE();
    case 310:
      if (lookahead == 'r') ADVANCE(79);
      END_STATE();
    case 311:
      if (lookahead == 'r') ADVANCE(283);
      END_STATE();
    case 312:
      if (lookahead == 'r') ADVANCE(348);
      END_STATE();
    case 313:
      if (lookahead == 'r') ADVANCE(329);
      END_STATE();
    case 314:
      if (lookahead == 'r') ADVANCE(369);
      END_STATE();
    case 315:
      if (lookahead == 'r') ADVANCE(195);
      END_STATE();
    case 316:
      if (lookahead == 'r') ADVANCE(179);
      END_STATE();
    case 317:
      if (lookahead == 'r') ADVANCE(344);
      END_STATE();
    case 318:
      if (lookahead == 'r') ADVANCE(331);
      END_STATE();
    case 319:
      if (lookahead == 'r') ADVANCE(208);
      END_STATE();
    case 320:
      if (lookahead == 'r') ADVANCE(326);
      END_STATE();
    case 321:
      if (lookahead == 'r') ADVANCE(80);
      END_STATE();
    case 322:
      if (lookahead == 'r') ADVANCE(198);
      END_STATE();
    case 323:
      if (lookahead == 'r') ADVANCE(201);
      END_STATE();
    case 324:
      if (lookahead == 'r') ADVANCE(81);
      END_STATE();
    case 325:
      if (lookahead == 'r') ADVANCE(82);
      END_STATE();
    case 326:
      if (lookahead == 'r') ADVANCE(288);
      END_STATE();
    case 327:
      if (lookahead == 's') ADVANCE(13);
      END_STATE();
    case 328:
      if (lookahead == 's') ADVANCE(508);
      END_STATE();
    case 329:
      if (lookahead == 's') ADVANCE(514);
      END_STATE();
    case 330:
      if (lookahead == 's') ADVANCE(470);
      END_STATE();
    case 331:
      if (lookahead == 's') ADVANCE(467);
      END_STATE();
    case 332:
      if (lookahead == 's') ADVANCE(463);
      END_STATE();
    case 333:
      if (lookahead == 's') ADVANCE(462);
      END_STATE();
    case 334:
      if (lookahead == 's') ADVANCE(495);
      END_STATE();
    case 335:
      if (lookahead == 's') ADVANCE(493);
      END_STATE();
    case 336:
      if (lookahead == 's') ADVANCE(454);
      END_STATE();
    case 337:
      if (lookahead == 's') ADVANCE(494);
      END_STATE();
    case 338:
      if (lookahead == 's') ADVANCE(476);
      END_STATE();
    case 339:
      if (lookahead == 's') ADVANCE(475);
      END_STATE();
    case 340:
      if (lookahead == 's') ADVANCE(496);
      END_STATE();
    case 341:
      if (lookahead == 's') ADVANCE(474);
      END_STATE();
    case 342:
      if (lookahead == 's') ADVANCE(473);
      END_STATE();
    case 343:
      if (lookahead == 's') ADVANCE(479);
      END_STATE();
    case 344:
      if (lookahead == 's') ADVANCE(455);
      END_STATE();
    case 345:
      if (lookahead == 's') ADVANCE(127);
      END_STATE();
    case 346:
      if (lookahead == 's') ADVANCE(289);
      END_STATE();
    case 347:
      if (lookahead == 's') ADVANCE(376);
      END_STATE();
    case 348:
      if (lookahead == 's') ADVANCE(185);
      END_STATE();
    case 349:
      if (lookahead == 's') ADVANCE(107);
      END_STATE();
    case 350:
      if (lookahead == 's') ADVANCE(20);
      END_STATE();
    case 351:
      if (lookahead == 's') ADVANCE(364);
      END_STATE();
    case 352:
      if (lookahead == 's') ADVANCE(112);
      END_STATE();
    case 353:
      if (lookahead == 's') ADVANCE(200);
      END_STATE();
    case 354:
      if (lookahead == 's') ADVANCE(85);
      END_STATE();
    case 355:
      if (lookahead == 's') ADVANCE(22);
      END_STATE();
    case 356:
      if (lookahead == 's') ADVANCE(23);
      END_STATE();
    case 357:
      if (lookahead == 's') ADVANCE(24);
      END_STATE();
    case 358:
      if (lookahead == 't') ADVANCE(499);
      END_STATE();
    case 359:
      if (lookahead == 't') ADVANCE(501);
      END_STATE();
    case 360:
      if (lookahead == 't') ADVANCE(507);
      END_STATE();
    case 361:
      if (lookahead == 't') ADVANCE(492);
      END_STATE();
    case 362:
      if (lookahead == 't') ADVANCE(509);
      END_STATE();
    case 363:
      if (lookahead == 't') ADVANCE(438);
      END_STATE();
    case 364:
      if (lookahead == 't') ADVANCE(461);
      END_STATE();
    case 365:
      if (lookahead == 't') ADVANCE(77);
      END_STATE();
    case 366:
      if (lookahead == 't') ADVANCE(169);
      END_STATE();
    case 367:
      if (lookahead == 't') ADVANCE(298);
      END_STATE();
    case 368:
      if (lookahead == 't') ADVANCE(395);
      END_STATE();
    case 369:
      if (lookahead == 't') ADVANCE(16);
      END_STATE();
    case 370:
      if (lookahead == 't') ADVANCE(174);
      END_STATE();
    case 371:
      if (lookahead == 't') ADVANCE(170);
      END_STATE();
    case 372:
      if (lookahead == 't') ADVANCE(204);
      END_STATE();
    case 373:
      if (lookahead == 't') ADVANCE(114);
      END_STATE();
    case 374:
      if (lookahead == 't') ADVANCE(145);
      END_STATE();
    case 375:
      if (lookahead == 't') ADVANCE(202);
      END_STATE();
    case 376:
      if (lookahead == 't') ADVANCE(25);
      END_STATE();
    case 377:
      if (lookahead == 'u') ADVANCE(158);
      END_STATE();
    case 378:
      if (lookahead == 'u') ADVANCE(310);
      END_STATE();
    case 379:
      if (lookahead == 'u') ADVANCE(97);
      END_STATE();
    case 380:
      if (lookahead == 'u') ADVANCE(220);
      END_STATE();
    case 381:
      if (lookahead == 'u') ADVANCE(105);
      END_STATE();
    case 382:
      if (lookahead == 'u') ADVANCE(361);
      END_STATE();
    case 383:
      if (lookahead == 'u') ADVANCE(189);
      END_STATE();
    case 384:
      if (lookahead == 'u') ADVANCE(221);
      END_STATE();
    case 385:
      if (lookahead == 'u') ADVANCE(98);
      END_STATE();
    case 386:
      if (lookahead == 'u') ADVANCE(190);
      END_STATE();
    case 387:
      if (lookahead == 'u') ADVANCE(192);
      END_STATE();
    case 388:
      if (lookahead == 'u') ADVANCE(321);
      END_STATE();
    case 389:
      if (lookahead == 'u') ADVANCE(324);
      END_STATE();
    case 390:
      if (lookahead == 'u') ADVANCE(325);
      END_STATE();
    case 391:
      if (lookahead == 'v') ADVANCE(142);
      END_STATE();
    case 392:
      if (lookahead == 'v') ADVANCE(43);
      END_STATE();
    case 393:
      if (lookahead == 'w') ADVANCE(282);
      END_STATE();
    case 394:
      if (lookahead == 'y') ADVANCE(480);
      END_STATE();
    case 395:
      if (lookahead == 'y') ADVANCE(26);
      END_STATE();
    case 396:
      if (lookahead == 'z') ADVANCE(116);
      END_STATE();
    case 397:
      if (lookahead == 'z') ADVANCE(117);
      END_STATE();
    case 398:
      if (lookahead == '{') ADVANCE(428);
      END_STATE();
    case 399:
      if (lookahead == '"' ||
          lookahead == '/' ||
          lookahead == '\\' ||
          lookahead == 'n' ||
          lookahead == 'r' ||
          lookahead == 't') ADVANCE(427);
      END_STATE();
    case 400:
      if (eof) ADVANCE(401);
      ADVANCE_MAP(
        '"', 418,
        '#', 402,
        '(', 9,
        ',', 498,
        '.', 443,
        '/', 497,
        '=', 444,
        '[', 441,
        ']', 442,
        'a', 251,
        'b', 35,
        'c', 272,
        'd', 103,
        'e', 232,
        'f', 30,
        'g', 177,
        'h', 125,
        'i', 252,
        'l', 172,
        'm', 31,
        'n', 46,
        'o', 297,
        'p', 32,
        'r', 126,
        's', 273,
        't', 33,
        'u', 268,
        'v', 120,
        'w', 34,
        '{', 439,
        '}', 440,
      );
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') SKIP(400);
      if (('0' <= lookahead && lookahead <= '9')) ADVANCE(413);
      END_STATE();
    case 401:
      ACCEPT_TOKEN(ts_builtin_sym_end);
      END_STATE();
    case 402:
      ACCEPT_TOKEN(sym_comment);
      if (lookahead != 0 &&
          lookahead != '\n') ADVANCE(402);
      END_STATE();
    case 403:
      ACCEPT_TOKEN(sym_block_comment);
      END_STATE();
    case 404:
      ACCEPT_TOKEN(sym_block_comment);
      if (lookahead == '*') ADVANCE(7);
      if (lookahead != 0) ADVANCE(10);
      END_STATE();
    case 405:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'a') ADVANCE(408);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('b' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 406:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'e') ADVANCE(415);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 407:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'e') ADVANCE(417);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 408:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'l') ADVANCE(410);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 409:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'r') ADVANCE(411);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 410:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 's') ADVANCE(407);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 411:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == 'u') ADVANCE(406);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 412:
      ACCEPT_TOKEN(sym_identifier);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 413:
      ACCEPT_TOKEN(sym_integer);
      if (('0' <= lookahead && lookahead <= '9')) ADVANCE(413);
      END_STATE();
    case 414:
      ACCEPT_TOKEN(anon_sym_true);
      END_STATE();
    case 415:
      ACCEPT_TOKEN(anon_sym_true);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 416:
      ACCEPT_TOKEN(anon_sym_false);
      END_STATE();
    case 417:
      ACCEPT_TOKEN(anon_sym_false);
      if (lookahead == '+' ||
          lookahead == '-' ||
          ('0' <= lookahead && lookahead <= '9') ||
          ('A' <= lookahead && lookahead <= 'Z') ||
          lookahead == '_' ||
          ('a' <= lookahead && lookahead <= 'z')) ADVANCE(412);
      END_STATE();
    case 418:
      ACCEPT_TOKEN(anon_sym_DQUOTE);
      END_STATE();
    case 419:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == '\n') ADVANCE(426);
      if (lookahead == '"' ||
          lookahead == '$' ||
          lookahead == '\\') ADVANCE(402);
      if (lookahead != 0) ADVANCE(419);
      END_STATE();
    case 420:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == '#') ADVANCE(419);
      if (lookahead == '(') ADVANCE(423);
      if (lookahead == '/') ADVANCE(425);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') ADVANCE(420);
      if (lookahead != 0 &&
          (lookahead < '"' || '$' < lookahead) &&
          lookahead != '\\') ADVANCE(426);
      END_STATE();
    case 421:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == ')') ADVANCE(426);
      if (lookahead == '*') ADVANCE(422);
      if (lookahead == '"' ||
          lookahead == '$' ||
          lookahead == '\\') ADVANCE(10);
      if (lookahead != 0) ADVANCE(424);
      END_STATE();
    case 422:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == ')') ADVANCE(424);
      if (lookahead == '*') ADVANCE(421);
      if (lookahead == '"' ||
          lookahead == '$' ||
          lookahead == '\\') ADVANCE(10);
      if (lookahead != 0) ADVANCE(424);
      END_STATE();
    case 423:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == '*') ADVANCE(424);
      if (lookahead != 0 &&
          lookahead != '"' &&
          lookahead != '$' &&
          lookahead != '\\') ADVANCE(426);
      END_STATE();
    case 424:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == '*') ADVANCE(421);
      if (lookahead == '"' ||
          lookahead == '$' ||
          lookahead == '\\') ADVANCE(10);
      if (lookahead != 0) ADVANCE(424);
      END_STATE();
    case 425:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead == '/') ADVANCE(419);
      if (lookahead != 0 &&
          lookahead != '"' &&
          lookahead != '$' &&
          lookahead != '\\') ADVANCE(426);
      END_STATE();
    case 426:
      ACCEPT_TOKEN(sym_string_content);
      if (lookahead != 0 &&
          lookahead != '"' &&
          lookahead != '$' &&
          lookahead != '\\') ADVANCE(426);
      END_STATE();
    case 427:
      ACCEPT_TOKEN(sym_escape_sequence);
      END_STATE();
    case 428:
      ACCEPT_TOKEN(anon_sym_DOLLAR_LBRACE);
      END_STATE();
    case 429:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == '\n') ADVANCE(436);
      if (lookahead == '}') ADVANCE(402);
      if (lookahead != 0) ADVANCE(429);
      END_STATE();
    case 430:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == '#') ADVANCE(429);
      if (lookahead == '(') ADVANCE(433);
      if (lookahead == '/') ADVANCE(435);
      if (('\t' <= lookahead && lookahead <= '\r') ||
          lookahead == ' ') ADVANCE(430);
      if (lookahead != 0 &&
          lookahead != '}') ADVANCE(436);
      END_STATE();
    case 431:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == ')') ADVANCE(436);
      if (lookahead == '*') ADVANCE(432);
      if (lookahead == '}') ADVANCE(10);
      if (lookahead != 0) ADVANCE(434);
      END_STATE();
    case 432:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == ')') ADVANCE(434);
      if (lookahead == '*') ADVANCE(431);
      if (lookahead == '}') ADVANCE(10);
      if (lookahead != 0) ADVANCE(434);
      END_STATE();
    case 433:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == '*') ADVANCE(434);
      if (lookahead != 0 &&
          lookahead != '}') ADVANCE(436);
      END_STATE();
    case 434:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == '*') ADVANCE(431);
      if (lookahead == '}') ADVANCE(10);
      if (lookahead != 0) ADVANCE(434);
      END_STATE();
    case 435:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead == '/') ADVANCE(429);
      if (lookahead != 0 &&
          lookahead != '}') ADVANCE(436);
      END_STATE();
    case 436:
      ACCEPT_TOKEN(aux_sym_env_interpolation_token1);
      if (lookahead != 0 &&
          lookahead != '}') ADVANCE(436);
      END_STATE();
    case 437:
      ACCEPT_TOKEN(anon_sym_RBRACE);
      END_STATE();
    case 438:
      ACCEPT_TOKEN(anon_sym_project);
      END_STATE();
    case 439:
      ACCEPT_TOKEN(anon_sym_LBRACE);
      END_STATE();
    case 440:
      ACCEPT_TOKEN(anon_sym_RBRACE2);
      END_STATE();
    case 441:
      ACCEPT_TOKEN(anon_sym_LBRACK);
      END_STATE();
    case 442:
      ACCEPT_TOKEN(anon_sym_RBRACK);
      END_STATE();
    case 443:
      ACCEPT_TOKEN(anon_sym_DOT);
      END_STATE();
    case 444:
      ACCEPT_TOKEN(anon_sym_EQ);
      END_STATE();
    case 445:
      ACCEPT_TOKEN(anon_sym_type);
      END_STATE();
    case 446:
      ACCEPT_TOKEN(anon_sym_std);
      if (lookahead == 'l') ADVANCE(178);
      END_STATE();
    case 447:
      ACCEPT_TOKEN(anon_sym_description);
      END_STATE();
    case 448:
      ACCEPT_TOKEN(anon_sym_optimize);
      END_STATE();
    case 449:
      ACCEPT_TOKEN(anon_sym_debug_DASHinfo);
      END_STATE();
    case 450:
      ACCEPT_TOKEN(anon_sym_runtime_DASHlink);
      END_STATE();
    case 451:
      ACCEPT_TOKEN(anon_sym_libc);
      END_STATE();
    case 452:
      ACCEPT_TOKEN(anon_sym_stdlib);
      END_STATE();
    case 453:
      ACCEPT_TOKEN(anon_sym_lto);
      END_STATE();
    case 454:
      ACCEPT_TOKEN(anon_sym_warnings);
      if (lookahead == '-') ADVANCE(44);
      END_STATE();
    case 455:
      ACCEPT_TOKEN(anon_sym_warnings_DASHas_DASHerrors);
      END_STATE();
    case 456:
      ACCEPT_TOKEN(anon_sym_android_DASHapi_DASHlevel);
      END_STATE();
    case 457:
      ACCEPT_TOKEN(anon_sym_macos_DASHmin);
      END_STATE();
    case 458:
      ACCEPT_TOKEN(anon_sym_ios_DASHmin);
      END_STATE();
    case 459:
      ACCEPT_TOKEN(anon_sym_tvos_DASHmin);
      END_STATE();
    case 460:
      ACCEPT_TOKEN(anon_sym_watchos_DASHmin);
      END_STATE();
    case 461:
      ACCEPT_TOKEN(anon_sym_manifest);
      END_STATE();
    case 462:
      ACCEPT_TOKEN(anon_sym_sources);
      END_STATE();
    case 463:
      ACCEPT_TOKEN(anon_sym_modules);
      END_STATE();
    case 464:
      ACCEPT_TOKEN(anon_sym_include);
      END_STATE();
    case 465:
      ACCEPT_TOKEN(anon_sym_exclude);
      END_STATE();
    case 466:
      ACCEPT_TOKEN(anon_sym_export_DASHmap);
      END_STATE();
    case 467:
      ACCEPT_TOKEN(anon_sym_headers);
      END_STATE();
    case 468:
      ACCEPT_TOKEN(anon_sym_public);
      END_STATE();
    case 469:
      ACCEPT_TOKEN(anon_sym_private);
      END_STATE();
    case 470:
      ACCEPT_TOKEN(anon_sym_defines);
      END_STATE();
    case 471:
      ACCEPT_TOKEN(anon_sym_links);
      END_STATE();
    case 472:
      ACCEPT_TOKEN(anon_sym_frameworks);
      END_STATE();
    case 473:
      ACCEPT_TOKEN(anon_sym_framework_DASHpaths);
      END_STATE();
    case 474:
      ACCEPT_TOKEN(anon_sym_compile_DASHflags);
      END_STATE();
    case 475:
      ACCEPT_TOKEN(anon_sym_link_DASHflags);
      END_STATE();
    case 476:
      ACCEPT_TOKEN(anon_sym_resources);
      END_STATE();
    case 477:
      ACCEPT_TOKEN(anon_sym_pre_DASHbuild);
      END_STATE();
    case 478:
      ACCEPT_TOKEN(anon_sym_post_DASHbuild);
      END_STATE();
    case 479:
      ACCEPT_TOKEN(anon_sym_emscripten_DASHflags);
      END_STATE();
    case 480:
      ACCEPT_TOKEN(anon_sym_copy);
      END_STATE();
    case 481:
      ACCEPT_TOKEN(anon_sym_from);
      END_STATE();
    case 482:
      ACCEPT_TOKEN(anon_sym_to);
      END_STATE();
    case 483:
      ACCEPT_TOKEN(anon_sym_to);
      if (lookahead == 'o') ADVANCE(210);
      END_STATE();
    case 484:
      ACCEPT_TOKEN(anon_sym_unity_DASHbuild);
      END_STATE();
    case 485:
      ACCEPT_TOKEN(anon_sym_enabled);
      END_STATE();
    case 486:
      ACCEPT_TOKEN(anon_sym_batch_DASHsize);
      END_STATE();
    case 487:
      ACCEPT_TOKEN(anon_sym_pch);
      END_STATE();
    case 488:
      ACCEPT_TOKEN(anon_sym_header);
      END_STATE();
    case 489:
      ACCEPT_TOKEN(anon_sym_header);
      if (lookahead == 's') ADVANCE(467);
      END_STATE();
    case 490:
      ACCEPT_TOKEN(anon_sym_source);
      END_STATE();
    case 491:
      ACCEPT_TOKEN(anon_sym_source);
      if (lookahead == 's') ADVANCE(462);
      END_STATE();
    case 492:
      ACCEPT_TOKEN(anon_sym_output);
      END_STATE();
    case 493:
      ACCEPT_TOKEN(anon_sym_binaries);
      END_STATE();
    case 494:
      ACCEPT_TOKEN(anon_sym_libraries);
      END_STATE();
    case 495:
      ACCEPT_TOKEN(anon_sym_symbols);
      END_STATE();
    case 496:
      ACCEPT_TOKEN(anon_sym_dependencies);
      END_STATE();
    case 497:
      ACCEPT_TOKEN(anon_sym_SLASH);
      if (lookahead == '/') ADVANCE(402);
      END_STATE();
    case 498:
      ACCEPT_TOKEN(anon_sym_COMMA);
      END_STATE();
    case 499:
      ACCEPT_TOKEN(anon_sym_git);
      END_STATE();
    case 500:
      ACCEPT_TOKEN(anon_sym_tag);
      END_STATE();
    case 501:
      ACCEPT_TOKEN(anon_sym_commit);
      END_STATE();
    case 502:
      ACCEPT_TOKEN(anon_sym_path);
      END_STATE();
    case 503:
      ACCEPT_TOKEN(anon_sym_version);
      END_STATE();
    case 504:
      ACCEPT_TOKEN(anon_sym_option);
      END_STATE();
    case 505:
      ACCEPT_TOKEN(anon_sym_assembler);
      END_STATE();
    case 506:
      ACCEPT_TOKEN(anon_sym_tool);
      END_STATE();
    case 507:
      ACCEPT_TOKEN(anon_sym_format);
      END_STATE();
    case 508:
      ACCEPT_TOKEN(anon_sym_flags);
      END_STATE();
    case 509:
      ACCEPT_TOKEN(anon_sym_default);
      END_STATE();
    case 510:
      ACCEPT_TOKEN(anon_sym_package);
      END_STATE();
    case 511:
      ACCEPT_TOKEN(anon_sym_name);
      END_STATE();
    case 512:
      ACCEPT_TOKEN(anon_sym_license);
      END_STATE();
    case 513:
      ACCEPT_TOKEN(anon_sym_homepage);
      END_STATE();
    case 514:
      ACCEPT_TOKEN(anon_sym_authors);
      END_STATE();
    case 515:
      ACCEPT_TOKEN(anon_sym_exports);
      END_STATE();
    default:
      return false;
  }
}

static const TSLexMode ts_lex_modes[STATE_COUNT] = {
  [0] = {.lex_state = 0},
  [1] = {.lex_state = 0},
  [2] = {.lex_state = 0},
  [3] = {.lex_state = 0},
  [4] = {.lex_state = 4},
  [5] = {.lex_state = 4},
  [6] = {.lex_state = 4},
  [7] = {.lex_state = 4},
  [8] = {.lex_state = 4},
  [9] = {.lex_state = 0},
  [10] = {.lex_state = 4},
  [11] = {.lex_state = 4},
  [12] = {.lex_state = 4},
  [13] = {.lex_state = 4},
  [14] = {.lex_state = 4},
  [15] = {.lex_state = 4},
  [16] = {.lex_state = 4},
  [17] = {.lex_state = 4},
  [18] = {.lex_state = 4},
  [19] = {.lex_state = 4},
  [20] = {.lex_state = 4},
  [21] = {.lex_state = 4},
  [22] = {.lex_state = 4},
  [23] = {.lex_state = 4},
  [24] = {.lex_state = 4},
  [25] = {.lex_state = 4},
  [26] = {.lex_state = 4},
  [27] = {.lex_state = 4},
  [28] = {.lex_state = 4},
  [29] = {.lex_state = 4},
  [30] = {.lex_state = 4},
  [31] = {.lex_state = 4},
  [32] = {.lex_state = 4},
  [33] = {.lex_state = 4},
  [34] = {.lex_state = 0},
  [35] = {.lex_state = 0},
  [36] = {.lex_state = 0},
  [37] = {.lex_state = 4},
  [38] = {.lex_state = 4},
  [39] = {.lex_state = 0},
  [40] = {.lex_state = 4},
  [41] = {.lex_state = 0},
  [42] = {.lex_state = 0},
  [43] = {.lex_state = 0},
  [44] = {.lex_state = 0},
  [45] = {.lex_state = 0},
  [46] = {.lex_state = 0},
  [47] = {.lex_state = 1},
  [48] = {.lex_state = 0},
  [49] = {.lex_state = 0},
  [50] = {.lex_state = 0},
  [51] = {.lex_state = 0},
  [52] = {.lex_state = 0},
  [53] = {.lex_state = 0},
  [54] = {.lex_state = 0},
  [55] = {.lex_state = 4},
  [56] = {.lex_state = 4},
  [57] = {.lex_state = 4},
  [58] = {.lex_state = 4},
  [59] = {.lex_state = 0},
  [60] = {.lex_state = 0},
  [61] = {.lex_state = 0},
  [62] = {.lex_state = 0},
  [63] = {.lex_state = 3},
  [64] = {.lex_state = 0},
  [65] = {.lex_state = 3},
  [66] = {.lex_state = 3},
  [67] = {.lex_state = 0},
  [68] = {.lex_state = 0},
  [69] = {.lex_state = 3},
  [70] = {.lex_state = 3},
  [71] = {.lex_state = 0},
  [72] = {.lex_state = 0},
  [73] = {.lex_state = 6},
  [74] = {.lex_state = 0},
  [75] = {.lex_state = 6},
  [76] = {.lex_state = 0},
  [77] = {.lex_state = 5},
  [78] = {.lex_state = 0},
  [79] = {.lex_state = 0},
  [80] = {.lex_state = 6},
  [81] = {.lex_state = 0},
  [82] = {.lex_state = 0},
  [83] = {.lex_state = 5},
  [84] = {.lex_state = 6},
  [85] = {.lex_state = 0},
  [86] = {.lex_state = 6},
  [87] = {.lex_state = 0},
  [88] = {.lex_state = 5},
  [89] = {.lex_state = 0},
  [90] = {.lex_state = 0},
  [91] = {.lex_state = 0},
  [92] = {.lex_state = 0},
  [93] = {.lex_state = 0},
  [94] = {.lex_state = 6},
  [95] = {.lex_state = 0},
  [96] = {.lex_state = 0},
  [97] = {.lex_state = 0},
  [98] = {.lex_state = 0},
  [99] = {.lex_state = 0},
  [100] = {.lex_state = 6},
  [101] = {.lex_state = 0},
  [102] = {.lex_state = 6},
  [103] = {.lex_state = 0},
  [104] = {.lex_state = 0},
  [105] = {.lex_state = 0},
  [106] = {.lex_state = 0},
  [107] = {.lex_state = 0},
  [108] = {.lex_state = 5},
  [109] = {.lex_state = 5},
  [110] = {.lex_state = 5},
  [111] = {.lex_state = 0},
  [112] = {.lex_state = 0},
  [113] = {.lex_state = 0},
  [114] = {.lex_state = 0},
  [115] = {.lex_state = 0},
  [116] = {.lex_state = 0},
  [117] = {.lex_state = 3},
  [118] = {.lex_state = 0},
  [119] = {.lex_state = 0},
  [120] = {.lex_state = 0},
  [121] = {.lex_state = 0},
  [122] = {.lex_state = 0},
  [123] = {.lex_state = 0},
  [124] = {.lex_state = 0},
  [125] = {.lex_state = 0},
  [126] = {.lex_state = 0},
  [127] = {.lex_state = 0},
  [128] = {.lex_state = 0},
  [129] = {.lex_state = 0},
  [130] = {.lex_state = 0},
  [131] = {.lex_state = 0},
  [132] = {.lex_state = 0},
  [133] = {.lex_state = 0},
  [134] = {.lex_state = 0},
  [135] = {.lex_state = 0},
  [136] = {.lex_state = 0},
  [137] = {.lex_state = 0},
  [138] = {.lex_state = 6},
  [139] = {.lex_state = 6},
  [140] = {.lex_state = 0},
  [141] = {.lex_state = 0},
  [142] = {.lex_state = 0},
  [143] = {.lex_state = 0},
  [144] = {.lex_state = 0},
  [145] = {.lex_state = 0},
  [146] = {.lex_state = 0},
  [147] = {.lex_state = 6},
  [148] = {.lex_state = 6},
  [149] = {.lex_state = 0},
  [150] = {.lex_state = 0},
  [151] = {.lex_state = 0},
  [152] = {.lex_state = 6},
  [153] = {.lex_state = 0},
  [154] = {.lex_state = 6},
  [155] = {.lex_state = 0},
  [156] = {.lex_state = 0},
  [157] = {.lex_state = 0},
  [158] = {.lex_state = 6},
  [159] = {.lex_state = 6},
  [160] = {.lex_state = 6},
  [161] = {.lex_state = 0},
  [162] = {.lex_state = 0},
  [163] = {.lex_state = 0},
  [164] = {.lex_state = 0},
  [165] = {.lex_state = 0},
  [166] = {.lex_state = 6},
  [167] = {.lex_state = 0},
  [168] = {.lex_state = 6},
  [169] = {.lex_state = 0},
  [170] = {.lex_state = 0},
  [171] = {.lex_state = 5},
  [172] = {.lex_state = 6},
  [173] = {.lex_state = 0},
  [174] = {.lex_state = 0},
  [175] = {.lex_state = 0},
  [176] = {.lex_state = 0},
  [177] = {.lex_state = 0},
  [178] = {.lex_state = 0},
  [179] = {.lex_state = 0},
  [180] = {.lex_state = 0},
  [181] = {.lex_state = 0},
  [182] = {.lex_state = 0},
  [183] = {.lex_state = 0},
  [184] = {.lex_state = 0},
  [185] = {.lex_state = 6},
  [186] = {.lex_state = 0},
  [187] = {.lex_state = 0},
  [188] = {.lex_state = 0},
  [189] = {.lex_state = 0},
  [190] = {.lex_state = 0},
  [191] = {.lex_state = 0},
  [192] = {.lex_state = 0},
  [193] = {.lex_state = 0},
  [194] = {.lex_state = 0},
  [195] = {.lex_state = 0},
  [196] = {.lex_state = 1},
  [197] = {.lex_state = 0},
  [198] = {.lex_state = 0},
  [199] = {.lex_state = 0},
  [200] = {.lex_state = 0},
  [201] = {.lex_state = 0},
  [202] = {.lex_state = 0},
  [203] = {.lex_state = 0},
  [204] = {.lex_state = 0},
  [205] = {.lex_state = 0},
  [206] = {.lex_state = 0},
  [207] = {.lex_state = 0},
  [208] = {.lex_state = 430},
  [209] = {.lex_state = 0},
  [210] = {.lex_state = 0},
  [211] = {.lex_state = 0},
};

static const uint16_t ts_parse_table[LARGE_STATE_COUNT][SYMBOL_COUNT] = {
  [0] = {
    [ts_builtin_sym_end] = ACTIONS(1),
    [sym_comment] = ACTIONS(3),
    [sym_block_comment] = ACTIONS(3),
    [sym_integer] = ACTIONS(1),
    [anon_sym_true] = ACTIONS(1),
    [anon_sym_false] = ACTIONS(1),
    [anon_sym_DQUOTE] = ACTIONS(1),
    [sym_escape_sequence] = ACTIONS(1),
    [anon_sym_DOLLAR_LBRACE] = ACTIONS(1),
    [anon_sym_project] = ACTIONS(1),
    [anon_sym_LBRACE] = ACTIONS(1),
    [anon_sym_RBRACE2] = ACTIONS(1),
    [anon_sym_LBRACK] = ACTIONS(1),
    [anon_sym_RBRACK] = ACTIONS(1),
    [anon_sym_DOT] = ACTIONS(1),
    [anon_sym_EQ] = ACTIONS(1),
    [anon_sym_type] = ACTIONS(1),
    [anon_sym_std] = ACTIONS(1),
    [anon_sym_description] = ACTIONS(1),
    [anon_sym_optimize] = ACTIONS(1),
    [anon_sym_debug_DASHinfo] = ACTIONS(1),
    [anon_sym_runtime_DASHlink] = ACTIONS(1),
    [anon_sym_libc] = ACTIONS(1),
    [anon_sym_stdlib] = ACTIONS(1),
    [anon_sym_lto] = ACTIONS(1),
    [anon_sym_warnings] = ACTIONS(1),
    [anon_sym_warnings_DASHas_DASHerrors] = ACTIONS(1),
    [anon_sym_android_DASHapi_DASHlevel] = ACTIONS(1),
    [anon_sym_macos_DASHmin] = ACTIONS(1),
    [anon_sym_ios_DASHmin] = ACTIONS(1),
    [anon_sym_tvos_DASHmin] = ACTIONS(1),
    [anon_sym_watchos_DASHmin] = ACTIONS(1),
    [anon_sym_manifest] = ACTIONS(1),
    [anon_sym_sources] = ACTIONS(1),
    [anon_sym_modules] = ACTIONS(1),
    [anon_sym_include] = ACTIONS(1),
    [anon_sym_exclude] = ACTIONS(1),
    [anon_sym_export_DASHmap] = ACTIONS(1),
    [anon_sym_headers] = ACTIONS(1),
    [anon_sym_public] = ACTIONS(1),
    [anon_sym_private] = ACTIONS(1),
    [anon_sym_defines] = ACTIONS(1),
    [anon_sym_links] = ACTIONS(1),
    [anon_sym_frameworks] = ACTIONS(1),
    [anon_sym_framework_DASHpaths] = ACTIONS(1),
    [anon_sym_compile_DASHflags] = ACTIONS(1),
    [anon_sym_link_DASHflags] = ACTIONS(1),
    [anon_sym_resources] = ACTIONS(1),
    [anon_sym_pre_DASHbuild] = ACTIONS(1),
    [anon_sym_post_DASHbuild] = ACTIONS(1),
    [anon_sym_emscripten_DASHflags] = ACTIONS(1),
    [anon_sym_copy] = ACTIONS(1),
    [anon_sym_from] = ACTIONS(1),
    [anon_sym_to] = ACTIONS(1),
    [anon_sym_unity_DASHbuild] = ACTIONS(1),
    [anon_sym_enabled] = ACTIONS(1),
    [anon_sym_batch_DASHsize] = ACTIONS(1),
    [anon_sym_pch] = ACTIONS(1),
    [anon_sym_header] = ACTIONS(1),
    [anon_sym_source] = ACTIONS(1),
    [anon_sym_output] = ACTIONS(1),
    [anon_sym_binaries] = ACTIONS(1),
    [anon_sym_libraries] = ACTIONS(1),
    [anon_sym_symbols] = ACTIONS(1),
    [anon_sym_dependencies] = ACTIONS(1),
    [anon_sym_SLASH] = ACTIONS(1),
    [anon_sym_COMMA] = ACTIONS(1),
    [anon_sym_git] = ACTIONS(1),
    [anon_sym_tag] = ACTIONS(1),
    [anon_sym_commit] = ACTIONS(1),
    [anon_sym_path] = ACTIONS(1),
    [anon_sym_version] = ACTIONS(1),
    [anon_sym_option] = ACTIONS(1),
    [anon_sym_assembler] = ACTIONS(1),
    [anon_sym_tool] = ACTIONS(1),
    [anon_sym_format] = ACTIONS(1),
    [anon_sym_flags] = ACTIONS(1),
    [anon_sym_default] = ACTIONS(1),
    [anon_sym_package] = ACTIONS(1),
    [anon_sym_name] = ACTIONS(1),
    [anon_sym_license] = ACTIONS(1),
    [anon_sym_homepage] = ACTIONS(1),
    [anon_sym_authors] = ACTIONS(1),
    [anon_sym_exports] = ACTIONS(1),
  },
  [1] = {
    [sym_source_file] = STATE(163),
    [sym_project_decl] = STATE(133),
    [sym_package_decl] = STATE(182),
    [ts_builtin_sym_end] = ACTIONS(5),
    [sym_comment] = ACTIONS(3),
    [sym_block_comment] = ACTIONS(3),
    [anon_sym_project] = ACTIONS(7),
    [anon_sym_package] = ACTIONS(9),
  },
  [2] = {
    [sym_comment] = ACTIONS(3),
    [sym_block_comment] = ACTIONS(3),
    [anon_sym_DQUOTE] = ACTIONS(11),
    [anon_sym_RBRACE2] = ACTIONS(11),
    [anon_sym_LBRACK] = ACTIONS(11),
    [anon_sym_type] = ACTIONS(11),
    [anon_sym_std] = ACTIONS(13),
    [anon_sym_description] = ACTIONS(11),
    [anon_sym_optimize] = ACTIONS(11),
    [anon_sym_debug_DASHinfo] = ACTIONS(11),
    [anon_sym_runtime_DASHlink] = ACTIONS(11),
    [anon_sym_libc] = ACTIONS(11),
    [anon_sym_stdlib] = ACTIONS(11),
    [anon_sym_lto] = ACTIONS(11),
    [anon_sym_warnings] = ACTIONS(13),
    [anon_sym_warnings_DASHas_DASHerrors] = ACTIONS(11),
    [anon_sym_android_DASHapi_DASHlevel] = ACTIONS(11),
    [anon_sym_macos_DASHmin] = ACTIONS(11),
    [anon_sym_ios_DASHmin] = ACTIONS(11),
    [anon_sym_tvos_DASHmin] = ACTIONS(11),
    [anon_sym_watchos_DASHmin] = ACTIONS(11),
    [anon_sym_manifest] = ACTIONS(11),
    [anon_sym_sources] = ACTIONS(11),
    [anon_sym_modules] = ACTIONS(11),
    [anon_sym_include] = ACTIONS(11),
    [anon_sym_exclude] = ACTIONS(11),
    [anon_sym_export_DASHmap] = ACTIONS(11),
    [anon_sym_headers] = ACTIONS(11),
    [anon_sym_public] = ACTIONS(11),
    [anon_sym_private] = ACTIONS(11),
    [anon_sym_defines] = ACTIONS(11),
    [anon_sym_links] = ACTIONS(11),
    [anon_sym_frameworks] = ACTIONS(11),
    [anon_sym_framework_DASHpaths] = ACTIONS(11),
    [anon_sym_compile_DASHflags] = ACTIONS(11),
    [anon_sym_link_DASHflags] = ACTIONS(11),
    [anon_sym_resources] = ACTIONS(11),
    [anon_sym_pre_DASHbuild] = ACTIONS(11),
    [anon_sym_post_DASHbuild] = ACTIONS(11),
    [anon_sym_emscripten_DASHflags] = ACTIONS(11),
    [anon_sym_copy] = ACTIONS(11),
    [anon_sym_from] = ACTIONS(11),
    [anon_sym_to] = ACTIONS(13),
    [anon_sym_unity_DASHbuild] = ACTIONS(11),
    [anon_sym_pch] = ACTIONS(11),
    [anon_sym_header] = ACTIONS(13),
    [anon_sym_source] = ACTIONS(13),
    [anon_sym_output] = ACTIONS(11),
    [anon_sym_binaries] = ACTIONS(11),
    [anon_sym_libraries] = ACTIONS(11),
    [anon_sym_symbols] = ACTIONS(11),
    [anon_sym_dependencies] = ACTIONS(11),
    [anon_sym_COMMA] = ACTIONS(11),
    [anon_sym_git] = ACTIONS(11),
    [anon_sym_tag] = ACTIONS(11),
    [anon_sym_commit] = ACTIONS(11),
    [anon_sym_path] = ACTIONS(11),
    [anon_sym_version] = ACTIONS(11),
    [anon_sym_option] = ACTIONS(11),
    [anon_sym_assembler] = ACTIONS(11),
    [anon_sym_tool] = ACTIONS(11),
    [anon_sym_format] = ACTIONS(11),
    [anon_sym_flags] = ACTIONS(11),
    [anon_sym_name] = ACTIONS(11),
    [anon_sym_license] = ACTIONS(11),
    [anon_sym_homepage] = ACTIONS(11),
    [anon_sym_authors] = ACTIONS(11),
    [anon_sym_exports] = ACTIONS(11),
  },
  [3] = {
    [sym_comment] = ACTIONS(3),
    [sym_block_comment] = ACTIONS(3),
    [anon_sym_DQUOTE] = ACTIONS(15),
    [anon_sym_RBRACE2] = ACTIONS(15),
    [anon_sym_LBRACK] = ACTIONS(15),
    [anon_sym_type] = ACTIONS(15),
    [anon_sym_std] = ACTIONS(17),
    [anon_sym_description] = ACTIONS(15),
    [anon_sym_optimize] = ACTIONS(15),
    [anon_sym_debug_DASHinfo] = ACTIONS(15),
    [anon_sym_runtime_DASHlink] = ACTIONS(15),
    [anon_sym_libc] = ACTIONS(15),
    [anon_sym_stdlib] = ACTIONS(15),
    [anon_sym_lto] = ACTIONS(15),
    [anon_sym_warnings] = ACTIONS(17),
    [anon_sym_warnings_DASHas_DASHerrors] = ACTIONS(15),
    [anon_sym_android_DASHapi_DASHlevel] = ACTIONS(15),
    [anon_sym_macos_DASHmin] = ACTIONS(15),
    [anon_sym_ios_DASHmin] = ACTIONS(15),
    [anon_sym_tvos_DASHmin] = ACTIONS(15),
    [anon_sym_watchos_DASHmin] = ACTIONS(15),
    [anon_sym_manifest] = ACTIONS(15),
    [anon_sym_sources] = ACTIONS(15),
    [anon_sym_modules] = ACTIONS(15),
    [anon_sym_include] = ACTIONS(15),
    [anon_sym_exclude] = ACTIONS(15),
    [anon_sym_export_DASHmap] = ACTIONS(15),
    [anon_sym_headers] = ACTIONS(15),
    [anon_sym_public] = ACTIONS(15),
    [anon_sym_private] = ACTIONS(15),
    [anon_sym_defines] = ACTIONS(15),
    [anon_sym_links] = ACTIONS(15),
    [anon_sym_frameworks] = ACTIONS(15),
    [anon_sym_framework_DASHpaths] = ACTIONS(15),
    [anon_sym_compile_DASHflags] = ACTIONS(15),
    [anon_sym_link_DASHflags] = ACTIONS(15),
    [anon_sym_resources] = ACTIONS(15),
    [anon_sym_pre_DASHbuild] = ACTIONS(15),
    [anon_sym_post_DASHbuild] = ACTIONS(15),
    [anon_sym_emscripten_DASHflags] = ACTIONS(15),
    [anon_sym_copy] = ACTIONS(15),
    [anon_sym_from] = ACTIONS(15),
    [anon_sym_to] = ACTIONS(17),
    [anon_sym_unity_DASHbuild] = ACTIONS(15),
    [anon_sym_pch] = ACTIONS(15),
    [anon_sym_header] = ACTIONS(17),
    [anon_sym_source] = ACTIONS(17),
    [anon_sym_output] = ACTIONS(15),
    [anon_sym_binaries] = ACTIONS(15),
    [anon_sym_libraries] = ACTIONS(15),
    [anon_sym_symbols] = ACTIONS(15),
    [anon_sym_dependencies] = ACTIONS(15),
    [anon_sym_COMMA] = ACTIONS(15),
    [anon_sym_git] = ACTIONS(15),
    [anon_sym_tag] = ACTIONS(15),
    [anon_sym_commit] = ACTIONS(15),
    [anon_sym_path] = ACTIONS(15),
    [anon_sym_version] = ACTIONS(15),
    [anon_sym_option] = ACTIONS(15),
    [anon_sym_assembler] = ACTIONS(15),
    [anon_sym_tool] = ACTIONS(15),
    [anon_sym_format] = ACTIONS(15),
    [anon_sym_flags] = ACTIONS(15),
    [anon_sym_name] = ACTIONS(15),
    [anon_sym_license] = ACTIONS(15),
    [anon_sym_homepage] = ACTIONS(15),
    [anon_sym_authors] = ACTIONS(15),
    [anon_sym_exports] = ACTIONS(15),
  },
};

static const uint16_t ts_small_parse_table[] = {
  [0] = 19,
    ACTIONS(19), 1,
      anon_sym_RBRACE2,
    ACTIONS(21), 1,
      anon_sym_LBRACK,
    ACTIONS(29), 1,
      anon_sym_headers,
    ACTIONS(33), 1,
      anon_sym_copy,
    ACTIONS(35), 1,
      anon_sym_unity_DASHbuild,
    ACTIONS(37), 1,
      anon_sym_pch,
    ACTIONS(39), 1,
      anon_sym_output,
    ACTIONS(41), 1,
      anon_sym_dependencies,
    ACTIONS(43), 1,
      anon_sym_option,
    ACTIONS(45), 1,
      anon_sym_assembler,
    STATE(188), 1,
      sym__prop_keyword,
    STATE(194), 1,
      sym__string_list_keyword,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(25), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(27), 2,
      anon_sym_sources,
      anon_sym_modules,
    STATE(6), 2,
      sym_project_item,
      aux_sym_project_decl_repeat1,
    ACTIONS(31), 10,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
    STATE(30), 12,
      sym_condition_block,
      sym_property_stmt,
      sym_sources_block,
      sym_headers_block,
      sym_string_list_block,
      sym_copy_block,
      sym_unity_build_block,
      sym_pch_block,
      sym_output_block,
      sym_dependencies_block,
      sym_assembler_block,
      sym_option_block,
    ACTIONS(23), 15,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
  [96] = 19,
    ACTIONS(21), 1,
      anon_sym_LBRACK,
    ACTIONS(29), 1,
      anon_sym_headers,
    ACTIONS(33), 1,
      anon_sym_copy,
    ACTIONS(35), 1,
      anon_sym_unity_DASHbuild,
    ACTIONS(37), 1,
      anon_sym_pch,
    ACTIONS(39), 1,
      anon_sym_output,
    ACTIONS(41), 1,
      anon_sym_dependencies,
    ACTIONS(43), 1,
      anon_sym_option,
    ACTIONS(45), 1,
      anon_sym_assembler,
    ACTIONS(47), 1,
      anon_sym_RBRACE2,
    STATE(188), 1,
      sym__prop_keyword,
    STATE(194), 1,
      sym__string_list_keyword,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(25), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(27), 2,
      anon_sym_sources,
      anon_sym_modules,
    STATE(6), 2,
      sym_project_item,
      aux_sym_project_decl_repeat1,
    ACTIONS(31), 10,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
    STATE(30), 12,
      sym_condition_block,
      sym_property_stmt,
      sym_sources_block,
      sym_headers_block,
      sym_string_list_block,
      sym_copy_block,
      sym_unity_build_block,
      sym_pch_block,
      sym_output_block,
      sym_dependencies_block,
      sym_assembler_block,
      sym_option_block,
    ACTIONS(23), 15,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
  [192] = 19,
    ACTIONS(49), 1,
      anon_sym_RBRACE2,
    ACTIONS(51), 1,
      anon_sym_LBRACK,
    ACTIONS(63), 1,
      anon_sym_headers,
    ACTIONS(69), 1,
      anon_sym_copy,
    ACTIONS(72), 1,
      anon_sym_unity_DASHbuild,
    ACTIONS(75), 1,
      anon_sym_pch,
    ACTIONS(78), 1,
      anon_sym_output,
    ACTIONS(81), 1,
      anon_sym_dependencies,
    ACTIONS(84), 1,
      anon_sym_option,
    ACTIONS(87), 1,
      anon_sym_assembler,
    STATE(188), 1,
      sym__prop_keyword,
    STATE(194), 1,
      sym__string_list_keyword,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(57), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(60), 2,
      anon_sym_sources,
      anon_sym_modules,
    STATE(6), 2,
      sym_project_item,
      aux_sym_project_decl_repeat1,
    ACTIONS(66), 10,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
    STATE(30), 12,
      sym_condition_block,
      sym_property_stmt,
      sym_sources_block,
      sym_headers_block,
      sym_string_list_block,
      sym_copy_block,
      sym_unity_build_block,
      sym_pch_block,
      sym_output_block,
      sym_dependencies_block,
      sym_assembler_block,
      sym_option_block,
    ACTIONS(54), 15,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
  [288] = 19,
    ACTIONS(21), 1,
      anon_sym_LBRACK,
    ACTIONS(29), 1,
      anon_sym_headers,
    ACTIONS(33), 1,
      anon_sym_copy,
    ACTIONS(35), 1,
      anon_sym_unity_DASHbuild,
    ACTIONS(37), 1,
      anon_sym_pch,
    ACTIONS(39), 1,
      anon_sym_output,
    ACTIONS(41), 1,
      anon_sym_dependencies,
    ACTIONS(43), 1,
      anon_sym_option,
    ACTIONS(45), 1,
      anon_sym_assembler,
    ACTIONS(90), 1,
      anon_sym_RBRACE2,
    STATE(188), 1,
      sym__prop_keyword,
    STATE(194), 1,
      sym__string_list_keyword,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(25), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(27), 2,
      anon_sym_sources,
      anon_sym_modules,
    STATE(4), 2,
      sym_project_item,
      aux_sym_project_decl_repeat1,
    ACTIONS(31), 10,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
    STATE(30), 12,
      sym_condition_block,
      sym_property_stmt,
      sym_sources_block,
      sym_headers_block,
      sym_string_list_block,
      sym_copy_block,
      sym_unity_build_block,
      sym_pch_block,
      sym_output_block,
      sym_dependencies_block,
      sym_assembler_block,
      sym_option_block,
    ACTIONS(23), 15,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
  [384] = 19,
    ACTIONS(21), 1,
      anon_sym_LBRACK,
    ACTIONS(29), 1,
      anon_sym_headers,
    ACTIONS(33), 1,
      anon_sym_copy,
    ACTIONS(35), 1,
      anon_sym_unity_DASHbuild,
    ACTIONS(37), 1,
      anon_sym_pch,
    ACTIONS(39), 1,
      anon_sym_output,
    ACTIONS(41), 1,
      anon_sym_dependencies,
    ACTIONS(43), 1,
      anon_sym_option,
    ACTIONS(45), 1,
      anon_sym_assembler,
    ACTIONS(92), 1,
      anon_sym_RBRACE2,
    STATE(188), 1,
      sym__prop_keyword,
    STATE(194), 1,
      sym__string_list_keyword,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(25), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(27), 2,
      anon_sym_sources,
      anon_sym_modules,
    STATE(5), 2,
      sym_project_item,
      aux_sym_project_decl_repeat1,
    ACTIONS(31), 10,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
    STATE(30), 12,
      sym_condition_block,
      sym_property_stmt,
      sym_sources_block,
      sym_headers_block,
      sym_string_list_block,
      sym_copy_block,
      sym_unity_build_block,
      sym_pch_block,
      sym_output_block,
      sym_dependencies_block,
      sym_assembler_block,
      sym_option_block,
    ACTIONS(23), 15,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
  [480] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(96), 4,
      anon_sym_std,
      anon_sym_warnings,
      anon_sym_header,
      anon_sym_source,
    ACTIONS(94), 43,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_export_DASHmap,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_enabled,
      anon_sym_batch_DASHsize,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
      anon_sym_default,
  [536] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(100), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(98), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [584] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(104), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(102), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [632] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(108), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(106), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [680] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(112), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(110), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [728] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(116), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(114), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [776] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(120), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(118), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [824] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(124), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(122), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [872] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(128), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(126), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [920] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(132), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(130), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [968] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(136), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(134), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1016] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(140), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(138), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1064] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(144), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(142), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1112] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(148), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(146), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1160] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(152), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(150), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1208] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(156), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(154), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1256] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(160), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(158), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1304] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(164), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(162), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1352] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(168), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(166), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1400] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(172), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(170), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1448] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(176), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(174), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1496] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(180), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(178), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1544] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(184), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(182), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1592] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(188), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(186), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1640] = 3,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(192), 2,
      anon_sym_std,
      anon_sym_warnings,
    ACTIONS(190), 37,
      anon_sym_RBRACE2,
      anon_sym_LBRACK,
      anon_sym_type,
      anon_sym_description,
      anon_sym_optimize,
      anon_sym_debug_DASHinfo,
      anon_sym_runtime_DASHlink,
      anon_sym_libc,
      anon_sym_stdlib,
      anon_sym_lto,
      anon_sym_warnings_DASHas_DASHerrors,
      anon_sym_android_DASHapi_DASHlevel,
      anon_sym_macos_DASHmin,
      anon_sym_ios_DASHmin,
      anon_sym_tvos_DASHmin,
      anon_sym_watchos_DASHmin,
      anon_sym_manifest,
      anon_sym_sources,
      anon_sym_modules,
      anon_sym_headers,
      anon_sym_defines,
      anon_sym_links,
      anon_sym_frameworks,
      anon_sym_framework_DASHpaths,
      anon_sym_compile_DASHflags,
      anon_sym_link_DASHflags,
      anon_sym_resources,
      anon_sym_pre_DASHbuild,
      anon_sym_post_DASHbuild,
      anon_sym_emscripten_DASHflags,
      anon_sym_copy,
      anon_sym_unity_DASHbuild,
      anon_sym_pch,
      anon_sym_output,
      anon_sym_dependencies,
      anon_sym_option,
      anon_sym_assembler,
  [1688] = 7,
    ACTIONS(194), 1,
      anon_sym_RBRACE2,
    ACTIONS(198), 1,
      anon_sym_authors,
    ACTIONS(200), 1,
      anon_sym_exports,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(36), 2,
      sym_package_item,
      aux_sym_package_decl_repeat1,
    STATE(45), 2,
      sym_authors_block,
      sym_exports_block,
    ACTIONS(196), 5,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
  [1717] = 7,
    ACTIONS(198), 1,
      anon_sym_authors,
    ACTIONS(200), 1,
      anon_sym_exports,
    ACTIONS(202), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(34), 2,
      sym_package_item,
      aux_sym_package_decl_repeat1,
    STATE(45), 2,
      sym_authors_block,
      sym_exports_block,
    ACTIONS(196), 5,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
  [1746] = 7,
    ACTIONS(204), 1,
      anon_sym_RBRACE2,
    ACTIONS(209), 1,
      anon_sym_authors,
    ACTIONS(212), 1,
      anon_sym_exports,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(36), 2,
      sym_package_item,
      aux_sym_package_decl_repeat1,
    STATE(45), 2,
      sym_authors_block,
      sym_exports_block,
    ACTIONS(206), 5,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
  [1775] = 6,
    ACTIONS(215), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(217), 2,
      anon_sym_include,
      anon_sym_exclude,
    ACTIONS(219), 2,
      anon_sym_defines,
      anon_sym_flags,
    ACTIONS(221), 2,
      anon_sym_tool,
      anon_sym_format,
    STATE(40), 2,
      sym_assembler_item,
      aux_sym_assembler_block_repeat1,
  [1799] = 6,
    ACTIONS(223), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(217), 2,
      anon_sym_include,
      anon_sym_exclude,
    ACTIONS(219), 2,
      anon_sym_defines,
      anon_sym_flags,
    ACTIONS(221), 2,
      anon_sym_tool,
      anon_sym_format,
    STATE(37), 2,
      sym_assembler_item,
      aux_sym_assembler_block_repeat1,
  [1823] = 6,
    ACTIONS(225), 1,
      anon_sym_RBRACE2,
    ACTIONS(229), 1,
      anon_sym_option,
    STATE(41), 1,
      aux_sym_dep_object_repeat1,
    STATE(46), 1,
      sym_dep_field,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(227), 5,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
  [1847] = 6,
    ACTIONS(231), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(233), 2,
      anon_sym_include,
      anon_sym_exclude,
    ACTIONS(236), 2,
      anon_sym_defines,
      anon_sym_flags,
    ACTIONS(239), 2,
      anon_sym_tool,
      anon_sym_format,
    STATE(40), 2,
      sym_assembler_item,
      aux_sym_assembler_block_repeat1,
  [1871] = 6,
    ACTIONS(229), 1,
      anon_sym_option,
    ACTIONS(242), 1,
      anon_sym_RBRACE2,
    STATE(42), 1,
      aux_sym_dep_object_repeat1,
    STATE(46), 1,
      sym_dep_field,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(227), 5,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
  [1895] = 6,
    ACTIONS(244), 1,
      anon_sym_RBRACE2,
    ACTIONS(249), 1,
      anon_sym_option,
    STATE(42), 1,
      aux_sym_dep_object_repeat1,
    STATE(46), 1,
      sym_dep_field,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(246), 5,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
  [1919] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(252), 8,
      anon_sym_RBRACE2,
      anon_sym_COMMA,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [1934] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(254), 8,
      anon_sym_RBRACE2,
      anon_sym_COMMA,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [1949] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(256), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [1964] = 3,
    ACTIONS(260), 1,
      anon_sym_COMMA,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(258), 7,
      anon_sym_RBRACE2,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [1981] = 6,
    ACTIONS(262), 1,
      sym_identifier,
    ACTIONS(264), 1,
      sym_integer,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(266), 2,
      anon_sym_true,
      anon_sym_false,
    STATE(18), 3,
      sym_boolean,
      sym_string,
      sym__prop_value,
  [2004] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(270), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [2019] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(272), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [2034] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(274), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [2049] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(276), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [2064] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(278), 8,
      anon_sym_RBRACE2,
      anon_sym_COMMA,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [2079] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(280), 8,
      anon_sym_RBRACE2,
      anon_sym_description,
      anon_sym_version,
      anon_sym_name,
      anon_sym_license,
      anon_sym_homepage,
      anon_sym_authors,
      anon_sym_exports,
  [2094] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(282), 8,
      anon_sym_RBRACE2,
      anon_sym_COMMA,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [2109] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(284), 7,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_defines,
      anon_sym_tool,
      anon_sym_format,
      anon_sym_flags,
  [2123] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(286), 7,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_defines,
      anon_sym_tool,
      anon_sym_format,
      anon_sym_flags,
  [2137] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(288), 7,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_defines,
      anon_sym_tool,
      anon_sym_format,
      anon_sym_flags,
  [2151] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(290), 7,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_defines,
      anon_sym_tool,
      anon_sym_format,
      anon_sym_flags,
  [2165] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(244), 7,
      anon_sym_RBRACE2,
      anon_sym_git,
      anon_sym_tag,
      anon_sym_commit,
      anon_sym_path,
      anon_sym_version,
      anon_sym_option,
  [2179] = 5,
    ACTIONS(292), 1,
      anon_sym_RBRACE2,
    ACTIONS(296), 1,
      anon_sym_export_DASHmap,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(294), 2,
      anon_sym_include,
      anon_sym_exclude,
    STATE(68), 2,
      sym_sources_item,
      aux_sym_sources_block_repeat1,
  [2198] = 6,
    ACTIONS(298), 1,
      anon_sym_RBRACE2,
    ACTIONS(300), 1,
      anon_sym_defines,
    ACTIONS(302), 1,
      anon_sym_dependencies,
    ACTIONS(304), 1,
      anon_sym_default,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(67), 2,
      sym_option_field,
      aux_sym_option_block_repeat1,
  [2219] = 5,
    ACTIONS(296), 1,
      anon_sym_export_DASHmap,
    ACTIONS(306), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(294), 2,
      anon_sym_include,
      anon_sym_exclude,
    STATE(60), 2,
      sym_sources_item,
      aux_sym_sources_block_repeat1,
  [2238] = 6,
    ACTIONS(310), 1,
      anon_sym_DQUOTE,
    ACTIONS(312), 1,
      sym_string_content,
    ACTIONS(314), 1,
      sym_escape_sequence,
    ACTIONS(316), 1,
      anon_sym_DOLLAR_LBRACE,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    STATE(69), 2,
      sym_env_interpolation,
      aux_sym_string_repeat1,
  [2259] = 6,
    ACTIONS(318), 1,
      anon_sym_RBRACE2,
    ACTIONS(320), 1,
      anon_sym_defines,
    ACTIONS(323), 1,
      anon_sym_dependencies,
    ACTIONS(326), 1,
      anon_sym_default,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(64), 2,
      sym_option_field,
      aux_sym_option_block_repeat1,
  [2280] = 6,
    ACTIONS(329), 1,
      anon_sym_DQUOTE,
    ACTIONS(331), 1,
      sym_string_content,
    ACTIONS(334), 1,
      sym_escape_sequence,
    ACTIONS(337), 1,
      anon_sym_DOLLAR_LBRACE,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    STATE(65), 2,
      sym_env_interpolation,
      aux_sym_string_repeat1,
  [2301] = 6,
    ACTIONS(316), 1,
      anon_sym_DOLLAR_LBRACE,
    ACTIONS(340), 1,
      anon_sym_DQUOTE,
    ACTIONS(342), 1,
      sym_string_content,
    ACTIONS(344), 1,
      sym_escape_sequence,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    STATE(70), 2,
      sym_env_interpolation,
      aux_sym_string_repeat1,
  [2322] = 6,
    ACTIONS(300), 1,
      anon_sym_defines,
    ACTIONS(302), 1,
      anon_sym_dependencies,
    ACTIONS(304), 1,
      anon_sym_default,
    ACTIONS(346), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(64), 2,
      sym_option_field,
      aux_sym_option_block_repeat1,
  [2343] = 5,
    ACTIONS(348), 1,
      anon_sym_RBRACE2,
    ACTIONS(353), 1,
      anon_sym_export_DASHmap,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(350), 2,
      anon_sym_include,
      anon_sym_exclude,
    STATE(68), 2,
      sym_sources_item,
      aux_sym_sources_block_repeat1,
  [2362] = 6,
    ACTIONS(316), 1,
      anon_sym_DOLLAR_LBRACE,
    ACTIONS(356), 1,
      anon_sym_DQUOTE,
    ACTIONS(358), 1,
      sym_string_content,
    ACTIONS(360), 1,
      sym_escape_sequence,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    STATE(65), 2,
      sym_env_interpolation,
      aux_sym_string_repeat1,
  [2383] = 6,
    ACTIONS(316), 1,
      anon_sym_DOLLAR_LBRACE,
    ACTIONS(358), 1,
      sym_string_content,
    ACTIONS(360), 1,
      sym_escape_sequence,
    ACTIONS(362), 1,
      anon_sym_DQUOTE,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    STATE(65), 2,
      sym_env_interpolation,
      aux_sym_string_repeat1,
  [2404] = 6,
    ACTIONS(364), 1,
      anon_sym_RBRACE2,
    ACTIONS(366), 1,
      anon_sym_exclude,
    ACTIONS(368), 1,
      anon_sym_enabled,
    ACTIONS(370), 1,
      anon_sym_batch_DASHsize,
    STATE(79), 1,
      aux_sym_unity_build_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2424] = 6,
    ACTIONS(372), 1,
      anon_sym_RBRACE2,
    ACTIONS(374), 1,
      anon_sym_binaries,
    ACTIONS(376), 1,
      anon_sym_libraries,
    ACTIONS(378), 1,
      anon_sym_symbols,
    STATE(81), 1,
      aux_sym_output_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2444] = 5,
    ACTIONS(380), 1,
      sym_identifier,
    ACTIONS(382), 1,
      anon_sym_RBRACE2,
    STATE(210), 1,
      sym_dep_name,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(84), 2,
      sym_dep_item,
      aux_sym_dependencies_block_repeat1,
  [2462] = 4,
    ACTIONS(384), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(386), 2,
      anon_sym_public,
      anon_sym_private,
    STATE(74), 2,
      sym_header_item,
      aux_sym_headers_block_repeat1,
  [2478] = 5,
    ACTIONS(380), 1,
      sym_identifier,
    ACTIONS(389), 1,
      anon_sym_RBRACE2,
    STATE(210), 1,
      sym_dep_name,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(80), 2,
      sym_dep_item,
      aux_sym_dependencies_block_repeat1,
  [2496] = 6,
    ACTIONS(374), 1,
      anon_sym_binaries,
    ACTIONS(376), 1,
      anon_sym_libraries,
    ACTIONS(378), 1,
      anon_sym_symbols,
    ACTIONS(391), 1,
      anon_sym_RBRACE2,
    STATE(72), 1,
      aux_sym_output_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2516] = 6,
    ACTIONS(393), 1,
      anon_sym_RBRACE2,
    ACTIONS(395), 1,
      anon_sym_modules,
    ACTIONS(397), 1,
      anon_sym_header,
    ACTIONS(399), 1,
      anon_sym_source,
    STATE(83), 1,
      aux_sym_pch_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2536] = 6,
    ACTIONS(401), 1,
      anon_sym_RBRACE2,
    ACTIONS(403), 1,
      anon_sym_exclude,
    ACTIONS(406), 1,
      anon_sym_enabled,
    ACTIONS(409), 1,
      anon_sym_batch_DASHsize,
    STATE(78), 1,
      aux_sym_unity_build_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2556] = 6,
    ACTIONS(366), 1,
      anon_sym_exclude,
    ACTIONS(368), 1,
      anon_sym_enabled,
    ACTIONS(370), 1,
      anon_sym_batch_DASHsize,
    ACTIONS(412), 1,
      anon_sym_RBRACE2,
    STATE(78), 1,
      aux_sym_unity_build_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2576] = 5,
    ACTIONS(380), 1,
      sym_identifier,
    ACTIONS(414), 1,
      anon_sym_RBRACE2,
    STATE(210), 1,
      sym_dep_name,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(84), 2,
      sym_dep_item,
      aux_sym_dependencies_block_repeat1,
  [2594] = 6,
    ACTIONS(416), 1,
      anon_sym_RBRACE2,
    ACTIONS(418), 1,
      anon_sym_binaries,
    ACTIONS(421), 1,
      anon_sym_libraries,
    ACTIONS(424), 1,
      anon_sym_symbols,
    STATE(81), 1,
      aux_sym_output_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2614] = 5,
    ACTIONS(427), 1,
      anon_sym_DQUOTE,
    ACTIONS(429), 1,
      anon_sym_LBRACE,
    STATE(139), 1,
      sym_dep_value,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(138), 2,
      sym_string,
      sym_dep_object,
  [2632] = 6,
    ACTIONS(395), 1,
      anon_sym_modules,
    ACTIONS(397), 1,
      anon_sym_header,
    ACTIONS(399), 1,
      anon_sym_source,
    ACTIONS(431), 1,
      anon_sym_RBRACE2,
    STATE(88), 1,
      aux_sym_pch_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2652] = 5,
    ACTIONS(433), 1,
      sym_identifier,
    ACTIONS(436), 1,
      anon_sym_RBRACE2,
    STATE(210), 1,
      sym_dep_name,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(84), 2,
      sym_dep_item,
      aux_sym_dependencies_block_repeat1,
  [2670] = 4,
    ACTIONS(438), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(440), 2,
      anon_sym_public,
      anon_sym_private,
    STATE(87), 2,
      sym_header_item,
      aux_sym_headers_block_repeat1,
  [2686] = 5,
    ACTIONS(380), 1,
      sym_identifier,
    ACTIONS(442), 1,
      anon_sym_RBRACE2,
    STATE(210), 1,
      sym_dep_name,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(73), 2,
      sym_dep_item,
      aux_sym_dependencies_block_repeat1,
  [2704] = 4,
    ACTIONS(444), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(440), 2,
      anon_sym_public,
      anon_sym_private,
    STATE(74), 2,
      sym_header_item,
      aux_sym_headers_block_repeat1,
  [2720] = 6,
    ACTIONS(446), 1,
      anon_sym_RBRACE2,
    ACTIONS(448), 1,
      anon_sym_modules,
    ACTIONS(451), 1,
      anon_sym_header,
    ACTIONS(454), 1,
      anon_sym_source,
    STATE(88), 1,
      aux_sym_pch_block_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [2740] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(457), 4,
      anon_sym_RBRACE2,
      anon_sym_defines,
      anon_sym_dependencies,
      anon_sym_default,
  [2751] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(459), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2766] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(461), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2781] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(463), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(99), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2796] = 4,
    ACTIONS(465), 1,
      anon_sym_DQUOTE,
    ACTIONS(468), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2811] = 4,
    ACTIONS(470), 1,
      sym_identifier,
    ACTIONS(473), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(94), 2,
      sym_export_entry,
      aux_sym_exports_block_repeat1,
  [2826] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(475), 4,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_export_DASHmap,
  [2837] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(477), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(91), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2852] = 4,
    ACTIONS(479), 1,
      anon_sym_RBRACE2,
    ACTIONS(481), 1,
      anon_sym_from,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(97), 2,
      sym_copy_item,
      aux_sym_copy_block_repeat1,
  [2867] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(484), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(105), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2882] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(486), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2897] = 4,
    ACTIONS(488), 1,
      sym_identifier,
    ACTIONS(490), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(94), 2,
      sym_export_entry,
      aux_sym_exports_block_repeat1,
  [2912] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(492), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(116), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2927] = 4,
    ACTIONS(488), 1,
      sym_identifier,
    ACTIONS(494), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(100), 2,
      sym_export_entry,
      aux_sym_exports_block_repeat1,
  [2942] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(496), 4,
      anon_sym_RBRACE2,
      anon_sym_include,
      anon_sym_exclude,
      anon_sym_export_DASHmap,
  [2953] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(498), 4,
      anon_sym_RBRACE2,
      anon_sym_exclude,
      anon_sym_enabled,
      anon_sym_batch_DASHsize,
  [2964] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(500), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [2979] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(502), 4,
      anon_sym_RBRACE2,
      anon_sym_exclude,
      anon_sym_enabled,
      anon_sym_batch_DASHsize,
  [2990] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(504), 4,
      anon_sym_RBRACE2,
      anon_sym_exclude,
      anon_sym_enabled,
      anon_sym_batch_DASHsize,
  [3001] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(506), 4,
      anon_sym_RBRACE2,
      anon_sym_modules,
      anon_sym_header,
      anon_sym_source,
  [3012] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(508), 4,
      anon_sym_RBRACE2,
      anon_sym_modules,
      anon_sym_header,
      anon_sym_source,
  [3023] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(510), 4,
      anon_sym_RBRACE2,
      anon_sym_modules,
      anon_sym_header,
      anon_sym_source,
  [3034] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(512), 4,
      anon_sym_RBRACE2,
      anon_sym_binaries,
      anon_sym_libraries,
      anon_sym_symbols,
  [3045] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(514), 4,
      anon_sym_RBRACE2,
      anon_sym_binaries,
      anon_sym_libraries,
      anon_sym_symbols,
  [3056] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(516), 4,
      anon_sym_RBRACE2,
      anon_sym_binaries,
      anon_sym_libraries,
      anon_sym_symbols,
  [3067] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(389), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(120), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [3082] = 4,
    ACTIONS(518), 1,
      anon_sym_RBRACE2,
    ACTIONS(520), 1,
      anon_sym_from,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(97), 2,
      sym_copy_item,
      aux_sym_copy_block_repeat1,
  [3097] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(522), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [3112] = 3,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(524), 2,
      anon_sym_DQUOTE,
      sym_string_content,
    ACTIONS(526), 2,
      sym_escape_sequence,
      anon_sym_DOLLAR_LBRACE,
  [3125] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(528), 4,
      anon_sym_RBRACE2,
      anon_sym_exclude,
      anon_sym_enabled,
      anon_sym_batch_DASHsize,
  [3136] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(530), 4,
      anon_sym_RBRACE2,
      anon_sym_defines,
      anon_sym_dependencies,
      anon_sym_default,
  [3147] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(414), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(93), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [3162] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(532), 4,
      anon_sym_RBRACE2,
      anon_sym_defines,
      anon_sym_dependencies,
      anon_sym_default,
  [3173] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(534), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(90), 2,
      sym_string,
      aux_sym_string_list_block_repeat1,
  [3188] = 4,
    ACTIONS(520), 1,
      anon_sym_from,
    ACTIONS(536), 1,
      anon_sym_RBRACE2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    STATE(115), 2,
      sym_copy_item,
      aux_sym_copy_block_repeat1,
  [3203] = 4,
    ACTIONS(538), 1,
      anon_sym_RBRACK,
    ACTIONS(540), 1,
      anon_sym_DOT,
    STATE(130), 1,
      aux_sym_condition_expr_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3217] = 4,
    ACTIONS(542), 1,
      anon_sym_RBRACK,
    ACTIONS(544), 1,
      anon_sym_DOT,
    STATE(125), 1,
      aux_sym_condition_expr_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3231] = 3,
    STATE(106), 1,
      sym_boolean,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(547), 2,
      anon_sym_true,
      anon_sym_false,
  [3243] = 3,
    STATE(108), 1,
      sym_boolean,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(547), 2,
      anon_sym_true,
      anon_sym_false,
  [3255] = 3,
    STATE(89), 1,
      sym_boolean,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(547), 2,
      anon_sym_true,
      anon_sym_false,
  [3267] = 3,
    STATE(103), 1,
      sym_boolean,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(547), 2,
      anon_sym_true,
      anon_sym_false,
  [3279] = 4,
    ACTIONS(540), 1,
      anon_sym_DOT,
    ACTIONS(549), 1,
      anon_sym_RBRACK,
    STATE(125), 1,
      aux_sym_condition_expr_repeat1,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3293] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(551), 3,
      anon_sym_RBRACE2,
      anon_sym_public,
      anon_sym_private,
  [3303] = 4,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    ACTIONS(553), 1,
      anon_sym_LBRACE,
    STATE(43), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3317] = 4,
    ACTIONS(9), 1,
      anon_sym_package,
    ACTIONS(555), 1,
      ts_builtin_sym_end,
    STATE(190), 1,
      sym_package_decl,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3331] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(112), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3342] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(110), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3353] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(111), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3364] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(171), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3375] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(557), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3384] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(559), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3393] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(56), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3404] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(113), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3415] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(145), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3426] = 3,
    ACTIONS(427), 1,
      anon_sym_DQUOTE,
    STATE(154), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3437] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(561), 2,
      ts_builtin_sym_end,
      anon_sym_package,
  [3446] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(563), 2,
      anon_sym_RBRACE2,
      anon_sym_from,
  [3455] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(565), 2,
      ts_builtin_sym_end,
      anon_sym_package,
  [3464] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(567), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3473] = 3,
    ACTIONS(569), 1,
      sym_identifier,
    STATE(173), 1,
      sym_condition_expr,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3484] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(95), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3495] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(52), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3506] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(542), 2,
      anon_sym_RBRACK,
      anon_sym_DOT,
  [3515] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(571), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3524] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(131), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3535] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(573), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3544] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(48), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3555] = 3,
    ACTIONS(268), 1,
      anon_sym_DQUOTE,
    STATE(109), 1,
      sym_string,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3566] = 3,
    ACTIONS(575), 1,
      anon_sym_EQ,
    ACTIONS(577), 1,
      anon_sym_SLASH,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3577] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(11), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3586] = 2,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
    ACTIONS(15), 2,
      sym_identifier,
      anon_sym_RBRACE2,
  [3595] = 2,
    ACTIONS(579), 1,
      sym_identifier,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3603] = 2,
    ACTIONS(581), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3611] = 2,
    ACTIONS(583), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3619] = 2,
    ACTIONS(585), 1,
      ts_builtin_sym_end,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3627] = 2,
    ACTIONS(587), 1,
      ts_builtin_sym_end,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3635] = 2,
    ACTIONS(589), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3643] = 2,
    ACTIONS(591), 1,
      sym_identifier,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3651] = 2,
    ACTIONS(593), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3659] = 2,
    ACTIONS(595), 1,
      sym_identifier,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3667] = 2,
    ACTIONS(597), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3675] = 2,
    ACTIONS(599), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3683] = 2,
    ACTIONS(601), 1,
      anon_sym_to,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3691] = 2,
    ACTIONS(603), 1,
      sym_identifier,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3699] = 2,
    ACTIONS(605), 1,
      anon_sym_RBRACK,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3707] = 2,
    ACTIONS(607), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3715] = 2,
    ACTIONS(609), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3723] = 2,
    ACTIONS(611), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3731] = 2,
    ACTIONS(613), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3739] = 2,
    ACTIONS(615), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3747] = 2,
    ACTIONS(617), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3755] = 2,
    ACTIONS(619), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3763] = 2,
    ACTIONS(621), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3771] = 2,
    ACTIONS(555), 1,
      ts_builtin_sym_end,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3779] = 2,
    ACTIONS(623), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3787] = 2,
    ACTIONS(625), 1,
      ts_builtin_sym_end,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3795] = 2,
    ACTIONS(627), 1,
      sym_identifier,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3803] = 2,
    ACTIONS(629), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3811] = 2,
    ACTIONS(631), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3819] = 2,
    ACTIONS(633), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3827] = 2,
    ACTIONS(635), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3835] = 2,
    ACTIONS(637), 1,
      ts_builtin_sym_end,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3843] = 2,
    ACTIONS(639), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3851] = 2,
    ACTIONS(641), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3859] = 2,
    ACTIONS(643), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3867] = 2,
    ACTIONS(645), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3875] = 2,
    ACTIONS(647), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3883] = 2,
    ACTIONS(649), 1,
      anon_sym_RBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3891] = 2,
    ACTIONS(651), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3899] = 2,
    ACTIONS(653), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3907] = 2,
    ACTIONS(655), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3915] = 2,
    ACTIONS(657), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3923] = 2,
    ACTIONS(659), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3931] = 2,
    ACTIONS(661), 1,
      sym_integer,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3939] = 2,
    ACTIONS(663), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3947] = 2,
    ACTIONS(665), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3955] = 2,
    ACTIONS(667), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3963] = 2,
    ACTIONS(669), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3971] = 2,
    ACTIONS(671), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3979] = 2,
    ACTIONS(673), 1,
      aux_sym_env_interpolation_token1,
    ACTIONS(308), 2,
      sym_comment,
      sym_block_comment,
  [3987] = 2,
    ACTIONS(675), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [3995] = 2,
    ACTIONS(677), 1,
      anon_sym_EQ,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
  [4003] = 2,
    ACTIONS(679), 1,
      anon_sym_LBRACE,
    ACTIONS(3), 2,
      sym_comment,
      sym_block_comment,
};

static const uint32_t ts_small_parse_table_map[] = {
  [SMALL_STATE(4)] = 0,
  [SMALL_STATE(5)] = 96,
  [SMALL_STATE(6)] = 192,
  [SMALL_STATE(7)] = 288,
  [SMALL_STATE(8)] = 384,
  [SMALL_STATE(9)] = 480,
  [SMALL_STATE(10)] = 536,
  [SMALL_STATE(11)] = 584,
  [SMALL_STATE(12)] = 632,
  [SMALL_STATE(13)] = 680,
  [SMALL_STATE(14)] = 728,
  [SMALL_STATE(15)] = 776,
  [SMALL_STATE(16)] = 824,
  [SMALL_STATE(17)] = 872,
  [SMALL_STATE(18)] = 920,
  [SMALL_STATE(19)] = 968,
  [SMALL_STATE(20)] = 1016,
  [SMALL_STATE(21)] = 1064,
  [SMALL_STATE(22)] = 1112,
  [SMALL_STATE(23)] = 1160,
  [SMALL_STATE(24)] = 1208,
  [SMALL_STATE(25)] = 1256,
  [SMALL_STATE(26)] = 1304,
  [SMALL_STATE(27)] = 1352,
  [SMALL_STATE(28)] = 1400,
  [SMALL_STATE(29)] = 1448,
  [SMALL_STATE(30)] = 1496,
  [SMALL_STATE(31)] = 1544,
  [SMALL_STATE(32)] = 1592,
  [SMALL_STATE(33)] = 1640,
  [SMALL_STATE(34)] = 1688,
  [SMALL_STATE(35)] = 1717,
  [SMALL_STATE(36)] = 1746,
  [SMALL_STATE(37)] = 1775,
  [SMALL_STATE(38)] = 1799,
  [SMALL_STATE(39)] = 1823,
  [SMALL_STATE(40)] = 1847,
  [SMALL_STATE(41)] = 1871,
  [SMALL_STATE(42)] = 1895,
  [SMALL_STATE(43)] = 1919,
  [SMALL_STATE(44)] = 1934,
  [SMALL_STATE(45)] = 1949,
  [SMALL_STATE(46)] = 1964,
  [SMALL_STATE(47)] = 1981,
  [SMALL_STATE(48)] = 2004,
  [SMALL_STATE(49)] = 2019,
  [SMALL_STATE(50)] = 2034,
  [SMALL_STATE(51)] = 2049,
  [SMALL_STATE(52)] = 2064,
  [SMALL_STATE(53)] = 2079,
  [SMALL_STATE(54)] = 2094,
  [SMALL_STATE(55)] = 2109,
  [SMALL_STATE(56)] = 2123,
  [SMALL_STATE(57)] = 2137,
  [SMALL_STATE(58)] = 2151,
  [SMALL_STATE(59)] = 2165,
  [SMALL_STATE(60)] = 2179,
  [SMALL_STATE(61)] = 2198,
  [SMALL_STATE(62)] = 2219,
  [SMALL_STATE(63)] = 2238,
  [SMALL_STATE(64)] = 2259,
  [SMALL_STATE(65)] = 2280,
  [SMALL_STATE(66)] = 2301,
  [SMALL_STATE(67)] = 2322,
  [SMALL_STATE(68)] = 2343,
  [SMALL_STATE(69)] = 2362,
  [SMALL_STATE(70)] = 2383,
  [SMALL_STATE(71)] = 2404,
  [SMALL_STATE(72)] = 2424,
  [SMALL_STATE(73)] = 2444,
  [SMALL_STATE(74)] = 2462,
  [SMALL_STATE(75)] = 2478,
  [SMALL_STATE(76)] = 2496,
  [SMALL_STATE(77)] = 2516,
  [SMALL_STATE(78)] = 2536,
  [SMALL_STATE(79)] = 2556,
  [SMALL_STATE(80)] = 2576,
  [SMALL_STATE(81)] = 2594,
  [SMALL_STATE(82)] = 2614,
  [SMALL_STATE(83)] = 2632,
  [SMALL_STATE(84)] = 2652,
  [SMALL_STATE(85)] = 2670,
  [SMALL_STATE(86)] = 2686,
  [SMALL_STATE(87)] = 2704,
  [SMALL_STATE(88)] = 2720,
  [SMALL_STATE(89)] = 2740,
  [SMALL_STATE(90)] = 2751,
  [SMALL_STATE(91)] = 2766,
  [SMALL_STATE(92)] = 2781,
  [SMALL_STATE(93)] = 2796,
  [SMALL_STATE(94)] = 2811,
  [SMALL_STATE(95)] = 2826,
  [SMALL_STATE(96)] = 2837,
  [SMALL_STATE(97)] = 2852,
  [SMALL_STATE(98)] = 2867,
  [SMALL_STATE(99)] = 2882,
  [SMALL_STATE(100)] = 2897,
  [SMALL_STATE(101)] = 2912,
  [SMALL_STATE(102)] = 2927,
  [SMALL_STATE(103)] = 2942,
  [SMALL_STATE(104)] = 2953,
  [SMALL_STATE(105)] = 2964,
  [SMALL_STATE(106)] = 2979,
  [SMALL_STATE(107)] = 2990,
  [SMALL_STATE(108)] = 3001,
  [SMALL_STATE(109)] = 3012,
  [SMALL_STATE(110)] = 3023,
  [SMALL_STATE(111)] = 3034,
  [SMALL_STATE(112)] = 3045,
  [SMALL_STATE(113)] = 3056,
  [SMALL_STATE(114)] = 3067,
  [SMALL_STATE(115)] = 3082,
  [SMALL_STATE(116)] = 3097,
  [SMALL_STATE(117)] = 3112,
  [SMALL_STATE(118)] = 3125,
  [SMALL_STATE(119)] = 3136,
  [SMALL_STATE(120)] = 3147,
  [SMALL_STATE(121)] = 3162,
  [SMALL_STATE(122)] = 3173,
  [SMALL_STATE(123)] = 3188,
  [SMALL_STATE(124)] = 3203,
  [SMALL_STATE(125)] = 3217,
  [SMALL_STATE(126)] = 3231,
  [SMALL_STATE(127)] = 3243,
  [SMALL_STATE(128)] = 3255,
  [SMALL_STATE(129)] = 3267,
  [SMALL_STATE(130)] = 3279,
  [SMALL_STATE(131)] = 3293,
  [SMALL_STATE(132)] = 3303,
  [SMALL_STATE(133)] = 3317,
  [SMALL_STATE(134)] = 3331,
  [SMALL_STATE(135)] = 3342,
  [SMALL_STATE(136)] = 3353,
  [SMALL_STATE(137)] = 3364,
  [SMALL_STATE(138)] = 3375,
  [SMALL_STATE(139)] = 3384,
  [SMALL_STATE(140)] = 3393,
  [SMALL_STATE(141)] = 3404,
  [SMALL_STATE(142)] = 3415,
  [SMALL_STATE(143)] = 3426,
  [SMALL_STATE(144)] = 3437,
  [SMALL_STATE(145)] = 3446,
  [SMALL_STATE(146)] = 3455,
  [SMALL_STATE(147)] = 3464,
  [SMALL_STATE(148)] = 3473,
  [SMALL_STATE(149)] = 3484,
  [SMALL_STATE(150)] = 3495,
  [SMALL_STATE(151)] = 3506,
  [SMALL_STATE(152)] = 3515,
  [SMALL_STATE(153)] = 3524,
  [SMALL_STATE(154)] = 3535,
  [SMALL_STATE(155)] = 3544,
  [SMALL_STATE(156)] = 3555,
  [SMALL_STATE(157)] = 3566,
  [SMALL_STATE(158)] = 3577,
  [SMALL_STATE(159)] = 3586,
  [SMALL_STATE(160)] = 3595,
  [SMALL_STATE(161)] = 3603,
  [SMALL_STATE(162)] = 3611,
  [SMALL_STATE(163)] = 3619,
  [SMALL_STATE(164)] = 3627,
  [SMALL_STATE(165)] = 3635,
  [SMALL_STATE(166)] = 3643,
  [SMALL_STATE(167)] = 3651,
  [SMALL_STATE(168)] = 3659,
  [SMALL_STATE(169)] = 3667,
  [SMALL_STATE(170)] = 3675,
  [SMALL_STATE(171)] = 3683,
  [SMALL_STATE(172)] = 3691,
  [SMALL_STATE(173)] = 3699,
  [SMALL_STATE(174)] = 3707,
  [SMALL_STATE(175)] = 3715,
  [SMALL_STATE(176)] = 3723,
  [SMALL_STATE(177)] = 3731,
  [SMALL_STATE(178)] = 3739,
  [SMALL_STATE(179)] = 3747,
  [SMALL_STATE(180)] = 3755,
  [SMALL_STATE(181)] = 3763,
  [SMALL_STATE(182)] = 3771,
  [SMALL_STATE(183)] = 3779,
  [SMALL_STATE(184)] = 3787,
  [SMALL_STATE(185)] = 3795,
  [SMALL_STATE(186)] = 3803,
  [SMALL_STATE(187)] = 3811,
  [SMALL_STATE(188)] = 3819,
  [SMALL_STATE(189)] = 3827,
  [SMALL_STATE(190)] = 3835,
  [SMALL_STATE(191)] = 3843,
  [SMALL_STATE(192)] = 3851,
  [SMALL_STATE(193)] = 3859,
  [SMALL_STATE(194)] = 3867,
  [SMALL_STATE(195)] = 3875,
  [SMALL_STATE(196)] = 3883,
  [SMALL_STATE(197)] = 3891,
  [SMALL_STATE(198)] = 3899,
  [SMALL_STATE(199)] = 3907,
  [SMALL_STATE(200)] = 3915,
  [SMALL_STATE(201)] = 3923,
  [SMALL_STATE(202)] = 3931,
  [SMALL_STATE(203)] = 3939,
  [SMALL_STATE(204)] = 3947,
  [SMALL_STATE(205)] = 3955,
  [SMALL_STATE(206)] = 3963,
  [SMALL_STATE(207)] = 3971,
  [SMALL_STATE(208)] = 3979,
  [SMALL_STATE(209)] = 3987,
  [SMALL_STATE(210)] = 3995,
  [SMALL_STATE(211)] = 4003,
};

static const TSParseActionEntry ts_parse_actions[] = {
  [0] = {.entry = {.count = 0, .reusable = false}},
  [1] = {.entry = {.count = 1, .reusable = false}}, RECOVER(),
  [3] = {.entry = {.count = 1, .reusable = true}}, SHIFT_EXTRA(),
  [5] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_source_file, 0, 0, 0),
  [7] = {.entry = {.count = 1, .reusable = true}}, SHIFT(166),
  [9] = {.entry = {.count = 1, .reusable = true}}, SHIFT(200),
  [11] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_string, 2, 0, 0),
  [13] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_string, 2, 0, 0),
  [15] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_string, 3, 0, 0),
  [17] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_string, 3, 0, 0),
  [19] = {.entry = {.count = 1, .reusable = true}}, SHIFT(28),
  [21] = {.entry = {.count = 1, .reusable = true}}, SHIFT(148),
  [23] = {.entry = {.count = 1, .reusable = true}}, SHIFT(188),
  [25] = {.entry = {.count = 1, .reusable = false}}, SHIFT(188),
  [27] = {.entry = {.count = 1, .reusable = true}}, SHIFT(161),
  [29] = {.entry = {.count = 1, .reusable = true}}, SHIFT(162),
  [31] = {.entry = {.count = 1, .reusable = true}}, SHIFT(194),
  [33] = {.entry = {.count = 1, .reusable = true}}, SHIFT(170),
  [35] = {.entry = {.count = 1, .reusable = true}}, SHIFT(183),
  [37] = {.entry = {.count = 1, .reusable = true}}, SHIFT(195),
  [39] = {.entry = {.count = 1, .reusable = true}}, SHIFT(198),
  [41] = {.entry = {.count = 1, .reusable = true}}, SHIFT(204),
  [43] = {.entry = {.count = 1, .reusable = true}}, SHIFT(160),
  [45] = {.entry = {.count = 1, .reusable = true}}, SHIFT(176),
  [47] = {.entry = {.count = 1, .reusable = true}}, SHIFT(144),
  [49] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0),
  [51] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(148),
  [54] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(188),
  [57] = {.entry = {.count = 2, .reusable = false}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(188),
  [60] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(161),
  [63] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(162),
  [66] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(194),
  [69] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(170),
  [72] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(183),
  [75] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(195),
  [78] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(198),
  [81] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(204),
  [84] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(160),
  [87] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_project_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(176),
  [90] = {.entry = {.count = 1, .reusable = true}}, SHIFT(31),
  [92] = {.entry = {.count = 1, .reusable = true}}, SHIFT(146),
  [94] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_boolean, 1, 0, 0),
  [96] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_boolean, 1, 0, 0),
  [98] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_unity_build_block, 4, 0, 7),
  [100] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_unity_build_block, 4, 0, 7),
  [102] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_copy_block, 3, 0, 0),
  [104] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_copy_block, 3, 0, 0),
  [106] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_pch_block, 3, 0, 0),
  [108] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_pch_block, 3, 0, 0),
  [110] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_output_block, 3, 0, 0),
  [112] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_output_block, 3, 0, 0),
  [114] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dependencies_block, 3, 0, 0),
  [116] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_dependencies_block, 3, 0, 0),
  [118] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_block, 3, 0, 0),
  [120] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_assembler_block, 3, 0, 0),
  [122] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_sources_block, 4, 0, 3),
  [124] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_sources_block, 4, 0, 3),
  [126] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_unity_build_block, 3, 0, 0),
  [128] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_unity_build_block, 3, 0, 0),
  [130] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_property_stmt, 3, 0, 4),
  [132] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_property_stmt, 3, 0, 4),
  [134] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_string_list_block, 3, 0, 3),
  [136] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_string_list_block, 3, 0, 3),
  [138] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_sources_block, 3, 0, 3),
  [140] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_sources_block, 3, 0, 3),
  [142] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_headers_block, 4, 0, 0),
  [144] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_headers_block, 4, 0, 0),
  [146] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_copy_block, 4, 0, 0),
  [148] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_copy_block, 4, 0, 0),
  [150] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_pch_block, 4, 0, 9),
  [152] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_pch_block, 4, 0, 9),
  [154] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_output_block, 4, 0, 11),
  [156] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_output_block, 4, 0, 11),
  [158] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dependencies_block, 4, 0, 0),
  [160] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_dependencies_block, 4, 0, 0),
  [162] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_option_block, 4, 0, 1),
  [164] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_option_block, 4, 0, 1),
  [166] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_block, 4, 0, 0),
  [168] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_assembler_block, 4, 0, 0),
  [170] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_condition_block, 6, 0, 13),
  [172] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_condition_block, 6, 0, 13),
  [174] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_string_list_block, 4, 0, 3),
  [176] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_string_list_block, 4, 0, 3),
  [178] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_project_item, 1, 0, 0),
  [180] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_project_item, 1, 0, 0),
  [182] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_condition_block, 5, 0, 13),
  [184] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_condition_block, 5, 0, 13),
  [186] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_option_block, 5, 0, 1),
  [188] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_option_block, 5, 0, 1),
  [190] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_headers_block, 3, 0, 0),
  [192] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_headers_block, 3, 0, 0),
  [194] = {.entry = {.count = 1, .reusable = true}}, SHIFT(184),
  [196] = {.entry = {.count = 1, .reusable = true}}, SHIFT(165),
  [198] = {.entry = {.count = 1, .reusable = true}}, SHIFT(211),
  [200] = {.entry = {.count = 1, .reusable = true}}, SHIFT(206),
  [202] = {.entry = {.count = 1, .reusable = true}}, SHIFT(164),
  [204] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_package_decl_repeat1, 2, 0, 0),
  [206] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_package_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(165),
  [209] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_package_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(211),
  [212] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_package_decl_repeat1, 2, 0, 0), SHIFT_REPEAT(206),
  [215] = {.entry = {.count = 1, .reusable = true}}, SHIFT(27),
  [217] = {.entry = {.count = 1, .reusable = true}}, SHIFT(140),
  [219] = {.entry = {.count = 1, .reusable = true}}, SHIFT(205),
  [221] = {.entry = {.count = 1, .reusable = true}}, SHIFT(207),
  [223] = {.entry = {.count = 1, .reusable = true}}, SHIFT(15),
  [225] = {.entry = {.count = 1, .reusable = true}}, SHIFT(147),
  [227] = {.entry = {.count = 1, .reusable = true}}, SHIFT(186),
  [229] = {.entry = {.count = 1, .reusable = true}}, SHIFT(187),
  [231] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_assembler_block_repeat1, 2, 0, 0),
  [233] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_assembler_block_repeat1, 2, 0, 0), SHIFT_REPEAT(140),
  [236] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_assembler_block_repeat1, 2, 0, 0), SHIFT_REPEAT(205),
  [239] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_assembler_block_repeat1, 2, 0, 0), SHIFT_REPEAT(207),
  [242] = {.entry = {.count = 1, .reusable = true}}, SHIFT(152),
  [244] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_dep_object_repeat1, 2, 0, 0),
  [246] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_dep_object_repeat1, 2, 0, 0), SHIFT_REPEAT(186),
  [249] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_dep_object_repeat1, 2, 0, 0), SHIFT_REPEAT(187),
  [252] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_field, 3, 0, 0),
  [254] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_field, 5, 0, 0),
  [256] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_package_item, 1, 0, 0),
  [258] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_dep_object_repeat1, 1, 0, 0),
  [260] = {.entry = {.count = 1, .reusable = true}}, SHIFT(59),
  [262] = {.entry = {.count = 1, .reusable = false}}, SHIFT(18),
  [264] = {.entry = {.count = 1, .reusable = true}}, SHIFT(18),
  [266] = {.entry = {.count = 1, .reusable = false}}, SHIFT(9),
  [268] = {.entry = {.count = 1, .reusable = true}}, SHIFT(63),
  [270] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_package_item, 3, 0, 2),
  [272] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_authors_block, 3, 0, 0),
  [274] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_exports_block, 3, 0, 0),
  [276] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_authors_block, 4, 0, 0),
  [278] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_field, 3, 0, 2),
  [280] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_exports_block, 4, 0, 0),
  [282] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_field, 4, 0, 0),
  [284] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_item, 3, 0, 2),
  [286] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_item, 2, 0, 5),
  [288] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_item, 4, 0, 0),
  [290] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_assembler_item, 3, 0, 0),
  [292] = {.entry = {.count = 1, .reusable = true}}, SHIFT(16),
  [294] = {.entry = {.count = 1, .reusable = true}}, SHIFT(149),
  [296] = {.entry = {.count = 1, .reusable = true}}, SHIFT(191),
  [298] = {.entry = {.count = 1, .reusable = true}}, SHIFT(26),
  [300] = {.entry = {.count = 1, .reusable = true}}, SHIFT(174),
  [302] = {.entry = {.count = 1, .reusable = true}}, SHIFT(175),
  [304] = {.entry = {.count = 1, .reusable = true}}, SHIFT(179),
  [306] = {.entry = {.count = 1, .reusable = true}}, SHIFT(20),
  [308] = {.entry = {.count = 1, .reusable = false}}, SHIFT_EXTRA(),
  [310] = {.entry = {.count = 1, .reusable = false}}, SHIFT(2),
  [312] = {.entry = {.count = 1, .reusable = false}}, SHIFT(69),
  [314] = {.entry = {.count = 1, .reusable = true}}, SHIFT(69),
  [316] = {.entry = {.count = 1, .reusable = true}}, SHIFT(208),
  [318] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_option_block_repeat1, 2, 0, 0),
  [320] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_option_block_repeat1, 2, 0, 0), SHIFT_REPEAT(174),
  [323] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_option_block_repeat1, 2, 0, 0), SHIFT_REPEAT(175),
  [326] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_option_block_repeat1, 2, 0, 0), SHIFT_REPEAT(179),
  [329] = {.entry = {.count = 1, .reusable = false}}, REDUCE(aux_sym_string_repeat1, 2, 0, 0),
  [331] = {.entry = {.count = 2, .reusable = false}}, REDUCE(aux_sym_string_repeat1, 2, 0, 0), SHIFT_REPEAT(65),
  [334] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_string_repeat1, 2, 0, 0), SHIFT_REPEAT(65),
  [337] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_string_repeat1, 2, 0, 0), SHIFT_REPEAT(208),
  [340] = {.entry = {.count = 1, .reusable = false}}, SHIFT(158),
  [342] = {.entry = {.count = 1, .reusable = false}}, SHIFT(70),
  [344] = {.entry = {.count = 1, .reusable = true}}, SHIFT(70),
  [346] = {.entry = {.count = 1, .reusable = true}}, SHIFT(32),
  [348] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_sources_block_repeat1, 2, 0, 0),
  [350] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_sources_block_repeat1, 2, 0, 0), SHIFT_REPEAT(149),
  [353] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_sources_block_repeat1, 2, 0, 0), SHIFT_REPEAT(191),
  [356] = {.entry = {.count = 1, .reusable = false}}, SHIFT(3),
  [358] = {.entry = {.count = 1, .reusable = false}}, SHIFT(65),
  [360] = {.entry = {.count = 1, .reusable = true}}, SHIFT(65),
  [362] = {.entry = {.count = 1, .reusable = false}}, SHIFT(159),
  [364] = {.entry = {.count = 1, .reusable = true}}, SHIFT(17),
  [366] = {.entry = {.count = 1, .reusable = true}}, SHIFT(177),
  [368] = {.entry = {.count = 1, .reusable = true}}, SHIFT(178),
  [370] = {.entry = {.count = 1, .reusable = true}}, SHIFT(180),
  [372] = {.entry = {.count = 1, .reusable = true}}, SHIFT(24),
  [374] = {.entry = {.count = 1, .reusable = true}}, SHIFT(199),
  [376] = {.entry = {.count = 1, .reusable = true}}, SHIFT(201),
  [378] = {.entry = {.count = 1, .reusable = true}}, SHIFT(203),
  [380] = {.entry = {.count = 1, .reusable = true}}, SHIFT(157),
  [382] = {.entry = {.count = 1, .reusable = true}}, SHIFT(25),
  [384] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_headers_block_repeat1, 2, 0, 0),
  [386] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_headers_block_repeat1, 2, 0, 0), SHIFT_REPEAT(153),
  [389] = {.entry = {.count = 1, .reusable = true}}, SHIFT(119),
  [391] = {.entry = {.count = 1, .reusable = true}}, SHIFT(13),
  [393] = {.entry = {.count = 1, .reusable = true}}, SHIFT(12),
  [395] = {.entry = {.count = 1, .reusable = true}}, SHIFT(189),
  [397] = {.entry = {.count = 1, .reusable = true}}, SHIFT(192),
  [399] = {.entry = {.count = 1, .reusable = true}}, SHIFT(193),
  [401] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 2, 0, 8),
  [403] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 2, 0, 8), SHIFT_REPEAT(177),
  [406] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 2, 0, 8), SHIFT_REPEAT(178),
  [409] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 2, 0, 8), SHIFT_REPEAT(180),
  [412] = {.entry = {.count = 1, .reusable = true}}, SHIFT(10),
  [414] = {.entry = {.count = 1, .reusable = true}}, SHIFT(121),
  [416] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 2, 0, 12),
  [418] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 2, 0, 12), SHIFT_REPEAT(199),
  [421] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 2, 0, 12), SHIFT_REPEAT(201),
  [424] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 2, 0, 12), SHIFT_REPEAT(203),
  [427] = {.entry = {.count = 1, .reusable = true}}, SHIFT(66),
  [429] = {.entry = {.count = 1, .reusable = true}}, SHIFT(39),
  [431] = {.entry = {.count = 1, .reusable = true}}, SHIFT(23),
  [433] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_dependencies_block_repeat1, 2, 0, 0), SHIFT_REPEAT(157),
  [436] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_dependencies_block_repeat1, 2, 0, 0),
  [438] = {.entry = {.count = 1, .reusable = true}}, SHIFT(33),
  [440] = {.entry = {.count = 1, .reusable = true}}, SHIFT(153),
  [442] = {.entry = {.count = 1, .reusable = true}}, SHIFT(14),
  [444] = {.entry = {.count = 1, .reusable = true}}, SHIFT(21),
  [446] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 2, 0, 10),
  [448] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 2, 0, 10), SHIFT_REPEAT(189),
  [451] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 2, 0, 10), SHIFT_REPEAT(192),
  [454] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 2, 0, 10), SHIFT_REPEAT(193),
  [457] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_option_field, 3, 0, 2),
  [459] = {.entry = {.count = 1, .reusable = true}}, SHIFT(44),
  [461] = {.entry = {.count = 1, .reusable = true}}, SHIFT(29),
  [463] = {.entry = {.count = 1, .reusable = true}}, SHIFT(49),
  [465] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_string_list_block_repeat1, 2, 0, 0), SHIFT_REPEAT(63),
  [468] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_string_list_block_repeat1, 2, 0, 0),
  [470] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_exports_block_repeat1, 2, 0, 0), SHIFT_REPEAT(167),
  [473] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_exports_block_repeat1, 2, 0, 0),
  [475] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_sources_item, 2, 0, 5),
  [477] = {.entry = {.count = 1, .reusable = true}}, SHIFT(19),
  [479] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_copy_block_repeat1, 2, 0, 0),
  [481] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_copy_block_repeat1, 2, 0, 0), SHIFT_REPEAT(137),
  [484] = {.entry = {.count = 1, .reusable = true}}, SHIFT(104),
  [486] = {.entry = {.count = 1, .reusable = true}}, SHIFT(51),
  [488] = {.entry = {.count = 1, .reusable = true}}, SHIFT(167),
  [490] = {.entry = {.count = 1, .reusable = true}}, SHIFT(53),
  [492] = {.entry = {.count = 1, .reusable = true}}, SHIFT(58),
  [494] = {.entry = {.count = 1, .reusable = true}}, SHIFT(50),
  [496] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_sources_item, 3, 0, 2),
  [498] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 3, 0, 0),
  [500] = {.entry = {.count = 1, .reusable = true}}, SHIFT(118),
  [502] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 3, 0, 14),
  [504] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 3, 0, 15),
  [506] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 3, 0, 16),
  [508] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 3, 0, 17),
  [510] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_pch_block_repeat1, 3, 0, 18),
  [512] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 3, 0, 19),
  [514] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 3, 0, 20),
  [516] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_output_block_repeat1, 3, 0, 21),
  [518] = {.entry = {.count = 1, .reusable = true}}, SHIFT(22),
  [520] = {.entry = {.count = 1, .reusable = true}}, SHIFT(137),
  [522] = {.entry = {.count = 1, .reusable = true}}, SHIFT(57),
  [524] = {.entry = {.count = 1, .reusable = false}}, REDUCE(sym_env_interpolation, 3, 0, 23),
  [526] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_env_interpolation, 3, 0, 23),
  [528] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_unity_build_block_repeat1, 4, 0, 0),
  [530] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_option_field, 3, 0, 0),
  [532] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_option_field, 4, 0, 0),
  [534] = {.entry = {.count = 1, .reusable = true}}, SHIFT(54),
  [536] = {.entry = {.count = 1, .reusable = true}}, SHIFT(11),
  [538] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_condition_expr, 1, 0, 0),
  [540] = {.entry = {.count = 1, .reusable = true}}, SHIFT(172),
  [542] = {.entry = {.count = 1, .reusable = true}}, REDUCE(aux_sym_condition_expr_repeat1, 2, 0, 0),
  [544] = {.entry = {.count = 2, .reusable = true}}, REDUCE(aux_sym_condition_expr_repeat1, 2, 0, 0), SHIFT_REPEAT(172),
  [547] = {.entry = {.count = 1, .reusable = true}}, SHIFT(9),
  [549] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_condition_expr, 2, 0, 0),
  [551] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_header_item, 2, 0, 6),
  [553] = {.entry = {.count = 1, .reusable = true}}, SHIFT(122),
  [555] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_source_file, 1, 0, 0),
  [557] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_value, 1, 0, 0),
  [559] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_item, 3, 0, 22),
  [561] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_project_decl, 5, 0, 1),
  [563] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_copy_item, 4, 0, 24),
  [565] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_project_decl, 4, 0, 1),
  [567] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_object, 2, 0, 0),
  [569] = {.entry = {.count = 1, .reusable = true}}, SHIFT(124),
  [571] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_object, 3, 0, 0),
  [573] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_export_entry, 3, 0, 4),
  [575] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_name, 1, 0, 0),
  [577] = {.entry = {.count = 1, .reusable = false}}, SHIFT(168),
  [579] = {.entry = {.count = 1, .reusable = true}}, SHIFT(209),
  [581] = {.entry = {.count = 1, .reusable = true}}, SHIFT(62),
  [583] = {.entry = {.count = 1, .reusable = true}}, SHIFT(85),
  [585] = {.entry = {.count = 1, .reusable = true}},  ACCEPT_INPUT(),
  [587] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_package_decl, 3, 0, 0),
  [589] = {.entry = {.count = 1, .reusable = true}}, SHIFT(155),
  [591] = {.entry = {.count = 1, .reusable = true}}, SHIFT(197),
  [593] = {.entry = {.count = 1, .reusable = true}}, SHIFT(143),
  [595] = {.entry = {.count = 1, .reusable = true}}, SHIFT(169),
  [597] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_dep_name, 3, 0, 0),
  [599] = {.entry = {.count = 1, .reusable = true}}, SHIFT(123),
  [601] = {.entry = {.count = 1, .reusable = true}}, SHIFT(142),
  [603] = {.entry = {.count = 1, .reusable = true}}, SHIFT(151),
  [605] = {.entry = {.count = 1, .reusable = true}}, SHIFT(181),
  [607] = {.entry = {.count = 1, .reusable = true}}, SHIFT(114),
  [609] = {.entry = {.count = 1, .reusable = true}}, SHIFT(75),
  [611] = {.entry = {.count = 1, .reusable = true}}, SHIFT(38),
  [613] = {.entry = {.count = 1, .reusable = true}}, SHIFT(98),
  [615] = {.entry = {.count = 1, .reusable = true}}, SHIFT(126),
  [617] = {.entry = {.count = 1, .reusable = true}}, SHIFT(128),
  [619] = {.entry = {.count = 1, .reusable = true}}, SHIFT(202),
  [621] = {.entry = {.count = 1, .reusable = true}}, SHIFT(7),
  [623] = {.entry = {.count = 1, .reusable = true}}, SHIFT(71),
  [625] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_package_decl, 4, 0, 0),
  [627] = {.entry = {.count = 1, .reusable = true}}, SHIFT(55),
  [629] = {.entry = {.count = 1, .reusable = true}}, SHIFT(150),
  [631] = {.entry = {.count = 1, .reusable = true}}, SHIFT(132),
  [633] = {.entry = {.count = 1, .reusable = true}}, SHIFT(47),
  [635] = {.entry = {.count = 1, .reusable = true}}, SHIFT(127),
  [637] = {.entry = {.count = 1, .reusable = true}}, REDUCE(sym_source_file, 2, 0, 0),
  [639] = {.entry = {.count = 1, .reusable = true}}, SHIFT(129),
  [641] = {.entry = {.count = 1, .reusable = true}}, SHIFT(156),
  [643] = {.entry = {.count = 1, .reusable = true}}, SHIFT(135),
  [645] = {.entry = {.count = 1, .reusable = true}}, SHIFT(96),
  [647] = {.entry = {.count = 1, .reusable = true}}, SHIFT(77),
  [649] = {.entry = {.count = 1, .reusable = true}}, SHIFT(117),
  [651] = {.entry = {.count = 1, .reusable = true}}, SHIFT(8),
  [653] = {.entry = {.count = 1, .reusable = true}}, SHIFT(76),
  [655] = {.entry = {.count = 1, .reusable = true}}, SHIFT(136),
  [657] = {.entry = {.count = 1, .reusable = true}}, SHIFT(35),
  [659] = {.entry = {.count = 1, .reusable = true}}, SHIFT(134),
  [661] = {.entry = {.count = 1, .reusable = true}}, SHIFT(107),
  [663] = {.entry = {.count = 1, .reusable = true}}, SHIFT(141),
  [665] = {.entry = {.count = 1, .reusable = true}}, SHIFT(86),
  [667] = {.entry = {.count = 1, .reusable = true}}, SHIFT(101),
  [669] = {.entry = {.count = 1, .reusable = true}}, SHIFT(102),
  [671] = {.entry = {.count = 1, .reusable = true}}, SHIFT(185),
  [673] = {.entry = {.count = 1, .reusable = false}}, SHIFT(196),
  [675] = {.entry = {.count = 1, .reusable = true}}, SHIFT(61),
  [677] = {.entry = {.count = 1, .reusable = true}}, SHIFT(82),
  [679] = {.entry = {.count = 1, .reusable = true}}, SHIFT(92),
};

#ifdef __cplusplus
extern "C" {
#endif
#ifdef TREE_SITTER_HIDE_SYMBOLS
#define TS_PUBLIC
#elif defined(_WIN32)
#define TS_PUBLIC __declspec(dllexport)
#else
#define TS_PUBLIC __attribute__((visibility("default")))
#endif

TS_PUBLIC const TSLanguage *tree_sitter_dotori(void) {
  static const TSLanguage language = {
    .version = LANGUAGE_VERSION,
    .symbol_count = SYMBOL_COUNT,
    .alias_count = ALIAS_COUNT,
    .token_count = TOKEN_COUNT,
    .external_token_count = EXTERNAL_TOKEN_COUNT,
    .state_count = STATE_COUNT,
    .large_state_count = LARGE_STATE_COUNT,
    .production_id_count = PRODUCTION_ID_COUNT,
    .field_count = FIELD_COUNT,
    .max_alias_sequence_length = MAX_ALIAS_SEQUENCE_LENGTH,
    .parse_table = &ts_parse_table[0][0],
    .small_parse_table = ts_small_parse_table,
    .small_parse_table_map = ts_small_parse_table_map,
    .parse_actions = ts_parse_actions,
    .symbol_names = ts_symbol_names,
    .field_names = ts_field_names,
    .field_map_slices = ts_field_map_slices,
    .field_map_entries = ts_field_map_entries,
    .symbol_metadata = ts_symbol_metadata,
    .public_symbol_map = ts_symbol_map,
    .alias_map = ts_non_terminal_alias_map,
    .alias_sequences = &ts_alias_sequences[0][0],
    .lex_modes = ts_lex_modes,
    .lex_fn = ts_lex,
    .primary_state_ids = ts_primary_state_ids,
  };
  return &language;
}
#ifdef __cplusplus
}
#endif
