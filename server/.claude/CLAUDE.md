# CLAUDE.md — server

Conventions du serveur .NET 10 d'EnBref. Le vocabulaire métier vient de
`docs/ubiquitous-language.md`, qui fait autorité ; ce fichier ne couvre que l'architecture et le
style.

## Architecture — vertical slices, deux projets

Le serveur est reconstruit de zéro (le code importé de `myfanwy` a été supprimé). Une
fonctionnalité = **un dossier**, contenant son endpoint, son handler, ses modèles.

```
src/
├── Api/                     racine de composition : Program.cs, DI, ServerVersion (/health, back-office)
│   ├── Features/
│   │   ├── CollectHeadlines/ collecte, appelée en mémoire (génération, back-office)
│   │   ├── GenerateRecap/   Endpoint (Add…/Map…), Handler (+ Command, Result ; type → pipeline),
│   │   │                    IRecapWriter (GenerationAgent : prompt + validation, TestRecapWriter)
│   │   ├── ReadPublishedRecap/ lecture du récap publié à la source, appelée en mémoire (back-office)
│   │   └── ToggleDailyGeneration/ DailyGenerationSwitch : génération quotidienne démarrée ou arrêtée (back-office)
│   ├── Shared/              types servant à plusieurs slices : Recap (modèle, RecapType → libellé et artefact),
│   │                        RecapContract (JSON publié, ADR-008 : sérialisation et relecture)
│   └── BackOffice/          Blazor Server, servi sous /back-office
└── Infrastructure/          implémentations des dépendances sortantes
    ├── Collection/          IFeedReader : RssFeedReader (réel), FakeFeedReader (récap de test)
    ├── Llm/                 ILlmClient : AnthropicLlmClient (erreurs SDK → LlmStatus)
    └── Publication/         IPublicationRepository : GithubPublicationRepository (API Contents GitHub, publication et
                             lecture sans CDN ; erreurs → PublicationResult, ArtifactReadResult)
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
| LLM | Claude seul fournisseur (ADR-007) ; stubs en test, couche d'inférence à venir |
| Dépôt de publication | la destination peut changer, et le récap de test ne publie que sur `test.json` |

**LiteDB n'en fait pas partie.** L'historique est un détail interne ; un `IRepository<T>` posé « au
cas où » ajoute de l'indirection sans bénéfice.

Règle générale : une abstraction devient partagée quand un **deuxième** appelant la réclame, pas
avant. `Api/Shared/` ne contient que des types qui servent à au moins deux slices (le récap et son
contrat, depuis `ReadPublishedRecap`) : ce n'est pas un endroit où ranger les choses par défaut.

## Back-office — design

Avant tout travail d'interface du back-office, lire **[`server/design.md`](../design.md)** : lien
vers les maquettes (source de vérité, pas de copie dans le dépôt), correspondance maquettes ↔ pages
Blazor et articulation avec le design system.

## Ce qui est interdit

- **Un endpoint qui déclenche une génération sans intention explicite.** Une génération consomme
  des crédits et peut écraser le récap publié. Elle se déclenche par le job planifié, par une action
  explicite du back-office, ou par `POST /api/recaps/generations` (ADR-006).
  Aucun autre déclencheur, et jamais en `GET`.
- **Publier un récap de test ailleurs que sur `test.json`.** Il ne doit écraser ni `latest.json` ni
  `demo.json` (ADR-007). Cette garantie se tient dans le code, pas dans la configuration.
- **Appeler le LLM pour un récap de test.** Il ne consomme pas de crédits.
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
| `GithubToken` | publication — **vide en local**, sinon on écrase la production ; absent → publication en échec (502), lecture sans jeton (dépôt public) |
| `Anthropic:ApiKey` | API Claude — user secrets en local, `Anthropic__ApiKey` en production |
| `Anthropic:Model` | modèle Claude — `claude-haiku-4-5` en Development, `claude-opus-5-5` sinon |

Aucun secret en clair dans le dépôt, y compris dans `.agentsworkspace/`.

## Le job quotidien

Quartz, `0 0 17 * * ?`, fuseau du conteneur (`Europe/Paris`). Il enchaîne collecte, génération,
publication, puis notifie sur ntfy en cas d'échec. Il ne génère rien si la génération quotidienne
est arrêtée depuis le back-office (`DailyGenerationSwitch`, en mémoire : repart démarrée au
redémarrage).

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

Image Docker : contexte de build = **racine du dépôt** (ADR-009). `Api.csproj` lie `tokens.css`,
`bundle.css` et le logo de `design-system/` dans `wwwroot/design-system/`, sans copie.
