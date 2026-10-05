# Les trois récaps

EnBref produit trois sortes de récap. Elles partagent la même chaîne (collecte, génération,
publication) mais n'en parcourent pas les mêmes étapes et ne servent pas le même but. Le
vocabulaire vient de [`ubiquitous-language.md`](ubiquitous-language.md) ; les décisions, d'ADR-006
et ADR-007 ([`architecture-decision-record.md`](architecture-decision-record.md)).

| | Récap du jour | Récap de démo | Récap de test |
|---|---|---|---|
| **Code** | `RecapType.Daily` | `RecapType.Demo` | `RecapType.Test` |
| **Rôle** | Production | Boucle complète à la demande | Valider la publication uniquement |
| **Déclencheur** | Job quotidien, 17 h | Back-office, `POST /api/recaps/generations`, smoke test | Back-office, `POST /api/recaps/generations`, CI |
| **Collecte** | Flux RSS réels | Flux RSS réels | Fausse source |
| **LLM** | API Claude | API Claude | Aucun |
| **Crédits** | Oui | Oui | Non |
| **Artefact** | `latest.json` | `demo.json` | `test.json` |
| **Lu par** | L'application iOS | La revue App Store, les tests de chargement | Les vérifications de bout en bout |

## Récap du jour : la production

Le seul récap que lisent les clients en usage normal. Le job quotidien collecte les titres des flux
RSS réels, les confie à l'API Claude, puis publie le résultat sur `latest.json`. Un récap par jour.

## Récap de démo : une boucle complète, à la demande

Il parcourt la même chaîne que le récap du jour (flux RSS réels, API Claude, publication), mais
seulement sur demande explicite, et publie sur `demo.json`. C'est le seul moyen d'éprouver la
génération réelle hors du job quotidien.

Le résultat sert aussi de contenu figé pour la revue App Store : **chaque génération doit être
suivie d'une relecture manuelle** de `demo.json`.

## Récap de test : la publication, sans crédits

Il lit la fausse source et **n'appelle pas le LLM** : il ne consomme aucun crédit et ne vérifie pas
la génération. Ce qu'il prouve, c'est la publication de bout en bout, jusqu'au CDN, sur `test.json`.

Il n'écrase **jamais** `latest.json` ni `demo.json`. Cette garantie se tient dans le code, pas dans
la configuration ni dans les paramètres d'appel.
