# Harmony Thin dependency manifest

This FastScriptReload integration uses the official NuGet package
`Lib.Harmony.Thin 2.4.2` instead of the dependency-merged Fat build. The Fat
build shipped by FastScriptReload commit
`51140b71d9e5df1de231b33ec20ee089b18bebec` contains invalid TypeReference
metadata that Unity Burst 1.8.x rejects while scanning Editor assemblies.

## Provenance

The dependency closure was restored from NuGet.org for a `net48` project.
`MonoMod.Backports` and `MonoMod.ILHelpers` use their package-provided
`netstandard2.1` variants because the .NET Framework Backports variant defines
`ReadOnlySpan` types that already exist in Unity 2022.3's mscorlib. Package
versions and assembly identities remain unchanged. Only the DLLs below are
vendored. `System.ValueTuple, Version=4.0.3.0` is not duplicated because the
project already provides the same assembly identity as an Editor-only plugin at
`Assets/Plugins/ForEditor/CompressImageLibs/System.ValueTuple.dll`.

| Package | Version | Source revision |
| --- | --- | --- |
| Lib.Harmony.Thin | 2.4.2 | `pardeike/Harmony@a264a1bf1ce689e4589e8dcc54b1e2818602a90a` |
| Mono.Cecil | 0.11.6 | `jbevain/cecil@0.11.6` |
| MonoMod.Backports | 1.1.2 | `MonoMod/MonoMod@a1b82852b2574742776af08818487b90b0bfab93` |
| MonoMod.Core | 1.3.3 | `MonoMod/MonoMod@aa4a8474906ded4423d222cdf9ea5cf884c32b5f` |
| MonoMod.ILHelpers | 1.1.0 | `MonoMod/MonoMod@a1b82852b2574742776af08818487b90b0bfab93` |
| MonoMod.Utils | 25.0.11 | `MonoMod/MonoMod@aa4a8474906ded4423d222cdf9ea5cf884c32b5f` |
| Iced (inside MonoMod.Core) | 1.21.0 | `icedland/iced@v1.21.0` |

NuGet sources:

- https://www.nuget.org/packages/Lib.Harmony.Thin/2.4.2
- https://www.nuget.org/packages/Mono.Cecil/0.11.6
- https://www.nuget.org/packages/MonoMod.Backports/1.1.2
- https://www.nuget.org/packages/MonoMod.Core/1.3.3
- https://www.nuget.org/packages/MonoMod.ILHelpers/1.1.0
- https://www.nuget.org/packages/MonoMod.Utils/25.0.11

## SHA-256

| File | SHA-256 |
| --- | --- |
| `0Harmony.dll` | `657D779DD07781CC04D95EEFDFECC6B209AC2B9B21F66B7A6B395732CC28C129` |
| `Mono.Cecil.dll` | `C41BDB9FFD3C5F6E17D2382C1012D73703E035E3F1100245FDD4E08C8DC6EB5B` |
| `Mono.Cecil.Mdb.dll` | `570A437DEA0271D1D5C8B7D6A408B0B2635BDB0E8B8D5051878F3E7FCA087F89` |
| `Mono.Cecil.Pdb.dll` | `50A1A1A79DC86FCFB8B51249B5325A10DD93D193C52999CF6775D25030A4E606` |
| `Mono.Cecil.Rocks.dll` | `842E09959084EDA733AAB1A5354D7AF79E29594F4D8B91C8792103E5C755ED9B` |
| `MonoMod.Backports.dll` (netstandard2.1) | `B3FC7AAC52FFE6579AE1009A285A3D892B167151B0E18B5C1066CCB88E8B44A4` |
| `MonoMod.Core.dll` | `4BB34DD557481564105E279EE92912D2FA615BA9F39E5212F0031BF62EFF9571` |
| `MonoMod.Iced.dll` | `7890F9EEAC088C52796C1AE73FA8FEB1E7967A36E63F71D84CFB0445045A19C0` |
| `MonoMod.ILHelpers.dll` (netstandard2.1) | `A13590452F2BDF72718F43B410F43E110F273FCE57FABD58AC5043A59B9BE049` |
| `MonoMod.Utils.dll` | `F0BDD7717CCA42312E55CCD23E3EE7B61DF104FE9440881D2E8ABBEBAF8FE644` |
| Existing `System.ValueTuple.dll` | `D6FB0DCFEE1490A8168117ED1B55758F11DB38475417B3668D19F89DCB55CBDD` |

The replaced Fat `0Harmony.dll` had SHA-256
`77E6901ECC606AEC66C2A972782A3779E4F50C037D2D165EB7ECECDD4D8F794D`.

## Roslyn filename alignment

For the Unity 2021+ branch, FastScriptReload had renamed two files without
changing their internal assembly names. Burst requires those names to match.
The binaries and their existing Unity GUIDs are preserved; only filenames and
asmdef references change:

| File | Assembly | SHA-256 |
| --- | --- | --- |
| `Microsoft.CodeAnalysis.dll` | `Microsoft.CodeAnalysis, Version=4.6.0.0` | `233E5D17D266068A72D4F044D52D2AB06FDFD07C1816BB0CB5BE5EDF73179E35` |
| `Microsoft.CodeAnalysis.CSharp.dll` | `Microsoft.CodeAnalysis.CSharp, Version=4.6.0.0` | `D8E70399FF5725728BA13373D4FC5F6B7B219FC78D520B68DDF01ABB1389DC70` |

## Unity import contract

All vendored dependency DLLs use the same PluginImporter constraint as FSR's
Harmony DLL:

`UNITY_EDITOR || LiveScriptReload_IncludeInBuild_Enabled`

This project must not define `LiveScriptReload_IncludeInBuild_Enabled`, so the
dependency group remains Editor-only and does not provide Player hot reload.

## Licenses

All listed components are distributed under the MIT License. Copyright
notices:

- Copyright (c) 2017 Andreas Pardeike (Harmony)
- Copyright (c) 2008-2015 Jb Evain and Copyright (c) 2008-2011 Novell, Inc. (Mono.Cecil)
- Copyright (c) 2015-2020 0x0ade (MonoMod)
- Copyright (c) 2018-present iced project and contributors (Iced)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
