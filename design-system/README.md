EnBref, c'est un récap d'actualité par jour, lu en cinq minutes ou écouté. L'interface s'efface : elle reprend les conventions natives d'iOS (SF Pro, listes groupées, cartes blanches sur fond gris) et une seule couleur d'action, l'encre. Le même système sert l'app iOS et l'admin web.

## Principes

- **Natif d'abord.** Sur iOS, utiliser les composants système (`List` en style `.insetGrouped`, `Toggle`, `Picker` segmenté, `navigationTitle` large) et les habiller avec ces tokens. Ne recréer à la main que ce qui n'existe pas (carte rubrique, progression segmentée, mini-lecteur).
- **L'encre est l'accent.** `accent` (#111 en clair, blanc en sombre) est la seule couleur d'action. Pas de bleu système, pas de couleur de marque. Le vert `switch-on` n'apparaît que dans les interrupteurs.
- **Ce qui est lu se replie.** Une rubrique marquée comme lue devient une ligne de 56 px (`RubricCard`) ; la progression (`ProgressSegments`) montre d'un coup d'œil ce qui reste.
- **Rien à côté du texte.** Pas d'image, de source, de lien ni de publicité dans le récap.

## Contenu et ton

- Français, ton neutre et factuel, vouvoiement : « La notification arrive chaque jour à cette heure, même en mode Concentration Sommeil si vous l'autorisez. »
- Libellés courts, majuscule au premier mot seulement : « Heure du récap », « Aperçu des titres », « Marquer comme lu », « Relire ».
- Les en-têtes de section et les noms de rubrique sont saisis en casse normale (« Réception », « Politique ») ; c'est le style qui les met en capitales.
- Comptes en toutes lettres autour du chiffre : « 3 sur 7 rubriques lues », « 5 min ».
- Heures : `7:30` dans les contrôles (puces, pastille de valeur), « 7 h 30 » ou « 7 h » dans les phrases (« Prochain récap demain à 7 h 30 »).
- Dates longues sans abréviation : « Lundi 28 septembre ».
- Typographie française : guillemets « » avec espaces insécables, espace insécable avant `:` `;` `?` `!`, apostrophe typographique ’ dans le texte publié, séparateur de milliers espace (« 3 000 amendements »), virgule décimale (« 1,25× »).
- Titres de brève : une phrase informative, deux-points pour introduire le fait (« Budget 2027 : les députés ouvrent les débats »). Corps : deux ou trois phrases.
- Pas d'emoji. Le seul glyphe décoratif est le point « ● » devant « En lecture ».

## Couleur

- Fond d'écran `bg-grouped` ; tout contenu vit dans des cartes `surface` à `radius-card`. Les cartes n'ont ni ombre ni bordure.
- Texte : `label` pour titres et libellés, `label-body` pour le corps des brèves, `label-secondary` pour dates, en-têtes de section, durées et pieds. Ne pas poser `label-secondary` sur `fill-tertiary` (4,4:1 en clair).
- Actions et états sélectionnés : `accent` avec `on-accent` dessus.
- Progression : `progress-done`, `progress-current`, `progress-track`.
- Matières translucides : `material-bar` (barre de titre, flou 20 px) et `material-player` (mini-lecteur, flou 24 px).
- Admin web seulement : `danger`, `success`, `warning`, toujours accompagnés d'un mot (`StatusBadge`, message d'erreur de `TextField`).
- Deux thèmes, Clair et Sombre, suivis automatiquement (réglage Apparence : Clair / Sombre / Auto). En sombre, `accent` passe au blanc et `on-accent` au noir : utiliser les tokens, jamais `#fff` en dur sur un bouton.
- Contrastes : tous les textes atteignent 4,5:1 sur leurs fonds dans les deux thèmes. Exceptions conservées de la maquette en thème clair : `control-border` et `glyph-tertiary` (#C7C7CC, 1,6:1) — la pastille est toujours suivie de son libellé et le chevron est redondant avec la ligne cliquable.

## Typographie

- Une seule famille : SF Pro (police système), `--font-sans`. Sur le web, la pile retombe sur `system-ui` puis Segoe UI hors Apple. Ne charger aucune police web.
- Interface : `large-title` (34/700, un par écran), `headline` (17/600), `body` (17), `callout` (16/600), `subhead` (15), `control` (14/500), `eyebrow` (13/600, capitales), `footnote` (13), `caption` (12).
- Lecture : `article-title-m` + `article-body-m` par défaut ; `-s` et `-l` suivent le bouton « Aa » et Dynamic Type. Sur iOS, mapper sur les styles système pour hériter de Dynamic Type : titre de brève `.headline`, corps `.callout`, avec `@ScaledMetric` si une taille exacte est nécessaire.
- Chiffres tabulaires (`font-variant-numeric: tabular-nums`, `.monospacedDigit()` en SwiftUI) pour les heures et compteurs.
- `text-wrap: pretty` sur les titres et corps de brèves.

## Espacement et mise en page

- Gouttière iPhone `space-16` entre bord d'écran et carte ; padding interne des cartes et inset des lignes `space-18` ; barre de titre `space-20`.
- Entre deux cartes `space-12` ; avant un en-tête de section `space-26` ; en-têtes et pieds de section retirés de `space-36`.
- Hauteurs fixes : lignes `row-height` (52), `row-height-compact` (48), `row-height-tall` (60) ; boutons `button-height` (50) ; cibles tactiles `tap-target` (44) minimum.
- Admin web : même fond `bg-grouped`, barre latérale de 240 px sur `surface`, contenu centré à 640 px max pour les formulaires et réglages (une colonne de `ListSection`), 1 120 px max pour les tableaux. Grand titre `large-title` en tête de chaque page.

## Rayons, ombres, matières

- `radius-card` (22) est la signature : cartes, sections, champs groupés. `radius-player` (28) pour le mini-lecteur, `radius-full` pour pilules, disques et interrupteurs, `radius-lg` (12) pour les puces, `radius-md` / `radius-sm` pour le contrôle segmenté et les pastilles de valeur.
- Ombres réservées aux éléments qui flottent ou se déplacent : `shadow-raised` (bouton « Aa »), `shadow-segment`, `shadow-knob`, `shadow-player`. En sombre, elles disparaissent sauf celles du bouton d'interrupteur et du lecteur.
- Séparateurs : `hairline` (0,5 px) en `separator`, démarrant à l'inset de ligne.

## Mouvement et états

- Interrupteurs et segments : 0,2 s. Remplissage des segments de progression : 0,3 s. Repli d'une rubrique lue : transition système (`withAnimation(.snappy)` en SwiftUI).
- Appui : opacité 0,8 sur le bouton principal ; sur iOS, laisser l'effet natif.
- Désactivé : opacité 0,4 (flèche de réordonnancement aux extrémités : 0,2). Rubrique masquée : libellé à 0,4.
- Focus clavier (web) : anneau `focus-ring` plein de 2 px, décalé de 2 px, sur tout élément interactif.
- Respecter « Réduire les animations » : couper les transitions, garder les changements d'état.

## Iconographie

- iOS : SF Symbols — `play.fill`, `pause.fill`, `forward.fill`, `checkmark`, `chevron.right`, `chevron.up`, `chevron.down`, `textformat.size` pour « Aa » (ou le texte « Aa » comme dans la maquette).
- Web : le composant `Icon` fournit les mêmes glyphes en SVG `currentColor`. Les fichiers du groupe Icons sont dessinés à l'encre #111111 pour un usage en `<img>`.
- Pas d'emoji, pas d'icône dans les lignes de réglages ni devant les rubriques.
- L'icône d'app (groupe Logos) est le seul logo : hexagone noir, partage et lignes de texte. Ne pas la redessiner ni la recolorer.

## Correspondance SwiftUI

| Token | iOS |
| --- | --- |
| `bg-grouped` | `Color(.systemGroupedBackground)` |
| `surface` | `Color(.secondarySystemGroupedBackground)` |
| `fill-tertiary` | `Color(.tertiarySystemFill)` (approché) |
| `label` / `label-secondary` | `Color(.label)` / `Color(.secondaryLabel)` |
| `label-body` | `Color(.label).opacity(0.8)` |
| `separator` | `Color(.separator)` |
| `accent` / `on-accent` | `Color.primary` / `Color(.systemBackground)` — définir `AccentColor` à #111111 / #FFFFFF dans l'asset catalog |
| `switch-on` | `Color.green` (tint par défaut du `Toggle`) |
| `radius-card` | `.clipShape(.rect(cornerRadius: 22))` |
| `material-bar` / `material-player` | `.bar` / `.regularMaterial` (Liquid Glass sous iOS 26) |

Les valeurs hexadécimales de `tokens.json` sont celles de la maquette ; sur iOS, préférer les couleurs système ci-dessus, qui suivent aussi le contraste augmenté.

## Composants

Bundle React `window.EnBref` (`components/bundle.js` + `components/bundle.css`, après `tokens.css`) pour l'admin web et les maquettes. Lecture : `LargeTitle`, `SummaryCard`, `ProgressSegments`, `RubricCard`, `MiniPlayer`. Réglages : `ListSection`, `ListRow`, `Switch`, `SegmentedControl`, `ChoiceChips`, `CheckCircle`. Actions : `Button`, `IconButton`, `Icon`. Ajouts propres à l'admin web : `TextField` et `StatusBadge`, absents de la maquette mais nécessaires pour saisir et suivre les récaps.
