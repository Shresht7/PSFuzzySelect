# `PSFuzzySelect`

`PSFuzzySelect` is a PowerShell module that provides a fuzzy selection interface for PowerShell objects. It allows users to quickly filter and select items from a list of objects using a fuzzy matching algorithm and forwards the selected objects down the pipeline.

Think `fzf` but as a first-class PowerShell cmdlet that understands the PowerShell pipeline.

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

> [!CAUTION]
> We have to collect and retain all pipeline objects in memory while the user is making their selection, so be cautious when using `Select-Fuzzy` with a large number of objects as it may lead to high memory usage (~600MB for ~1M objects in my tests). For excessively large lists, you are better off not blocking the pipeline and using a more traditional approach to selection like `Select-Object`.

### Parameters

| Parameter          | Alias          | Description                                                        |
| ------------------ | -------------- | ------------------------------------------------------------------ |
| `-Property`        | (Positional 0) | The properties of the input objects to display and search against. |
| `-MultiSelect`     |                | Enables multiple item selection using `Tab`.                       |
| `-Prompt`          |                | The prompt message to display next to the search input field.      |
| `-ShowPreview`     | `-Details`     | Indicates whether to show the preview pane.                        |
| `-PreviewSize`     |                | The size of the preview pane (e.g., "50%", "30").                  |
| `-PreviewPosition` |                | The position of the preview pane (Right, Left, Top, Bottom).       |
| `-PreviewScript`   | `-Preview`     | Script block used to generate the preview content.                 |

### Key Bindings

#### Navigation

| Key                                                     | Action                                                                 |
| ------------------------------------------------------- | ---------------------------------------------------------------------- |
| <kbd>Up</kbd> / <kbd>Down</kbd> Arrow                   | Move the cursor up or down.                                            |
| <kbd>Ctrl</kbd> + <kbd>Up</kbd> / <kbd>Down</kbd> Arrow | Jump to the very top or bottom of the list.                            |
| <kbd>Tab</kbd> / <kbd>Shift</kbd> + <kbd>Tab</kbd>      | Move the cursor (In `-MultiSelect` mode, this also toggles selection). |

#### Selection

| Key              | Action                                                     |
| ---------------- | ---------------------------------------------------------- |
| <kbd>Enter</kbd> | Confirm the current selection and exit.                    |
| <kbd>Esc</kbd>   | Cancel the selection and exit without outputting anything. |

#### Search Query Editing

| Key                                                        | Action                                            |
| ---------------------------------------------------------- | ------------------------------------------------- |
| <kbd>Characters</kbd>                                      | Type to filter the list.                          |
| <kbd>Backspace</kbd>                                       | Delete the character before the cursor.           |
| <kbd>Delete</kbd>                                          | Delete the character at the cursor.               |
| <kbd>Left</kbd> / <kbd>Right</kbd> Arrow                   | Move the cursor within the search query.          |
| <kbd>Ctrl</kbd> + <kbd>Left</kbd> / <kbd>Right</kbd> Arrow | Jump between words in the search query.           |
| <kbd>Home</kbd> / <kbd>End</kbd>                           | Jump to the beginning or end of the search query. |

---

## Development

###  Build

1. **Build the project**:
   ```pwsh
   dotnet build
   ```

2. **Import the module**:
   ```pwsh
   Import-Module .\Module\PSFuzzySelect.psd1 -Force
   ```

### Documentation

This project uses [platyPS](https://github.com/PowerShell/platyPS) to generate PowerShell help from Markdown.

#### Generate Markdown from the DLL

Generates markdown files in the `Docs` folder based on the cmdlets defined in the `PSFuzzySelect.dll` assembly:

```pwsh
Import-Module platyPS
Import-Module .\Module\PSFuzzySelect.dll -Force
New-MarkdownHelp -Module PSFuzzySelect -OutputFolder Docs -Force
```

#### Update Markdown

If you've already generated the markdown and just want to refresh it with new parameters or changes from the assembly:

```pwsh
Update-MarkdownHelpModule -Path .\Docs
```

#### Generate External Help XML

To generate the `PSFuzzySelect.dll-Help.xml` file used by PowerShell for `Get-Help`:

```pwsh
New-ExternalHelp -Path .\Docs -OutputPath .\Module -Force
```

---

## License

This project is licensed under the [MIT License](LICENSE)
