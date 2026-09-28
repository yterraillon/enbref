---
name: spike
description: Analyse une issue GitHub et rédige un plan d'implémentation, ou pose des questions si des infos manquent
allowed-tools: Bash(gh:*), Read, Grep, Glob
argument-hint: [repo] [issue-number]
---
spike org-name/repo-name issue-number
Tu analyses l'issue $2 du repo $1.

1. Récupère l'issue : `gh issue view $2 -R $1 --json title,body,comments,labels`
2. Explore le codebase pour identifier les fichiers et composants concernés.
3. Remplis la **Definition of Ready** (§ 3 ci-dessous). C'est elle qui tranche :
   un seul critère en ❌ et l'issue n'est pas prête.

**Si aucun critère DoR n'est en ❌ :**
- Poste un commentaire unique au format « Format du commentaire » ci-dessous
  (Spécifications + Plan + DoR) : `gh issue comment $2 -R $1 --body "..."`
- Ajoute le label : `gh issue edit $2 -R $1 --add-label "spike-done"`
- Déplace la carte en `Ready` sur le board (voir « Board » ci-dessous).

**Sinon :**
- Poste un commentaire contenant la DoR remplie — les ❌ montrent ce qui
  manque — suivie des questions précises et bloquantes. Pas de plan : tant
  qu'une réponse manque, un plan serait une hypothèse déguisée en décision.
- Ajoute le label : `gh issue edit $2 -R $1 --add-label "spike-blocked"`
  (pas de déplacement sur le board : l'issue reste où elle est.)

Ne modifie AUCUN fichier du repo. Analyse et commentaires uniquement.

## Contexte du projet

EnBref produit un récap quotidien de l'actualité à partir de titres RSS, le publie sur GitHub Pages,
et une app iOS le lit. **Le serveur n'est pas exposé sur internet** (ADR-001) et il n'y a ni compte
ni utilisateur.

⚠️ **Le serveur de `server/` vient d'être importé de myfanwy et n'est pas conforme** aux règles du
projet (MediatR, modules, OpenAI, modèle de récap différent de la cible). Voir
`docs/architecture.md` § 6. Un plan qui s'appuie sur ces structures pour du code neuf est à
signaler ; un plan qui les refactore est légitime.

## Format du commentaire

Un seul commentaire, trois sections séparées par `---`. Ne mélange jamais le
*quoi* et le *comment* : les specs se relisent avec le métier, le plan avec le
code, et les deux ne vieillissent pas au même rythme.

### 1. Spécifications — le *quoi*

Ce que le système doit faire. Aucun nom de fichier, de classe ou de table dans
cette section. Vocabulaire de `docs/ubiquitous-language.md` obligatoire.

```markdown
## 📋 Spécifications

### Besoin
<1 à 3 phrases : qui, quoi, pourquoi.>

### Comportement attendu
<Règles métier. Cas nominal, puis cas limites et cas d'erreur.>

### Critères d'acceptation
- [ ] <Observable et vérifiable de l'extérieur, pas « le service X renvoie Y ».>

### Hors périmètre
- <Ce qui a été écarté explicitement, pour éviter la dérive en revue.>
```

### 2. Plan d'implémentation — le *comment*

```markdown
## 🔧 Plan d'implémentation

### Étapes
1. <Une étape = un changement cohérent et testable isolément.>

### Fichiers touchés
| Fichier | Nature du changement |
|---|---|

### Risques et points d'attention
- <Ce qui peut casser ailleurs, ce qui est irréversible, ce qui est incertain.>

### Tests
- <Bruno end-to-end, vérification manuelle, tests serveur si l'emplacement est
  tranché d'ici là.>

### Impacts architecture → ADR
- <Sujet structurant → décision à acter, en une ligne.>
```

**La sous-section « Impacts architecture → ADR » est toujours présente.** Si
rien n'est structurant, écris-le noir sur blanc : « Aucun impact architecture
identifié. » Une absence de section se lit comme un oubli, pas comme un
constat.

Sont structurants : l'ajout ou le retrait d'une dépendance, une rupture du
contrat publié, toute fonctionnalité qui exigerait d'exposer le serveur, un
changement du modèle de données, un changement de fournisseur LLM, le pipeline
CI/CD et le versionnement, et tout mécanisme transverse (cache, jobs de fond,
notifications).

Le spike **n'écrit pas l'ADR** — il le signale. La rédaction se fait dans
`docs/architecture-decision-record.md`, dans le même changement que le code, en
ajoutant un ADR numéroté en fin de fichier (on n'édite pas un ADR existant).

### 3. Definition of Ready

Tableau final, toujours complet. Statuts : `✅` couvert · `⚠️` hypothèse à
confirmer · `❌` bloquant · `➖` sans objet. Chaque `⚠️` et chaque `❌` porte une
note d'une ligne — un statut sans justification n'est pas relisable.

```markdown
## ✅ Definition of Ready

| Critère | Statut | Note |
|---|---|---|
| Critères d'acceptation observables et testables | | |
| Contrat publié : le récap change-t-il de forme ? | | |
| Le serveur reste-t-il non exposé (ADR-001) ? | | |
| Vocabulaire conforme au glossaire (ou notion nouvelle à définir) | | |
| Forme du code neuf : vertical slice, sans MediatR ni module | | |
| Coût LLM : la génération reste-t-elle non déclenchable par accident ? | | |
| Secrets : nouvelle clé de configuration à documenter ? | | |
| Impact architecture → ADR (cf. plan) | | |
```

Précisions sur trois lignes qui se remplissent souvent à la légère :

- **Contrat publié** — `latest.json` est la seule chose que connaissent les
  clients. Tout champ ajouté, renommé ou supprimé impacte l'app iOS **et** le
  test Bruno `tests/endtoend/`. Rappel : un champ optionnel côté Swift avale
  silencieusement une clé renommée — ça compile, et l'écran se vide. `➖` n'est
  légitime que si rien de ce qui est publié ne bouge.
- **Serveur non exposé** — un endpoint destiné à l'app, un historique
  consultable depuis le client, une notification push ciblée : tout cela
  contredit ADR-001 et demande un ADR de remplacement. Le back-office reste
  LAN-only, ce qui est la raison pour laquelle il n'a pas d'authentification.
- **Coût LLM** — une génération ne se déclenche que par le job planifié ou une
  action explicite du back-office. Le récap de test ne publie jamais, et cette
  garantie tient dans le code, pas dans un `GithubToken` laissé vide.

Ce tableau est relu tel quel à la contre-étude : il doit se suffire à lui-même,
sans relire le plan.

## Board

EnBref Development Board — projet `6`, owner `yterraillon`. IDs stables :

| | |
|---|---|
| Project ID | `PVT_kwHOAKHwqs4BOqzm` |
| Champ `Status` | `PVTSSF_lAHOAKHwqs4BOqzmzg9TVo8` |
| Option `Ready` | `73e13010` |

Autres options du champ `Status`, si besoin : `Ideas` `b8fdc431`, `Todo`
`f75ad846`, `In Progress` `47fc9ee4`, `Done` `98236657`.

**1. Récupère l'ID de la carte** (`--limit` est obligatoire : le défaut de 30
tronque la liste et l'issue passerait pour absente) :

```bash
gh project item-list 6 --owner yterraillon --limit 200 --format json \
  --jq '.items[] | select(.content.repository == "$1" and .content.number == $2) | .id'
```

**2. Déplace-la** en injectant l'ID obtenu :

```bash
gh project item-edit --id <ITEM_ID> \
  --project-id PVT_kwHOAKHwqs4BOqzm \
  --field-id PVTSSF_lAHOAKHwqs4BOqzmzg9TVo8 \
  --single-select-option-id 73e13010
```

Si l'étape 1 ne renvoie rien, l'issue n'est pas sur le board : signale-le dans
ta réponse et n'ajoute pas la carte toi-même.
