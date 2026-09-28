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

**Date :** 2026-09-28 · **Statut :** Accepté

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

**Date :** 2026-09-28 · **Statut :** Accepté

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
