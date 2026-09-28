# EnBref — serveur

API .NET 10 qui produit et publie le récap quotidien : collecte des titres dans les flux RSS,
génération via LLM, publication sur le dépôt CDN GitHub.

Le code est **repris tel quel** du module `Modules/EnBref` du monolithe
[myfanwy](https://github.com/yterraillon/myfanwy), sans historique git. Le refactoring
(BuildingBlocks, alignement sur `docs/ubiquitous-language.md`, passage à l'API Claude) est un
chantier distinct — voir « Dette assumée » plus bas.

## Structure

```
server/
├── Directory.Build.props      suppression d'audit NuGet (AutoMapper 14, dernière version MIT)
├── Directory.Packages.props   versions NuGet centralisées
├── global.json                pin du SDK .NET 10
├── enbref.server.slnx
└── src/
    ├── Api/                   host ASP.NET — Program.cs, contrôleurs, Dockerfile
    ├── BuildingBlocks/
    │   ├── Application/       abstractions : IRepository, INotificationService, IAiAgent, IObjectStorage*
    │   └── Infrastructure/    LiteDB, ntfy, helpers HttpClient et sérialisation
    └── Modules/EnBref/
        ├── EnBref.Application/     modèles et features (CQRS via MediatR)
        └── EnBref.Infrastructure/  agents OpenAI, CDN GitHub, RSS, job Quartz, LiteDB
```

Deux dossiers apparaissent à l'exécution locale, tous deux ignorés par git : `src/Database/` (base
LiteDB) et `src/Data/En-Bref/` (créé à la demande par `LocalStorageService`, qui ne sert que le
chemin Azure Blob aujourd'hui désactivé).

L'app Blazor d'administration (« back-office ») n'existe pas encore : elle viendra comme projet
`src/BackOffice`, à côté de `src/Api`.

## Commandes

```bash
dotnet restore enbref.server.slnx
dotnet build enbref.server.slnx
dotnet run --project src/Api        # http://localhost:5240/swagger
```

Stack Docker locale (depuis la racine du monorepo) :

```bash
cp infra/.env.local.example infra/.env.local   # puis renseigner les secrets
docker compose -f infra/compose.local.yml up -d --build --wait
```

## Endpoints

| Méthode | Route | Description |
|---|---|---|
| `GET` | `/api/enbref/en-bref` | Déclenche une génération **et sa publication** sur le CDN |
| `GET` | `/health` | Sonde du healthcheck Docker |
| `GET` | `/swagger` | Documentation OpenAPI |

⚠️ `GET /api/enbref/en-bref` n'est pas une lecture : il génère un récap et **écrase
`latest-recap.json` en production** si `GithubToken` est renseigné.

## Configuration

Secrets, via le Secret Manager en local (`dotnet user-secrets --project src/Api`) et via des
variables d'environnement en conteneur :

| Clé | Rôle |
|---|---|
| `OpenAiApiKey` | Génération et formatage du récap |
| `GithubToken` | Publication sur `yterraillon/yterraillon.github.io` |
| `NtfyToken` | Notification d'échec sur ntfy |
| `EnBrefConnectionString` | Azure Blob — hérité, inutilisé depuis le passage au CDN GitHub |

Base LiteDB : `..\Database\EnBref.db` en local, `/data/EnBref.db` en conteneur
(`ConnectionStrings` dans `src/Api/appsettings.json`). Le basculement local ↔ conteneur se fait sur
`ASPNETCORE_ENVIRONMENT` : toute valeur autre que `Development` active les chemins conteneur.

## Génération quotidienne

Le job Quartz `GenerateDailyRecapJob` est enregistré par `AddEnBrefInfrastructure` et déclenche une
génération tous les jours à **17:00** (cron `0 0 17 * * ?`, fuseau du conteneur). Il tourne donc dans
toute instance du serveur, stack locale comprise.

## Dette assumée

Reprise iso de myfanwy, à traiter au refactoring :

- Le vocabulaire du code (`Section`, `RecapSectionMetric`, `latest-recap.json`) précède
  `docs/ubiquitous-language.md` et le contredit — écart consigné au § 9 du glossaire.
- Le LLM est OpenAI, pas l'API Claude visée.
- `BuildingBlocks/Infrastructure/RssReader` et `Application/Logging/LoggingBehavior` sont importés
  sans être utilisés ; les lecteurs/écrivains Azure Blob sont commentés dans la DI.
- Le topic ntfy reste `https://ntfy.checquy.ovh/myfanwy`, codé en dur dans
  `BuildingBlocks/Infrastructure/DependencyInjection.cs`.
- Aucun projet de test : l'étape `test` de la CI est à rétablir avec le premier.
