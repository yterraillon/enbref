# Design du back-office

Maquettes du back-office : <https://claude.ai/artifact/WWqJKhC4Ae6XMKufzgJ4Z7#page-dcb81aee8d83>

L'artefact (canvas « EnBref — Admin », privé) est la source de vérité ; il n'en existe pas de copie
dans le dépôt. Un instantané daté ne s'ajoutera que si les maquettes sont figées.

## Écrans

Correspondance entre les écrans des maquettes et les pages Blazor (`src/Api/BackOffice/Pages/`).
Le libellé retenu fait foi quand il diffère de la maquette.

| Maquette | Libellé retenu | Page | Route |
|---|---|---|---|
| Aujourd'hui | « Accueil » | `Home.razor` | `/back-office` |
| Récaps | « Récaps » | `Recaps.razor` | `/back-office/recaps` |
| Sources | « Flux RSS » | `Feeds.razor` | `/back-office/feeds` |
| Réglages | « Réglages » | regroupe `Info.razor` et `Llm.razor` (à fusionner) | à définir |

« Sources » est écarté : au glossaire, la source est le média, le flux ce qu'il publie, et la page
liste des flux.

## Maquettes et design system

Les maquettes appliquent le design system EnBref. **La copie du dépôt (`design-system/`) fait foi**,
pas l'artefact de design system que référence le canvas. Côté code, le back-office importe
`design-system/tokens.css` puis `design-system/components/bundle.css`, et n'utilise que les
variables CSS (`var(--accent)`…), jamais de valeur en dur ; lire `design-system/README.md` avant
tout travail d'interface. En cas d'écart, le design system fait foi pour les valeurs (couleurs,
espacements, typographie), la maquette pour les écrans (contenu, disposition, parcours).

Le vocabulaire affiché suit le glossaire (`docs/ubiquitous-language.md`), y compris quand une
maquette emploie un autre mot.
