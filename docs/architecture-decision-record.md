# Architecture Decision Records

Les décisions structurantes d'EnBref, dans l'ordre où elles ont été prises.

**Règles d'usage :**

- Un ADR est ajouté **dans le même changement que le code** qu'il justifie, jamais après coup.
- On **n'édite pas** un ADR accepté. Un changement de décision se documente par un **nouvel** ADR
  qui remplace le précédent ; l'ancien passe en statut « Remplacé par ADR-NNN » et reste en place.
- Sont structurants : l'ajout ou le retrait d'une dépendance, un déplacement de frontière dans le
  serveur, une rupture du contrat publié, un changement du modèle de données, le pipeline CI/CD et
  le versionnement, tout mécanisme transverse (cache, jobs de fond, notifications), et toute
  fonctionnalité qui exigerait d'exposer le serveur sur internet.

**Statuts :** Proposé · Accepté · Remplacé par ADR-NNN.

**Format :** contexte, décision, conséquences — y compris celles qui coûtent. Un ADR qui ne liste
que des avantages n'a pas été écrit honnêtement.

---

## ADR-001 — Publier le récap sur GitHub Pages plutôt que le servir depuis l'API

**Date :** 2026-09-28 · **Statut :** Accepté

### Contexte

Le serveur tourne sur un NAS Synology domestique. Les clients (application iOS, et potentiellement
le site) doivent pouvoir lire le récap du jour à tout moment. L'exposer depuis l'API imposerait
d'ouvrir le NAS sur internet : reverse proxy, certificat, authentification, limitation de débit,
et une disponibilité tributaire de la connexion domestique.

### Décision

Le serveur **publie** le récap sur `yterraillon/yterraillon.github.io` via l'API GitHub. Les clients
lisent ce fichier statique. **Aucun port n'est ouvert** vers le serveur, dont les échanges réseau
sont tous sortants.

### Conséquences

- La disponibilité du récap est celle de GitHub Pages, pas celle du NAS.
- Le CDN absorbe la charge de lecture, quel que soit le nombre d'utilisateurs.
- La surface d'attaque se limite aux jetons sortants (GitHub, LLM).
- **Le récap publié est public.** Il n'y a pas de contrôle d'accès possible sur GitHub Pages.
- **Coût :** aucune fonctionnalité client → serveur n'est possible (historique dans l'app,
  personnalisation, contenu par utilisateur). Chacune nécessiterait un nouvel ADR.
- **Coût :** la publication est asynchrone et non transactionnelle. Un échec après génération laisse
  un récap en base sans équivalent publié.
- Le cache du CDN impose une revalidation par `ETag` côté client.

---

## ADR-002 — Vertical slices sans MediatR et sans modules

**Date :** 2026-09-28 · **Statut :** Remplacé par ADR-005

### Contexte

Le serveur a été importé de `myfanwy`, un hôte qui hébergeait plusieurs modules métier. Il en a
hérité l'architecture : un module `EnBref` découpé en projets `Application` et `Infrastructure`,
MediatR pour franchir les frontières, des abstractions de dépôt et de stockage d'objets.

EnBref n'a qu'un domaine, quelques cas d'usage, et un seul déclencheur réel — un job quotidien.

### Décision

Refactorer vers des **vertical slices** : une fonctionnalité par dossier, contenant son endpoint,
son handler et ses modèles. **Pas de MediatR** : les handlers sont appelés directement. **Pas de
modules** : le domaine est l'application.

Restent partagées les seules dépendances techniques transverses : lecture RSS, client LLM,
publication, LiteDB, notification.

### Conséquences

- Une fonctionnalité se lit de bout en bout dans un dossier.
- Une dépendance de handler devient visible dans sa signature, sans indirection par un bus.
- **Coût :** le refactor est important et touche tout le serveur existant.
- **Coût :** plus de pipeline behaviors. Le logging transverse, aujourd'hui fourni par
  `LoggingBehavior`, devra être réécrit explicitement ou porté par un middleware.
- **Coût :** sans frontière de projet, rien n'empêche mécaniquement un handler d'appeler
  directement un détail d'infrastructure. C'est un invariant que la revue devra tenir.

---

## ADR-003 — Génération par API Claude, récap de test par les modèles GitHub

**Date :** 2026-09-28 · **Statut :** Remplacé par ADR-007

### Contexte

Le code importé enchaîne deux agents OpenAI : un qui rédige le récap en texte libre, un second qui
le convertit en JSON. Cette mise en forme en deux temps est fragile — le second agent peut échouer à
produire du JSON valide — et double le coût de chaque génération.

Par ailleurs, vérifier la chaîne de génération en CI ne doit pas consommer de crédits de production.

### Décision

Le **récap du jour** est généré par l'**API Claude**, en un seul appel produisant directement la
structure attendue. Le **récap de test** emploie les **modèles GitHub**, gratuits pour cet usage.
Les deux passent par la même abstraction d'agent de génération.

### Conséquences

- Un seul appel LLM par récap au lieu de deux.
- La CI peut vérifier la chaîne complète sans coût ni clé de production.
- **Coût :** deux fournisseurs à maintenir, dont les sorties peuvent diverger. Le récap de test
  valide la mécanique, pas la qualité rédactionnelle de la production.
- La structure du récap est imposée au modèle, pas espérée de lui : les sept catégories et le
  plafond de 200 caractères sont des contraintes de sortie, à valider avant publication.

---

## ADR-004 — Versionnement CalVer des images serveur

**Date :** 2026-09-28 · **Statut :** Accepté

### Contexte

Le serveur est distribué en image Docker sur GHCR privé et déployé à la main sur le NAS via
Portainer. Il n'a pas d'API publique dont il faudrait signaler les ruptures : le seul contrat est le
récap publié, versionné indépendamment.

### Décision

Versionnement **CalVer** `YYYY.MM.DD.NN`, posé en tag Docker, en label OCI
`org.opencontainers.image.version` et en release GitHub.

### Conséquences

- Un tag dit quand l'image a été produite, ce qui est l'information utile pour un déploiement manuel.
- Plusieurs publications le même jour sont distinguées par `NN`.
- **Coût :** la version ne dit rien de la compatibilité. C'est acceptable tant que le serveur n'a
  qu'un consommateur — lui-même — mais cesserait de l'être si l'API devenait publique.

---

## ADR-005 — Reconstruction du serveur : projets Api et Infrastructure

**Date :** 2026-10-02 · **Statut :** Accepté · **Remplace :** ADR-002 (dont il reconduit tout, sauf l'interdiction du découpage en projets)

### Contexte

Refactorer le serveur importé s'est révélé plus coûteux que le réécrire : il a été supprimé et le
serveur est reconstruit de zéro. ADR-002 interdisait tout découpage en projets. Or le serveur a
deux natures distinctes : des cas d'usage (une génération, le reste en lecture pour audit) et des
adaptateurs vers l'extérieur (flux RSS, LLM, GitHub, LiteDB, ntfy).

### Décision

Deux projets :

- **`Api`** — racine de composition (`Program.cs`, DI), vertical slices sous `Features/<Slice>/`
  (endpoint, handler, modèles), et back-office Blazor Server sous `BackOffice/`, servi par le même
  hôte à `/back-office`.
- **`Infrastructure`** — implémentations des dépendances sortantes. `Api` référence
  `Infrastructure`, jamais l'inverse.

Le reste d'ADR-002 tient : **pas de MediatR**, **pas de modules**, handlers appelés directement.

Dépendances ajoutées : `Microsoft.AspNetCore.OpenApi` (document OpenAPI natif) et
`Swashbuckle.AspNetCore.SwaggerUI` (interface `/swagger`, attendue par la stack locale).

### Conséquences

- La frontière de projet empêche mécaniquement un adaptateur d'appeler un handler.
- Le back-office appelle les handlers en mémoire : un seul processus et un seul conteneur, sans
  appel HTTP interne. La question ouverte « Blazor séparé ou servi par l'API » est tranchée.
- **Coût :** les contrats (lecteur de flux, agent de génération, publieur) doivent vivre là où
  `Infrastructure` peut les implémenter, ce qui les éloigne des slices qui les consomment.
- **Coût :** le back-office partage le cycle de vie de l'API. Un plantage Blazor emporte le job
  planifié avec lui.

---

## ADR-006 — Déclencheur HTTP de génération protégé par clé

**Date :** 2026-10-02 · **Statut :** Accepté

### Contexte

Une génération ne devait partir que du job planifié ou du back-office. Or il faut aussi pouvoir la
déclencher à la main, depuis Bruno en développement ou pour un smoke test, sans attendre le
back-office. Le serveur n'est pas exposé sur internet (ADR-001), mais un endpoint anonyme reste
appelable par tout poste du réseau local, Swagger compris, et une génération consomme des crédits
et peut écraser le récap publié.

### Décision

`POST /api/recaps/generations` est un **troisième déclencheur** autorisé. Il exige le header
`X-Api-Key`, comparé en temps constant à la clé de configuration `GenerationApiKey`. Si la clé n'est
pas configurée, le endpoint est **fermé** (401) et non ouvert. Le job et le back-office appellent le
handler en mémoire, sans passer par la clé.

Le caractère LAN-only du serveur et du back-office est garanti par l'**infrastructure** (aucun port
publié vers internet sur le NAS), pas par le code : le back-office reste sans authentification.

### Conséquences

- Un appel accidentel ou anonyme sur le réseau local ne déclenche rien.
- Bruno peut tester la chaîne de génération sans back-office. Le smoke test s'en sert pour générer
  et publier un récap de démo, seul moyen de vérifier une publication réelle sans toucher
  `latest.json`.
- **Coût :** chaque smoke test remplace le récap de démo, qui doit ensuite être relu à la main.
- **Coût :** un secret de plus à gérer, en local comme en production.
- **Coût :** la clé ne distingue pas les types de récap. Qui la détient peut publier un récap du
  jour ; seul le handler peut restreindre ce qu'un appel a le droit de faire.
- **Coût :** si une règle réseau ouvrait le port par erreur, le back-office serait public. Rien dans
  le code ne l'empêche.

---

## ADR-007 — Récap de test sans LLM, publié sur un artefact dédié

**Date :** 2026-10-05 · **Statut :** Accepté · **Remplace :** ADR-003 (dont il reconduit la génération par l'API Claude en un seul appel)

### Contexte

ADR-003 confiait le récap de test aux modèles GitHub, gratuits, pour vérifier la chaîne en CI sans
consommer de crédits. Ces modèles n'existent plus : il ne reste qu'un fournisseur, l'API Claude.

Il faut pourtant toujours pouvoir vérifier la chaîne sans dépenser de crédits ni toucher au récap
lu par les clients. Or ce que seul un test de bout en bout prouve, c'est la **publication** :
l'écriture sur le dépôt de publication et la mise à disposition sur le CDN.

### Décision

- Le **récap du jour** et le **récap de démo** sont générés par l'**API Claude**, en un seul appel
  produisant directement la structure attendue (repris d'ADR-003).
- Le **récap de test** lit la fausse source, **n'appelle aucun LLM**, et est **publié** sur un
  artefact dédié, `test.json`. Il n'écrase jamais `latest.json` ni `demo.json`.
- L'artefact découle du type de récap, dans le code : aucun paramètre d'appel ni aucune
  configuration ne permet à un récap de test d'atteindre `latest.json` ou `demo.json`.

### Conséquences

- La CI et le back-office vérifient une publication réelle sans crédits ni relecture manuelle.
- Le smoke test n'a plus besoin de remplacer le récap de démo pour prouver une publication.
- **Coût :** plus rien ne vérifie l'appel au LLM sans consommer de crédits. La génération réelle
  n'est éprouvée que par le récap de démo et le job quotidien.
- **Coût :** un troisième artefact sur le CDN, lisible publiquement, au contenu sans intérêt.
- **Coût :** le récap de test emprunte désormais le chemin de publication ; la garantie qu'il
  n'atteint ni `latest.json` ni `demo.json` repose entièrement sur le code et doit être testée.
- L'abstraction LLM n'a plus deux implémentations à servir ; elle reste justifiée par les stubs de
  test et par la couche d'inférence prévue.
