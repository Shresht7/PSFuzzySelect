# `PSFuzzySelect`

`PSFuzzySelect` is a PowerShell module that provides a fuzzy selection interface for PowerShell objects. It allows users to quickly filter and select items from a list of objects using a fuzzy matching algorithm and forwards the selected objects down the pipeline.

Think `fzf` but as a first-class PowerShell cmdlet that understands PowerShell objects.

> [!WARNING]
> ⚠️ This module is in active development and ~~may~~ will have breaking changes. ⚠️

---

## Usage

### Basic

```powershell
# Pick one file or folder from the current directory
Get-ChildItem | Select-Fuzzy

# Match only on the Name property of the file or folder
Get-ChildItem | Select-Fuzzy -Property Name

# Supports regular strings just as well
git branch --format="%(refname:short)" | Select-Fuzzy
```

### Pipeline Integration

```powershell
# Select a process interactively and stop it
Get-Process | Select-Fuzzy | Stop-Process

# Stage files interactively
git status --porcelain | Select-Fuzzy | ForEach-Object { git add $_ }
```

### Multiple Selection

```powershell
# Select multiple processes interactively and stop them
Get-Process | Select-Fuzzy -MultiSelect | Stop-Process
```

### Preview Display

```powershell
# Select a file and preview its content in the right pane
Get-ChildItem -File -Recurse | Select-Fuzzy -Preview -PreviewScript { Get-Content $_ }

# Preview pane at the top instead of the right and take 60% of the screen height
Get-ChildItem -File -Recurse | Select-Fuzzy -Preview -PreviewScript { Get-Content $_ } -PreviewPosition Top -PreviewSize "60%"

# Preview git log for each branch
git branch --format="%(refname:short)" | Select-Fuzzy -Preview -PreviewScript { git log -20 --oneline --pretty=format:"%h %s" $_ }
```

## Parameters

<!-- TODO -->

## Key Bindings

<!-- TODO -->

---

## Development

To build the project, run the following command in the project directory:

```pwsh
dotnet build
```

Then you can import the module in PowerShell:

```pwsh
Import-Module .\Module\PSFuzzySelect.psd1 -Force
```

> [!NOTE] `-Force` is necessary because PowerShell caches loaded assemblies

---

## License

This project is licensed under the [MIT License](LICENSE)
