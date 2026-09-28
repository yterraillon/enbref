# Déploiement

Deux environnements pour l'instant : la stack Docker **locale** et la **production**. Pas de tier
preview.

| | Local | Production |
|---|---|---|
| Orchestration | `infra/compose.local.yml` | `stacks/apps/enbref/docker-compose.yml` du dépôt `checquy` |
| Image | buildée depuis `server/` | `ghcr.io/yterraillon/enbref/enbref-server:<CalVer>` |
| Hôte | poste de dev | NAS Synology `therook`, derrière SWAG |
| Données | `infra/data/` | `/volume1/docker/enbref/data` |

## Image

`.github/workflows/release-server.yml` construit et pousse l'image à chaque push sur `main` touchant
`server/**` (ou à la demande). Version **CalVer** `YYYY.MM.DD.NN`, posée en tag Docker, en label OCI
`org.opencontainers.image.version` et en release GitHub.

Dockerfile : `server/src/Api/Dockerfile`, contexte de build `./server`.

## Local

```bash
cp infra/.env.local.example infra/.env.local   # renseigner OpenAiApiKey et NtfyToken
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

Secrets injectés en variables d'environnement par la stack : `OpenAiApiKey`, `GithubToken` et
`NtfyToken`.

Mise à jour : relever le tag CalVer dans le compose, puis redéployer la stack.

## Publication du récap

Le récap n'est pas servi par le serveur : il est **publié** sur
`yterraillon/yterraillon.github.io`, sous `cdn/en-bref/data/latest-recap.json`, et les clients le
lisent là. Le serveur n'a donc pas besoin d'être joignable depuis l'extérieur pour que le récap soit
consultable.

Le test Bruno `tests/endtoend/enbref/` vérifie chaque jour à 16h30 UTC que l'artefact publié est bien
daté du jour (`.github/workflows/e2e-recap-publication.yml`, alerte Discord en cas d'échec).

## Migration depuis myfanwy

EnBref tournait dans le conteneur `myfanwy` (`ghcr.io/yterraillon/myfanwy`), sur le même NAS. Pendant
la transition, les deux applications embarquent le job de 17:00 et publieraient **toutes deux** sur
`latest-recap.json`.

Ordre de bascule :

1. Déployer la stack `enbref` en production.
2. Dans le même temps, retirer l'enregistrement Quartz du module EnBref de myfanwy
   (`Modules/EnBref/EnBref.Infrastructure/DependencyInjection.cs`, blocs `AddQuartz` et
   `AddQuartzHostedService`) et redéployer myfanwy.
3. Supprimer le workflow `e2e-testing-recap-publication.yml` de myfanwy : il vit désormais ici.
4. Après quelques jours de production verte, retirer le module d'EnBref de myfanwy.

Les données ne sont pas reprises : le volume `enbref` démarre vide et une base `EnBref.db` neuve est
créée au premier démarrage. Les métriques historiques restent dans `Myfanwy.db`.
