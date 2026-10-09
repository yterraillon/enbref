# RubricCard

Une rubrique du récap : nom, brèves, action « Marquer comme lu » ; une fois lue, la carte se replie sur une ligne de 56 px.

- **Fournir** : `name`, `items` (`{title, body}` — deux à trois brèves), `read` + `onReadChange` ou `defaultRead`, `current` pendant la lecture audio, `textSize` (`s`, `m`, `l`) selon le réglage Aa / Dynamic Type.
- Nom en `eyebrow` `label-secondary` ; « ● En lecture » en `eyebrow` `accent` quand `current`.
- Brève : titre `article-title-*` en `label`, corps `article-body-*` en `label-body`, séparées par un filet `separator`.
- Repliée : pastille cochée `accent`, nom en `headline`, « Relire » en `subhead` `label-secondary` ; un toucher la déplie.
- Pas d'image, pas de source, pas de lien : le récap se lit d'un trait.
