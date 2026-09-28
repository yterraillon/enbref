# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EnBref est une application qui envoie un résumé quotidien de l'actualité, basés sur les titres de flux RSS avec un un monorepo structuré comme suis:
- **server/** - .NET 10 API with Clean Architecture (see `server/.claude/CLAUDE.md`)
- **ios/** - Swift 6 / SwiftUI app that displays the recap (see `ios/.claude/CLAUDE.md`)
- **web/** - A décider / interface d'admin. Affiche : les crédits restants sur l'api, la disponibilité des LLMs utilisés, le récap du jour non caché (re-pull à chaque fois), le récap de test, permet de générer le récap (ou le récap de test) à la main. affichera plusieurs métriques utiles 

## Ubiquitous Language

Le glossaire du domaine est dans **[`docs/ubiquitous-language.md`](../docs/ubiquitous-language.md)**. Il fait autorité sur le vocabulaire : un terme canonique par notion, sa correspondance FR (métier/UI) ↔ EN (code), et les synonymes interdits.

**Deux règles, sans exception :**

1. **Notion existante — utiliser le terme du glossaire.**
   Avant de nommer une classe, une variable, une route, un champ de DTO ou un libellé d'interface, vérifier si la notion y figure et réutiliser le terme canonique tel quel. Ne jamais introduire un synonyme d'un terme déjà défini, même localement. Si le code existant contredit le glossaire, c'est le glossaire qui a raison : le signaler plutôt que de recopier l'écart (voir sa section « Incohérences connues »).

2. **Notion nouvelle — la définir ensemble avant de l'implémenter.**
   Ne pas nommer unilatéralement une notion métier qui n'est pas dans le glossaire. Proposer le terme canonique FR, les identifiants de code envisagés, une définition en une à trois phrases et les alias à proscrire ; attendre la validation ; puis ajouter l'entrée dans `docs/ubiquitous-language.md` **dans le même changement que le code**, jamais après coup.

Le glossaire couvre le **métier uniquement**. Les termes d'architecture restent dans `docs/architecture.md` et les `CLAUDE.md` de `server/` et `ios/`.

## Environments

TODO

See `docs/deployment.md` for full deployment documentation.


## Récap de démo

TODO. Disponible sur github. Peut etre généré from scrach depuis l'api. Pas généré quotidiennement, mais seulement à la demande pour des fins de testing. 


## Stack Docker locale

TODO

## Testing

Bruno pour l'API et les tests end-to-end (`server/tests/endtoend/`)
TODO


## General Behavior

- At the end of each work step, show me the remaining TODOs you have in memory
- Show me the changes you are going to make before applying them
- En fin de story, propose `/follow-up` : il trie les tâches hors scope restantes et les range là où elles seront revues (sous-issues GitHub pour les findings durables, checklist `## Suites` dans la PR pour le reste). Voir `.claude/skills/follow-up/SKILL.md`
