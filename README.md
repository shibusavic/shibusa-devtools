# Shibusa Dev Tools

## Utilities

**Shibusa.DevTools.Cli** is a console control program fro accessing the other dev tools.

```
devtools
devtools --help
devtools --help ft
```

**Shibusa.DevTools.FindText.Cli** is a console application that can search across specific directories and file extensions for one or more regular expressions. Expressions can be applied as either individual or cumulative.

```
devtools ft --help
devtools ft . "Assembly\." 
devtools ft -d . -e "Assembly\."
```

**Shibusa.DevTools.CsProjects.Cli** is a console application that reads `csproj` files and writes them and some of their dependencies to the console.

```
devtools cs --help
devtools cs -d /c/repos
```