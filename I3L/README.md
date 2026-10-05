# I3L

BootTo.NET bundles the I3L compiler for its custom CoreLib. MSBuild invokes it automatically; the native host handles linking and runtime symbols.

Keep `I3L.exe` and `libLLVM.dll` together. For manual use:

```text
I3L.exe <input-assembly> <output-file> <target>[;<code-model>[;<cpu>[;<features>]]]
```

Use `.S` for assembly output; other filenames produce an object. Source: [github](https://github.com/nifanfa/I3L).