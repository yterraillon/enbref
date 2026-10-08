# tests

Tests end-to-end [Bruno](https://www.usebruno.com/). Les tests unitaires du serveur vivent dans
`server/tests/` (TUnit).

## Collections

| Collection | Cible | Vérifie |
|---|---|---|
| `endtoend/enbref/` | CDN de production | `LatestRecapShouldBePublishedToday` : le récap du jour publié est du JSON valide, daté du jour |
| `endtoend/dev/` | serveur local | le déclencheur de génération répond 200 |
| `endtoend/smoke/` | serveur + CDN | `/health` répond avec la version, puis génère et publie un **récap de test** et vérifie que `test.json` a été republié, dans la forme d'ADR-008 |

`enbref/` ne nécessite ni serveur ni secret.

`smoke/` publie sur `test.json` uniquement (ADR-007) : le récap de test n'appelle pas le LLM, ne
consomme aucun crédit et ne touche ni `latest.json` ni `demo.json`. Il exige un serveur dont le
`GithubToken` est renseigné (sans lui, la génération répond 502), et attend 90 s le déploiement de
GitHub Pages.

⚠️ `dev/GenerateRecapDaily` appelle l'API Claude : chaque lancement consomme des crédits.

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
