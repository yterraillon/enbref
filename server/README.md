# server

API .NET 10 d'EnBref : collecte des titres, génération du récap, publication sur le CDN GitHub, et
back-office. Reconstruite de zéro ; les conventions sont dans `.claude/CLAUDE.md`, les décisions
dans `docs/architecture-decision-record.md` (ADR-005 à 007).

## Structure

```
src/Api/              racine de composition, slices (Features/), back-office Blazor (BackOffice/)
src/Infrastructure/   implémentations des dépendances sortantes
tests/                tests unitaires TUnit (Api.Tests, Infrastructure.Tests)
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
| `/back-office` | back-office (réseau local uniquement) : récap du jour, génération à la main |
| `/back-office/recaps` | les trois récaps publiés, lus sans cache, et leur régénération |
| `/back-office/feeds` | titres collectés et état de chaque flux |
| `/back-office/settings` | génération quotidienne, modèle et test du modèle, version et environnement |
| `/health` | sonde, renvoie `{ status, version }` |
| `/swagger` | documentation OpenAPI |
