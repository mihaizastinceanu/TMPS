# TMPS Laboratory Works

**Course:** TMPS, Technical University of Moldova (UTM)
**Author:** Mihai Zastinceanu, FAF-243
**Language / platform:** C# 12, .NET 8

Each laboratory work is on its own branch. The project of each lab is in the root of its branch.

| Lab | Topic | Branch |
|---|---|---|
| Lab 0 | SOLID Principles (SRP, OCP, ISP) | [`lab-0`](../../tree/lab-0) |
| Lab 1 | Creational Design Patterns | [`lab-1`](../../tree/lab-1) |

## How to run a lab

Requires the .NET 8 SDK.

```
git clone https://github.com/mihaizastinceanu/TMPS.git
cd TMPS
git checkout lab-1
dotnet run --project CarServiceCreational
dotnet test
```

| Branch | Run command |
|---|---|
| `lab-0` | `dotnet run --project CarServiceCenter` |
| `lab-1` | `dotnet run --project CarServiceCreational` |
