# Ubiquitous language — EnBref

Ce glossaire fait autorité sur le vocabulaire métier d'EnBref. Il définit, pour chaque notion, **un
seul terme canonique**, sa correspondance FR (métier, libellés d'interface) ↔ EN (identifiants de
code), et les synonymes à proscrire.

**Il prime sur le code.** Si un identifiant existant contredit une entrée ci-dessous, c'est le code
qui est en écart : le signaler (§ 9) plutôt que de recopier l'écart dans du code neuf.

**Il ne couvre que le métier.** Les termes d'architecture (vertical slice, handler, endpoint…)
relèvent de `docs/architecture.md` et des `CLAUDE.md` de `server/`, `ios/` et `web/`.

**Une notion absente d'ici ne se nomme pas unilatéralement.** Proposer le terme canonique FR, les
identifiants EN envisagés, une définition en une à trois phrases et les alias à proscrire ; attendre
la validation ; puis ajouter l'entrée **dans le même changement que le code**.

---

## 1. Conventions

- **FR pour le métier et l'interface** : libellés affichés, documentation, messages d'erreur
  destinés à un humain, titres d'issues et de PR.
- **EN pour le code** : classes, propriétés, méthodes, champs JSON, routes, noms de fichiers.
- Le passage FR → EN est donné par chaque entrée. On ne le réinvente pas au cas par cas.

---

## 2. Le récap

### Récap
**Code** : `Recap` · **UI** : « Récap »

Le récapitulatif de l'actualité d'une journée, produit à partir des titres collectés dans les flux
RSS. Un récap et un seul par jour, identifié par sa date. Il contient exactement sept catégories,
chacune portant une à deux brèves.

**À ne pas dire** : résumé, digest, summary, briefing, newsletter, édition.

### Récap du jour
**Code** : `DailyRecap` · **UI** : « Récap du jour » · **Artefact** : `latest.json`

Le récap le plus récent. Produit chaque jour à 17 h par le job de génération, à partir de l'API
Claude, puis publié sur le dépôt de publication. C'est le seul récap que l'application iOS consomme
en usage normal.

**À ne pas dire** : récap courant, latest (employé seul), récap actuel.

### Récap de démo
**Code** : `DemoRecap` · **UI** : « Récap de démo » · **Artefact** : `demo.json`

Un récap figé, généré une fois puis relu et validé à la main. Il ne suit pas le cycle quotidien et
ne change qu'à la demande. Deux usages : la revue App Store, qui exige un contenu stable et
présentable, et la vérification qu'un client sait charger et afficher un récap sans dépendre de la
génération.

Il ne se régénère que sur demande explicite, et **chaque génération doit être suivie d'une relecture
manuelle** de `demo.json`. Le smoke test ne le touche pas : il vérifie la publication avec le récap
de test (ADR-007).

**À ne pas dire** : récap de test (c'est une autre notion, voir ci-dessous), fixture, mock.

### Récap de test
**Code** : `TestRecap` · **UI** : « Récap de test » · **Artefact** : `test.json`

Un récap produit à la demande à partir de la fausse source, **sans appel au LLM**, et publié sur
`test.json` — depuis la CI ou depuis le back-office. Il vérifie la chaîne de publication de bout en
bout sans consommer de crédits. Il n'écrase **jamais** `latest.json` ni `demo.json` (ADR-007).

**À ne pas dire** : récap de démo, récap jetable, dry run.

### Type de récap
**Code** : `RecapType` (`Daily`, `Demo`, `Test`) · **UI** : « Type de récap »

Ce que produit une génération : récap du jour, de démo ou de test. Le type détermine si le LLM est
appelé et l'artefact de publication ; un type `Test` n'appelle jamais le LLM et ne publie que sur
`test.json`, quoi que demande l'appelant.

**À ne pas dire** : mode, variante, variant, flavor, cible, target.

### Artefact
**Code** : `Artifact` · **UI** : « Artefact »

Le fichier publié sur le dépôt de publication pour un type de récap : `latest.json` (récap du
jour), `demo.json` (récap de démo) ou `test.json` (récap de test). L'artefact découle du type, dans
le code, jamais d'un paramètre d'appel ni de la configuration (ADR-007). Sa forme JSON est fixée par
ADR-008.

**À ne pas dire** : fichier, export, snapshot, dump.

---

## 3. Le contenu d'un récap

### Brève
**Code** : `Brief` · **UI** : « Brève »

L'unité de contenu d'un récap : un sujet d'actualité réduit à un titre et un résumé d'une phrase,
rattaché à une catégorie. Une catégorie porte une à deux brèves, soit sept à quatorze brèves par
récap.

**À ne pas dire** : item, entrée, article, actualité, news, card, sujet.

### Titre
**Code** : `Brief.Title` · **UI** : « Titre »

L'intitulé d'une brève, écrit par le LLM lors de la génération et affiché à l'utilisateur. À ne pas
confondre avec le **titre collecté** (§ 4), qui est la matière première brute extraite d'un flux et
n'est jamais affichée.

**À ne pas dire** : headline (réservé au titre collecté), intitulé, accroche, libellé.

### Résumé
**Code** : `Brief.Summary` · **UI** : « Résumé »

Le corps d'une brève : **une phrase** qui explique le sujet annoncé par le titre. Plafonné à
**200 caractères** — voir le budget de lecture (§ 7).

**À ne pas dire** : description, contenu, texte, body, synthèse.

### Catégorie
**Code** : `Category` · **UI** : « Catégorie »

Le thème d'actualité auquel une brève est rattachée. La liste est **fixe, fermée et ordonnée** : un
récap présente toujours ces sept catégories, dans cet ordre.

| Ordre | UI (FR) | Code (EN) |
|---|---|---|
| 1 | Politique | `Politics` |
| 2 | International | `International` |
| 3 | Économie | `Economy` |
| 4 | Société | `Society` |
| 5 | Technologies & Science | `TechnologyAndScience` |
| 6 | Sport | `Sport` |
| 7 | Culture | `Culture` |

Le LLM n'invente pas de catégorie : il range les brèves dans celles-ci.

**À ne pas dire** : rubrique, thème, section, sujet, tag, topic.

---

## 4. La matière première

### Flux
**Code** : `Feed` · **UI** : « Flux »

Un flux RSS publié par une source, interrogé lors de la collecte. La liste des flux est **en dur**
dans la configuration pour le moment ; elle sera gérable depuis le back-office ultérieurement.

**À ne pas dire** : canal, feed RSS, stream, abonnement.

### Source
**Code** : `Source` · **UI** : « Source »

Le média qui publie un flux. Une source peut publier plusieurs flux (par exemple un flux par
rubrique). Les sources ne sont **pas** citées dans le récap à ce stade — l'attribution est reportée
à une version ultérieure.

**À ne pas dire** : éditeur, média, publisher, provider, fournisseur.

### Titre collecté
**Code** : `Headline` · **UI** : « Titre collecté »

Le titre brut d'un article, extrait tel quel d'un flux lors de la collecte. C'est la matière
première de la génération. Un titre collecté **n'est jamais affiché** à l'utilisateur : il alimente
le LLM, qui produit des brèves.

**À ne pas dire** : titre (employé seul — réservé au titre d'une brève, § 3), article, item, entrée.

### État du flux
**Code** : `FeedStatus` (`Available`, `Unreachable`, `Invalid`, `Empty`) · **UI** : « État du flux »

Ce que la collecte constate sur un flux : exploitable (au moins un titre collecté), injoignable,
illisible (ni RSS ni Atom), ou vide. Un flux qui n'est pas exploitable n'interrompt pas la collecte
des autres.

**À ne pas dire** : statut, santé, health. « Disponibilité » reste réservé aux LLM (§ 6) ; seule
exception assumée, la valeur `Available` (« exploitable »), choisie parce qu'elle se lit d'elle-même.

---

## 5. Les traitements

### Collecte
**Code** : `Collection` · **UI** : « Collecte »

L'étape qui interroge les flux et en extrait les titres collectés du jour. Elle précède la
génération et ne fait appel à aucun LLM.

**À ne pas dire** : scraping, ingestion, fetch, crawl, agrégation.

### Résultat de collecte
**Code** : `CollectionResult` · **UI** : « Résultat de collecte »

L'état et les titres collectés de chaque flux interrogé, et la liste de tous les titres, **doublons
compris** : une dépêche reprise par plusieurs sources signale un sujet fréquent. La collecte aboutit
dès qu'au moins un titre est collecté ; sinon, la génération échoue.

**À ne pas dire** : rapport, fetch result, résultat RSS.

### Génération
**Code** : `Generation` · **UI** : « Génération »

L'étape qui transforme les titres collectés en un récap, via le LLM. Le job quotidien enchaîne
collecte puis génération à 17 h ; en cas d'échec, trois tentatives avant alerte sur ntfy.

**À ne pas dire** : build, création, compilation, traitement.

### Publication
**Code** : `Publication` · **UI** : « Publication »

L'étape qui dépose un récap sur le dépôt de publication, sous `latest.json`, `demo.json` ou
`test.json` selon le type de récap, via l'API GitHub.

Le dépôt de publication est abstrait dans le code par `IPublicationRepository`.

**À ne pas dire** : déploiement, push, upload, export. « Publisher » et « publieur » sont réservés :
le premier désigne une source (§ 4), le second n'est plus employé.

### Résultat de publication
**Code** : `PublicationResult` · **UI** : « Résultat de publication »

Ce que constate la publication d'un récap : le commit qui porte l'artefact publié, ou la cause de
l'échec. Une publication en échec fait échouer la génération qui l'a demandée.

**À ne pas dire** : résultat de push, résultat d'upload, rapport de publication.

### Historique
**Code** : `History` · **UI** : « Historique »

L'ensemble des récaps passés, conservés en base sur le serveur. L'historique est la source de vérité
interne ; il n'est pas exposé aux clients, qui ne lisent que les artefacts publiés.

**À ne pas dire** : archive, backlog, journal.

---

## 6. Le back-office

### Back-office
**Code** : `BackOffice` · **UI** : « Back-office »

L'interface d'administration, accessible **sur le réseau local uniquement**. Elle affiche l'état du
système et permet de déclencher une génération à la main.

**À ne pas dire** : admin, console, dashboard, interface d'admin, panneau.

### Crédits
**Code** : `Credits` · **UI** : « Crédits »

Le solde restant sur le compte du fournisseur de LLM, affiché au back-office.

**À ne pas dire** : quota, budget, solde, tokens.

### Disponibilité
**Code** : `Availability` · **UI** : « Disponibilité »

L'état de santé d'un LLM utilisé, tel que constaté par le serveur : joignable et répondant, ou non.

**À ne pas dire** : statut, santé, health, uptime, état.

---

## 7. Structure et budget de lecture

La forme d'un récap est contrainte. Ces règles ne sont pas indicatives : elles cadrent le prompt de
génération et le contrat consommé par les clients.

```
Récap                                   1 par jour, identifié par sa date
└── Catégorie × 7                       liste fixe, fermée, ordonnée (§ 3)
    └── Brève × 1 à 2                   soit 7 à 14 brèves par récap
        ├── Titre                       intitulé rédigé par le LLM
        └── Résumé                      une phrase, ≤ 200 caractères
```

**Le récap se lit en deux minutes au maximum.** À 220 mots par minute, cela donne environ 440 mots
pour quatorze brèves, soit une trentaine de mots par brève. D'où le plafond de 200 caractères sur le
résumé. Si une évolution assouplit l'une de ces bornes, l'autre doit être revue en conséquence — le
budget de lecture est la contrainte, le plafond de caractères n'en est que la traduction.

---

## 8. Récapitulatif des termes proscrits

| Ne pas dire | Dire |
|---|---|
| résumé, digest, briefing, newsletter | Récap |
| latest (seul), récap courant | Récap du jour |
| fixture, mock | Récap de démo |
| dry run, récap jetable | Récap de test |
| item, entrée, article, news, card | Brève |
| headline, intitulé, accroche | Titre |
| description, body, contenu, synthèse | Résumé |
| rubrique, thème, section, tag, topic | Catégorie |
| canal, stream, abonnement | Flux |
| éditeur, média, publisher | Source |
| titre (seul), item brut | Titre collecté |
| scraping, ingestion, crawl | Collecte |
| push, upload, déploiement | Publication |
| fichier, export, snapshot (d'un récap publié) | Artefact |
| publieur, publisher (pour le dépôt de publication) | Dépôt de publication (`IPublicationRepository`) |
| admin, console, dashboard | Back-office |
| quota, solde, tokens | Crédits |
| statut, santé, health, uptime | Disponibilité |

---

## 9. Incohérences connues

Écarts identifiés, assumés et en attente de correction. Les redécouvrir à chaque revue use la
confiance dans le rapport ; **en revanche, du code ou de la doc neufs qui les reproduisent sont
bloquants**.

### Écarts des artefacts publiés

Le serveur importé de `myfanwy` a été supprimé et le serveur est reconstruit de zéro (ADR-005). Ses
écarts de code ont disparu avec lui ; restent ceux des artefacts qu'il a publiés, et que lisent les
clients et le test Bruno quotidien.

| Publié aujourd'hui | Terme canonique | Nature de l'écart |
|---|---|---|
| `latest-recap.json` | `latest.json` | Chemin publié réel : `cdn/en-bref/data/latest-recap.json`. |
| `sections` (titre + texte libre) | `Category` portant des `Brief` | Pas de catégories fixes ni de brèves dans l'artefact publié. |
| `title` (« Récap du … ») | — | Un récap n'a pas de titre au glossaire ; le champ est asserté par le test Bruno quotidien. |

Absents du code à ce stade : l'historique, les
**crédits** et la **disponibilité**. Le back-office n'est qu'une coquille.

### Autres écarts

- **`.claude/CLAUDE.md` § Project Overview** — employait « récap de test » pour désigner le **récap
  de démo** (§ 2) ; corrigé. « Le récap du jour non caché » désigne un affichage back-office qui
  reste à nommer si on l'implémente.
- **Heure de génération** — 17 h, conformément au cron `0 0 17 * * ?` du job. L'ancien serveur mentionnait
  16 h comme cible ; c'est 17 h qui fait foi.
- **Accentuation et nombre des catégories** — le glossaire retient « Économie » (accentué) et
  « Technologies & Science » comme libellés d'interface. À confirmer au premier rendu réel dans
  l'application.
- **Plafond de 200 caractères sur le résumé** — dérivé du budget de lecture (§ 7), pas encore
  éprouvé sur une génération réelle. À réévaluer après le premier récap produit par l'API Claude.
- **Aucune attribution de source** — les brèves ne citent pas les titres collectés dont elles
  proviennent. C'est un choix assumé pour la première version, pas un oubli. Le jour où
  l'attribution arrive, le lien brève → titre collecté devra être nommé ici avant d'être implémenté.
