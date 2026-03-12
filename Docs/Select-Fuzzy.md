---
external help file: PSFuzzySelect.dll-Help.xml
Module Name: PSFuzzySelect
online version:
schema: 2.0.0
---

# Select-Fuzzy

## SYNOPSIS
Provides an interactive fuzzy selection interface for PowerShell pipeline objects.

## SYNTAX

```
Select-Fuzzy [-InputObject <PSObject>] [-MultiSelect] [[-Property] <String[]>] [-Prompt <String>]
 [-Query <String>] [-ShowPreview] [-PreviewSize <String>] [-PreviewPosition <PreviewPosition>]
 [-PreviewScript <ScriptBlock>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
`Select-Fuzzy` presents a full-screen terminal-user-interface for fuzzy-filtering and selecting objects from the pipeline. Items appear in the user-interface as the upstream command produces them (streaming). The selected objects are written to the output pipeline after the user confirms.

  Navigation
- **Up/Down Arrow**: Move the cursor up or down.
- **Ctrl + Up/Down Arrow**: Jump to the very top or bottom of the list.
- **Tab / Shift + Tab**: Move the cursor (In `-MultiSelect` mode, this also toggles selection).

  Selection
- **Enter**: Confirm the current selection and exit.
- **Esc**: Cancel the selection and exit without outputting anything.

  Search Query Editing
- **Characters**: Type to filter the list.
- **Backspace**: Delete the character before the cursor.
- **Delete**: Delete the character at the cursor.
- **Left/Right Arrow**: Move the cursor within the search query.
- **Ctrl + Left/Right Arrow**: Jump between words in the search query.
- **Home / End**: Jump to the beginning or end of the search query.

## EXAMPLES

### Example 1: Basic File Selection

```powershell
Get-ChildItem -File | Select-Fuzzy
```

Interactively select a file from the current directory.

### Example 2: Selecting by Property

```powershell
Get-ChildItem -File -Recurse | Select-Fuzzy -Property Name
```

Pick a file by its name from a recursive list.

### Example 3: Multi-select with Processes

```powershell
Get-Process | Select-Fuzzy -MultiSelect | Stop-Process
```

Select multiple processes and stop them.

### Example 4: Using the Preview Pane

```powershell
Get-ChildItem -File -Recurse | Select-Fuzzy -ShowPreview -PreviewScript { Get-Content $_.FullName }
```

Select multiple files with a live content preview in a side pane.

## PARAMETERS

### -InputObject
The input object(s) to be processed by the cmdlet. This parameter accepts input from the pipeline, allowing users to pipe objects directly into the cmdlet for fuzzy selection.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -MultiSelect
Indicates whether multiple items can be selected in the fuzzy selector. When enabled, use Tab or Shift+Tab to toggle selection on items.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -PreviewPosition
The position of the preview pane relative to the main list. Valid values: Right, Left, Top, Bottom. Defaults to Right.

```yaml
Type: PreviewPosition
Parameter Sets: (All)
Aliases:
Accepted values: Right, Left, Top, Bottom

Required: False
Position: Named
Default value: Right
Accept pipeline input: False
Accept wildcard characters: False
```

### -PreviewScript
A script block used to generate the preview content for the currently highlighted item. The current item is available as `$_` or `$PSItem`. The output of the script is rendered as text in the preview pane.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases: Preview, PreviewScriptBlock

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PreviewSize
The size of the preview pane, specified as a percentage (e.g., "50%") or a fixed number of columns/rows (e.g., "30"). Defaults to "50%".

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 50%
Accept pipeline input: False
Accept wildcard characters: False
```

### -Prompt
The prompt message to display next to the search input field (default: "Search:").

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Search:
Accept pipeline input: False
Accept wildcard characters: False
```

### -Property
The properties of the input objects to display and search against. If not specified, the cmdlet uses the object's default display properties or its `ToString()` representation.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ShowPreview
Indicates whether to show the preview pane. This is implicitly enabled if `-PreviewScript` is provided.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Details

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Query
The initial search query to populate the search input field when the fuzzy selector UI is launched.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject

## OUTPUTS

### System.Management.Automation.PSObject

## NOTES

## RELATED LINKS
