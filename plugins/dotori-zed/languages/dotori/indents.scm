; Increase indent inside { ... } blocks
(project_decl body: (block "{" @indent))
(package_decl       "{" @indent)

; Decrease indent at closing brace
"}" @dedent
