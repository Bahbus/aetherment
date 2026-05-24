# Install Guide
### Plugin
Add `https://aetherment.sevii.dev/plugin` to your Custom Plugin Repositories in `/xlsettings` > Experimental

### Plugin renderer compatibility (Proton/Wine)
If the plugin has rendering glitches under Proton/Wine, you can enable one or both D3D11 compatibility flags before launching XIVLauncher/Dalamud:

- `AETHERMENT_D3D11_COMPAT_DISABLE_DEPTH=1`  
  Disables depth testing/writes for Aetherment's 3D renderer.
- `AETHERMENT_D3D11_COMPAT_DISABLE_CULL=1`  
  Disables face culling for Aetherment's 3D renderer.

Examples:

- Linux (shell launch):
  - `AETHERMENT_D3D11_COMPAT_DISABLE_DEPTH=1 xivlauncher`
  - `AETHERMENT_D3D11_COMPAT_DISABLE_CULL=1 xivlauncher`
  - `AETHERMENT_D3D11_COMPAT_DISABLE_DEPTH=1 AETHERMENT_D3D11_COMPAT_DISABLE_CULL=1 xivlauncher`
- Windows (PowerShell before launch):
  - `$env:AETHERMENT_D3D11_COMPAT_DISABLE_DEPTH = "1"`
  - `$env:AETHERMENT_D3D11_COMPAT_DISABLE_CULL = "1"`
  - `Start-Process "XIVLauncher.exe"`

You can confirm active values in the plugin log line printed during initialization.

### Desktop Client
Download the latest version for your system from the releases tab.
NOTE: this currently does not have much functionality besides mod creation through the gui tools tab and CLI.

# Support
If you wish to support me and my work, you can do so [here](https://buymeacoffee.com/sevii77)
