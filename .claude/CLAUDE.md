# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EnBref produit un résumé quotidien de l'actualité à partir des titres de flux RSS. Monorepo :

- **server/** — API .NET 10 (voir `server/.claude/CLAUDE.md`). Collecte, génération, publication du
  récap, et **back-office** Blazor accessible **sur le réseau local uniquement**. Le back-office
  affiche les crédits restants, la disponibilité des LLM utilisés, le récap du jour rechargé sans
  cache, le récap de démo, et permet de déclencher une génération à la main. D'autres métriques y
  seront ajoutées.
- **ios/** — application Swift 6 / SwiftUI qui affiche le récap (voir `ios/.claude/CLAUDE.md`)
- **web/** — landing page d'EnBref, HTML/CSS statique (voir `web/.claude/CLAUDE.md`)

**Le serveur n'est pas exposé sur internet.** Il publie le récap sur GitHub Pages ; les clients
lisent ce fichier statique. Toute fonctionnalité qui exigerait un appel client → serveur remet ce
choix en cause et relève d'un ADR (`docs/architecture-decision-record.md`, ADR-001).

## Ubiquitous Language

Le glossaire du domaine est dans **[`docs/ubiquitous-language.md`](../docs/ubiquitous-language.md)**.
Il fait autorité sur le vocabulaire : un terme canonique par notion, sa correspondance FR
(métier/UI) ↔ EN (code), et les synonymes interdits.

**Deux règles, sans exception :**

1. **Notion existante — utiliser le terme du glossaire.**
   Avant de nommer une classe, une variable, une route, un champ de DTO ou un libellé d'interface,
   vérifier si la notion y figure et réutiliser le terme canonique tel quel. Ne jamais introduire un
   synonyme d'un terme déjà défini, même localement. Si le code existant contredit le glossaire,
   c'est le glossaire qui a raison : le signaler plutôt que de recopier l'écart (voir sa section
   « Incohérences connues »).

2. **Notion nouvelle — la définir ensemble avant de l'implémenter.**
   Ne pas nommer unilatéralement une notion métier qui n'est pas dans le glossaire. Proposer le
   terme canonique FR, les identifiants de code envisagés, une définition en une à trois phrases et
   les alias à proscrire ; attendre la validation ; puis ajouter l'entrée dans
   `docs/ubiquitous-language.md` **dans le même changement que le code**, jamais après coup.

Le glossaire couvre le **métier uniquement**. Les termes d'architecture restent dans
`docs/architecture.md` et les `CLAUDE.md` de `server/`, `ios/` et `web/`.

⚠️ **L'artefact publié aujourd'hui par l'ancien serveur n'est pas conforme au glossaire**
(`latest-recap.json`, `sections`…). Les écarts sont recensés au § 9 ; **du code neuf qui les
reproduit est une régression**.

## Architecture

`docs/architecture.md` décrit la cible, `docs/architecture-decision-record.md` les décisions actées.

L'essentiel pour écrire du code serveur : **vertical slices, sans MediatR, sans modules**, dans deux
projets `Api` (slices, DI, back-office) et `Infrastructure` (dépendances sortantes) — ADR-005. Une
fonctionnalité = un dossier contenant son endpoint, son handler et ses modèles. Le serveur est
reconstruit de zéro sur ce modèle.

Un ADR accepté ne se contourne pas. Un changement structurant se documente par un ADR ajouté **dans
le même changement que le code**, et on n'édite jamais un ADR existant : on en ajoute un nouveau.

## Environments

Deux environnements, pas de tier preview. Détail dans `docs/deployment.md`.

| | Local | Production |
|---|---|---|
| Orchestration | `infra/compose.local.yml` | dépôt d'infrastructure `checquy` |
| Image | buildée depuis `server/` | `ghcr.io/yterraillon/enbref/enbref-server:<CalVer>` |
| Hôte | poste de dev | NAS Synology `therook` |

Versionnement **CalVer** `YYYY.MM.DD.NN` (ADR-004).

⚠️ La stack locale fait tourner le job de 17 h. Laisser `GithubToken` **vide** en local : renseigné,
une génération écrase le récap de **production**.

## Les trois récaps

Trois notions distinctes — ne pas les confondre, le glossaire les sépare explicitement :

| | Artefact | Généré | Rôle |
|---|---|---|---|
| **Récap du jour** | `latest.json` | Job quotidien 17 h, API Claude | Ce que lisent les clients |
| **Récap de démo** | `demo.json` | À la demande, puis figé et relu | Revue App Store, test de chargement |
| **Récap de test** | `test.json` | À la demande, fausse source, sans LLM | CI, vérification de la publication |

Le rôle de chacun est détaillé dans `docs/recaps.md`. Aucun n'est encore publié par le code.

## Stack Docker locale

```bash
cp infra/.env.local.example infra/.env.local
docker compose -f infra/compose.local.yml up -d --build --wait
docker compose -f infra/compose.local.yml logs -f
docker compose -f infra/compose.local.yml down -v   # arrêt + purge des données
```

API sur <http://localhost:8080/swagger>, sonde sur `/health`. La stack tourne en
`ASPNETCORE_ENVIRONMENT=Production` : c'est ce qui bascule le code sur les chemins conteneur et la
lecture des secrets en variables d'environnement.

## Testing

- **Unitaires** : TUnit, `server/tests/` (`Api.Tests`, `Infrastructure.Tests`).
- **End-to-end** : Bruno, `tests/endtoend/` — `enbref/` vérifie chaque jour le récap publié sur le
  CDN, `dev/` appelle le déclencheur de génération local, `smoke/` génère un récap de démo et vérifie
  sa publication. Voir `tests/README.md`.

## General Behavior

- À la fin de chaque étape de travail, montre-moi les TODOs qu'il te reste en mémoire
- Montre-moi les changements que tu vas faire avant de les appliquer
- En fin de story, propose `/follow-up` : il trie les tâches hors scope restantes et les range là où
  elles seront revues (sous-issues GitHub pour les findings durables, checklist `## Suites` dans la
  PR pour le reste). Voir `.claude/skills/follow-up/SKILL.md`
