# CLAUDE.md — server

Conventions du serveur .NET 10 d'EnBref. Le vocabulaire métier vient de
`docs/ubiquitous-language.md`, qui fait autorité ; ce fichier ne couvre que l'architecture et le
style.

## Architecture — vertical slices, deux projets

Le serveur est reconstruit de zéro (le code importé de `myfanwy` a été supprimé). Une
fonctionnalité = **un dossier**, contenant son endpoint, son handler, ses modèles.

```
src/
├── Api/                     racine de composition : Program.cs, DI
│   ├── Features/
│   │   ├── CollectHeadlines/ collecte, appelée en mémoire (génération, back-office)
│   │   └── GenerateRecap/   Endpoint (Add…/Map…), Handler, Command
│   ├── Shared/              uniquement ce qui sert à plusieurs slices
│   └── BackOffice/          Blazor Server, servi sous /back-office
└── Infrastructure/          implémentations des dépendances sortantes
    └── Collection/          IFeedReader : RssFeedReader (réel), FakeFeedReader (récap de test)
```

**Pas de MediatR.** Les handlers sont des classes ordinaires, injectées et appelées directement par
l'endpoint, le job Quartz ou le back-office.

**Pas de modules.** Le domaine est l'application.

**`Api` → `Infrastructure`, jamais l'inverse.** Pas de projet `Application` (ADR-005, qui remplace
ADR-002).

Chaque slice expose `Add<Slice>()` pour la DI et `Map<Slice>()` pour la route, appelés depuis
`Program.cs`.

### Quand une abstraction est justifiée

Trois dépendances sortantes doivent rester derrière un contrat, chacune pour une raison précise :

| Dépendance | Pourquoi l'abstraire |
|---|---|
| Flux RSS | les sources changent, le format aussi (RSS, Atom) |
| LLM | Claude en production, modèles GitHub pour le récap de test : deux implémentations |
| Dépôt de publication | la destination peut changer, et le récap de test ne doit **pas** publier |

**LiteDB n'en fait pas partie.** L'historique est un détail interne ; un `IRepository<T>` posé « au
cas où » ajoute de l'indirection sans bénéfice.

Règle générale : une abstraction devient partagée quand un **deuxième** appelant la réclame, pas
avant. `Shared/` n'est pas un endroit où ranger les choses par défaut.

## Ce qui est interdit

- **Un endpoint qui déclenche une génération sans intention explicite.** Une génération consomme
  des crédits et peut écraser le récap publié. Elle se déclenche par le job planifié, par une action
  explicite du back-office, ou par `POST /api/recaps/generations` (ADR-006).
  Aucun autre déclencheur, et jamais en `GET`.
- **Publier depuis un chemin de test.** Le récap de test ne doit écraser ni `latest.json` ni
  `demo.json`. Cette garantie se tient dans le code, pas dans la configuration.
- **Exposer le serveur.** Aucun endpoint n'est destiné à un client externe. Le back-office est
  LAN-only, garanti par l'infrastructure et non par le code (ADR-006) : c'est ce qui lui permet de
  se passer d'authentification.
- **Ajouter une dépendance sans ADR.** `Directory.Packages.props` centralise les versions ; une
  entrée nouvelle est une décision.
- **Mettre une règle métier dans un contrôleur, un job Quartz ou un composant Blazor.** Ces trois-là
  déclenchent et affichent ; ils ne décident pas.

## Le contrat publié

`latest.json` est le seul contrat d'EnBref, et la seule chose que connaissent les clients.

**Toute modification est une rupture** tant qu'une version déployée de l'app iOS lit l'ancienne
forme. Un champ renommé devient `nil` côté Swift sans lever d'erreur : l'écran se vide en silence.
Un changement de contrat exige un ADR et une vérification côté app — jamais un simple commit
serveur.

La forme cible est fixée au § 7 du glossaire : sept catégories ordonnées, une à deux brèves par
catégorie, un titre et un résumé de 200 caractères maximum par brève. **L'artefact publié
aujourd'hui (par l'ancien serveur) a une autre forme** : voir le § 9 du glossaire.

## Nommage C#

- Le vocabulaire métier suit le glossaire : `Recap`, `Brief`, `Category`, `Headline`, `Feed`,
  `Source`, `Collection`, `Generation`, `Publication`, `History`.
- `Headline` = titre brut RSS. `Brief.Title` = titre affiché. **Ne pas employer `Title` seul pour un
  titre collecté.**
- Anglais pour tout le code ; français pour les libellés d'interface, la documentation et les
  messages destinés à un humain.
- Un nom de fonctionnalité décrit l'intention : `GenerateDailyRecap`, `PublishRecap`.

## Configuration et secrets

Clés lues dans le Secret Manager en développement et dans les variables d'environnement en
`Production`. La clé ntfy sera définie à l'étape 2 de la reconstruction.

| Clé | Rôle |
|---|---|
| `GithubToken` | publication — **vide en local**, sinon on écrase la production |
| `Anthropic:ApiKey` | API Claude — user secrets en local, `Anthropic__ApiKey` en production |
| `Anthropic:Model` | modèle Claude — `claude-haiku-4-5` en Development, `claude-opus-5-5` sinon |

Aucun secret en clair dans le dépôt, y compris dans `.agentsworkspace/`.

## Le job quotidien

Quartz, `0 0 17 * * ?`, fuseau du conteneur (`Europe/Paris`). Il enchaîne collecte, génération,
publication, puis notifie sur ntfy en cas d'échec.

Il tourne **aussi dans la stack locale**. Une stack laissée allumée à 17 h avec un `GithubToken`
renseigné publie en production.

## Tests

- **Unitaires** : TUnit, `server/tests/Api.Tests/` et `server/tests/Infrastructure.Tests/`,
  à l'image de `src/`. Pas de bibliothèque de mock : des stubs écrits à la main. Lancés par `dotnet test --solution enbref.server.slnx` et par `build-server.yml`.
- **End-to-end** : Bruno, à la racine (`tests/endtoend/`) — voir `tests/README.md`.

## Build

```bash
dotnet build server/enbref.server.slnx
dotnet run --project server/src/Api
```

SDK .NET 10 (`global.json`), versions de paquets centralisées (`Directory.Packages.props`),
warnings traités en erreurs (`Directory.Build.props`).
