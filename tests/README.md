# tests

Tests end-to-end [Bruno](https://www.usebruno.com/). Les tests unitaires du serveur vivent dans
`server/tests/` (TUnit).

## Collections

| Collection | Cible | Vérifie |
|---|---|---|
| `endtoend/enbref/` | CDN de production | `LatestRecapShouldBePublishedToday` : le récap du jour publié est du JSON valide, daté du jour |
| `endtoend/dev/` | serveur local | le déclencheur de génération répond 200 |
| `endtoend/smoke/` | serveur + CDN | `/health` répond avec la version, puis génère et publie un **récap de démo** et vérifie que `demo.json` a été republié |

`enbref/` ne nécessite ni serveur ni secret.

⚠️ `smoke/` écrase `demo.json` sur le CDN — **relire `demo.json` à la main après chaque lancement**
(glossaire, Récap de démo) : il exige un serveur dont le `GithubToken` est renseigné,
et attend 90 s le déploiement de GitHub Pages. Il échouera tant que la génération n'est pas
implémentée (étape 2).

## Lancer

```bash
cd tests/endtoend/dev   && npx @usebruno/cli run --env local
cd tests/endtoend/smoke && npx @usebruno/cli run --env local
cd tests/endtoend/enbref && npx @usebruno/cli run
```

## Planification

`smoke/` tourne à chaque pull request, contre l'image Docker démarrée dans le runner
(`.github/workflows/pr-server.yml`, environnement `ci`). `enbref/` et `dev/` se lancent à la main :
le test quotidien du récap publié a été retiré.
