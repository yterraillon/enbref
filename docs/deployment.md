# Déploiement

Deux environnements pour l'instant : la stack Docker **locale** et la **production**. Pas de tier
preview.

| | Local | Production |
|---|---|---|
| Orchestration | `infra/compose.local.yml` | `stacks/apps/enbref/docker-compose.yml` du dépôt `checquy` |
| Image | buildée depuis `server/` et `design-system/` | `ghcr.io/yterraillon/enbref/enbref-server:<CalVer>` |
| Hôte | poste de dev | NAS Synology `therook`, derrière SWAG |
| Données | `infra/data/` | `/volume1/docker/enbref/data` |

## CI/CD

| Workflow | Déclencheur | Fait |
|---|---|---|
| `build-server.yml` | push sur une branche ≠ `main` (`server/**`, `design-system/**`) | restore + build Release |
| `pr-server.yml` | PR vers `main` | build, tests unitaires TUnit, puis smoke test Bruno (`tests/endtoend/smoke/`) sur l'image Docker démarrée dans le runner |
| `release-server.yml` | push sur `main` (merge), ou à la demande | image poussée sur GHCR (`:<CalVer>` et `:latest`), tag et release GitHub |

Version **CalVer** `YYYY.MM.DD.NN` (ADR-004), calculée sur les tags du jour, passée au build par
`--build-arg APP_VERSION` : elle devient l'`InformationalVersion` de l'assembly et le label OCI
`org.opencontainers.image.version`. Le serveur l'expose sur `/health` et `/back-office/settings` ; hors
image, elle vaut `dev`. L'argument ne s'appelle pas `VERSION` : MSBuild lirait la variable
d'environnement comme `$(Version)` et la restauration échouerait.

Le smoke test de PR génère et publie le **récap de test** sur `test.json` (ADR-007) : il lit le
secret `ENBREF_GITHUB_TOKEN`, mais n'appelle pas le LLM et n'a donc besoin d'aucune clé Anthropic.
Le job reste en `continue-on-error` jusqu'à un premier run vert de la publication.

Le test quotidien du récap publié (`e2e-recap-publication.yml`) a été retiré ; la collection
`tests/endtoend/enbref/` reste lançable à la main.

Dockerfile : `server/src/Api/Dockerfile`, contexte de build : la racine du dépôt (`.dockerignore`
racine, qui ne laisse passer que `server/` et `design-system/`) — ADR-009.

## Local

```bash
cp infra/.env.local.example infra/.env.local
docker compose -f infra/compose.local.yml up -d --build --wait
docker compose -f infra/compose.local.yml logs -f
docker compose -f infra/compose.local.yml down -v   # arrêt + purge des données
```

L'API répond sur <http://localhost:8080/swagger>, la sonde sur `/health`.

⚠️ Laisser `GithubToken` **vide** en local : renseigné, le job quotidien de 17:00 publie sur le CDN
de production comme le ferait la prod.

## Production

La stack vit dans le dépôt d'infrastructure `checquy`, aux côtés des autres services auto-hébergés :
`stacks/apps/enbref/docker-compose.yml`. Elle suit les conventions maison — réseau `appNet`,
`read_only: true`, `cap_drop: ALL`, `no-new-privileges`, logging json-file 10m/3, PUID/PGID du NAS.

Secrets injectés en variables d'environnement par la stack : `GithubToken`.
Les clés du LLM et de ntfy seront définies à l'étape 2 de la reconstruction du serveur.

**Le serveur ne doit jamais être publié vers internet** : aucune règle de pare-feu, de reverse proxy
ni de redirection de port vers EnBref. C'est l'infrastructure, et elle seule, qui garantit que le
back-office (sans authentification) reste sur le réseau local (ADR-006).

Mise à jour : relever le tag CalVer dans le compose, puis redéployer la stack.

## Publication du récap

Le récap n'est pas servi par le serveur : il est **publié** sur
`yterraillon/yterraillon.github.io` par l'API GitHub (`Infrastructure/Publication/GithubPublicationRepository`),
sous `cdn/en-bref/data/`, et les clients le lisent là. Le serveur n'a donc pas besoin d'être
joignable depuis l'extérieur pour que le récap soit consultable.

| Type de récap | Artefact |
|---|---|
| Récap du jour | `latest.json` |
| Récap de démo | `demo.json` |
| Récap de test | `test.json` |

Forme JSON : ADR-008. Sans `GithubToken`, la publication échoue (502) sans rien appeler.
`latest-recap.json`, publié par l'ancien serveur, n'est plus mis à jour par le serveur reconstruit.
Aucun client publié ne le lit : l'application iOS n'est pas encore sortie.

Le test Bruno `tests/endtoend/enbref/` vérifie que l'artefact publié est bien daté du jour. Il n'est
plus planifié (voir § CI/CD).

## Migration depuis myfanwy

EnBref tournait dans le conteneur `myfanwy` (`ghcr.io/yterraillon/myfanwy`), sur le même NAS. Pendant
la transition, les deux applications embarquent le job de 17:00 : myfanwy publie
`latest-recap.json` (ancienne forme), enbref publie `latest.json` (ADR-008). Les deux artefacts ne
se marchent pas dessus, et aucun client publié ne lit l'un ou l'autre : l'ordre de bascule n'a pas
de contrainte de compatibilité.

Ordre de bascule :

1. Déployer la stack `enbref` en production.
2. Dans le même temps, retirer l'enregistrement Quartz du module EnBref de myfanwy
   (`Modules/EnBref/EnBref.Infrastructure/DependencyInjection.cs`, blocs `AddQuartz` et
   `AddQuartzHostedService`) et redéployer myfanwy.
3. Supprimer le workflow `e2e-testing-recap-publication.yml` de myfanwy : il vit désormais ici.
4. Après quelques jours de production verte, retirer le module d'EnBref de myfanwy.

Les données ne sont pas reprises : le volume `enbref` démarre vide et une base `EnBref.db` neuve est
créée au premier démarrage. Les métriques historiques restent dans `Myfanwy.db`.
