# `PSFuzzySelect`

`PSFuzzySelect` is a PowerShell module that provides a fuzzy selection interface for PowerShell objects. It allows users to quickly filter and select items from a list of objects using a fuzzy matching algorithm and forwards the selected objects down the pipeline.

## Development

To build the project, run the following command in the project directory:

```pwsh
dotnet build
```

Then you can import the module in PowerShell:

```pwsh
Import-Module .\PSFuzzySelect.psd1 -Force
```

> [!NOTE] `-Force` is necessary because PowerShell caches loaded assemblies

---

## License

This project is licensed under the [MIT License](LICENSE)
