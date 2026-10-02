# server

API .NET 10 d'EnBref : collecte des titres, génération du récap, publication sur le CDN GitHub, et
back-office. Reconstruite de zéro ; les conventions sont dans `.claude/CLAUDE.md`, les décisions
dans `docs/architecture-decision-record.md` (ADR-005 à 007).

## Structure

```
src/Api/              racine de composition, slices (Features/), back-office Blazor (BackOffice/)
src/Infrastructure/   implémentations des dépendances sortantes
tests/Api.Tests/      tests unitaires TUnit
```

## Lancer

```bash
dotnet build enbref.server.slnx
dotnet test --solution enbref.server.slnx
dotnet run --project src/Api          # http://localhost:5080/swagger
```

| Route | Rôle |
|---|---|
| `POST /api/recaps/generations` | déclenche une génération |
| `/back-office` | back-office (réseau local uniquement) |
| `/health` | sonde |
| `/swagger` | documentation OpenAPI |
