---
name: follow-up
description: Fin de story — trie les tâches restantes hors scope et les dispatche (sous-issues GitHub, checklist PR) pour qu'aucune ne soit oubliée
allowed-tools: Bash(gh:*), Bash(git:*), Read, Grep, Glob
argument-hint: [numéro d'issue parente]
---

Tu rassembles les tâches qui restent en fin de story et tu les ranges là où elles seront
effectivement revues. Sans ce tri, elles finissent dans un `.txt` de `.agentsworkspace/` — dossier
jetable et non committé — ou nulle part.

## Le principe : trois catégories, trois destinations

| | Catégorie | Exemple | Destination |
|---|---|---|---|
| **A** | À sa main, maintenant | parcours navigateur, lancer les suites Bruno | Checklist `## Suites` dans le corps de la PR + récap terminal |
| **B** | Après merge / déploiement | vérifier le 1er tag CalVer, le 1er récap publié | Même checklist `## Suites` dans la PR |
| **C** | Hors scope, durable | code hérité à supprimer, dette technique repérée en passant | **Une issue GitHub par finding**, ajoutée au board, rattachée à la story |

**Arbitrage des cas limites :** si l'item est encore utile dans un mois → **C** ; s'il est attaché à
un événement précis (merge, déploiement, release) → **B** ; sinon → **A**.

**Jamais d'issue fourre-tout** type « X – post release » : une issue = un finding, fermable seule.

## Étapes

### 1. Cadrage

- Branche courante : `git branch --show-current`.
- PR de la branche : `gh pr view --json number,url,body`. S'il n'y a pas de PR (session infra/ops,
  ou debrief avant ouverture), le skill fonctionne quand même : seules les catégories **A** et **C**
  sont produites, et A finit en récap terminal.
- Issue parente : l'argument `$1` s'il est fourni, sinon déduite du corps de la PR (`Closes #…`,
  `Fixes #…`) ou du nom de branche, sinon demande-la. Sans parent, les issues créées restent
  autonomes — ce n'est pas bloquant.

### 2. Collecte

Rassemble les restes de la session, sans en écarter par avance :

- ce que tu as annoncé ne pas pouvoir faire toi-même (validation visuelle, outil absent du PATH) ;
- les findings d'une review qui n'ont pas été corrigés ;
- les vérifications reportées à après le merge ou le déploiement ;
- les pistes explicitement écartées comme hors périmètre pendant l'implémentation.

Si des fichiers de `.agentsworkspace/` concernent la story, inclus leur contenu.

### 3. Tri

Classe chaque item en A / B / C selon la règle ci-dessus. Un item qui ne rentre nulle part est
probablement une tâche déjà faite ou du bruit : propose de le supprimer plutôt que de le forcer
dans une catégorie.

### 4. Validation — bloquante

Affiche le tableau *item / catégorie / destination proposée* et **attends l'accord**. L'utilisateur
peut reclasser une ligne, la supprimer, ou en ajouter une. **Aucune issue n'est créée et aucune PR
n'est modifiée avant cet accord.**

### 5. Dispatch

**C → une issue par finding.**

Contrôle de doublon d'abord : `gh issue list --search "<mots-clés>" --state open`. Si une issue
proche existe, propose d'y ajouter un commentaire plutôt que d'en ouvrir une seconde.

```bash
gh issue create --title "[TECH] …" --body-file <tmp> \
  --label <bug|enhancement si évident> --project "EnBref Development Board"
gh issue edit <nouvelle> --parent <story>
```

- Préfixe de titre conforme au backlog : `[SERVER]`, `[iOS]`, `[WEB]`, `[INFRA]`, `[TECH]`,
  `[DOC]`. Pas de préfixe si aucun ne s'applique.
- Corps **en français**, via `--body-file` (accents + multiligne), structuré ainsi :

```markdown
## Contexte
D'où vient le finding : story #M, PR #N, ou session infra du <date>.

## Constat
Ce qui a été observé, factuellement.

## Pourquoi c'était hors scope
Une phrase.

## Piste
L'option recommandée si tu en as une, sans la présenter comme tranchée.
```

- Vocabulaire métier conforme à `docs/ubiquitous-language.md`.
- Un finding de sécurité se dit comme tel dans le titre et le corps, sans le noyer.

**A + B → corps de la PR.**

Lis le body existant et **ajoute** la section ci-dessous à la fin — ne réécris jamais le reste du
corps rédigé par `create-pr` :

```markdown
## Suites

### À ta main avant merge
- [ ] Stack locale sur http://localhost:8080/swagger …

### Après merge
- [ ] Vérifier le tag GHCR `:2026.MM.DD.01` au 1er run
- [ ] Vérifier le récap publié après le job de 17 h

### Hors scope — suivi ailleurs
- #12 Supprimer le code Azure Blob mort du module
```

Puis `gh pr edit <n> --body-file <tmp>`. Les cases sont cochables directement dans la PR. Omets les
sous-sections vides. Si une section `## Suites` existe déjà (relance du skill), mets-la à jour au
lieu d'en ajouter une seconde.

**A sans PR** → récap terminal uniquement, rien de stocké.

### 6. Rapport final

Dans cet ordre : les URLs des issues créées avec leur catégorie, l'URL de la PR mise à jour, puis la
liste « à ta main » répétée en clair — c'est la seule qui demande une action immédiate.

## Ce que tu ne fais pas

- Tu ne corriges rien, même « évident » : les findings sont rangés, pas traités.
- Tu ne commites pas, tu ne pousses pas, tu ne merges pas.
- Tu ne fermes pas la story ni la PR.
- Tu ne lances pas les tests — c'est la CI qui fait foi.
- Tu ne crées jamais une issue sans la validation de l'étape 4.
