# Shibusa Dev Tools

**Shibusa.DevTools.Cli** is a console control program for accessing the other dev tools.

```
devtools
devtools --help
devtools --help fl
```

**Shibusa.DevTools.Config.Cli** is a console app for preserving aliases in a `.config` file.

```
devtools config --help
devtools config show
devtools config show fl
devtools config show sql
devtools config add sql.prod "c:\myprod.db"
devtools config delete sql.prod
devtools config remove sql.prod
```

**Shibusa.DevTools.FindLines.Cli** is a console application that can search across specific directories and file extensions for one or more regular expressions.
```
devtools fl --help
devtools fl . "Assembly\." 
devtools fl -d . -e "Assembly\." # equivalent to previous command
```

**Shibusa.DevTools.CsProjects.Cli** is a console application that reads `csproj` files and writes them and some of their dependencies to the console.

```
devtools cs --help
devtools cs -d /c/repos
```