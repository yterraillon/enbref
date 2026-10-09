# Design du back-office

Maquettes du back-office : <https://claude.ai/artifact/WWqJKhC4Ae6XMKufzgJ4Z7#page-dcb81aee8d83>

L'artefact (canvas « EnBref — Admin », privé) est la source de vérité ; il n'en existe pas de copie
dans le dépôt. Un instantané daté ne s'ajoutera que si les maquettes sont figées.

## Écrans

Tels que nommés dans les maquettes, face aux pages Blazor (`src/Api/BackOffice/Pages/`) :

| Maquette | Page actuelle | Route |
|---|---|---|
| Aujourd'hui | `Home.razor` (« Accueil ») | `/back-office` |
| Récaps | `Recaps.razor` | `/back-office/recaps` |
| Sources | `Feeds.razor` (« Flux ») | `/back-office/feeds` |
| Réglages | — (`Llm.razor`, `Info.razor` en partie) | — |

## Maquettes et design system

Les maquettes appliquent le design system EnBref. Côté code, le back-office importe
`design-system/tokens.css` puis `design-system/components/bundle.css`, et n'utilise que les
variables CSS (`var(--accent)`…), jamais de valeur en dur ; lire `design-system/README.md` avant
tout travail d'interface. En cas d'écart, le design system fait foi pour les valeurs (couleurs,
espacements, typographie), la maquette pour les écrans (contenu, disposition, parcours).

Le vocabulaire affiché suit le glossaire (`docs/ubiquitous-language.md`), y compris quand une
maquette emploie un autre mot.
