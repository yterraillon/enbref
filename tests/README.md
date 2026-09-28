# tests

Tests end-to-end [Bruno](https://www.usebruno.com/).

## Ce qui est testé

`endtoend/enbref/` vérifie le **récap publié**, pas le serveur. C'est cohérent avec l'architecture :
le serveur n'est pas exposé, et ce que voient les clients est le fichier sur GitHub Pages. Si
l'artefact est correct et daté du jour, la chaîne complète — collecte, génération, publication — a
fonctionné.

| Test | Vérifie |
|---|---|
| `LatestRecapShouldBePublishedToday` | `latest-recap.json` est accessible, est du JSON conforme, et a été créé aujourd'hui |

## Lancer

Depuis l'interface Bruno, en ouvrant `endtoend/enbref/` comme collection. En ligne de commande :

```bash
npx @usebruno/cli run tests/endtoend/enbref
```

Les tests interrogent le **CDN de production** — ils ne nécessitent ni serveur local ni secret, mais
ils dépendent de ce qui a réellement été publié.

## Planification

Le test tourne chaque jour à 16h30 UTC, après la génération de 17 h (heure de Paris), via
`.github/workflows/e2e-recap-publication.yml`. Échec ⇒ alerte Discord.

> ⚠️ Ce workflow est référencé par [`docs/deployment.md`](../docs/deployment.md) et par la solution
> `server/enbref.server.slnx`, mais **n'existe pas encore** dans `.github/workflows/`.

## À trancher

L'emplacement des tests serveur (unitaires, intégration) n'est pas décidé : ici sous `tests/`, ou
dans `server/` aux côtés du code. Rien n'est écrit tant que le refactor du serveur n'a pas commencé.
