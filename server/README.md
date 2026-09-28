# server

API .NET 10 d'EnBref : collecte des titres, génération du récap, publication sur le CDN GitHub, et
back-office d'administration.

> ⚠️ **Ce code vient d'être importé de `myfanwy` en conservant sa structure d'origine.** Il ne
> respecte pas les règles du projet — voir « Écarts » plus bas. Ne pas s'en inspirer pour du code
> neuf : les conventions cibles sont dans [`.claude/CLAUDE.md`](.claude/CLAUDE.md).

## Lancer

```bash
dotnet build server/enbref.server.slnx
dotnet run --project server/src/Api
```

Ou via la stack Docker locale, qui reproduit la production :

```bash
cp infra/.env.local.example infra/.env.local
docker compose -f infra/compose.local.yml up -d --build --wait
```

<http://localhost:8080/swagger> · sonde sur `/health`

En développement les secrets sont lus dans le **Secret Manager** ; en `Production` — y compris dans
la stack locale — dans les **variables d'environnement**. Le basculement se fait dans
`Api/App/DependencyInjection.cs`.

## Structure actuelle

```
server/
├── enbref.server.slnx
├── Directory.Build.props        suppression d'audit NuGet (AutoMapper, cf. le fichier)
├── Directory.Packages.props     versions centralisées
├── global.json                  SDK .NET 10
└── src/
    ├── Api/                     hôte web, contrôleurs, DI, Dockerfile
    ├── BuildingBlocks/
    │   ├── Application/         IAiAgent, IRepository, IObjectStorage*, logging
    │   └── Infrastructure/      LiteDB, ntfy, lecture RSS, HTTP, JSON
    └── Modules/EnBref/          le module métier — voir son README
```

Le détail du module et son flux de données : [`src/Modules/EnBref/README.md`](src/Modules/EnBref/README.md).

## Configuration

| Clé | Rôle |
|---|---|
| `OpenAiApiKey` | Génération du récap (à remplacer par l'API Claude) |
| `GithubToken` | Publication sur `yterraillon.github.io` — **vide en local** |
| `NtfyToken` | Notification d'échec de génération |

## Le job quotidien

Quartz déclenche `GenerateDailyRecapJob` à 17 h (`0 0 17 * * ?`, fuseau du conteneur =
`Europe/Paris`). Il enchaîne collecte, génération et publication, puis notifie sur ntfy en cas
d'échec.

Ce job tourne **aussi dans la stack locale** : laisser la stack allumée à 17 h avec un `GithubToken`
renseigné publierait sur le CDN de production.

## Écarts avec les règles du projet

À traiter au refactor. Détail en [§ 6 de l'architecture](../docs/architecture.md#6-état-réel-du-serveur)
et au [§ 9 du glossaire](../docs/ubiquitous-language.md#9-incohérences-connues).

| Cible | Réel |
|---|---|
| Vertical slices | `Modules/EnBref/{Application,Infrastructure}` |
| Pas de MediatR | MediatR 12, `ISender`, `IRequestHandler` |
| API Claude | Deux agents OpenAI enchaînés |
| 7 catégories fixes portant des brèves | `Section { Title, Text }` libre |
| Flux en configuration | Deux URLs en dur dans le handler |

**À traiter en priorité :** `GET /api/enbref/en-bref` déclenche une génération complète. L'endpoint
est anonyme, consomme des crédits LLM à chaque appel et écrase le récap publié.

## Tests

Les tests end-to-end Bruno sont à la racine du dépôt : [`tests/`](../tests/). Il n'y a pas encore de
tests unitaires côté serveur — leur emplacement reste à trancher.
